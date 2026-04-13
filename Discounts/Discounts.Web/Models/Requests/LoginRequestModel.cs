// Copyright (C) TBC Bank. All Rights Reserved.
using System.ComponentModel.DataAnnotations;

namespace Discounts.Web.Models.Requests
{
    public class LoginRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
