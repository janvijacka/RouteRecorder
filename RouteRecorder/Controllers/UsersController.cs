using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RouteRecorder.Models;
using RouteRecorder.ViewModels;

namespace RouteRecorder.Controllers
{
    public class UsersController : Controller
    {
        private UserManager<AppUser> _userManager;
        private IPasswordHasher<AppUser> _passwordHasher;
        private IPasswordValidator<AppUser> _passwordValidator;

        public IActionResult Index()
        {
            return View(_userManager.Users);
        }

        public UsersController(UserManager<AppUser> userManager, IPasswordHasher<AppUser> passwordHasher, IPasswordValidator<AppUser> passwordValidator)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _passwordValidator = passwordValidator;
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser newUser = new AppUser
                {
                    UserName = userViewModel.Username,
                    Email = userViewModel.Email,
                };
                IdentityResult result = await _userManager.CreateAsync(newUser, userViewModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete != null)
            {
                var result = await _userManager.DeleteAsync(userToDelete);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found!");
            }
            return View("Index");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors) 
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
