using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedService.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using MedService.Models;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MedService.Controllers
{
    public class SpecializationsController : Controller
    {
        private readonly DbMedServiceContext _context;
        private readonly IMemoryCache _memoryCache;

        public SpecializationsController(DbMedServiceContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: Types
        public async Task<IActionResult> Index()
        {
            var specializations = await GetCachedSpecializationsWithDoctorsAsync();
            return View(specializations);

            /*
            return _context.Specializations != null ?
                        View(await _context.Specializations.ToListAsync()) :
                        Problem("Entity set 'DbMedServiceContext.Specializations' is null.");
            */
        }

        private async Task<List<Specialization>> GetCachedSpecializationsWithDoctorsAsync()
        {
            string cacheKey = "specializations_with_doctors";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Specialization> specializations))
            {
                specializations = await _context.Specializations
                    .Include(s => s.Doctors) 
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(cacheKey, specializations, cacheOptions);
            }

            return specializations;
        }

        // GET: Types/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Specializations == null)
            {
                return NotFound();
            }
            var specialization = await _context.Specializations
                .Include(s => s.Doctors)
                    //.ThenInclude()
                .FirstOrDefaultAsync(m => m.SpecializationId == id);

            if (specialization == null)
            {
                return NotFound();
            }

            return View(specialization);
        }

        // GET: Types/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Types/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpecializationId,SpecializationName")] MedService.Models.Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                var specializationExists = await _context.Specializations.AnyAsync(s => s.SpecializationName == specialization.SpecializationName);
                if (specializationExists)
                {
                    ModelState.AddModelError("SpecializationName", "A type with that name already exists.");
                    return View(specialization);
                }

                _context.Add(specialization);
                await _context.SaveChangesAsync();

                _memoryCache.Remove("specializations_with_doctors");

                return RedirectToAction(nameof(Index));
            }
            return View(specialization);
        }

        // GET: Types/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Specializations == null)
            {
                return NotFound();
            }

            var @specialization = await _context.Specializations.FindAsync(id);
            if (@specialization == null)
            {
                return NotFound();
            }
            return View(@specialization);
        }

        // POST: Types/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecializationId,SpecializationName")] MedService.Models.Specialization specialization)
        {
            if (id != specialization.SpecializationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var specializationExists = await _context.Specializations
                                                 .AnyAsync(s => s.SpecializationName == specialization.SpecializationName && s.SpecializationId != specialization.SpecializationId);
                if (specializationExists)
                {
                    ModelState.AddModelError("SpecializationName", "A specialization with that name already exists.");
                    return View(specialization);
                }

                try
                {
                    _context.Update(specialization);
                    await _context.SaveChangesAsync();

                    _memoryCache.Remove("specializations_with_doctors");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecializationExists(specialization.SpecializationId))
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
            return View(specialization);
        }

        // GET: Types/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Specializations == null)
            {
                return NotFound();
            }

            var specialization = await _context.Specializations
                .FirstOrDefaultAsync(m => m.SpecializationId == id);
            if (specialization == null)
            {
                return NotFound();
            }

            return View(specialization);
        }

        // POST: Types/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Specializations == null)
            {
                return Problem("Entity set 'DbMedServiceContext.Types' is null.");
            }

            var specialization = await _context.Specializations
                .Include(t => t.Doctors)
                .FirstOrDefaultAsync(m => m.SpecializationId == id);

            if (specialization != null)
            {
                if (specialization.Doctors != null)
                {
                    _context.Doctors.RemoveRange(specialization.Doctors);
                }

                _context.Specializations.Remove(specialization);

                await _context.SaveChangesAsync();

                _memoryCache.Remove("specializations_with_doctors");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SpecializationExists(int id)
        {
            return (_context.Specializations?.Any(e => e.SpecializationId == id)).GetValueOrDefault();
        }

        [HttpGet]
        public IActionResult GetSpecializations(string term)
        {
            var specializations = _context.Specializations
                .Where(s => s.SpecializationName.Contains(term))
                .Select(s => new { id = s.SpecializationId, text = s.SpecializationName })
                .ToList();
            return Json(specializations);
        }

    }
}