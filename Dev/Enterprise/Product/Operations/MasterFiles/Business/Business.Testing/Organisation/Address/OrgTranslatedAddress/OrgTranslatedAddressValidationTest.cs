using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTranslatedAddressValidationTest : BusinessObjectValidationTestCase
	{
		#region OrgTranslatedAddressRowError

		public void TestOrgTranslatedAddressRowError()
		{
			translatedAddress1.Validation.ValidateAll();
			Assert(!translatedAddress1.RowErrors.Contains("Please access the faulty translation via the Translated Address drop down menu to rectify the error(s)."));

			translatedAddress1.OTA_Address1 = string.Empty;
			translatedAddress1.Validation.ValidateAll();
			Assert(translatedAddress1.RowErrors.Contains("Please access the faulty translation via the Translated Address drop down menu to rectify the error(s)."));

			translatedAddress1.OTA_Address1 = "EEE";
			translatedAddress1.Validation.ValidateAll();
			Assert(!translatedAddress1.RowErrors.Contains("Please access the faulty translation via the Translated Address drop down menu to rectify the error(s)."));
		}

		#endregion

		#region OTA_Address1

		public void TestCheckOTA_Address1()
		{
			translatedAddress1.OTA_Address1 = string.Empty;
			Assert("Empty address, has errors", translatedAddress1.OTA_Address1Info.HasErrors());

			translatedAddress1.OTA_Address1 = "Test address";
			Assert("Valid address, no errors", !translatedAddress1.OTA_Address1Info.HasErrors());

			AssertEnglishValidationCheckForEnglishAddresses(translatedAddress1, translatedAddress1.OTA_Address1Info);
		}

		void AssertEnglishValidationCheckForEnglishAddresses(OrgTranslatedAddress address, ZPropertyInfo info)
		{
			address.OTA_Language = Core.Constants.Languages.EnglishAmerican;
			info.Value = (ZString)"abc";
			AssertNoErrors("No errors as value was english", info);

			info.Value = (ZString)char.ToString((char)300);
			AssertHasErrors("Errors as value was not english and address was marked as english", info);

			address.OTA_Language = Core.Constants.Languages.EnglishAmerican;
			info.Value = (ZString)"Français";
			AssertHasErrors("Errors as value was not english (that was French!) and address was marked as english", info);

			address.OTA_Language = Core.Constants.Languages.German;
			info.Value = (ZString)"Die Straße";
			AssertNoErrors("No errors as value was not english, but neither was address", info);

			address.OTA_Language = Core.Constants.Languages.Gujarati;
			info.Value = (ZString)char.ToString((char)290);
			AssertNoErrors("No errors as value was not english, but neither was address", info);

			Factory.Save();

			address.OTA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertHasErrors("Language set back to English, info value hasn't change, but should still be revalidated", info);
		}

		#endregion

		#region OTA_Address2

		public void TestCheckOTA_Address2()
		{
			AssertEnglishValidationCheckForEnglishAddresses(translatedAddress1, translatedAddress1.OTA_Address2Info);
		}

		#endregion

		#region OTA_Language

		public void TestCheckOTA_Language()
		{
			translatedAddress1.OTA_Language = string.Empty;
			Assert("No language entered, has errors", translatedAddress1.OTA_LanguageInfo.HasErrors());

			translatedAddress1.OTA_Language = "EEE";
			Assert("Invalid code, has errors", translatedAddress1.OTA_LanguageInfo.HasErrors());

			translatedAddress1.OTA_Language = Core.SharedConstants.Languages.Swahili;
			Assert("Valid code, no errors", !translatedAddress1.OTA_LanguageInfo.HasErrors());

			translatedAddress1.OTA_Address1 = "Créteil";
			translatedAddress1.OTA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertHasError(translatedAddress1.OTA_LanguageInfo, "Non-English Characters detected in this address. Please select the proper language for this address.");
		}

		#endregion

		#region OTA_City

		public void TestCheckOTA_City()
		{
			translatedAddress1.OTA_City = "Test City";
			Assert("Valid City, no errors", !translatedAddress1.OTA_CityInfo.HasErrors());

			AssertEnglishValidationCheckForEnglishAddresses(translatedAddress1, translatedAddress1.OTA_CityInfo);
		}

		#endregion

		#region OTA_CompanyName

		public void TestCheckOTA_CompanyName()
		{
			AssertEnglishValidationCheckForEnglishAddresses(translatedAddress1, translatedAddress1.OTA_CompanyNameInfo);
		}

		#endregion

		#region OTA_ValidationStatus

		public void TestOTA_ValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<OrgAddress>();
			translatedAddress1 = address.AddNewTranslatedAddress();
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			address.OA_RN_NKCountryCode = "AU";
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertHasError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Verified;
			AssertNoError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Instance.Registry.EnableAddressValidationWebService = false;
			translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
		}

		public void TestNoNeedToValidateInactiveAddresses()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				var address = Factory.New<OrgAddress>();
				address.OA_RN_NKCountryCode = "AU";
				translatedAddress1 = address.AddNewTranslatedAddress();
				translatedAddress1.OTA_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				translatedAddress1.ClearAllNotifications();

				translatedAddress1.ParentAddress.OA_IsActive = false;
				address.Validation.ValidateOA_ValidationStatus();
				AssertNoError(translatedAddress1.OTA_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}

			ErrorReporter.Clear();
		}

		#endregion

		#region OA_State

		public void TestCheckOA_State()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;

			translatedAddress1.ParentAddress.OA_RN_NKCountryCode = "AU";

			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			address.Header.OH_RL_NKClosestPort = "AUSYD";
			address.OA_RN_NKCountryCode = "AU";

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			translatedAddress1.OTA_State = string.Empty;
			translatedAddress1.Validation.ValidateOTA_State();
			AssertHasErrors("State must be entered", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "XXX";
			translatedAddress1.Validation.ValidateOTA_State();
			AssertHasErrors("State must be valid", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "NSW";
			translatedAddress1.Validation.ValidateOTA_State();
			AssertNoErrors(translatedAddress1.OTA_StateInfo);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			translatedAddress1.OTA_State = string.Empty;
			translatedAddress1.Validation.ValidateOTA_State();
			AssertNoErrors("State can be empty as NoValidationRule is specified", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "XXX";
			translatedAddress1.Validation.ValidateOTA_State();
			AssertNoErrors("State can be invalid as NoValidationRule is specified", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "NSW";
			translatedAddress1.Validation.ValidateOTA_State();
			AssertNoErrors(translatedAddress1.OTA_StateInfo);

			RefCountry sg = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "SG");
			sg.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			address.Header.OH_RL_NKClosestPort = "SGAYC";
			address.OA_RN_NKCountryCode = "SG";
			translatedAddress1.OTA_State = string.Empty;
			translatedAddress1.Validation.ValidateOTA_State();
			AssertHasErrors(translatedAddress1.OTA_StateInfo);

			address.Header.OH_RL_NKClosestPort = "";
			AssertEnglishValidationCheckForEnglishAddresses(translatedAddress1, translatedAddress1.OTA_StateInfo);

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			address.Header.OH_RL_NKClosestPort = "AUSYD";
			address.OA_RN_NKCountryCode = "AU";

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			translatedAddress1.OTA_State = string.Empty;
			AssertNoErrors("State must be entered", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "XXX";
			AssertNoErrors("State must be valid", translatedAddress1.OTA_StateInfo);
			translatedAddress1.OTA_State = "NSW";
			AssertNoErrors(translatedAddress1.OTA_StateInfo);
		}

		#endregion

		#region Implementation

		OrgAddress address;
		OrgTranslatedAddress translatedAddress1;
		OrgTranslatedAddress translatedAddress2;

		protected override void SetUp()
		{
			base.SetUp();
			address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Language = Core.Constants.Languages.ChineseTraditional;
			translatedAddress1 = address.AddNewTranslatedAddress();
			translatedAddress1.FillWithValidTestData();
			translatedAddress2 = address.AddNewTranslatedAddress();
			translatedAddress2.FillWithValidTestData();
			translatedAddress2.OTA_Language = Core.Constants.Languages.French;
		}

		#endregion
	}
}
