
$msbuild = "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSbuild.exe"

$buildOptions = "/t:rebuild /p:Configuration=Release /p:VersionNumber=0.3.0.0"

Invoke-Expression $msbuild EmailNotification.sln 


