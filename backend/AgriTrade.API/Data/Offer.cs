using System;
using System.Collections.Generic;

namespace AgriTrade.Backend.Data
{
    public class Offer
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public Seller Seller { get; set; } = null!;
        public string Product { get; set; } = string.Empty;
        public string? Variety { get; set; }
        public decimal QuantityKg { get; set; }
        public string? Quality { get; set; }
        public DateTime? ReadyDate { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Packaging { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = "عالی";
        public DateTime CreatedAt { get; set; }
        public virtual HashSet<Match> Matches { get; set; } = new();
    }
}