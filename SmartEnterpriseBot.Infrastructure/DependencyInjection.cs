using Microsoft.Extensions.DependencyInjection;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Infrastructure.Servicees;
using SmartEnterpriseBot.Infrastructure.Services;
using SmartEnterpriseBot.Infrastructure.Services.Storage;

namespace SmartEnterpriseBot.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IAiAnswerService, AiAnswerService>();
            services.AddScoped<IKnowledgeService, KnowledgeService>();
            services.AddScoped<ISearchIndexerService, SearchIndexerService>();

            return services;
        }
    }
}
