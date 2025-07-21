using InterBanking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterBanking.Api.Data;

public class InterDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<QrToken> QrTokens { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlServer("Server=127.0.0.1:5432;User Id=elvin;Password=Elv123!;Database=BankDB;Trusted_Connection=True;");
    }
}