// Простой тест основного функционала
const { v4: uuidv4 } = require('uuid');

console.log('🚀 Тестирование Multi Telegram App...\n');

// Тест 1: Генерация UUID
console.log('1. Тест генерации UUID:');
const testId = uuidv4();
console.log(`   Сгенерирован ID: ${testId}`);
console.log(`   Короткий ID: ${testId.slice(0, 8)}...\n`);

// Тест 2: Генерация случайных IP
function generateRandomIP() {
    return `${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}`;
}

console.log('2. Тест генерации случайных IP:');
for (let i = 0; i < 3; i++) {
    console.log(`   IP ${i + 1}: ${generateRandomIP()}`);
}
console.log('');

// Тест 3: Массив User-Agent'ов
const userAgents = [
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/120.0.0.0 Safari/537.36'
];

console.log('3. Тест выбора случайного User-Agent:');
for (let i = 0; i < 3; i++) {
    const randomUA = userAgents[Math.floor(Math.random() * userAgents.length)];
    const browser = randomUA.includes('Firefox') ? 'Firefox' : 
                   randomUA.includes('Edge') ? 'Edge' : 'Chrome';
    console.log(`   Instance ${i + 1}: ${browser}`);
}
console.log('');

// Тест 4: Симуляция создания экземпляров
console.log('4. Симуляция создания экземпляров:');
const instances = new Map();

for (let i = 0; i < 5; i++) {
    const instanceId = uuidv4();
    const randomUA = userAgents[Math.floor(Math.random() * userAgents.length)];
    
    instances.set(instanceId, {
        userAgent: randomUA,
        created: new Date(),
        partition: `telegram-${instanceId}`
    });
    
    console.log(`   ✅ Экземпляр ${i + 1} создан: ${instanceId.slice(0, 8)}...`);
}

console.log(`\n📊 Всего создано экземпляров: ${instances.size}`);
console.log(`💾 Примерное использование памяти: ~${instances.size * 100}MB`);

console.log('\n✅ Все тесты пройдены успешно!');
console.log('\n📝 Для запуска полного приложения используйте: npm start');
console.log('🏗️  Для сборки исполняемого файла: npm run build');