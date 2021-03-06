﻿using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class SignRequest
    {
        [Required, StringLength(256, MinimumLength = 6), EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(32, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
