function Initialize{
	param (
		$config
	)
	
	#RESOLVE DRIVE
	$cd = (Get-Location).ToString()
	$global:drive = $cd -replace '([A-z])[:].+','$1'
	Write-Host CurrentDrive = $global:drive
	
	$xConfig = [xml] (get-content ($global:drive + ':\Projetos\DBMigrator\sln\DbMigrator.Core.Test\MSSQL\migrate\config-'+$config+'.xml'))

	
	#VARI�VEIS DO CONFIG
	$global:migratorName = $xConfig.values.migratorName
	$global:scriptsPath = $xConfig.values.scriptsPath
	$global:userFile = $xConfig.values.userFile
	
	#VARI�VEIS DO PROGRAMA
	$global:userDataKeys = @('DB_Name','DB_NiceName','DB_Instance','DB_Server', 'DB_ConnectionString', 'DBM_Filter', 'DBM_ToNode', 'DBM_Instance');
	$global:migratorExe = ':\Projetos\DBMigrator\sln\DbMigrator.Core.Test\MSSQL\migrate\DBM.exe'
	$global:startDirectory = ':\Projetos\DBMigrator\sln\DbMigrator.Core.Test\MSSQL\migrate'
	
	#VARI�VEIS DO USU�RIO
	SetDefaultUserData
	LoadUserData
	SaveUserData



	Set-Location $cd
}