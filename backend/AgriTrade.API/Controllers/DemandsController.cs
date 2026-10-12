using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DemandsController : ControllerBase
    {
        private readonly AgriTradeContext _context;

        public DemandsController(AgriTradeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Demand>>> Get()
        {
            var query = _context.Demands
                .Include(d => d.Buyer)
                .Include(d => d.Matches)
                .AsQueryable();

            if (Request.Query.ContainsKey("product"))
            {
                var product = Request.Query["product"].ToString();
                query = query.Where(d => d.Product == product);
            }

            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                query = query.Where(d => d.Status == status);
            }

            if (Request.Query.ContainsKey("buyerId"))
            {
                if (Guid.TryParse(Request.Query["buyerId"], out var buyerId))
                    query = query.Where(d => d.BuyerId == buyerId);
            }

            var skip = Request.Query.ContainsKey("skip") ? int.Parse(Request.Query["skip"]) : 0;
            var take = Request.Query.ContainsKey("take") ? int.Parse(Request.Query["take"]) : 50;

            var result = await query.OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Demand>> Get(Guid id)
        {
            var demand = await _context.Demands
                .Include(d => d.Buyer)
                .Include(d => d.Matches)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demand == null)
                return NotFound();

            return Ok(demand);
        }

        [HttpPost]
        public async Task<ActionResult<Demand>> Post([FromBody] DemandCreateDto dto)
        {
            var allowedProducts = new[] { "انجیر", "انار" };
            if (!allowedProducts.Contains(dto.Product))
                return BadRequest(new { message = "محصول باید انجیر یا انار باشد" });

            if (dto.QuantityKg <= 0)
                return BadRequest(new { message = "مقدار باید بزرگتر از صفر باشد" });

            if (!_context.Buyers.Any(b => b.Id == dto.BuyerId))
                return NotFound(new { message = "خریدار یافت نشد" });

            var demand = new Demand
            {
                Id = Guid.NewGuid(),
                BuyerId = dto.BuyerId.Value,
                Product = dto.Product,
                Variety = dto.Variety,
                QuantityKg = dto.QuantityKg,
                MinQuality = dto.MinQuality,
                MaxUnitPrice = dto.MaxUnitPrice,
                DeliveryLocation = dto.DeliveryLocation,
                NeededDate = dto.NeededDate,
                Status = "فعال",
                CreatedAt = DateTime.UtcNow
            };

            _context.Demands.Add(demand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = demand.Id }, demand);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Demand>> Put(Guid id, [FromBody] DemandUpdateDto dto)
        {
            var demand = await _context.Demands.FindAsync(id);
            if (demand == null)
                return NotFound();

            if (dto.QuantityKg.HasValue) demand.QuantityKg = dto.QuantityKg.Value;
            demand.MinQuality = dto.MinQuality ?? demand.MinQuality;
            if (dto.MaxUnitPrice.HasValue) demand.MaxUnitPrice = dto.MaxUnitPrice.Value;
            demand.DeliveryLocation = dto.DeliveryLocation ?? demand.DeliveryLocation;
            demand.NeededDate = dto.NeededDate ?? demand.NeededDate;
            demand.Status = dto.Status ?? demand.Status;

            await _context.SaveChangesAsync();

            return Ok(demand);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var demand = await _context.Demands.FindAsync(id);
            if (demand == null)
                return NotFound();

            demand.Status = "حذف‌شده";
            await _context.SaveChangesAsync();

            return Ok(new { message = "تقاضا با موفقیت حذف شد" });
        }
    }

    public class DemandCreateDto
    {
        public Guid? BuyerId { get; set; }
        public string Product { get; set; } = string.Empty;
        public string? Variety { get; set; }
        public decimal QuantityKg { get; set; }
        public string? MinQuality { get; set; }
        public decimal? MaxUnitPrice { get; set; }
        public string? DeliveryLocation { get; set; }
        public DateTime? NeededDate { get; set; }
    }

    public class DemandUpdateDto
    {
        public decimal? QuantityKg { get; set; }
        public string? MinQuality { get; set; }
        public decimal? MaxUnitPrice { get; set; }
        public string? DeliveryLocation { get; set; }
        public DateTime? NeededDate { get; set; }
        public string? Status { get; set; }
    }
}
