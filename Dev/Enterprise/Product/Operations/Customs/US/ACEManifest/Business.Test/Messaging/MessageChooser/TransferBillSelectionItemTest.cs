using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class TransferBillSelectionItemTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "MAN001";
			var arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			transferHeaderSelectionItem.StatusCode = "6";
			AssertType(typeof(AsycudaTransferHeader), transferHeaderSelectionItem.TransferHeader);
			AssertEquals("6", transferHeaderSelectionItem.StatusCode);
			AssertEquals("", transferHeaderSelectionItem.SelectionDescription(true));
		}

		public void TestDefaultStatusCode()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "MAN001";
			var arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.InBondNumber = "001";
			var transferHeaderSelectionItem1 = new TransferHeaderSelectionItem(transferHeader);
			AssertEquals("StatusCode defaults to 3", AIMArrivalStatusCodes.Codes.InbondArrivedAtDestination, transferHeaderSelectionItem1.StatusCode);
			transferBill.InBondNumber = "";
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.International;
			var transferHeaderSelectionItem2 = new TransferHeaderSelectionItem(transferHeader);
			AssertEquals("StatusCode defaults to 7", AIMArrivalStatusCodes.Codes.InbondExportedAtDestination, transferHeaderSelectionItem2.StatusCode);
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferHeaderSelectionItem3 = new TransferHeaderSelectionItem(transferHeader);
			AssertEquals("StatusCode defaults to 4", AIMArrivalStatusCodes.Codes.LocalTransfer, transferHeaderSelectionItem3.StatusCode);
		}
	}
}
