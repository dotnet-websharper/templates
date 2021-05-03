@echo off
setlocal
    
dotnet tool restore
dotnet paket update
if errorlevel 1 exit /b %errorlevel%
    
:: Allow running `build SomeTask` instead of `build -t SomeTask`
set _Add-t=""
set FirstArg=%1
if not "%FirstArg%"=="" if not "%FirstArg:~0,1%"=="-" set _Add-t=1
if "%_Add-t%"=="1" (
  dotnet fake run build.fsx -t %*
) else (
  dotnet fake run build.fsx %*
)
