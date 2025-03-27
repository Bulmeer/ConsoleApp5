using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ConsoleApp5.Tests
{
    [TestFixture]                            // Атрибут, указывающий что класс содержит тесты
    public class LoggerTests                 // Класс тестов для проверки логгера
    {
        private Logger<string> _logger;      // Логгер для тестирования
        private List<string> _loggedMessages; // Список для хранения залогированных сообщений
        
        [SetUp]                              // Метод, вызываемый перед каждым тестом
        public void Setup()                  // Метод для настройки тестового окружения
        {
            _logger = new Logger<string>();  // Создаём новый логгер с типом сообщений string
            _loggedMessages = new List<string>(); // Создаём пустой список для сообщений
            
            // Добавляем обработчик события логирования для проверки
            _logger.LogEvent += (message) => _loggedMessages.Add(message); // Подписываемся на событие логирования, сохраняя сообщения в список
        }
        
        [TearDown]                           // Метод, вызываемый после каждого теста
        public void Teardown()               // Метод для очистки тестового окружения
        {
            _logger = null;                  // Очищаем ссылку на логгер
            _loggedMessages = null;          // Очищаем ссылку на список сообщений
        }
        
        [Test]                               // Атрибут, указывающий что метод является тестом
        public void Log_AddsMessageToLoggedMessages() // Тест проверяет логирование сообщения
        {
            // Arrange
            string testMessage = "Тестовое сообщение"; // Создаём тестовое сообщение
            
            // Act
            _logger.Log(testMessage);        // Логируем тестовое сообщение
            
            // Assert
            Assert.That(_loggedMessages, Has.Count.EqualTo(1)); // Проверяем, что в список добавлено одно сообщение
            Assert.That(_loggedMessages[0], Is.EqualTo(testMessage)); // Проверяем, что сообщение соответствует отправленному
        }
        
        [Test]                               // Атрибут для тестового метода
        public void LogInfo_AddsMessageToLoggedMessages() // Тест проверяет логирование информационного сообщения
        {
            // Arrange
            string testMessage = "Информационное сообщение"; // Создаём тестовое информационное сообщение
            
            // Act
            _logger.LogInfo(testMessage);    // Логируем информационное сообщение
            
            // Assert
            Assert.That(_loggedMessages, Has.Count.EqualTo(1)); // Проверяем, что в список добавлено одно сообщение
            Assert.That(_loggedMessages[0], Is.EqualTo(testMessage)); // Проверяем, что сообщение соответствует отправленному
        }
        
        [Test]                               // Атрибут для тестового метода
        public void LogWarning_AddsMessageToLoggedMessages() // Тест проверяет логирование предупреждения
        {
            // Arrange
            string testMessage = "Предупреждение"; // Создаём тестовое предупреждение
            
            // Act
            _logger.LogWarning(testMessage); // Логируем предупреждение
            
            // Assert
            Assert.That(_loggedMessages, Has.Count.EqualTo(1)); // Проверяем, что в список добавлено одно сообщение
            Assert.That(_loggedMessages[0], Is.EqualTo(testMessage)); // Проверяем, что сообщение соответствует отправленному
        }
        
        [Test]                               // Атрибут для тестового метода
        public void LogError_AddsMessageToLoggedMessages() // Тест проверяет логирование ошибки
        {
            // Arrange
            string testMessage = "Ошибка";   // Создаём тестовое сообщение об ошибке
            
            // Act
            _logger.LogError(testMessage);   // Логируем ошибку
            
            // Assert
            Assert.That(_loggedMessages, Has.Count.EqualTo(1)); // Проверяем, что в список добавлено одно сообщение
            Assert.That(_loggedMessages[0], Is.EqualTo(testMessage)); // Проверяем, что сообщение соответствует отправленному
        }
        
        [Test]                               // Атрибут для тестового метода
        public void MultipleLogsWithDifferentMethods_AddsAllMessagesToLoggedMessages() // Тест проверяет многократное логирование разными методами
        {
            // Arrange
            string infoMessage = "Информация"; // Создаём информационное сообщение
            string warningMessage = "Предупреждение"; // Создаём предупреждающее сообщение
            string errorMessage = "Ошибка";  // Создаём сообщение об ошибке
            
            // Act
            _logger.LogInfo(infoMessage);    // Логируем информационное сообщение
            _logger.LogWarning(warningMessage); // Логируем предупреждение
            _logger.LogError(errorMessage);  // Логируем ошибку
            
            // Assert
            Assert.That(_loggedMessages, Has.Count.EqualTo(3)); // Проверяем, что в список добавлено три сообщения
            Assert.That(_loggedMessages, Does.Contain(infoMessage)); // Проверяем наличие информационного сообщения
            Assert.That(_loggedMessages, Does.Contain(warningMessage)); // Проверяем наличие предупреждения
            Assert.That(_loggedMessages, Does.Contain(errorMessage)); // Проверяем наличие сообщения об ошибке
        }
        
        [Test]                               // Атрибут для тестового метода
        public void LogEvent_NotRegistered_DoesNotThrowException() // Тест проверяет отсутствие исключений при логировании без подписчиков
        {
            // Arrange
            var newLogger = new Logger<string>(); // Создаём новый логгер без подписчиков
            
            // Act & Assert
            Assert.That(() => newLogger.Log("Test message"), Throws.Nothing); // Проверяем, что метод не выбрасывает исключений
        }
    }
} 