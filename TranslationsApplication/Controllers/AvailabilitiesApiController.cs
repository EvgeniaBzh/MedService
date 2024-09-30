using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilitiesApiController : ControllerBase
    {
        private readonly DbMedServiceContext _context;

        public AvailabilitiesApiController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: api/AvailabilitiesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Availability>>> GetAvailabilities()
        {
            return await _context.Availabilities.ToListAsync();
        }

        // GET: api/AvailabilitiesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Availability>> GetAvailability(string id)
        {
            var availability = await _context.Availabilities.FindAsync(id);

            if (availability == null)
            {
                return NotFound();
            }

            return availability;
        }

        // POST: api/AvailabilitiesApi
        [HttpPost]
        public async Task<ActionResult<Availability>> PostAvailability(Availability availability)
        {
            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailability), new { id = availability.AvailabilityId }, availability);
        }

        // PUT: api/AvailabilitiesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvailability(string id, Availability availability)
        {
            if (id != availability.AvailabilityId)
            {
                return BadRequest();
            }

            _context.Entry(availability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/AvailabilitiesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(string id)
        {
            var availability = await _context.Availabilities.FindAsync(id);
            if (availability == null)
            {
                return NotFound();
            }

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AvailabilityExists(string id)
        {
            return _context.Availabilities.Any(e => e.AvailabilityId == id);
        }
    }
}
