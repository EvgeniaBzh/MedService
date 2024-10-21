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
    public class PatientsApiController : ControllerBase
    {
        private readonly DbMedServiceContext _context;

        public PatientsApiController(DbMedServiceContext context)
        {
            _context = context;
        }

        // GET: api/PatientsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalItems = await _context.Patients.CountAsync();  
            var patients = await _context.Patients
                .Skip(skip)  
                .Take(limit) 
                .ToListAsync();  

            var nextLink = (skip + limit < totalItems) ?
                Url.Action(nameof(GetPatients), new { skip = skip + limit, limit = limit }) :
                null;

            var response = new
            {
                data = patients,
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

        // GET: api/PatientsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(string id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            return patient;
        }

        // POST: api/PatientsApi
        [HttpPost]
        public async Task<ActionResult<Patient>> PostPatient(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, patient);
        }

        // PUT: api/PatientsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatient(string id, Patient patient)
        {
            if (id != patient.PatientId)
            {
                return BadRequest();
            }

            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id))
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

        // DELETE: api/PatientsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}
