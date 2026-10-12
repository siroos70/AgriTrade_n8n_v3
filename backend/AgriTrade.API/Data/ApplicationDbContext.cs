using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriTrade.Backend.Data
{
    public class AgriTradeContext : IdentityDbContext
    {
        public AgriTradeContext(DbOptions<AgriTradeContext> options) : base(options) { }

        public virtual DbSet<Seller> Sellers { get; set; } = null!;
        public virtual DbSet<Buyer> Buyers { get; set; } = null!;
        public virtual DbSet<Offer> Offers { get; set; } = null!;
        public virtual DbSet<Demand> Demands { get; set; } = null!;
        public virtual DbSet<Match> Matches { get; set; } = null!;
        public virtual DbSet<Deal> Deals { get; set; } = null!;
        public virtual DbSet<Commission> Commissions { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnConfiguring(DbOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=agritrade_db;Username=agritrade;Password=***", b => b.UseNetTopologySuite());
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Sellers
            builder.Entity<Seller>(e =>
            {
                e.HasIndex(s => new { s.Status });
                e.HasIndex(s => new { s.Phone }).IsUnique();
                e.Property(s => s.Name).IsRequired();
            });

            // Buyers
            builder.Entity<Buyer>(e =>
            {
                e.HasIndex(b => new { b.Status });
                e.HasIndex(b => new { b.Phone }).IsUnique();
                e.Property(b => b.Name).IsRequired();
            });

            // Offers
            builder.Entity<Offer>(e =>
            {
                e.HasIndex(o => new { o.Product, o.Status });
                e.HasOne(o => o.Seller).WithOne(s => s.Offers).OnDelete(DeleteBehavior.SetNull);
                e.Property(o => o.QuantityKg).HasPrecision(18, 3);
                e.Property(o => o.UnitPrice).HasPrecision(18, 2);
                e.Property(o => o.Product).IsRequired();
            });

            // Demands
            builder.Entity<Demand>(e =>
            {
                e.HasIndex(d => new { d.Product, d.Status });
                e.HasOne(d => d.Buyer).WithOne(b => b.Demands).OnDelete(DeleteBehavior.SetNull);
                e.Property(d => d.QuantityKg).HasPrecision(18, 3);
                e.Property(d => d.MaxUnitPrice).HasPrecision(18, 2);
                e.Property(d => d.Product).IsRequired();
            });

            // Matches
            builder.Entity<Match>(e =>
            {
                e.HasIndex(m => new { m.OfferId, m.DemandId }).IsUnique();
                e.HasIndex(m => new { m.Status });
                e.HasOne(m => m.Offer).WithOne(o => o.Matches).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(m => m.Demand).WithOne(d => d.Matches).OnDelete(DeleteBehavior.SetNull);
                e.Property(m => m.Score).HasPrecision(18, 2);
            });

            // Deals
            builder.Entity<Deal>(e =>
            {
                e.HasIndex(d => new { d.Status });
                e.HasIndex(d => new { d.Product });
                e.HasOne(d => d.Seller).WithOne(s => s.Deals).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(d => d.Buyer).WithOne(b => b.Deals).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(d => d.Match).WithOne(m => m.Deals).OnDelete(DeleteBehavior.SetNull);
                e.Property(d => d.QuantityKg).HasPrecision(18, 3);
                e.Property(d => d.UnitPrice).HasPrecision(18, 2);
                e.Property(d => d.TotalValue).HasPrecision(18, 2);
            });

            // Commissions
            builder.Entity<Commission>(e =>
            {
                e.HasOne(c => c.Deal).WithOne(d => d.Commissions).OnDelete(DeleteBehavior.SetNull);
                e.Property(c => c.Amount).HasPrecision(18, 2);
                e.Property(c => c.Rate).HasPrecision(18, 4);
            });

            // Payments
            builder.Entity<Payment>(e =>
            {
                e.HasOne(p => p.Deal).WithOne(d => d.Payments).OnDelete(DeleteBehavior.SetNull);
                e.Property(p => p.Amount).HasPrecision(18, 2);
            });

            // AuditLogs
            builder.Entity<AuditLog>(e =>
            {
                e.HasIndex(a => new { a.Entity, a.CreatedAt });
                e.HasIndex(a => new { a.Actor });
            });
        }
    }
}