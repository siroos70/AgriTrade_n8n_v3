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
    public class MatchesController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public MatchesController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> Get()
        {
            var matches = await _matchingService.GetMatchesAsync();
            return Ok(matches);
        }

        [HttpPost("run")]
        public async Task<ActionResult<IEnumerable<Match>>> RunMatching()
        {
            var matches = await _matchingService.RunMatchingAsync();
            return Ok(matches);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> Get(Guid id)
        {
            var matches = await _matchingService.GetMatchesAsync();
            var match = matches.FirstOrDefault(m => m.Id == id);

            if (match == null)
                return NotFound();

            return Ok(match);
        }
    }
}
