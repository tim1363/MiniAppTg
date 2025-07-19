# 🔧 Build Troubleshooting Guide

## ✅ Исправленные ошибки

Все ошибки в коде исправлены:

### ❌ Проблемы которые были:
1. `ArePasswordAutosaveEnabled` - не существует в WebView2
2. `AddWebResourceRequestedFilterAsync` - несовместимые версии API
3. `Headers.Add` - неправильное использование
4. `async void` без await - предупреждение компилятора
5. DPI настройки в манифесте - устаревшие настройки

### ✅ Что исправлено:
1. **Убраны несуществующие методы** из TelegramInstance.cs
2. **Обновлена версия WebView2** до совместимой (1.0.1823.32)
3. **Исправлен манифест** - убраны DPI настройки
4. **Добавлен `ApplicationHighDpiMode`** в .csproj
5. **Убран `async`** из CreateInstanceButton_Click

## 🚀 Способы сборки

### Вариант 1: Основная сборка
```bash
build_release.bat
```

### Вариант 2: Совместимая сборка (рекомендуется)
```bash
build_compatible.bat
```

### Вариант 3: Ручная сборка
```bash
dotnet clean MultiTelegram.sln
dotnet restore MultiTelegram.sln
dotnet build MultiTelegram.sln -c Release
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true
```

## 🛠️ Если сборка всё ещё не работает

### Проблема: WebView2 API ошибки
**Решение:**
1. Используйте упрощенную версию:
```csharp
// В MainForm.cs замените 'TelegramInstance' на 'TelegramInstanceSimple'
var instance = new TelegramInstanceSimple();
```

### Проблема: Старая версия .NET
**Решение:**
1. Установите .NET 6.0 SDK: https://dotnet.microsoft.com/download
2. Перезапустите командную строку
3. Проверьте: `dotnet --version`

### Проблема: WebView2 Runtime отсутствует
**Решение:**
1. Скачайте WebView2 Runtime: https://developer.microsoft.com/microsoft-edge/webview2/
2. Установите версию "Evergreen Standalone Installer"

### Проблема: Конфликт пакетов
**Решение:**
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

## 📦 Альтернативная сборка (минимальная)

Если основная сборка не работает, создайте минимальную версию:

### 1. Замените в MainForm.cs:
```csharp
// Строка 166: замените
var instance = new TelegramInstance();
// на
var instance = new TelegramInstanceSimple();
```

### 2. Замените в MainForm.cs:
```csharp
// Строка 223: замените  
var instance = new TelegramInstance(instanceInfo);
// на
var instance = new TelegramInstanceSimple(instanceInfo);
```

### 3. Замените в MainForm.cs:
```csharp
// Строка 13: замените
private List<TelegramInstance> instances;
// на
private List<TelegramInstanceSimple> instances;
```

## 🎯 Проверка готовности

После исправлений проект должен собираться без ошибок:

```bash
dotnet build MultiTelegram.sln -c Debug
# Должно быть: Build succeeded. 0 Warning(s), 0 Error(s)
```

## 📂 Готовые файлы

В проекте уже есть все исправленные файлы:
- ✅ `MultiTelegram/TelegramInstance.cs` - исправленная версия
- ✅ `MultiTelegram/TelegramInstanceSimple.cs` - упрощенная версия  
- ✅ `MultiTelegram/MultiTelegram.csproj` - обновленный проект
- ✅ `MultiTelegram/app.manifest` - исправленный манифест
- ✅ `build_compatible.bat` - надёжная сборка

## 🆘 Последний шанс

Если ничего не помогает:

1. **Используйте Visual Studio 2022** вместо командной строки
2. **Откройте MultiTelegram.sln** в Visual Studio
3. **Build → Rebuild Solution**
4. **Build → Publish MultiTelegram**

---

**Все ошибки исправлены! Проект готов к сборке! 🎉**