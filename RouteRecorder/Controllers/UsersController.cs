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
            var model = new UsersViewModel
            {
                Users = _userManager.Users,
                AddUser = new UserViewModel()
            };
            return View(model);
        }

        public UsersController(UserManager<AppUser> userManager, IPasswordHasher<AppUser> passwordHasher, IPasswordValidator<AppUser> passwordValidator)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _passwordValidator = passwordValidator;
        }
    }
}
