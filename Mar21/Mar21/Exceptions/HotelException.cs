namespace HotelManagementSystem.Exceptions
{
    public class HotelException : Exception
    {
        public HotelException(string message) : base(message) { }

        public HotelException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class RoomNotAvailableException : HotelException
    {
        public RoomNotAvailableException(string message) : base(message) { }
    }

    public class CustomerNotFoundException : HotelException
    {
        public CustomerNotFoundException(string message) : base(message) { }
    }

    public class ReservationNotFoundException : HotelException
    {
        public ReservationNotFoundException(string message) : base(message) { }
    }
}
