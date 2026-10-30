using TMS.Models;

namespace TMS.Interfaces
{
    public interface ITransportService
    {
        TransportViewModel GetTransportPage(DateOnly? date = null, string? search = null, int? shiftId = null);

        string? AddBooking(BookingModel booking);

        bool CancelBooking(int bookingId);
    }
}
