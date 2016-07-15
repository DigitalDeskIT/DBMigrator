function SetDefaultUserData {
	$global:userDataKeys | ForEach-Object {
		Set-Variable -Name $_ -Value '' -Scope global
	}
}

function SaveUserData {
	if (Test-Path $global:userFile){
		Remove-Item $global:userFile
	}
	$global:userDataKeys | ForEach-Object{
		$var = Get-Variable -Name $_ -Scope Global
		
		($var.Name+'|'+$var.Value) | Out-File $global:userFile -Append
	}
}

function PrintUserData {
	
	Write-Host ''
	Write-Host 'VARIÁVEIS'
	$global:userDataKeys | ForEach-Object{
		$var = Get-Variable -Name $_ -Scope Global
		Write-Host $_ : $var.Value
	}
	Write-Host ''
}

function LoadUserData{
	if (Test-Path $global:userFile){
		Get-Content $global:userFile | ForEach-Object{
			if ($_){
			
				$data = $_ -split '[|]'
				Set-Variable -Name $data[0] -Value $data[1] -Scope Global
			}
		}
	}
}