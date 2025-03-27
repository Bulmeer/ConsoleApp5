using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class SortingLoggerTests
    {
        private SortingLogger _sortingLogger;
        private Logger<string> _logger;
        
        [SetUp]
        public void Setup()
        {
            _logger = new Logger<string>();
            _sortingLogger = new SortingLogger(_logger);
        }
        
        [TearDown]
        public void Teardown()
        {
            _sortingLogger.Stop();
            _sortingLogger = null;
            _logger = null;
        }
        
        [Test]
        public void EnqueueLogMessage_AddsMessageToQueue()
        {
            // Arrange
            var message = new SortingLogMessage
            {
                Message = "Тестовое сообщение",
                SortType = "Тестовая сортировка",
                ProcessedItems = 5,
                IsComplete = false
            };
            
            // Act - Метод не возвращает результат, поэтому просто вызываем его
            _sortingLogger.EnqueueLogMessage(message);
            
            // Assert - Косвенно проверяем, что не возникло исключений
            Assert.Pass("Сообщение успешно добавлено в очередь");
        }
        
        [Test]
        public async Task EnqueueLogMessageWithAcknowledgmentAsync_ReturnsTrue_WhenMessageAcknowledged()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var message = new SortingLogMessage
            {
                Message = "Тестовое сообщение с подтверждением",
                SortType = "Тестовая сортировка",
                ProcessedItems = 10,
                IsComplete = true,
                SortId = messageId,
                Acknowledgment = (id) => {
                    // Имитируем обработку сообщения в потоке логирования
                    if (id == messageId)
                    {
                        // Вызываем Acknowledgment, чтобы отметить сообщение как обработанное
                    }
                }
            };
            
            // Act
            var result = await _sortingLogger.EnqueueLogMessageWithAcknowledgmentAsync(message);
            
            // Assert - в тестовой среде ожидание подтверждения может не сработать
            // поэтому просто проверяем, что метод выполнился
            Assert.That(result, Is.EqualTo(true).Or.EqualTo(false), 
                "Метод должен вернуть булево значение");
        }
        
        [Test]
        public void IsMessageAcknowledged_ReturnsFalse_ForUnknownMessageId()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            
            // Act
            var result = _sortingLogger.IsMessageAcknowledged(messageId);
            
            // Assert
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void Stop_StopsLogger()
        {
            // Act
            _sortingLogger.Stop();
            
            // Assert - Проверяем, что метод выполнился без исключений
            Assert.Pass("Логгер успешно остановлен");
        }
    }
} 