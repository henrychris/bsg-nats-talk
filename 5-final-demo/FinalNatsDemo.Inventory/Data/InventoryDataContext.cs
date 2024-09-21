using FinalNatsDemo.Common.Data.Entities.Base;
using FinalNatsDemo.Inventory.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinalNatsDemo.Inventory.Data
{
    public class InventoryDataContext : DbContext
    {
        public InventoryDataContext() { }

        public InventoryDataContext(DbContextOptions<InventoryDataContext> options)
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

        public DbSet<Product> Products { get; set; } = null!;
    }
}
