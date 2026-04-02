@echo off
title Close Shader Variant Collection Server

echo ========================================
echo   Checking server status...
echo ========================================
echo.

:: Find process on port 8880
for /f "tokens=5" %%a in ('netstat -ano 2^>nul ^| findstr ":8880" ^| findstr "LISTENING"') do (
    echo Found process PID: %%a
    echo Stopping...
    taskkill /PID %%a /F
    echo.
    echo ========================================
    echo   Server stopped
    echo ========================================
    goto :done
)

echo ========================================
echo   Server is not running
echo ========================================

:done
exit