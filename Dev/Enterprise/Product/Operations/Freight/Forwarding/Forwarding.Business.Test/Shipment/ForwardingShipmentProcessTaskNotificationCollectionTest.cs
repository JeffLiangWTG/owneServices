using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentProcessTaskNotificationCollection))]
	sealed class ForwardingShipmentProcessTaskNotificationCollectionTest : ActiveBusinessObjectCollectionTestCase<ForwardingShipmentProcessTaskNotificationCollection>
	{
		protected override ForwardingShipmentProcessTaskNotificationCollection GetCollectionToTest()
		{
			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			return new ForwardingShipmentProcessTaskNotificationCollection(forwardingShipment.WorkflowItems.Triggers.AddNew());
		}
	}
}
