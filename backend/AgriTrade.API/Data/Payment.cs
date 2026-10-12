using System;

namespace AgriTrade.Backend.Data
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public Deal Deal { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = " ثبت شده";
        public string? GatewayRef { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}