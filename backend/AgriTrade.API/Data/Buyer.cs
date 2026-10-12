using System;
using System.Collections.Generic;

namespace AgriTrade.Backend.Data
{
    public class Buyer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "عالی";
        public DateTime CreatedAt { get; set; }
        public virtual HashSet<Demand> Demands { get; set; } = new();
        public virtual HashSet<Deal> Deals { get; set; } = new();
    }
}