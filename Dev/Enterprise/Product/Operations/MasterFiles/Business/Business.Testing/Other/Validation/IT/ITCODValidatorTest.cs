using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITCODValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidLength()
		{
			var customsCode = CreateCusCode();
			var expectedError = "The COD registration code needs to be 16 or 11 in length.";
			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "008912301536";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			customsCode.OK_CustomsRegNo = "008912301536";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678A";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678AA";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678AA";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);
		}

		public void TestIsValidPattern()
		{
			var customsCode = CreateCusCode();
			var expectedError = @"The COD registration code pattern is invalid.
Valid patterns are:
	AAAAAAnnAnnAnnnA
	nnnnnnnnnnn";

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A6789";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678#";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA1AA45A678A";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AA5AAA12A45A678A";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678A";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A6789";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA12A45A678#";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AAAAAA1AA45A678A";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "AA5AAA12A45A678A";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			customsCode.OK_CustomsRegNo = "0089123015#";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "0089123015A";
			AssertEquals("There is warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			customsCode.OK_CustomsRegNo = "0089123015#";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);

			customsCode.OK_CustomsRegNo = "0089123015A";
			AssertEquals("There is error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, expectedError);
		}

		public void TestIsValidCheckDigit()
		{
			var customsCode = CreateCusCode();
			var expectedError = "The check digit in the COD registration code is incorrect.";

			customsCode.OK_CustomsRegNo = "00891230153";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

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

			customsCode.OK_CustomsRegNo = "AAAAAA55A45A678A";
			AssertEquals("There is no warning/error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());
		}

		public void TestValidate()
		{
			var expectedError = "The COD registration code needs to be 16 or 11 in length. Suffix";

			AssertExceptionThrown<ArgumentNullException>("Exception expected when targetPropertyInfo is null", () => new ITCODValidator().Validate("", null));

			var dummyBizo = Factory.New<ITCodValidatorDummyBizObject>();

			dummyBizo.Z0_Description = "008912301536";
			dummyBizo.Validation.ValidateZ0_Date();
			AssertHasMessageErrorContaining(dummyBizo.Z0_DateInfo, expectedError);

			dummyBizo.Z0_Description = "00891230153";
			dummyBizo.Validation.ValidateZ0_Date();
			AssertNoMessageErrorContaining(dummyBizo.Z0_DateInfo, expectedError);
		}

		OrgCusCode CreateCusCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			customsCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;

			return customsCode;
		}

		class ITCodValidatorDummyBizObject : DummyBusinessObject
		{
			public ITCodValidatorDummyBizObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation() => new ITCodValidatorDummyBizoValidation(this);
		}

		class ITCodValidatorDummyBizoValidation : DummyBizoValidation
		{
			public ITCodValidatorDummyBizoValidation(AutoDummyBizo parent) : base(parent)
			{
			}

			protected override void CheckZ0_Date()
			{
				base.CheckZ0_Date();
				new ITCODValidator().Validate(Parent.Z0_Description, Parent.Z0_DateInfo, "Suffix", NotificationType.MessageError);
			}
		}
	}
}
