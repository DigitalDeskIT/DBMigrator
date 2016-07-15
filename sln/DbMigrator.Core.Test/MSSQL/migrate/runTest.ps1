. '.\fn.UserData.ps1'
. '.\fn.Db.ps1'
. '.\fn.DbMigrate.ps1'
. '.\fn.Initialize.ps1'

Initialize -UserFile 'UserDataTest.txt'

DBDrop -Ask $false
$server = $global:DB_Server+'\'+$global:DB_Instance
DBCreate -Server $server -Name $global:DB_Name
DBMMigrate -Mode migrate -IncludeTest 's'