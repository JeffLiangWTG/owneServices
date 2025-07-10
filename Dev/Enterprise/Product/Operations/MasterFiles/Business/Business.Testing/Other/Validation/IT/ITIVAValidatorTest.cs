using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITIVAValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidLength()
		{
			var customsCode = CreateCusCode();
			var expectedError = "The IVA registration code needs to be 13 or 11 in length.";
			customsCode.OK_CustomsRegNo = "IT00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "IT008912301536";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			customsCode.OK_CustomsRegNo = "IT008912301536";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "008912301536";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			customsCode.OK_CustomsRegNo = "008912301536";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);
		}

		public void TestIsValidPattern()
		{
			var customsCode = CreateCusCode();
			var expectedError = @"The IVA registration code pattern is invalid.
Valid patterns are:
	ITnnnnnnnnnnn
	nnnnnnnnnnn";

			customsCode.OK_CustomsRegNo = "IT0089A23015A";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT0089123015#";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "it00891230153";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT008#1230153";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			customsCode.OK_CustomsRegNo = "IT0089A23015A";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT0089123015#";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "it00891230153";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT008#1230153";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			customsCode.OK_CustomsRegNo = "0089123015#";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "008912301IT";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "0089123015#";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "008912301IT";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);
		}

		public void TestIsValidCheckDigit()
		{
			var customsCode = CreateCusCode();
			var expectedError = "The check digit in the IVA registration code is incorrect.";

			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "00891230155";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			customsCode.OK_CustomsRegNo = "00891230155";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "97459060584";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "97459060586";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			customsCode.OK_CustomsRegNo = "97459060586";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "IT00891230155";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			customsCode.OK_CustomsRegNo = "IT00891230155";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "IT97459060584";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "IT97459060586";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			customsCode.OK_CustomsRegNo = "IT97459060586";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);
		}

		OrgCusCode CreateCusCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;

			return customsCode;
		}
	}
}
