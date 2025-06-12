using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageHistory.SqlServer.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddMessageHistoryServices(this IServiceCollection services, SqlServerConfig config)
		{
			services.AddSingleton<IContextFactory<MessageHistoryContext>>(sp => new SqlContextFactory(config));
			
			services.AddScoped<IMessageFlowRepository, MessageFlowRepository>();

			return services;
		}
	}
}
