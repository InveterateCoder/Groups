using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chat.Web.Pages
{
    public class IndexModel : PageModel
    {
        public User _user;
        public IndexModel(User user) => _user = user;
        public void OnGet()
        {
        }
    }
}