using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MedService.Models;
using MedService.Infrastructure.Services;

namespace MedService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlobService _blobService;

        public HomeController(ILogger<HomeController> logger, BlobService blobService)
        {
            _logger = logger;
            _blobService = blobService;
        }

        public IActionResult Index()
        {
            string imageUrl = _blobService.GetBlobUrl("newcontainer", "image3.jpg");
            ViewBag.ImageUrl = imageUrl; 
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
