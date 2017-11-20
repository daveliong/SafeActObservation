##########[][][][][][][][][][][][][][][][]##########
#Add SharePoint PowerShell SnapIn if not already added
$snapin="Microsoft.SharePoint.PowerShell"
 if ((Get-PSSnapin $snapin -ErrorAction SilentlyContinue) -eq $null) {
    Add-PSSnapin $snapin
}
else {
    write-host "PSSnapin $snapin not found"
}

#region variables

$wspSAOFile = "RCR.SafeActObservation.wsp"
$featureIdSAO = "49c69ac6-61e8-4dd8-80ee-6f5d79053ece"
$wspFrameworkFile = "RCR.SP.Framework.wsp"
$featureIdFramework = ""
$isWSPDeploy = $false
$waitTime = 2

#endregion

#region methods

#Function to check that SharePoint services is running
function checkSPServices
{
	Write-Host "Checking SharePoint services is running..."
	
	# Make sure SharePoint Admin service is started
	$SPAdminSvc = Get-Service | where {$_.Name -eq "SPAdminV4"}
	if ( $SPAdminSvc.Status -eq "Stopped" )
	{
	   Write-Host "Starting SharePoint Admin service..."
	   Start-Service -Name $SPAdminSvc.Name
	   sleep $waitTime
	}
	
	# Make sure SharePoint Timer service is started
	$SPTimerSvc = Get-Service | where {$_.Name -eq "SPTimerV4"}
	if ( $SPTimerSvc.Status -eq "Stopped" )
	{
	   Write-Host "Starting SharePoint Timer..."
	   Start-Service -Name $SPTimerSvc.Name
	   sleep $waitTime
	}
	
}

# Function to check if solution is already installed
function checkSolutionDeploy ($solutionName)
{
	$isSolultionDeploy = $false
	
	$currentDirectory = Get-Location
	$solutionPath = $currentDirectory
	
	if ($solutionPath -ne "")
	{			
		# fetch the path
		$temp = AddPathChar($solutionPath);

		# set the solution name
		$solutionName = [string]::Concat($temp, $solutionName);

		$file = Get-ChildItem $solutionName
		$solutionID = $file.Name
			
		#Check if the solution package is already added and installed
		$Solution = Get-SPSolution | Where{$_.Name -eq $solutionID}
		if ($Solution -ne $null)
		{
		  Write-Host ""
		  Write-Host "The solution $solutionID is already installed"
		  Write-Host ""
		  $isSolultionDeploy = $true
		}
		else {
			Write-Host ""	
			Write-Host "The solution $solutionID has not been installed!"
			Write-Host ""
		}
	}
	
	return $isSolultionDeploy
}

#Function to check if SharePoint framework has been installed
function checkPreInstallation($wspFile)
{
	Write-Host "Checking if $wspFile solution has been mandatory installed. Please wait...."
	$isSolultionDeploy = checkSolutionDeploy $wspFile
	$isPreInstall = $false
	
	if ($isSolultionDeploy -eq $false)
	{
		checkSPServices
		$isPreInstall = installWSP $wspFile $waitTime $false
		
		if ($isPreInstall -eq $true)
		{
			Write-Host "$wspFile solution was successfully installed."
		} else {
			Write-Host "$wspFile solution was NOT successfully installed!"
		}
		
	}
	else {
		Write-Host "$wspFile solution has already been installed."
		$isPreInstall = $true
	}
	
	return $isPreInstall
}

#Function to update existing solution package in SharePoint
function updateSolution ($solutionName, $sleeptime)
{
	$isUpdate = $false
	
	Write-Host -f Yellow "Do you want to update the solution $solutionName ?"
	$confirmUpdate = Read-Host 
	
	if (($confirmUpdate -eq "y") -or ($confirmUpdate -eq "Y"))
	{
		$currentDirectory = Get-Location
		$solutionPath = $currentDirectory #Read-Host 
		
		if ($solutionPath -ne "")
		{
			# fetch the path
			$temp = AddPathChar($solutionPath);

			# set the solution name
			$solutionName = [string]::Concat($temp, $solutionName);

			$file = Get-ChildItem $solutionName
			$solutionID = $file.Name
			$farm = Get-SPFarm
			
			write-host "Updating the $solutionName ..."
			Update-SPSolution -Identity $solutionID -LiteralPath $solutionName -GACDeployment
			WaitForInsallation -Name $solutionID
			
			Write-Host ""
			Write-Host "$solutionID was successfully updated" 			
		}
	}
	else {
		Write-Host "Updating $solutionName was cancel!" 
	}
	
}

