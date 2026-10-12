using AgriTrade.Backend.Data;

namespace AgriTrade.Backend.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoiceAsync(Guid dealId);
        Task<byte[]> GenerateCommissionReportAsync(Guid commissionId);
        Task<byte[]> GenerateDealDocumentAsync(Guid dealId);
        Task<byte[]> GenerateSellerReportAsync(Guid sellerId);
        Task<byte[]> GenerateBuyerReportAsync(Guid buyerId);
    }

    public class PdfService : IPdfService
    {
        private readonly AgriTradeContext _context;

        public PdfService(AgriTradeContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateInvoiceAsync(Guid dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .FirstOrDefaultAsync(d => d.Id == dealId);

            if (deal == null)
                throw new Exception("Deal not found");

            var commissions = await _context.Commissions
                .Where(c => c.DealId == dealId)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => p.DealId == dealId)
                .ToListAsync();

            var document = DocumentGenerator.GenerateInvoice(deal, commissions, payments);
            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateCommissionReportAsync(Guid commissionId)
        {
            var commission = await _context.Commissions
                .Include(c => c.Deal)
                .FirstOrDefaultAsync(c => c.Id == commissionId);

            if (commission == null)
                throw new Exception("Commission not found");

            var document = DocumentGenerator.GenerateCommissionReport(commission);
            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateDealDocumentAsync(Guid dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .FirstOrDefaultAsync(d => d.Id == dealId);

            if (deal == null)
                throw new Exception("Deal not found");

            var document = DocumentGenerator.GenerateDealDocument(deal);
            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateSellerReportAsync(Guid sellerId)
        {
            var seller = await _context.Sellers.FindAsync(sellerId);
            if (seller == null)
                throw new Exception("Seller not found");

            var deals = await _context.Deals
                .Where(d => d.SellerId == sellerId)
                .ToListAsync();

            var document = DocumentGenerator.GenerateSellerReport(seller, deals);
            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateBuyerReportAsync(Guid buyerId)
        {
            var buyer = await _context.Buyers.FindAsync(buyerId);
            if (buyer == null)
                throw new Exception("Buyer not found");

            var deals = await _context.Deals
                .Where(d => d.BuyerId == buyerId)
                .ToListAsync();

            var document = DocumentGenerator.GenerateBuyerReport(buyer, deals);
            return document.GeneratePdf();
        }
    }
}
