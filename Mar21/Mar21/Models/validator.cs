public static class Validator
{
    public static bool ValidateDates(DateTime checkIn, DateTime checkOut, out string error)
    {
        error = string.Empty;

        if (checkIn < DateTime.Today)
        {
            error = "ƒата заезда не может быть в прошлом";
            return false;
        }

        if (checkOut <= checkIn)
        {
            error = "ƒата выезда должна быть после даты заезда";
            return false;
        }

        if ((checkOut - checkIn).Days > 30)
        {
            error = "ћаксимальный срок проживани€ - 30 дней";
            return false;
        }

        return true;
    }

    public static bool ValidateEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
               email.Contains("@") &&
               email.Contains(".");
    }
}