using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentGatewayTest : TestCaseWithFactory
	{
		#region JSG_OA_ForwarderAddress

		public void TestForwarder()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var shipmentGateway = Factory.New<ShipmentGateway>();

			shipmentGateway.JSG_OA_ForwarderAddress = org.MainAddress.PK;

			AssertEquals(org.PK, shipmentGateway.Forwarder.PK);
		}

		#endregion

		#region JSG_Sequence

		public void TestJSG_Sequence_ReadOnly()
		{
			var shipmentGateway = Factory.New<ShipmentGateway>();
			Assert(shipmentGateway.JSG_SequenceInfo.ReadOnly);
		}

		#endregion

		#region UniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<ForwardingShipment>(shipment1.PK);

			var gateway1 = shipment1.Gateways.AddNew();
			gateway1.JSG_OA_ForwarderAddress = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A")).MainAddress.PK;

			var gateway2 = shipment2.Gateways.AddNew();
			gateway2.JSG_OA_ForwarderAddress = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B")).MainAddress.PK;

			AssertEquals("Precondition: gateway1", (byte)1, gateway1.JSG_Sequence);
			AssertEquals("Precondition: gateway2", (byte)1, gateway2.JSG_Sequence);

			Factory.Save();
			Assert("Shipment 1 can be saved", true);

			try
			{
				factory2.Save();
				Fail("Should not be able to save Shipment 2");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals(shipment1.HumanReadableName + " cannot be saved because its Gateways have been modified by another user. Please close and re-open the Shipment to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
