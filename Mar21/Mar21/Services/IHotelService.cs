using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IHotelService
    {
        // Room operations
        List<Room> GetAvailableRooms(DateTime checkIn, DateTime checkOut);
        List<Room> GetAllRooms();
        Room? GetRoomById(int roomId);

        // Customer operations
        Customer? GetCustomerByEmail(string email);
        Customer? GetCustomerById(int customerId);
        int? CreateCustomer(Customer customer);
        bool UpdateCustomer(Customer customer);

        // Reservation operations
        bool CreateReservation(int customerId, int roomId, int employeeId,
                              DateTime checkIn, DateTime checkOut, string? specialRequests = null);
        bool CheckInReservation(int reservationId);
        bool CheckOutReservation(int reservationId);
        bool CancelReservation(int reservationId);
        List<Reservation> GetReservationsByCustomer(int customerId);
        Reservation? GetReservationById(int reservationId);

        // Employee operations
        List<Employee> GetActiveEmployees();
        Employee? GetEmployeeById(int employeeId);

        // Reporting
        decimal GetTotalRevenue(DateTime startDate, DateTime endDate);
        int GetOccupancyRate(DateTime date);
    }
}