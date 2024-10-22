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
using System.Text;

namespace MedService.Controllers
{
    public class PatientsController : Controller
    {
        private readonly DbMedServiceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(DbMedServiceContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Patients.ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(patient.MedicalCardFilePath))
            {
                string medicalCardPath = Path.Combine(Directory.GetCurrentDirectory(), "MedicalCards", patient.MedicalCardFilePath);
                if (System.IO.File.Exists(medicalCardPath))
                {
                    var fileContent = await System.IO.File.ReadAllTextAsync(medicalCardPath);
                    ViewData["MedicalCardContent"] = fileContent;
                }
                else
                {
                    ViewData["MedicalCardContent"] = "Medical card not found.";
                }
            }
            else
            {
                ViewData["MedicalCardContent"] = "No medical card available.";
            }

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMedicalCard(string id, string medicalCardContent)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null || patient.MedicalCardFilePath == null)
            {
                return NotFound();
            }

            string medicalCardPath = Path.Combine(Directory.GetCurrentDirectory(), "MedicalCards", patient.MedicalCardFilePath);

            if (System.IO.File.Exists(medicalCardPath))
            {
                await System.IO.File.WriteAllTextAsync(medicalCardPath, medicalCardContent);
            }

            return RedirectToAction(nameof(Details), new { id });
        }



        // GET: Patients/Create
        public IActionResult Create()
        {
            ViewData["Sex"] = new SelectList(Enum.GetValues(typeof(Sex)).Cast<Sex>());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientName,PatientEmail,PatientPassword,DateOfBirth,PatientSex,MedicalCardFilePath")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientEmail == patient.PatientEmail);
                if (patientExists)
                {
                    ModelState.AddModelError("PatientEmail", "Пацієнт з таким email вже існує.");
                    return View(patient);
                }

                var user = new ApplicationUser { UserName = patient.PatientEmail, Email = patient.PatientEmail };
                var result = await _userManager.CreateAsync(user, patient.PatientPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "patient");

                    patient.PatientId = user.Id;
                    _context.Add(patient);
                    await _context.SaveChangesAsync();

                    string medicalCardDirectory = Path.Combine(Directory.GetCurrentDirectory(), "MedicalCards");
                    if (!Directory.Exists(medicalCardDirectory))
                    {
                        Directory.CreateDirectory(medicalCardDirectory); 
                    }

                    string medicalCardFileName = $"{patient.PatientId}_MedicalCard.txt"; 
                    string medicalCardFilePath = Path.Combine(medicalCardDirectory, medicalCardFileName);

                    await System.IO.File.WriteAllTextAsync(medicalCardFilePath, "Medical card content here...");

                    patient.MedicalCardFilePath = medicalCardFileName;
                    _context.Update(patient);
                    await _context.SaveChangesAsync(); 

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
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            ViewData["Sex"] = new SelectList(Enum.GetValues(typeof(Sex)).Cast<Sex>());
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PatientId,PatientName,PatientEmail,PatientPassword,DateOfBirth,PatientSex,MedicalCardFilePath")] Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
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
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }

        // GET: Patients/Map
        public IActionResult Map()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientsCoordinates()
        {
            var patientCoordinates = await _context.Patients
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .Select(p => new
                {
                    p.PatientId,
                    p.PatientName,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                })
                .ToListAsync();

            foreach (var patient in patientCoordinates)
            {
                Console.WriteLine($"Patient: {patient.PatientName}, Latitude: {patient.Latitude}, Longitude: {patient.Longitude}");
            }

            return Json(patientCoordinates);

        }

    }
}
