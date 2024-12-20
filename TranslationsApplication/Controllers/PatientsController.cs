﻿using System;
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
using Microsoft.Extensions.Caching.Memory;

namespace MedService.Controllers
{
    public class PatientsController : Controller
    {
        private readonly DbMedServiceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _memoryCache;

        public PatientsController(DbMedServiceContext context, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var patients = await GetCachedPatientsWithMedicalCardsAsync();
            return View(patients);
        }

        private async Task<List<Patient>> GetCachedPatientsWithMedicalCardsAsync()
        {
            string cacheKey = "patients";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Patient> patients))
            {
                patients = await _context.Patients
                    .Include(p => p.DoctorAvailabilities) 
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                // Store in cache
                _memoryCache.Set(cacheKey, patients, cacheOptions);
            }

            return patients;
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

                    _memoryCache.Remove("patients");

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

                    _memoryCache.Remove("patients");
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

            _memoryCache.Remove("patients");
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }

        // GET: Patients/Map
        public async Task<IActionResult> Map()
        {
            var patients = await _context.Patients
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .ToListAsync();

            ViewBag.Patients = new SelectList(patients, "PatientId", "PatientName");
            return View();
        }

        // GET: Patients/GetPatientCoordinates/{id}
        [HttpGet]
        public async Task<IActionResult> GetPatientCoordinates(string id)
        {
            var patient = await _context.Patients
                .Where(p => p.PatientId == id && p.Latitude.HasValue && p.Longitude.HasValue)
                .Select(p => new
                {
                    p.PatientId,
                    p.PatientName,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return Json(null);
            }

            return Json(patient);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatientCoordinates()
        {
            var patients = await _context.Patients
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .Select(p => new
                {
                    p.PatientId,
                    p.PatientName,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                })
                .ToListAsync();

            return Json(patients);
        }
    }
}
