using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	sealed class USExportAsycudaBillValdiationForMasterChildTest : AsycudaBillValidationAbstractTest
	{
		public void TestChangingPorts()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");
			var pgWWK = helper.CreateNewOrGetExistingCusCodeList("PG", RefCusCodeListTypes.Codes.CustomsOffice, "WWK", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(pgWWK.PK, "PORT", "PGWWK");
			Factory.Save();

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_JobReference = "whatever";
			var masterBill = header.MasterBill;
			AssertEquals(0, masterBill.ABL_RL_NKPortOfDischargeInfo.Notifications.Count());
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertEquals(0, masterBill.ABL_RL_NKPortOfDischargeInfo.Notifications.Count());
			header.AMA_RL_NKPortOfDischarge = "VUVLI";
			AssertEquals(0, masterBill.ABL_RL_NKPortOfDischargeInfo.Notifications.Count());
			header.AMA_RL_NKPortOfDischarge = "DEBLN";
			AssertEquals(0, masterBill.ABL_RL_NKPortOfDischargeInfo.Notifications.Count());
			header.AMA_RL_NKPortOfDischarge = "GBLHR";
			AssertEquals(0, masterBill.ABL_RL_NKPortOfDischargeInfo.Notifications.Count());

			header.AMA_RL_NKPortOfDischarge = "";
			AssertHasMessageErrorContaining(masterBill.ABL_RL_NKPortOfDischargeInfo, "not entered");
			header.AMA_RL_NKPortOfDischarge = "PGXXX";
			AssertNoWarningContaining(masterBill.ABL_RL_NKPortOfDischargeInfo, "not entered");
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
			bill.ABL_BolType = AsycudaBill.ChildBolCode;

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
			((USExportAsycudaBillValdiationForMasterChild)bill.Validation).ValidateAESITNNumbers();
			((USExportAsycudaBillValdiationForMasterChild)bill.Validation).ValidateInBondNumbers();
			((USExportAsycudaBillValdiationForMasterChild)bill.Validation).ValidateABL_UCRNumber();
			AssertNoMessageErrorContaining(bill.AESITNNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.InBondNumbersInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_UCRNumberInfo, messageError);
		}

		public void TestAddNotificationFromCusEntryNumber()
		{
			var messageError = "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;

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

		public void TestScheduleDExpAttribute()
		{
			var cusCodeList = Factory.NewWithValidTestData(typeof(ZZRefCusCodeListCombined)) as ZZRefCusCodeListCombined;
			cusCodeList.ZZD_Code = "2809";
			cusCodeList.ZZD_StartDate = ZDateTime.BrettsBirthday;
			cusCodeList.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
			cusCodeList.ZZD_IsSea = true;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;

			var refCusCodeListAttribute = Factory.NewWithValidTestData(typeof(ZZRefCusCodeListAttributeCombined)) as ZZRefCusCodeListAttributeCombined;
			refCusCodeListAttribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ROLE;
			refCusCodeListAttribute.ZZE_Value = "EXP";
			refCusCodeListAttribute.ZZE_ZZD_CodeList = cusCodeList.PK;

			Factory.Save();

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			header.MasterBill.Validation.ValidateABL_CustomsLoadPort();
			AssertHasMessageError("ABL_CustomsLoadPort is madatory", masterBill.ABL_CustomsLoadPortInfo, "You have not entered a value.");

			masterBill.ABL_CustomsLoadPort = "2809";
			header.MasterBill.Validation.ValidateABL_CustomsLoadPort();
			AssertNoMessageError(masterBill.ABL_CustomsLoadPortInfo, "You have not entered a value.");
			AssertNoMessageError(masterBill.ABL_CustomsLoadPortInfo, ListValidation.InvalidCodeMessageError);

			masterBill.ABL_CustomsLoadPort = "*";
			header.MasterBill.Validation.ValidateABL_CustomsLoadPort();
			AssertHasMessageError(masterBill.ABL_CustomsLoadPortInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
