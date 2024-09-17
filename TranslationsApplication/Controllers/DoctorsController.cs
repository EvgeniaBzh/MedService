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

namespace MedService.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly DbMedServiceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(DbMedServiceContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var dbMedServiceContext = _context.Doctors.Include(d => d.Specialization);

            // Завантажуємо дані
            var doctors = await dbMedServiceContext.ToListAsync();

            // Обробляємо можливі null значення
            foreach (var doctor in doctors)
            {
                if (doctor.Specialization == null)
                {
                    doctor.Specialization = new Specialization(); // або обробка іншого способу
                }
            }

            var specializations = _context.Specializations.ToList();
            ViewData["SpecializationId"] = new SelectList(specializations, "SpecializationId", "SpecializationName");
            return View(doctors);
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
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
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
                // Check if a doctor with the same email already exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorEmail == doctor.DoctorEmail);
                if (doctorExists)
                {
                    ModelState.AddModelError("DoctorEmail", "A doctor with this email already exists.");
                    ViewData["SpecializationId"] = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);
                    return View(doctor);
                }

                // Створення нового користувача в системі Identity
                var user = new ApplicationUser { UserName = doctor.DoctorEmail, Email = doctor.DoctorEmail };
                var result = await _userManager.CreateAsync(user, doctor.DoctorPassword);

                if (result.Succeeded)
                {
                    // Призначення ролі "doctor"
                    await _userManager.AddToRoleAsync(user, "doctor");

                    // Присвоєння DoctorId користувачу
                    doctor.DoctorId = user.Id;

                    // Додавання лікаря до бази даних
                    _context.Add(doctor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Виведення помилок створення користувача
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
                .Include(d => d.DoctorAvailabilities)  // Include DoctorAvailabilities to preselect the checkboxes
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            // Отримати список спеціалізацій і доступних годин
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "SpecializationId", "SpecializationName", doctor.SpecializationId);

            // Перевірка, чи є доступні години в базі даних
            var availabilities = await _context.Availabilities.ToListAsync();
            if (availabilities == null || !availabilities.Any())
            {
                // Обробка випадку, якщо немає доступних годин
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

                    // Update name and specialization only
                    existingDoctor.DoctorName = doctor.DoctorName;
                    existingDoctor.SpecializationId = doctor.SpecializationId;

                    // Видалити попередні години
                    existingDoctor.DoctorAvailabilities.Clear();

                    // Додати вибрані години
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
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(string id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
