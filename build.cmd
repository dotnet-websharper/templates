@echo off

setlocal
set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin;%PATH%
set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin;%PATH%
set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin;%PATH%

set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft SDKs\F#\4.1\Framework\v4.0
set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft SDKs\F#\4.0\Framework\v4.0
set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft SDKs\F#\3.1\Framework\v4.0
set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft SDKs\F#\3.0\Framework\v4.0
set PATH=%PATH%;%ProgramFiles%\Microsoft SDKs\F#\4.1\Framework\v4.0
set PATH=%PATH%;%ProgramFiles%\Microsoft SDKs\F#\4.0\Framework\v4.0
set PATH=%PATH%;%ProgramFiles%\Microsoft SDKs\F#\3.1\Framework\v4.0
set PATH=%PATH%;%ProgramFiles%\Microsoft SDKs\F#\3.0\Framework\v4.0

.paket\paket.bootstrapper.exe
.paket\paket update

fsi.exe --exec init.fsx

rem rd /s /q WebSharper.Vsix/bin
rem rd /s /q WebSharper.Vsix/obj
rem MSBuild.exe WebSharper.Vsix.sln /p:Configuration=Release /t:Rebuild
