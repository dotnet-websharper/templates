@echo off
setlocal
    
dotnet tool restore
dotnet paket update
if errorlevel 1 exit /b %errorlevel%
    
dotnet fake run build.fsx -t %*