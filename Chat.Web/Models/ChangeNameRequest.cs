using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class ChangeNameRequest
    {
        public SignRequest SignInInfo { get; set; }
        [Required, StringLength(64, MinimumLength = 5)]
        public string NewName { get; set; }
    }
}
