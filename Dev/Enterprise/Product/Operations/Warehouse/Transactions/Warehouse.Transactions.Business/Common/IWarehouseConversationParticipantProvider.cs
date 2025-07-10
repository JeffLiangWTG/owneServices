using System.Collections.Generic;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWarehouseConversationParticipantProvider
	{
		IEnumerable<IConversationParticipant> GetAdditionalParticipants(WhsDocket docket, JobConversationParticipant sender);
	}
}