#Function to install solution package from SharePoint
function installWSP ($solutionName, $sleeptime, $featureEnabled)
{
	$isInstall = $false
	
	Write-Host -f Yellow "Do you want to install the solution $solutionName ?"
	$confirmInstall = Read-Host 
	
	if (($confirmInstall -eq "y") -or ($confirmInstall -eq "Y"))
	{
		#Write-Host -f Yellow "Type the SharePoint Portal Site URL:"
		#$url = Read-Host 
		
		#Write-Host -f Yellow "Type the folder path location for $solutionName:"
		$currentDirectory = Get-Location
		$solutionPath = $currentDirectory #Read-Host 
		$isSolutionAdded = $false
		$isSolutionUpdated = $false
		
		try
		{
			if (($url -ne "") -and ($solutionPath -ne ""))
			{
				# fetch the path
				$temp = AddPathChar($solutionPath);

				# set the solution name
				$solutionName = [string]::Concat($temp, $solutionName);

				$file = Get-ChildItem $solutionName
				$solutionID = $file.Name
				$farm = Get-SPFarm
					
				#Check if the solution package is already added and installed
				$Solution = Get-SPSolution | Where{$_.Name -eq $solutionID}
				if ($Solution -ne $null){
					if ($Solution.Deployed -eq $true)
					{
						updateSolution $solutionName $sleeptime
						$isSolutionUpdated = $true
					}
					else {
						Write-Host "Removing the existing solution package $solutionID ...." 
						Remove-SPSolution -Identity $solutionID -Confirm:$false					
						$isSolutionAdded = $true				
					}
				}
				else {
					$isSolutionAdded = $true											
				}
				
				if ($isSolutionAdded -eq $true)
				{
					Write-Host "Adding the solution package $solutionID ..."
					Add-SPSolution $solutionName
				}
				
				if ($isSolutionUpdated -eq $false)
				{
					Write-Host "Installing the solution package $solutionID ..."
					#Install-SPSolution -Identity $solutionID -Webapplication $url -GacDeployment -Force -CompatibilityLevel 15
					Install-SPSolution -Identity $solutionID -GacDeployment -Force -CompatibilityLevel 15
				}
				
				WaitForInsallation -Name $solutionID
				
				Write-Host ""
				Write-Host "$solutionID was deployed" 
				Write-Host ""

				if ($featureEnabled -eq $true)
				{
					$featureId = ""
					
					# ENABLE the feature for all web sites
					if ($solutionName -eq $wspSAOFile)
					{
						$featureId = $featureIdSAO
					}
					
					if ($featureId -ne "")
					{
						Write-Host ""
						Write-Host "Enabling Features for $solutionID ..."
					
						$webApp = Get-SPWebApplication
						$webApp | Get-SPSite -limit all | ForEach-Object {Enable-SPFeature -Identity $featureId -Url $_.Url -Force -Confirm:0; Write-Host $_.Url}
					
						Write-Host ""
						Write-Host "Features enabled for $solutionID"
					}
				}
				
				Write-Host ""
				Write-Host "Deployment was successful"	
				$isInstall = $true
			 }
			 else {
				Write-Host "Installing $solutionName was cancel. Please enter site url!"
			 }
		}
		Catch
		{
			$ErrorMessage = $_.Exception.Message
			Write-Host "Caught a system exception at installWSP function! Error: $ErrorMessage"	
		}
		
	}
	else {
		Write-Host "Installation of $solutionName was cancel"
	}
	
	return $isInstall
	
}

