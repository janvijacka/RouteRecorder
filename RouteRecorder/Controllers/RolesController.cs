﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RouteRecorder.Models;

namespace RouteRecorder.Controllers
{
    public class RolesController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager,  UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(_roleManager.Roles.OrderBy(r => r.Name).ToList());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsync(string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(name);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var roleToDelte = await _roleManager.FindByIdAsync(id);
            if (roleToDelte != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(roleToDelte);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            ModelState.AddModelError("", "No role found");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonmembers = new List<AppUser>();
            foreach (AppUser user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonmembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                RoleMembers = members,
                RoleNonMembers = nonmembers
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignAsync(RoleModifications roleModifications)
        {
            AppUser user;
            IdentityResult result;
            foreach (var id in roleModifications.AddIds ?? new string[] { })
            {
                user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    result = await _userManager.AddToRoleAsync(user, roleModifications.RoleName);
                    if (result != IdentityResult.Success)
                    {
                        AddErrors(result);
                    }
                }
            }
            foreach (var id in roleModifications.DeleteIds ?? new string[] { })
            {
                user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    result = await _userManager.RemoveFromRoleAsync(user, roleModifications.RoleName);
                    if (result != IdentityResult.Success)
                    {
                        AddErrors(result);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        private void AddErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
