@echo off
title Close Shader Variant Collection Server

echo ========================================
echo   Checking server status...
echo ========================================
echo.

:: Find and kill process on port 8880
set "found=0"
for /f "tokens=5" %%a in ('netstat -ano 2^>nul ^| findstr ":8880" ^| findstr "LISTENING"') do (
    if not "%%a"=="" (
        echo Found process PID: %%a
        echo Stopping...
        taskkill /PID %%a /F
        set "found=1"
    )
)

echo.
if "%found%"=="0" (
    echo ========================================
    echo   Server is not running
    echo ========================================
) else (
    echo ========================================
    echo   Server stopped
    echo ========================================
)

echo.
echo Press any key to close...
pause >nul
