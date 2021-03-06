##########[][][][][][][][][][][][][][][][]##########
#Add SharePoint PowerShell SnapIn if not already added
 if ((Get-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue) -eq $null) {
    Add-PSSnapin "Microsoft.SharePoint.PowerShell"
}

## David Liong v1.0
## This script opens the Norfolks Cost centre CSV file (currently on http://webservices.rcrtom.com.au/corporate-it/InfraEmpExtracts/Forms/AllItems.aspx)
## Imports the values and compares then against the existing Norfolk cost cost list in SharePoint intranet site
## If an item exists in SharePoint it is not updated (identical values)
## New items in the CSV file are added to the SharePoint list
## Tested to around 4000 unique cost codes

#region variables
$updatedItem = 0

<#---------Logfile Info----------#>  
$script:logfile = "C:\SharePoint\scripts\logs\NorfolksCostCodes-" + $(get-date -format ddMMyyyyHHmmss) + ".log" 
$script:Seperator = $('-' * 25) 
$script:loginitialized = $false 
$script:FileHeader = $seperator + "***Application Information*** NORFOLK Cost Codes import ***" + $seperator

## Update these values ##
$targetSite = "http://vm188.rcrtom.com.au/applications" #"http://webservices.rcrtom.com.au/applications"
$targetLib = "NorfolksCostCodes"
$titleColumn = "Title"
$desColumn = "Description"
$archivedColumn = "Archived"
$csvSourceFile =  "\\webservices.rcrtom.com.au\corporate-it\InfraEmpExtracts\infraemp.csv" #Get the UNC path of the sharepoint site #"C:\Projects\SafeActObservation\Scripts\infraemp.csv"
## ##

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
Function Update-SPList ($web, [string]$listName, [string]$columnName, [string]$codeVal, [string]$codeDesc)
{
	$caml = "<Where>
			    <And>
					 <IsNotNull>
			            <FieldRef Name='"+$columnName+"' />
			         </IsNotNull>
					<Eq>
						<FieldRef Name='"+$columnName+"' />
						<Value Type='Text'>"+$codeVal+"</Value>
					</Eq>
			   	</And>
			   </Where>"
			   
	$query = new-object Microsoft.SharePoint.SPQuery
	$query.Query=$caml
	
	$list = $web.Lists[$listName] 
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
				if($spItem[$columnName] -eq $codeVal) 
				{  #match found, no need to create a new element.
					$codeExists = "Y";
					write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| found matching Cost Code, no update required for " + $codeVal)
					break;
				}
				else {
					$codeExists = "N";
				}
				
				if($codeExists -eq "N")
				{	#code not found in existing itemm. Create a new item
					$newItem = $list.Items.Add()
					
					 #Add properties to this list item
					$newItem[$columnName] = $codeVal
					if ($codeDesc -eq "NULL")
					{
						$codeDesc = "No description"
					}
					$newItem[$desColumn] = $codeDesc
					$newItem[$archivedColumn] = "No"
					
					#Update the object so it gets saved to the list
					$newItem.Update()
					write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Created new Cost Code " + " (" + $codeVal + ") in SharePoint.")
					$counter = $counter + 1	
					$codeExists = "Y"
				}
			}
			Catch [system.exception]
			{
				write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Caught a system exception at Update-SPList function. Error: " + $Error[0])
			}
		}
	}
	#if (($codeExists -eq "N") -and ($counter -eq 0))
	else{
		#code not found in existing itemm. Create a new item
		$newItem = $list.Items.Add()
			
		#Add properties to this list item
        $newItem[$columnName] = $codeVal
		if ($codeDesc -eq "NULL")
		{
			$codeDesc = "No description"
		}
		$newItem[$desColumn] = $codeDesc
		$newItem[$archivedColumn] = "No"
		
		#Update the object so it gets saved to the list
        $newItem.Update()
		write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Created new Cost Code " + " (" + $codeVal + ") in SharePoint.")
		$counter = $counter + 1	
	}
		
	#write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|Total items updated: " + $counter)
	return $counter
}

