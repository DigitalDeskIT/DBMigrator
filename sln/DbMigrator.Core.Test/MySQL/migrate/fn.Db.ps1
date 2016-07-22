function DBDrop{
	param(
		$ask = $true
	)
	if($global:DB_Server -imatch 'localhost'){
		if(-not $ask){
			$drop = 's'
		}
		else {			
			$drop = Read-Host ('REMOVER o banco de dados '+$global:DB_Name+' agora [s/n]?')
		}
		if ($drop -imatch '^s$'){
			$dropQuery = 'DROP DATABASE IF EXISTS '+ $global:DB_Name +';'
			
			Write-Host '' $dropQuery
			$result = mysqlcmd $global:DB_ConnectionStringServer $dropQuery 
			Write-Host '' $result
		}
		elseif ($drop -imatch '^n$'){
			return
		}
		else{
			Read-Host '?'
			DBDrop
		}
	}
	else{
		Write-Host $global:DB_Server proibido.
		Write-Host 'Apenas bancos locais podem ser excluidos por este comando.'
	}
}

function DBCreate{
	param($name)
	
	if ( -not $name){
		$sufix = Read-Host ('Sufixo do Banco ' + $global:migratorName)
		$name = $global:migratorName +$sufix
		Write-Host ''
	}
	Write-Host ('Criando novo banco '+$name+'...')
	$dropQuery = 'CREATE DATABASE IF NOT EXISTS `'+$global:DB_Name+'` ;'
			
	Write-Host '' $dropQuery
	$result = mysqlcmd $global:DB_ConnectionStringServer $dropQuery 
	Write-Host '' $result
	
}

function DBSetup {
	
	$selectedIndex = 0

	$User = Read-Host 'User'
	$Password = Read-Host 'Password'
	$Database = Read-Host 'Database'
	$MySqlHost = Read-Host 'Host'
	$ConnectionString = 'server=' + $MySqlHost + ';port=3306;uid=' + $User + ';pwd=' + $Password + ';database='+$Database
	$ConnectionStringServer = 'server=' + $MySqlHost + ';port=3306;uid=' + $User + ';pwd=' + $Password
	
	$opts = @{
		'DB_NiceName'=('Local '+$MySqlHost+' '+$Database);
		'DB_ConnectionString'=$ConnectionString;
		'DB_Server'=$MySqlHost;
		'DB_Instance'=$MySqlHost;
		'DB_Name'=$Database
	}
	
	$global:DB_ConnectionString = $ConnectionString
	$global:DB_NiceName=('Local '+$MySqlHost+' '+$Database);
	$global:DB_ConnectionString=$ConnectionString;
	$global:DB_ConnectionStringServer=$ConnectionStringServer;
	$global:DB_Server=$MySqlHost;
	$global:DB_Instance=$MySqlHost;
	$global:DB_Name=$Database
	
	#Write-Host ''
	#$i = 0
	#$opts | ForEach-Object {
	#	echo ( $i.ToString() + ' - ' + $_.DB_NiceName + ' - ' + $i.ToString() )
	#	$i = $i + 1
	#}
	#Write-Host ''
	
	#$opt = $opts[$selectedIndex]
	#$opt.Keys | ForEach-Object{
	#	Set-Variable -Name $_ -Value $opt[$_] -Scope Global
	#}
}

function mysqlcmd {
	param($connectionstring,$query)
	
	Try {
		[void][System.Reflection.Assembly]::LoadWithPartialName('MySql.Data')
		$Connection = New-Object MySql.Data.MySqlClient.MySqlConnection
		$Connection.ConnectionString = $connectionstring
		$Connection.Open()
		
		$Command = New-Object MySql.Data.MySqlClient.MySqlCommand($query, $Connection)
		$Command.ExecuteNonQuery()
	}

	Catch {
		Write-Host 'ERROR : Unable to run query :' + $query + ' > ' + $Error[0]
	}

	Finally {
		$Connection.Close()
	}
}