@ECHO OFF
@echo SET /p ver= What is the version number?

SET mypath = %cd%

@echo EmailNotification\nuget.exe pack -Version %ver% -symbols EmailNotification\EmailNotification.csproj
@echo EmailNotification\nuget.exe push .\EmailNotification.%ver%.nupkg
pause