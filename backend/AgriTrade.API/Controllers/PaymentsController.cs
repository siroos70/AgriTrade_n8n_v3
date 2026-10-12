using AgriTrade.Backend.Data;
using AgriTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> Get()
        {
            var payments = await _paymentService.GetPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("deal/{dealId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetByDeal(Guid dealId)
        {
            var payments = await _paymentService.GetDealPaymentsAsync(dealId);
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> Get(Guid id)
        {
            var ctx = HttpContext.RequestServices.GetRequiredService<AgriTradeContext>();
            var payment = await ctx.Payments
                .Include(p => p.Deal)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> Post([FromBody] PaymentCreateDto dto)
        {
            var payment = await _paymentService.CreatePaymentAsync(dto.DealId, dto.Amount, dto.Method);
            return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PaymentStatusUpdateDto dto)
        {
            var payment = await _paymentService.UpdatePaymentStatusAsync(id, dto.Status, dto.GatewayRef);
            return Ok(payment);
        }
    }

    public class PaymentCreateDto
    {
        public Guid DealId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
    }

    public class PaymentStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string? GatewayRef { get; set; }
    }
}
