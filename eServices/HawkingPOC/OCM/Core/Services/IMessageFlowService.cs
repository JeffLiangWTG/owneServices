using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Core.Services
{
	public interface IMessageFlowService
	{
		Task<MessageFlow> CreateNewFlowAsync(Message message);
		Task<MessageFlow> GetFlowAsync(int id);
	}
}
