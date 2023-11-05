using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() 
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<Metrica> Metrics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
