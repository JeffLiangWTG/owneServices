using System;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	public class NullMessageRepository : IMessageRepository
	{
		readonly IMessageRepository decoratedRepository;

		readonly static Message nullMessage = new Message
		{
			From = "foo",
			To = "bar",
			Name = "foo-bar",
			IsCompressed = false,
			Body = new byte[16]
		};

		public NullMessageRepository(IMessageRepository decoratedRepository)
		{
			this.decoratedRepository = decoratedRepository;
		}

		public Task<Message> RetrieveAsync(Guid id)
		{
			return Task.FromResult(nullMessage);
		}

		public Task<Guid> StoreAsync(Message message)
		{
			return Task.FromResult(Guid.NewGuid());
		}
	}
}
