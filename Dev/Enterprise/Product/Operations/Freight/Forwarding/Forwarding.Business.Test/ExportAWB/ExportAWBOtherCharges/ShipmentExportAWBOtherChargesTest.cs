using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentExportAWBOtherCharges))]
	sealed class ShipmentExportAWBOtherChargesTest : ExportAWBOtherChargesTest
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestDefaultValues()
		{
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, OtherCharges.EO_PPDCLT);
		}

		public void TestGetNewValidation()
		{
			AssertEquals("Type of Validation", typeof(ShipmentExportAWBOtherChargesValidation), OtherCharges.Validation.GetType());
		}

		public void TestHumanReadableName()
		{
			AssertEquals("House Air Waybill Other Charge", OtherCharges.HumanReadableName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ShipmentExportAWBOtherCharges>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var header = factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			header.ForceSavingByFactory = true;
			header.EH_ParentID = shipment.PK;

			var result = factory.NewWithValidTestData<ShipmentExportAWBOtherCharges>();
			result.EO_EH = header.PK;

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			OtherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			OtherCharges.EO_EH = Factory.New<ShipmentExportAWBHeader>().PK;
		}

		#endregion
	}
}
