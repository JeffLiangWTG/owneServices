using System;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressValidationTest : TestCaseWithFactory
	{
		public void TestEmailValidation()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Email = "Richard.White@edi.com.au";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_EmailInfo.HasErrors() || docAddress.E2_EmailInfo.HasWarnings());

			docAddress.E2_Email = "RichardWhitegonebad";
			AssertEquals("There should be an error.", true, docAddress.E2_EmailInfo.HasErrors());
		}

		public void TestStateValidation()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_State = "NSW";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_StateInfo.HasErrors() || docAddress.E2_StateInfo.HasWarnings());

			docAddress.E2_State = "ABCdefghi!!";
			AssertEquals("There should be an error.", true, docAddress.E2_StateInfo.HasErrors());

			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			docAddress.E2_State = "AAA";
			AssertHasError(docAddress.E2_StateInfo, "Enter a valid State.");

			docAddress.E2_State = "";
			AssertHasError(docAddress.E2_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			docAddress.E2_State = "NSW";
			AssertHasError(docAddress.E2_StateInfo, "You must NOT enter a state. The state validation rule for the country/region Australia is currently set to \"Must Not Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			docAddress.E2_State = "VIC";
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_RN_NKCountryCode = "";
			docAddress.E2_State = "NSW";
			AssertHasWarning(docAddress.E2_StateInfo, "This state code needs to be followed by valid country/region code.");

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			docAddress.E2_State = "AAA";
			AssertNoWarning(docAddress.E2_StateInfo, "Enter a valid State.");
			docAddress.E2_State = "";
			AssertNoError(docAddress.E2_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			docAddress.E2_State = "NSW";
			AssertNoError(docAddress.E2_StateInfo, "You must NOT enter a state. The state validation rule for the country/region Australia is currently set to \"Must Not Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");
		}

		public void TestStateValidationWhenValueNotFromRefStatesList()
		{
			var previousSetting = DataRegistry.Instance.EnableAddressValidationWebService;
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_State = "Victoria";
			AssertHasError(docAddress.E2_StateInfo, "Enter a valid State.");

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			docAddress.ValidationStatus = AddressValidationStatus.Verified;
			docAddress.ValidatePostcodeAndStateForAddress();
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_StateInfo.HasErrors() || docAddress.E2_StateInfo.HasWarnings());

			DataRegistry.Instance.EnableAddressValidationWebService = previousSetting;
		}

		public void TestPostCodeValidation()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			docAddress.E2_RN_NKCountryCode = "AU";
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			docAddress.E2_Postcode = "123";
			AssertHasWarning(docAddress.E2_PostcodeInfo, InvalidPostcodeFormattingWarning(au));

			docAddress.E2_Postcode = "";
			AssertHasError(docAddress.E2_PostcodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			docAddress.E2_Postcode = "1234";
			AssertEquals("Should have no errors or warnings", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());

			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			docAddress.E2_Postcode = "VIC";
			AssertNoErrors(docAddress.E2_PostcodeInfo);

			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeFormatted;
			docAddress.E2_Postcode = "1234";
			AssertEquals("Should have no errors or warnings", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());

			docAddress.E2_Postcode = "invalid";
			AssertHasError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(au));

			docAddress.E2_Postcode = "";
			AssertHasError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(au));

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			docAddress.ValidationStatus = AddressValidationStatus.Invalid;
			docAddress.E2_Postcode = "";
			AssertNoError(docAddress.E2_PostcodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");
		}

		public void TestStateAndPostcodeValidationWithDifferentValidationStatus()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";

			MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ManuallyVerified, au);
			MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Unverifiable, au);
			MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.CountryNotAvailable, au);
			MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ToBeVerified, au);
			MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ExcludeBackgroundValidation, au);

			MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Invalid, au);
			MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Verified, au);
			MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.VerifiedToStreet, au);
		}

		public void TestStateAndPostcodeValidationWithDifferentValidationStatus_EDIClient()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Unverifiable, au);
				MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.CountryNotAvailable, au);
				MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ToBeVerified, au);
				MustBeEnteredValidationInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ExcludeBackgroundValidation, au);

				MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.ManuallyVerified, au);
				MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Invalid, au);
				MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.Verified, au);
				MustBeEnteredValidationNotInCountrySpecificRulesApplying(docAddress, AddressValidationStatus.VerifiedToStreet, au);
			}
		}

		public void TestPostcodeMustBeFormattedValidationWithDifferentValidationStatus()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeFormatted;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";

			MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ManuallyVerified);
			MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Unverifiable);
			MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.CountryNotAvailable);
			MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ToBeVerified);
			MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ExcludeBackgroundValidation);

			MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Invalid);
			MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Verified);
			MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.VerifiedToStreet);
		}

		public void TestPostcodeMustBeFormattedValidationWithDifferentValidationStatus_EDIClient()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeFormatted;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Unverifiable);
				MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.CountryNotAvailable);
				MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ToBeVerified);
				MustBeFormattedValidationInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ExcludeBackgroundValidation);

				MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.ManuallyVerified);
				MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Invalid);
				MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.Verified);
				MustBeFormattedValidationNotInCountrySpecificRulesApplying(docAddress, au, AddressValidationStatus.VerifiedToStreet);
			}
		}

		public void TestPostCodeValidationForPoland()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			var poland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "PL");
			docAddress.E2_RN_NKCountryCode = "PL";

			docAddress.E2_Postcode = "12345";
			AssertHasWarning(docAddress.E2_PostcodeInfo, InvalidPostcodeFormattingWarning(poland));

			docAddress.E2_Postcode = "12 345";
			AssertHasWarning(docAddress.E2_PostcodeInfo, InvalidPostcodeFormattingWarning(poland));

			docAddress.E2_Postcode = "12-345";
			AssertEquals("Should have no errors or warnings", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());
		}

		public void TestIsWebVerified()
		{
			var addressValidation = new AddressValidation();

			CombineAssertions(() =>
			{
				AssertEquals("Verified is web verified", true, addressValidation.IsWebVerified(AddressValidationStatus.Verified));
				AssertEquals("VerifiedToStreet is web verified", true, addressValidation.IsWebVerified(AddressValidationStatus.VerifiedToStreet));
				AssertEquals("CountryNotAvailable is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.CountryNotAvailable));
				AssertEquals("Invalid is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.Invalid));
				AssertEquals("ManuallyVerified is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.ManuallyVerified));
				AssertEquals("ToBeVerified is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.ToBeVerified));
				AssertEquals("Unverifiable is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.Unverifiable));
				AssertEquals("NotRequired is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.NotRequired));
				AssertEquals("ExcludeBackgroundValidation is not web verified", false, addressValidation.IsWebVerified(AddressValidationStatus.ExcludeBackgroundValidation));
			});
		}

		#region Implementation

		void MustBeEnteredValidationInCountrySpecificRulesApplying(JobDocAddress docAddress, string addressValidationStatus, RefCountry country)
		{
			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "NSW";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_StateInfo.HasErrors() || docAddress.E2_StateInfo.HasWarnings());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "ABCdefghi!!";
			AssertEquals("There should be an error.", true, docAddress.E2_StateInfo.HasErrors());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "";
			AssertHasError(docAddress.E2_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "NSW";
			AssertHasWarning(docAddress.E2_PostcodeInfo, InvalidPostcodeFormattingWarning(country));

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "";
			AssertHasError(docAddress.E2_PostcodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "1234";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());
		}

		void MustBeEnteredValidationNotInCountrySpecificRulesApplying(JobDocAddress docAddress, string addressValidationStatus, RefCountry country)
		{
			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "NSW";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_StateInfo.HasErrors() || docAddress.E2_StateInfo.HasWarnings());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "ABCdefghi!!";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_StateInfo.HasErrors() || docAddress.E2_StateInfo.HasWarnings());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_State = "";
			AssertNoError(docAddress.E2_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "NSW";
			AssertNoWarning(docAddress.E2_PostcodeInfo, InvalidPostcodeFormattingWarning(country));

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "";
			AssertNoError(docAddress.E2_PostcodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "1234";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());
		}

		void MustBeFormattedValidationInCountrySpecificRulesApplying(JobDocAddress docAddress, RefCountry country, string addressValidationStatus)
		{
			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "1234";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "invalid";
			AssertHasError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(country));

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "";
			AssertHasError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(country));
		}

		void MustBeFormattedValidationNotInCountrySpecificRulesApplying(JobDocAddress docAddress, RefCountry country, string addressValidationStatus)
		{
			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "1234";
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "invalid";
			AssertNoError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(country));

			docAddress.ValidationStatus = addressValidationStatus;
			docAddress.E2_Postcode = "";
			AssertNoError(docAddress.E2_PostcodeInfo, InvalidPostcodeMustBeFormattedError(country));
		}

		public void TestCountryWithNoPostcodeFormattingRuleHasNoErrorsOrWarnings()
		{
			var fakeCountry = Factory.NewWithValidTestData<RefCountry>();
			fakeCountry.Code = "XX";
			fakeCountry.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeFormatted;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "XX";

			docAddress.E2_Postcode = "";
			AssertEquals("Empty postcode should have no errors or warnings", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());

			docAddress.E2_Postcode = "1234";
			AssertEquals("Non-empty postcode should have no errors or warnings", false, docAddress.E2_PostcodeInfo.HasErrors() || docAddress.E2_PostcodeInfo.HasWarnings());
		}

		string InvalidPostcodeFormattingWarning(RefCountry country)
		{
			return string.Format("The entered postcode does not comply with the postcode format rules of the country/region ({0}). The postcode validation rule for the country/region {1} is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain->Locations->Countries/Regions.", country.PostcodeFormattingRule.Format, country.RN_DescMultilingual);
		}

		string InvalidPostcodeMustBeFormattedError(RefCountry country)
		{
			return string.Format("The entered postcode does not comply with the postcode format rules of the country/region ({0}). The postcode validation rule for the country/region {1} is currently set to \"{2}\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain->Locations->Countries/Regions.", country.PostcodeFormattingRule.Format, country.RN_DescMultilingual, country.Lookups.PostCodeValidationRules[country.RN_PostcodeValidationRule].Description);
		}

		#endregion
	}
}
