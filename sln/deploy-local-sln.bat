set "targetdirectory=Q:\DbMigrator2\LastRelease"
set "solutiondir=%cd%\"

set "csprojPath=DbMigrator.Console\DbMigrator.Console.csproj"
set "projPath=DbMigrator.Console\"
set "projName=DbMigrator.Console"
set "projType=library"
call deploy-local-proj.bat
if not %errorlevel%== 0 goto DeployError

goto DeployEnd
:DeployError

echo Error!
pause
exit /b 1

:DeployEnd