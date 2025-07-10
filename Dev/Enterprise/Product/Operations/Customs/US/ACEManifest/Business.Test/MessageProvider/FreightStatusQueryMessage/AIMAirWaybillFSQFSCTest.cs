using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMAirWaybillFSQFSCTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "SHA-123456789";
			var bill = (AsycudaBill)header.MasterBill;
			var airWaybillFSQFSC = new AIMAirWaybillForFSQ(bill, null);
			AssertEquals("SHA", airWaybillFSQFSC.AirWaybillPrefix);
			AssertEquals("12345678", airWaybillFSQFSC.AWBSerialNumber);
			AssertNullOrEmpty("HAWBNumber empty for master bill", airWaybillFSQFSC.HAWBNumber);
			AssertEquals("", airWaybillFSQFSC.PartArrivalReference);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "H0035";
			var arrival = header.ArrivalHeaders.AddNew();
			arrival.ATH_Reference = "B";
			var airWaybillFSQFSC1 = new AIMAirWaybillForFSQ(bill1, arrival);
			AssertEquals("SHA", airWaybillFSQFSC1.AirWaybillPrefix);
			AssertEquals("12345678", airWaybillFSQFSC1.AWBSerialNumber);
			AssertEquals("H0035", airWaybillFSQFSC1.HAWBNumber);
			AssertEquals("B", airWaybillFSQFSC1.PartArrivalReference);
		}
	}
}
