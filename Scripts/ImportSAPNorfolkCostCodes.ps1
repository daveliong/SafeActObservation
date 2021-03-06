##########[][][][][][][][][][][][][][][][]##########
#Add SharePoint PowerShell SnapIn if not already added
 if ((Get-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue) -eq $null) {
    Add-PSSnapin "Microsoft.SharePoint.PowerShell"
}

## David Liong v1.0
## This script imports Norfolks and SAP cost codes records from each SharePoint list and combine them into one master list in SharePoint

#region variables
$updatedItem = 0

<#---------Logfile Info----------#>  
$script:logfile = "C:\SharePoint\scripts\logs\SAPNorfolksCostCodes-" + $(get-date -format ddMMyyyyHHmmss) + ".log" 
$script:Seperator = $('-' * 25) 
$script:loginitialized = $false 
$script:FileHeader = $seperator + "***Application Information*** NORFOLK & SAP Cost Codes import ***" + $seperator

$targetSite = "http://vm188.rcrtom.com.au/applications" #"http://webservices.rcrtom.com.au/applications"
$sourceNorfolksList = "NorfolksCostCodes"
$sourceSAPList = "CostCodes"
$targetMasterList = "MasterCostCodes"
$costCodeTypeNorfolk = "Infrastructure"
$costCodeTypeSAP = "SAP"

$titleColumn = "Title"

#Norfolks cost cost SharePoint list fields 
$desColumn = "Description"

#SAP cost cost SharePoint list fields
$codeColumn = "Code"
$areaColumn = "Area"
$compNameColumn = "Company"

$archivedColumn = "Archived"
$masterCodeTypeColumn = "CostCodeType"
#endregion

#region function methods
#Logs any information passed to it.
function write-log([string]$info)
{   
     if($loginitialized -eq $false)
     {    
                     $FileHeader > $logfile   
                     $script:loginitialized = $True     
     }      
        
     $info >> $logfile  
} 



# Add any new Norfolk cost centre item into SharePoint list
Function Update-SPMasterList ([string]$listName, [string]$titleVal, [string]$codeDesc, [string]$costCodeType, [string]$archiveVal, [string]$companyVal, [string]$codeVal)
{
	$spWeb = Get-SPWeb $targetSite
	$listName = $targetMasterList
	$caml = "<Where>
			    <And>
					 <IsNotNull>
			            <FieldRef Name='"+$columnName+"' />
			         </IsNotNull>
					<Eq>
						<FieldRef Name='"+$columnName+"' />
						<Value Type='Text'>"+$titleVal+"</Value>
					</Eq>
			   	</And>
			   </Where>"
			   
	$query = new-object Microsoft.SharePoint.SPQuery
	$query.Query = $caml
	
	$list = $spWeb.Lists[$listName] 
	$spItems = $list.GetItems($query) 
	
	#iterate through SharePoint list item
	$counter = 0
	if ($spItems -ne $null)
	{
		$codeExists = "N"
		foreach ($spItem in $spItems)
		{
			try
			{
				if($spItem[$columnName] -eq $titleVal) 
				{  #match found, no need to create a new element.
					$codeExists = "Y";
					write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Updating cost code: " + $titleVal)
					
					#$spItem[$columnName] = $titleVal
					$spItem[$areaColumn] = $codeDesc
					$spItem[$archivedColumn] = $archiveVal
					$spItem[$compNameColumn] = $companyVal
					$spItem[$codeColumn] = $codeVal
					#$spItem[$masterCodeTypeColumn] = $costCodeType				
					#$spItem.Update()		
					$spItem.SystemUpdate($false)
				}
				else {
					$codeExists = "N";
				}
				
				if($codeExists -eq "N")
				{	#code not found in existing item. Create a new item
					$newItem = $list.Items.Add()
					
					 #Add properties to this list item
					$newItem[$columnName] = $titleVal
					$newItem[$areaColumn] = $codeDesc
					$newItem[$archivedColumn] = $archiveVal
					$newItem[$masterCodeTypeColumn] = $costCodeType
					$newItem[$compNameColumn] = $companyVal
					$newItem[$codeColumn] = $codeVal
					
					#Update the object so it gets saved to the list
					$newItem.Update()
					write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Created new Cost Code " + " (" + $titleVal + ") in " + $listName)
					$counter = $counter + 1	
					$codeExists = "Y"
					$newItem = $null
				}
			}
			Catch [system.exception]
			{
				write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Caught a system exception at Update-SPMasterList function. Error: " + $Error[0])
			}
			
		}
	}
	else{
		#code not found in existing itemm. Create a new item
		$newItem = $list.Items.Add()
			
		#Add properties to this list item
        $newItem[$columnName] = $titleVal
		$newItem[$areaColumn] = $codeDesc
		$newItem[$archivedColumn] = $archiveVal
		$newItem[$masterCodeTypeColumn] = $costCodeType
		$newItem[$compNameColumn] = $companyVal
		$newItem[$codeColumn] = $codeVal
		
		#Update the object so it gets saved to the list
        $newItem.Update()
		write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Created new Cost Code " + " (" + $titleVal + ") in " + $listName)
		$counter = $counter + 1	
		$newItem = $null
	}
		
	#write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Total items updated: " + $counter)
	$query = $null
	$spItems = $null
	$list = $null
	$spWeb.Dispose()
	
	return $counter
}


