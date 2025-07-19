@echo off
chcp 65001 >nul 2>&1
echo Multi Telegram - Windows Application Build
echo ==========================================

echo.
echo [1/4] Checking .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found! Please install .NET 6.0 or higher
    echo Download: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo SUCCESS: .NET SDK found

echo.
echo [2/4] Restoring dependencies...
dotnet restore MultiTelegram.sln
if errorlevel 1 (
    echo ERROR: Failed to restore dependencies
    pause
    exit /b 1
)

echo.
echo [3/4] Building application (Release)...
dotnet build MultiTelegram.sln --configuration Release --no-restore
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo [4/4] Creating executable file...
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
if errorlevel 1 (
    echo ERROR: Failed to create exe file
    pause
    exit /b 1
)

echo.
echo SUCCESS: Build completed successfully!
echo.
echo Output directory:
echo    MultiTelegram\bin\Release\net6.0-windows\win-x64\publish\
echo.
echo Executable file: MultiTelegram.exe
echo.

pause