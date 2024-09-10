using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MedService.Models;

namespace MedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Diagram2 : ControllerBase
    {
        private readonly DbMedServiceContext dbContext;

        public Diagram2(DbMedServiceContext context)
        {
            dbContext = context;
        }

        [HttpGet("orderTypeDistribution")]
        public IActionResult GetOrderTypeDistribution()
        {
            /*var result = dbContext.Orders
                .GroupBy(o => o.Type.TypeName)
                .Select(group => new
                {
                    TypeName = group.Key,
                    Count = group.Count()
                })
                .ToList();
*/
            return Ok(/*result*/);
        }
    }
}
