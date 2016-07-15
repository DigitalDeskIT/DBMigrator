function DBMSetInstance{
	Write-Host ''
	$validInstances = @('STAGING', 'RELEASE')
	$validInstances | ForEach {
		Write-Host ('- '+$_)
	}
	Write-Host ''
	
	$instance = Read-Host Instancia
	
	$valid = $false
	$validInstances | ForEach {
		$reg  = '^'+ $_ + '$'
		if($instance -imatch $reg){
			$valid = $true
		}
	}
	
	if ($valid -eq $false){
		return
	}
	else{
		$global:DBM_Instance = $instance.ToUpper()
	}
}

function DBMMigrate{
	param(
		$includeTest = '',
		$mode = 'migrate',
		$scriptsPath
	)
	
	# if(-not $global:DBM_Instance){
		# Write-Host 'Instancia de migra��o requerida.'
		# return
	# }
	if(-not $global:DB_ConnectionString){
		Write-Host 'É necessário configurar o banco utilizado.'
		return
	}
	
	$filter = ' '
	if($global:DBM_Instance -match 'NONE'){
		$filter='!instance'
	}
	else{
		$filter='core|(instance&(_any|'+$global:DBM_Instance+'))'
	}
	if($global:DB_Server.CompareTo('localhost') -eq 0){
	
		while ($includeTest -inotmatch '^(s|n)$'){
			$includeTest = Read-Host 'Executar scripts de teste [s/n]?'
		}
			
		if($includeTest -imatch 'n'){
			$filter = $filter + '&!test'
		}
	}
	
	$config = $global:drive + $global:scriptsPath + '\config.json'
	$sqlPath = $global:drive + $global:scriptsPath
	$conn = $global:DB_ConnectionString
	$file = 'LastRun-'+$mode+'.txt'
	& ($global:drive + $global:migratorExe) 'migrate' --mode $mode --scriptsMapPath $config --connectionString $conn | Tee-Object $file
}