#Function to remove solution from SharePoint
function unInstallWSP ($solution, $sleeptime)
{
	#Write-Host -f Yellow "Type the SharePoint Portal Site URL to uninstall:"
	#$url = Read-Host 
	$isUninstall = $false
	
	Write-Host ""
	Write-Host "Going to uninstall $solution ..."
	Write-Host ""
	
	$file = Get-ChildItem $solution
	$solutionID = $file.Name

	try
	{
		$farm = Get-SPFarm
		$sol = $farm.Solutions[$solutionID]

		if($sol)
		{   
		   if( $sol.Deployed -eq $TRUE ) 
		   {
				#Disable the feature for all web sites
				#Write-Host "Disabling Features for $solutionID ..."
				#$webApp = Get-SPWebApplication
				#$webApp | Get-SPSite -limit all | ForEach-Object {	
				#	if($url.ToLower() -eq $_.Url.ToLower())
				#	{
				#		Disable-SPFeature -Identity "a228ed17-bc22-4966-bb80-0acf59d99e1e" -Url $_.Url -Force -Confirm:0 ; 
				#		Write-Host $_.Url
				#	}
				#}
				#Write-Host -f "Features Disabled for $solutionID"
				
				Uninstall-SPSolution -Identity $solutionID -Confirm:0 -AllWebApplications
				
				while( $sol.JobExists ) {
					Write-Host "Waiting for retraction..."
					sleep $sleeptime
				}
				
				Write-Host ""
				Write-Host "$solutionID is retracted."
	
		   }
		  
			Write-Host ""
			Write-Host "Going to Remove $solutionID"
			Write-Host ""

			Remove-SPSolution -Identity $solutionID -Force -Confirm:0
			
			Write-Host ""
			Write-Host "$solutionID is deleted from this Farm"
			Write-Host ""
			
			$isUninstall = $true
		}
	}
	Catch
	{
		$ErrorMessage = $_.Exception.Message
		Write-Host "Caught a system exception at unInstallWSP function! Warning: $ErrorMessage"
		Write-Host -f Yellow "Please manually remove $solution from Central Administration."
		#Uninstall-SPSolution -Identity $solution #Retracts a deployed SharePoint solution.
		#Remove-SPSolution -Identity $solution #Removes a SharePoint solution from a farm.		
	}
	
	return $isUninstall
}

function AddPathChar()
{
  param([string]$source)
  if (  $source.EndsWith("\") )
  {
  }
  else
  {
    $source = $source + "\";
  }
  return $source;
}

Function WaitForInsallation([string] $Name)
{
	Write-Host -NoNewline "Waiting for deployment job to complete" $Name "."
	$wspSol = get-SpSolution $Name
 
	while($wspSol.JobExists)
	{
	  sleep $waitTime
	  Write-Host -NoNewline "."
	  $wspSol = get-SpSolution $Name
	}
	 Write-Host ""
	 Write-Host -ForegroundColor Green "Deployment job is finished"
}


#endregion

#region MAIN program
Write-Host "Deploying $wspSAOFile solution. Please wait...."
$deploy = checkSolutionDeploy $wspSAOFile

if ($deploy -eq $false)
{
	$isWSPDeploy = checkPreInstallation $wspFrameworkFile
	
	if ($isWSPDeploy -eq $true)
	{
		$isInstall = installWSP $wspSAOFile $waitTime $false
		
		if ($isInstall -eq $true)
		{
				Write-Host "Deployment of $wspSAOFile was successful"
		}
		else {
			Write-Host "Deployment of $wspSAOFile was NOT successful!"
		}
	}
	else {
		Write-Host "Pre-requisite $wspFrameworkFile solution was not successfully installed. Please investigate issue to install it BEFORE installing $wspSAOFile"
	}
}
else {
	Write-Host ""
	Write-Host "$wspSAOFile is already installed."
	Write-Host -f Yellow "Please confirm that you want to re-install $wspSAOFile [Y] , [N]:"
	
	$confirmResponse = Read-Host 
	
	if (($confirmResponse -eq "y") -or ($confirmResponse -eq "Y"))
	{
		checkSPServices
		$isRemove = unInstallWSP $wspSAOFile $waitTime
		
		if ($isRemove)
		{
			$isInstall = installWSP $wspSAOFile $waitTime $false
			if ($isInstall -eq $true)
			{
				Write-Host "Deployment of $wspSAOFile was successful"
			}
			else {
				Write-Host "Deployment of $wspSAOFile was NOT successful!"
			}
			
		}
		else {
			Write-Host ""
			Write-Host "Uninstalling $wspSAOFile was NOT successful!"
		}
			
	}
	else {	
		updateSolution $wspSAOFile $waitTime		
	}
}
#endregion