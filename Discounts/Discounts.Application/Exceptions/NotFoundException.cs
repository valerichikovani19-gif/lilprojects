// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public abstract class NotFoundException : ApplicationException
    {
        protected NotFoundException(string message) : base(message)
        {
        }
    }
}
