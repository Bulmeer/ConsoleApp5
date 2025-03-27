using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Linq;

#region Исключения

// Базовый класс пользовательских исключений
[Serializable]
public class CarApplicationException : Exception
{
    public string ErrorCode { get; }

    public CarApplicationException() : base("Произошла ошибка в автомобильном приложении") { }

    public CarApplicationException(string message) : base(message) { }

    public CarApplicationException(string message, Exception innerException)
        : base(message, innerException) { }

    public CarApplicationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

// Пользовательское исключение для проблем с конфигурацией
[Serializable]
public class ConfigurationException : CarApplicationException
{
    public ConfigurationException() : base("Произошла ошибка при работе с конфигурацией") { }

    public ConfigurationException(string message) : base(message) { }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }

    public ConfigurationException(string message, string errorCode)
        : base(message, errorCode) { }
}

// Пользовательское исключение для проблем с автомобильными деталями
[Serializable]
public class CarPartException : CarApplicationException
{
    public string PartName { get; }

    public CarPartException() : base("Произошла ошибка при работе с деталью автомобиля") { }

    public CarPartException(string message) : base(message) { }

    public CarPartException(string message, string partName)
        : base(message)
    {
        PartName = partName;
    }

    public CarPartException(string message, Exception innerException)
        : base(message, innerException) { }

    public CarPartException(string message, string errorCode, string partName)
        : base(message, errorCode)
    {
        PartName = partName;
    }
}

// Обработчик исключений
static class ExceptionHandler
{
    // Логгер для исключений
    private static readonly Logger<string> logger = new Logger<string>();

    // Статический конструктор
    static ExceptionHandler()
    {
        // Подключаем обработчики логов
        ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
        FileLogWriter fileWriter = new FileLogWriter("exceptions.log");

        logger.LogEvent += consoleWriter.WriteToConsole;
        logger.LogEvent += fileWriter.WriteToFile;
    }

    // Обработка стандартных исключений
    public static void HandleStandardException(Exception ex)
    {
        if (ex is FileNotFoundException)
        {
            logger.LogError($"Ошибка: Файл не найден. {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Файл не найден: {ex.Message}");
            Console.ResetColor();
        }
        else if (ex is IOException)
        {
            logger.LogError($"Ошибка ввода/вывода: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка при чтении/записи файла: {ex.Message}");
            Console.ResetColor();
        }
        else if (ex is XmlException)
        {
            logger.LogError($"Ошибка обработки XML: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка в формате XML: {ex.Message}");
            Console.ResetColor();
        }
        else if (ex is ArgumentException)
        {
            logger.LogError($"Ошибка в аргументах: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Неверный аргумент: {ex.Message}");
            Console.ResetColor();
        }
        else
        {
            logger.LogError($"Необработанное стандартное исключение: {ex.GetType().Name} - {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Произошла неизвестная ошибка: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
            }
            Console.ResetColor();
        }
    }

