@echo off
echo ====================================
echo Multi Telegram Manager - Build Script
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

echo [1/4] Cleaning previous builds...
if exist "MultiTelegram\bin" rmdir /s /q "MultiTelegram\bin"
if exist "MultiTelegram\obj" rmdir /s /q "MultiTelegram\obj"
if exist "Release" rmdir /s /q "Release"

echo [2/4] Restoring dependencies...
dotnet restore MultiTelegram.sln
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore dependencies!
    pause
    exit /b 1
)

echo [3/4] Building release version...
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo [4/4] Preparing release package...
mkdir Release
copy "MultiTelegram\bin\Release\net6.0-windows\win-x64\publish\MultiTelegram.exe" "Release\"
copy "README.md" "Release\"
copy "INSTALL_WINDOWS.md" "Release\"

echo.
echo ====================================
echo BUILD COMPLETED SUCCESSFULLY!
echo ====================================
echo.
echo Release files are in the 'Release' folder:
echo - MultiTelegram.exe (Main executable)
echo - README.md (Documentation)
echo - INSTALL_WINDOWS.md (Installation guide)
echo.
echo File size: 
for %%i in (Release\MultiTelegram.exe) do echo MultiTelegram.exe: %%~zi bytes (%%~zi bytes / 1024 / 1024 = ~%%~zi MB)
echo.
echo You can now distribute the MultiTelegram.exe file!
echo.
pause