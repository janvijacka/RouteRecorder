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
        public async Task<IActionResult> AddAsync(UserViewModel userViewModel)
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
        public async Task<IActionResult> DeleteAsync(string id)
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

        public async Task<IActionResult> UpdateAsync(string id)
        {
            AppUser userToUpdate = await _userManager.FindByIdAsync(id);
            if (userToUpdate == null)
            {
                return View("NotFound");
            }
            return View(userToUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAsync(string id, string email, string password)
        {
            AppUser userToUpdate = await _userManager.FindByIdAsync(id);
            if (userToUpdate != null)
            {
                IdentityResult validPassword;
                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)) 
                {
                    userToUpdate.Email = email;
                    validPassword = await _passwordValidator.ValidateAsync(_userManager, userToUpdate, password);
                    if (validPassword.Succeeded) 
                    {
                        userToUpdate.PasswordHash = _passwordHasher.HashPassword(userToUpdate, password);
                        IdentityResult result = await _userManager.UpdateAsync(userToUpdate);
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
                        AddErrors(validPassword);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found!");
            }
            return View(userToUpdate);
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
