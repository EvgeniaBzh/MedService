using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedService.Models;
using System.Threading.Tasks;

namespace MedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsApiController : ControllerBase
    {
        private readonly DbMedServiceContext _context;

        public DoctorsApiController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: api/DoctorsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalItems = await _context.Doctors.CountAsync();
            var doctors = await _context.Doctors
                .Skip(skip)  
                .Take(limit) 
                .ToListAsync(); 

            var nextLink = (skip + limit < totalItems) ?
                Url.Action(nameof(GetDoctors), new { skip = skip + limit, limit = limit }) :
                null;

            var response = new
            {
                data = doctors,
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

        // GET: api/DoctorsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(string id)
        {
            var doctor = await _context.Doctors
                                       .FirstOrDefaultAsync(d => d.DoctorId == id); 

            if (doctor == null)
            {
                return NotFound();
            }

            return doctor;
        }


        // POST: api/DoctorsApi
        [HttpPost]
        public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
        }

        // PUT: api/DoctorsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctor(string id, Doctor doctor)
        {
            if (id != doctor.DoctorId)
            {
                return BadRequest();
            }

            _context.Entry(doctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(id))
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

        // DELETE: api/DoctorsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DoctorExists(string id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
