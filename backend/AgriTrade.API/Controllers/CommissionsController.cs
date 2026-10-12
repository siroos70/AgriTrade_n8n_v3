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
    public class CommissionsController : ControllerBase
    {
        private readonly ICommissionService _commissionService;

        public CommissionsController(ICommissionService commissionService)
        {
            _commissionService = commissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Commission>>> Get()
        {
            var commissions = await _commissionService.GetCommissionsAsync();
            return Ok(commissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Commission>> Get(Guid id)
        {
            var commission = await _commissionService.GetCommissionAsync(id);
            if (commission == null)
                return NotFound();

            return Ok(commission);
        }

        [HttpPost]
        public async Task<ActionResult<Commission>> Post([FromBody] CommissionCreateDto dto)
        {
            var commission = await _commissionService.CalculateCommissionAsync(dto.DealId, dto.Rate);
            return CreatedAtAction(nameof(Get), new { id = commission.Id }, commission);
        }
    }

    public class CommissionCreateDto
    {
        public Guid DealId { get; set; }
        public decimal? Rate { get; set; }
        public string? CommissionType { get; set; }
    }
}
