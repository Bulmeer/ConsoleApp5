using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class GarageTests
    {
        private Garage _garage;
        private Car _testCar;
        private SportsCar _testSportsCar;
        
        [SetUp]
        public void Setup()
        {
            _garage = new Garage();
            
            // Создаем тестовый обычный автомобиль (Toyota)
            _testCar = new Car(
                model: "Toyota",
                engineModel: "V6",
                wheelType: "Всесезонные",
                doorType: "4-дверный",
                seatMaterial: "Ткань",
                steeringType: "Гидроусилитель",
                brakeType: "Дисковые",
                batteryType: "Стандартная",
                transmissionType: "Автомат",
                bodyType: "Хэтчбек",
                bodyColor: "Белый",
                interiorMaterial: "Ткань",
                interiorLayout: "5-местный",
                suspensionType: "Стандартная",
                suspensionBrand: "Toyota"
            );
            
            // Создаем тестовый спортивный автомобиль (Porsche)
            _testSportsCar = new SportsCar(
                model: "Porsche",
                engineModel: "Boxer",
                wheelType: "Спортивные",
                doorType: "2-дверный",
                seatMaterial: "Кожа",
                steeringType: "Спортивный",
                brakeType: "Керамические",
                batteryType: "Усиленная",
                transmissionType: "PDK",
                bodyType: "Купе",
                bodyColor: "Желтый",
                interiorMaterial: "Алькантара",
                interiorLayout: "2-местный",
                suspensionType: "Спортивная",
                suspensionBrand: "Porsche",
                hasTurbo: true
            );
        }
        
        [TearDown]
        public void Teardown()
        {
            _garage = null;
            _testCar = null;
            _testSportsCar = null;
        }
        
        [Test]
        public void AddCar_AddsCarsToGarage()
        {
            // Act
            _garage.AddCar(_testCar);
            
            // Assert
            Assert.That(_garage.Cars.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void GetSportsCarWriter_CanAddSportsCar()
        {
            // Arrange
            var writer = _garage.GetSportsCarWriter();
            
            // Act
            writer.AddVehicle(_testSportsCar);
            
            // Assert
            Assert.That(_garage.Cars.Count, Is.EqualTo(1));
            Assert.That(_garage.Cars.GetVehicle(0), Is.TypeOf<SportsCar>());
        }
        
        [Test]
        public void FindCars_ReturnsMatchingCars()
        {
            // Arrange
            _garage.AddCar(_testCar);
            _garage.AddCar(_testSportsCar);
            
            // Act
            var foundCars = _garage.FindCars(car => car.Model.Contains("Por"));
            
            // Assert
            Assert.That(foundCars.Count, Is.EqualTo(1));
            Assert.That(foundCars[0].Model, Is.EqualTo("Porsche"));
        }
        
        [Test]
        public async Task SortByModelAsync_SortsCarsByModel()
        {
            // Arrange
            _garage.AddCar(_testCar);
            _garage.AddCar(_testSportsCar);
            
            // Act
            await _garage.SortByModelAsync();
            
            // Assert
            Assert.That(_garage.Cars.GetVehicle(0).Model, Is.EqualTo("Porsche"));
            Assert.That(_garage.Cars.GetVehicle(1).Model, Is.EqualTo("Toyota"));
        }
        
        [Test]
        public void ProcessAllCars_ExecutesActionOnAllCars()
        {
            // Arrange
            _garage.AddCar(_testCar);
            _garage.AddCar(_testSportsCar);
            int count = 0;
            
            // Act
            _garage.ProcessAllCars(_ => count++);
            
            // Assert
            Assert.That(count, Is.EqualTo(2));
        }
    }
} 