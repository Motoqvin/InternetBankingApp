using InterBanking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterBanking.Api.Data;

public class InterDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<QrToken> QrTokens { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql();
    }

    public InterDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}
}