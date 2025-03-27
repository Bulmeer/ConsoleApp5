using NUnit.Framework;
using System;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class ExceptionTests
    {
        [Test]
        public void CarApplicationException_DefaultConstructor_SetsCorrectMessage()
        {
            // Act
            var exception = new CarApplicationException();
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Произошла ошибка в автомобильном приложении"));
        }
        
        [Test]
        public void CarApplicationException_MessageConstructor_SetsCorrectMessage()
        {
            // Arrange
            string message = "Тестовое сообщение об ошибке";
            
            // Act
            var exception = new CarApplicationException(message);
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo(message));
        }
        
        [Test]
        public void CarApplicationException_MessageAndErrorCodeConstructor_SetsCorrectProperties()
        {
            // Arrange
            string message = "Тестовое сообщение об ошибке";
            string errorCode = "ERR-001";
            
            // Act
            var exception = new CarApplicationException(message, errorCode);
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.ErrorCode, Is.EqualTo(errorCode));
        }
        
        [Test]
        public void ConfigurationException_DefaultConstructor_SetsCorrectMessage()
        {
            // Act
            var exception = new ConfigurationException();
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Произошла ошибка при работе с конфигурацией"));
        }
        
        [Test]
        public void ConfigurationException_InheritsFromCarApplicationException()
        {
            // Act
            var exception = new ConfigurationException();
            
            // Assert
            Assert.That(exception, Is.InstanceOf<CarApplicationException>());
        }
        
        [Test]
        public void CarPartException_DefaultConstructor_SetsCorrectMessage()
        {
            // Act
            var exception = new CarPartException();
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Произошла ошибка при работе с деталью автомобиля"));
        }
        
        [Test]
        public void CarPartException_MessageAndPartNameConstructor_SetsCorrectProperties()
        {
            // Arrange
            string message = "Ошибка с деталью";
            string partName = "Двигатель";
            
            // Act
            var exception = new CarPartException(message, partName);
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.PartName, Is.EqualTo(partName));
        }
    }
} 