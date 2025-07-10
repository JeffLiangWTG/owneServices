using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	public class HRJobApplicantValidationTest : BusinessObjectValidationTestCase
	{
		#region HA_PER

		public void TestCheckHA_PER_FirstSave()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			applicant.Validation.ValidateAll();

			AssertNoErrors(applicant.HA_PERInfo);
		}

		#endregion

		#region Test validation

		public void TestEmailAddressValidation()
		{
			Applicant.HA_EmailAddress = "///";
			AssertHasErrors("Not a valid email address, should have errors", Applicant.HA_EmailAddressInfo);

			Applicant.HA_EmailAddress = "blah@blah.com";
			AssertNoErrors("Valid email address, should NOT have errors", Applicant.HA_EmailAddressInfo);

			Applicant.HA_EmailAddress = "blah";
			AssertHasErrors("Not a valid email address, should have errors", Applicant.HA_EmailAddressInfo);
		}

		public void TestApplicantCountryValidation()
		{
			AssertEquals("Without any error at first ", 0, Applicant.HA_RN_NKCountryInfo.GetErrors().Count());
			Applicant.HA_RN_NKCountry = "XX";
			AssertEquals("now it must have an error ", 1, Applicant.HA_RN_NKCountryInfo.GetErrors().Count());
			var countries = Factory.Load<RefCountry>(new ZQuery());
			Applicant.HA_RN_NKCountry = countries[0].Code;
			AssertEquals("again it must be without error", 0, Applicant.HA_RN_NKCountryInfo.GetErrors().Count());
		}

		public void TestApplicantNationalityValidation()
		{
			AssertEquals("Without any error at first ", 0, Applicant.HA_RN_NKNationalityCodeISOInfo.GetErrors().Count());
			Applicant.HA_RN_NKNationalityCodeISO = "XX";
			AssertEquals("now it must have an error ", 1, Applicant.HA_RN_NKNationalityCodeISOInfo.GetErrors().Count());
			var countries = Factory.Load<RefCountry>(new ZQuery());
			Applicant.HA_RN_NKNationalityCodeISO = countries[0].Code;
			AssertEquals("again it must be without error", 0, Applicant.HA_RN_NKNationalityCodeISOInfo.GetErrors().Count());
		}

		public void TestApplicantCurrentWageCurrencyValidation()
		{
			AssertEquals("Without any error at first ", 0, Applicant.HA_RX_NKCurrentWageCurrencyInfo.GetErrors().Count());
			Applicant.HA_RX_NKCurrentWageCurrency = "XXX";
			AssertEquals("now it must have an error ", 1, Applicant.HA_RX_NKCurrentWageCurrencyInfo.GetErrors().Count());
			var currencies = Factory.Load<RefCurrency>(new ZQuery());
			Applicant.HA_RX_NKCurrentWageCurrency = currencies[0].RX_Code;
			AssertEquals("again it must be without error", 0, Applicant.HA_RX_NKCurrentWageCurrencyInfo.GetErrors().Count());
		}

		public void TestApplicantExpectedWageCurrencyValidation()
		{
			AssertEquals("Without any error at first ", 0, Applicant.HA_RX_NKWageExpectationCurrencyInfo.GetErrors().Count());
			Applicant.HA_RX_NKWageExpectationCurrency = "XXX";
			AssertEquals("now it must have an error ", 1, Applicant.HA_RX_NKWageExpectationCurrencyInfo.GetErrors().Count());
			var currencies = Factory.Load<RefCurrency>(new ZQuery());
			Applicant.HA_RX_NKWageExpectationCurrency = currencies[0].RX_Code;
			AssertEquals("again it must be without error", 0, Applicant.HA_RX_NKWageExpectationCurrencyInfo.GetErrors().Count());
		}

		public void TestValidateState()
		{
			AssertEquals("Without any warning at first ", 0, Applicant.HA_StateInfo.GetWarnings().Count());
			Applicant.HA_State = "123";
			AssertEquals("now it must have an warning ", 1, Applicant.HA_StateInfo.GetWarnings().Count());
			var country = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			Applicant.HA_RN_NKCountry = country[0].RN_Code;
			var states = Factory.Load<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "NSW"));
			Applicant.HA_State = states[0].RW_Code;
			AssertEquals("again it must be without warning", 0, Applicant.HA_StateInfo.GetWarnings().Count());
		}

		protected virtual bool ExpectedShouldValidatePasswordIfEmpty
		{
			get { return false; }
		}

		#endregion

		#region Test Mandatory Fields

		public void TestMandatoryFields()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "xxx";

			AssertEquals("At the first it must be without any error", 0, applicant.Notifications.GetErrors().Count());
			applicant.HA_EmailAddress = "";
			AssertEquals("Now it must have an error", 1, applicant.Notifications.GetErrors().Count());
			AssertEquals("and it must be in email", 1, applicant.HA_EmailAddressInfo.GetErrors().Count());
			applicant.HA_EmailAddress = "xxx@xxx.com";
			AssertEquals("The error must be removed", 0, applicant.HA_EmailAddressInfo.GetErrors().Count());
			AssertEquals("The error must be removed", 0, applicant.Notifications.GetErrors().Count());

			applicant.HA_FullName = "";
			AssertEquals("Now it must have an error", 1, applicant.Person.Notifications.GetErrors().Count());
			AssertEquals("and it must be in FullName", 1, applicant.HA_FullNameInfo.GetErrors().Count());
			applicant.HA_FullName = "Mohsen";
			AssertEquals("The error must be removed", 0, applicant.HA_FullNameInfo.GetErrors().Count());
			AssertEquals("The error must be removed", 0, applicant.Person.Notifications.GetErrors().Count());

			applicant.HA_RN_NKNationalityCodeISO = ZString.Empty;
			AssertNoErrors("Nationality code should not be mandatory", applicant.HA_RN_NKNationalityCodeISOInfo);
			applicant.HA_RN_NKNationalityCodeISO = "--";
			AssertHasErrors("invalid code, should have errors", applicant.HA_RN_NKNationalityCodeISOInfo);
			applicant.HA_RN_NKNationalityCodeISO = "AU";
			AssertNoErrors("Valid code, should not have errors", applicant.HA_RN_NKNationalityCodeISOInfo);
		}

		#endregion

		#region Phone Numbers Formatted

		public void TestCheckHA_FaxNum_Formatted()
		{
			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			jobApplicant.HA_FaxNum_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", jobApplicant.HA_FaxNum_FormattedInfo);

			jobApplicant.HA_FaxNum_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", jobApplicant.HA_FaxNum_FormattedInfo);

			jobApplicant.HA_FaxNum_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", jobApplicant.HA_FaxNum_FormattedInfo);
		}

		public void TestCheckHA_HomePhone_Formatted()
		{
			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			jobApplicant.HA_HomePhone_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", jobApplicant.HA_HomePhone_FormattedInfo);

			jobApplicant.HA_HomePhone_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", jobApplicant.HA_HomePhone_FormattedInfo);

			jobApplicant.HA_HomePhone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", jobApplicant.HA_HomePhone_FormattedInfo);

			jobApplicant.HA_HomePhone_Formatted = "你好";
			Assert(jobApplicant.HA_HomePhone_FormattedInfo.Notifications.ToMessageListString().Contains("Formatted only accepts Western European languages characters"));
		}

		public void TestCheckHA_MobilePhone_Formatted()
		{
			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			jobApplicant.HA_MobilePhone_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", jobApplicant.HA_MobilePhone_FormattedInfo);

			jobApplicant.HA_MobilePhone_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", jobApplicant.HA_MobilePhone_FormattedInfo);

			jobApplicant.HA_MobilePhone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", jobApplicant.HA_MobilePhone_FormattedInfo);

			jobApplicant.HA_MobilePhone_Formatted = "你好";
			Assert(jobApplicant.HA_MobilePhone_FormattedInfo.Notifications.ToMessageListString().Contains("Formatted only accepts Western European languages characters"));
		}

		public void TestCheckHA_WorkPhone_Formatted()
		{
			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			jobApplicant.HA_WorkPhone_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", jobApplicant.HA_WorkPhone_FormattedInfo);

			jobApplicant.HA_WorkPhone_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", jobApplicant.HA_WorkPhone_FormattedInfo);

			jobApplicant.HA_WorkPhone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", jobApplicant.HA_WorkPhone_FormattedInfo);
		}

		#endregion

		#region Implementation

		protected HRJobApplicant Applicant
		{
			get { return applicant ?? (applicant = GetNewJobApplicant()); }
		}

		protected virtual HRJobApplicant GetNewJobApplicant()
		{
			return Factory.New<HRJobApplicant>();
		}

		HRJobApplicant applicant;

		#endregion
	}
}
