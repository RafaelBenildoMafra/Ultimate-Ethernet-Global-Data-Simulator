using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Models;
using System.Diagnostics;

namespace EthernetGlobalData.Data
{
    public class ProtocolContext : DbContext
    {
        public ProtocolContext(DbContextOptions<ProtocolContext> options)
            : base(options)
        {
        }

        //public DbSet<Node> Node { get; set; } = default!;
        //public DbSet<Point> Point { get; set; } = default!;

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // configures one-to-many relationship
        //    modelBuilder.Entity<Point>()
        //        .HasOne<Node>(s => s.Node)
        //        .WithMany(g => g.Points)
        //        .HasForeignKey(s => s.Node);
        //}
        //

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
