﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RouteRecorder.Models;
using RouteRecorder.Services;
using RouteRecorder.ViewModels;

namespace RouteRecorder.Controllers
{
    public class RoutesController : Controller
    {
        public RouteService _routeService;
        public UserManager<AppUser> _userManager;

        private const int PageSize = 10;

        public RoutesController(RouteService routeService, UserManager<AppUser> userManager)
        {
            _routeService = routeService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,User,Visitor")]
        public IActionResult Index(int? pageNumber)
        {
            IEnumerable<RouteViewModel> allRoutes = _routeService.GetRoutes();
            var paginatedRoutes = PaginatedList<RouteViewModel>.CreatePaginatedList(allRoutes, pageNumber ?? 1, PageSize);
            return View(paginatedRoutes);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UploadGpx(IFormFile file)
        {
            if (file.Length > 0)
            {
                string filepath = Path.GetFullPath(file.FileName);
                var user = await _userManager.GetUserAsync(User);
                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    await _routeService.SaveRouteFromGpx(stream, user.UserName);
                    stream.Close();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
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

        [Authorize(Roles = "Admin,User,Visitor")]
        public async Task<IActionResult> ShowMap(int id)
        {
            var routeToShow = await _routeService.GetByIdAsync(id);
            if (routeToShow == null)
            {
                return RedirectToAction("Index"); //Dodělat vrácení chyby
            }
            ViewBag.Points = _routeService.GetPoints(routeToShow);
            var routeToShowViewModel = await _routeService.GetVMByIdAsync(id);
            return View("ShowMap", routeToShowViewModel);
        }
    }
}
