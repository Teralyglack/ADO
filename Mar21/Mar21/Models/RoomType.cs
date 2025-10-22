using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class RoomType
    {
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Название типа обязательно")]
        [StringLength(50, ErrorMessage = "Название типа не может превышать 50 символов")]
        public string TypeName { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        public decimal BasePrice { get; set; }

        [Range(1, 10, ErrorMessage = "Вместимость должна быть от 1 до 10 человек")]
        public int MaxOccupancy { get; set; }

        public string? Amenities { get; set; }
        public string? Description { get; set; }
    }
}