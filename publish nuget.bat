@ECHO OFF
SET /p ver= What is the version number?

EmailNotification\nuget.exe pack -Version %ver% -symbols EmailNotification\EmailNotification.csproj
EmailNotification\nuget.exe push .\EmailNotification.%ver%.nupkg
pause