using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaTransferBillValidationTest : TestCaseWithFactory
	{
		public void TestCheckATB_BillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill = header.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillNumber = "HB999";
			AssertHasWarningContaining(transferBill.ATB_BillNumberInfo, "The Bill Number entered does not match any Bills on this Manifest");
			transferBill.ATB_BillNumber = "HB001";
			AssertNoWarningContaining(transferBill.ATB_BillNumberInfo, "The Bill Number entered does not match any Bills on this Manifest");
			AssertNoMessageErrorContaining(transferBill.ATB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			transferBill.ATB_BillNumber = "";
			AssertHasMessageErrorContaining(transferBill.ATB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckATB_BillNumber_OneActiveTransferPerBill_SameTransfer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill1 = header.Bills.AddNew();
			houseBill1.ABL_BillNumber = "HB001";
			var houseBill2 = header.Bills.AddNew();
			houseBill2.ABL_BillNumber = "HB002";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferBill1 = transferHeader.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = "HB001";
			transferBill1.ATB_MessageStatus = "";
			AssertNoErrorContaining(transferBill1.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			var transferBill2 = transferHeader.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = "HB001";
			transferBill2.ATB_MessageStatus = "";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "ARV"; // AIMTransferStatusCodes.Arrived;
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TCD"; // AIMTransferStatusCodes.TransferCancelled;
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TSN"; // AIMTransferStatusCodes.TransferSent;
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill2.ATB_BillNumber = "HB002";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_BillNumber = "MAN001";
			AssertNoErrorContaining(transferBill1.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill2.ATB_BillNumber = "MAN001";
			transferBill1.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill1.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
		}

		public void TestCheckATB_BillNumber_OneActiveTransferPerBill_SameArrival()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill1 = header.Bills.AddNew();
			houseBill1.ABL_BillNumber = "HB001";
			var houseBill2 = header.Bills.AddNew();
			houseBill2.ABL_BillNumber = "HB002";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = "HB001";
			transferBill1.ATB_MessageStatus = "";
			var transferHeader2 = arrivalHeader1.TransferHeaders.AddNew();
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = "HB001";
			transferBill2.ATB_MessageStatus = "";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TCD";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "ARV";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TSN";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill2.ATB_BillNumber = "HB002";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.ATB_BillNumber = "MAN001";
			AssertNoErrorContaining(transferBill1.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill2.ATB_BillNumber = "MAN001";
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
			transferBill1.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill1.ATB_BillNumberInfo, "There can only be one active Transfer per Bill on the same flight.");
		}

		public void TestCheckATB_BillNumber_OneActiveTransferOnMAWB_SameTransfer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill = header.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = "MAN001";
			transferBill1.ATB_MessageStatus = "";
			var transferBill2 = transferHeader1.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = "HB001";
			transferBill2.ATB_MessageStatus = "";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TCD";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "ARV";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TSN";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
		}

		public void TestCheckATB_BillNumber_OneActiveTransferOnMAWB_SameArrival()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN001";
			var houseBill = header.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader1.TransferHeaders.AddNew();
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = "MAN001";
			transferBill1.ATB_MessageStatus = "";
			var transferHeader2 = arrivalHeader1.TransferHeaders.AddNew();
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			transferBill2.ATB_BillNumber = "HB001";
			transferBill2.ATB_MessageStatus = "";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TCD";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "ARV";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertNoErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
			transferBill1.ATB_MessageStatus = "TSN";
			transferBill2.Validation.ValidateATB_BillNumber();
			AssertHasErrorContaining(transferBill2.ATB_BillNumberInfo, "A Master Bill cannot be transferred with a House Bill on the same flight.");
		}
	}
}
