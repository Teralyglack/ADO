using System.Data;
using System.Data.SqlClient;
using HotelManagementSystem.Models;
using HotelManagementSystem.Exceptions;

namespace HotelManagementSystem.Services
{
    public class HotelService : IHotelService
    {
        private readonly string _connectionString;

        public HotelService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public List<Room> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
        {
            ValidateDates(checkIn, checkOut);
            var rooms = new List<Room>();

            try
            {
                Console.WriteLine($"Подключаемся к БД: {_connectionString}"); // Отладка

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Подключение к БД установлено успешно"); // Отладка

                    var sql = @"
                SELECT r.room_id, r.hotel_id, r.type_id, r.room_number, r.floor, r.status,
                       rt.type_name, rt.base_price, rt.max_occupancy, rt.amenities, rt.description
                FROM rooms r
                JOIN room_types rt ON r.type_id = rt.type_id
                WHERE r.status = 'available'
                AND r.room_id NOT IN (
                    SELECT room_id FROM reservations
                    WHERE status IN ('confirmed', 'checked-in')
                    AND check_in_date < @CheckOut AND check_out_date > @CheckIn
                )
                ORDER BY rt.base_price, r.floor";

                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 30; // Таймаут 30 секунд
                        cmd.Parameters.AddWithValue("@CheckIn", checkIn);
                        cmd.Parameters.AddWithValue("@CheckOut", checkOut);

                        Console.WriteLine("Выполняем SQL запрос..."); // Отладка

                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("SQL запрос выполнен, читаем данные..."); // Отладка

                            int count = 0;
                            while (reader.Read())
                            {
                                var room = MapRoomFromReader(reader);
                                rooms.Add(room);
                                count++;
                            }
                            Console.WriteLine($"Прочитано {count} комнат"); // Отладка
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"ОШИБКА SQL: {sqlEx.Message}");
                Console.WriteLine($"Номер ошибки: {sqlEx.Number}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОБЩАЯ ОШИБКА: {ex.Message}");
                throw;
            }

            return rooms;
        }

        //public List<Room> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
        //{
        //    ValidateDates(checkIn, checkOut);

        //    var rooms = new List<Room>();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();

        //        var sql = @"
        //            SELECT r.room_id, r.hotel_id, r.type_id, r.room_number, r.floor, r.status,
        //                   rt.type_name, rt.base_price, rt.max_occupancy, rt.amenities, rt.description
        //            FROM rooms r
        //            JOIN room_types rt ON r.type_id = rt.type_id
        //            WHERE r.status = 'available'
        //            AND r.room_id NOT IN (
        //                SELECT room_id FROM reservations
        //                WHERE status IN ('confirmed', 'checked-in')
        //                AND check_in_date < @CheckOut AND check_out_date > @CheckIn
        //            )
        //            ORDER BY rt.base_price, r.floor";

        //        using (var cmd = new SqlCommand(sql, connection))
        //        {
        //            cmd.Parameters.AddWithValue("@CheckIn", checkIn);
        //            cmd.Parameters.AddWithValue("@CheckOut", checkOut);

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var room = MapRoomFromReader(reader);
        //                    rooms.Add(room);
        //                }
        //            }
        //        }
        //    }
        //    return rooms;
        //}

