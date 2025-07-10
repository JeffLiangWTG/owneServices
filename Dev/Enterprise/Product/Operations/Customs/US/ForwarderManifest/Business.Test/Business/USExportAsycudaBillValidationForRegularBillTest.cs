using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	sealed class USExportAsycudaBillValidationForRegularBillTest : AsycudaBillValidationAbstractTest
	{
		public void TestManifestUQ()
		{
			var headerOne = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var billOne = headerOne.Bills.AddNew();
			billOne.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			billOne.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertEquals(1, billOne.ABL_ManifestUQInfo.Notifications.Count());
			AssertHasMessageErrorContaining(billOne.ABL_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckITNAndExemptionCodeAndInBondNumber()
		{
			var messageError = "An AES ITN or an AES Exemption Code or an In-Bond Number must be provided";

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			((USExportAsycudaBillValdiationForMasterChild)masterBill.Validation).ValidateAESITNNumbers();
			((USExportAsycudaBillValdiationForMasterChild)masterBill.Validation).ValidateInBondNumbers();
			((USExportAsycudaBillValdiationForMasterChild)masterBill.Validation).ValidateABL_UCRNumber();
			AssertNoMessageErrorContaining(masterBill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(masterBill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(masterBill.ABL_UCRNumberInfo, messageError);

			var bill = header.Bills.AddNew();
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;

			bill.ABL_UCRNumber = "123";
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			bill.ABL_UCRNumber = string.Empty;
			AssertHasMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			bill.InBondNumbers = "123";
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			bill.InBondNumbers = string.Empty;
			AssertHasMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			bill.AESITNNumbers = "123";
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			bill.AESITNNumbers = string.Empty;
			AssertHasMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertHasMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);

			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			((USExportAsycudaBillValidationForRegularBill)bill.Validation).ValidateAESITNNumbers();
			((USExportAsycudaBillValidationForRegularBill)bill.Validation).ValidateInBondNumbers();
			((USExportAsycudaBillValidationForRegularBill)bill.Validation).ValidateABL_UCRNumber();
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);
		}

		public void TestAddNotificationFromCusEntryNumber()
		{
			var messageError = "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;

			bill.AESITNNumbers = "a1234";
			AssertHasMessageErrorContaining(bill.AESITNNumbersInfo, messageError);

			bill.AESITNNumbers = "X20120112901245";
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);

			bill.AESITNNumbers = "0123456789012345";
			AssertHasMessageErrorContaining(bill.AESITNNumbersInfo, messageError);

			messageError = "In-Bond Number should be 9 digits.";
			bill.InBondNumbers = "0123456789";
			AssertHasMessageErrorContaining(bill.InBondNumbersInfo, messageError);

			bill.InBondNumbers = "012345678";
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);

			bill.InBondNumbers = "a12345678";
			AssertHasMessageErrorContaining(bill.InBondNumbersInfo, messageError);
		}
	}
}
