// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class UnauthorizedActionException : ApplicationException
    {
        public UnauthorizedActionException(string message) : base(message)
        {
        }
    }
}
