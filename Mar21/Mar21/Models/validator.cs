public static class Validator
{
    public static bool ValidateDates(DateTime checkIn, DateTime checkOut, out string error)
    {
        error = string.Empty;

        if (checkIn < DateTime.Today)
        {
            error = "���� ������ �� ����� ���� � �������";
            return false;
        }

        if (checkOut <= checkIn)
        {
            error = "���� ������ ������ ���� ����� ���� ������";
            return false;
        }

        if ((checkOut - checkIn).Days > 30)
        {
            error = "������������ ���� ���������� - 30 ����";
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