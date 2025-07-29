using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartEnterpriseBot.API.Configuration
{
    public static class AzureServiceConfiguration
    {
        public static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<BlobServiceClient>(provider =>
            {
                var connectionString = configuration.GetConnectionString("BlobConnectionString");
                return new BlobServiceClient(connectionString);
            });

            services.AddSingleton<BlobContainerClient>(provider =>
            {
                var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
                var containerName = configuration["AzureBlobStorage:ContainerName"];
                return blobServiceClient.GetBlobContainerClient(containerName);
            });

            services.AddSingleton<SearchClient>(provider =>
            {
                var endpoint = configuration["AzureCognitiveSearch:SearchServiceEndpoint"];
                var apiKey = configuration["AzureCognitiveSearch:SearchApiKey"];
                var indexName = configuration["AzureCognitiveSearch:IndexName"];
                return new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(apiKey));
            });

            services.AddSingleton<OpenAIClient>(provider =>
            {
                var endpoint = configuration["AzureOpenAI:Endpoint"];
                var key = configuration["AzureOpenAI:Key"];
                return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            });

            return services;
        }
    }
}
