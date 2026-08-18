using ECommerce.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImages> ProductImages => Set<ProductImages>();
        public DbSet<Style> Styles => Set<Style>();
    }
}
