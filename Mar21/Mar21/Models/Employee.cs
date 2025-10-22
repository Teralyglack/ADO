using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Должность обязательна")]
        public string Position { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string? Email { get; set; }

        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}";
    }
}
