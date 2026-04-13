// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class InvalidCredentialsException : BadRequestException
    {
        public InvalidCredentialsException()
            : base("Invalid email or password")
        {
        }
    }
}
