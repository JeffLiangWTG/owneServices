using MongoDB.Driver;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	public class MongoMessageRepositoryFactory
    {
		readonly MongoDbConfig config;
		readonly MongoClient mongo;

		public MongoMessageRepositoryFactory(MongoDbConfig config)
		{
			this.config = config;
			mongo = new MongoClient(config.ToMongoUrl());
		}

		public IMessageRepository CreateRepository()
		{
			return //new NullMessageRepository(
				new CompressedMessageRepository(new MongoMessageRepository(mongo, config))
			//)
			;
		}
    }
}
