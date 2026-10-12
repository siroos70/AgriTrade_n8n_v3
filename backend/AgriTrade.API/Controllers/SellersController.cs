using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SellersController : ControllerBase
    {
        private readonly AgriTradeContext _context;

        public SellersController(AgriTradeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seller>>> Get()
        {
            var query = _context.Sellers.AsQueryable();

            if (Request.Query.ContainsKey("search"))
            {
                var search = Request.Query["search"].ToString().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(search) ||
                                         s.Phone.Contains(search) ||
                                         (s.Region != null && s.Region.ToLower().Contains(search)));
            }

            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                query = query.Where(s => s.Status == status);
            }

            var skip = Request.Query.ContainsKey("skip") ? int.Parse(Request.Query["skip"]) : 0;
            var take = Request.Query.ContainsKey("take") ? int.Parse(Request.Query["take"]) : 50;

            var result = await query.OrderByDescending(s => s.CreatedAt)
                .Skip(skip).Take(take).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Seller>> Get(Guid id)
        {
            var seller = await _context.Sellers
                .Include(s => s.Offers)
                .Include(s => s.Deals)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seller == null)
                return NotFound();

            return Ok(seller);
        }

        [HttpPost]
        public async Task<ActionResult<Seller>> Post([FromBody] SellerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "نام و شماره تماس الزامی است" });

            if (_context.Sellers.Any(s => s.Phone == dto.Phone))
                return Conflict(new { message = "شماره تماس قبلاً ثبت شده است" });

            var seller = new Seller
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                Region = dto.Region,
                Notes = dto.Notes,
                Status = "فعال",
                CreatedAt = DateTime.UtcNow
            };

            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = seller.Id }, seller);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Seller>> Put(Guid id, [FromBody] SellerUpdateDto dto)
        {
            var seller = await _context.Sellers.FindAsync(id);
            if (seller == null)
                return NotFound();

            seller.Name = dto.Name ?? seller.Name;
            seller.Phone = dto.Phone ?? seller.Phone;
            seller.Region = dto.Region ?? seller.Region;
            seller.Notes = dto.Notes ?? seller.Notes;
            seller.Status = dto.Status ?? seller.Status;

            await _context.SaveChangesAsync();

            return Ok(seller);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var seller = await _context.Sellers.FindAsync(id);
            if (seller == null)
                return NotFound();

            seller.Status = "حذف‌شده";
            await _context.SaveChangesAsync();

            return Ok(new { message = "فروشنده با موفقیت حذف شد" });
        }
    }

    public class SellerCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? Notes { get; set; }
    }

    public class SellerUpdateDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }
}
