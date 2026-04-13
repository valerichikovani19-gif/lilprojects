// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException(string emailOrId)
            : base($"The user {emailOrId} was not found")
        {
        }
    }
}
