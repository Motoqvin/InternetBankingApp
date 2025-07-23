using System.Reflection;
using InterBanking.Api.Data;
using Api.Repositories;
using InterBanking.Api.Repositories.Base;
using InterBanking.Api.Services;
using InterBanking.Api.Services.Base;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IInterBankingService, InterBankingService>();
builder.Services.AddScoped<IInterBankingRepository, InterBankingRepository>();

builder.Services.AddAutoMapper((cfg) =>
{
    cfg.AddMaps(Assembly.GetExecutingAssembly());
});

builder.Services.AddDbContext<InterDbContext>(options =>
{
    options.UseNpgsql( "Host=db;Port=5432;Database=postgres;Username=postgres;Password=mysecretpassword;");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();

