using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMAirWaybillManifestTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "SHA-123456789";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_IsConsolidation = true;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "H0035";
			var aimAirWaybill = new AIMAirWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("SHA", aimAirWaybill.AirWaybillPrefix);
			AssertEquals("12345678", aimAirWaybill.AWBSerialNumber);
			Assert(aimAirWaybill.IsMasterAirWaybill);
			AssertEquals("", aimAirWaybill.HAWBNumber);
			AssertEquals("", aimAirWaybill.PackageTrackingIdentifier);
			AssertEquals("", aimAirWaybill.PartArrivalReference);
		}

		public void TestPropertiesWhenNotConsolidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "001-87289829";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_IsConsolidation = false;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "G5829";
			var aimAirWaybill = new AIMAirWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("001", aimAirWaybill.AirWaybillPrefix);
			AssertEquals("87289829", aimAirWaybill.AWBSerialNumber);
			AssertEquals("Not a consolidated master", false, aimAirWaybill.IsMasterAirWaybill);
			AssertEquals("", aimAirWaybill.HAWBNumber);
			AssertEquals("", aimAirWaybill.PackageTrackingIdentifier);
		}
	}
}
