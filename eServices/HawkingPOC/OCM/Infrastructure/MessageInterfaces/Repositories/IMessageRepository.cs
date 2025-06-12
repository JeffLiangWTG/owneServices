using System;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public interface IMessageRepository
    {
	    Task<Guid> StoreAsync(Message message);
	    Task<Message> RetrieveAsync(Guid id);
    }
}
