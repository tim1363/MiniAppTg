@echo off
echo 🚀 Multi Telegram - Сборка Windows приложения
echo ================================================

echo.
echo 📦 Проверка .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET SDK не найден! Установите .NET 6.0 или выше
    echo 📥 Скачать: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✅ .NET SDK найден

echo.
echo 🔧 Восстановление зависимостей...
dotnet restore MultiTelegram.sln
if errorlevel 1 (
    echo ❌ Ошибка восстановления зависимостей
    pause
    exit /b 1
)

echo.
echo 🏗️ Сборка приложения (Release)...
dotnet build MultiTelegram.sln --configuration Release --no-restore
if errorlevel 1 (
    echo ❌ Ошибка сборки
    pause
    exit /b 1
)

echo.
echo 📦 Создание исполняемого файла...
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
if errorlevel 1 (
    echo ❌ Ошибка создания exe файла
    pause
    exit /b 1
)

echo.
echo ✅ Сборка завершена успешно!
echo.
echo 📁 Файлы находятся в:
echo    MultiTelegram\bin\Release\net6.0-windows\win-x64\publish\
echo.
echo 🎯 Готовый файл: MultiTelegram.exe
echo.

pause