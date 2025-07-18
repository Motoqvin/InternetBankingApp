using Api.Data;
using Api.Repositories;
using Api.Repositories.Base;
using Api.Services;
using Api.Services.Base;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IInterBankingService, InterBankingService>();
builder.Services.AddScoped<IInterBankingRepository, InterBankingRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InterDbContext>();

var app = builder.Build();


app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();

