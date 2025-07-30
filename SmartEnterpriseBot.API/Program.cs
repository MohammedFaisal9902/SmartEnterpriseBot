using Microsoft.EntityFrameworkCore;
using SmartEnterpriseBot.API.Configuration;
using SmartEnterpriseBot.API.DataSeed;
using SmartEnterpriseBot.API.Middleware.MiddlewareExtensions;
using SmartEnterpriseBot.Infrastructure;
using SmartEnterpriseBot.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureIdentityOptions();

builder.Services.AddAzureServices(builder.Configuration);

builder.Services.AddSingleton(provider =>
{
    return builder.Configuration["AzureOpenAI:Deployment"] ?? "gpt-35-turbo";
});

builder.Services.AddMemoryCache();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddInfrastructureServices();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminUserAsync(scope.ServiceProvider);
}
app.UseHsts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartEnterpriseBot API V1");
    });
}

app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();