using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ForwardingShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestGetISFHouseBill()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ams = shipment.Numbers.AddNew();
			ams.CE_EntryNum = "AMS00001";
			ams.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			AssertEquals("ISF house bill", "AMS00001", shipment.GetUpperCaseAMSBill());
			ams.CE_EntryNum = "ams00001";
			AssertEquals("ISF house bill", "AMS00001", shipment.GetUpperCaseAMSBill());
		}
	}
}
