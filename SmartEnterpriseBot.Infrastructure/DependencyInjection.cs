using Microsoft.Extensions.DependencyInjection;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Infrastructure.Servicees;
using SmartEnterpriseBot.Infrastructure.Servicees.RoleService;
using SmartEnterpriseBot.Infrastructure.Servicees.SearchService;
using SmartEnterpriseBot.Infrastructure.Services;

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
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
