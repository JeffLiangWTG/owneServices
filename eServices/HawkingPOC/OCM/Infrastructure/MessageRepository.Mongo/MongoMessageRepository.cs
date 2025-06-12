using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	public class MongoMessageRepository : IMessageRepository
	{
		IMongoDatabase database;
		IMongoCollection<Message> messages;

		public MongoMessageRepository(IMongoClient mongo, MongoDbConfig config)
		{
			database =  mongo.GetDatabase(config.DatabaseName);
			messages = database.GetCollection<Message>(config.MessagesCollection);
		}

		public async Task<Message> RetrieveAsync(Guid id)
		{
			Console.WriteLine($"Retrieving message {id}");

			var result = await messages.FindAsync(m => m.Id == id).ConfigureAwait(false);
			return await result.FirstAsync();
		}

		public async Task<Guid> StoreAsync(Message message)
		{
			await messages.InsertOneAsync(message).ConfigureAwait(false);
			Console.WriteLine($"Stored message {message.Id}");
			return message.Id;
		}
	}
}
