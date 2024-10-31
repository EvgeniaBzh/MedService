using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

public class UploadController : Controller
{
    private readonly BlobService _blobService;

    public UploadController(BlobService blobService)
    {
        _blobService = blobService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.ImageUrl = _blobService.GetBlobUrl("newcontainer", "image3.jpg");
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length > 0)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                await _blobService.UploadFileAsync("newcontainer", stream, file.FileName);
            }
        }
        return RedirectToAction("Index");
    }
}