# Read cost centre item from source SharePoint list and copy to destination list
Function Import-SPList ([string]$listName, [string]$columnName, [string]$costCodeType)
{
	write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|Reading SharePoint list: " + $listName)

	$web = Get-SPWeb $targetSite
	
	$caml = "<Where>
					 <IsNotNull>
			            <FieldRef Name='"+$columnName+"' />
			         </IsNotNull>
			   </Where>"
			   
	$query = new-object Microsoft.SharePoint.SPQuery
	$query.Query = $caml
	$list = $web.Lists[$listName] 
	$spItems = $list.GetItems($query) 
	
	#iterate through SharePoint list item
	$counter = 0
	if ($spItems -ne $null)
	{
		$codeExists = "N"
		foreach ($spItem in $spItems)
		{
			if($spItem[$columnName] -ne $null) 
	        {  #match found, no need to create a new element.
				$codeExists = "Y";
				write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| found Cost Code for " + $spItem[$columnName])
				
				if ($costCodeType -eq $costCodeTypeNorfolk)
				{
					#Import Norfolk cost cost into the master list
					$updatedItem = Update-SPMasterList $listName $spItem[$columnName] $spItem[$desColumn] $costCodeTypeNorfolk $spItem[$archivedColumn] "" $spItem[$columnName]
				}
				elseif ($costCodeType -eq $costCodeTypeSAP)
				{
					$updatedItem = Update-SPMasterList $listName $spItem[$columnName] $spItem[$areaColumn] $costCodeTypeSAP $spItem[$archivedColumn] $spItem[$compNameColumn] $spItem[$codeColumn]
				}
				
				$counter = $counter + $updatedItem	
			}
			else {
				$codeExists = "N";
			}
		
		}
	}
	else{
		write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| No items found for list: " + $listName)
	}
		
	write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Total items updated: " + $counter)
	
	$query = $null
	$spItems = $null
	$list = $null
	$web.Dispose()
	
}

#endregion


#Trap all exceptions to the log
Trap [Exception] {
  write-log $("$UseInfo`t$_. - Line:(" + $($_.InvocationInfo.ScriptLineNUmber)+":"+$($_.InvocationInfo.OffsetInLine)+ ") " + $($_.InvocationInfo.Line))
  continue
}


#region begin script
write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Begin populating Cost Codes...")
write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Site:" + $targetSite + " , Norfolk List:" + $sourceNorfolksList + " , SAP list:" + $sourceSAPList + " , Master list:" + $targetMasterList)
Write-Host "Getting Cost Codes..." 

Import-SPList $sourceNorfolksList $titleColumn $costCodeTypeNorfolk
Import-SPList $sourceSAPList $titleColumn $costCodeTypeSAP

write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|" + $seperator + "Finish populating Cost Codes" + $seperator)
#endregion