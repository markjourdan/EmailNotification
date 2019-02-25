
$ver = Read-Host 'What is the version number?'

$currentDir = Split-Path $MyInvocation.MyCommand.Path -Parent

$parentDir = Split-Path $currentDir -Parent

$nuget = $currentDir + "\nuget.exe"
$project = $parentDir + "\src\EmailNotification\EmailNotification.csproj"

Invoke-Expression "& '$($currentDir)\nuget.exe' pack -Version $($ver) -symbols -outputdirectory '$($currentDir)\release' '$($project)'"

Invoke-Expression "& '$($currentDir)\nuget.exe' push '$($currentDir)\release\EmailNotification.$($ver).nupkg' -Source https://api.nuget.org/v3/index.json -NonInteractive"

Read-Host "Press any key to continue..."