    // Обработка пользовательских исключений
    public static void HandleCarApplicationException(CarApplicationException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        if (ex is ConfigurationException)
        {
            logger.LogError($"Ошибка конфигурации: {ex.Message}, Код ошибки: {ex.ErrorCode}");
            Console.WriteLine($"Ошибка конфигурации: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ErrorCode))
            {
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
            }
        }
        else if (ex is CarPartException partEx)
        {
            logger.LogError($"Ошибка в детали '{partEx.PartName}': {ex.Message}, Код ошибки: {ex.ErrorCode}");
            Console.WriteLine($"Ошибка в детали '{partEx.PartName}': {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ErrorCode))
            {
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
            }
        }
        else
        {
            logger.LogError($"Пользовательская ошибка: {ex.Message}, Код ошибки: {ex.ErrorCode}");
            Console.WriteLine($"Произошла ошибка в приложении: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ErrorCode))
            {
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
            }
        }

        if (ex.InnerException != null)
        {
            logger.LogError($"Внутреннее исключение: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            Console.WriteLine($"Причина: {ex.InnerException.Message}");
        }

        Console.ResetColor();
    }
}

#endregion

#region Конфигурация

// Класс, представляющий конфигурацию приложения
[Serializable]
public class AppConfiguration
{
    public string ApplicationName { get; set; } = "Автомобильный гараж";
    public string Version { get; set; } = "1.0";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public string LogFilePath { get; set; } = "app.log";
    public int MaxCarsInGarage { get; set; } = 10;
    public List<string> SupportedCarBrands { get; set; } = new List<string>
    {
        "Toyota", "BMW", "Mercedes", "Audi", "Ferrari"
    };
}

// Класс для управления конфигурацией
public class ConfigurationManager
{
    private static readonly string ConfigFileName = "app_config.xml";
    private static readonly Logger<string> logger = new Logger<string>();
    private static AppConfiguration _config;

    // Статический конструктор
    static ConfigurationManager()
    {
        // Подключаем обработчики логов
        ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
        FileLogWriter fileWriter = new FileLogWriter("config.log");

        logger.LogEvent += consoleWriter.WriteToConsole;
        logger.LogEvent += fileWriter.WriteToFile;
    }

    // Получить конфигурацию
    public static AppConfiguration GetConfiguration()
    {
        if (_config == null)
        {
            try
            {
                _config = LoadConfiguration();
                logger.LogInfo("Конфигурация успешно загружена");
            }
            catch (FileNotFoundException ex)
            {
                logger.LogWarning("Файл конфигурации не найден. Создаём конфигурацию по умолчанию.");
                _config = CreateDefaultConfiguration();
                SaveConfiguration(_config);
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при загрузке конфигурации: {ex.Message}");
                throw new ConfigurationException("Не удалось загрузить конфигурацию", ex) { };
            }
        }
        return _config;
    }

    // Загрузить конфигурацию из файла
    private static AppConfiguration LoadConfiguration()
    {
        if (!File.Exists(ConfigFileName))
        {
            throw new FileNotFoundException("Файл конфигурации не найден", ConfigFileName);
        }

        try
        {
            using (var stream = new FileStream(ConfigFileName, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(AppConfiguration));
                return (AppConfiguration)serializer.Deserialize(stream);
            }
        }
        catch (XmlException ex)
        {
            throw new ConfigurationException("Ошибка при разборе XML конфигурации", ex) { };
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("Неизвестная ошибка при загрузке конфигурации", ex) { };
        }
    }

    // Сохранить конфигурацию в файл
    public static void SaveConfiguration(AppConfiguration config)
    {
        try
        {
            using (var stream = new FileStream(ConfigFileName, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(AppConfiguration));
                serializer.Serialize(stream, config);
            }
            logger.LogInfo("Конфигурация успешно сохранена");
        }
        catch (Exception ex)
        {
            logger.LogError($"Ошибка при сохранении конфигурации: {ex.Message}");
            throw new ConfigurationException("Не удалось сохранить конфигурацию", ex) { };
        }
    }

    // Создать конфигурацию по умолчанию
    private static AppConfiguration CreateDefaultConfiguration()
    {
        return new AppConfiguration();
    }
}

#endregion

#region Логирование

// Делегат для события логирования
public delegate void LogHandler<T>(T message);

// Обобщенный класс логирования
public class Logger<T>
{
    // Событие для логирования
    public event LogHandler<T> LogEvent;

    // Метод для записи лога
    public void Log(T message)
    {
        // Вызываем событие, если есть подписчики
        LogEvent?.Invoke(message);
    }

    // Методы для разных уровней логирования
    public void LogInfo(T message)
    {
        Log(message);
    }

    public void LogWarning(T message)
    {
        Log(message);
    }

    public void LogError(T message)
    {
        Log(message);
    }
}

// Класс сообщения для логирования сортировки
public class SortingLogMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Message { get; set; }
    public int ProcessedItems { get; set; }
    public string SortType { get; set; }
    public bool IsComplete { get; set; }
    // Добавляем уникальный идентификатор сортировки для отслеживания конкретных операций
    public Guid SortId { get; set; } = Guid.NewGuid();
    // Добавляем действие по подтверждению приема сообщения (обратная связь)
    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: этот делегат вызывается из потока логирования обратно в поток сортировки
    public Action<Guid> Acknowledgment { get; set; }

    public override string ToString()
    {
        return $"[Сортировка {SortType}] {Message}" + (ProcessedItems > 0 ? $" (обработано элементов: {ProcessedItems})" : "");
    }
}

// Обработчик логирования сортировки в отдельном потоке
public class SortingLogger
{
    // Очередь сообщений для взаимодействия между потоками
    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: поток сортировки помещает сообщения в очередь, поток логирования забирает их
    private readonly BlockingCollection<SortingLogMessage> _logQueue = new BlockingCollection<SortingLogMessage>();

    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: используется для остановки потока логирования из другого потока
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public readonly Logger<string> _logger;

    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: задача выполняющаяся в отдельном потоке
    private Task _loggerTask;

    private bool _isRunning = true;

    // Словарь для отслеживания подтвержденных сообщений между потоками
    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: потокобезопасный словарь для проверки обработки сообщений
    private readonly ConcurrentDictionary<Guid, bool> _acknowledgedMessages = new ConcurrentDictionary<Guid, bool>();

    // Событие для сигнализации о важных состояниях логирования между потоками
    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: поток логирования вызывает это событие, на которое подписываются другие потоки
    public event Action<string, double> LoggingProgressChanged;

    public SortingLogger(Logger<string> logger)
    {
        _logger = logger;
        StartLoggerThread();
    }

    // Метод для запуска потока логирования
    private void StartLoggerThread()
    {
        // Запускаем отдельный поток для логирования
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: создаём отдельный поток с долгоживущей задачей
        _loggerTask = Task.Factory.StartNew(() =>
        {
            try
            {
                int totalProcessed = 0;
                int messageCount = 0;
                string currentSortType = "";

                while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Получаем сообщение из очереди с таймаутом (взаимодействие между потоками)
                    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: блокирующая операция ожидания сообщения из другого потока
                    if (_logQueue.TryTake(out SortingLogMessage message, 100, _cancellationTokenSource.Token))
                    {
                        // Логируем сообщение
                        _logger.LogInfo(message.ToString());

                        // Отправляем прогресс через событие (взаимодействие между потоками)
                        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: уведомление подписчиков (потоков пользовательского интерфейса) о прогрессе
                        if (!string.IsNullOrEmpty(currentSortType) && currentSortType == message.SortType)
                        {
                            double progress = messageCount > 0 ? (double)message.ProcessedItems / (messageCount * 10) : 0;
                            LoggingProgressChanged?.Invoke(message.SortType, Math.Min(progress, 0.99));
                        }

                        // Если это сообщение с прогрессом
                        if (message.ProcessedItems > 0)
                        {
                            totalProcessed = message.ProcessedItems;
                            messageCount++;
                            currentSortType = message.SortType;
                        }

                        // Если это последнее сообщение для текущей сортировки, сигнализируем о завершении
                        if (message.IsComplete)
                        {
                            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: сигнализируем о 100% завершении потокам UI
                            LoggingProgressChanged?.Invoke(message.SortType, 1.0); // 100% выполнено

                            // Отмечаем сообщение как подтвержденное (взаимодействие между потоками)
                            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: обновляем потокобезопасный словарь
                            _acknowledgedMessages[message.SortId] = true;

                            // Вызываем обратное действие для подтверждения приема (взаимодействие между потоками)
                            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: обратный вызов в поток, создавший сообщение
                            message.Acknowledgment?.Invoke(message.SortId);

                            // Сбрасываем счетчики для следующей сортировки
                            totalProcessed = 0;
                            messageCount = 0;
                            currentSortType = "";

                            Thread.Sleep(50); // Даем время на обработку оставшихся сообщений
                        }
                        else
                        {
                            // Для не-завершающих сообщений тоже отправляем подтверждение
                            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: отмечаем и подтверждаем промежуточные сообщения
                            _acknowledgedMessages[message.SortId] = true;
                            message.Acknowledgment?.Invoke(message.SortId);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены
                _logger.LogInfo("Поток логирования сортировки был отменен");
            }
            catch (Exception ex)
            {
                // Обработка других исключений
                _logger.LogError($"Ошибка в потоке логирования: {ex.Message}");
            }
        }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    // Добавление сообщения в очередь логирования с ожиданием подтверждения (взаимодействие между потоками)
    public async Task<bool> EnqueueLogMessageWithAcknowledgmentAsync(SortingLogMessage message)
    {
        if (!_isRunning || _cancellationTokenSource.IsCancellationRequested)
            return false;

        // Устанавливаем идентификатор, если он не был установлен
        if (message.SortId == Guid.Empty)
            message.SortId = Guid.NewGuid();

        // Готовим TaskCompletionSource для ожидания подтверждения (взаимодействие между потоками)
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: этот объект позволяет асинхронно ожидать сигнала от другого потока
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        // Устанавливаем действие подтверждения (взаимодействие между потоками)
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: настраиваем обратный вызов, который будет сигнализировать о завершении
        message.Acknowledgment = (id) =>
        {
            tcs.TrySetResult(true);
        };

        // Добавляем сообщение в очередь (взаимодействие между потоками)
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: передаём данные в другой поток через очередь
        _logQueue.Add(message);

        // Ждем подтверждения с таймаутом (взаимодействие между потоками)
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: ожидаем сигнала от потока логирования или истечения таймаута
        Task timeoutTask = Task.Delay(1000);
        Task completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        return completedTask == tcs.Task;
    }

    // Обратно-совместимый метод добавления сообщения без ожидания подтверждения
    public void EnqueueLogMessage(SortingLogMessage message)
    {
        if (_isRunning && !_cancellationTokenSource.IsCancellationRequested)
        {
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: добавление в очередь без ожидания подтверждения
            _logQueue.Add(message);
        }
    }

    // Проверка, было ли сообщение подтверждено
    public bool IsMessageAcknowledged(Guid messageId)
    {
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: проверка в потокобезопасном словаре статуса сообщения
        return _acknowledgedMessages.TryGetValue(messageId, out bool acknowledged) && acknowledged;
    }

    // Ожидание завершения всех операций логирования
    public async Task WaitForLoggingAsync()
    {
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: ожидание опустошения очереди сообщений
        int attempts = 0;
        while (_logQueue.Count > 0 && attempts < 100)
        {
            await Task.Delay(10);
            attempts++;
        }
        await Task.Delay(50); // Небольшая задержка для обработки
    }

    // Синхронная версия для обратной совместимости
    public void WaitForLogging()
    {
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: синхронное ожидание завершения асинхронных операций
        WaitForLoggingAsync().GetAwaiter().GetResult();
    }

    // Остановка потока логирования
    public void Stop()
    {
        _isRunning = false;
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: сигнал остановки потока логирования
        _cancellationTokenSource.Cancel();
        try
        {
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: ожидание завершения потока логирования
            _loggerTask?.Wait(1000);
        }
        catch (Exception)
        {
            // Игнорируем исключения при остановке потока
        }
    }

    // Деструктор для очистки ресурсов
    ~SortingLogger()
    {
        Stop();
    }
}

// Класс для записи логов в консоль
class ConsoleLogWriter
{
    // Метод для вывода лога в консоль
    public void WriteToConsole<T>(T message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[КОНСОЛЬ ЛОГ] {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
        Console.ResetColor();
    }
}

// Класс для записи логов в файл
class FileLogWriter
{
    private readonly string _logFilePath;

    public FileLogWriter(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    // Метод для вывода лога в файл
    public void WriteToFile<T>(T message)
    {
        try
        {
            // Добавляем лог в файл
            File.AppendAllText(_logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи в файл лога: {ex.Message}");
        }
    }
}

#endregion

#region Модели

// Абстрактный класс для деталей
public abstract class Part
{
    public string Name { get; set; }
    public Part(string name) => Name = name;
}

// Конкретные детали
public class Engine : Part { public Engine(string model) : base($"Двигатель {model}") { } }
public class Wheel : Part { public Wheel(string type) : base($"Колесо {type}") { } }
public class Door : Part { public Door(string type) : base($"Дверь {type}") { } }
public class Seat : Part { public Seat(string material) : base($"Сиденье {material}") { } }
public class SteeringWheel : Part { public SteeringWheel(string type) : base($"Руль {type}") { } }
public class BrakeSystem : Part { public BrakeSystem(string type) : base($"Тормозная система {type}") { } }
public class Battery : Part { public Battery(string type) : base($"Аккумулятор {type}") { } }
public class Transmission : Part { public Transmission(string type) : base($"Трансмиссия {type}") { } }
public class Radiator : Part { public Radiator() : base("Радиатор Behr") { } }
public class Exhaust : Part { public Exhaust() : base("Выхлопная система Borla") { } }
public class FuelTank : Part { public FuelTank() : base("Топливный бак Bosch") { } }
public class Suspension : Part { public Suspension() : base("Подвеска Bilstein") { } }
public class Airbag : Part { public Airbag() : base("Подушка безопасности Takata") { } }
public class Dashboard : Part { public Dashboard() : base("Приборная панель VDO") { } }
public class Headlights : Part { public Headlights() : base("Фары Osram LED") { } }
public class Mirror : Part { public Mirror() : base("Зеркало заднего вида Gentex") { } }
public class Horn : Part { public Horn() : base("Клаксон Hella") { } }
public class Wiper : Part { public Wiper() : base("Стеклоочиститель Bosch AeroTwin") { } }
public class NavigationSystem : Part { public NavigationSystem() : base("Навигационная система Garmin") { } }

// Независимый класс кузова
public class CarBody
{
    public string Type { get; set; }
    public string Color { get; set; }

    public CarBody(string type, string color)
    {
        Type = type;
        Color = color;
    }

    public void ShowBodyInfo()
    {
        Console.WriteLine($"Тип кузова: {Type}, Цвет: {Color}");
    }
}

// Независимый класс интерьера автомобиля
public class VehicleInterior
{
    public string Material { get; set; }
    public string Layout { get; set; }

    public VehicleInterior(string material, string layout)
    {
        Material = material;
        Layout = layout;
    }

    public void ShowInteriorInfo()
    {
        Console.WriteLine($"Материал интерьера: {Material}, Расположение сидений: {Layout}");
    }
}

// Независимый класс системы подвески
public class SuspensionSystem
{
    public string Type { get; set; }
    public string Brand { get; set; }

    public SuspensionSystem(string type, string brand)
    {
        Type = type;
        Brand = brand;
    }

    public void ShowSuspensionInfo()
    {
        Console.WriteLine($"Тип подвески: {Type}, Бренд: {Brand}");
    }
}

// Независимый класс трансмиссии
public class TransmissionSystem
{
    public string Type { get; set; }
    public string Brand { get; set; }

    public TransmissionSystem(string type, string brand)
    {
        Type = type;
        Brand = brand;
    }

    public void ShowTransmissionInfo()
    {
        Console.WriteLine($"Тип трансмиссии: {Type}, Бренд: {Brand}");
    }
}

// Класс автомобиля
public class Car
{
    // Логгер для класса Car
    protected static Logger<string> logger = new Logger<string>();

    public string Model { get; set; }
    public List<Part> Parts { get; private set; }
    public CarBody Body { get; private set; }
    public VehicleInterior Interior { get; private set; }
    public SuspensionSystem Suspension { get; private set; }
    public TransmissionSystem Transmission { get; private set; }

    // Статический конструктор для подключения логгеров
    static Car()
    {
        // Создаем экземпляры обработчиков логов
        ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
        FileLogWriter fileWriter = new FileLogWriter("car_logs.txt");

        // Подписываем обработчики на событие логирования
        logger.LogEvent += consoleWriter.WriteToConsole;
        logger.LogEvent += fileWriter.WriteToFile;
    }

    public Car(string model, string engineModel, string wheelType, string doorType, string seatMaterial, string steeringType, string brakeType, string batteryType, string transmissionType, string bodyType, string bodyColor, string interiorMaterial, string interiorLayout, string suspensionType, string suspensionBrand)
    {
        Model = model;
        logger.LogInfo($"Создание автомобиля {Model}");

        Body = new CarBody(bodyType, bodyColor); // Уникальный кузов
        Interior = new VehicleInterior(interiorMaterial, interiorLayout); // Уникальный интерьер
        Suspension = new SuspensionSystem(suspensionType, suspensionBrand); // Уникальная подвеска
        Transmission = new TransmissionSystem(transmissionType, "ZF"); // Уникальная трансмиссия

        Parts = new List<Part>
        {
            new Engine(engineModel),
            new Wheel(wheelType), // 1 уникальное колесо
            new Door(doorType), // 1 уникальная дверь
            new Seat(seatMaterial), // 1 уникальное сиденье
            new SteeringWheel(steeringType),
            new BrakeSystem(brakeType),
            new Battery(batteryType),
            new Transmission(transmissionType),
            new Radiator(),
            new Exhaust(),
            new FuelTank(),
            new Suspension(),
            new Airbag(),
            new Dashboard(),
            new Headlights(),
            new Mirror(),
            new Horn(),
            new Wiper(),
            new NavigationSystem()
        };

        // Добавляем уникальные колеса
        for (int i = 1; i < 4; i++)
            Parts.Add(new Wheel(wheelType)); // добавляем еще 3 колеса

        // Добавляем уникальные двери
        for (int i = 1; i < 4; i++)
            Parts.Add(new Door(doorType)); // добавляем еще 3 двери

        // Добавляем уникальные сиденья
        for (int i = 1; i < 5; i++)
            Parts.Add(new Seat(seatMaterial)); // добавляем еще 4 сиденья

        logger.LogInfo($"Автомобиль {Model} успешно создан с {Parts.Count} деталями");
    }

    public void ShowParts()
    {
        logger.LogInfo($"Отображение деталей автомобиля {Model}");
        Console.WriteLine($"Автомобиль {Model} состоит из:");
        HashSet<string> uniqueParts = new HashSet<string>();

        foreach (var part in Parts)
        {
            if (uniqueParts.Add(part.Name)) // добавляем только уникальные детали
            {
                Console.WriteLine("- " + part.Name);
            }
        }

        // Показываем кузов, интерьер, подвеску и трансмиссию
        Body.ShowBodyInfo();
        Interior.ShowInteriorInfo();
        Suspension.ShowSuspensionInfo();
        Transmission.ShowTransmissionInfo();

        logger.LogInfo($"Отображение деталей автомобиля {Model} завершено");
    }
}

// Класс спортивного автомобиля, наследуется от Car
public class SportsCar : Car
{
    public bool HasTurbo { get; set; }

    public SportsCar(string model, string engineModel, string wheelType, string doorType, string seatMaterial,
        string steeringType, string brakeType, string batteryType, string transmissionType,
        string bodyType, string bodyColor, string interiorMaterial, string interiorLayout,
        string suspensionType, string suspensionBrand, bool hasTurbo)
        : base(model, engineModel, wheelType, doorType, seatMaterial, steeringType, brakeType, batteryType, transmissionType, bodyType, bodyColor, interiorMaterial,
              interiorLayout, suspensionType, suspensionBrand)
    {
        HasTurbo = hasTurbo;
    }

    public new void ShowParts()
    {
        base.ShowParts();
        Console.WriteLine($"Турбонаддув: {(HasTurbo ? "Да" : "Нет")}");
    }
}

// Интерфейс для чтения из коллекции (ковариантность)
public interface IVehicleReader<out T>
{
    T GetVehicle(int index);
    int Count { get; }
}

// Интерфейс для записи в коллекцию (контравариантность)
public interface IVehicleWriter<in T>
{
    void AddVehicle(T vehicle);
}

#endregion

#region Сериализация

// Интерфейс для сериализации и десериализации данных
public interface IDataSerializer<T>
{
    // Название формата сериализации
    string FormatName { get; }

    // Сериализация коллекции в строку
    string Serialize(IEnumerable<T> data);

    // Десериализация строки в коллекцию
    IEnumerable<T> Deserialize(string data);

    // Сериализация в файл
    void SerializeToFile(IEnumerable<T> data, string filePath);

    // Десериализация из файла
    IEnumerable<T> DeserializeFromFile(string filePath);
}

// Базовый класс сериализатора с общими методами
public abstract class DataSerializerBase<T> : IDataSerializer<T>
{
    public abstract string FormatName { get; }

    // Абстрактные методы, которые должны реализовать конкретные сериализаторы
    public abstract string Serialize(IEnumerable<T> data);
    public abstract IEnumerable<T> Deserialize(string data);

    // Общие методы для работы с файлами
    public virtual void SerializeToFile(IEnumerable<T> data, string filePath)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

        try
        {
            string serializedData = Serialize(data);
            File.WriteAllText(filePath, serializedData);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Ошибка при сериализации в файл {filePath}: {ex.Message}", ex);
        }
    }

    public virtual IEnumerable<T> DeserializeFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл {filePath} не найден", filePath);

        try
        {
            string data = File.ReadAllText(filePath);
            return Deserialize(data);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Ошибка при десериализации из файла {filePath}: {ex.Message}", ex);
        }
    }
}

// Сериализатор XML
public class XmlDataSerializer<T> : DataSerializerBase<T> where T : class
{
    private readonly XmlSerializer _serializer;
    private readonly Logger<string> _logger;

    public override string FormatName => "XML";

    public XmlDataSerializer(Logger<string> logger)
    {
        _serializer = new XmlSerializer(typeof(List<T>));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string Serialize(IEnumerable<T> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        _logger.LogInfo($"Сериализация {typeof(T).Name} в формат {FormatName}");

        try
        {
            using (var stringWriter = new StringWriter())
            {
                _serializer.Serialize(stringWriter, data.ToList());
                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при сериализации в {FormatName}: {ex.Message}");
            throw new SerializationException($"Ошибка при сериализации в {FormatName}: {ex.Message}", ex);
        }
    }

    public override IEnumerable<T> Deserialize(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentException("Данные для десериализации не могут быть пустыми", nameof(data));

        _logger.LogInfo($"Десериализация из формата {FormatName} в {typeof(T).Name}");

        try
        {
            using (var stringReader = new StringReader(data))
            {
                return (List<T>)_serializer.Deserialize(stringReader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при десериализации из {FormatName}: {ex.Message}");
            throw new SerializationException($"Ошибка при десериализации из {FormatName}: {ex.Message}", ex);
        }
    }

    public override void SerializeToFile(IEnumerable<T> data, string filePath)
    {
        _logger.LogInfo($"Сохранение {typeof(T).Name} в файл {filePath} в формате {FormatName}");
        base.SerializeToFile(data, filePath);
        _logger.LogInfo($"Сохранение в файл {filePath} завершено успешно");
    }

    public override IEnumerable<T> DeserializeFromFile(string filePath)
    {
        _logger.LogInfo($"Загрузка из файла {filePath} в формате {FormatName}");
        var result = base.DeserializeFromFile(filePath);
        _logger.LogInfo($"Загрузка из файла {filePath} завершена успешно, загружено {result.Count()} элементов");
        return result;
    }
}

// Сериализатор JSON (заглушка)
public class JsonDataSerializer<T> : DataSerializerBase<T> where T : class
{
    private readonly Logger<string> _logger;

    public override string FormatName => "JSON";

    public JsonDataSerializer(Logger<string> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string Serialize(IEnumerable<T> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        _logger.LogInfo($"Сериализация {typeof(T).Name} в формат {FormatName}");

        try
        {
            // Используем System.Text.Json вместо Newtonsoft.Json
            _logger.LogInfo("Это программная заглушка, полная реализация JSON будет добавлена позже");
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при сериализации в {FormatName}: {ex.Message}");
            throw new SerializationException($"Ошибка при сериализации в {FormatName}: {ex.Message}", ex);
        }
    }

    public override IEnumerable<T> Deserialize(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentException("Данные для десериализации не могут быть пустыми", nameof(data));

        _logger.LogInfo($"Десериализация из формата {FormatName} в {typeof(T).Name}");

        try
        {
            // Используем System.Text.Json вместо Newtonsoft.Json
            _logger.LogInfo("Это программная заглушка, полная реализация JSON будет добавлена позже");
            return JsonSerializer.Deserialize<List<T>>(data);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при десериализации из {FormatName}: {ex.Message}");
            throw new SerializationException($"Ошибка при десериализации из {FormatName}: {ex.Message}", ex);
        }
    }
}

// Фабрика сериализаторов
public class SerializerFactory
{
    private readonly Dictionary<string, Func<Logger<string>, IDataSerializer<SerializableCar>>> _serializers;
    private readonly Logger<string> _logger;

    public SerializerFactory(Logger<string> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _serializers = new Dictionary<string, Func<Logger<string>, IDataSerializer<SerializableCar>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "xml", logger => new XmlDataSerializer<SerializableCar>(logger) },
            { "json", logger => new JsonDataSerializer<SerializableCar>(logger) }
        };
    }

    public IDataSerializer<SerializableCar> GetSerializer(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Формат не может быть пустым", nameof(format));

        if (!_serializers.TryGetValue(format, out var serializerFactory))
            throw new ArgumentException($"Неподдерживаемый формат: {format}", nameof(format));

        _logger.LogInfo($"Создан сериализатор для формата {format}");
        return serializerFactory(_logger);
    }

    public IEnumerable<string> GetSupportedFormats()
    {
        return _serializers.Keys;
    }
}

// Класс для хранения сериализуемой версии автомобиля
[Serializable]
public class SerializableCar
{
    public string Model { get; set; }
    public string BodyType { get; set; }
    public string BodyColor { get; set; }
    public string InteriorMaterial { get; set; }
    public string InteriorLayout { get; set; }
    public string SuspensionType { get; set; }
    public string SuspensionBrand { get; set; }
    public string TransmissionType { get; set; }
    public bool HasTurbo { get; set; }
    public List<string> PartNames { get; set; } = new List<string>();

    // Конструктор по умолчанию для сериализации
    public SerializableCar() { }

    // Конструктор для преобразования из Car
    public SerializableCar(Car car)
    {
        Model = car.Model;
        BodyType = car.Body.Type;
        BodyColor = car.Body.Color;
        InteriorMaterial = car.Interior.Material;
        InteriorLayout = car.Interior.Layout;
        SuspensionType = car.Suspension.Type;
        SuspensionBrand = car.Suspension.Brand;
        TransmissionType = car.Transmission.Type;

        // Проверяем, является ли автомобиль спортивным
        if (car is SportsCar sportsCar)
        {
            HasTurbo = sportsCar.HasTurbo;
        }

        // Сохраняем названия деталей
        foreach (var part in car.Parts)
        {
            PartNames.Add(part.Name);
        }
    }

    // Метод для преобразования обратно в Car
    public Car ToCar()
    {
        // Упрощенная версия воссоздания Car из SerializableCar
        // В реальном приложении здесь бы был более сложный код воссоздания полного объекта
        if (HasTurbo)
        {
            return new SportsCar(
                Model,
                "Восстановленный двигатель",
                "Восстановленные колеса",
                "Восстановленные двери",
                InteriorMaterial,
                "Восстановленный руль",
                "Восстановленные тормоза",
                "Восстановленный аккумулятор",
                TransmissionType,
                BodyType,
                BodyColor,
                InteriorMaterial,
                InteriorLayout,
                SuspensionType,
                SuspensionBrand,
                true);
        }
        else
        {
            return new Car(
                Model,
                "Восстановленный двигатель",
                "Восстановленные колеса",
                "Восстановленные двери",
                InteriorMaterial,
                "Восстановленный руль",
                "Восстановленные тормоза",
                "Восстановленный аккумулятор",
                TransmissionType,
                BodyType,
                BodyColor,
                InteriorMaterial,
                InteriorLayout,
                SuspensionType,
                SuspensionBrand);
        }
    }
}

#endregion

#region Коллекции и контейнеры

// Обобщенная коллекция с ограничением типа
public class GarageCollection<T> : ICollection<T>, IEnumerable<T>, IVehicleReader<T>, IVehicleWriter<T> where T : Car
{
    private List<T> _vehicles = new List<T>();
    private SortingLogger _sortingLogger;

    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: блокировка для синхронизации доступа к сортировке
    private readonly object _sortLock = new object();

    private bool _isSorting = false;

    // Добавляем событие для отслеживания прогресса сортировки
    // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: событие, позволяющее потоку сортировки сообщать о прогрессе
    public event Action<string, double> SortingProgressChanged;

    // Конструктор с логгером
    public GarageCollection(Logger<string> logger)
    {
        _sortingLogger = new SortingLogger(logger);
        // Подписываемся на событие прогресса логирования
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: подписка на события от потока логирования
        _sortingLogger.LoggingProgressChanged += (sortType, progress) =>
        {
            SortingProgressChanged?.Invoke(sortType, progress);
        };
    }

    // Реализация ICollection<T>
    public int Count => _vehicles.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _vehicles.Add(item);
    }

    public void Clear()
    {
        _vehicles.Clear();
    }

    public bool Contains(T item)
    {
        return _vehicles.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _vehicles.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _vehicles.Remove(item);
    }

    // Реализация IEnumerable<T>
    public IEnumerator<T> GetEnumerator()
    {
        return _vehicles.GetEnumerator();
    }

    // Реализация IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Реализация IVehicleReader<T> (ковариантность)
    public T GetVehicle(int index)
    {
        if (index < 0 || index >= _vehicles.Count)
            throw new IndexOutOfRangeException();
        return _vehicles[index];
    }

    // Реализация IVehicleWriter<T> (контравариантность)
    public void AddVehicle(T vehicle)
    {
        _vehicles.Add(vehicle);
    }

    // Дополнительные методы
    public T this[int index]
    {
        get { return _vehicles[index]; }
        set { _vehicles[index] = value; }
    }

    // Метод для получения процентного прогресса сортировки
    public static string GetProgressString(double progress)
    {
        int percent = (int)(progress * 100);
        int barLength = 20;
        int completedLength = (int)(barLength * progress);

        string bar = "[" + new string('#', completedLength) + new string(' ', barLength - completedLength) + "]";
        return $"{bar} {percent}%";
    }

    // Асинхронный метод сортировки с использованием Func<T, T, int>
    public async Task SortByAsync(Func<T, T, int> compareFunc, string sortType = "пользовательская")
    {
        // Предотвращаем одновременные сортировки (взаимодействие между потоками)
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: проверка блокировки перед началом сортировки
        if (_isSorting)
        {
            _sortingLogger._logger.LogWarning($"Сортировка {sortType} отложена, так как другая сортировка уже запущена");
            await Task.Delay(50);
        }

        // Блокировка для синхронизации между потоками
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: блокировка для предотвращения параллельных сортировок
        lock (_sortLock)
        {
            _isSorting = true;
        }

        try
        {
            // Идентификатор текущей операции сортировки
            Guid sortId = Guid.NewGuid();

            // Отправляем сообщение о начале сортировки с ожиданием подтверждения
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: отправка сообщения в поток логирования и ожидание обратного вызова
            bool startAcknowledged = await _sortingLogger.EnqueueLogMessageWithAcknowledgmentAsync(new SortingLogMessage
            {
                Message = "Начало сортировки",
                SortType = sortType,
                ProcessedItems = 0,
                IsComplete = false,
                SortId = sortId
            });

            if (!startAcknowledged)
            {
                _sortingLogger._logger.LogWarning($"Не получено подтверждение о начале сортировки {sortType}");
            }

            // Для двустороннего взаимодействия будем использовать список подтверждений
            List<Guid> pendingAcknowledgments = new List<Guid>();
            pendingAcknowledgments.Add(sortId);

            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: запуск сортировки в отдельном потоке
            await Task.Run(async () =>
            {
                // Сортируем в отдельном потоке
                // Для демонстрации логирования процесса сортировки, реализуем свою сортировку
                int n = _vehicles.Count;
                int processedItems = 0;
                int lastReportedItems = 0;

                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = 0; j < n - i - 1; j++)
                    {
                        if (compareFunc(_vehicles[j], _vehicles[j + 1]) > 0)
                        {
                            // Меняем элементы местами
                            T temp = _vehicles[j];
                            _vehicles[j] = _vehicles[j + 1];
                            _vehicles[j + 1] = temp;
                        }

                        processedItems++;

                        // Каждые несколько операций отправляем сообщение о прогрессе
                        if (processedItems % 10 == 0 || processedItems == (n * (n - 1)) / 2)
                        {
                            if (processedItems - lastReportedItems >= 10)
                            {
                                lastReportedItems = processedItems;

                                // Отслеживаем идентификатор для этого сообщения о прогрессе
                                Guid progressId = Guid.NewGuid();
                                pendingAcknowledgments.Add(progressId);

                                // Отправляем сообщение о прогрессе с ожиданием подтверждения
                                // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: отправка информации о прогрессе и ожидание подтверждения
                                await _sortingLogger.EnqueueLogMessageWithAcknowledgmentAsync(new SortingLogMessage
                                {
                                    Message = "Прогресс сортировки",
                                    SortType = sortType,
                                    ProcessedItems = processedItems,
                                    IsComplete = false,
                                    SortId = progressId
                                });

                                // Имитация длительной операции для демонстрации асинхронности
                                Thread.Sleep(10);

                                // Очищаем подтвержденные сообщения
                                // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: проверка подтверждений из потока логирования
                                pendingAcknowledgments.RemoveAll(id => _sortingLogger.IsMessageAcknowledged(id));
                            }
                        }
                    }
                }

                // Отправляем сообщение о завершении сортировки с ожиданием подтверждения
                // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: сигнализация о завершении сортировки и ожидание подтверждения
                Guid completeId = Guid.NewGuid();
                bool completeAcknowledged = await _sortingLogger.EnqueueLogMessageWithAcknowledgmentAsync(new SortingLogMessage
                {
                    Message = "Сортировка завершена",
                    SortType = sortType,
                    ProcessedItems = processedItems,
                    IsComplete = true,
                    SortId = completeId
                });

                if (!completeAcknowledged)
                {
                    _sortingLogger._logger.LogWarning($"Не получено подтверждение о завершении сортировки {sortType}");
                }
            });

            // Ожидаем завершения всех операций логирования асинхронно (взаимодействие между потоками)
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: ожидание завершения всех операций в потоке логирования
            await _sortingLogger.WaitForLoggingAsync();

            // Дополнительно выводим отчет о взаимодействии
            _sortingLogger._logger.LogInfo($"Взаимодействие потоков при сортировке {sortType} завершено успешно");
        }
        finally
        {
            // Снимаем блокировку (взаимодействие между потоками)
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: освобождение блокировки для разрешения других сортировок
            lock (_sortLock)
            {
                _isSorting = false;
            }
        }
    }

    // Асинхронный метод сортировки с использованием делегата Comparison<T>
    public async Task SortAsync(Comparison<T> comparison, string sortType = "по умолчанию")
    {
        // Преобразуем Comparison<T> в Func<T, T, int>
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: делегирование асинхронной операции другому методу
        await SortByAsync((x, y) => comparison(x, y), sortType);
    }

    // Метод для выполнения действия над каждым элементом с использованием Action<T>
    public void ForEach(Action<T> action)
    {
        foreach (var vehicle in _vehicles)
        {
            action(vehicle);
        }
    }

    // Метод поиска с использованием Func<T, bool>
    public List<T> Find(Func<T, bool> predicate)
    {
        return _vehicles.FindAll(vehicle => predicate(vehicle));
    }

    // Метод для фильтрации по условию с использованием Func<T, bool>
    public GarageCollection<T> Filter(Func<T, bool> filter)
    {
        GarageCollection<T> result = new GarageCollection<T>(_sortingLogger._logger);
        foreach (var vehicle in _vehicles)
        {
            if (filter(vehicle))
            {
                result.Add(vehicle);
            }
        }
        return result;
    }

    // Останавливает логгер при завершении работы с коллекцией
    public void Dispose()
    {
        // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: остановка потока логирования
        _sortingLogger.Stop();
    }

    // Методы для сериализации и десериализации коллекции
    public void SaveToFile(string filePath, string format, SerializerFactory serializerFactory)
    {
        if (serializerFactory == null)
            throw new ArgumentNullException(nameof(serializerFactory));

        try
        {
            var serializer = serializerFactory.GetSerializer(format);

            // Преобразуем Car в SerializableCar для безопасной сериализации
            List<SerializableCar> serializableCars = _vehicles.Select(car => new SerializableCar(car)).ToList();

            // Используем сериализатор для записи в файл
            serializer.SerializeToFile(serializableCars, filePath);
        }
        catch (Exception ex)
        {
            _sortingLogger._logger.LogError($"Ошибка при сохранении в файл {filePath} в формате {format}: {ex.Message}");
            throw;
        }
    }

    public void LoadFromFile(string filePath, string format, SerializerFactory serializerFactory)
    {
        if (serializerFactory == null)
            throw new ArgumentNullException(nameof(serializerFactory));

        try
        {
            var serializer = serializerFactory.GetSerializer(format);

            // Десериализуем в SerializableCar
            var serializableCars = serializer.DeserializeFromFile(filePath);

            // Очищаем текущую коллекцию
            _vehicles.Clear();

            // Преобразуем SerializableCar обратно в Car и добавляем в коллекцию
            foreach (var serializableCar in serializableCars)
            {
                Car car = serializableCar.ToCar();
                if (car is T typedCar)
                {
                    _vehicles.Add(typedCar);
                }
                else
                {
                    _sortingLogger._logger.LogWarning($"Невозможно преобразовать десериализованный автомобиль {serializableCar.Model} в тип {typeof(T).Name}");
                }
            }
        }
        catch (Exception ex)
        {
            _sortingLogger._logger.LogError($"Ошибка при загрузке из файла {filePath} в формате {format}: {ex.Message}");
            throw;
        }
    }
}

// Салон/Гараж для автомобилей
public class Garage
{
    // Создаем логгер для класса Garage
    private static Logger<string> logger = new Logger<string>();
    private GarageCollection<Car> cars;

    // Статический конструктор для подключения логгеров
    static Garage()
    {
        // Создаем экземпляры обработчиков логов
        ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
        FileLogWriter fileWriter = new FileLogWriter("garage_logs.txt");

        // Подписываем обработчики на событие логирования
        logger.LogEvent += consoleWriter.WriteToConsole;
        logger.LogEvent += fileWriter.WriteToFile;
    }

    public Garage()
    {
        cars = new GarageCollection<Car>(logger);
    }

    public void AddCar(Car car)
    {
        logger.LogInfo($"Добавление автомобиля {car.Model} в гараж");
        cars.Add(car);
    }

    public void ShowCars()
    {
        logger.LogInfo("Отображение всех автомобилей в гараже");
        Console.WriteLine("Автомобили в гараже:");
        foreach (var car in cars)
        {
            Console.WriteLine("- " + car.Model);
            car.ShowParts(); // Вывод всех деталей автомобиля
            Console.WriteLine(); // Пустая строка для разделения
        }
        logger.LogInfo("Отображение всех автомобилей завершено");
    }

    // Демонстрация ковариантности
    public IVehicleReader<Car> GetReader()
    {
        logger.LogInfo("Получение reader-интерфейса для демонстрации ковариантности");
        return cars;
    }

    // Демонстрация контравариантности
    public IVehicleWriter<SportsCar> GetSportsCarWriter()
    {
        logger.LogInfo("Получение writer-интерфейса для демонстрации контравариантности");
        // Контравариантность позволяет присвоить IVehicleWriter<Car> переменной типа IVehicleWriter<SportsCar>
        return cars as IVehicleWriter<SportsCar>;
    }

    // Доступ к коллекции автомобилей
    public GarageCollection<Car> Cars => cars;

    // Асинхронная сортировка автомобилей по модели
    public async Task SortByModelAsync()
    {
        logger.LogInfo("Запуск асинхронной сортировки автомобилей по модели");
        try
        {
            await cars.SortAsync((car1, car2) => string.Compare(car1.Model, car2.Model), "по модели (алфавитный порядок)");
            logger.LogInfo("Асинхронная сортировка автомобилей по модели завершена");
        }
        catch (Exception ex)
        {
            logger.LogError($"Ошибка при сортировке по модели: {ex.Message}");
            throw;
        }
    }

    // Асинхронная сортировка автомобилей по заданному критерию
    public async Task SortCarsAsync(Func<Car, Car, int> comparer, string sortType)
    {
        logger.LogInfo($"Запуск асинхронной сортировки автомобилей по критерию: {sortType}");
        try
        {
            // Создаем делегат Comparison<Car> из Func<Car, Car, int>
            Comparison<Car> comparison = (car1, car2) => comparer(car1, car2);
            await cars.SortAsync(comparison, sortType);
            logger.LogInfo($"Асинхронная сортировка автомобилей по критерию: {sortType} завершена");
        }
        catch (Exception ex)
        {
            logger.LogError($"Ошибка при сортировке по критерию {sortType}: {ex.Message}");
            throw;
        }
    }

    // Выполнение действия над всеми автомобилями (Action)
    public void ProcessAllCars(Action<Car> action)
    {
        logger.LogInfo("Выполнение действия над всеми автомобилями");
        cars.ForEach(action);
    }

    // Поиск автомобилей по критерию (Func)
    public List<Car> FindCars(Func<Car, bool> criteria)
    {
        logger.LogInfo("Поиск автомобилей по критерию");
        return cars.Find(criteria);
    }

    // Сохранение гаража в файл
    public void SaveToFile(string filePath, string format)
    {
        logger.LogInfo($"Сохранение гаража в файл {filePath} в формате {format}");
        try
        {
            SerializerFactory factory = new SerializerFactory(logger);
            cars.SaveToFile(filePath, format, factory);
            logger.LogInfo($"Гараж успешно сохранен в файл {filePath}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Ошибка при сохранении гаража: {ex.Message}");
            ExceptionHandler.HandleStandardException(ex);
        }
    }

    // Загрузка гаража из файла
    public void LoadFromFile(string filePath, string format)
    {
        logger.LogInfo($"Загрузка гаража из файла {filePath} в формате {format}");
        try
        {
            SerializerFactory factory = new SerializerFactory(logger);
            cars.LoadFromFile(filePath, format, factory);
            logger.LogInfo($"Гараж успешно загружен из файла {filePath}, автомобилей: {cars.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Ошибка при загрузке гаража: {ex.Message}");
            ExceptionHandler.HandleStandardException(ex);
        }
    }
}

#endregion

#region Основная программа

// Основной класс программы
public class Program
{
    // Создаем логгер для основной программы
    private static Logger<string> logger = new Logger<string>();

    static Program()
    {
        // Создаем экземпляры обработчиков логов
        ConsoleLogWriter consoleWriter = new ConsoleLogWriter();
        FileLogWriter fileWriter = new FileLogWriter("program_logs.txt");

        // Подписываем обработчики на событие логирования
        logger.LogEvent += consoleWriter.WriteToConsole;
        logger.LogEvent += fileWriter.WriteToFile;
    }

    static async Task Main()
    {
        try
        {
            // Загружаем конфигурацию
            AppConfiguration config = ConfigurationManager.GetConfiguration();
            logger.LogInfo($"Запуск программы {config.ApplicationName} версии {config.Version}");
            logger.LogInfo($"Максимальное количество автомобилей в гараже: {config.MaxCarsInGarage}");
            logger.LogInfo($"Поддерживаемые марки: {string.Join(", ", config.SupportedCarBrands)}");

            Garage garage = new Garage();

            // Проверяем, разрешена ли марка автомобиля конфигурацией
            AddCarWithValidation(garage, "Toyota Camry", "V6 2GR-FE", "Michelin Pilot Sport 4", "Металлическая", "Кожа", "Momo Prototipo", "Brembo ABS", "Varta AGM", "Aisin ECT", "Седан", "Белый", "Кожа", "5 мест", "Независимая", "Bilstein", config);
            AddCarWithValidation(garage, "BMW X5", "Twin Turbo N63", "Pirelli P Zero", "Алюминиевые", "Алькантара", "M-Technic", "Carbon Ceramic", "Bosch AGM", "ZF 8HP", "Кроссовер", "Черный", "Алькантара", "5 мест", "Независимая", "Bosch", config);
            AddCarWithValidation(garage, "Mercedes S-Class", "V8 M177", "Continental ContiSportContact 5", "Комбинированная", "Натуральная кожа", "AMG Performance", "Hydraulic", "Exide Premium", "9G-Tronic", "Лимузин", "Серебристый", "Натуральная кожа", "5 мест", "Пневматическая", "Bilstein", config);
            AddCarWithValidation(garage, "Audi Q7", "V6 TDI 3.0", "Bridgestone Blizzak DM-V2", "Стеклопластик", "Ткань", "Audi Sport", "Disc", "Varta Silver Dynamic", "Tiptronic", "Универсал", "Синий", "Ткань", "7 мест", "Независимая", "Bosch", config);

            // Добавляем спортивный автомобиль
            AddSportsCarWithValidation(garage, "Ferrari 488 GTB", "V8 Twin-Turbo", "Pirelli P Zero Corsa", "Карбоновые", "Alcantara",
                "Ferrari Carbon", "Brembo Carbon-Ceramic", "Li-Ion", "7-ступенчатая F1",
                "Купе", "Красный", "Карбон и алькантара", "2 места",
                "Спортивная", "Magneti Marelli", true, config);

            // Попробуем добавить неподдерживаемую марку (вызовет исключение)
            try
            {
                AddCarWithValidation(garage, "Lada Vesta", "1.6L", "Кама", "Стальные", "Ткань", "Базовый", "Дисковые", "6СТ-60", "5-ступенчатая МКПП", "Седан", "Синий", "Ткань", "5 мест", "Макферсон", "Автоваз", config);
            }
            catch (CarApplicationException ex)
            {
                ExceptionHandler.HandleCarApplicationException(ex);
            }

            // Демонстрация обработки исключений - попытка загрузить несуществующий файл конфигурации
            try
            {
                if (!File.Exists("nonexistent_file.txt"))
                {
                    throw new FileNotFoundException("Демонстрация обработки стандартного исключения", "nonexistent_file.txt");
                }
            }
            catch (FileNotFoundException ex)
            {
                ExceptionHandler.HandleStandardException(ex);
            }

            // Демонстрация обработки исключений - ошибка ввода/вывода
            try
            {
                if (Directory.Exists("C:\\Windows\\System32"))
                {
                    throw new IOException("Демонстрация обработки исключения ввода/вывода");
                }
            }
            catch (IOException ex)
            {
                ExceptionHandler.HandleStandardException(ex);
            }

            // Демонстрация пользовательского исключения с деталью
            try
            {
                throw new CarPartException("Демонстрация пользовательского исключения о детали", "ERR-PART-001", "Двигатель V8");
            }
            catch (CarPartException ex)
            {
                ExceptionHandler.HandleCarApplicationException(ex);
            }

            logger.LogInfo("Отображение списка автомобилей");
            garage.ShowCars();

            // Демонстрация асинхронной сортировки
            Console.WriteLine("\n=== Демонстрация асинхронной сортировки в отдельном потоке ===");

            // Подписка на события прогресса сортировки (взаимодействие между потоками)
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: подписка основного потока на события прогресса сортировки
            garage.Cars.SortingProgressChanged += (sortType, progress) =>
            {
                Console.Write($"\rПрогресс сортировки {sortType}: {GarageCollection<Car>.GetProgressString(progress)}");
                if (progress >= 0.99)
                    Console.WriteLine();
            };

            Console.WriteLine("\nАсинхронная сортировка по модели (в алфавитном порядке):");
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: запуск асинхронной сортировки и ожидание её завершения
            await garage.SortByModelAsync();
            garage.ShowCars();

            // Добавляем паузу между сортировками
            await Task.Delay(100);

            Console.WriteLine("\nАсинхронная сортировка по модели (в обратном порядке):");
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: еще один запуск асинхронной сортировки
            await garage.SortCarsAsync((car1, car2) => string.Compare(car2.Model, car1.Model), "по модели (обратный порядок)");
            garage.ShowCars();

            // Добавляем паузу между сортировками
            await Task.Delay(100);

            Console.WriteLine("\nАсинхронная сортировка по типу кузова:");
            await garage.SortCarsAsync((car1, car2) => string.Compare(car1.Body.Type, car2.Body.Type), "по типу кузова");
            garage.ShowCars();

            // Выполнение нескольких сортировок параллельно
            Console.WriteLine("\n=== Демонстрация параллельной асинхронной сортировки ===");
            Console.WriteLine("Начало параллельной сортировки нескольких коллекций...");

            // Создаем три дополнительных гаража для параллельной сортировки
            Garage garage1 = new Garage();
            Garage garage2 = new Garage();
            Garage garage3 = new Garage();

            // Наполняем их автомобилями
            for (int i = 0; i < 5; i++)
            {
                garage1.AddCar(new Car($"Toyota Camry {i}", "V6", "Michelin", "Стандартные", "Кожа", "Стандарт", "ABS", "Standard", "Auto", "Седан", "Белый", "Кожа", "5 мест", "Независимая", "Standard"));
                garage2.AddCar(new Car($"BMW X5 {i}", "V8", "Pirelli", "Люкс", "Кожа", "Спорт", "ABS+", "Premium", "Auto", "Кроссовер", "Черный", "Кожа", "5 мест", "Независимая", "Premium"));
                garage3.AddCar(new Car($"Mercedes S {i}", "V12", "Continental", "Премиум", "Кожа", "Люкс", "ABS++", "Luxury", "Auto", "Седан", "Серебро", "Кожа", "5 мест", "Пневматическая", "Luxury"));
            }

            // Запускаем сортировки параллельно (взаимодействие между потоками)
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: запуск нескольких параллельных сортировок в разных потоках
            Task[] sortTasks = new Task[]
            {
                garage1.SortByModelAsync(),
                garage2.SortCarsAsync((car1, car2) => string.Compare(car2.Model, car1.Model), "по модели (обратный порядок)"),
                garage3.SortCarsAsync((car1, car2) => string.Compare(car1.Body.Type, car2.Body.Type), "по типу кузова")
            };

            // Ожидаем завершения всех сортировок (взаимодействие между потоками)
            // ВЗАИМОДЕЙСТВИЕ МЕЖДУ ПОТОКАМИ: ожидание завершения всех параллельных задач
            await Task.WhenAll(sortTasks);

            Console.WriteLine("Все параллельные сортировки завершены!");

            // Остальная демонстрация
            await PerformDemonstrationsAsync(garage);

            logger.LogInfo("Программа завершена успешно");

            // Демонстрация сериализации и десериализации
            Console.WriteLine("\n=== Демонстрация сериализации и десериализации ===");

            // Создаем фабрику сериализаторов
            SerializerFactory factory = new SerializerFactory(logger);

            // Выводим список поддерживаемых форматов
            Console.WriteLine("Поддерживаемые форматы сериализации:");
            foreach (var format in factory.GetSupportedFormats())
            {
                Console.WriteLine($"- {format}");
            }

            // Демонстрация сериализации в XML
            Console.WriteLine("\nСериализация гаража в XML:");
            string xmlFilePath = "garage.xml";
            try
            {
                garage.SaveToFile(xmlFilePath, "XML");
                Console.WriteLine($"Гараж успешно сохранен в файл {xmlFilePath}");

                // Выводим содержимое файла
                Console.WriteLine("\nСодержимое файла XML:");
                string xmlContent = File.ReadAllText(xmlFilePath);
                Console.WriteLine(xmlContent.Substring(0, Math.Min(500, xmlContent.Length)) + (xmlContent.Length > 500 ? "..." : ""));
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при демонстрации сериализации XML: {ex.Message}");
                ExceptionHandler.HandleStandardException(ex);
            }

            // Создаем новый гараж и загружаем в него данные из файла
            Console.WriteLine("\nДесериализация гаража из XML:");
            try
            {
                Garage newGarage = new Garage();
                newGarage.LoadFromFile(xmlFilePath, "XML");
                Console.WriteLine("Автомобили, загруженные из XML файла:");
                newGarage.ShowCars();
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при демонстрации десериализации XML: {ex.Message}");
                ExceptionHandler.HandleStandardException(ex);
            }

            // Демонстрация JSON (программная заглушка)
            Console.WriteLine("\nСериализация гаража в JSON (программная заглушка):");
            string jsonFilePath = "garage.json";
            try
            {
                garage.SaveToFile(jsonFilePath, "JSON");
                Console.WriteLine($"Гараж успешно сохранен в файл {jsonFilePath}");

                // Выводим содержимое файла
                Console.WriteLine("\nСодержимое файла JSON:");
                string jsonContent = File.ReadAllText(jsonFilePath);
                Console.WriteLine(jsonContent.Substring(0, Math.Min(500, jsonContent.Length)) + (jsonContent.Length > 500 ? "..." : ""));
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при демонстрации сериализации JSON: {ex.Message}");
                ExceptionHandler.HandleStandardException(ex);
            }
        }
        catch (CarApplicationException ex)
        {
            ExceptionHandler.HandleCarApplicationException(ex);
        }
        catch (Exception ex)
        {
            ExceptionHandler.HandleStandardException(ex);
        }

        Console.WriteLine("\n--- Информация о диаграмме классов ---");
        Console.WriteLine("Для создания диаграммы классов рекомендуется использовать Visual Studio и расширение Class Designer:");
        Console.WriteLine("1. В Visual Studio выберите Проект -> Добавить новый элемент");
        Console.WriteLine("2. Выберите Диаграмма классов (Class Diagram)");
        Console.WriteLine("3. Перетащите существующие классы из обозревателя решений на диаграмму");
        Console.WriteLine("4. Сохраните диаграмму в формате .cd");
        Console.WriteLine();
        Console.WriteLine("Альтернативно, для создания UML диаграммы можно использовать:");
        Console.WriteLine("- PlantUML (https://plantuml.com/)");
        Console.WriteLine("- Microsoft Visio");
        Console.WriteLine("- Lucidchart");
        Console.WriteLine("- Draw.io");
    }

    // Метод для выполнения всех демонстраций
    private static async Task PerformDemonstrationsAsync(Garage garage)
    {
        // Добавляем небольшую задержку перед демонстрацией ковариантности и контравариантности
        await Task.Delay(100);

        logger.LogInfo("Демонстрация ковариантности");
        // Демонстрация ковариантности и контравариантности
        Console.WriteLine("\nДемонстрация ковариантности:");
        IVehicleReader<Car> reader = garage.GetReader();
        Console.WriteLine($"Первый автомобиль в коллекции: {reader.GetVehicle(0).Model}");

        // Использование ковариантного интерфейса для работы с базовым типом
        // Создаем новую коллекцию спортивных автомобилей
        GarageCollection<SportsCar> sportsCars = new GarageCollection<SportsCar>(logger);
        sportsCars.Add(new SportsCar("Lamborghini Huracan", "V10", "Pirelli P Zero", "Карбоновые", "Алькантара",
            "Карбоновый", "Керамические", "Литий-ионный", "7-ступенчатая роботизированная",
            "Купе", "Желтый", "Алькантара", "2 места", "Спортивная", "Bilstein", true));

        IVehicleReader<Car> carReader = sportsCars; // Ковариантность в действии

        logger.LogInfo("Демонстрация контравариантности");
        // Использование контравариантного интерфейса для работы с производным типом
        Console.WriteLine("\nДемонстрация контравариантности:");
        IVehicleWriter<SportsCar> writer = garage.GetSportsCarWriter();
        if (writer != null)
        {
            Console.WriteLine("Можно добавлять спортивные автомобили через интерфейс для базового класса");
        }

        // Использование Action для вывода только моделей автомобилей
        logger.LogInfo("Вывод только моделей автомобилей с использованием Action");
        Console.WriteLine("\nТолько модели автомобилей (с использованием Action):");
        garage.ProcessAllCars(car => Console.WriteLine($"Модель: {car.Model}"));

        // Поиск автомобилей с использованием Func
        logger.LogInfo("Поиск автомобилей с кожаным салоном");
        Console.WriteLine("\nПоиск автомобилей с кожаным салоном:");
        List<Car> leatherCars = garage.FindCars(car => car.Interior.Material.Contains("кожа") || car.Interior.Material.Contains("Кожа"));
        foreach (var car in leatherCars)
        {
            Console.WriteLine($"- {car.Model} (материал салона: {car.Interior.Material})");
        }

        // Демонстрация использования собственного фильтра
        logger.LogInfo("Фильтрация автомобилей с 5 местами");
        Console.WriteLine("\nФильтрация: автомобили с 5 местами:");
        var fiveSeatCars = garage.Cars.Filter(car => car.Interior.Layout == "5 мест");
        foreach (var car in fiveSeatCars)
        {
            Console.WriteLine($"- {car.Model} (мест: {car.Interior.Layout})");
        }
    }

    // Метод для добавления автомобиля с проверкой
    private static void AddCarWithValidation(Garage garage, string model, string engineModel, string wheelType, string doorType, string seatMaterial, string steeringType, string brakeType, string batteryType, string transmissionType, string bodyType, string bodyColor, string interiorMaterial, string interiorLayout, string suspensionType, string suspensionBrand, AppConfiguration config)
    {
        // Проверяем, поддерживается ли марка
        string brand = model.Split(' ')[0];
        if (!config.SupportedCarBrands.Contains(brand))
        {
            throw new CarApplicationException($"Марка {brand} не поддерживается системой", "ERR-BRAND-001");
        }

        // Проверяем, не превышен ли лимит автомобилей
        if (garage.Cars.Count >= config.MaxCarsInGarage)
        {
            throw new CarApplicationException($"Превышен лимит автомобилей в гараже ({config.MaxCarsInGarage})", "ERR-LIMIT-001");
        }

        logger.LogInfo($"Добавление автомобиля {model}");
        garage.AddCar(new Car(model, engineModel, wheelType, doorType, seatMaterial, steeringType, brakeType, batteryType, transmissionType, bodyType, bodyColor, interiorMaterial, interiorLayout, suspensionType, suspensionBrand));
    }

    // Метод для добавления спортивного автомобиля с проверкой
    private static void AddSportsCarWithValidation(Garage garage, string model, string engineModel, string wheelType, string doorType, string seatMaterial, string steeringType, string brakeType, string batteryType, string transmissionType, string bodyType, string bodyColor, string interiorMaterial, string interiorLayout, string suspensionType, string suspensionBrand, bool hasTurbo, AppConfiguration config)
    {
        // Проверяем, поддерживается ли марка
        string brand = model.Split(' ')[0];
        if (!config.SupportedCarBrands.Contains(brand))
        {
            throw new CarApplicationException($"Марка {brand} не поддерживается системой", "ERR-BRAND-001");
        }

        // Проверяем, не превышен ли лимит автомобилей
        if (garage.Cars.Count >= config.MaxCarsInGarage)
        {
            throw new CarApplicationException($"Превышен лимит автомобилей в гараже ({config.MaxCarsInGarage})", "ERR-LIMIT-001");
        }

        logger.LogInfo($"Добавление спортивного автомобиля {model}");
        garage.AddCar(new SportsCar(model, engineModel, wheelType, doorType, seatMaterial, steeringType, brakeType, batteryType, transmissionType, bodyType, bodyColor, interiorMaterial, interiorLayout, suspensionType, suspensionBrand, hasTurbo));
    }
}

#endregion
