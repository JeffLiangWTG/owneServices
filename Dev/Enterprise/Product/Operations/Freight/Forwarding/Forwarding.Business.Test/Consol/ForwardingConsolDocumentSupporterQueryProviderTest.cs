using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolDocumentSupporterQueryProviderTest : TestCaseWithFactory
	{
		public void TestConfirmBOLPrinting()
		{
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterQueryProvider();

			AssertEquals(true, queryProvider.ConfirmBOLPrinting(Factory.New<ForwardingShipment>()));
		}

		public void TestGetDeliveryAgentsToPrint()
		{
			DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom = new DeliveryAgentToSelectFromForPrintingCollection(Factory);
			DeliveryAgentToSelectFromForPrinting agent1 = deliveryAgentsToSelectFrom.AddNew();
			DeliveryAgentToSelectFromForPrinting agent2 = deliveryAgentsToSelectFrom.AddNew();
			DeliveryAgentToSelectFromForPrinting agent3 = deliveryAgentsToSelectFrom.AddNew();

			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterQueryProvider();
			DeliveryAgentOrgHeader[] deliveryAgents = queryProvider.GetDeliveryAgentsToPrint(deliveryAgentsToSelectFrom);

			AssertContainsExactElementsInAnyOrder(new[] { agent1, agent2, agent3 }, deliveryAgents);
		}

		public void TestGetImportCargoLabelToPrint()
		{
			DocumentImportCargoLabel importCargoLabel = new DocumentImportCargoLabel(Factory.New<ForwardingConsol>());
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = new ForwardingConsolDocumentSupporterQueryProvider();
			AssertEquals(importCargoLabel, queryProvider.GetImportCargoLabelToPrint(importCargoLabel));
		}
	}
}
