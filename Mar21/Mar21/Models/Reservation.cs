using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; }
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Дата заезда обязательна")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Дата выезда обязательна")]
        public DateTime CheckOutDate { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "confirmed"; // confirmed, checked-in, checked-out, cancelled
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Customer? Customer { get; set; }
        public Room? Room { get; set; }
        public Employee? Employee { get; set; }

        public int Nights => (CheckOutDate - CheckInDate).Days;
        public bool IsActive => Status == "confirmed" || Status == "checked-in";
    }
}