using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class ShipmentImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest : ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest
	{
		protected override IWorkflowProvider WorkflowProviderWithMilestoneToComplete
		{
			get
			{
				return Shipment;
			}
		}

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					Order.JD_JS = shipment.PK;
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;
	}
}
