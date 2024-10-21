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
    public class SpecializationsApiController : ControllerBase
    {
        private readonly DbMedServiceContext _context;

        public SpecializationsApiController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: api/SpecializationsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Specialization>>> GetSpecializations([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalItems = await _context.Specializations.CountAsync();
            var specializations = await _context.Specializations
                .Skip(skip)  
                .Take(limit) 
                .ToListAsync();  

            var nextLink = (skip + limit < totalItems) ?
                Url.Action(nameof(GetSpecializations), new { skip = skip + limit, limit = limit }) :
                null;

            var response = new
            {
                data = specializations,
                pagination = new
                {
                    totalItems = totalItems,
                    skip = skip,
                    limit = limit,
                    nextLink = nextLink
                }
            };

            return Ok(response);
        }

        // GET: api/SpecializationsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Specialization>> GetSpecialization(int id)
        {
            var specialization = await _context.Specializations.FindAsync(id);

            if (specialization == null)
            {
                return NotFound();
            }

            return specialization;
        }

        // POST: api/SpecializationsApi
        [HttpPost]
        public async Task<ActionResult<Specialization>> PostSpecialization(Specialization specialization)
        {
            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpecialization), new { id = specialization.SpecializationId }, specialization);
        }

        // PUT: api/SpecializationsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialization(int id, Specialization specialization)
        {
            if (id != specialization.SpecializationId)
            {
                return BadRequest();
            }

            _context.Entry(specialization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecializationExists(id))
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

        // DELETE: api/SpecializationsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var specialization = await _context.Specializations.FindAsync(id);
            if (specialization == null)
            {
                return NotFound();
            }

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpecializationExists(int id)
        {
            return _context.Specializations.Any(e => e.SpecializationId == id);
        }
    }
}
