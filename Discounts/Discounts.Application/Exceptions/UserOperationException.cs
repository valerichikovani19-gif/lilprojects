// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class UserOperationException : Exception
    {
        public UserOperationException(string message) : base(message)
        {
        }

        public UserOperationException(IEnumerable<string> errors)
            : base(string.Join(" | ", errors))
        {
        }
    }
}
