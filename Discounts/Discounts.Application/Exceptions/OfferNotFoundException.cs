// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class OfferNotFoundException : NotFoundException
    {
        public OfferNotFoundException(int id)
            : base($"The offer with ID {id} was not found")
        {
        }
    }
}
