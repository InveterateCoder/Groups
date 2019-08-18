using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class AccountChangeRequest
    {
        public SignRequest SignInInfo { get; set; }
        [Required, StringLength(64, MinimumLength = 5)]
        public string NewName { get; set; }
        [Required, StringLength(32, MinimumLength = 8), DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}