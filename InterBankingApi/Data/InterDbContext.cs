using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class InterDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<QrToken> QrTokens { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Server=localhost;Database=BankDb;Trusted_Connection=True;");
}