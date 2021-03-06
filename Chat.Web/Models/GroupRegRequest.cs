﻿using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class GroupRegRequest
    {
        [Required, StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }
        [Required, StringLength(64, MinimumLength = 5)]
        public string GroupName { get; set; }
        [StringLength(32, MinimumLength = 8)]
        public string GroupPassword { get; set; }
    }
}
