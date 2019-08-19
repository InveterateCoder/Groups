using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class AccountChangeRequest
    {
        public SignRequest SignInInfo { get; set; }
        [StringLength(64, MinimumLength = 5)]
        public string NewName { get; set; }
        [StringLength(32, MinimumLength = 8), DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}