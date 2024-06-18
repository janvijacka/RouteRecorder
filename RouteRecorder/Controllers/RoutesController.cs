using Microsoft.AspNetCore.Mvc;
using RouteRecorder.Services;
using RouteRecorder.DTO;

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
            IEnumerable<RouteDTO> allRoutes = _routeService.GetRoutes();
            return View(allRoutes);
        }

        
        [HttpPost]
        public async Task<IActionResult> UploadGpx(IFormFile file)
        {
            if (file.Length > 0)
            {
                string filepath = Path.GetFullPath(file.FileName);
                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    await _routeService.SaveRouteFromGpx(stream);
                    stream.Close();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var routeToDelete = await _routeService.GetByIdAsync(id);
            if (routeToDelete == null)
            {
                return RedirectToAction("Index"); //Dodělat vrácení chyby
            }
            await _routeService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
