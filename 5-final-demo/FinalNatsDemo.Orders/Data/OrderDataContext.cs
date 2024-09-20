using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Orders.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Orders.Data
{
    public class OrderDataContext : DbContext
    {
        public OrderDataContext() { }

        public OrderDataContext(DbContextOptions<OrderDataContext> options)
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

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
    }
}
