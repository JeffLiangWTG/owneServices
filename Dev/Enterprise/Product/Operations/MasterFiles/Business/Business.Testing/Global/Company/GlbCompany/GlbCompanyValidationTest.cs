using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanyValidationTest : BusinessObjectValidationTestCase
	{
		#region GC_Code

		public void TestValidateGC_Code()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "";
			Assert("Error expected - Company Code is required", company1.GC_CodeInfo.HasErrors());
			company1.GC_Code = "ABC";
			Assert("No error expected", !company1.GC_CodeInfo.HasErrors());

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "ABC";
			company1.Validation.ValidateGC_Code();
			Assert("Error expected - Company Code should be unique", company1.GC_CodeInfo.HasErrors());
			Assert("Error expected - Company Code should be unique", company2.GC_CodeInfo.HasErrors());

			company2.GC_Code = "EFG";
			company1.Validation.ValidateGC_Code();
			Assert("No error expected", !company1.GC_CodeInfo.HasErrors());
			Assert("No error expected", !company2.GC_CodeInfo.HasErrors());

			company2.GC_Code = "AB";
			Assert("Error expected - Company Code length should be 3", company2.GC_CodeInfo.HasErrors());
			company2.GC_Code = "(#)";
			Assert("Error expected - Company Code should be character and digit only", company2.GC_CodeInfo.HasErrors());
			company2.GC_Code = "AB_";
			Assert("Error expected - Company Code should be character and digit only", company2.GC_CodeInfo.HasErrors());
			company2.GC_Code = "1$T";
			Assert("Error expected - Company Code should be character and digit only", company2.GC_CodeInfo.HasErrors());
			company2.GC_Code = "!AA";
			Assert("Error expected - Company Code should be character and digit only", company2.GC_CodeInfo.HasErrors());
			company2.GC_Code = "18Y";
			Assert("No error expected", !company2.GC_CodeInfo.HasErrors());
		}

		#endregion

		#region GC_OH

		public void TestValidateGC_OH()
		{
			GlbCompany company = Factory.New<GlbCompany>();

			company.GC_Code = GlbCompany.DemoCompanyCode;
			company.GC_OH_OrgProxy = ZGuid.Empty;
			Assert("No error expected", !company.GC_OH_OrgProxyInfo.HasErrors());
			company.GC_OH_OrgProxy = ZGuid.Invalid;
			Assert("Error expected - Organisation is not valid", company.GC_OH_OrgProxyInfo.HasErrors());

			company.GC_Code = "AAA";
			company.GC_OH_OrgProxy = ZGuid.Empty;
			Assert("Error expected - Organisation is required", company.GC_OH_OrgProxyInfo.HasErrors());
			company.GC_OH_OrgProxy = ZGuid.Invalid;
			Assert("Error expected - Organisation is not valid", company.GC_OH_OrgProxyInfo.HasErrors());
			company.GC_OH_OrgProxy = ZGuid.NewZGuid();
			Assert("No error expected", !company.GC_OH_OrgProxyInfo.HasErrors());
		}

		#endregion

		#region GC_RX_NKLocalCurrency

		public void TestCheckGC_RX_NKLocalCurrency()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "XXX";
			AssertHasErrors("Invalid Currency", company.GC_RX_NKLocalCurrencyInfo);

			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("Valid Currency", company.GC_RX_NKLocalCurrencyInfo);

			company.GC_RX_NKLocalCurrency = string.Empty;
			AssertHasErrors("Currency is mandatory", company.GC_RX_NKLocalCurrencyInfo);
		}

		public void TestCheckGC_RX_NKLocalCurrency_WithRefAccElectronicProcessingFee()
		{
			var fee = Factory.NewWithValidTestData<RefAccElectronicProcessingFee>();
			fee.EPF_Currency = Core.Constants.CurrencyCodes.China;
			fee.EPF_Code = "SHD";
			fee.EPF_Category = "STL";
			fee.EPF_SystemCode = "CWN";
			fee.EPF_ValidFrom = ZDateTime.BrettsBirthday;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("Valid Currency", company.GC_RX_NKLocalCurrencyInfo);

			ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.China;
			AssertNoErrors("Valid Currency without ElectronicProcessingCharge Enable", company.GC_RX_NKLocalCurrencyInfo);

			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			AssertHasErrors("Invalid Currency with ElectronicProcessingCharge Enable", company.GC_RX_NKLocalCurrencyInfo);
		}

		#endregion

		#region GC_StartDateIsValidZDateTimeRange

		public void TestGC_StartDateIsValidZDateTimeRange()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_StartDate = new ZDateTime(1899, 12, 31);
			string error = "Date must be within 01-JAN-1900 and 06-JUN-2079.";
			AssertHasErrors(error, company.GC_StartDateInfo);

			company.GC_StartDate = new ZDateTime(1900, 01, 01);
			AssertNoErrors(error, company.GC_StartDateInfo);

			company.GC_StartDate = new ZDateTime(2079, 06, 07);
			AssertHasErrors(error, company.GC_StartDateInfo);

			company.GC_StartDate = new ZDateTime(2079, 06, 06);
			AssertNoErrors(error, company.GC_StartDateInfo);

			company.GC_StartDate = new ZDateTime(2011, 11, 11);
			AssertNoErrors(error, company.GC_StartDateInfo);

			ExceptionReporterTestListener.Instance.Clear(); //we don't need to be notified that we set an invalid date.
		}

		#endregion

		#region GC_IsActive

		public void TestOrgHeaderIsActiveValidation_DeactivatingCompanyWithActiveBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;
			company.Branches.Add(branch);
			Factory.Save();
			Assert(branch.GB_Phone_FormattedInfo.HasError("Please enter a Phone Number."));

			company.GC_IsActive = false;
			Factory.Save();
			Assert(company.GC_IsActiveInfo.HasError("Deactivate all active branches before deactivating the global company"));

			branch.GB_IsActive = false;
			Factory.Save();
			Assert(!company.GC_IsActiveInfo.HasNotifications());

			branch.GB_IsActive = true;
			Factory.Save();
			Assert(branch.GB_IsActiveInfo.HasError("Activate the global company before activating company branches"));
		}

		#endregion

		#region Phone Numbers

		public void TestGC_Phone_Formatted()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";

			company.GC_Phone_Formatted = "4324ff";
			AssertHasErrors("Contains invalid characters. Error expected", company.GC_Phone_FormattedInfo);

			company.GC_Phone_Formatted = "135 2063 2715";
			AssertHasErrors("Invalid AU number. Error expected", company.GC_Phone_FormattedInfo);

			company.GC_Phone_Formatted = "0426 829 924";
			AssertNoNotifications("Valid phone number, no notifications expected", company.GC_Phone_FormattedInfo);

			company.GC_RN_NKCountryCode = string.Empty;
			company.GC_Phone_Formatted = "0426 829 924";
			AssertHasErrors("Invalid international number with empty country code. Error expected", company.GC_Phone_FormattedInfo);
		}

		public void TestGC_Phone_DoesNotValidateWhenInactive()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";
			company.GC_IsActive = false;
			company.GC_Phone_Formatted = "ZZZ";
			AssertEquals("No Error expected - Company is Inactive", false, company.GC_Phone_FormattedInfo.HasErrors());

			company.GC_IsActive = true;
			company.GC_Phone_Formatted = "ZZZ";
			AssertEquals("Error expected - Company is Active", true, company.GC_Phone_FormattedInfo.HasErrors());

			company.GC_Phone_Formatted = "0455555555";
			AssertEquals("No Error expected - Phone is valid", false, company.GC_Phone_FormattedInfo.HasErrors());
		}

		public void TestGC_Fax_Formatted()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";

			company.GC_Fax_Formatted = "4324ff";
			AssertHasErrors("Contains invalid characters. Error expected", company.GC_Fax_FormattedInfo);

			company.GC_Fax_Formatted = "135 2063 2715";
			AssertHasErrors("Invalid AU number. Error expected", company.GC_Fax_FormattedInfo);

			company.GC_Fax_Formatted = "0426 829 924";
			AssertNoNotifications("Valid phone number, no notifications expected", company.GC_Fax_FormattedInfo);

			company.GC_RN_NKCountryCode = string.Empty;
			company.GC_Fax_Formatted = "0426 829 924";
			AssertHasErrors("Invalid international number with empty country code. Error expected", company.GC_Fax_FormattedInfo);
		}

		#endregion

		#region GC_Name

		public void TestCheckGC_Name()
		{
			var company = Factory.New<GlbCompany>();

			company.GC_Name = "Demo company";
			AssertNoErrors(company.GC_NameInfo);

			company.GC_Name = "";
			AssertHasErrors(company.GC_NameInfo);
		}

		#endregion

		#region GC_RN_NKCountryCode

		public void TestCheckGC_RN_NKCountryCode()
		{
			RefCountry country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "AA";
			Factory.Save();

			Licences testLicence = new Licences();
			GlbCompany company = Factory.New<GlbCompany>();

			company.GC_RN_NKCountryCode = "AA";
			AssertHasErrors("Invalid Country", company.GC_RN_NKCountryCodeInfo);

			company.GC_RN_NKCountryCode = "XX";
			AssertHasErrors("Invalid Country", company.GC_RN_NKCountryCodeInfo);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertNoErrors("Valid Country", company.GC_RN_NKCountryCodeInfo);

			company.GC_RN_NKCountryCode = "AA";
			AssertHasErrors("Invalid Country", company.GC_RN_NKCountryCodeInfo);

			company.GC_RN_NKCountryCode = ZString.Empty;
			AssertHasErrors("Country is mandatory", company.GC_RN_NKCountryCodeInfo);
		}

		#endregion

		#region Adresses

		public void TestCheckGC_Address1()
		{
			GlbCompany company = Factory.New<GlbCompany>();

			company.GC_Address1 = "Address 1";
			AssertNoErrors("Valid Address 1", company.GC_Address1Info);

			company.GC_Address1 = ZString.Empty;
			AssertHasErrors("Address 1 is mandatory", company.GC_Address1Info);
		}

		public void TestGC_ValidationStatus()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_ValidationStatus = AddressValidationStatus.Invalid;
			AssertHasError(company.GC_ValidationStatusInfo, "The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			company.GC_ValidationStatus = AddressValidationStatus.Verified;
			AssertNoError(company.GC_ValidationStatusInfo, "The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Instance.Registry.EnableAddressValidationWebService = false;
			company.GC_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(company.GC_ValidationStatusInfo, "The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
		}

		public void TestNoNeedToValidateInactiveAddresses()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_IsActive = true;
				company.GC_RN_NKCountryCode = "AU";
				company.GC_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(company.GC_ValidationStatusInfo, "The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				company.ClearAllNotifications();

				company.GC_IsActive = false;
				company.Validation.ValidateGC_ValidationStatus();
				AssertNoError(company.GC_ValidationStatusInfo, "The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}

			ErrorReporter.Clear();
		}

		#endregion

		#region GC_City

		public void TestValidateCityWhenShouldValidateAddressIsFalse()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var company = Factory.New<GlbCompany>();

			company.GC_City = "Sydney";
			AssertNoErrors("Valid City", company.GC_CityInfo);

			company.GC_City = ZString.Empty;
			AssertHasErrors("City is mandatory", company.GC_CityInfo);
		}

		public void TestMandatoryValidationAppliesForCityWhenManuallyVerifying()
		{
			var company = Factory.New<GlbCompany>();

			Env.Instance.Registry.EnableAddressValidationWebService = true;

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.Verified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.Verified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			company.Validation.ValidateGC_City();
			AssertHasError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.ToBeVerified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.ToBeVerified;
			company.Validation.ValidateGC_City();
			AssertHasError(company.GC_CityInfo, "Please enter a City.");
		}

		#endregion

		#region GC_State

		public void TestCheckGC_State()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			GlbCompany company = Factory.New<GlbCompany>();
			AssertNoErrors(company.GC_StateInfo);

			company.GC_RN_NKCountryCode = "SG";
			RefCountry sg = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "SG");
			sg.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			company.GC_State = "AAAA";
			AssertNoErrors(company.GC_StateInfo);
			company.GC_State = "";
			AssertHasErrors(company.GC_StateInfo);
		}

		#endregion

		#region GC_Email

		public void TestGC_Email()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Email = "test@test.com";
			AssertNoErrors("Email is fine", company.GC_EmailInfo);

			company.GC_Email = "invalid";
			AssertHasErrors("Email is not required, but was entered invalid", company.GC_EmailInfo);

			company.GC_Email = string.Empty;
			AssertNoErrors("Email is not required, and is empty", company.GC_EmailInfo);
		}

		#endregion

		#region GC_WebAddress

		public void TestWhenGC_WebAddressIsEmpty()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BRA";
			Factory.Save();

			Env.Registry.WebBranch = branch.PK.ToGuid();

			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "ABC";
			branch.GB_GC = company1.PK;
			branch.Company.GC_Code = company1.GC_Code;
			Factory.Save();

			company1.GC_WebAddress = "";
			AssertHasErrors("When web address is used for web tracker, it must not be empty", company1.GC_WebAddressInfo);

			company1.GC_WebAddress = "www.cargowise.com";
			AssertNoErrors(company1.GC_WebAddressInfo);

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "DEF";
			company2.GC_WebAddress = "";
			AssertNoErrors("When web address is not used for web tracker,it can be empty", company2.GC_WebAddressInfo);
		}

		public void TestGC_WebAddress()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_WebAddress = "www.cargowise.com";
			AssertNoErrors(company.GC_WebAddressInfo);

			company.GC_WebAddress = "invalid";
			AssertHasErrors(company.GC_WebAddressInfo);

			company.GC_WebAddress = "";
			AssertNoErrors(company.GC_WebAddressInfo);
		}

		#endregion

		#region GC_BusinessRegNo

		public void TestGC_BusinessRegNo()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			company.GC_BusinessRegNo = "A000001";
			AssertNoWarnings(company.GC_BusinessRegNoInfo);

			company.GC_RN_NKCountryCode = "ES";
			company.GC_BusinessRegNo = "";
			AssertHasWarnings("For Spain login companies, you must record your companies NIF.", company.GC_BusinessRegNoInfo);

			company.GC_RN_NKCountryCode = "AU";
			company.GC_BusinessRegNo = "";
			AssertNoWarnings(company.GC_BusinessRegNoInfo);

			Factory.Save();

			company.GC_BusinessRegNo = "A000002";
			AssertHasWarnings(company.GC_BusinessRegNoInfo);
		}

		#endregion

		#region GC_BusinessRegNo2

		public void TestGC_BusinessRegNo2()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			company.GC_BusinessRegNo2 = "B000001";
			AssertNoWarnings(company.GC_BusinessRegNo2Info);

			company.GC_BusinessRegNo2 = "";
			AssertNoWarnings(company.GC_BusinessRegNo2Info);

			Factory.Save();

			company.GC_BusinessRegNo2 = "B000002";
			AssertHasWarnings(company.GC_BusinessRegNo2Info);
		}

		#endregion

	}
}
