
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentBasicRegistrationControlJobTest : ShipmentBasicRegistrationControlJobTestBase
	{
		protected override void TestAutoCreateRegistryIsOn(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			AssertNotNull(shipment.ShipmentJobHeader);
			AssertNull(form.ShipmentBasicRegistrationControl.JobHandler.InitializationMessage);
		}

		protected override void TestAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			AssertNull(shipment.ShipmentJobHeader);
			AssertEquals("The registry item [TESTLOCATION] has been set so that billing jobs will only be created upon entry to the Billing tab.", form.ShipmentBasicRegistrationControl.JobHandler.InitializationMessage);
		}

		protected override void TestJobExistsAndAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment)
		{
			AssertNotNull(shipment.ShipmentJobHeader);
			AssertNull(form.ShipmentBasicRegistrationControl.JobHandler.InitializationMessage);
		}
	}
}
