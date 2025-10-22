using System.Globalization;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.Validators;
using HotelManagementSystem.Exceptions;

class Program
{
    private const string ConnectionString = @"Server=localhost\MSSQLSERVER11;Database=Normal_bd_ado;User ID=sa;Password=1qwe4rt6;TrustServerCertificate=True;";
    private static readonly HotelService service = new HotelService(ConnectionString);

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            DisplayHeader();
            DisplayMainMenu();

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAvailableRooms();
                    break;
                case "2":
                    BookRoom();
                    break;
                case "3":
                    CheckIn();
                    break;
                case "4":
                    CheckOut();
                    break;
                case "5":
                    ManageCustomers();
                    break;
                case "6":
                    ViewReservations();
                    break;
                case "7":
                    ShowReports();
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    ShowMessage("Неверный выбор. Нажмите любую клавишу...", ConsoleColor.Red);
                    Console.ReadKey();
                    break;
            }
        }

        ShowMessage("До свидания!", ConsoleColor.Green);
    }

    static void DisplayHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Система управления гостиницей ===");
        Console.ResetColor();
        Console.WriteLine($"Сегодня: {DateTime.Today:dd.MM.yyyy}");
        Console.WriteLine();
    }

    static void DisplayMainMenu()
    {
        Console.WriteLine("1. Просмотреть свободные номера");
        Console.WriteLine("2. Забронировать номер");
        Console.WriteLine("3. Заселение");
        Console.WriteLine("4. Выселение");
        Console.WriteLine("5. Управление клиентами");
        Console.WriteLine("6. Просмотр бронирований");
        Console.WriteLine("7. Отчеты");
        Console.WriteLine("0. Выход");
        Console.WriteLine();
        Console.Write("Выберите действие: ");
    }

    static void ShowAvailableRooms()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Свободные номера ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите дату заезда (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime checkIn))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }
            Console.WriteLine($"Дата заезда: {checkIn}"); // ОТЛАДКА

            Console.Write("Введите дату выезда (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime checkOut))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }
            Console.WriteLine($"Дата выезда: {checkOut}");

            if (checkIn >= checkOut)
            {
                ShowMessage("Дата выезда должна быть после даты заезда!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.WriteLine("Вызываем GetAvailableRooms..."); // ОТЛАДКА
            var rooms = service.GetAvailableRooms(checkIn, checkOut);
            Console.WriteLine("GetAvailableRooms завершен");

        

            if (rooms.Count == 0)
            {
                ShowMessage("Нет доступных номеров на выбранные даты.", ConsoleColor.Yellow);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"Найдено {rooms.Count} свободных номеров:");
                Console.WriteLine();

                foreach (var room in rooms)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Номер {room.RoomInfo}");
                    Console.ResetColor();
                    Console.WriteLine($"  Тип: {room.RoomType?.TypeName}");
                    Console.WriteLine($"  Вместимость: {room.RoomType?.MaxOccupancy} чел.");
                    Console.WriteLine($"  Удобства: {room.RoomType?.Amenities ?? "нет"}");
                    Console.WriteLine($"  Описание: {room.RoomType?.Description ?? "нет"}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ОШИБКА: {ex.Message}"); // ОТЛАДКА
            Console.WriteLine($"StackTrace: {ex.StackTrace}"); // ОТЛАДКАа
        }

        WaitForKey();
    }

    static void BookRoom()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Бронирование номера ===");
        Console.WriteLine();

        try
        {
            // Get customer info
            var customer = GetOrCreateCustomer();
            if (customer == null) return;

            // Get dates
            Console.Write("Введите дату заезда (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime checkIn))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.Write("Введите дату выезда (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime checkOut))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            if (checkIn >= checkOut)
            {
                ShowMessage("Дата выезда должна быть после даты заезда!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            // Show available rooms
            var rooms = service.GetAvailableRooms(checkIn, checkOut);
            if (rooms.Count == 0)
            {
                ShowMessage("Нет доступных номеров на выбранные даты.", ConsoleColor.Yellow);
                WaitForKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Доступные номера:");
            for (int i = 0; i < rooms.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {rooms[i].RoomInfo}");
            }

            Console.Write($"\nВыберите номер (1-{rooms.Count}): ");
            if (!int.TryParse(Console.ReadLine(), out int roomChoice) || roomChoice < 1 || roomChoice > rooms.Count)
            {
                ShowMessage("Неверный выбор номера!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var selectedRoom = rooms[roomChoice - 1];

            // Get employee
            var employees = service.GetActiveEmployees();
            if (employees.Count == 0)
            {
                ShowMessage("Нет активных сотрудников!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.WriteLine("\nСотрудники:");
            for (int i = 0; i < employees.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {employees[i].FullName} - {employees[i].Position}");
            }

            Console.Write($"\nВыберите сотрудника (1-{employees.Count}): ");
            if (!int.TryParse(Console.ReadLine(), out int empChoice) || empChoice < 1 || empChoice > employees.Count)
            {
                ShowMessage("Неверный выбор сотрудника!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var selectedEmployee = employees[empChoice - 1];

            // Special requests
            Console.Write("\nОсобые пожелания (необязательно): ");
            string? specialRequests = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(specialRequests))
                specialRequests = null;

            // Confirm booking
            Console.WriteLine($"\nПодтвердите бронирование:");
            Console.WriteLine($"Клиент: {customer.FullName}");
            Console.WriteLine($"Номер: {selectedRoom.RoomInfo}");
            Console.WriteLine($"Даты: {checkIn:dd.MM.yyyy} - {checkOut:dd.MM.yyyy}");
            Console.WriteLine($"Количество ночей: {(checkOut - checkIn).Days}");
            Console.WriteLine($"Сотрудник: {selectedEmployee.FullName}");
            Console.Write("\nПодтвердить бронирование? (y/n): ");

            if (Console.ReadLine()?.ToLower() != "y")
            {
                ShowMessage("Бронирование отменено.", ConsoleColor.Yellow);
                WaitForKey();
                return;
            }

            // Create reservation
            bool success = service.CreateReservation(
                customer.CustomerId,
                selectedRoom.RoomId,
                selectedEmployee.EmployeeId,
                checkIn, checkOut,
                specialRequests
            );

            if (success)
            {
                ShowMessage("Бронирование успешно создано!", ConsoleColor.Green);
            }
            else
            {
                ShowMessage("Ошибка при создании бронирования!", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void CheckIn()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Заселение ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите ID бронирования: ");
            if (!int.TryParse(Console.ReadLine(), out int reservationId))
            {
                ShowMessage("Неверный ID бронирования!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var reservation = service.GetReservationById(reservationId);
            if (reservation == null)
            {
                ShowMessage("Бронирование не найдено!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            if (reservation.Status != "confirmed")
            {
                ShowMessage($"Невозможно заселить. Статус бронирования: {reservation.Status}", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.WriteLine($"\nИнформация о бронировании:");
            Console.WriteLine($"Клиент: {reservation.Customer?.FullName}");
            Console.WriteLine($"Номер: {reservation.Room?.RoomNumber}");
            Console.WriteLine($"Даты: {reservation.CheckInDate:dd.MM.yyyy} - {reservation.CheckOutDate:dd.MM.yyyy}");
            Console.WriteLine($"Сумма: {reservation.TotalAmount:F2} руб.");
            Console.Write("\nПодтвердить заселение? (y/n): ");

            if (Console.ReadLine()?.ToLower() != "y")
            {
                ShowMessage("Заселение отменено.", ConsoleColor.Yellow);
                WaitForKey();
                return;
            }

            bool success = service.CheckInReservation(reservationId);
            if (success)
            {
                ShowMessage("Заселение успешно выполнено!", ConsoleColor.Green);
            }
            else
            {
                ShowMessage("Ошибка при заселении!", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void CheckOut()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Выселение ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите ID бронирования: ");
            if (!int.TryParse(Console.ReadLine(), out int reservationId))
            {
                ShowMessage("Неверный ID бронирования!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var reservation = service.GetReservationById(reservationId);
            if (reservation == null)
            {
                ShowMessage("Бронирование не найдено!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            if (reservation.Status != "checked-in")
            {
                ShowMessage($"Невозможно выселить. Статус бронирования: {reservation.Status}", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.WriteLine($"\nИнформация о бронировании:");
            Console.WriteLine($"Клиент: {reservation.Customer?.FullName}");
            Console.WriteLine($"Номер: {reservation.Room?.RoomNumber}");
            Console.WriteLine($"Даты: {reservation.CheckInDate:dd.MM.yyyy} - {reservation.CheckOutDate:dd.MM.yyyy}");
            Console.WriteLine($"Сумма: {reservation.TotalAmount:F2} руб.");
            Console.Write("\nПодтвердить выселение? (y/n): ");

            if (Console.ReadLine()?.ToLower() != "y")
            {
                ShowMessage("Выселение отменено.", ConsoleColor.Yellow);
                WaitForKey();
                return;
            }

            bool success = service.CheckOutReservation(reservationId);
            if (success)
            {
                ShowMessage("Выселение успешно выполнено!", ConsoleColor.Green);
            }
            else
            {
                ShowMessage("Ошибка при выселении!", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void ManageCustomers()
    {
        bool back = false;

        while (!back)
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine("=== Управление клиентами ===");
            Console.WriteLine();
            Console.WriteLine("1. Поиск клиента по email");
            Console.WriteLine("2. Создать нового клиента");
            Console.WriteLine("3. Обновить данные клиента");
            Console.WriteLine("0. Назад");
            Console.Write("\nВыберите действие: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SearchCustomer();
                    break;
                case "2":
                    CreateCustomer();
                    break;
                case "3":
                    UpdateCustomer();
                    break;
                case "0":
                    back = true;
                    break;
                default:
                    ShowMessage("Неверный выбор!", ConsoleColor.Red);
                    WaitForKey();
                    break;
            }
        }
    }

    static void SearchCustomer()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Поиск клиента ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите email клиента: ");
            string email = Console.ReadLine()?.Trim() ?? "";

            if (!InputValidator.IsValidEmail(email))
            {
                ShowMessage("Неверный формат email!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var customer = service.GetCustomerByEmail(email);
            if (customer == null)
            {
                ShowMessage("Клиент не найден!", ConsoleColor.Yellow);
            }
            else
            {
                Console.WriteLine($"\nНайден клиент:");
                Console.WriteLine($"ID: {customer.CustomerId}");
                Console.WriteLine($"ФИО: {customer.FullName}");
                Console.WriteLine($"Email: {customer.Email}");
                Console.WriteLine($"Телефон: {customer.Phone ?? "не указан"}");
                Console.WriteLine($"Паспорт: {customer.PassportNumber}");
                Console.WriteLine($"Дата рождения: {customer.DateOfBirth?.ToString("dd.MM.yyyy") ?? "не указана"}");
                Console.WriteLine($"Страна: {customer.Country ?? "не указана"}");
                Console.WriteLine($"Зарегистрирован: {customer.CreatedAt:dd.MM.yyyy}");

                // Show reservations
                var reservations = service.GetReservationsByCustomer(customer.CustomerId);
                if (reservations.Count > 0)
                {
                    Console.WriteLine($"\nБронирования ({reservations.Count}):");
                    foreach (var res in reservations)
                    {
                        Console.WriteLine($"  #{res.ReservationId}: {res.Room?.RoomNumber} - " +
                                        $"{res.CheckInDate:dd.MM.yy} - {res.CheckOutDate:dd.MM.yy} - " +
                                        $"{res.TotalAmount:F2} руб. - {res.Status}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void CreateCustomer()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Создание клиента ===");
        Console.WriteLine();

        try
        {
            var customer = GetCustomerInfoFromUser();
            if (customer == null) return;

            var customerId = service.CreateCustomer(customer);
            if (customerId.HasValue)
            {
                ShowMessage($"Клиент успешно создан! ID: {customerId.Value}", ConsoleColor.Green);
            }
            else
            {
                ShowMessage("Ошибка при создании клиента!", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void UpdateCustomer()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Обновление данных клиента ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите email клиента для обновления: ");
            string email = Console.ReadLine()?.Trim() ?? "";

            var existingCustomer = service.GetCustomerByEmail(email);
            if (existingCustomer == null)
            {
                ShowMessage("Клиент не найден!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.WriteLine($"\nТекущие данные клиента:");
            Console.WriteLine($"ФИО: {existingCustomer.FullName}");
            Console.WriteLine($"Телефон: {existingCustomer.Phone ?? "не указан"}");
            Console.WriteLine($"Паспорт: {existingCustomer.PassportNumber}");
            Console.WriteLine($"Дата рождения: {existingCustomer.DateOfBirth?.ToString("dd.MM.yyyy") ?? "не указана"}");
            Console.WriteLine($"Страна: {existingCustomer.Country ?? "не указана"}");

            Console.WriteLine($"\nВведите новые данные:");

            var updatedCustomer = GetCustomerInfoFromUser();
            if (updatedCustomer == null) return;

            updatedCustomer.CustomerId = existingCustomer.CustomerId;

            bool success = service.UpdateCustomer(updatedCustomer);
            if (success)
            {
                ShowMessage("Данные клиента успешно обновлены!", ConsoleColor.Green);
            }
            else
            {
                ShowMessage("Ошибка при обновлении данных клиента!", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static Customer? GetCustomerInfoFromUser()
    {
        var customer = new Customer();

        Console.Write("Имя: ");
        customer.FirstName = Console.ReadLine()?.Trim() ?? "";
        if (!InputValidator.IsValidName(customer.FirstName))
        {
            ShowMessage("Неверное имя!", ConsoleColor.Red);
            return null;
        }

        Console.Write("Фамилия: ");
        customer.LastName = Console.ReadLine()?.Trim() ?? "";
        if (!InputValidator.IsValidName(customer.LastName))
        {
            ShowMessage("Неверная фамилия!", ConsoleColor.Red);
            return null;
        }

        Console.Write("Email: ");
        customer.Email = Console.ReadLine()?.Trim() ?? "";
        if (!InputValidator.IsValidEmail(customer.Email))
        {
            ShowMessage("Неверный формат email!", ConsoleColor.Red);
            return null;
        }

        Console.Write("Телефон (необязательно): ");
        string phone = Console.ReadLine()?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(phone) && !InputValidator.IsValidPhone(phone))
        {
            ShowMessage("Неверный формат телефона!", ConsoleColor.Red);
            return null;
        }
        customer.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

        Console.Write("Номер паспорта: ");
        customer.PassportNumber = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(customer.PassportNumber))
        {
            ShowMessage("Номер паспорта обязателен!", ConsoleColor.Red);
            return null;
        }

        Console.Write("Дата рождения (дд.мм.гггг, необязательно): ");
        string dobInput = Console.ReadLine()?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(dobInput))
        {
            if (!InputValidator.TryParseDate(dobInput, out DateTime dob))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                return null;
            }
            customer.DateOfBirth = dob;
        }

        Console.Write("Страна (необязательно): ");
        string country = Console.ReadLine()?.Trim() ?? "";
        customer.Country = string.IsNullOrWhiteSpace(country) ? null : country;

        return customer;
    }

    static Customer? GetOrCreateCustomer()
    {
        Console.Write("Введите email клиента: ");
        string email = Console.ReadLine()?.Trim() ?? "";

        if (!InputValidator.IsValidEmail(email))
        {
            ShowMessage("Неверный формат email!", ConsoleColor.Red);
            return null;
        }

        // Try to find existing customer
        var customer = service.GetCustomerByEmail(email);
        if (customer != null)
        {
            Console.WriteLine($"Найден существующий клиент: {customer.FullName}");
            Console.Write("Использовать этого клиента? (y/n): ");

            if (Console.ReadLine()?.ToLower() == "y")
            {
                return customer;
            }
        }

        // Create new customer
        Console.WriteLine("\nСоздание нового клиента:");
        return GetCustomerInfoFromUser();
    }

    static void ViewReservations()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Просмотр бронирований ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите email клиента: ");
            string email = Console.ReadLine()?.Trim() ?? "";

            if (!InputValidator.IsValidEmail(email))
            {
                ShowMessage("Неверный формат email!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            var customer = service.GetCustomerByEmail(email);
            if (customer == null)
            {
                ShowMessage("Клиент не найден!", ConsoleColor.Yellow);
                WaitForKey();
                return;
            }

            var reservations = service.GetReservationsByCustomer(customer.CustomerId);

            if (reservations.Count == 0)
            {
                ShowMessage("У клиента нет бронирований.", ConsoleColor.Yellow);
            }
            else
            {
                Console.WriteLine($"\nБронирования клиента {customer.FullName}:");
                Console.WriteLine();

                foreach (var res in reservations)
                {
                    Console.ForegroundColor = GetStatusColor(res.Status);
                    Console.WriteLine($"Бронирование #{res.ReservationId}");
                    Console.ResetColor();
                    Console.WriteLine($"  Номер: {res.Room?.RoomNumber}");
                    Console.WriteLine($"  Даты: {res.CheckInDate:dd.MM.yyyy} - {res.CheckOutDate:dd.MM.yyyy}");
                    Console.WriteLine($"  Ночей: {res.Nights}");
                    Console.WriteLine($"  Сумма: {res.TotalAmount:F2} руб.");
                    Console.WriteLine($"  Статус: {res.Status}");
                    Console.WriteLine($"  Создано: {res.CreatedAt:dd.MM.yyyy HH:mm}");
                    if (!string.IsNullOrEmpty(res.SpecialRequests))
                        Console.WriteLine($"  Пожелания: {res.SpecialRequests}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void ShowReports()
    {
        bool back = false;

        while (!back)
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine("=== Отчеты ===");
            Console.WriteLine();
            Console.WriteLine("1. Выручка за период");
            Console.WriteLine("2. Загрузка номеров");
            Console.WriteLine("3. Все номера");
            Console.WriteLine("0. Назад");
            Console.Write("\nВыберите отчет: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowRevenueReport();
                    break;
                case "2":
                    ShowOccupancyReport();
                    break;
                case "3":
                    ShowAllRooms();
                    break;
                case "0":
                    back = true;
                    break;
                default:
                    ShowMessage("Неверный выбор!", ConsoleColor.Red);
                    WaitForKey();
                    break;
            }
        }
    }

    static void ShowRevenueReport()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Отчет по выручке ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите начальную дату (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime startDate))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            Console.Write("Введите конечную дату (дд.мм.гггг): ");
            if (!InputValidator.TryParseDate(Console.ReadLine(), out DateTime endDate))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            if (startDate > endDate)
            {
                ShowMessage("Начальная дата не может быть позже конечной!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            decimal revenue = service.GetTotalRevenue(startDate, endDate);

            Console.WriteLine($"\nВыручка за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{revenue:F2} руб.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void ShowOccupancyReport()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Загрузка номеров ===");
        Console.WriteLine();

        try
        {
            Console.Write("Введите дату для проверки (дд.мм.гггг, по умолчанию сегодня): ");
            string dateInput = Console.ReadLine()?.Trim() ?? "";

            DateTime date;
            if (string.IsNullOrWhiteSpace(dateInput))
            {
                date = DateTime.Today;
            }
            else if (!InputValidator.TryParseDate(dateInput, out date))
            {
                ShowMessage("Неверный формат даты!", ConsoleColor.Red);
                WaitForKey();
                return;
            }

            int occupancyRate = service.GetOccupancyRate(date);

            Console.WriteLine($"\nЗагрузка номеров на {date:dd.MM.yyyy}:");
            Console.Write("Загруженность: ");

            if (occupancyRate < 30)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (occupancyRate < 70)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{occupancyRate}%");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    static void ShowAllRooms()
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine("=== Все номера ===");
        Console.WriteLine();

        try
        {
            var rooms = service.GetAllRooms();

            if (rooms.Count == 0)
            {
                ShowMessage("Нет номеров в базе данных.", ConsoleColor.Yellow);
            }
            else
            {
                Console.WriteLine($"Всего номеров: {rooms.Count}");
                Console.WriteLine();

                foreach (var room in rooms)
                {
                    Console.Write($"Номер {room.RoomNumber} ({room.Floor} этаж): ");
                    Console.ForegroundColor = GetRoomStatusColor(room.Status);
                    Console.Write($"{room.Status}");
                    Console.ResetColor();
                    Console.WriteLine($" - {room.RoomType?.TypeName} - {room.RoomType?.BasePrice:F2} руб./ночь");
                    Console.WriteLine($"  Вместимость: {room.RoomType?.MaxOccupancy} чел.");
                    Console.WriteLine($"  Удобства: {room.RoomType?.Amenities ?? "нет"}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", ConsoleColor.Red);
        }

        WaitForKey();
    }

    #region Helper Methods

    static void ShowMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    static void WaitForKey()
    {
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    static ConsoleColor GetStatusColor(string status)
    {
        return status.ToLower() switch
        {
            "confirmed" => ConsoleColor.Blue,
            "checked-in" => ConsoleColor.Green,
            "checked-out" => ConsoleColor.Gray,
            "cancelled" => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }

    static ConsoleColor GetRoomStatusColor(string status)
    {
        return status.ToLower() switch
        {
            "available" => ConsoleColor.Green,
            "occupied" => ConsoleColor.Red,
            "maintenance" => ConsoleColor.Yellow,
            _ => ConsoleColor.White
        };
    }

    #endregion
}