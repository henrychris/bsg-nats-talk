using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Shipping.Data.Entities;
using FinalNatsDemo.Shipping.Data.Entities.External;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Shipping.Data
{
    public class ShippingDataContext : DbContext
    {
        public ShippingDataContext() { }

        public ShippingDataContext(DbContextOptions<ShippingDataContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // we don't want ef core SQL logs.
            optionsBuilder.UseLoggerFactory(
                LoggerFactory.Create(builder =>
                    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Error)
                )
            );
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken token = default)
        {
            foreach (
                var entity in ChangeTracker
                    .Entries()
                    .Where(x => x.Entity is BaseEntity && x.State == EntityState.Modified)
                    .Select(x => x.Entity)
                    .Cast<BaseEntity>()
            )
            {
                entity.DateUpdated = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, token);
        }

        public DbSet<ShippingRecord> ShippingRecords { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
    }
}
