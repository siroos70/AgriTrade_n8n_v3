using System;
using System.Collections.Generic;

namespace AgriTrade.Backend.Data
{
    public class Demand
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public Buyer Buyer { get; set; } = null!;
        public string Product { get; set; } = string.Empty;
        public string? Variety { get; set; }
        public decimal QuantityKg { get; set; }
        public string? MinQuality { get; set; }
        public decimal? MaxUnitPrice { get; set; }
        public string? DeliveryLocation { get; set; }
        public DateTime? NeededDate { get; set; }
        public string Status { get; set; } = "عالی";
        public DateTime CreatedAt { get; set; }
        public virtual HashSet<Match> Matches { get; set; } = new();
    }
}