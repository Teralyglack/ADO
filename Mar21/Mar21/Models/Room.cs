using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Номер комнаты обязателен")]
        [StringLength(10, ErrorMessage = "Номер комнаты не может превышать 10 символов")]
        public string RoomNumber { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Этаж должен быть от 1 до 100")]
        public int? Floor { get; set; }

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } = "available"; // available, occupied, maintenance

        public RoomType? RoomType { get; set; }

        public string RoomInfo => $"№{RoomNumber} ({Floor} этаж) - {RoomType?.TypeName} - {RoomType?.BasePrice:F2} руб./ночь";
    }
}