# Read Norfolk CSV file
Function Import-CSV-To-SPList
{
    [CmdletBinding()]
    Param(
        [Parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
            [string]$CSVFilePath = "CSVFile.csv",
        [Parameter(Position=1, Mandatory=$true)]
            [string]$SiteURL = "http://SharePointWebApp/sites/SiteCollection",
        [Parameter(Position=2, Mandatory=$true)]
            [string]$Library = "Shared Documents"     
    ) #end param
	
    Process
    {
        Try
        {
            #Get Web
            $web = Get-SPWeb $SiteUrl

            Write-Host -Background Black -Foreground Green "Updating list..."

            # Loop through the CSV file entries
			$counter = 0
            Import-Csv $CSVFilePath -Header H1,H2,H3,H4,H5,H6,H7,H8,H9,H10,H11,H12,H13,H14,H15,CostCode,Desc,H18,H19,H20,H21,H22 | ForEach-Object{

                # Assign the new document name, and build the destination SharePoint path
                $SPTitle = $_.CostCode
				
				if($SPTitle -ne "")
				{
					#Write-Host "Cost code: " $SPTitle	"-" $_.Desc
					if ($SPTitle -ne "cost-centre")
					{#Do not populate column field name
						$itemUpdated = Update-SPList $web $Library $titleColumn $SPTitle $_.Desc
						$updatedItem = $updatedItem + $itemUpdated
					}
					  
				}#end if
				$counter = $counter + 1
            } #end CSV loop
			
			Write-Host "Total updated items: " $updatedItem
			Write-Host "Total CSV records: " $counter
			write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|Total updated items: " + $updatedItem)
			#write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|Total CSV records: " + $counter)
			
			$web.Dispose()
			
        } #end Try
        Catch
        {
            write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Caught a system exception at Update-SPMasterList function. Error: " + $Error[0])
        }
		
    } #end Process
	
} #end function 

#We've checked to see if each Cost Code is in the SP List, but if any codes have been deleted from Norfolks then we need to remove them from SP list as well by flagging the cost item as archived . Check if the current Cost Code exist already in the list
Function Archive-CostCode
{
	[CmdletBinding()]
    Param(
        [Parameter(Position=0, Mandatory=$true)]
            [string]$SiteURL = "http://SharePointWebApp/sites/SiteCollection",
		[Parameter(Position=2, Mandatory=$true)]
            [string]$listName = "Shared Documents",
		[Parameter(Position=3, Mandatory=$true)]
            [string]$columnName = "Title",	
		[Parameter(Position=4, Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
            [string]$CSVFile = "CSVFile.csv"   
    ) #end param
	
	Process
    {
		write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Archiving any Cost Code in SharePoint...")
		write-host "Archiving any Cost Code in SharePoint..."
		
		#Get all items in this list and save them to a variable (only use this on relatively small lists say sub 500 items)
		$caml = "<Where>
			  <And>
				 <IsNotNull>
					<FieldRef Name='"+$columnName+"' />
				 </IsNotNull>
				 <IsNotNull>
					<FieldRef Name='"+$columnName+"' />
				 </IsNotNull>
			  </And>
		   </Where>"
		
		Try
		{
			$web = Get-SPWeb $SiteUrl
			$list2 = $web.Lists[$listName] 
			
			$query = new-object Microsoft.SharePoint.SPQuery
			$query.Query = $caml
			$items2 = $list2.GetItems($query)   

			$deleteItemIDs = New-Object System.Collections.ArrayList
			$deleteItemIDs.Clear()

			$existingServers = New-Object System.Collections.ArrayList
			$existingServers.Clear()
			
			#Read CSV and add all cost code into an array
			Import-Csv $CSVFile -Header H1,H2,H3,H4,H5,H6,H7,H8,H9,H10,H11,H12,H13,H14,H15,CostCode,Desc,H18,H19,H20,H21,H22 | ForEach-Object{

					# Assign the new document name, and build the destination SharePoint path
					$codeVal = $_.CostCode
					
					if($codeVal -ne "")
					{
						if ($codeVal -ne "cost-centre")
						{#Do not populate column field name
							Clear-Host
							$existingServers.Add($codeVal)
						}
						  
					}#end if
					
			} #end CSV loop
			
			#Write-Host "Total cost costs: " $existingServers.Count
			
			#Add SharePoint list ID into an array if CSV cost code is not exist in SharePoint
			Foreach($item in $items2) 
			{ 
				if($existingServers -notcontains $item[$titleColumn])
				{	
					$itemID = $item["ID"]
					if($deleteItemIDs -notcontains $itemID)
					{ 
						$deleteItemIDs.Add($item["ID"])
					}	
				}
			}     
			
			Foreach($id in $deleteItemIDs)
			{
				if($id -lt 100000)
				{
					$item = $list2.GetItemById($id) 
					if($item[$archivedColumn] -ne "Yes")
					{
						write-host "Archiving " $item[$columnName] " - " $item[$desColumn]
						
						$item[$archivedColumn] = "Yes"
						$item.Update()
						write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Archiving " + $item[$columnName] + " in SharePoint.")
					}
				}
			}
			
			$deleteItemIDs.Clear()
			$existingServers.Clear()
			$web.Dispose()
			
		}
		Catch
		{
			#Write-host "Error: " $Error[0]
			Write-Error "Error: " $Error[0]
		}
		
	} #End process
}

Function Test1
{
	#Get Web
    $web = Get-SPWeb $targetSite
	$itemUpdated = Update-SPList $web $targetLib $titleColumn "Test 1" "hello world"
	$updatedItem = $updatedItem + $itemUpdated
	Write-Host "Total updated items: " $updatedItem
}

#endregion


#Trap all exceptions to the log
Trap [Exception] {
  write-log $("$UseInfo`t$_. - Line:(" + $($_.InvocationInfo.ScriptLineNUmber)+":"+$($_.InvocationInfo.OffsetInLine)+ ") " + $($_.InvocationInfo.Line))
  continue
}


#region begin script
write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Begin populating Cost Codes.")
write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| " + $targetSite + " " + $targetLib)

Write-Host "Getting Norfolk Cost Codes..." 
Import-CSV-To-SPList -CSVFilePath $csvSourceFile -SiteURL $targetSite -Library $targetLib
Archive-CostCode -SiteURL $targetSite -listName $targetLib -columnName $titleColumn -CSVFile $csvSourceFile

write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|" + $seperator + "Finish populating Cost Codes" + $seperator)
#endregion