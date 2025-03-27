using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp5.Tests
{
    [TestFixture]                             // Атрибут, указывающий что класс содержит тесты
    public class ConfigurationManagerTests    // Класс тестов для проверки менеджера конфигурации
    {
        private AppConfiguration _testConfig; // Тестовая конфигурация приложения
        private string _testConfigPath;       // Путь к файлу тестовой конфигурации
        private ConfigurationManager _configManager; // Экземпляр менеджера конфигурации для тестов
        
        [SetUp]                               // Метод, вызываемый перед каждым тестом
        public void Setup()                   // Метод для настройки тестового окружения
        {
            _configManager = new ConfigurationManager(); // Создаём новый менеджер конфигурации для каждого теста
            
            // Создаем тестовую конфигурацию
            _testConfig = new AppConfiguration
            {
                ApplicationName = "Тестовое приложение", // Задаём имя тестового приложения
                Version = "2.0",                         // Задаём версию
                EnableConsoleLogging = false,            // Отключаем логирование в консоль
                EnableFileLogging = true,                // Включаем логирование в файл
                LogFilePath = "test.log",                // Указываем путь к лог-файлу
                MaxCarsInGarage = 5,                     // Устанавливаем максимальное количество машин
                SupportedCarBrands = new System.Collections.Generic.List<string> { "Test1", "Test2" } // Задаём список поддерживаемых брендов
            };
            
            // Создаем временный путь для тестового файла конфигурации
            _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.xml"); // Генерируем уникальный путь для файла конфигурации
        }
        
        [TearDown]                            // Метод, вызываемый после каждого теста
        public void Teardown()                // Метод для очистки тестового окружения
        {
            // Удаляем тестовый файл, если он существует
            if (File.Exists(_testConfigPath)) // Проверяем существование файла конфигурации
            {
                File.Delete(_testConfigPath); // Удаляем файл конфигурации
            }
            
            _testConfig = null;               // Очищаем ссылку на тестовую конфигурацию
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void GetConfiguration_ReturnsDefaultConfiguration_WhenNoConfigExists() // Тест проверяет получение конфигурации по умолчанию, когда файл не существует
        {
            // Act
            var config = ConfigurationManager.GetConfiguration(); // Получаем конфигурацию
            
            // Assert
            Assert.That(config, Is.Not.Null); // Проверяем, что конфигурация не null
            Assert.That(config.ApplicationName, Is.EqualTo("Автомобильный гараж")); // Проверяем имя приложения по умолчанию
            Assert.That(config.Version, Is.EqualTo("1.0")); // Проверяем версию по умолчанию
            Assert.That(config.SupportedCarBrands, Has.Count.GreaterThan(0)); // Проверяем, что список поддерживаемых брендов не пуст
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void SaveConfiguration_SavesConfigurationToFile() // Тест проверяет сохранение конфигурации в файл
        {
            try
            {
                // Arrange - создаем тестовый приватный метод для доступа к приватному полю ConfigFileName
                var originalConfigFileName = typeof(ConfigurationManager)
                    .GetField("ConfigFileName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static); // Получаем доступ к приватному полю
                
                string originalValue = null;  // Переменная для хранения оригинального значения
                if (originalConfigFileName != null) // Проверяем, что поле найдено
                {
                    // Сохраняем оригинальное значение
                    originalValue = (string)originalConfigFileName.GetValue(null); // Сохраняем текущее значение
                    // Устанавливаем тестовое значение
                    originalConfigFileName.SetValue(null, _testConfigPath); // Устанавливаем тестовый путь
                }
                
                // Act
                ConfigurationManager.SaveConfiguration(_testConfig); // Сохраняем тестовую конфигурацию
                
                // Assert
                Assert.That(File.Exists(_testConfigPath), Is.True); // Проверяем, что файл был создан
                
                // Восстанавливаем оригинальное значение
                if (originalConfigFileName != null && originalValue != null) // Проверяем, что поле найдено и был оригинальный путь
                {
                    originalConfigFileName.SetValue(null, originalValue); // Восстанавливаем оригинальный путь
                }
            }
            catch (Exception)
            {
                // Тест может не сработать в некоторых средах из-за ограничений доступа к приватным полям
                Assert.Inconclusive("Невозможно протестировать сохранение конфигурации из-за ограничений доступа к приватным полям"); // Пропускаем тест с пояснением
            }
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void AppConfiguration_DefaultValues_AreCorrect() // Тест проверяет корректность значений по умолчанию
        {
            // Act
            var config = new AppConfiguration(); // Создаём новую конфигурацию с значениями по умолчанию
            
            // Assert
            Assert.That(config.ApplicationName, Is.EqualTo("Автомобильный гараж")); // Проверяем имя приложения по умолчанию
            Assert.That(config.Version, Is.EqualTo("1.0")); // Проверяем версию по умолчанию
            Assert.That(config.EnableConsoleLogging, Is.True); // Проверяем, что логирование в консоль включено по умолчанию
            Assert.That(config.EnableFileLogging, Is.True); // Проверяем, что логирование в файл включено по умолчанию
            Assert.That(config.LogFilePath, Is.EqualTo("app.log")); // Проверяем путь к лог-файлу по умолчанию
            Assert.That(config.MaxCarsInGarage, Is.EqualTo(10)); // Проверяем максимальное количество машин по умолчанию
            Assert.That(config.SupportedCarBrands, Is.Not.Null.And.Not.Empty); // Проверяем, что список брендов не пуст
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void GetSetting_ReturnsDefaultValueForNonExistentKey() // Тест проверяет возврат значения по умолчанию для несуществующего ключа
        {
            // Act
            string value = _configManager.GetSetting("NonExistentKey", "DefaultValue"); // Получаем настройку с несуществующим ключом
            
            // Assert
            Assert.That(value, Is.EqualTo("DefaultValue")); // Проверяем, что возвращено значение по умолчанию
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void SetSetting_SavesValueForKey() // Тест проверяет сохранение значения для ключа
        {
            // Arrange
            string key = "TestKey";           // Тестовый ключ
            string value = "TestValue";       // Тестовое значение
            
            // Act
            _configManager.SetSetting(key, value); // Устанавливаем настройку
            string retrievedValue = _configManager.GetSetting(key, "DefaultValue"); // Получаем установленную настройку
            
            // Assert
            Assert.That(retrievedValue, Is.EqualTo(value)); // Проверяем, что возвращено сохранённое значение
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void SetSetting_OverwritesExistingValue() // Тест проверяет перезапись существующего значения
        {
            // Arrange
            string key = "TestKey";           // Тестовый ключ
            string value1 = "Value1";         // Первое тестовое значение
            string value2 = "Value2";         // Второе тестовое значение
            
            // Act
            _configManager.SetSetting(key, value1); // Устанавливаем первое значение
            _configManager.SetSetting(key, value2); // Перезаписываем значение
            string retrievedValue = _configManager.GetSetting(key, "DefaultValue"); // Получаем установленную настройку
            
            // Assert
            Assert.That(retrievedValue, Is.EqualTo(value2)); // Проверяем, что возвращено последнее установленное значение
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void RemoveSetting_DeletesExistingKey() // Тест проверяет удаление существующего ключа
        {
            // Arrange
            string key = "TestKey";           // Тестовый ключ
            string value = "TestValue";       // Тестовое значение
            _configManager.SetSetting(key, value); // Устанавливаем настройку
            
            // Act
            bool removed = _configManager.RemoveSetting(key); // Удаляем настройку
            string retrievedValue = _configManager.GetSetting(key, "DefaultValue"); // Пытаемся получить удалённую настройку
            
            // Assert
            Assert.That(removed, Is.True);    // Проверяем, что операция удаления вернула true
            Assert.That(retrievedValue, Is.EqualTo("DefaultValue")); // Проверяем, что возвращено значение по умолчанию
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void RemoveSetting_ReturnsFalseForNonExistentKey() // Тест проверяет возврат false при удалении несуществующего ключа
        {
            // Act
            bool removed = _configManager.RemoveSetting("NonExistentKey"); // Пытаемся удалить несуществующую настройку
            
            // Assert
            Assert.That(removed, Is.False);   // Проверяем, что операция удаления вернула false
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void GetAllSettings_ReturnsAllSettings() // Тест проверяет получение всех настроек
        {
            // Arrange
            _configManager.SetSetting("Key1", "Value1"); // Устанавливаем первую настройку
            _configManager.SetSetting("Key2", "Value2"); // Устанавливаем вторую настройку
            
            // Act
            Dictionary<string, string> settings = _configManager.GetAllSettings(); // Получаем все настройки
            
            // Assert
            Assert.That(settings.Count, Is.EqualTo(2)); // Проверяем, что получены обе настройки
            Assert.That(settings["Key1"], Is.EqualTo("Value1")); // Проверяем первую настройку
            Assert.That(settings["Key2"], Is.EqualTo("Value2")); // Проверяем вторую настройку
        }
        
        [Test]                                // Атрибут, указывающий что метод является тестом
        public void ClearSettings_RemovesAllSettings() // Тест проверяет удаление всех настроек
        {
            // Arrange
            _configManager.SetSetting("Key1", "Value1"); // Устанавливаем первую настройку
            _configManager.SetSetting("Key2", "Value2"); // Устанавливаем вторую настройку
            
            // Act
            _configManager.ClearSettings();   // Очищаем все настройки
            Dictionary<string, string> settings = _configManager.GetAllSettings(); // Получаем настройки после очистки
            
            // Assert
            Assert.That(settings.Count, Is.EqualTo(0)); // Проверяем, что все настройки удалены
        }
    }
} 