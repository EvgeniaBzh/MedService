using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedService.Models;
using MedService.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace MedService.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly DbMedServiceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _memoryCache;

        public DoctorsController(DbMedServiceContext context, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var doctors = await GetCachedDoctorsWithSpecializationsAsync();

            foreach (var doctor in doctors)
            {
                if (doctor.Specialization == null)
                {
                    doctor.Specialization = new Specialization();
                }
            }

            var specializations = _context.Specializations.ToList();
            ViewData["SpecializationId"] = new SelectList(specializations, "SpecializationId", "SpecializationName");
            return View(doctors);
        }


        private async Task<List<Doctor>> GetCachedDoctorsWithSpecializationsAsync()
        {
            string cacheKey = "doctors_with_specializations";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Doctor> doctors))
            {
                doctors = await _context.Doctors
                    .Include(d => d.Specialization)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(cacheKey, doctors, cacheOptions);
            }

            return doctors;
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .Include(d => d.DoctorAvailabilities)
                    .ThenInclude(da => da.Availability)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            var availableTimes = doctor.DoctorAvailabilities
                .Where(da => da.PatientId == null)
                .ToList();

            ViewBag.AvailableTimes = availableTimes;

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.PatientId = patientId;

            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(string doctorId, string patientId, string selectedAvailabilityId)
        {
            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(selectedAvailabilityId))
            {
                return BadRequest();
            }

            var availability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.AvailabilityId == selectedAvailabilityId);

            if (availability == null || availability.PatientId != null)
            {
                return NotFound();
            }

            availability.PatientId = patientId;
            _context.DoctorAvailabilities.Update(availability);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = doctorId });
        }


        // GET: Doctors/Create
        public IActionResult Create()
        {
            var specializations = _context.Specializations.ToList();
            ViewData["SpecializationId"] = new SelectList(specializations, "SpecializationId", "SpecializationName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorName,DoctorEmail,DoctorPassword,DoctorPhoto,SpecializationId")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorEmail == doctor.DoctorEmail);
                if (doctorExists)
                {
                    ModelState.AddModelError("DoctorEmail", "A doctor with this email already exists.");
                    ViewData["SpecializationId"] = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);
                    return View(doctor);
                }

                var user = new ApplicationUser { UserName = doctor.DoctorEmail, Email = doctor.DoctorEmail };
                var result = await _userManager.CreateAsync(user, doctor.DoctorPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "doctor");

                    doctor.DoctorId = user.Id;

                    _context.Add(doctor);
                    await _context.SaveChangesAsync();

                    _memoryCache.Remove("doctors_with_specializations");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.DoctorAvailabilities)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.SpecializationId = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);

            var availabilities = await _context.Availabilities.ToListAsync();
            if (availabilities == null || !availabilities.Any())
            {
                ModelState.AddModelError(string.Empty, "No available hours found.");
                return View(doctor);
            }

            ViewBag.Availabilities = availabilities;


            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("DoctorId,DoctorName,SpecializationId")] Doctor doctor, string[] AvailableHours)
        {
            if (id != doctor.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDoctor = await _context.Doctors
                        .Include(d => d.DoctorAvailabilities)
                        .FirstOrDefaultAsync(d => d.DoctorId == id);

                    if (existingDoctor == null)
                    {
                        return NotFound();
                    }

                    existingDoctor.DoctorName = doctor.DoctorName;
                    existingDoctor.SpecializationId = doctor.SpecializationId;

                    existingDoctor.DoctorAvailabilities.Clear();

                    foreach (var availabilityId in AvailableHours)
                    {
                        var doctorAvailability = new DoctorAvailability
                        {
                            DoctorId = existingDoctor.DoctorId,
                            AvailabilityId = availabilityId
                        };
                        existingDoctor.DoctorAvailabilities.Add(doctorAvailability);
                    }

                    _context.Update(existingDoctor);

                    _memoryCache.Remove("doctors_with_specializations");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();

            _memoryCache.Remove("doctors_with_specializations");

            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(string id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}