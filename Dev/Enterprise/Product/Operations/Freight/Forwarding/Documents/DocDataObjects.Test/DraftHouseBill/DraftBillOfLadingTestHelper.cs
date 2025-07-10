using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	public static class DraftBillOfLadingTestHelper
	{
		public static IEDIMessage CreateDraftBillOfLadingMessage(this BusinessObjectFactory factory, string draftBillOfLadingUXML = "")
		{
			var builder = new InterchangeBuilder(factory);

			var uXML = draftBillOfLadingUXML.WrapInInterchange();

			if (!builder.TryParseUniversalXml(uXML, InterchangeBuilder.MessageDirection.Receive, out var interchange))
			{
				throw new InvalidOperationException("Interchange could not be created for draft bill of lading message");
			}

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			messageQuery.FetchOnlyFromLocalCache = true;

			var message = factory.Load<IEDIMessage>(messageQuery).Single();
			message.EM_Status = EDIMessageStatusList.Codes.Linked;

			return message;
		}
	}
}
