using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAddressValidationTest : BusinessObjectValidationTestCase
	{
		const string InvalidAddressMessage =
			"There is an invalid address recorded on this job. " +
			"Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. " +
			"The suggestion box is accessed by clicking the envelope icon next to the address.";

		public void TestValidateAll_WhenDisablingSuppressErrorOnRegistryAndRecord_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

			var address = Factory.CreateValidOrgAddress();
			address.OA_SuppressAddressValidationError = false;
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new OrgAddressValidation(address);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateAll_WhenDisablingSuppressErrorOnRegistryButNotOnRecord_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

			var address = Factory.CreateValidOrgAddress();
			address.OA_SuppressAddressValidationError = true;
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new OrgAddressValidation(address);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateAll_WhenDisablingSuppressErrorOnRecordButNotOnRegistry_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

			var address = Factory.CreateValidOrgAddress();
			address.OA_SuppressAddressValidationError = false;
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new OrgAddressValidation(address);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateAll_WhenEnablingSuppressErrorOnRegistryAndRecord_ShouldDowngradeErrorToWarning()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

			var address = Factory.CreateValidOrgAddress();
			address.OA_SuppressAddressValidationError = true;
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new OrgAddressValidation(address);

			validation.ValidateAll();

			AssertNoError(
				"Should not treat invalid address message as error.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertHasWarning(
				"Should treat invalid address message as warning.",
				address.OA_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		#region OA_AuthorityToLeave

		public void TestCheckOA_AuthorityToLeave()
		{
			address.OA_AuthorityToLeave = "XXX";
			AssertHasError(address.OA_AuthorityToLeaveInfo, "Enter a valid Authority To Leave.");

			address.OA_AuthorityToLeave = "DEF";
			AssertNoErrors("DEF is a valid Authority To Leave option.", address.OA_AuthorityToLeaveInfo);

			address.OA_AuthorityToLeave = "YES";
			AssertNoErrors("YES is a valid Authority To Leave option.", address.OA_AuthorityToLeaveInfo);

			address.OA_AuthorityToLeave = "NO";
			AssertNoErrors("NO is a valid Authority To Leave option.", address.OA_AuthorityToLeaveInfo);

			address.OA_AuthorityToLeave = "";
			AssertHasError(address.OA_AuthorityToLeaveInfo, "Please enter an Authority To Leave.");
		}

		#endregion

		#region OA_AdditionalAddressInformation

		public void TestCheckOA_AdditionalAddressInformation()
		{
			address.UnrestrictedAdditionalAddressInformation = string.Empty;
			Assert("VALID: Empty value", !address.OA_AdditionalAddressInformationInfo.HasErrors());

			address.UnrestrictedAdditionalAddressInformation = new string('0', address.OA_AdditionalAddressInformationInfo.MaxLength - 1);
			Assert("VALID: Value length less than max length", !address.OA_AdditionalAddressInformationInfo.HasErrors());

			address.UnrestrictedAdditionalAddressInformation = new string('0', address.OA_AdditionalAddressInformationInfo.MaxLength);
			Assert("VALID: Value length less equal to max length", !address.OA_AdditionalAddressInformationInfo.HasErrors());
		}

		#endregion

		#region OA_Address1

		public void TestCheckOA_Address1()
		{
			// Minimum length = 4
			address.OA_Address1 = "T";
			Assert("No fAddress, has errors", address.OA_Address1Info.HasErrors());

			address.OA_Address1 = "Te";
			Assert("No fAddress, has errors", address.OA_Address1Info.HasErrors());

			address.OA_Address1 = "Tes";
			Assert("No fAddress, has errors", address.OA_Address1Info.HasErrors());

			address.OA_Address1 = "Test";
			Assert("Valid fAddress, no errors", !address.OA_Address1Info.HasErrors());

			// Valid
			address.OA_Address1 = "Test fAddress";
			Assert("Valid fAddress, no errors", !address.OA_Address1Info.HasErrors());

			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_Address1Info);

			// Blank not allowed
			address.OA_Address1 = string.Empty;
			Assert("No fAddress, has errors", address.OA_Address1Info.HasErrors());
		}

		#endregion

		#region OA_Address2

		public void TestMandatoryValidationAppliesForAddress2WhenManuallyVerifying()
		{
			var oldFields = Env.Registry.GetOrgForwarderRequiredFields();
			var fields = new OrgRequiredFields(true, false, false, false, false, false, false, false, false, false, false);
			var oldValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				Env.Registry.SetOrgForwarderRequiredFields(fields);
				org.OH_IsForwarder = true;
				address.OA_Address2 = "Back alley";
				address.ValidationStatus = AddressValidationStatus.Verified;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");
				address.OA_Address2 = ZString.Empty;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");

				address.OA_Address2 = "Back alley";
				address.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");
				address.OA_Address2 = ZString.Empty;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");

				address.OA_Address2 = "Back alley";
				address.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");
				address.OA_Address2 = ZString.Empty;
				AssertHasError(address.OA_Address2Info, "Please enter an Address 2.");

				address.OA_Address2 = "Back alley";
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;
				AssertNoError(address.OA_Address2Info, "Please enter an Address 2.");
				address.OA_Address2 = ZString.Empty;
				AssertHasError(address.OA_Address2Info, "Please enter an Address 2.");
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = oldValue;
				Env.Registry.SetOrgForwarderRequiredFields(oldFields);
			}
		}

		public void TestCheckOA_Address2()
		{
			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_Address2Info);
		}

		#endregion

		#region OA_Language

		public void TestCheckOA_Language()
		{
			address.OA_Language = string.Empty;
			Assert("No language entered, has errors", address.OA_LanguageInfo.HasErrors());

			address.OA_Language = "EEE";
			Assert("Invalid code, has errors", address.OA_LanguageInfo.HasErrors());

			address.OA_Language = Core.SharedConstants.Languages.Swahili;
			Assert("Valid code, no errors", !address.OA_LanguageInfo.HasErrors());

			address.OA_Address1 = "Créteil";
			address.OA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertHasError(address.OA_LanguageInfo, "Non-English Characters detected in this address. Please select the proper language for this address.");
		}

		public void TestLanguageForMainAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			org.MainAddress.OA_Language = Core.Constants.Languages.Gujarati;
			AssertNoWarnings("Language can be anything as this org is not involved in overseas transactions", org.MainAddress.OA_LanguageInfo);

			org.OH_IsConsignee = true;
			org.MainAddress.OA_Language = Core.Constants.Languages.Hindi;
			AssertHasWarnings("Language must be english as this org is involved in overseas transactions", org.MainAddress.OA_LanguageInfo);

			org.MainAddress.OA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertNoWarnings("Language is english so no errors as this org is involved in overseas transactions", org.MainAddress.OA_LanguageInfo);
		}

		#endregion

		#region OA_City

		public void TestMandatoryValidationAppliesForCityWhenManuallyVerifying()
		{
			var oldFields = Env.Registry.GetOrgForwarderRequiredFields();
			var fields = new OrgRequiredFields(false, false, true, false, false, false, false, false, false, false, false);
			var oldValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				Env.Registry.SetOrgForwarderRequiredFields(fields);
				org.OH_IsForwarder = true;
				address.OA_City = "Sydney";
				address.ValidationStatus = AddressValidationStatus.Verified;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");
				address.OA_City = ZString.Empty;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");

				address.OA_City = "Sydney";
				address.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");
				address.OA_City = ZString.Empty;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");

				address.OA_City = "Sydney";
				address.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");
				address.OA_City = ZString.Empty;
				AssertHasError(address.OA_CityInfo, "Please enter a City.");

				address.OA_City = "Sydney";
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;
				AssertNoError(address.OA_CityInfo, "Please enter a City.");
				address.OA_City = ZString.Empty;
				AssertHasError(address.OA_CityInfo, "Please enter a City.");
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = oldValue;
				Env.Registry.SetOrgForwarderRequiredFields(oldFields);
			}
		}

		public void TestCheckOA_City()
		{
			address.OA_City = "Test City";
			Assert("Valid City, no errors", !address.OA_CityInfo.HasErrors());

			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_CityInfo);
		}

		#endregion

		#region OA_Code

		public void TestCheckOA_Code()
		{
			address.OA_Code = string.Empty;
			Assert("No fAddress Short Code, has errors", address.OA_CodeInfo.HasErrors());

			address.OA_Code = "Test Code";
			Assert("Valid fAddress Short Code, no errors", !address.OA_CodeInfo.HasErrors());

			address.OA_Address1 = "in the middle of nowhere";
			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_CodeInfo);
		}

		public void TestCheckOA_CodeIsUnique()
		{
			OrgAddress address1 = org.Addresses.AddNew();
			address1.OA_Code = "Short Code1";
			OrgAddress address2 = org.Addresses.AddNew();
			address2.OA_Code = "Short Code2";
			OrgAddress address3 = org.Addresses.AddNew();
			address3.OA_Code = "Short Code3";

			Assert("Address1 - no error expected", !address1.OA_CodeInfo.HasErrors());
			Assert("Address2 - no error expected", !address2.OA_CodeInfo.HasErrors());
			Assert("Address3 - no error expected", !address3.OA_CodeInfo.HasErrors());

			address3.OA_Code = "Short CODE1";
			Assert("Address1 - error expected (same code)", address1.OA_CodeInfo.HasErrors());
			Assert("Address2 - no error expected", !address2.OA_CodeInfo.HasErrors());
			Assert("Address3 - error expected (same code)", address3.OA_CodeInfo.HasErrors());

			address3.OA_Code = "Short Code3";
			Assert("Address1 - no error expected", !address1.OA_CodeInfo.HasErrors());
			Assert("Address2 - no error expected", !address2.OA_CodeInfo.HasErrors());
			Assert("Address3 - no error expected", !address3.OA_CodeInfo.HasErrors());
		}

		#endregion

		#region OA_DeliveryRouteAndSequence

		public void TestCheckOA_DeliveryRouteAndSequence()
		{
			ReadOnlyCodeDescriptionPairList list = Env.Registry.DeliveryRoutesList;

			CodeDescriptionPairList newList = new CodeDescriptionPairList();
			newList.AddPair("ABC", "ABC Desc");
			Env.Registry.DeliveryRoutesList = newList;

			address.OA_DeliveryRoute = "XXX";
			address.OA_DeliveryRouteSequence = 1;
			Assert("Incorrect delivery route set, has errors", address.OA_DeliveryRouteInfo.HasErrors());

			address.OA_DeliveryRoute = "ABC";
			address.OA_DeliveryRouteSequence = 1;
			Assert("Correct delivery route set, no errors", !address.OA_DeliveryRouteSequenceInfo.HasErrors());

			address.OA_DeliveryRoute = string.Empty;
			address.OA_DeliveryRouteSequence = 1;
			Assert("No delivery route but sequence is not zero, has errors", address.OA_DeliveryRouteSequenceInfo.HasErrors());

			address.OA_DeliveryRoute = "TST";
			address.OA_DeliveryRouteSequence = 0;
			Assert("With delivery route but sequence is zero, has errors", address.OA_DeliveryRouteSequenceInfo.HasErrors());

			address.OA_DeliveryRoute = "TST";
			address.OA_DeliveryRouteSequence = 1;
			Assert("With delivery route and sequence is not zero, no errors", !address.OA_DeliveryRouteSequenceInfo.HasErrors());

			Env.Registry.DeliveryRoutesList = list;
		}

		#endregion

		#region OA_JobLoadingDuration

		public void TestCheckOA_JobLoadingDuration()
		{
			address.OA_JobLoadingDuration = -5;
			AssertHasError(address.OA_JobLoadingDurationInfo, "Job loading during timespan should be greater than or equal to zero.");
			address.OA_JobLoadingDuration = 0;
			AssertNoErrors(address.OA_JobLoadingDurationInfo);
		}

		#endregion

		#region OA_State

		public void TestCheckOA_State()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			address.Header.OH_RL_NKClosestPort = "AUSYD";
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			address.OA_State = string.Empty;
			AssertHasErrors("State must be entered", address.OA_StateInfo);
			address.OA_State = "XXX";
			AssertHasErrors("State must be valid", address.OA_StateInfo);
			address.OA_State = "NSW";
			AssertNoErrors(address.OA_StateInfo);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			address.OA_State = string.Empty;
			AssertNoErrors("State can be empty as NoValidationRule is specified", address.OA_StateInfo);
			address.OA_State = "XXX";
			AssertNoErrors("State can be invalid as NoValidationRule is specified", address.OA_StateInfo);
			address.OA_State = "NSW";
			AssertNoErrors(address.OA_StateInfo);

			RefCountry sg = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "SG");
			sg.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			address.Header.OH_RL_NKClosestPort = "SGAYC";
			address.OA_State = string.Empty;
			AssertHasErrors(address.OA_StateInfo);

			address.Header.OH_RL_NKClosestPort = "";
			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_StateInfo);

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			address.Header.OH_RL_NKClosestPort = "AUSYD";
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			address.ValidationStatus = "MAN";
			address.OA_State = string.Empty;
			AssertHasErrors("State must be entered", address.OA_StateInfo);
			address.ValidationStatus = "MAN";
			address.OA_State = "XXX";
			AssertHasErrors("State must be valid", address.OA_StateInfo);
			address.ValidationStatus = "MAN";
			address.OA_State = "NSW";
			AssertNoErrors(address.OA_StateInfo);
		}

		public void TestCheckOA_State_AdditionalAddresses()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			address.Header.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress addressAdditional = org.Addresses.AddNew();
			addressAdditional.OA_RL_NKRelatedPortCode = "";

			addressAdditional.OA_RN_NKCountryCode = "AU";
			addressAdditional.OA_Address1 = "ADDRESS1";
			addressAdditional.OA_State = string.Empty;
			addressAdditional.Validation.ValidateOA_State();
			AssertHasErrors(addressAdditional.OA_StateInfo);
			addressAdditional.OA_State = "XXX";
			addressAdditional.Validation.ValidateOA_State();
			AssertHasErrors(addressAdditional.OA_StateInfo);
			addressAdditional.OA_State = "NSW";
			addressAdditional.Validation.ValidateOA_State();
			AssertNoErrors(addressAdditional.OA_StateInfo);

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			addressAdditional.ValidationStatus = "MAN";
			addressAdditional.OA_State = string.Empty;
			AssertHasErrors(addressAdditional.OA_StateInfo);
			addressAdditional.ValidationStatus = "MAN";
			addressAdditional.OA_State = "XXX";
			AssertHasErrors(addressAdditional.OA_StateInfo);
			addressAdditional.ValidationStatus = "MAN";
			addressAdditional.OA_State = "NSW";
			AssertNoErrors(addressAdditional.OA_StateInfo);
		}

		#endregion

		#region OA_RN_NKCountryCode

		public void TestOA_RN_NKCountryCode()
		{
			address.OA_RN_NKCountryCode = string.Empty;
			AssertHasErrors("Country must not be entered", address.OA_RN_NKCountryCodeInfo);

			address.OA_RN_NKCountryCode = "AU";
			AssertNoErrors("Country must not be entered", address.OA_RN_NKCountryCodeInfo);
		}

		#endregion

		#region OA_Postcode

		public void TestCheckOA_Postcode()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			address.Header.OH_RL_NKClosestPort = "SGSIN";

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			address.OA_PostCode = "";
			address.OA_PostCode = "XBX";
			AssertNoErrors(address.OA_PostCodeInfo);
		}

		public void TestCheckPostCodeAndStateInCasesCountrySepcificRulesApplying()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;

			address.Header.OH_RL_NKClosestPort = "AUSYD";
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			address.OA_PostCode = "";
			address.ValidationStatus = "MAN";
			address.Validation.ValidateOA_PostCode();
			AssertHasErrors(address.OA_PostCodeInfo);

			address.OA_State = "";
			address.ValidationStatus = "MAN";
			address.Validation.ValidateOA_State();
			AssertHasErrors(address.OA_StateInfo);

			address.OA_PostCode = "";
			address.ValidationStatus = "UNV";
			address.Validation.ValidateOA_PostCode();
			AssertHasErrors(address.OA_PostCodeInfo);

			address.OA_State = "";
			address.ValidationStatus = "UNV";
			address.Validation.ValidateOA_State();
			AssertHasErrors(address.OA_StateInfo);

			address.OA_PostCode = "";
			address.ValidationStatus = "CNA";
			address.Validation.ValidateOA_PostCode();
			AssertHasErrors(address.OA_PostCodeInfo);

			address.OA_State = "";
			address.ValidationStatus = "CNA";
			address.Validation.ValidateOA_State();
			AssertHasErrors(address.OA_StateInfo);
		}

		public void TestCheckPostcodeMustBeFormattedInCasesCountrySpecificRulesApplying()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeFormatted;
			var countrySpecificRulesApplying = new string[] { AddressValidationStatus.ManuallyVerified, AddressValidationStatus.Unverifiable, AddressValidationStatus.CountryNotAvailable, AddressValidationStatus.ToBeVerified, AddressValidationStatus.ExcludeBackgroundValidation };

			foreach (var status in countrySpecificRulesApplying)
			{
				address.Postcode = "";
				address.ValidationStatus = status;
				address.Validation.ValidateOA_PostCode();
				AssertHasErrors(string.Format("empty postcode should have error for status \"{0}\"", status), address.OA_PostCodeInfo);

				address.Postcode = "invalid";
				address.ValidationStatus = status;
				address.Validation.ValidateOA_PostCode();
				AssertHasErrors(string.Format("invalid postcode should have error for status \"{0}\"", status), address.OA_PostCodeInfo);

				address.Postcode = "1234";
				address.ValidationStatus = status;
				address.Validation.ValidateOA_PostCode();
				AssertNoErrors(string.Format("valid postcode should have no errors for status \"{0}\"", status), address.OA_PostCodeInfo);
			}
		}

		public void TestPostcodeMustBeEnteredHasWarningForInvalidPostcode()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			var countrySpecificRulesApplying = new string[] { AddressValidationStatus.ManuallyVerified, AddressValidationStatus.Unverifiable, AddressValidationStatus.CountryNotAvailable, AddressValidationStatus.ToBeVerified, AddressValidationStatus.ExcludeBackgroundValidation };

			foreach (var status in countrySpecificRulesApplying)
			{
				address.Postcode = "invalid";
				address.ValidationStatus = status;
				address.Validation.ValidateOA_PostCode();
				AssertHasWarning(string.Format("invalid postcode with Must Be Entered validation should have warning for status \"{0}\"", status), address.OA_PostCodeInfo, InvalidPostcodeFormattingWarning(au));
			}
		}

		string InvalidPostcodeFormattingWarning(RefCountry country)
		{
			return string.Format("The entered postcode does not comply with the postcode format rules of the country/region ({0}). The postcode validation rule for the country/region {1} is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain->Locations->Countries/Regions.", country.PostcodeFormattingRule.Format, country.RN_DescMultilingual);
		}

		#endregion

		#region OA_Phone_Formatted

		public void TestRequirePhoneOrBusinessNumber()
		{
			org.OH_IsConsignee = true;
			Env.Registry.SetOrgConsigneeRequiredFields(RequiredFieldsLocalBusinessNoOrPhone);

			org.PrimaryRegistrationNumber.Number = ZString.Empty;
			address.OA_Phone_Formatted = ZString.Empty;

			address.Validation.ValidateOA_Phone_Formatted();

			AssertHasErrors("Either phone or local business number should be required", address.OA_Phone_FormattedInfo);

			org.PrimaryRegistrationNumber.Number = "23213";
			address.Validation.ValidateOA_Phone_Formatted();
			AssertNoErrors("Local Business Number entered, phone should not be required", address.OA_Phone_FormattedInfo);

			org.PrimaryRegistrationNumber.Number = ZString.Empty;
			address.OA_Phone_Formatted = "0280012200";
			address.Validation.ValidateOA_Phone_Formatted();
			AssertNoErrors("Phone entered, should not have errors", address.OA_Phone_FormattedInfo);

			org.OH_IsConsignee = false;

			org.PrimaryRegistrationNumber.Number = ZString.Empty;
			address.OA_Phone_Formatted = ZString.Empty;
			address.Validation.ValidateOA_Phone_Formatted();
			AssertNoErrors("Neither phone or local business number should be required", address.OA_Phone_FormattedInfo);

			org.PrimaryRegistrationNumber.Number = "23213";
			address.Validation.ValidateOA_Phone_Formatted();
			AssertNoErrors("Neither phone or local business number should be required", address.OA_Phone_FormattedInfo);

			org.PrimaryRegistrationNumber.Number = ZString.Empty;
			address.OA_Phone_Formatted = "0280012200";
			address.Validation.ValidateOA_Phone_Formatted();
			AssertNoErrors("Neither phone or local business number should be required", address.OA_Phone_FormattedInfo);
		}

		public void TestCheckOA_Phone_Formatted()
		{
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			address.OA_Phone_Formatted = "0296654455";
			AssertNoErrors("Valid phone number, no errors expected", address.OA_Phone_FormattedInfo);

			address.OA_RL_NKRelatedPortCode = string.Empty;
			address.OA_RN_NKCountryCode = "CN";
			address.OA_Phone_Formatted = "0296654455";
			AssertHasErrors("Invalid phone number with different country code, error expected", address.OA_Phone_FormattedInfo);

			address.IgnorePhoneNumberError = true;
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Phone_Formatted = "02966555";
			AssertNoErrors("Number is invalid but error is ignored, no errors expected", address.OA_Phone_FormattedInfo);

			address.IgnorePhoneNumberError = false;
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Phone_Formatted = "02966544";
			AssertHasErrors("Incorrect number of digits, error expected", address.OA_Phone_FormattedInfo);

			address.OA_Phone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", address.OA_Phone_FormattedInfo);
		}

		public void TestCheckOA_Phone_Formatted_Warning()
		{
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Env.Registry.SetDowngradeInvalidPhoneNumbersToAWarning(false);
			address.OA_Phone_Formatted = "02966544";
			AssertHasErrors("Incorrect number of digits, error expected", address.OA_Phone_FormattedInfo);

			Env.Registry.SetDowngradeInvalidPhoneNumbersToAWarning(true);
			address.OA_Phone_Formatted = "0296";
			AssertNoErrors("Incorrect format, but no errors expected", address.OA_Phone_FormattedInfo);
			AssertHasWarnings("Incorrect format, warning expected", address.OA_Phone_FormattedInfo);

			address.OA_Phone_Formatted = "0296654455";
			AssertNoNotifications("Valid phone number, no notifications expected", address.OA_Phone_FormattedInfo);
		}

		#endregion

		#region OA_Fax_Formatted

		public void TestCheckOA_Fax_Formatted()
		{
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			address.OA_Fax_Formatted = "0296654455";
			AssertNoErrors("Valid phone number, no errors expected", address.OA_Fax_FormattedInfo);

			address.OA_RL_NKRelatedPortCode = string.Empty;
			address.OA_RN_NKCountryCode = "CN";
			address.OA_Fax_Formatted = "0296654455";
			AssertHasErrors("Invalid phone number with different country code, error expected", address.OA_Fax_FormattedInfo);

			address.IgnoreFaxNumberError = true;
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Fax_Formatted = "02966555";
			AssertNoErrors("Number is invalid but error is ignored, no errors expected", address.OA_Fax_FormattedInfo);

			address.IgnoreFaxNumberError = false;
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Fax_Formatted = "02966544";
			AssertHasErrors("Incorrect number of digits, error expected", address.OA_Fax_FormattedInfo);

			address.OA_Fax_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", address.OA_Fax_FormattedInfo);
		}

		public void TestCheckOA_Fax_FormattedRequired()
		{
			org.OH_IsConsignee = true;
			Env.Registry.SetOrgConsigneeRequiredFields(RequiredFieldsFaxOnly);

			// Required for Main fAddress
			address.OA_Fax_Formatted = "+61280012200";
			address.RunPreSaveValidation();
			AssertNoErrors("Fax is fine", address.OA_Fax_FormattedInfo);

			// Required (but invalid) for Main fAddress
			address.OA_Fax_Formatted = string.Empty;
			address.RunPreSaveValidation();
			AssertHasErrors("Fax is required", address.OA_Fax_FormattedInfo);

			// Not required for non-main fAddress
			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled("PST"); // Other fAddress
			address2.OA_Fax_Formatted = string.Empty;
			address2.RunPreSaveValidation();
			AssertNoErrors("Fax is not required", address2.OA_Fax_FormattedInfo);
		}

		#endregion

		#region OA_Mobile_Formatted

		public void TestCheckOA_Mobile_Formatted()
		{
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			address.OA_Mobile_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", address.OA_Mobile_FormattedInfo);

			address.OA_RL_NKRelatedPortCode = string.Empty;
			address.OA_RN_NKCountryCode = "CN";
			address.OA_Mobile_Formatted = "0426829924";
			AssertHasErrors("Invalid phone number with different country code, error expected", address.OA_Mobile_FormattedInfo);

			address.OA_RN_NKCountryCode = "AU";
			address.OA_Mobile_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, warning expected", address.OA_Mobile_FormattedInfo);

			address.OA_Mobile_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", address.OA_Mobile_FormattedInfo);
		}

		#endregion

		#region OA_Email

		public void TestCheckOA_Email()
		{
			org.OH_IsConsignee = true;
			Env.Registry.SetOrgConsigneeRequiredFields(RequiredFieldsEmailOnly);

			// Required for Main fAddress
			address.OA_Email = "test@test.com";
			address.RunPreSaveValidation();
			AssertNoErrors("Email is fine", address.OA_EmailInfo);

			// Required (but invalid) for Main fAddress
			address.OA_Email = string.Empty;
			address.RunPreSaveValidation();
			AssertHasErrors("Email is required", address.OA_EmailInfo);

			// Unrequired for non-main fAddress
			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled("PST"); // Other fAddress
			address2.OA_Email = string.Empty;
			address2.RunPreSaveValidation();
			AssertNoErrors("Email is not required", address2.OA_EmailInfo);

			address2.OA_Email = "invalid";
			AssertHasErrors("Email is not required, but was entered invalid", address2.OA_EmailInfo);

			address2.OA_Email = "valid@example.com";
			AssertNoErrors("Email is not required, and is valid", address2.OA_EmailInfo);

			address2.OA_Email = string.Empty;
			AssertNoErrors("Email is not required, and is empty", address2.OA_EmailInfo);
		}

		#endregion

		#region Fax / Email / Web

		public void TestCheckRequireFaxEmailOrWeb()
		{
			org.OH_IsConsignee = true;
			Env.Registry.SetOrgConsigneeRequiredFields(RequiredFieldsFaxEmailOrWeb);

			address.Header.MainWebURL.PU_URL = string.Empty;
			address.OA_Email = "test@test.com";
			address.OA_Fax_Formatted = string.Empty;
			address.RunPreSaveValidation();
			AssertNoErrors("Fax/Email/Web is supplied", address.OA_EmailInfo);
			AssertNoErrors("Fax/Email/Web is not required", address.OA_Fax_FormattedInfo);
			AssertNoErrors("Fax/Email/Web is not required", address.Header.MainWebURL.PU_URLInfo);

			address.Header.MainWebURL.PU_URL = string.Empty;
			address.OA_Email = string.Empty;
			address.OA_Fax_Formatted = "+61280012200";
			address.RunPreSaveValidation();
			AssertNoErrors("Fax/Email/Web is not required", address.OA_EmailInfo);
			AssertNoErrors("Fax/Email/Web is supplied", address.OA_Fax_FormattedInfo);
			AssertNoErrors("Fax/Email/Web is not required", address.Header.MainWebURL.PU_URLInfo);

			address.Header.MainWebURL.PU_URL = "http://www.example.com";
			address.OA_Email = string.Empty;
			address.OA_Fax_Formatted = string.Empty;
			address.RunPreSaveValidation();
			AssertNoErrors("Fax/Email/Web is not required", address.OA_EmailInfo);
			AssertNoErrors("Fax/Email/Web is not required", address.OA_Fax_FormattedInfo);
			AssertNoErrors("Fax/Email/Web is supplied", address.Header.MainWebURL.PU_URLInfo);

			address.Header.MainWebURL.PU_URL = string.Empty;
			address.OA_Email = string.Empty;
			address.OA_Fax_Formatted = string.Empty;
			address.RunPreSaveValidation();
			AssertHasErrors("Fax/Email/Web is required", address.OA_EmailInfo);
			AssertHasErrors("Fax/Email/Web is required", address.OA_Fax_FormattedInfo);
			AssertHasErrors("Fax/Email/Web is required", address.Header.MainWebURL.PU_URLInfo);

			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled("PST"); // Other fAddress
			address2.Header.MainWebURL.PU_URL = string.Empty;
			address2.OA_Email = string.Empty;
			address2.OA_Fax_Formatted = string.Empty;
			address.RunPreSaveValidation();
			AssertNoErrors("Fax/Email/Web is not required", address2.OA_EmailInfo);
			AssertNoErrors("Fax/Email/Web is not required", address2.OA_Fax_FormattedInfo);
			AssertHasErrors("Fax/Email/Web is required", address2.Header.MainWebURL.PU_URLInfo); // Fax/Email are fAddress Type dependant - MainWebURL.PU_URL is not.
		}

		#endregion

		#region OA_AirEquipmentNeeded

		public void TestOA_AirEquipmentNeeded()
		{
			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_AIREquipmentNeededInfo);
			addr.OA_AIREquipmentNeeded = string.Empty;
			AssertHasErrors("Mandatory", addr.OA_AIREquipmentNeededInfo);

			addr.OA_AIREquipmentNeeded = addr.Lookups.CartageEquipmentNeededAir[0].Code;
			AssertNoErrors(addr.OA_AIREquipmentNeededInfo);

			addr.OA_AIREquipmentNeeded = "ZAZ";
			AssertHasErrors("List validation", addr.OA_AIREquipmentNeededInfo);
		}

		public void TestOA_AirEquipmentNeeded_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_AIREquipmentNeededInfo);
			addr.OA_AIREquipmentNeeded = string.Empty;
			AssertNoErrors("Mandatory", addr.OA_AIREquipmentNeededInfo);

			addr.OA_AIREquipmentNeeded = addr.Lookups.CartageEquipmentNeededAir[0].Code;
			AssertNoErrors(addr.OA_AIREquipmentNeededInfo);

			addr.OA_AIREquipmentNeeded = "ZAZ";
			AssertNoErrors("List validation", addr.OA_AIREquipmentNeededInfo);
		}

		#endregion

		#region OA_FCLEquipmentNeeded

		public void TestOA_FCLEquipmentNeeded()
		{
			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_FCLEquipmentNeededInfo);
			addr.OA_FCLEquipmentNeeded = string.Empty;
			AssertHasErrors("Mandatory", addr.OA_FCLEquipmentNeededInfo);

			addr.OA_FCLEquipmentNeeded = addr.Lookups.CartageEquipmentNeededFCL[0].Code;
			AssertNoErrors(addr.OA_FCLEquipmentNeededInfo);

			addr.OA_FCLEquipmentNeeded = "ZAZ";
			AssertHasErrors("List validation", addr.OA_FCLEquipmentNeededInfo);
		}

		public void TestOA_FCLEquipmentNeeded_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_FCLEquipmentNeededInfo);
			addr.OA_FCLEquipmentNeeded = string.Empty;
			AssertNoErrors("Mandatory", addr.OA_FCLEquipmentNeededInfo);

			addr.OA_FCLEquipmentNeeded = addr.Lookups.CartageEquipmentNeededFCL[0].Code;
			AssertNoErrors(addr.OA_FCLEquipmentNeededInfo);

			addr.OA_FCLEquipmentNeeded = "ZAZ";
			AssertNoErrors("List validation", addr.OA_FCLEquipmentNeededInfo);
		}

		#endregion

		#region OA_LCLEquipmentNeeded

		public void TestOA_LCLEquipmentNeeded()
		{
			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_LCLEquipmentNeededInfo);
			addr.OA_LCLEquipmentNeeded = string.Empty;
			AssertHasErrors("Mandatory", addr.OA_LCLEquipmentNeededInfo);

			addr.OA_LCLEquipmentNeeded = addr.Lookups.CartageEquipmentNeededLCL[0].Code;
			AssertNoErrors(addr.OA_LCLEquipmentNeededInfo);

			addr.OA_LCLEquipmentNeeded = "ZAZ";
			AssertHasErrors("List validation", addr.OA_LCLEquipmentNeededInfo);
		}

		public void TestOA_LCLEquipmentNeeded_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			OrgAddress addr = AddNewAddress(OrgAddressType.Postal, true);

			AssertNoErrors("Precondition", addr.OA_LCLEquipmentNeededInfo);
			addr.OA_LCLEquipmentNeeded = string.Empty;
			AssertNoErrors("Mandatory", addr.OA_LCLEquipmentNeededInfo);

			addr.OA_LCLEquipmentNeeded = addr.Lookups.CartageEquipmentNeededLCL[0].Code;
			AssertNoErrors(addr.OA_LCLEquipmentNeededInfo);

			addr.OA_LCLEquipmentNeeded = "ZAZ";
			AssertNoErrors("List validation", addr.OA_LCLEquipmentNeededInfo);
		}

		#endregion

		#region OA_CompanyNameOverride

		public void TestCheckOA_CompanyNameOverride()
		{
			org.OH_FullName = "~~~ABCD~~~";
			address.OA_Address1 = "Test fAddress";
			address.Validation.ValidateOA_CompanyNameOverride();
			Assert("Valid fAddress, no errors", !address.OA_CompanyNameOverrideInfo.HasErrors());

			org.OH_FullName = "~~~" + char.ConvertFromUtf32(21271) + char.ConvertFromUtf32(24038);
			address.Validation.ValidateOA_CompanyNameOverride();
			Assert("should no longer have errors", !address.OA_CompanyNameOverrideInfo.HasErrors());

			AssertEnglishValidationCheckForEnglishAddresses(address, address.OA_CompanyNameOverrideInfo);
		}

		#endregion

		#region OA_RL_NKRelatedPortCode

		public void TestOA_RL_NKRelatedPortCode()
		{
			Type type = typeof(BusinessObject);
			FieldInfo rowField = type.GetField("Row", BindingFlags.Instance | BindingFlags.NonPublic);
			DataRow oaRow = rowField.GetValue(address) as DataRow;
			oaRow["OA_RL_NKRelatedPortCode"] = "AUSYD";

			DataRow ohRow = rowField.GetValue(org) as DataRow;
			ohRow["OH_RL_NKClosestPort"] = "USLAX";

			address.Validation.ValidateOA_RL_NKRelatedPortCode();
			Assert("Should have error with ports mismatch", address.OA_RL_NKRelatedPortCodeInfo.HasErrors());

			ohRow["OH_RL_NKClosestPort"] = "AUSYD";
			address.Validation.ValidateOA_RL_NKRelatedPortCode();
			Assert("Shouldn't have error as ports match", !address.OA_RL_NKRelatedPortCodeInfo.HasErrors());

			OrgAddress newmain = address.Header.Addresses.AddNew();
			address.Header.Addresses.SwapMainAddress(address, newmain);
			ohRow["OH_RL_NKClosestPort"] = "UAIEV";
			address.Validation.ValidateOA_RL_NKRelatedPortCode();
			Assert("Shouldn't have error as ports mismatch, but it is not a main address", !address.OA_RL_NKRelatedPortCodeInfo.HasErrors());
		}

		public void TestOA_RL_NKRelatedPortCodeMandatory()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = string.Empty;
			AssertHasError(mainAddress.OA_RL_NKRelatedPortCodeInfo, "Please enter a Related City/Port.");
		}

		#endregion

		#region OA_ValidationStatus

		public void TestOA_ValidationStatus()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			address.OA_RN_NKCountryCode = "AU";
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertHasError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Instance.Registry.EnableAddressValidationWebService = false;
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
		}

		public void TestNoNeedToValidateInactiveAddresses()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				var address = Factory.New<OrgAddress>();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				address.ClearAllNotifications();

				address.OA_IsActive = false;
				address.Validation.ValidateOA_ValidationStatus();
				AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}

			ErrorReporter.Clear();
		}

		public void TestAddWarningWhenAddressIsTemporaryOrgAddress()
		{
			var rawRegistryValue = DataRegistry.Instance.EnableAddressValidationWebService;

			try
			{
				DataRegistry.Instance.EnableAddressValidationWebService = true;
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				{
					var infoMsg = "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.";

					address.IsTemporaryOrgAddress = true;
					address.OA_ValidationStatus = AddressValidationStatus.Invalid;
					AssertNoError(address.OA_ValidationStatusInfo, infoMsg);
					AssertHasWarning(address.OA_ValidationStatusInfo, infoMsg);
				}
			}
			finally
			{
				DataRegistry.Instance.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		#endregion

		#region TestValidateOrgHeaderHasAtLeastOneActiveAddress

		public void TestValidateOrgHeaderHasAtLeastOneActiveAddress()
		{
			org.Addresses.AddNew();
			org.Addresses.AddNew();

			foreach (OrgAddress address in org.Addresses)
			{
				address.RunPreSaveValidation();
				AssertNoErrors(address.OA_IsActiveInfo);
			}

			foreach (OrgAddress address in org.Addresses)
			{
				address.OA_IsActive = false;
			}

			foreach (OrgAddress address in org.Addresses)
			{
				address.RunPreSaveValidation();
				AssertHasErrors(address.OA_IsActiveInfo);
			}

			org.Addresses[0].OA_IsActive = true;
			foreach (OrgAddress address in org.Addresses)
			{
				address.RunPreSaveValidation();
				AssertNoErrors(address.OA_IsActiveInfo);
			}
		}

		#endregion

		#region TestRegistratedAddressCannotBeInactive

		public void TestRegistratedAddressCannotBeInactive()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_IsActive = true;
			org.CustomsCodes.AddNew().OK_OA_PremisesAddress = address.PK;

			address.OA_IsActive = false;
			AssertHasError(address.OA_IsActiveInfo, "Addresses set in 'Registration Numbers / Codes' cannot be inactive.");

			address.OA_IsActive = true;
			AssertNoErrors(address.OA_IsActiveInfo);
		}

		#endregion

		#region TestMainAddressCannotBeInactive

		public void TestMainAddressCannotBeInactive()
		{
			OrgAddress address1 = org.Addresses[0];
			OrgAddress address2 = org.Addresses.AddNew();
			OrgAddress address3 = org.Addresses.AddNew();

			Assert(address1.IsMainAddress);
			address1.OA_IsActive = false;
			AssertHasErrors(address1.OA_IsActiveInfo);

			Assert(!address3.IsMainAddress);
			address3.OA_IsActive = false;
			AssertNoErrors(address3.OA_IsActiveInfo);
		}

		#endregion

		#region TestAWBAddressFields

		public void TestAWBAddressFields()
		{
			const string alphaError = "This address is marked as an AWB address, and must only contain alphabetical characters in this field.";
			const string textError = "This address is marked as an AWB address, and must only contain alphanumeric characters and prescribed special characters in this field.";

			const string alphaValue = "ABC";
			const string textValue = "A1 B2-C3.";
			const string extraCharactersValue = "@blah!$";

			Action<ZPropertyInfo> assertFieldIsAlphaOnly = info => AssertAWBValidation(info, new ZString[] { alphaValue }, new ZString[] { textValue }, alphaError);
			Action<ZPropertyInfo> assertFieldIsTextOnly = info => AssertAWBValidation(info, new ZString[] { textValue }, new ZString[] { extraCharactersValue }, textError);

			var address = Factory.NewWithValidTestData<OrgAddress>();
			CombineAssertions(() =>
			{
				assertFieldIsAlphaOnly(address.OA_RN_NKCountryCodeInfo);
				assertFieldIsTextOnly(address.OA_PostCodeInfo);
				assertFieldIsTextOnly(address.OA_CompanyNameOverrideInfo);
				assertFieldIsTextOnly(address.OA_Address1Info);
				assertFieldIsTextOnly(address.OA_Address2Info);
				assertFieldIsTextOnly(address.OA_CityInfo);
				assertFieldIsTextOnly(address.OA_StateInfo);
			});
		}

		public void TestAWBAddressStatePostCodeValidation()
		{
			Func<string, string> errorMessageGetter = (name) => string.Format("{0} is not valid for AWB address.", name);
			var validValues = new List<ZString> { "", "AAAA", "ABC", "plain text", "123abc45", "AAA", "B.B.B", "b7", "A B", };
			var invalidValue = new List<ZString> { ".", "..", "x", "X", "-", "TBA" };

			var stateValidValues = new List<ZString> { "", "Guang zhou", "Jiangsu", "1" };
			var stateInvalidValues = new List<ZString> { ".", "-." };

			var postCodeValidValues = new List<ZString> { "", "abc-123", "111125" };
			var postCodeInvalidValues = new List<ZString> { ".", "-." };

			var address = Factory.NewWithValidTestData<OrgAddress>();
			CombineAssertions(() =>
			{
				AssertAWBValidation(address.OA_CompanyNameOverrideInfo, validValues, invalidValue, errorMessageGetter("Company Name"));
				AssertAWBValidation(address.OA_Address1Info, validValues, invalidValue, errorMessageGetter("Address 1"));
				AssertAWBValidation(address.OA_Address2Info, validValues, invalidValue, errorMessageGetter("Address 2"));
				AssertAWBValidation(address.OA_CityInfo, validValues, invalidValue, errorMessageGetter("City"));
				AssertAWBValidation(address.OA_StateInfo, stateValidValues, stateInvalidValues, errorMessageGetter("State"));
				AssertAWBValidation(address.OA_PostCodeInfo, postCodeValidValues, postCodeInvalidValues, errorMessageGetter("Postcode"));
			});
		}

		void AssertAWBValidation(ZPropertyInfo info, IEnumerable<ZString> validValues, IEnumerable<ZString> invalidValues, string expectedError)
		{
			var address = (OrgAddress)info.BizObj;

			address.AddressCapability.SetCapabilityDisabled(OrgAddressType.AWB.Code);
			info.Value = invalidValues.First().Left(info.MaxLength);
			AssertNoError(info, expectedError);

			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB.Code);

			foreach (var validValue in validValues)
			{
				info.Value = validValue.Left(info.MaxLength);
				AssertNoError(info, expectedError);
			}

			foreach (var invalidValue in invalidValues)
			{
				info.Value = invalidValue.Left(info.MaxLength);
				AssertHasError(info, expectedError);
			}
		}

		#endregion

		public void TestTSAKnownShipperAddressWarning()
		{
			string expectedError = "The address is a US TSA Known Shipper address. Changing it may cause TSA record inconsistency. All previous approved TSA Known Shipper linked to this address will be changed to not approved when you click the save button. Refer to the TSA Known Shipper tab which shows the details that have been last reviewed with TSA.";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var orgCountryDataTSARecord = Factory.New<OrgCountryData>();
			orgCountryDataTSARecord.OV_OA_ApprovedLocation = address.PK;
			orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "No";
			orgCountryDataTSARecord.OV_EXApprovalNumber = "1234";
			orgCountryDataTSARecord.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			orgCountryDataTSARecord.OV_OH_OrgHeader = address.OA_OH;
			Factory.Save();

			address.Address1 = "Test Address 1";
			AssertHasWarning(address.Address1Info, expectedError);
			address.Address2 = "Test Address 2";
			AssertHasWarning(address.Address2Info, expectedError);
			address.City = "Test City";
			AssertHasWarning(address.CityInfo, expectedError);
			address.Postcode = "0000";
			AssertHasWarning(address.PostcodeInfo, expectedError);
			address.State = "Test State";
			AssertHasWarning(address.StateCodeInfo, expectedError);

			orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "Yes";
			Factory.Save();

			address.Address1 = "Address Line 1";
			AssertHasWarning(address.Address1Info, expectedError);
			address.Address2 = "Address Line 2";
			AssertHasWarning(address.Address2Info, expectedError);
			address.City = "City";
			AssertHasWarning(address.CityInfo, expectedError);
			address.Postcode = "9999";
			AssertHasWarning(address.PostcodeInfo, expectedError);
			address.State = "State";
			AssertHasWarning(address.StateCodeInfo, expectedError);
		}

		public void TestMIDAddressWarning()
		{
			string expectedError = "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number.";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var orgCusCodeMIDRecord = Factory.New<OrgCusCode>();
			orgCusCodeMIDRecord.OK_OA_PremisesAddress = address.PK;
			orgCusCodeMIDRecord.OK_CodeType = "ABR";
			orgCusCodeMIDRecord.OK_CustomsRegNo = "abcd1234";
			orgCusCodeMIDRecord.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCodeMIDRecord.OK_OH = address.OA_OH;
			Factory.Save();

			address.Address1 = "Test Address 1";
			AssertNoWarning(address.Address1Info, expectedError);
			address.Address2 = "Test Address 2";
			AssertNoWarning(address.Address2Info, expectedError);
			address.City = "Test City";
			AssertNoWarning(address.CityInfo, expectedError);
			address.Postcode = "0000";
			AssertNoWarning(address.PostcodeInfo, expectedError);
			address.State = "Test State";
			AssertNoWarning(address.StateCodeInfo, expectedError);

			orgCusCodeMIDRecord.OK_CodeType = "MID";
			Factory.Save();

			address.Address1 = "Address Line 1";
			AssertHasWarning(address.Address1Info, expectedError);
			address.Address2 = "Address Line 2";
			AssertHasWarning(address.Address2Info, expectedError);
			address.City = "City";
			AssertHasWarning(address.CityInfo, expectedError);
			address.Postcode = "9999";
			AssertHasWarning(address.PostcodeInfo, expectedError);
			address.State = "State";
			AssertHasWarning(address.StateCodeInfo, expectedError);
		}

		public void TestDoNotCheckAddressIfUserHasNoPermissonToModify()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_City = "City";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_ValidationStatus = AddressValidationStatus.Invalid;
			Factory.Save();

			var validation = new OrgAddressValidation(address);
			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			validation.ValidateOA_ValidationStatus();
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
			validation.ValidateOA_ValidationStatus();
			AssertHasError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = false;
			validation.ValidateOA_ValidationStatus();
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = true;
			validation.ValidateOA_ValidationStatus();
			AssertHasError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = true;
			address.IgnoreValidationStatusError = true;
			validation.ValidateOA_ValidationStatus();
			AssertNoError(address.OA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
		}

		#region Implementation

		void AssertEnglishValidationCheckForEnglishAddresses(OrgAddress address, ZPropertyInfo info)
		{
			address.OA_Language = Core.Constants.Languages.EnglishAmerican;
			info.Value = (ZString)"abcd";
			AssertNoErrors("No errors as value was english", info);

			info.Value = (ZString)("abc" + char.ToString((char)300));
			AssertHasErrors("Errors as value was not english and address was marked as english", info);

			address.OA_Language = Core.Constants.Languages.EnglishAmerican;
			info.Value = (ZString)"Français";
			AssertHasErrors("Errors as value was not english (that was French!) and address was marked as english", info);

			address.OA_Language = Core.Constants.Languages.German;
			info.Value = (ZString)"Die Straße";
			AssertNoErrors("No errors as value was not english, but neither was address", info);

			address.OA_Language = Core.Constants.Languages.Gujarati;
			info.Value = (ZString)("abc" + char.ToString((char)290));
			AssertNoErrors("No errors as value was not english, but neither was address", info);

			Factory.Save();

			address.OA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertHasErrors("Language set back to English, info value hasn't change, but should still be revalidated", info);
		}

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.New<OrgHeader>();
			org.OH_Code = "Mr K. RULEZ!";
			org.OH_RL_NKClosestPort = "AUSYD";
			address = org.MainAddress;
			address.OA_Address1 = "ADDRESS1";
		}

		OrgHeader org;
		OrgAddress address;

		OrgAddress AddNewAddress(OrgAddressType type, ZBool isDefault)
		{
			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(type.Code);
			address.OA_Address1 = "Test Address";
			if (isDefault)
			{
				address.AddressCapability.SetIsMainAddress(type.Code);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(type.Code);
			}

			return address;
		}

		OrgRequiredFields RequiredFieldsLocalBusinessNoOrPhone
		{
			get { return new OrgRequiredFields(false, false, false, false, false, true, false, false, false, false, false); }
		}

		OrgRequiredFields RequiredFieldsEmailOnly
		{
			get { return new OrgRequiredFields(false, false, false, false, false, false, false, true, false, false, false); }
		}

		OrgRequiredFields RequiredFieldsFaxOnly
		{
			get { return new OrgRequiredFields(false, false, false, false, false, false, true, false, false, false, false); }
		}

		OrgRequiredFields RequiredFieldsFaxEmailOrWeb
		{
			get { return new OrgRequiredFields(false, false, false, false, false, false, false, false, false, true, false); }
		}

		#endregion
	}
}
