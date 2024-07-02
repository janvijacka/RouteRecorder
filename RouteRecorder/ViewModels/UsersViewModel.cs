using RouteRecorder.Models;

namespace RouteRecorder.ViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<AppUser> Users { get; set; }
        public UserViewModel AddUser { get; set; }
    }
}
