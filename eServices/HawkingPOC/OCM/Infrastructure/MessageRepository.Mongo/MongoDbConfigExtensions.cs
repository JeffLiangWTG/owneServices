using MongoDB.Driver;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	static class MongoDbConfigExtensions
    {
		public static MongoServerAddress GetServerAddress(this MongoDbConfig config)
		{
			return new MongoServerAddress(config.Host, config.Port);
		}

		public static MongoCredential GetCredential(this MongoDbConfig config)
		{
			return MongoCredential.CreateCredential(config.AuthDb, config.Username, config.Password);
		}

		public static MongoUrl ToMongoUrl(this MongoDbConfig config)
		{
			return new MongoUrlBuilder
			{
				Server = new MongoServerAddress(config.Host, config.Port),
				Username = config.Username,
				Password = config.Password,
				AuthenticationSource = config.AuthDb,
				WaitQueueSize = 5000
			}.ToMongoUrl();
		}
	}
}
