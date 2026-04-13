// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class UserAlreadyExistsException : BadRequestException
    {
        public UserAlreadyExistsException(string email)
            : base($"A user with the email - {email} - already exists")
        {
        }
    }
}
