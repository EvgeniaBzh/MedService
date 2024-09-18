using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MedService.Models;

namespace MedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Diagram2Controller : ControllerBase
    {
        private readonly DbMedServiceContext dbContext;

        public Diagram2Controller(DbMedServiceContext context)
        {
            dbContext = context;
        }

        [HttpGet("specializationDistribution")]
        public IActionResult GetSpecializationDistribution()
        {
            var result = dbContext.Doctors
                .GroupBy(d => d.Specialization.SpecializationName)
                .Select(group => new
                {
                    SpecializationName = group.Key,
                    Count = group.Count()
                })
                .ToList();

            return Ok(result);
        }
    }
}
