using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedService.Models;

namespace MedService.Controllers
{
    public class AvailabilitiesController : Controller
    {
        private readonly DbMedServiceContext _context;

        public AvailabilitiesController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: Availabilities
        public async Task<IActionResult> Index()
        {
            var availabilities = await _context.Availabilities
                .OrderBy(a => a.Day)   
                .ThenBy(a => a.Date)   
                .ToListAsync();

            return View(availabilities);
        }

        // GET: Availabilities/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (availability == null)
            {
                return NotFound();
            }

            return View(availability);
        }

        // GET: Availabilities/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DayOfWeek day, int startTime, int endTime)
        {
            if (startTime >= endTime)
            {
                ModelState.AddModelError(string.Empty, "Start time must be earlier than end time.");
                return View();
            }

            var availabilityList = new List<Availability>();

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(182);

            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == day)
                {
                    for (var time = startTime; time < endTime; time++)
                    {
                        var dateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, time, 0, 0);

                        var existingAvailability = await _context.Availabilities
                            .AnyAsync(a => a.Day == day && a.Date == dateTime);

                        if (!existingAvailability)
                        {
                            var availability = new Availability
                            {
                                AvailabilityId = Guid.NewGuid().ToString(),
                                Day = day,
                                Date = dateTime, // Use DateTime for both date and time
                                IsAvailable = true
                            };

                            availabilityList.Add(availability);
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            if (availabilityList.Count > 0)
            {
                _context.Availabilities.AddRange(availabilityList);
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "All selected time slots already exist.");
                return View();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Availabilities/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (availability == null)
            {
                return NotFound();
            }

            return View(availability);
        }

        // POST: Availabilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var availability = await _context.Availabilities.FindAsync(id);
            if (availability != null)
            {
                _context.Availabilities.Remove(availability);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AvailabilityExists(string id)
        {
            return _context.Availabilities.Any(e => e.AvailabilityId == id);
        }
    }
}
