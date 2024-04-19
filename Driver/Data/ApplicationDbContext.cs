using EthernetGlobalData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ThreadSafe;

namespace EthernetGlobalData.Data
{
    public class ApplicationDbContext : ThreadSafeDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Node> Node { get; set; }
        public DbSet<Point> Point { get; set; }
        public DbSet<Channel> Channel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Point>().ToTable("Points");
            modelBuilder.Entity<Channel>().ToTable("Channels");
        }
    }
}
