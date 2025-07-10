using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RestrictedCodeValidationTest : Customs.Business.Testing.CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			RestrictedCode code = Factory.New<RestrictedCode>();
			code.CY_Code = RestrictedCodeTypeList.Codes.RestrictedSPI;
			code.CY_Data = "$";
			AssertHasMessageError(code.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			code.CY_Data = PrimarySpecProgramIndicatorList.Codes.D;
			AssertNoMessageError(code.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			code.CY_Code = RestrictedCodeTypeList.Codes.RestrictedEntryType;
			code.CY_Data = "$";
			AssertHasMessageError(code.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			code.CY_Data = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageError(code.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			code.CY_Code = RestrictedCodeTypeList.Codes.RestrictedTariff;
			code.CY_Data = "12345";
			AssertHasMessageError(code.CY_DataInfo, RestrictedCodeValidation.InvalidRestrictedTariffCode);
			code.CY_Data = "1234";
			AssertNoMessageError(code.CY_DataInfo, RestrictedCodeValidation.InvalidRestrictedTariffCode);
			code.CY_Code = RestrictedCodeTypeList.Codes.FTZAllowsFDA;
			var messageError1 = $"Zone ID must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.";

			code.CY_Data = "AAAAAAA";
			AssertHasMessageError(code.CY_DataInfo, messageError1);
			code.CY_Data = "123A1A111";
			AssertNoMessageError(code.CY_DataInfo, messageError1);
		}

		public void TestCheckCY_Data_SameZoneID_Allowed()
		{
			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			var iorWrapper = OrgHeaderWrapper.New(iOR);
			var fTZAllowsFDACode = iorWrapper.FTZAllowsFDAs.AddNew();
			fTZAllowsFDACode.CY_Code = RestrictedCodeTypeList.Codes.FTZAllowsFDA;
			fTZAllowsFDACode.CY_Data = "123A1A111";
			Factory.Save();

			var iOR1 = Factory.NewWithValidTestData<OrgHeader>();
			var iorWrapper1 = OrgHeaderWrapper.New(iOR1);
			var fTZAllowsFDACode1 = iorWrapper1.FTZAllowsFDAs.AddNew();
			fTZAllowsFDACode1.CY_Code = RestrictedCodeTypeList.Codes.FTZAllowsFDA;
			fTZAllowsFDACode1.CY_Data = "123A1A111";
			fTZAllowsFDACode1.Validation.ValidateCY_Data();
			AssertNoErrors(fTZAllowsFDACode1.CY_DataInfo);
		}
	}
}
