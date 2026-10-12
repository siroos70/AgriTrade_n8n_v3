using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OffersController : ControllerBase
    {
        private readonly AgriTradeContext _context;

        public OffersController(AgriTradeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Offer>>> Get()
        {
            var query = _context.Offers
                .Include(o => o.Seller)
                .Include(o => o.Matches)
                .AsQueryable();

            if (Request.Query.ContainsKey("product"))
            {
                var product = Request.Query["product"].ToString();
                query = query.Where(o => o.Product == product);
            }

            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                query = query.Where(o => o.Status == status);
            }

            if (Request.Query.ContainsKey("sellerId"))
            {
                if (Guid.TryParse(Request.Query["sellerId"], out var sellerId))
                    query = query.Where(o => o.SellerId == sellerId);
            }

            var skip = Request.Query.ContainsKey("skip") ? int.Parse(Request.Query["skip"]) : 0;
            var take = Request.Query.ContainsKey("take") ? int.Parse(Request.Query["take"]) : 50;

            var result = await query.OrderByDescending(o => o.CreatedAt)
                .Skip(skip).Take(take).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Offer>> Get(Guid id)
        {
            var offer = await _context.Offers
                .Include(o => o.Seller)
                .Include(o => o.Matches)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offer == null)
                return NotFound();

            return Ok(offer);
        }

        [HttpPost]
        public async Task<ActionResult<Offer>> Post([FromBody] OfferCreateDto dto)
        {
            var allowedProducts = new[] { "انجیر", "انار" };
            if (!allowedProducts.Contains(dto.Product))
                return BadRequest(new { message = "محصول باید انجیر یا انار باشد" });

            if (dto.QuantityKg <= 0)
                return BadRequest(new { message = "مقدار باید بزرگتر از صفر باشد" });

            if (!_context.Sellers.Any(s => s.Id == dto.SellerId))
                return NotFound(new { message = "فروشنده یافت نشد" });

            var offer = new Offer
            {
                Id = Guid.NewGuid(),
                SellerId = dto.SellerId.Value,
                Product = dto.Product,
                Variety = dto.Variety,
                QuantityKg = dto.QuantityKg,
                Quality = dto.Quality,
                ReadyDate = dto.ReadyDate,
                UnitPrice = dto.UnitPrice,
                Packaging = dto.Packaging,
                Location = dto.Location,
                Status = "فعال",
                CreatedAt = DateTime.UtcNow
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = offer.Id }, offer);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Offer>> Put(Guid id, [FromBody] OfferUpdateDto dto)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null)
                return NotFound();

            offer.Variety = dto.Variety ?? offer.Variety;
            if (dto.QuantityKg.HasValue) offer.QuantityKg = dto.QuantityKg.Value;
            offer.Quality = dto.Quality ?? offer.Quality;
            offer.ReadyDate = dto.ReadyDate ?? offer.ReadyDate;
            if (dto.UnitPrice.HasValue) offer.UnitPrice = dto.UnitPrice.Value;
            offer.Packaging = dto.Packaging ?? offer.Packaging;
            offer.Location = dto.Location ?? offer.Location;
            offer.Status = dto.Status ?? offer.Status;

            await _context.SaveChangesAsync();

            return Ok(offer);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null)
                return NotFound();

            offer.Status = "حذف‌شده";
            await _context.SaveChangesAsync();

            return Ok(new { message = "عرضه با موفقیت حذف شد" });
        }
    }

    public class OfferCreateDto
    {
        public Guid? SellerId { get; set; }
        public string Product { get; set; } = string.Empty;
        public string? Variety { get; set; }
        public decimal QuantityKg { get; set; }
        public string? Quality { get; set; }
        public DateTime? ReadyDate { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Packaging { get; set; }
        public string? Location { get; set; }
    }

    public class OfferUpdateDto
    {
        public string? Variety { get; set; }
        public decimal? QuantityKg { get; set; }
        public string? Quality { get; set; }
        public DateTime? ReadyDate { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Packaging { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
    }
}
