using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CustomsRegistrationNumberValidation))]
sealed class CustomsRegistrationNumberValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMVARegistrationNumber()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>().AsMVARegistered("111111111111111");
		org.OH_Code = "TEST1";
		org.OH_RL_NKClosestPort = "NOOSL";
		var address = org.MainAddress;
		address.OA_RN_NKCountryCode = "NO";
		var mvaCustCode = org.CustomsCodes[0];
		OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
		Assert("Registration Number / Code: Norway MVA (VAT Tax ID) should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.", mvaCustCode.OK_CustomsRegNoInfo.HasNotifications());
		mvaCustCode.OK_CustomsRegNo = "GAFDSGas";
		Assert("Registration Number / Code: Norway MVA (VAT Tax ID) should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.", mvaCustCode.OK_CustomsRegNoInfo.HasNotifications());
		mvaCustCode.OK_CustomsRegNo = "123456785";
		AssertNoErrors(mvaCustCode.OK_CustomsRegNoInfo);
	}

	public void TestCheckGBRRegistrationNumber()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = "TEST1";
		org.OH_RL_NKClosestPort = "NOOSL";
		var address = org.MainAddress;
		address.OA_RN_NKCountryCode = "NO";
		var gbrCustCode = org.CustomsCodes.AddNew();
		gbrCustCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
		gbrCustCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
		gbrCustCode.OK_CustomsRegNo = "111111111111111";
		OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
		Assert("Registration Number / Code: Norway Government Business Code should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.", gbrCustCode.OK_CustomsRegNoInfo.HasNotifications());
		gbrCustCode.OK_CustomsRegNo = "GAFDSGas";
		Assert("Registration Number / Code: Norway Government Business Code should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.", gbrCustCode.OK_CustomsRegNoInfo.HasNotifications());
		gbrCustCode.OK_CustomsRegNo = "123456785";
		AssertNoErrors(gbrCustCode.OK_CustomsRegNoInfo);
	}

	public void TestCheckSSNRegistrationNumber()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = "TEST1";
		org.OH_RL_NKClosestPort = "NOOSL";
		var address = org.MainAddress;
		address.OA_RN_NKCountryCode = "NO";
		var gbrCustCode = org.CustomsCodes.AddNew();
		gbrCustCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
		gbrCustCode.OK_CodeType = NorwayOrgCusCodeInfo.OrgCusCodes.SSN;
		gbrCustCode.OK_CustomsRegNo = "12345678901";
		OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
		AssertHasError("Not a valid SSN", gbrCustCode.OK_CustomsRegNoInfo, "Not a valid Norwegian Social Security Number.");
		gbrCustCode.OK_CustomsRegNo = "1234567890";
		AssertHasError("Wrong length. 10 digits", gbrCustCode.OK_CustomsRegNoInfo, "Wrong length. Norwegian Social Security Numbers should be 11 digits.");
		gbrCustCode.OK_CustomsRegNo = "123456789012";
		AssertHasError("Wrong length. 12 digits", gbrCustCode.OK_CustomsRegNoInfo, "Wrong length. Norwegian Social Security Numbers should be 11 digits.");
		gbrCustCode.OK_CustomsRegNo = "ABC12345678";
		AssertHasError("Includes digits", gbrCustCode.OK_CustomsRegNoInfo, "Norwegian Social Security Numbers can only contain digits.");
		gbrCustCode.OK_CustomsRegNo = "08052621187";
		AssertNoErrors(gbrCustCode.OK_CustomsRegNoInfo);
	}

	public void TestCheckEMDRegistrationNumber()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var cusCode = org.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
		cusCode.OK_CodeType = OrgCusCode.NorwayCodeTypes.EMD;

		const string message = "The length of Registration Number / Code for Type 'EMD' cannot exceed 10 characters.";
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "12345678910";
			AssertHasMessageError("When length of Registration Number / Code > 10", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageError("When length of Registration Number / Code <= 10", cusCode.OK_CustomsRegNoInfo, message);
		});
	}
}
