using TMS.Models;

namespace TMS.Services
{
    // Fictional reference data for the prototype; no supplier or database integration.
    internal static class DemoTransportData
    {
        public static IReadOnlyList<VehicleModel> Vehicles { get; } =
            new List<VehicleModel>
            {
                new VehicleModel
                {
                    Id = 1,
                    Name = "Fleet Bus 01",
                    Plate = "30-1842",
                    Type = "Bus",
                    Capacity = 40,
                    Driver = "Somchai Prasert",
                },
                new VehicleModel
                {
                    Id = 2,
                    Name = "Fleet Bus 02",
                    Plate = "30-1965",
                    Type = "Bus",
                    Capacity = 40,
                    Driver = "Anan Suksai",
                },
                new VehicleModel
                {
                    Id = 3,
                    Name = "Fleet Van 01",
                    Plate = "นข 4821",
                    Type = "Van",
                    Capacity = 12,
                    Driver = "Wichai Sombat",
                },
                new VehicleModel
                {
                    Id = 4,
                    Name = "Fleet Van 02",
                    Plate = "นข 5917",
                    Type = "Van",
                    Capacity = 12,
                    Driver = "Prasong Meesuk",
                },
                new VehicleModel
                {
                    Id = 5,
                    Name = "Fleet Van 03",
                    Plate = "นข 6204",
                    Type = "Van",
                    Capacity = 12,
                    Driver = "Narin Chai",
                },
            }.AsReadOnly();

        public static IReadOnlyList<RouteModel> Routes { get; } =
            new List<RouteModel>
            {
                new RouteModel
                {
                    Id = 1,
                    Name = "R01 · Bang Na",
                    Stops = ["BTS Bang Na", "Central Bang Na", "Main campus · Bang Phli"],
                    DistanceKm = 22,
                    Coordinates = [[13.668,100.605],[13.670,100.634],[13.610,100.720]],
                },
                new RouteModel
                {
                    Id = 2,
                    Name = "R02 · On Nut",
                    Stops = ["BTS On Nut", "BTS Udom Suk", "Main campus · Bang Phli"],
                    DistanceKm = 28,
                    Coordinates = [[13.706,100.601],[13.680,100.610],[13.610,100.720]],
                },
                new RouteModel
                {
                    Id = 3,
                    Name = "R03 · Pak Nam",
                    Stops = ["BTS Pak Nam", "BTS Srinagarindra", "Main campus · Bang Phli"],
                    DistanceKm = 20,
                    Coordinates = [[13.603,100.597],[13.592,100.609],[13.610,100.720]],
                },
                new RouteModel
                {
                    Id = 4,
                    Name = "R04 · Bang Phli",
                    Stops = ["Mega Bangna", "Bang Phli Market", "Main campus · Bang Phli"],
                    DistanceKm = 16,
                    Coordinates = [[13.646,100.681],[13.605,100.707],[13.610,100.720]],
                },
            }.AsReadOnly();

        public static IReadOnlyList<ShiftModel> Shifts { get; } =
            new List<ShiftModel>
            {
                new ShiftModel
                {
                    Id = 1,
                    Name = "Day shift",
                    Departure = "06:30",
                    WorkHours = "08:00 – 17:00",
                },
                new ShiftModel
                {
                    Id = 2,
                    Name = "Night shift",
                    Departure = "18:30",
                    WorkHours = "20:00 – 05:00",
                },
                new ShiftModel
                {
                    Id = 3,
                    Name = "Overtime",
                    Departure = "20:00",
                    WorkHours = "17:00 – 20:30",
                },
            }.AsReadOnly();

        public static List<BookingModel> CreateBookings(DateOnly travelDate)
        {
            string[] employeeNames =
            [
                "Nattapong Srisuk",
                "Siriporn Wongchai",
                "Thanawat Kittipong",
                "Patcharin Saetang",
                "Kritsada Boonmee",
                "Waraporn Jaidee"
            ];
            string[] departments = ["Production", "Quality assurance", "Engineering"];
            var bookings = new List<BookingModel>();

            for (int employeeNumber = 1; employeeNumber <= 28; employeeNumber++)
            {
                int vehicleId = employeeNumber % Vehicles.Count + 1;
                int routeId = (vehicleId - 1) % Routes.Count + 1;
                RouteModel route = Routes.First(item => item.Id == routeId);

                bookings.Add(new BookingModel
                {
                    Id = employeeNumber,
                    EmployeeId = $"EMP{employeeNumber:0000}",
                    EmployeeName = employeeNames[(employeeNumber - 1) % employeeNames.Length],
                    Department = departments[employeeNumber % departments.Length],
                    VehicleId = vehicleId,
                    RouteId = routeId,
                    ShiftId = employeeNumber % Shifts.Count + 1,
                    TravelDate = travelDate,
                    PickupPoint = route.Stops[0],
                    Status = employeeNumber % 9 == 0 ? BookingStatus.Cancelled : BookingStatus.Confirmed
                });
            }

            return bookings;
        }
    }
}
