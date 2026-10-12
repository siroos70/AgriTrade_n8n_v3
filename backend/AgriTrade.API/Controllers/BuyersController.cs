using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BuyersController : ControllerBase
    {
        private readonly AgriTradeContext _context;

        public BuyersController(AgriTradeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Buyer>>> Get()
        {
            var query = _context.Buyers.AsQueryable();

            if (Request.Query.ContainsKey("search"))
            {
                var search = Request.Query["search"].ToString().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(search) ||
                                         b.Phone.Contains(search) ||
                                         (b.Region != null && b.Region.ToLower().Contains(search)));
            }

            if (Request.Query.ContainsKey("status"))
            {
                var status = Request.Query["status"].ToString();
                query = query.Where(b => b.Status == status);
            }

            var skip = Request.Query.ContainsKey("skip") ? int.Parse(Request.Query["skip"]) : 0;
            var take = Request.Query.ContainsKey("take") ? int.Parse(Request.Query["take"]) : 50;

            var result = await query.OrderByDescending(b => b.CreatedAt)
                .Skip(skip).Take(take).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Buyer>> Get(Guid id)
        {
            var buyer = await _context.Buyers
                .Include(b => b.Demands)
                .Include(b => b.Deals)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (buyer == null)
                return NotFound();

            return Ok(buyer);
        }

        [HttpPost]
        public async Task<ActionResult<Buyer>> Post([FromBody] BuyerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "نام و شماره تماس الزامی است" });

            if (_context.Buyers.Any(b => b.Phone == dto.Phone))
                return Conflict(new { message = "شماره تماس قبلاً ثبت شده است" });

            var buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                Region = dto.Region,
                Notes = dto.Notes,
                Status = "فعال",
                CreatedAt = DateTime.UtcNow
            };

            _context.Buyers.Add(buyer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = buyer.Id }, buyer);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Buyer>> Put(Guid id, [FromBody] BuyerUpdateDto dto)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer == null)
                return NotFound();

            buyer.Name = dto.Name ?? buyer.Name;
            buyer.Phone = dto.Phone ?? buyer.Phone;
            buyer.Region = dto.Region ?? buyer.Region;
            buyer.Notes = dto.Notes ?? buyer.Notes;
            buyer.Status = dto.Status ?? buyer.Status;

            await _context.SaveChangesAsync();

            return Ok(buyer);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer == null)
                return NotFound();

            buyer.Status = "حذف‌شده";
            await _context.SaveChangesAsync();

            return Ok(new { message = "خریدار با موفقیت حذف شد" });
        }
    }

    public class BuyerCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? Notes { get; set; }
    }

    public class BuyerUpdateDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }
}
