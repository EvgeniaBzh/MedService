using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedService.Models;

namespace MedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Diagram1Controller : ControllerBase
    {
        private readonly DbMedServiceContext _context;

        public Diagram1Controller(DbMedServiceContext context)
        {
            _context = context;
        }

        public record SpecializationStats(string SpecializationName, int DoctorCount);

        [HttpGet("specializationDistribution")]
        public async Task<ActionResult<IEnumerable<SpecializationStats>>> GetSpecializationDistribution()
        {
            var specializationCounts = await _context.Doctors
                .GroupBy(d => d.SpecializationId)
                .Select(g => new { SpecializationId = g.Key, Count = g.Count() })
                .ToListAsync();

            var specializations = await _context.Specializations.ToListAsync();

            var result = specializations
                .Select(s => new SpecializationStats(
                    s.SpecializationName,
                    specializationCounts.FirstOrDefault(sc => sc.SpecializationId == s.SpecializationId)?.Count ?? 0
                )).ToList();

            return Ok(result);
        }
    }
}
