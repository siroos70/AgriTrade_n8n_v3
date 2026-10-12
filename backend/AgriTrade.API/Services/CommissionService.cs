using AgriTrade.Backend.Data;

namespace AgriTrade.Backend.Services
{
    public interface ICommissionService
    {
        Task<Commission> CalculateCommissionAsync(Guid dealId, decimal? rate = null);
        Task<List<Commission>> GetCommissionsAsync();
        Task<Commission?> GetCommissionAsync(Guid id);
    }

    public class CommissionService : ICommissionService
    {
        private readonly AgriTradeContext _context;
        private readonly ITelegramService _telegram;

        public CommissionService(AgriTradeContext context, ITelegramService telegram)
        {
            _context = context;
            _telegram = telegram;
        }

        public async Task<Commission> CalculateCommissionAsync(Guid dealId, decimal? rate = null)
        {
            var deal = await _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .FirstOrDefaultAsync(d => d.Id == dealId);

            if (deal == null)
                throw new Exception("Deal not found");

            var commissionRate = rate ?? 0.0m; // Default commission rate can be configurable

            var commission = new Commission
            {
                Id = Guid.NewGuid(),
                DealId = dealId,
                CommissionType = "درصدی",
                Rate = commissionRate,
                Amount = deal.TotalValue * commissionRate / 100,
                Status = "ثبت‌شده",
                CreatedAt = DateTime.UtcNow
            };

            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync();

            await _telegram.SendMessageAsync($"کمیسیون جدید ثبت شد: معامله {dealId} | مبلغ {commission.Amount:C} | نرخ {commissionRate}%");

            return commission;
        }

        public async Task<List<Commission>> GetCommissionsAsync()
        {
            return await _context.Commissions
                .Include(c => c.Deal)
                .ToListAsync();
        }

        public async Task<Commission?> GetCommissionAsync(Guid id)
        {
            return await _context.Commissions
                .Include(c => c.Deal)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
