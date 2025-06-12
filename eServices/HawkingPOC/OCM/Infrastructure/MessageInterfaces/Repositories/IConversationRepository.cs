using System;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public interface IConversationRepository
	{
		Task<Conversation> CreateNewAsync(Guid messageId, string content);
	}
}
