using Microsoft.AspNetCore.Mvc;
using RouteRecorder.Services;

namespace RouteRecorder.Controllers
{
    public class RoutesController : Controller
    {
        public RouteService _routeService;

        public RoutesController(RouteService routeService)
        {
            _routeService = routeService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadGpx(IFormFile file)
        {
            if (file == null)
            {
                return View();
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                await _routeService.SaveRouteFromGpx(stream);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
