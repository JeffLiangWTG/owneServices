using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageRepositoryServices(this IServiceCollection services, MongoDbConfig config)
        {
			services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(config.ToMongoUrl()));
   			services.AddScoped<IMessageRepository>(sp =>
			//new NullMessageRepository(
				new CompressedMessageRepository(
					new MongoMessageRepository(sp.GetRequiredService<IMongoClient>(), config)))
//					)
			;

			return services;
        }
    }
}
