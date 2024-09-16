using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedService.Infrastructure.Services;
using MedService.Models;

namespace MedService.Controllers
{
    public class SpecializationsController : Controller
    {
        private readonly DbMedServiceContext _context;

        public SpecializationsController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: Types
        public async Task<IActionResult> Index()
        {
            return _context.Specializations != null ?
                        View(await _context.Specializations.ToListAsync()) :
                        Problem("Entity set 'DbMedServiceContext.Specializations'  is null.");
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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SpecializationExists(int id)
        {
            return (_context.Specializations?.Any(e => e.SpecializationId == id)).GetValueOrDefault();
        }
    }
}