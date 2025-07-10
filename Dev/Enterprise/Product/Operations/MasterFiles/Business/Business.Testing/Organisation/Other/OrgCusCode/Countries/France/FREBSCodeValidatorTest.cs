using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FREBSCodeValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEBSForFRPremisesAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var address1 = org.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);

			var code1 = org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "123456789", CountryCodes.France);
			code1.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertHasMessageError(code1.OK_OA_PremisesAddressInfo, "Only addresses flagged as EU customs address can be associated to EBS.");

			var code2 = org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "123456789", CountryCodes.Germany);
			code2.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoMessageError(code2.OK_OA_PremisesAddressInfo, "Only addresses flagged as EU customs address can be associated to EBS.");

			code1.OK_OA_PremisesAddress = address1.PK;
			AssertNoMessageError(code1.OK_OA_PremisesAddressInfo, "Only addresses flagged as EU customs address can be associated to EBS.");
		}

		public void TestValidateEBS_France_Formatting()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.EoriBranchSuffix;

			var formattingErrorMessage = "EORI Branch Suffix should be 5 digits and different to the EORI code, e.g. 00001";
			AssertNoMessageError("", cusCode.OK_CustomsRegNoInfo, formattingErrorMessage);

			cusCode.OK_CustomsRegNo = "0AS12";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, formattingErrorMessage);

			cusCode.OK_CustomsRegNo = "1234";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, formattingErrorMessage);

			cusCode.OK_CustomsRegNo = "12345";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, formattingErrorMessage);
		}

		public void TestValidateEBS_France_EORMandatory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "FRANG";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.EoriBranchSuffix;

			var missingEORErrorMessage = "There is no FR EORI to link this information to.";
			var wrongLengthEORMessage = "The linked FR EORI should be 9 digits long.";

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, missingEORErrorMessage);

			var eorCusCode = org.CustomsCodes.AddNew();
			eorCusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			eorCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(cusCode.OK_CustomsRegNoInfo, missingEORErrorMessage);

			eorCusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(cusCode.OK_CustomsRegNoInfo, missingEORErrorMessage);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, wrongLengthEORMessage);

			eorCusCode.OK_CustomsRegNo = "123456789";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, wrongLengthEORMessage);
		}
	}
}
