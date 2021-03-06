##########[][][][][][][][][][][][][][][][]##########
#Add SharePoint PowerShell SnapIn if not already added
 if ((Get-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue) -eq $null) {
    Add-PSSnapin "Microsoft.SharePoint.PowerShell"
}

## David Liong v1.0
## This script reads the content of an XML file and creates a new copy of the XML file

#region variables
<#---------Logfile Info----------#>  
$script:logfile = "C:\SharePoint\scripts\logs\FileParser-" + $(get-date -format ddMMyyyyHHmmss) + ".log" 
$script:Seperator = $('-' * 25) 
$script:loginitialized = $false 
$script:FileHeader = $seperator + "*** File Parser ***" + $seperator

$infoPathFile = "SAO-Test.xml"
$destFilePath = "c:\temp\sao\"
$prefixDestFileName = "SAO-"
$numFiles = 1000
$firstFileNum = 500

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

#Function to copy a source file to a destination file
function copy-Files($sourceFile)
{
	$outputFile = $destFilePath + $prefixDestFileName + $(get-date -format yyyy) + "-" + (get-date -format MM) + "-" + (get-date -format dd) + "T" + (get-date -format HHmmss) + ".xml"
  
	for ($i = $firstFileNum; $i -le $numFiles; $i++)
	{
		write-host "Copying $outputFile ..."
		copy-item $sourceFile $outputFile
		Start-Sleep -s 1
		$outputFile = $destFilePath + $prefixDestFileName + $(get-date -format yyyy) + "-" + (get-date -format MM) + "-" + (get-date -format dd) + "T" + (get-date -format HHmmss) + "-" + $i + ".xml"
	}
}

#Function to write multiple files
function write-Files($info)
{
  $outputFile = $prefixDestFileName + $(get-date -format yyyy) + "-" + (get-date -format MM) + "-" + (get-date -format dd) + "T" + (get-date -format HHmmss) + ".xml"
  
  for ($i = 1; $i -le $numFiles; $i++)
  {
	write-File $info $outputFile
	Start-Sleep -s 1
	$outputFile = $prefixDestFileName + $(get-date -format yyyy) + "-" + (get-date -format MM) + "-" + (get-date -format dd) + "T" + (get-date -format HHmmss) + "-" + $i + ".xml"
  }
}

# Function to read a file
function read-File([string] $fileName)
{
	write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Reading content of $fileName...")
	$fileContent =  get-content $fileName
	return $fileContent
}

#Write content to a file
function write-File([string] $content, [string] $destFile)
{
	write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Writing content to $destFile...")
	$content | Out-File $destFile
}

#Read content of an xml file
function read-XML([string] $xmlFile)
{
	write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Reading content of $xmlFile...")
	[xml]$xdoc = get-content $xmlFile
	$xContent = ""
	
	foreach($xitem in $xdoc.Users.User) 
	{
		#write-host $xitem.Name 
		$xContent = $xContent + $xitem.Name + "`n" 
	} 
	
	return $xContent
}
#endregion


#Trap all exceptions to the log
Trap [Exception] {
  write-log $("$UseInfo`t$_. - Line:(" + $($_.InvocationInfo.ScriptLineNUmber)+":"+$($_.InvocationInfo.OffsetInLine)+ ") " + $($_.InvocationInfo.Line))
  continue
}


#region begin script
write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "| Begin reading  file...")
Write-Host "Copying XML file..." 

copy-Files $infoPathFile

write-log ((get-date -format "dd-MM-yyyy HH:mm:ss").ToString() + "|" + $seperator + "Finish XML Parse" + $seperator)
Write-Host "Finish Parsing file" 

#endregion