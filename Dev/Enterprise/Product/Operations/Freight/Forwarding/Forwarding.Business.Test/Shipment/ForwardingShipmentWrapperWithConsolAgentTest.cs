using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentWrapperWithConsolAgent))]
	class ForwardingShipmentWrapperWithConsolAgentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			ForwardingShipment forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S0001010";
			OrgHeader consolAgent = Factory.New<OrgHeader>();
			consolAgent.OH_FullName = "Consol Agent007";

			ForwardingShipmentWrapperWithConsolAgent shipmentWrapper = new ForwardingShipmentWrapperWithConsolAgent(forwardingShipment, consolAgent);
			AssertEquals("Same Shipment", "S0001010", shipmentWrapper.Shipment.JS_UniqueConsignRef);
			AssertEquals("Same consol agent", "Consol Agent007", shipmentWrapper.ConsolAgentForARInvoice.OH_FullName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrgHeader consolAgent = Factory.New<OrgHeader>();

			return new ForwardingShipmentWrapperWithConsolAgent(shipment, consolAgent);
		}

		#endregion
	}
}
