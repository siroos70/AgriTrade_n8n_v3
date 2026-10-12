using AgriTrade.Backend.Data;
using Microsoft.Extensions.Logging;

namespace AgriTrade.Backend.Services
{
    public interface IAuditService
    {
        Task LogAsync(string entity, string action, string? details, string? actor);
    }

    public class AuditService : IAuditService
    {
        private readonly AgriTradeContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AgriTradeContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string entity, string action, string? details, string? actor)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                Entity = entity,
                Action = action,
                Details = details,
                Actor = actor,
                CreatedAt = DateTime.Now
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}