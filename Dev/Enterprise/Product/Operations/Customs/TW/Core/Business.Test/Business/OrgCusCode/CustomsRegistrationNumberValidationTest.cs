using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CustomsRegistrationNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAEOCustomsRegistrationNumber()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_RL_NKClosestPort = "TWKEL";
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "TW";
			var aeoCustomsCode = org.CustomsCodes.AddNew();
			aeoCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			aeoCustomsCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			aeoCustomsCode.OK_CustomsRegNo = "111111111111111";
			AssertHasMessageErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "The length of AEO (Authorized Economic Operator) shouldn't be more than 14.");
			aeoCustomsCode.OK_CustomsRegNo = "1111111111111A";
			AssertNoMessageErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "The length of AEO (Authorized Economic Operator) shouldn't be more than 14.");
			AssertHasMessageErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "AEO Number is formatted as TWAEO-nnnnnnnnn, but you only need to enter the 9 digit suffix component of the number here.");
			AssertHasWarningContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "Not allow special characters.");
			aeoCustomsCode.OK_CustomsRegNo = "11111111111111";
			AssertNoMessageErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "AEO Number is formatted as TWAEO-nnnnnnnnn, but you only need to enter the 9 digit suffix component of the number here.");
			AssertNoWarningContaining(aeoCustomsCode.OK_CustomsRegNoInfo, "Not allow special characters.");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = org.PK;
			var importerJobAddress = declaration.ImporterDocumentaryAddress;
			importerJobAddress.E2_AddressOverride = true;
			importerJobAddress.E2_RN_NKCountryCode = "AU";
			importerJobAddress.AEOCode = "123456ABC";
			AssertHasWarningContaining(importerJobAddress.AEOCodeInfo, "Not allow special characters.");
			importerJobAddress.AEOCode = "123456";
			AssertNoWarningContaining(importerJobAddress.AEOCodeInfo, "Not allow special characters.");
		}

		public void TestCheckATPCustomsRegistrationNumber()
		{
			var errorMessage = "The length of ATP (Agricultural Technology Park Bonded ID) shouldn't be more than 5.";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_RL_NKClosestPort = "TWKEL";
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "TW";
			var aeoCustomsCode = org.CustomsCodes.AddNew();
			aeoCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			aeoCustomsCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
			CombineAssertions(() =>
			{
				aeoCustomsCode.OK_CustomsRegNo = "123456";
				AssertHasErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, errorMessage);
				aeoCustomsCode.OK_CustomsRegNo = "12345";
				AssertNoErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, errorMessage);
			});
		}

		public void TestCheckSPKCustomsRegistrationNumber()
		{
			var errorMessage = "The length of SPK (Science Park Bonded ID) shouldn't be more than 5.";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_RL_NKClosestPort = "TWKEL";
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "TW";
			var aeoCustomsCode = org.CustomsCodes.AddNew();
			aeoCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			aeoCustomsCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.SciencePark;
			CombineAssertions(() =>
			{
				aeoCustomsCode.OK_CustomsRegNo = "123456";
				AssertHasErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, errorMessage);
				aeoCustomsCode.OK_CustomsRegNo = "12345";
				AssertNoErrorContaining(aeoCustomsCode.OK_CustomsRegNoInfo, errorMessage);
			});
		}
	}
}
