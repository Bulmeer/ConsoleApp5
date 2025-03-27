using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp5.Tests
{
    [TestFixture]                            // Атрибут, указывающий что класс содержит тесты
    public class CarTests                     // Класс тестов для проверки функциональности Car
    {
        private Car _car;                     // Поле для хранения тестируемого экземпляра Car
        
        [SetUp]                               // Метод, вызываемый перед каждым тестом
        public void Setup()                   // Метод для настройки тестового окружения
        {
            // Создаем тестовый автомобиль перед каждым тестом с заданными параметрами
            _car = new Car(
                model: "Тестовая модель",      // Задаём модель автомобиля
                engineModel: "V8",            // Указываем модель двигателя
                wheelType: "Летние",          // Указываем тип колёс
                doorType: "4-дверный",        // Задаём тип дверей
                seatMaterial: "Кожа",         // Указываем материал сидений
                steeringType: "Гидроусилитель", // Задаём тип руля
                brakeType: "Дисковые",        // Указываем тип тормозов
                batteryType: "Литиевая",      // Задаём тип аккумулятора
                transmissionType: "Автомат",   // Указываем тип коробки передач
                bodyType: "Седан",            // Задаём тип кузова
                bodyColor: "Черный",          // Указываем цвет кузова
                interiorMaterial: "Кожа",     // Задаём материал салона
                interiorLayout: "5-местный",   // Указываем компоновку салона
                suspensionType: "Стандартная", // Задаём тип подвески
                suspensionBrand: "Continental" // Указываем марку подвески
            );
        }
        
        [TearDown]                           // Метод, вызываемый после каждого теста
        public void Teardown()               // Метод для очистки тестового окружения
        {
            _car = null;                     // Очищаем ссылку на тестовый автомобиль для сборки мусора
        }
        
        [Test]                               // Атрибут, указывающий что метод является тестом
        public void Car_Constructor_SetsModelCorrectly() // Тест проверяет, что конструктор правильно устанавливает модель
        {
            // Assert
            Assert.That(_car.Model, Is.EqualTo("Тестовая модель")); // Проверяем, что модель соответствует заданной в конструкторе
        }
        
        [Test]                               // Атрибут для тестового метода
        public void Car_Constructor_CreatesParts() // Тест проверяет, что конструктор создаёт детали автомобиля
        {
            // Assert
            Assert.That(_car.Parts, Is.Not.Null); // Проверяем, что список деталей создан
            Assert.That(_car.Parts.Count, Is.GreaterThan(0)); // Проверяем, что список деталей не пустой
            Assert.That(_car.Parts, Has.Count.GreaterThan(0)); // Альтернативный способ проверки непустого списка
        }
        
        [Test]                               // Атрибут для тестового метода
        public void Car_Constructor_CreatesBodyWithCorrectProperties() // Тест проверяет свойства кузова
        {
            // Assert
            Assert.That(_car.Body, Is.Not.Null); // Проверяем, что кузов создан
            Assert.That(_car.Body.Type, Is.EqualTo("Седан")); // Проверяем тип кузова
            Assert.That(_car.Body.Color, Is.EqualTo("Черный")); // Проверяем цвет кузова
        }
        
        [Test]                               // Атрибут для тестового метода
        public void Car_Constructor_CreatesInteriorWithCorrectProperties() // Тест проверяет свойства салона
        {
            // Assert
            Assert.That(_car.Interior, Is.Not.Null); // Проверяем, что салон создан
            Assert.That(_car.Interior.Material, Is.EqualTo("Кожа")); // Проверяем материал салона
            Assert.That(_car.Interior.Layout, Is.EqualTo("5-местный")); // Проверяем компоновку салона
        }
        
        [Test]                               // Атрибут для тестового метода
        public void Car_Constructor_CreatesSuspensionWithCorrectProperties() // Тест проверяет свойства подвески
        {
            // Assert
            Assert.That(_car.Suspension, Is.Not.Null); // Проверяем, что подвеска создана
            Assert.That(_car.Suspension.Type, Is.EqualTo("Стандартная")); // Проверяем тип подвески
            Assert.That(_car.Suspension.Brand, Is.EqualTo("Continental")); // Проверяем марку подвески
        }
    }
} 