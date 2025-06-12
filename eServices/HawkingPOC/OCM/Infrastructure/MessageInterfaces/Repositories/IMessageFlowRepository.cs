using System;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public interface IMessageFlowRepository : IRepository<MessageFlow>
	{
		Task<MessageFlow> AddEventAsync(int messageFlowId, MessageEvent messageEvent);
		Task<MessageFlow> GetMessageFlowAsync(string sender, string recipient, Guid initialMessageId);
	}
}
