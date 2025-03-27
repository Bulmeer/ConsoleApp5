using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleApp5.Tests
{
    [TestFixture]
    public class GarageCollectionTests
    {
        private GarageCollection<Car> _garageCollection;
        private Logger<string> _logger;
        private Car _testCar1;
        private Car _testCar2;
        
        [SetUp]
        public void Setup()
        {
            _logger = new Logger<string>();
            _garageCollection = new GarageCollection<Car>(_logger);
            
            // Создаем первый тестовый автомобиль (Audi)
            _testCar1 = new Car(
                model: "Audi",
                engineModel: "V6",
                wheelType: "Летние",
                doorType: "4-дверный",
                seatMaterial: "Кожа",
                steeringType: "Электроусилитель",
                brakeType: "Дисковые",
                batteryType: "Стандартная",
                transmissionType: "Автомат",
                bodyType: "Седан",
                bodyColor: "Серебристый",
                interiorMaterial: "Кожа",
                interiorLayout: "5-местный",
                suspensionType: "Стандартная",
                suspensionBrand: "Audi"
            );
            
            // Создаем второй тестовый автомобиль (BMW)
            _testCar2 = new Car(
                model: "BMW",
                engineModel: "V8",
                wheelType: "Летние",
                doorType: "4-дверный",
                seatMaterial: "Кожа",
                steeringType: "Электроусилитель",
                brakeType: "Дисковые",
                batteryType: "Стандартная",
                transmissionType: "Автомат",
                bodyType: "Седан",
                bodyColor: "Черный",
                interiorMaterial: "Кожа",
                interiorLayout: "5-местный",
                suspensionType: "Спортивная",
                suspensionBrand: "BMW"
            );
        }
        
        [TearDown]
        public void Teardown()
        {
            _garageCollection.Dispose();
            _garageCollection = null;
            _testCar1 = null;
            _testCar2 = null;
        }
        
        [Test]
        public void Add_AddsCarToCollection()
        {
            // Arrange
            
            // Act
            _garageCollection.Add(_testCar1);
            
            // Assert
            Assert.That(_garageCollection.Count, Is.EqualTo(1));
            Assert.That(_garageCollection.ToList(), Does.Contain(_testCar1));
        }
        
        [Test]
        public void GetVehicle_ReturnsCorrectVehicle()
        {
            // Arrange
            _garageCollection.Add(_testCar1);
            _garageCollection.Add(_testCar2);
            
            // Act
            var car = _garageCollection.GetVehicle(1);
            
            // Assert
            Assert.That(car, Is.EqualTo(_testCar2));
        }
        
        [Test]
        public void GetVehicle_ThrowsExceptionForInvalidIndex()
        {
            // Arrange
            _garageCollection.Add(_testCar1);
            
            // Act & Assert
            Assert.That(() => _garageCollection.GetVehicle(5), Throws.TypeOf<IndexOutOfRangeException>());
        }
        
        [Test]
        public void Clear_RemovesAllVehicles()
        {
            // Arrange
            _garageCollection.Add(_testCar1);
            _garageCollection.Add(_testCar2);
            
            // Act
            _garageCollection.Clear();
            
            // Assert
            Assert.That(_garageCollection.Count, Is.EqualTo(0));
            Assert.That(_garageCollection, Is.Empty);
        }
        
        [Test]
        public void Contains_ReturnsTrueForExistingVehicle()
        {
            // Arrange
            _garageCollection.Add(_testCar1);
            
            // Act & Assert
            Assert.That(_garageCollection.Contains(_testCar1), Is.True);
            Assert.That(_garageCollection, Does.Contain(_testCar1));
        }
        
        [Test]
        public void Contains_ReturnsFalseForNonExistingVehicle()
        {
            // Act & Assert
            Assert.That(_garageCollection.Contains(_testCar1), Is.False);
        }
        
        [Test]
        public void Filter_ReturnsFilteredCollection()
        {
            // Arrange
            _garageCollection.Add(_testCar1);
            _garageCollection.Add(_testCar2);
            
            // Act
            var filteredCollection = _garageCollection.Filter(car => car.Model == "BMW");
            
            // Assert
            Assert.That(filteredCollection.Count, Is.EqualTo(1));
            Assert.That(filteredCollection.GetVehicle(0).Model, Is.EqualTo("BMW"));
        }
    }
} 