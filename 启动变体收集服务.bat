@echo off
title Shader Variant Collection Server

:: Check if port 8880 is already in use
netstat -ano | findstr ":8880" | findstr "LISTENING" >nul
if %errorlevel%==0 (
    echo ========================================
    echo   Server is already running
    echo ========================================
    echo.
    timeout /t 2 >nul
    exit
)

:: Change to script directory
cd /d "%~dp0"

:: Install dependencies if node_modules doesn't exist
if not exist "node_modules" (
    echo ========================================
    echo   Installing dependencies...
    echo ========================================
    echo.
    call npm install
    echo.
)

:: Start server in background with custom window title
echo ========================================
echo   Starting server...
echo ========================================
echo.
start "Shader Variant Collection Server" /b node server.js
echo   Server started on port 8880
echo ========================================
timeout /t 2 >nul
exit