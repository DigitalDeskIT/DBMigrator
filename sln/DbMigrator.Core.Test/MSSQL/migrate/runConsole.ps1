. '.\fn.UserData.ps1'
. '.\fn.Db.ps1'
. '.\fn.DbMigrate.ps1'
. '.\fn.Initialize.ps1'

$config = 'core'

Initialize $config

function RequestOption{
	Write-Host ('
-------------------------------------------------------------------------------

Configurações Atuais:

- DB Conexao          : '+$global:DB_NiceName+'
- DBM Instancia Atual : '+$global:DBM_Instance+'

Opções:

 1. [M]igrar
 2. [M]igrar [I]nterativo
 3. [M]igracao de [T]este
 4. [R]ecriar Banco
 5. [D]eletar banco
 6. [N]ovo banco
 7. [I]nstancia
 8. [A]pontar para banco
 9. [E]xit

')
	
	$option = Read-Host
	Write-Host ''
	
	if ( $option -imatch '^(M|1)$' ){
		Write-Host 'Migrar'
		DBMMigrate -Mode migrate
	}
	elseif ($option -imatch '^(MI|2)$'){
		Write-Host 'Migrar Interativo'
		DBMMigrate -Mode interactive
	}
	if ( $option -imatch '^(MT|3)$' ){
		Write-Host 'Migração de Teste'
		DBMMigrate -Mode test
	}
	elseif ($option -imatch '^(R|4)$'){
		Write-Host 'Recriar Banco'
		DBDrop
		$server = $global:DB_Server+'\'+$global:DB_Instance
		DBCreate -Server $server -Name $global:DB_Name
		DBMMigrate
	}
	elseif ($option -imatch '^(D|5)$'){
		Write-Host 'Deletar Banco'
		DBDrop
	}
	elseif ($option -imatch '^(N|6)$'){
		Write-Host 'Novo Banco'
		DBCreate
	}
	elseif ($option -imatch '^(I|7)$'){
		Write-Host 'Alterar Instância'
		DBMSetInstance
		SaveUserData $userDataKeys
	}
	elseif ($option -imatch '^(A|8)$'){
		Write-Host 'Apontar para Banco'
		DBSetup
		SaveUserData $userDataKeys
	}
	elseif ($option -imatch '^(E|9)$'){ exit 0 }
	return $option
}

while (1 -eq 1){
	RequestOption 
}