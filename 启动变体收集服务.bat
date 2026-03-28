@echo off
title Shader Variant Collection Server

:: Check if port 8880 is already in use
netstat -ano | findstr ":8880" | findstr "LISTENING" >nul
if %errorlevel%==0 (
    echo ========================================
    echo   Server is already running
    echo ========================================
    echo.
    echo Press any key to close...
    pause >nul
    exit
)

:: Start server
echo ========================================
echo   Starting server...
echo ========================================
echo.
cd /d "%~dp0"
call npm start

:: Keep window open if server exits
echo.
echo ========================================
echo   Server stopped
echo ========================================
echo.
echo Press any key to close...
pause >nul
