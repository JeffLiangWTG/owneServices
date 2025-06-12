using System;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using Polly;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public class RetryingMessageFlowRespository : RetryingRepository<MessageFlow>, IMessageFlowRepository
	{
		readonly IMessageFlowRepository repository;

		public RetryingMessageFlowRespository(IAsyncPolicy policy, IMessageFlowRepository repository) 
			: base(policy, repository)
		{
			this.repository = repository;
		}

		public async Task<MessageFlow> AddEventAsync(int messageFlowId, MessageEvent messageEvent)
		{
			return await Policy.ExecuteAsync(
				async () => await repository.AddEventAsync(messageFlowId, messageEvent));
		}

		public async Task<MessageFlow> GetMessageFlowAsync(string sender, string recipient, Guid initialMessageId)
		{
			return await Policy.ExecuteAsync(
				async () => await repository.GetMessageFlowAsync(sender, recipient, initialMessageId));
		}
	}
}
