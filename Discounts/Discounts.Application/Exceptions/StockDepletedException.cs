// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class StockDepletedException : BadRequestException
    {
        public StockDepletedException(string title)
            : base($"The offer - {title} -  is sold out")
        {
        }
    }
}