        public List<Room> GetAllRooms()
        {
            var rooms = new List<Room>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT r.room_id, r.hotel_id, r.type_id, r.room_number, r.floor, r.status,
                           rt.type_name, rt.base_price, rt.max_occupancy, rt.amenities, rt.description
                    FROM rooms r
                    JOIN room_types rt ON r.type_id = rt.type_id
                    ORDER BY r.floor, r.room_number";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var room = MapRoomFromReader(reader);
                            rooms.Add(room);
                        }
                    }
                }
            }
            return rooms;
        }

        public Room? GetRoomById(int roomId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT r.room_id, r.hotel_id, r.type_id, r.room_number, r.floor, r.status,
                           rt.type_name, rt.base_price, rt.max_occupancy, rt.amenities, rt.description
                    FROM rooms r
                    JOIN room_types rt ON r.type_id = rt.type_id
                    WHERE r.room_id = @RoomId";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@RoomId", roomId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRoomFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Customer? GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT customer_id, first_name, last_name, email, phone, 
                           passport_number, date_of_birth, country, created_at
                    FROM customers 
                    WHERE email = @Email";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCustomerFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Customer? GetCustomerById(int customerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT customer_id, first_name, last_name, email, phone, 
                           passport_number, date_of_birth, country, created_at
                    FROM customers 
                    WHERE customer_id = @CustomerId";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCustomerFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public int? CreateCustomer(Customer customer)
        {
            ValidateCustomer(customer);

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Check if customer with same email or passport already exists
                    var checkSql = @"
                        SELECT COUNT(*) FROM customers 
                        WHERE email = @Email OR passport_number = @PassportNumber";

                    using (var checkCmd = new SqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", customer.Email.Trim().ToLower());
                        checkCmd.Parameters.AddWithValue("@PassportNumber", customer.PassportNumber.Trim());

                        var count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            throw new HotelException("Клиент с таким email или номером паспорта уже существует");
                        }
                    }

                    var insertSql = @"
                        INSERT INTO customers (first_name, last_name, email, phone, 
                                              passport_number, date_of_birth, country, created_at)
                        VALUES (@FirstName, @LastName, @Email, @Phone, 
                                @PassportNumber, @DateOfBirth, @Country, @CreatedAt);
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", customer.FirstName.Trim());
                        cmd.Parameters.AddWithValue("@LastName", customer.LastName.Trim());
                        cmd.Parameters.AddWithValue("@Email", customer.Email.Trim().ToLower());
                        cmd.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PassportNumber", customer.PassportNumber.Trim());
                        cmd.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        var result = cmd.ExecuteScalar();
                        return result == null ? null : Convert.ToInt32(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new HotelException("Ошибка при создании клиента в базе данных", ex);
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            ValidateCustomer(customer);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    UPDATE customers 
                    SET first_name = @FirstName, last_name = @LastName, 
                        email = @Email, phone = @Phone, passport_number = @PassportNumber,
                        date_of_birth = @DateOfBirth, country = @Country
                    WHERE customer_id = @CustomerId";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    cmd.Parameters.AddWithValue("@FirstName", customer.FirstName.Trim());
                    cmd.Parameters.AddWithValue("@LastName", customer.LastName.Trim());
                    cmd.Parameters.AddWithValue("@Email", customer.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PassportNumber", customer.PassportNumber.Trim());
                    cmd.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CreateReservation(int customerId, int roomId, int employeeId,
                                    DateTime checkIn, DateTime checkOut, string? specialRequests = null)
        {
            ValidateDates(checkIn, checkOut);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if room is available
                        if (!IsRoomAvailable(roomId, checkIn, checkOut, connection, transaction))
                        {
                            throw new RoomNotAvailableException("Номер недоступен на выбранные даты");
                        }

                        // Calculate total amount
                        decimal basePrice = GetRoomBasePrice(roomId, connection, transaction);
                        int nights = (checkOut - checkIn).Days;
                        if (nights <= 0) nights = 1;
                        decimal total = basePrice * nights;

                        // Insert reservation
                        var sql = @"
                            INSERT INTO reservations (customer_id, room_id, employee_id, 
                                                    check_in_date, check_out_date, total_amount, 
                                                    special_requests, status, created_at)
                            VALUES (@CustomerId, @RoomId, @EmployeeId, 
                                    @CheckIn, @CheckOut, @TotalAmount, 
                                    @SpecialRequests, 'confirmed', @CreatedAt)";

                        using (var cmd = new SqlCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CustomerId", customerId);
                            cmd.Parameters.AddWithValue("@RoomId", roomId);
                            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                            cmd.Parameters.AddWithValue("@CheckIn", checkIn);
                            cmd.Parameters.AddWithValue("@CheckOut", checkOut);
                            cmd.Parameters.AddWithValue("@TotalAmount", total);
                            cmd.Parameters.AddWithValue("@SpecialRequests", specialRequests ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CheckInReservation(int reservationId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update reservation status
                        var sql1 = @"
                            UPDATE reservations 
                            SET status = 'checked-in' 
                            WHERE reservation_id = @Id AND status = 'confirmed'";

                        using (var cmd1 = new SqlCommand(sql1, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@Id", reservationId);
                            if (cmd1.ExecuteNonQuery() == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // Get room_id from reservation
                        int roomId = 0;
                        using (var cmdGetRoom = new SqlCommand(
                            "SELECT room_id FROM reservations WHERE reservation_id = @Id",
                            connection, transaction))
                        {
                            cmdGetRoom.Parameters.AddWithValue("@Id", reservationId);
                            var result = cmdGetRoom.ExecuteScalar();
                            if (result == null)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            roomId = (int)result;
                        }

                        // Update room status
                        var sql2 = "UPDATE rooms SET status = 'occupied' WHERE room_id = @RoomId";
                        using (var cmd2 = new SqlCommand(sql2, connection, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@RoomId", roomId);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CheckOutReservation(int reservationId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update reservation status
                        var sql1 = @"
                            UPDATE reservations 
                            SET status = 'checked-out' 
                            WHERE reservation_id = @Id AND status = 'checked-in'";

                        using (var cmd1 = new SqlCommand(sql1, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@Id", reservationId);
                            if (cmd1.ExecuteNonQuery() == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // Get room_id from reservation
                        int roomId = 0;
                        using (var cmdGetRoom = new SqlCommand(
                            "SELECT room_id FROM reservations WHERE reservation_id = @Id",
                            connection, transaction))
                        {
                            cmdGetRoom.Parameters.AddWithValue("@Id", reservationId);
                            roomId = (int)cmdGetRoom.ExecuteScalar();
                        }

                        // Update room status to available
                        var sql2 = "UPDATE rooms SET status = 'available' WHERE room_id = @RoomId";
                        using (var cmd2 = new SqlCommand(sql2, connection, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@RoomId", roomId);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CancelReservation(int reservationId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    UPDATE reservations 
                    SET status = 'cancelled' 
                    WHERE reservation_id = @Id AND status = 'confirmed'";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", reservationId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Reservation> GetReservationsByCustomer(int customerId)
        {
            var reservations = new List<Reservation>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT r.*, c.first_name, c.last_name, c.email,
                           rm.room_number, rm.floor,
                           rt.type_name, rt.base_price
                    FROM reservations r
                    JOIN customers c ON r.customer_id = c.customer_id
                    JOIN rooms rm ON r.room_id = rm.room_id
                    JOIN room_types rt ON rm.type_id = rt.type_id
                    WHERE r.customer_id = @CustomerId
                    ORDER BY r.created_at DESC";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var reservation = MapReservationFromReader(reader);
                            reservations.Add(reservation);
                        }
                    }
                }
            }
            return reservations;
        }

        public Reservation? GetReservationById(int reservationId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT r.*, c.first_name, c.last_name, c.email,
                           rm.room_number, rm.floor,
                           rt.type_name, rt.base_price,
                           e.first_name as emp_first_name, e.last_name as emp_last_name
                    FROM reservations r
                    JOIN customers c ON r.customer_id = c.customer_id
                    JOIN rooms rm ON r.room_id = rm.room_id
                    JOIN room_types rt ON rm.type_id = rt.type_id
                    JOIN employees e ON r.employee_id = e.employee_id
                    WHERE r.reservation_id = @ReservationId";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ReservationId", reservationId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReservationFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<Employee> GetActiveEmployees()
        {
            var employees = new List<Employee>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT employee_id, first_name, last_name, position, email, phone
                    FROM employees 
                    WHERE is_active = 1
                    ORDER BY position, first_name";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new Employee
                            {
                                EmployeeId = (int)reader["employee_id"],
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                Position = reader["position"].ToString(),
                                Email = reader["email"]?.ToString(),
                                Phone = reader["phone"]?.ToString()
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }
            return employees;
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT employee_id, first_name, last_name, position, email, phone
                    FROM employees 
                    WHERE employee_id = @EmployeeId AND is_active = 1";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                EmployeeId = (int)reader["employee_id"],
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                Position = reader["position"].ToString(),
                                Email = reader["email"]?.ToString(),
                                Phone = reader["phone"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public decimal GetTotalRevenue(DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT COALESCE(SUM(total_amount), 0) as total_revenue
                    FROM reservations 
                    WHERE status IN ('checked-in', 'checked-out')
                    AND check_in_date >= @StartDate AND check_in_date <= @EndDate";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        public int GetOccupancyRate(DateTime date)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT 
                        COUNT(*) as occupied_rooms,
                        (SELECT COUNT(*) FROM rooms WHERE status != 'maintenance') as total_rooms
                    FROM reservations 
                    WHERE status IN ('confirmed', 'checked-in')
                    AND check_in_date <= @Date AND check_out_date > @Date";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int occupied = (int)reader["occupied_rooms"];
                            int total = (int)reader["total_rooms"];

                            return total > 0 ? (int)Math.Round((double)occupied / total * 100) : 0;
                        }
                    }
                }
            }
            return 0;
        }

        #region Private Helper Methods

        private Room MapRoomFromReader(SqlDataReader reader)
        {
            return new Room
            {
                RoomId = (int)reader["room_id"],
                HotelId = (int)reader["hotel_id"],
                TypeId = (int)reader["type_id"],
                RoomNumber = reader["room_number"].ToString(),
                Floor = reader["floor"] as int?,
                Status = reader["status"].ToString(),
                RoomType = new RoomType
                {
                    TypeId = (int)reader["type_id"],
                    TypeName = reader["type_name"].ToString(),
                    BasePrice = (decimal)reader["base_price"],
                    MaxOccupancy = (int)reader["max_occupancy"],
                    Amenities = reader["amenities"]?.ToString(),
                    Description = reader["description"]?.ToString()
                }
            };
        }

        private Customer MapCustomerFromReader(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerId = (int)reader["customer_id"],
                FirstName = reader["first_name"].ToString(),
                LastName = reader["last_name"].ToString(),
                Email = reader["email"].ToString(),
                Phone = reader["phone"]?.ToString(),
                PassportNumber = reader["passport_number"].ToString(),
                DateOfBirth = reader["date_of_birth"] as DateTime?,
                Country = reader["country"]?.ToString(),
                CreatedAt = (DateTime)reader["created_at"]
            };
        }

        private Reservation MapReservationFromReader(SqlDataReader reader)
        {
            return new Reservation
            {
                ReservationId = (int)reader["reservation_id"],
                CustomerId = (int)reader["customer_id"],
                RoomId = (int)reader["room_id"],
                EmployeeId = (int)reader["employee_id"],
                CheckInDate = (DateTime)reader["check_in_date"],
                CheckOutDate = (DateTime)reader["check_out_date"],
                TotalAmount = (decimal)reader["total_amount"],
                Status = reader["status"].ToString(),
                SpecialRequests = reader["special_requests"]?.ToString(),
                CreatedAt = (DateTime)reader["created_at"],
                Customer = new Customer
                {
                    CustomerId = (int)reader["customer_id"],
                    FirstName = reader["first_name"].ToString(),
                    LastName = reader["last_name"].ToString(),
                    Email = reader["email"].ToString()
                },
                Room = new Room
                {
                    RoomId = (int)reader["room_id"],
                    RoomNumber = reader["room_number"].ToString(),
                    Floor = reader["floor"] as int?,
                    RoomType = new RoomType
                    {
                        TypeName = reader["type_name"].ToString(),
                        BasePrice = (decimal)reader["base_price"]
                    }
                }
            };
        }

        private bool IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut,
                                   SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM reservations 
                WHERE room_id = @RoomId 
                AND status IN ('confirmed', 'checked-in')
                AND check_in_date < @CheckOut AND check_out_date > @CheckIn";

            using (var cmd = new SqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@RoomId", roomId);
                cmd.Parameters.AddWithValue("@CheckIn", checkIn);
                cmd.Parameters.AddWithValue("@CheckOut", checkOut);

                return (int)cmd.ExecuteScalar() == 0;
            }
        }

        private decimal GetRoomBasePrice(int roomId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"
                SELECT base_price 
                FROM room_types rt 
                JOIN rooms r ON rt.type_id = r.type_id 
                WHERE r.room_id = @RoomId";

            using (var cmd = new SqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@RoomId", roomId);
                var result = cmd.ExecuteScalar();

                if (result == null)
                    throw new HotelException("Номер не найден");

                return (decimal)result;
            }
        }

        private void ValidateDates(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn < DateTime.Today)
                throw new ArgumentException("Дата заезда не может быть в прошлом");

            if (checkOut <= checkIn)
                throw new ArgumentException("Дата выезда должна быть после даты заезда");
        }

        private void ValidateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (string.IsNullOrWhiteSpace(customer.FirstName))
                throw new ArgumentException("Имя обязательно");

            if (string.IsNullOrWhiteSpace(customer.LastName))
                throw new ArgumentException("Фамилия обязательна");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new ArgumentException("Email обязателен");

            if (string.IsNullOrWhiteSpace(customer.PassportNumber))
                throw new ArgumentException("Номер паспорта обязателен");
        }

        #endregion
    }
}