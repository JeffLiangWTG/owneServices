using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMAirWaybillTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new AIMAirWaybill(null));
		}

		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "SHA-123456789";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "H0035";
			var aimAirWaybill = new AIMAirWaybill(bill);
			AssertEquals("SHA", aimAirWaybill.AirWaybillPrefix);
			AssertEquals("12345678", aimAirWaybill.AWBSerialNumber);
			Assert(!aimAirWaybill.IsMasterAirWaybill);
			AssertEquals("H0035", aimAirWaybill.HAWBNumber);
			AssertEquals("", aimAirWaybill.PackageTrackingIdentifier);
			AssertEquals("", aimAirWaybill.PartArrivalReference);
		}

		public void TestHAWBNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "SHA-123456789";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "H0035";
			var aimAirWaybill = new AIMAirWaybill(bill);
			var aimAirWaybillForMasterBill = new AIMAirWaybill((AsycudaBill)header.MasterBill);
			AssertEquals("H0035", aimAirWaybill.HAWBNumber);
			AssertEquals("", aimAirWaybillForMasterBill.HAWBNumber);
		}
	}
}
