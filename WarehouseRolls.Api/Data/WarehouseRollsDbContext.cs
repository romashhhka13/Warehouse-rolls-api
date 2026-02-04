using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Models;

namespace WarehouseRolls.Data
{
    public class WarehouseRollsDbContext : DbContext
    {
        public WarehouseRollsDbContext(DbContextOptions<WarehouseRollsDbContext> options)
            : base(options) { }

        // Таблица - всего одна таблица
        public DbSet<Roll> Rolls { get; set; }

        // Настройка схемы
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var roll = modelBuilder.Entity<Roll>();

            roll.ToTable("rolls").HasKey(r => r.Id);

            roll.Property(r => r.Id)
                .HasColumnName("id");

            roll.Property(r => r.Length)
                .HasColumnName("length")
                .IsRequired();

            roll.Property(r => r.Weight)
                .HasColumnName("weight")
                .IsRequired();

            roll.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            roll.Property(r => r.DeletedAt)
                .HasColumnName("deleted_at");

            roll.HasIndex(r => r.CreatedAt);
            roll.HasIndex(r => r.DeletedAt);

            // PostgreSQL constraints
            roll.ToTable("rolls", t =>
            {
                t.HasCheckConstraint("ck_roll_length_positive", "length > 0");
                t.HasCheckConstraint("ck_roll_weight_positive", "weight > 0");
            });
        }
    }
}