
nuget.ext

$msbuild = "$Env:Programfiles (x86)\Microsoft Visual Studio\2017\buildtools\msbuild\15.0\Bin\MSbuild.exe"
$solution = $PSScriptRoot + "\EmailNotification.sln"


& $msbuild $solution /t:rebuild /p:Configuration=Release /p:VersionNumber=0.3.0.0


