using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class GroupRequest
    {
        [Required, StringLength(64, MinimumLength = 5)]
        public string Name { get; set; }
        [Required, StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
