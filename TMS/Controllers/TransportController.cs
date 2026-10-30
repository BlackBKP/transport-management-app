using Microsoft.AspNetCore.Mvc;
using TMS.Interfaces;
using TMS.Models;

namespace TMS.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class TransportController : Controller
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }

        public IActionResult Index(DateOnly? date, string? search, int? shiftId)
        {
            return View(_transportService.GetTransportPage(date, search, shiftId));
        }

        public IActionResult Bookings(DateOnly? date, string? search, int? shiftId)
        {
            return View(_transportService.GetTransportPage(date, search, shiftId));
        }

        public IActionResult Vehicles()
        {
            return View(_transportService.GetTransportPage());
        }

        public IActionResult Routes()
        {
            return View(_transportService.GetTransportPage());
        }

        public IActionResult Map()
        {
            return View(_transportService.GetTransportPage());
        }

        public IActionResult Shifts()
        {
            return View(_transportService.GetTransportPage());
        }

        public IActionResult Create()
        {
            return View(_transportService.GetTransportPage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookingModel booking)
        {
            if (ModelState.IsValid)
            {
                string? validationError = _transportService.AddBooking(booking);

                if (validationError == null)
                {
                    TempData["Message"] = "Booking confirmed. Your transport is reserved.";

                    return RedirectToAction(nameof(Bookings), new
                    {
                        date = booking.TravelDate.ToString("yyyy-MM-dd")
                    });
                }

                ModelState.AddModelError(string.Empty, validationError);
            }

            TransportViewModel viewModel = _transportService.GetTransportPage();
            viewModel.Form = booking;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id, DateOnly? date)
        {
            bool cancelled = _transportService.CancelBooking(id);

            TempData["Message"] = cancelled
                ? "Booking cancelled. The seat is available again."
                : "Booking was not found or is already cancelled.";

            return RedirectToAction(nameof(Bookings), new
            {
                date = date?.ToString("yyyy-MM-dd")
            });
        }
    }
}

