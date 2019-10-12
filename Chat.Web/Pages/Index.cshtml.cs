using Chat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chat.Web.Pages
{
    [ResponseCache(Duration = 1296000)]
    public class IndexModel : PageModel
    {
        public User _user;
        public IndexModel(User user) => _user = user;
        public void OnGet()
        {
        }
    }
}