using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CargoWise.eHub.DataAccess.Integration;
using System.Collections;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class InboxAccessor
	{
		public static void InsertToInboxAndOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage inboxMessage, eHubGatewayMessage outboxMessage)
		{
			inboxAccessor.InsertToInboxAndOutbox(senderID, envelopeTrackingID, inboxMessage, outboxMessage);
		}

		static IInboxAccessor inboxAccessor = CargoWise.eHub.DataAccess.Integration.DataAccessFactories.NewInboxAccessorInstance();
	}
}
