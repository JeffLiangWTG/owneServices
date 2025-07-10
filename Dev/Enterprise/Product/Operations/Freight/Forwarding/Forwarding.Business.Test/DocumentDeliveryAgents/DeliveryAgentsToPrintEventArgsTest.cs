using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DeliveryAgentsToPrintEventArgsTest : TestCaseWithFactory
	{
		public void TestDeliveryAgentsToPrintEventArgs()
		{
			DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFromForPrinting = new DeliveryAgentToSelectFromForPrintingCollection(Factory);
			DeliveryAgentsToPrintEventArgs e = new DeliveryAgentsToPrintEventArgs(deliveryAgentsToSelectFromForPrinting);
			AssertNotNull("Delivery Agents to select from for printing is not null", e.DeliveryAgentsToSelectFrom);
			AssertNotNull("Delivery Agents to print is not null", e.DeliveryAgentsToPrint);
			AssertEquals("Cancel to print is false", false, e.CancelToPrint);
		}
	}
}
