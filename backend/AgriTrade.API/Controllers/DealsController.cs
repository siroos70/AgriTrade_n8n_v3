using AgriTrade.Backend.Data;
using AgriTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DealsController : ControllerBase
    {
        private readonly AgriTradeContext _context;
        private readonly ICommissionService _commissionService;
        private readonly ITelegramService _telegram;

        public DealsController(AgriTradeContext context, ICommissionService commissionService, ITelegramService telegram)
        {
            _context = context;
            _commissionService = commissionService;
            _telegram = telegram;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Deal>>> Get()
        {
            var query = _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .Include(d => d.Match)
                .AsQueryable();

            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                query = query.Where(d => d.Status == status);
            }

            if (Request.Query.ContainsKey("product"))
            {
                var product = Request.Query["product"].ToString();
                query = query.Where(d => d.Product == product);
            }

            var skip = Request.Query.ContainsKey("skip") ? int.Parse(Request.Query["skip"]) : 0;
            var take = Request.Query.ContainsKey("take") ? int.Parse(Request.Query["take"]) : 50;

            var result = await query.OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Deal>> Get(Guid id)
        {
            var deal = await _context.Deals
                .Include(d => d.Seller)
                .Include(d => d.Buyer)
                .Include(d => d.Match)
                .ThenInclude(m => m.Offer)
                .Include(d => d.Commissions)
                .Include(d => d.Payments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return NotFound();

            return Ok(deal);
        }

        [HttpPost]
        public async Task<ActionResult<Deal>> Post([FromBody] DealCreateDto dto)
        {
            if (!_context.Sellers.Any(s => s.Id == dto.SellerId))
                return NotFound(new { message = "فروشنده یافت نشد" });

            if (!_context.Buyers.Any(b => b.Id == dto.BuyerId))
                return NotFound(new { message = "خریدار یافت نشد" });

            var quantity = dto.QuantityKg;
            var unitPrice = dto.UnitPrice;
            var totalValue = quantity * unitPrice;

            var deal = new Deal
            {
                Id = Guid.NewGuid(),
                MatchId = dto.MatchId,
                SellerId = dto.SellerId,
                BuyerId = dto.BuyerId,
                Product = dto.Product,
                QuantityKg = quantity,
                UnitPrice = unitPrice,
                TotalValue = totalValue,
                Status = dto.Status ?? "مذاکره",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            // Notify via Telegram
            await _telegram.SendMessageAsync(
                $"معامله جدید ثبت شد: {dto.Product} | مقدار {quantity}kg | ارزش {totalValue:C} | وضعیت {deal.Status}"
            );

            return CreatedAtAction(nameof(Get), new { id = deal.Id }, deal);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Deal>> Put(Guid id, [FromBody] DealUpdateDto dto)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
                return NotFound();

            deal.Status = dto.Status ?? deal.Status;
            deal.Notes = dto.Notes ?? deal.Notes;

            if (dto.QuantityKg.HasValue) deal.QuantityKg = dto.QuantityKg.Value;
            if (dto.UnitPrice.HasValue)
            {
                deal.UnitPrice = dto.UnitPrice.Value;
                deal.TotalValue = deal.QuantityKg * deal.UnitPrice;
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (dto.Status == "قرارداد") deal.AgreedAt = DateTime.UtcNow;
                if (dto.Status == "تحویل") deal.DeliveredAt = DateTime.UtcNow;
                if (dto.Status == "تسویه‌شده") deal.SettledAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(deal);
        }

        [HttpPost("{id}/create-commission")]
        public async Task<ActionResult<Commission>> CreateCommission(Guid id, [FromQuery] decimal? rate = null)
        {
            var commission = await _commissionService.CalculateCommissionAsync(id, rate);
            return Ok(commission);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(Guid id)
        {
            var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
            try
            {
                var pdfBytes = await pdfService.GenerateInvoiceAsync(id);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
                return NotFound();

            deal.Status = "لغوشده";
            await _context.SaveChangesAsync();

            return Ok(new { message = "معامله با موفقیت لغو شد" });
        }
    }

    public class DealCreateDto
    {
        public Guid? MatchId { get; set; }
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; }
        public string Product { get; set; } = "انجیر";
        public decimal QuantityKg { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Status { get; set; } = "مذاکره";
        public string? Notes { get; set; }
    }

    public class DealUpdateDto
    {
        public decimal? QuantityKg { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
