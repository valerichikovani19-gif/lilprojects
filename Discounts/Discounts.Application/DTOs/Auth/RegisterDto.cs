// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        //merchant or default customer
        public string Role { get; set; } = "Customer";
    }
}
