using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaTransferBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestManifestBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill1 = header.Bills.AddNew();
			houseBill1.ABL_BillNumber = "HB001";
			houseBill1.ABL_GoodsDescription = "Bill 1";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillNumber = "HB001";
			var transferBillLookups = transferBill.Lookups;
			Factory.Save();
			var manifestBillsList = transferBillLookups.ManifestBillsList;
			AssertSame("is cached", manifestBillsList, transferBillLookups.ManifestBillsList);
			var masterbill = manifestBillsList["MAN001"];
			AssertEquals("masterbill.Description", "", masterbill.Description);
			AssertEquals("masterbill.PK", header.MasterBill.PK, masterbill.PK);
			var bill1 = manifestBillsList["HB001"];
			AssertEquals("bill1.Description", "Bill 1", bill1.Description);
			AssertEquals("bill1.PK", houseBill1.PK, bill1.PK);
			var houseBill2 = header.Bills.AddNew();
			houseBill2.ABL_BillNumber = "HB002";
			houseBill2.ABL_GoodsDescription = "Bill 2";
			ICodeDescription bill2 = null;
			AssertNoExceptionThrown("Change causes a rebuild of the list.", () => bill2 = transferBillLookups.ManifestBillsList["HB002"]);
			AssertEquals("bill2.Description", "Bill 2", bill2.Description);
			AssertEquals("bill2.PK", houseBill2.PK, bill2.PK);
			houseBill2.ABL_BillNumber = "HB002B";
			AssertNoExceptionThrown("Change causes a rebuild of the list.", () => bill2 = transferBillLookups.ManifestBillsList["HB002B"]);
			AssertEquals("bill2.Description", "Bill 2", bill2.Description);
			AssertEquals("bill2.PK", houseBill2.PK, bill2.PK);
		}

		public void TestTransferMessageStatusList()
		{
			var transferBill = Factory.New<AsycudaTransferBill>();
			var transferBillLookups = new AsycudaTransferBillLookups(transferBill);
			AssertType<AIMTransferStatusCodes>(transferBillLookups.TransferMessageStatusList);
		}
	}
}
