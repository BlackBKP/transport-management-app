using System.Text.Json;
using TMS.Interfaces;
using TMS.Models;

namespace TMS.Services
{
    public class TransportService : ITransportService
    {
        private readonly object _bookingLock = new object();
        private readonly string _bookingFilePath;
        private readonly List<BookingModel> _bookings;
        private static readonly JsonSerializerOptions FileJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static DateOnly Today
        {
            get { return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); }
        }

        public TransportService(ApplicationStorage storage)
        {
            string dataDirectory = storage.DataDirectory;
            Directory.CreateDirectory(dataDirectory);

            _bookingFilePath = Path.Combine(dataDirectory, "bookings.json");
            _bookings = LoadBookings();
        }

        public TransportViewModel GetTransportPage(
            DateOnly? date = null,
            string? search = null,
            int? shiftId = null)
        {
            DateOnly travelDate = date ?? Today;

            lock (_bookingLock)
            {
                List<BookingModel> filteredBookings = _bookings
                    .Where(booking => booking.TravelDate == travelDate)
                    .Where(booking => !shiftId.HasValue || booking.ShiftId == shiftId.Value)
                    .Where(booking => MatchesEmployeeSearch(booking, search))
                    .OrderByDescending(booking => booking.Id)
                    .Select(CopyBooking)
                    .ToList();

                return new TransportViewModel
                {
                    Date = travelDate,
                    Search = search ?? string.Empty,
                    ShiftId = shiftId,
                    Vehicles = DemoTransportData.Vehicles,
                    Routes = DemoTransportData.Routes,
                    Shifts = DemoTransportData.Shifts,
                    Bookings = filteredBookings
                };
            }
        }

        public string? AddBooking(BookingModel booking)
        {
            // Validate and reserve under the same lock to avoid double booking a seat.
            lock (_bookingLock)
            {
                string? validationError = ValidateBooking(booking);

                if (validationError != null)
                {
                    return validationError;
                }

                booking.Id = _bookings.Select(item => item.Id).DefaultIfEmpty().Max() + 1;
                booking.Status = BookingStatus.Confirmed;
                _bookings.Add(CopyBooking(booking));
                SaveBookings();

                return null;
            }
        }

        public bool CancelBooking(int bookingId)
        {
            lock (_bookingLock)
            {
                BookingModel? booking = _bookings.FirstOrDefault(item =>
                    item.Id == bookingId && item.Status == BookingStatus.Confirmed);

                if (booking == null)
                {
                    return false;
                }

                booking.Status = BookingStatus.Cancelled;
                SaveBookings();

                return true;
            }
        }

        private string? ValidateBooking(BookingModel booking)
        {
            VehicleModel? vehicle = DemoTransportData.Vehicles.FirstOrDefault(item => item.Id == booking.VehicleId);
            RouteModel? route = DemoTransportData.Routes.FirstOrDefault(item => item.Id == booking.RouteId);
            bool validShift = DemoTransportData.Shifts.Any(item => item.Id == booking.ShiftId);

            if (vehicle == null || route == null || !validShift)
            {
                return "Choose a valid vehicle, route and shift.";
            }

            if (booking.TravelDate < Today)
            {
                return "Travel date must be today or later.";
            }

            if (!route.Stops.Contains(booking.PickupPoint))
            {
                return "Choose a pickup point on the selected route.";
            }

            List<BookingModel> shiftBookings = _bookings
                .Where(item => item.Status == BookingStatus.Confirmed)
                .Where(item => item.TravelDate == booking.TravelDate && item.ShiftId == booking.ShiftId)
                .ToList();

            bool employeeAlreadyBooked = shiftBookings.Any(item =>
                item.EmployeeId.Equals(booking.EmployeeId, StringComparison.OrdinalIgnoreCase));

            if (employeeAlreadyBooked)
            {
                return "This employee already has a booking for this date and shift.";
            }

            bool vehicleAssignedToAnotherRoute = shiftBookings.Any(item =>
                item.VehicleId == booking.VehicleId && item.RouteId != booking.RouteId);

            if (vehicleAssignedToAnotherRoute)
            {
                return "This vehicle is assigned to another route for this shift. Choose another vehicle.";
            }

            int reservedSeats = shiftBookings.Count(item => item.VehicleId == booking.VehicleId);

            if (reservedSeats >= vehicle.Capacity)
            {
                return "This vehicle is fully booked for this shift.";
            }

            return null;
        }

        private List<BookingModel> LoadBookings()
        {
            if (!File.Exists(_bookingFilePath))
            {
                return DemoTransportData.CreateBookings(Today);
            }

            string bookingJson = File.ReadAllText(_bookingFilePath);
            return JsonSerializer.Deserialize<List<BookingModel>>(bookingJson) ?? new List<BookingModel>();
        }

        private void SaveBookings()
        {
            // Replace the destination only after the complete JSON file has been written.
            string temporaryFilePath = _bookingFilePath + ".tmp";
            string bookingJson = JsonSerializer.Serialize(_bookings, FileJsonOptions);

            File.WriteAllText(temporaryFilePath, bookingJson);
            File.Move(temporaryFilePath, _bookingFilePath, overwrite: true);
        }

        private static bool MatchesEmployeeSearch(BookingModel booking, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            string employeeDetails = $"{booking.EmployeeId} {booking.EmployeeName} {booking.Department}";
            return employeeDetails.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private static BookingModel CopyBooking(BookingModel booking)
        {
            return new BookingModel
            {
                Id = booking.Id,
                EmployeeId = booking.EmployeeId,
                EmployeeName = booking.EmployeeName,
                Department = booking.Department,
                VehicleId = booking.VehicleId,
                RouteId = booking.RouteId,
                ShiftId = booking.ShiftId,
                TravelDate = booking.TravelDate,
                PickupPoint = booking.PickupPoint,
                Notes = booking.Notes,
                Status = booking.Status
            };
        }
    }
}

