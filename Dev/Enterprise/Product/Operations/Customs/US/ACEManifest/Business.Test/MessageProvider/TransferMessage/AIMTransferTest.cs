using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMTransferTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HAWB001";
			var arrHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transfer = arrHeader.TransferHeaders.AddNew();
			transfer.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer.ATF_TransferType = "D";
			transfer.ATF_CarrierID = "13-1502798000";
			transfer.ATF_OnwardCarrier = "US01";
			transfer.ATF_DestinationWarehouseID = "1234";
			var transferBill = transfer.TransferBills.AddNew();
			transferBill.ATB_ABL_Bill = manifestHeader.MasterBill.PK;
			transferBill.AllocateInBondNumber("TST0001");
			var aimTransfer1 = new AIMTransfer(transferBill);
			AssertEquals("LAX", aimTransfer1.DestinationAirport);
			AssertEquals("D", aimTransfer1.DomesticInternationalIdentifier);
			AssertEquals("13-1502798000", aimTransfer1.BondedCarrierIDOrOnwardCarrier);
			AssertEquals(transferBill.InBondNumber, aimTransfer1.BondedPremisesIdentifierOrInbondControlNumber);
			transfer.ATF_CarrierID = "";
			transferBill.InBondNumber = "";
			var aimTransfer2 = new AIMTransfer(transferBill);
			AssertEquals("LAX", aimTransfer2.DestinationAirport);
			AssertEquals("D", aimTransfer2.DomesticInternationalIdentifier);
			AssertEquals("US01", aimTransfer2.BondedCarrierIDOrOnwardCarrier);
			AssertEquals("1234", aimTransfer2.BondedPremisesIdentifierOrInbondControlNumber);
			var aimTransfer3 = new AIMTransfer(null);
			AssertEquals("000", aimTransfer3.DestinationAirport);
			AssertEquals("", aimTransfer3.DomesticInternationalIdentifier);
			AssertEquals("", aimTransfer3.BondedCarrierIDOrOnwardCarrier);
			AssertEquals("", aimTransfer3.BondedPremisesIdentifierOrInbondControlNumber);
		}
	}
}
