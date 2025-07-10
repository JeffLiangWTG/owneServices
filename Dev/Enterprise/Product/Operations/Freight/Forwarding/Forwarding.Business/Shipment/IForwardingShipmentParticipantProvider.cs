using System.Collections.Generic;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IForwardingShipmentParticipantProvider
	{
		IEnumerable<IConversationParticipant> GetAdditionalParticipants(ForwardingShipment shipment, JobConversationParticipant sender);
	}
}
