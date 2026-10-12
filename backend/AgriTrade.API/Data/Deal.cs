using System;
using System.Collections.Generic;

namespace AgriTrade.Backend.Data
{
    public class Deal
    {
        public Guid Id { get; set; }
        public Guid? MatchId { get; set; }
        public Match? Match { get; set; }
        public Guid SellerId { get; set; }
        public Seller Seller { get; set; } = null!;
        public Guid BuyerId { get; set; }
        public Buyer Buyer { get; set; } = null!;
        public string Product { get; set; } = string.Empty;
        public decimal QuantityKg { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = "مذاکره";
        public DateTime? AgreedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual HashSet<Commission> Commissions { get; set; } = new();
        public virtual HashSet<Payment> Payments { get; set; } = new();
    }
}