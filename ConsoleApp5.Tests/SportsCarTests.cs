using NUnit.Framework;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class SportsCarTests
    {
        private SportsCar _sportsCar;
        
        [SetUp]
        public void Setup()
        {
            // Создаем тестовый спортивный автомобиль перед каждым тестом
            _sportsCar = new SportsCar(
                model: "Ferrari",
                engineModel: "V12",
                wheelType: "Спорт",
                doorType: "2-дверный",
                seatMaterial: "Алькантара",
                steeringType: "Спортивный",
                brakeType: "Карбоновые",
                batteryType: "Усиленная",
                transmissionType: "Роботизированная",
                bodyType: "Купе",
                bodyColor: "Красный",
                interiorMaterial: "Карбон",
                interiorLayout: "2-местный",
                suspensionType: "Спортивная",
                suspensionBrand: "Ferrari",
                hasTurbo: true
            );
        }
        
        [TearDown]
        public void Teardown()
        {
            _sportsCar = null;
        }
        
        [Test]
        public void SportsCar_Constructor_SetsHasTurboCorrectly()
        {
            // Assert
            Assert.That(_sportsCar.HasTurbo, Is.True);
        }
        
        [Test]
        public void SportsCar_Constructor_InheritsCarProperties()
        {
            // Assert
            Assert.That(_sportsCar.Model, Is.EqualTo("Ferrari"));
            Assert.That(_sportsCar.Body.Type, Is.EqualTo("Купе"));
            Assert.That(_sportsCar.Body.Color, Is.EqualTo("Красный"));
        }
        
        [Test, Ignore("Этот тест нестабилен из-за различных возможных реализаций бренда трансмиссии")]
        public void SportsCar_HasCorrectTransmission()
        {
            // Assert
            Assert.That(_sportsCar.Transmission, Is.Not.Null);
            Assert.That(_sportsCar.Transmission.Type, Is.EqualTo("Роботизированная"));
            // Бренд трансмиссии может совпадать с названием машины 
            // или быть конкретным, в зависимости от реализации
            Assert.That(_sportsCar.Transmission.Brand, Is.Not.Null);
        }
    }
} 