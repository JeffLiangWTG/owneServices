using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Repositories
{
	public class MessageFlowRepository : BaseRepository<MessageFlow>, IMessageFlowRepository
	{
		public MessageFlowRepository(IContextFactory<MessageHistoryContext> contextFactory)
			: base(contextFactory)
		{
		}

		protected override DbSet<MessageFlow> GetDbSet(MessageHistoryContext context) => context.Flows;

		public override async Task<MessageFlow> GetByIdAsync(int id)
		{
			using (var context = CreateContext())
			{
				var flow = await GetDbSet(context).FindAsync(id).ConfigureAwait(false);

				if (flow == null) { return null; }

				await context.Entry(flow)
					.Collection(f => f.Events)
					.Query()
					.OrderBy(e => e.Timestamp)
					.LoadAsync()
					.ConfigureAwait(false);

				return flow;
			}
		}

		public async Task<MessageFlow> AddEventAsync(int messageFlowId, MessageEvent messageEvent)
		{
			using (var context = CreateContext())
			{
				var messageFlow = await context.Flows
											   .Include(f => f.Events)
											   .SingleOrDefaultAsync(mf => mf.Id == messageFlowId)
											   .ConfigureAwait(false);

				messageFlow.AddEvent(messageEvent);
				await context.SaveChangesAsync().ConfigureAwait(false);

				return messageFlow;
			}
		}

		public async Task<MessageFlow> GetMessageFlowAsync(string sender, string recipient, Guid initialMessageId)
		{
			using (var context = CreateContext())
			{
				return await context.Flows
									.Include(f => f.Conversation)
									.SingleOrDefaultAsync(mf => mf.Sender == sender &&
																mf.Recipient == recipient &&
																mf.Conversation.InitialMessageId == initialMessageId)
									.ConfigureAwait(false);
			}
		}
	}
}
