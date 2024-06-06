using Microsoft.EntityFrameworkCore;

namespace RouteRecorder.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Record> Records { get; set; }
        public DbSet<Route> Routes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Route>()
                .HasMany(r => r.Records)
                .WithOne(rec => rec.Route)
                .HasForeignKey(rec => rec.RouteId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
