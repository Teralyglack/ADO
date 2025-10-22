using System.Globalization;

namespace HotelManagementSystem.Validators
{
    public static class InputValidator
    {
        public static bool TryParseDate(string input, out DateTime date)
        {

            date = DateTime.MinValue;

            // Проверка на null или пустую строку
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return DateTime.TryParseExact(input, "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            // Simple phone validation - can be enhanced
            return phone.Length >= 10 && phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')');
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Trim();
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Length >= 2 && name.All(c => char.IsLetter(c) || c == '-' || c == ' ');
        }
    }
}