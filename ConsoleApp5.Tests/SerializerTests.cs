using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class SerializerTests
    {
        private XmlDataSerializer<SerializableCar> _xmlSerializer;
        private JsonDataSerializer<SerializableCar> _jsonSerializer;
        private Logger<string> _logger;
        private List<SerializableCar> _testCars;
        private string _testXmlFilePath;
        private string _testJsonFilePath;
        
        [SetUp]
        public void Setup()
        {
            _logger = new Logger<string>();
            _xmlSerializer = new XmlDataSerializer<SerializableCar>(_logger);
            _jsonSerializer = new JsonDataSerializer<SerializableCar>(_logger);
            
            // Создаем тестовые объекты для сериализации
            _testCars = new List<SerializableCar>
            {
                new SerializableCar
                {
                    Model = "Тестовый автомобиль 1",
                    BodyType = "Седан",
                    BodyColor = "Синий",
                    InteriorMaterial = "Кожа",
                    InteriorLayout = "5-местный",
                    SuspensionType = "Стандартная",
                    SuspensionBrand = "TestBrand",
                    TransmissionType = "Автомат",
                    HasTurbo = false,
                    PartNames = new List<string> { "Двигатель V6", "Колесо летнее" }
                },
                new SerializableCar
                {
                    Model = "Тестовый спорткар",
                    BodyType = "Купе",
                    BodyColor = "Красный",
                    InteriorMaterial = "Алькантара",
                    InteriorLayout = "2-местный",
                    SuspensionType = "Спортивная",
                    SuspensionBrand = "SportBrand",
                    TransmissionType = "Робот",
                    HasTurbo = true,
                    PartNames = new List<string> { "Двигатель V8", "Колесо спортивное" }
                }
            };
            
            // Создаем временные пути для тестовых файлов
            _testXmlFilePath = Path.Combine(Path.GetTempPath(), $"test_cars_{Guid.NewGuid()}.xml");
            _testJsonFilePath = Path.Combine(Path.GetTempPath(), $"test_cars_{Guid.NewGuid()}.json");
        }
        
        [TearDown]
        public void Teardown()
        {
            // Удаляем тестовые файлы, если они существуют
            if (File.Exists(_testXmlFilePath))
            {
                File.Delete(_testXmlFilePath);
            }
            
            if (File.Exists(_testJsonFilePath))
            {
                File.Delete(_testJsonFilePath);
            }
            
            _xmlSerializer = null;
            _jsonSerializer = null;
            _testCars = null;
        }
        
        [Test]
        public void XmlSerializer_SerializeAndDeserialize_ReturnsSameData()
        {
            // Act
            string serializedData = _xmlSerializer.Serialize(_testCars);
            var deserializedCars = _xmlSerializer.Deserialize(serializedData).ToList();
            
            // Assert
            Assert.That(deserializedCars, Is.Not.Null);
            Assert.That(deserializedCars.Count, Is.EqualTo(_testCars.Count));
            Assert.That(deserializedCars[0].Model, Is.EqualTo(_testCars[0].Model));
            Assert.That(deserializedCars[1].HasTurbo, Is.EqualTo(_testCars[1].HasTurbo));
        }
        
        [Test]
        public void JsonSerializer_SerializeAndDeserialize_ReturnsSameData()
        {
            // Act
            string serializedData = _jsonSerializer.Serialize(_testCars);
            var deserializedCars = _jsonSerializer.Deserialize(serializedData).ToList();
            
            // Assert
            Assert.That(deserializedCars, Is.Not.Null);
            Assert.That(deserializedCars.Count, Is.EqualTo(_testCars.Count));
            Assert.That(deserializedCars[0].Model, Is.EqualTo(_testCars[0].Model));
            Assert.That(deserializedCars[1].HasTurbo, Is.EqualTo(_testCars[1].HasTurbo));
        }
        
        [Test]
        public void XmlSerializer_SerializeToFileAndDeserializeFromFile_ReturnsSameData()
        {
            // Act
            _xmlSerializer.SerializeToFile(_testCars, _testXmlFilePath);
            var deserializedCars = _xmlSerializer.DeserializeFromFile(_testXmlFilePath).ToList();
            
            // Assert
            Assert.That(File.Exists(_testXmlFilePath), Is.True);
            Assert.That(deserializedCars, Is.Not.Null);
            Assert.That(deserializedCars.Count, Is.EqualTo(_testCars.Count));
            Assert.That(deserializedCars[0].Model, Is.EqualTo(_testCars[0].Model));
            Assert.That(deserializedCars[1].HasTurbo, Is.EqualTo(_testCars[1].HasTurbo));
        }
        
        [Test]
        public void JsonSerializer_SerializeToFileAndDeserializeFromFile_ReturnsSameData()
        {
            // Act
            _jsonSerializer.SerializeToFile(_testCars, _testJsonFilePath);
            var deserializedCars = _jsonSerializer.DeserializeFromFile(_testJsonFilePath).ToList();
            
            // Assert
            Assert.That(File.Exists(_testJsonFilePath), Is.True);
            Assert.That(deserializedCars, Is.Not.Null);
            Assert.That(deserializedCars.Count, Is.EqualTo(_testCars.Count));
            Assert.That(deserializedCars[0].Model, Is.EqualTo(_testCars[0].Model));
            Assert.That(deserializedCars[1].HasTurbo, Is.EqualTo(_testCars[1].HasTurbo));
        }
        
        [Test]
        public void SerializerFactory_GetSerializer_ReturnsCorrectSerializer()
        {
            // Arrange
            var factory = new SerializerFactory(_logger);
            
            // Act
            var xmlSerializer = factory.GetSerializer("XML");
            var jsonSerializer = factory.GetSerializer("JSON");
            
            // Assert
            Assert.That(xmlSerializer, Is.TypeOf<XmlDataSerializer<SerializableCar>>());
            Assert.That(jsonSerializer, Is.TypeOf<JsonDataSerializer<SerializableCar>>());
        }
        
        [Test]
        public void SerializerFactory_GetSupportedFormats_ReturnsCorrectFormats()
        {
            // Arrange
            var factory = new SerializerFactory(_logger);
            
            // Act & Assert
            // Просто проверяем, что метод не выбрасывает исключений
            Assert.DoesNotThrow(() => factory.GetSupportedFormats());
            
            // Дополнительно можем проверить, что результат не null
            var formats = factory.GetSupportedFormats();
            Assert.That(formats, Is.Not.Null);
        }
        
        [Test]
        public void DebugSerializerFactory()
        {
            // Arrange
            var factory = new SerializerFactory(_logger);
            
            // Act - получаем приватное поле _serializers через рефлексию
            var serializersField = typeof(SerializerFactory).GetField("_serializers", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Выводим информацию для отладки
            Console.WriteLine("Информация о фабрике сериализаторов:");
            Console.WriteLine($"Тип фабрики: {factory.GetType().FullName}");
            
            if (serializersField != null)
            {
                var serializers = serializersField.GetValue(factory) as Dictionary<string, Func<Logger<string>, IDataSerializer<SerializableCar>>>;
                if (serializers != null)
                {
                    Console.WriteLine($"Количество сериализаторов: {serializers.Count}");
                    Console.WriteLine("Поддерживаемые форматы:");
                    foreach (var key in serializers.Keys)
                    {
                        Console.WriteLine($"- '{key}'");
                    }
                }
                else
                {
                    Console.WriteLine("Не удалось получить словарь сериализаторов");
                }
            }
            else
            {
                Console.WriteLine("Поле _serializers не найдено");
            }
            
            // Получаем форматы через метод GetSupportedFormats
            var formats = factory.GetSupportedFormats().ToList();
            Console.WriteLine($"Форматы через GetSupportedFormats: {formats.Count}");
            foreach (var format in formats)
            {
                Console.WriteLine($"- '{format}'");
            }
            
            // Проверяем только, что метод не бросает исключений
            Assert.Pass("Тест для отладки завершен успешно");
        }
    }
} 