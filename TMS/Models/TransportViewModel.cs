
namespace TMS.Models
{
    public class TransportViewModel
    {
        public DateOnly Date { get; set; }

        public string Search { get; set; } = string.Empty;

        public int? ShiftId { get; set; }

        public IReadOnlyList<VehicleModel> Vehicles { get; set; } = [];

        public IReadOnlyList<RouteModel> Routes { get; set; } = [];

        public IReadOnlyList<ShiftModel> Shifts { get; set; } = [];

        public IReadOnlyList<BookingModel> Bookings { get; set; } = [];

        public BookingModel Form { get; set; } = new BookingModel();
    }
}
