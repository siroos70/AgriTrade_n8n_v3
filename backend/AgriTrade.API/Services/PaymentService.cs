using AgriTrade.Backend.Data;

namespace AgriTrade.Backend.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Guid dealId, decimal amount, string method);
        Task<Payment> UpdatePaymentStatusAsync(Guid paymentId, string status, string? gatewayRef = null);
        Task<List<Payment>> GetPaymentsAsync();
        Task<List<Payment>> GetDealPaymentsAsync(Guid dealId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AgriTradeContext _context;
        private readonly ITelegramService _telegram;

        public PaymentService(AgriTradeContext context, ITelegramService telegram)
        {
            _context = context;
            _telegram = telegram;
        }

        public async Task<Payment> CreatePaymentAsync(Guid dealId, decimal amount, string method)
        {
            var deal = await _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .FirstOrDefaultAsync(d => d.Id == dealId);

            if (deal == null)
                throw new Exception("Deal not found");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                DealId = dealId,
                Amount = amount,
                Method = method,
                Status = "ثبت شده",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _telegram.SendMessageAsync($"پرداخت جدید: معامله {dealId} | مبلغ {amount:C} | روش {method} | وضعیت ثبت شده");

            return payment;
        }

        public async Task<Payment> UpdatePaymentStatusAsync(Guid paymentId, string status, string? gatewayRef = null)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new Exception("Payment not found");

            payment.Status = status;
            payment.GatewayRef = gatewayRef;
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<List<Payment>> GetPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Deal)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetDealPaymentsAsync(Guid dealId)
        {
            return await _context.Payments
                .Where(p => p.DealId == dealId)
                .ToListAsync();
        }
    }
}
