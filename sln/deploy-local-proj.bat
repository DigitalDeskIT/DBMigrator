C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %csprojPath% /p:Configuration=Release;DeployOnBuild=True /p:SolutionDir=%solutiondir%
if not %errorlevel%== 0 exit /b 1

set "targetprojpath=%targetdirectory%\%projName%"

if not %projType%==website goto EndCleanProjTypeWebsite
	del "%projPath%\obj\Release\Package\PackageTmp\bin\*.xml"
	del "%projPath%\obj\Release\Package\PackageTmp\bin\*.pdb"
	
	rmdir "%targetprojpath%" /s /q
	mkdir "%targetprojpath%"
	xcopy "%projPath%\obj\Release\Package\PackageTmp" "%targetprojpath%" /s /i
	if not %errorlevel%== 0 exit /b 1
:EndCleanProjTypeWebsite

if not %projType%==library goto EndCleanProjTypeLibrary
	del "%projPath%\bin\Release\*.xml"
	del "%projPath%\bin\Release\*.pdb"
	
	rmdir "%targetprojpath%" /s /q
	mkdir "%targetprojpath%"
	xcopy "%projPath%\bin\Release" "%targetprojpath%" /s /i
	if not %errorlevel%== 0 exit /b 1
:EndCleanProjTypeLibrary

if not %errorlevel%== 0 exit /b 1