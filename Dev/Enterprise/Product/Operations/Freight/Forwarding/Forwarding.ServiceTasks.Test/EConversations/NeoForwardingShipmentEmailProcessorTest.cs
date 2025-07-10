using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.ServiceTasks.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing
{
	public class NeoForwardingShipmentEmailProcessorTest : NeoBusinessObjectEmailProcessorTest<ForwardingShipment>
	{
		protected override NeoBusinessObjectEmailProcessor<ForwardingShipment> GetProcessor() => new NeoForwardingShipmentEmailProcessor();

		protected override ForwardingShipment GetExistingBusinessObject() => existingShipment;

		protected override string ExpectedEmailTypeName => "Shipments";

		protected override void SetUp()
		{
			existingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			base.SetUp();
		}
		ForwardingShipment existingShipment;
	}
}
