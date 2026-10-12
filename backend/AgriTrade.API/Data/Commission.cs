using System;

namespace AgriTrade.Backend.Data
{
    public class Commission
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public Deal Deal { get; set; } = null!;
        public string CommissionType { get; set; } = "درصدی";
        public decimal? Rate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "ثبت‌شده";
        public DateTime CreatedAt { get; set; }
    }
}