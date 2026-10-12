using System;

namespace AgriTrade.Backend.Data
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string Entity { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? Actor { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}