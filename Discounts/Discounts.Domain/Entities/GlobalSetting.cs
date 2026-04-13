// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Domain.Entities
{
    public class GlobalSetting
    {
        //keyvalue pairic shemedzlo
        public int Id { get; set; }
        public int ReservationTimeInMinutes { get; set; } = 30;
        public int MerchantEditWindowInHours { get; set; } = 24;
    }
}
