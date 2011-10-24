@ECHO OFF
SET /p ver= What is the version number?

EmailNotification\nuget.exe pack -sym EmailNotification\EmailNotification.csproj
EmailNotification\nuget.exe push EmailNotification.%ver%.nupkg
pause