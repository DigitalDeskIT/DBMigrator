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
			$server = '.\' + $global:DB_Instance
			$singleUserQuery = "IF db_id('" + $global:DB_Name + "') IS NOT NULL ALTER DATABASE ["+ $global:DB_Name +"] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
			$dropQuery = "IF db_id('"+ $global:DB_Name +"') IS NOT NULL DROP DATABASE ["+ $global:DB_Name +"];"
			
			Write-Host $singleUserQuery
			$result = sqlcmd '-S' $server '-Q' $singleUserQuery
			Write-Host '' $dropQuery
			$result = sqlcmd '-S' $server '-Q' $dropQuery 
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
	param($server,$name)
		
	if ( -not $server){
		$i = 0
		$servers = @()
		Write-Host ''
		Get-Service | Where { $_.Name -match '^MSSQL' -and $_.DisplayName -match 'SQL Server' -and $_.Status -match '^Running$' } | ForEach {
			$instance = ( $_.Name -split '[$]' )[1]
			$cServer = 'localhost\'+$instance;
			$servers += $cServer
			Write-Host ('' + $i + ' - '+ $cServer + ' - ' + $i)
			$i = $i+1
		}
		Write-Host ''
		$serverIndex = Read-Host 'Indice do Servidor de SQL'
		$server = $servers[$serverIndex]
	}
	if ( -not $name){
		$sufix = Read-Host ('Sufixo do Banco ' + $global:migratorName)
		$name = $global:migratorName +$sufix
		Write-Host ''
	}
	$server = $server -replace '^localhost','.'
	Write-Host ('Criando novo banco '+$name+' em '+$server+'...')
	$variables = ('db_name="'+$name+'"')
	& 'sqlcmd' '-S' $server '-i' 'CreateEmptyDB.sql' '-v' $variables
}

function DBSetup {
	$opts = @(
		# @{
			# "DB_NiceName"="DEFAULT";
			# "DB_ConnectionString"="server=localhost\SQLEXPRESS; database=Test; Trusted_Connection=True;";
			# "DB_Server"="localhost";
			# "DB_Instance"="SQLEXPRESS";
			# "DB_Name"="Test"
		# }
	)
	
	Get-Service | Where { $_.Name -match '^MSSQL' -and $_.DisplayName -match 'SQL Server' -and $_.Status -match '^Running$' } | ForEach {
		$instance =( $_.Name -split '[$]' )[1]
		$localExpressDbs = sqlcmd -Q "SET NOCOUNT ON; SELECT [Name] FROM sys.databases" -S ("localhost\"+$instance) | Where { $_ -match ('^'+$global:migratorName) } | ForEach-Object{
			$name = $_.trim()
			if ($name -notmatch '^[-]+|Name') {
				$opts += @{
					"DB_NiceName"=('Local '+$instance+' '+$name);
					"DB_ConnectionString"=("server=localhost\"+$instance+"; database="+$name+"; Trusted_Connection=True;");
					"DB_Server"="localhost";
					"DB_Instance"=$instance;
					"DB_Name"=$name
				}
			}
		}
	}
	
	Write-Host ''
	$i = 0
	$opts | ForEach-Object {
		echo ( $i.ToString() + ' - ' + $_.DB_NiceName + ' - ' + $i.ToString() )
		$i = $i + 1
	}
	Write-Host ''

	$selectedIndex = Read-Host 'Índice'

	$opt = $opts[$selectedIndex]
	$opt.Keys | ForEach-Object{
		Set-Variable -Name $_ -Value $opt[$_] -Scope Global
	}
}