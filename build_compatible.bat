@echo off
echo ====================================
echo Multi Telegram Manager - Compatible Build
echo ====================================
echo.

REM Check if .NET 6.0 SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 6.0 SDK is not installed!
    echo Please install .NET 6.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
if exist "MultiTelegram\bin" rmdir /s /q "MultiTelegram\bin"
if exist "MultiTelegram\obj" rmdir /s /q "MultiTelegram\obj"
if exist "Release" rmdir /s /q "Release"

echo [2/5] Restoring dependencies...
dotnet restore MultiTelegram.sln
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore dependencies!
    echo Try running: dotnet nuget locals all --clear
    pause
    exit /b 1
)

echo [3/5] Building in Debug mode first...
dotnet build MultiTelegram.sln -c Debug -v minimal
if %errorlevel% neq 0 (
    echo ERROR: Debug build failed!
    echo Check the error messages above
    pause
    exit /b 1
)

echo [4/5] Building release version...
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
if %errorlevel% neq 0 (
    echo ERROR: Release build failed!
    pause
    exit /b 1
)

echo [5/5] Preparing release package...
mkdir Release
copy "MultiTelegram\bin\Release\net6.0-windows\win-x64\publish\MultiTelegram.exe" "Release\"
copy "README.md" "Release\"
copy "QUICK_START.md" "Release\"

echo.
echo ====================================
echo BUILD COMPLETED SUCCESSFULLY!
echo ====================================
echo.
echo Release files are in the 'Release' folder:
dir Release
echo.
echo You can now run MultiTelegram.exe!
echo.
pause