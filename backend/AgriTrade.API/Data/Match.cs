using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace AgriTrade.Backend.Data
{
    public class Match
    {
        public Guid Id { get; set; }
        public Guid OfferId { get; set; }
        public Offer Offer { get; set; } = null!;
        public Guid DemandId { get; set; }
        public Demand Demand { get; set; } = null!;
        public decimal Score { get; set; }
        public JsonNode? Reasons { get; set; }
        public string Status { get; set; } = "جدید";
        public DateTime CreatedAt { get; set; }
        public virtual HashSet<Deal> Deals { get; set; } = new();
    }
}