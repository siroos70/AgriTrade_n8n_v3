using AgriTrade.Backend.Data;

namespace AgriTrade.Backend.Services
{
    public interface IMatchingService
    {
        Task<List<Match>> RunMatchingAsync();
        Task<List<Match>> GetMatchesAsync(Guid? offerId = null, Guid? demandId = null);
    }

    public class MatchingService : IMatchingService
    {
        private readonly AgriTradeContext _context;

        public MatchingService(AgriTradeContext context)
        {
            _context = context;
        }

        public async Task<List<Match>> RunMatchingAsync()
        {
            var offers = await _context.Offers
                .Include(o => o.Seller)
                .Where(o => o.Status == "فعال")
                .ToListAsync();

            var demands = await _context.Demands
                .Include(d => d.Buyer)
                .Where(d => d.Status == "فعال")
                .ToListAsync();

            var matches = new List<Match>();

            foreach (var offer in offers)
            {
                foreach (var demand in demands)
                {
                    if (offer.Product != demand.Product)
                        continue;

                    var existingMatch = await _context.Matches
                        .FirstOrDefaultAsync(m => m.OfferId == offer.Id && m.DemandId == demand.Id);

                    if (existingMatch != null)
                        continue;

                    var score = CalculateMatchScore(offer, demand);
                    if (score >= 70)
                    {
                        var match = new Match
                        {
                            Id = Guid.NewGuid(),
                            OfferId = offer.Id,
                            DemandId = demand.Id,
                            Score = score,
                            Status = "جدید",
                            CreatedAt = DateTime.UtcNow,
                            Reasons = System.Text.Json.Nodes.JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(new
                            {
                                روش = "تطبیق بر اساس محصول، مقدار، قیمت، زمان و کیفیت",
                                score = score
                            }))
                        };

                        _context.Matches.Add(match);
                        await _context.SaveChangesAsync();
                        matches.Add(match);
                    }
                }
            }

            return matches;
        }

        private decimal CalculateMatchScore(Offer offer, Demand demand)
        {
            decimal score = 0;

            // Product match (40 points)
            if (offer.Product == demand.Product)
                score += 40;

            // Variety match (bonus 5 points)
            if (!string.IsNullOrEmpty(offer.Variety) && !string.IsNullOrEmpty(demand.Variety) &&
                offer.Variety == demand.Variety)
                score += 5;

            // Quantity match (20 if offer >= demand, 10 otherwise)
            if (offer.QuantityKg >= demand.QuantityKg)
                score += 20;
            else
                score += 10;

            // Price match (20 points)
            if (demand.MaxUnitPrice.HasValue && offer.UnitPrice.HasValue)
            {
                if (offer.UnitPrice <= demand.MaxUnitPrice)
                    score += 20;
            }

            // Date match (10 points)
            if (demand.NeededDate.HasValue && offer.ReadyDate.HasValue)
            {
                if (offer.ReadyDate <= demand.NeededDate)
                    score += 10;
            }

            // Quality match (10 points)
            if (!string.IsNullOrEmpty(demand.MinQuality) && !string.IsNullOrEmpty(offer.Quality))
            {
                if (offer.Quality == demand.MinQuality)
                    score += 10;
            }

            return score;
        }

        public async Task<List<Match>> GetMatchesAsync(Guid? offerId = null, Guid? demandId = null)
        {
            var query = _context.Matches
                .Include(m => m.Offer)
                .Include(m => m.Demand)
                .AsQueryable();

            if (offerId.HasValue)
                query = query.Where(m => m.OfferId == offerId.Value);

            if (demandId.HasValue)
                query = query.Where(m => m.DemandId == demandId.Value);

            return await query.OrderByDescending(m => m.Score).ToListAsync();
        }
    }
}
