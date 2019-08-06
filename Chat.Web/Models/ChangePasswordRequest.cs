using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class ChangePasswordRequest
    {
        public SignRequest SignInInfo { get; set; }
        [Required, StringLength(32, MinimumLength = 8), DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
