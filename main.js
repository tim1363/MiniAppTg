const { app, BrowserWindow, Menu, ipcMain, session } = require('electron');
const path = require('path');
const { v4: uuidv4 } = require('uuid');

class MultiTelegramApp {
    constructor() {
        this.windows = new Map();
        this.setupApp();
    }

    setupApp() {
        // Отключаем аппаратное ускорение для совместимости
        app.disableHardwareAcceleration();
        
        app.whenReady().then(() => {
            this.createMainWindow();
            this.setupMenu();
        });

        app.on('window-all-closed', () => {
            if (process.platform !== 'darwin') {
                app.quit();
            }
        });

        app.on('activate', () => {
            if (BrowserWindow.getAllWindows().length === 0) {
                this.createMainWindow();
            }
        });

        // Обработчики IPC
        ipcMain.handle('create-telegram-instance', () => {
            return this.createTelegramInstance();
        });

        ipcMain.handle('get-instances', () => {
            return Array.from(this.windows.keys());
        });

        ipcMain.handle('close-instance', (event, instanceId) => {
            return this.closeInstance(instanceId);
        });
    }

    createMainWindow() {
        const mainWindow = new BrowserWindow({
            width: 1200,
            height: 800,
            webPreferences: {
                nodeIntegration: true,
                contextIsolation: false,
                enableRemoteModule: true
            },
            title: 'Multi Telegram Manager',
            icon: path.join(__dirname, 'icon.ico')
        });

        mainWindow.loadFile('manager.html');
        
        if (process.argv.includes('--debug')) {
            mainWindow.webContents.openDevTools();
        }
    }

    async createTelegramInstance() {
        const instanceId = uuidv4();
        
        // Создаем уникальную сессию для каждого экземпляра
        const partition = `telegram-${instanceId}`;
        const ses = session.fromPartition(partition, { cache: true });
        
        // Настраиваем уникальный User-Agent для обхода детекции
        const userAgents = [
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36',
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0',
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/120.0.0.0 Safari/537.36'
        ];
        
        const randomUA = userAgents[Math.floor(Math.random() * userAgents.length)];
        
        // Модифицируем заголовки для каждого экземпляра
        ses.webRequest.onBeforeSendHeaders((details, callback) => {
            details.requestHeaders['User-Agent'] = randomUA;
            details.requestHeaders['Accept-Language'] = 'ru-RU,ru;q=0.9,en;q=0.8';
            
            // Добавляем случайные заголовки для большей уникальности
            details.requestHeaders['X-Forwarded-For'] = this.generateRandomIP();
            details.requestHeaders['X-Real-IP'] = this.generateRandomIP();
            
            callback({ requestHeaders: details.requestHeaders });
        });

        // Создаем окно для Telegram
        const telegramWindow = new BrowserWindow({
            width: 420,
            height: 700,
            webPreferences: {
                session: ses,
                nodeIntegration: false,
                contextIsolation: true,
                webSecurity: true,
                partition: partition
            },
            title: `Telegram Instance ${instanceId.slice(0, 8)}`,
            icon: path.join(__dirname, 'telegram-icon.ico')
        });

        // Загружаем Telegram Web
        await telegramWindow.loadURL('https://web.telegram.org/k/');
        
        // Сохраняем экземпляр
        this.windows.set(instanceId, {
            window: telegramWindow,
            session: ses,
            userAgent: randomUA,
            created: new Date()
        });

        // Обработчик закрытия окна
        telegramWindow.on('closed', () => {
            this.windows.delete(instanceId);
        });

        return {
            instanceId,
            userAgent: randomUA,
            created: new Date()
        };
    }

    generateRandomIP() {
        return `${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}`;
    }

    closeInstance(instanceId) {
        const instance = this.windows.get(instanceId);
        if (instance) {
            instance.window.close();
            this.windows.delete(instanceId);
            return true;
        }
        return false;
    }

    setupMenu() {
        const template = [
            {
                label: 'Файл',
                submenu: [
                    {
                        label: 'Новый экземпляр Telegram',
                        accelerator: 'Ctrl+N',
                        click: () => {
                            this.createTelegramInstance();
                        }
                    },
                    { type: 'separator' },
                    {
                        label: 'Выход',
                        accelerator: process.platform === 'darwin' ? 'Cmd+Q' : 'Ctrl+Q',
                        click: () => {
                            app.quit();
                        }
                    }
                ]
            },
            {
                label: 'Вид',
                submenu: [
                    { role: 'reload', label: 'Перезагрузить' },
                    { role: 'forceReload', label: 'Принудительная перезагрузка' },
                    { role: 'toggleDevTools', label: 'Инструменты разработчика' },
                    { type: 'separator' },
                    { role: 'resetZoom', label: 'Сбросить масштаб' },
                    { role: 'zoomIn', label: 'Увеличить' },
                    { role: 'zoomOut', label: 'Уменьшить' },
                    { type: 'separator' },
                    { role: 'togglefullscreen', label: 'Полноэкранный режим' }
                ]
            }
        ];

        const menu = Menu.buildFromTemplate(template);
        Menu.setApplicationMenu(menu);
    }
}

// Запускаем приложение
new MultiTelegramApp();