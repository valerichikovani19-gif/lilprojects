// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public abstract class BadRequestException : ApplicationException
    {
        protected BadRequestException(string message) : base(message)
        {
        }
    }
}
