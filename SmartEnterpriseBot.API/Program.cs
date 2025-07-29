using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
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

builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>()
        .GetConnectionString("BlobConnectionString");
    return new BlobServiceClient(connectionString);
});

builder.Services.AddSingleton<BlobContainerClient>(provider =>
{
    var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
    var containerName = provider.GetRequiredService<IConfiguration>()["AzureBlobStorage:ContainerName"];
    return blobServiceClient.GetBlobContainerClient(containerName);
});

builder.Services.AddSingleton<SearchClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureCognitiveSearch:SearchServiceEndpoint"];
    var apiKey = config["AzureCognitiveSearch:SearchApiKey"];
    var indexName = config["AzureCognitiveSearch:IndexName"];
    return new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(apiKey));
});

builder.Services.AddSingleton<OpenAIClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureOpenAI:Endpoint"];
    var key = config["AzureOpenAI:Key"];
    return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});

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