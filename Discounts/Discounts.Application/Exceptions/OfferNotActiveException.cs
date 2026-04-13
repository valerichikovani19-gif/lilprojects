// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class OfferNotActiveException : BadRequestException
    {
        public OfferNotActiveException()
            : base("This offer is not currently active, either pending approval or has expired idk")
        {
        }
    }
}
