using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPersonValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTitleIsNotValidated()
		{
			var person = Factory.New<GlbPerson>();
			person.Validation.ValidatePER_NameTitle();
			AssertNoErrors(person.PER_NameTitleInfo);
		}

		public void TestFullName()
		{
			var person = Factory.New<GlbPerson>();
			person.Validation.ValidatePER_FullName();
			AssertHasErrors(person.PER_FullNameInfo);

			person.PER_FullName = "hey";
			AssertNoErrors(person.PER_FullNameInfo);
		}

		public void TestShutUpAboutTenYears()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_BirthDate = ZDate.BrettsBirthday;
			AssertEquals(0, person.PER_BirthDateInfo.Notifications.Count());
		}

		public void TestValidatePER_BirthDate()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_BirthDate = ZDateTime.Now.Date.AddYears(-20);
			AssertNoErrors("Date of Birth should not have errors.", person.PER_BirthDateInfo);

			person.PER_BirthDate = ZDateTime.Now.Date;
			AssertNoErrors("Date of Birth should not have errors.", person.PER_BirthDateInfo);

			person.PER_BirthDate = ZDate.Today.AddDays(1);
			AssertHasError("Date of Birth should have errors", person.PER_BirthDateInfo, "Birthdate cannot be in the future.");

			person.PER_BirthDate = ZDate.Invalid;
			AssertHasError("Date of Birth should have errors", person.PER_BirthDateInfo, "Enter a valid Birth Date.");
		}

		public void TestZAIdentityNumberValidation()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_Passport = "A1111111";
			person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.Namibia;
			person.PER_DriversLicenseNumber = ZString.Empty;
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);
			person.PER_DriversLicenseNumber = "ZZZ";
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);

			person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.SouthAfrica;
			person.PER_DriversLicenseNumber = ZString.Empty;
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);

			person.PER_DriversLicenseNumber = "ZZZ";
			AssertHasErrors(person.PER_DriversLicenseNumberInfo);
		}

		public void TestCheckPER_Gender()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_Gender = ZString.Empty;
			AssertNoErrors("Blank gender should be permitted", person.PER_GenderInfo);

			person.PER_Gender = "Y";
			AssertHasErrors("Invalid gender should have error", person.PER_GenderInfo);

			person.PER_Gender = Core.Constants.Genders.Woman;
			AssertNoErrors("Valid gender should not have error", person.PER_GenderInfo);

			Env.Security.PersonIntelligenceViewGender.IsAllowed = false;
			person.Validation.ValidatePER_Gender();
			AssertNoErrors("Gender viewed denied message should not have error", person.PER_GenderInfo);
		}

		public void TestCheckPER_RN_NKNationalityCodeISO()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_RN_NKNationalityCodeISO = ZString.Empty;
			AssertNoErrors("Blank nationality should be permitted", person.PER_RN_NKNationalityCodeISOInfo);

			person.PER_RN_NKNationalityCodeISO = "ZZ";
			AssertHasErrors("Invalid nationality should have error", person.PER_RN_NKNationalityCodeISOInfo);

			person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.France;
			AssertNoErrors("Valid nationality should not have error", person.PER_RN_NKNationalityCodeISOInfo);

			Env.Security.PersonIntelligenceViewNationality.IsAllowed = false;
			person.Validation.ValidatePER_RN_NKNationalityCodeISO();
			AssertNoErrors("Nationality viewed denied message should not have error", person.PER_GenderInfo);
		}

		public void TestCheckPER_EmailAddress_InvalidEmail()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_EmailAddress = "blahblah@gmail.com";
			AssertNoErrors(person.PER_EmailAddressInfo);

			person.PER_EmailAddress = "sdfdsfds";
			AssertHasErrorContaining(person.PER_EmailAddressInfo, "Email Address is not valid");
		}

		public void TestCheckPER_MobilePhone_InvalidNumber()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_MobilePhone = "+61 2 1234 5678";
			AssertNoErrors(person.PER_MobilePhoneInfo);

			person.PER_MobilePhone = "sdfsdfdsf";
			AssertHasErrorContaining(person.PER_MobilePhoneInfo, "The phone number as entered has a high probability of being incorrect.");
		}

		public void TestCheckPER_HomePhone_InvalidNumber()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_HomePhone = "+61 2 1234 5678";
			AssertNoErrors(person.PER_HomePhoneInfo);

			person.PER_HomePhone = "sdfsdfdsf";
			AssertHasErrorContaining(person.PER_HomePhoneInfo, "The phone number as entered has a high probability of being incorrect.");
		}

		public void TestCheckPER_FaxNumber_InvalidNumber()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_FaxNumber = "+61 2 1234 5678";
			AssertNoErrors(person.PER_FaxNumberInfo);

			person.PER_FaxNumber = "sdfsdfdsf";
			AssertHasErrorContaining(person.PER_FaxNumberInfo, "The phone number as entered has a high probability of being incorrect.");
		}

		public void TestAtLeastOneOfEmailAndMobilePhoneNotEmptyValidation()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_EmailAddress = "";
			person.PER_MobilePhone = "";

			AssertHasErrorContaining(person.PER_EmailAddressInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
			AssertHasErrorContaining(person.PER_MobilePhoneInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
		}

		public void TestWhenPersonHasContactAssociationEmailAndMobilePhoneCanBeEmpty()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_EmailAddress = ZString.Empty;
			person.PER_MobilePhone = ZString.Empty;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;

			person.ContactCollection.Load();

			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_MobilePhone();
			AssertEquals(1, person.ContactCollection.Count);
			AssertNoErrors("Contact association", person.PER_EmailAddressInfo);
			AssertNoErrors("Contact association", person.PER_MobilePhoneInfo);
		}

		public void TestWhenPersonHasApplicantAssociationEmailAndMobilePhoneCanBeEmpty()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_EmailAddress = ZString.Empty;
			person.PER_MobilePhone = ZString.Empty;

			var applicant = Factory.New<IHRJobApplicant>();
			applicant.HA_PER = person.PK;

			person.ApplicantCollection.Load();

			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_MobilePhone();
			AssertEquals(1, person.ApplicantCollection.Count);
			AssertNoErrors("Applicant association", person.PER_EmailAddressInfo);
			AssertNoErrors("Applicant association", person.PER_MobilePhoneInfo);
		}

		public void TestWhenPersonHasStaffAssociationEmailAndMobilePhoneCanBeEmpty()
		{
			var person = Factory.New<GlbPerson>();

			person.PER_EmailAddress = ZString.Empty;
			person.PER_MobilePhone = ZString.Empty;

			var staff = Factory.New<GlbStaff>();
			staff.GS_PER = person.PK;

			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_MobilePhone();
			AssertEquals(1, person.StaffCollection.Count);
			AssertNoErrors("Staff association", person.PER_EmailAddressInfo);
			AssertNoErrors("Staff association", person.PER_MobilePhoneInfo);
		}

		public void TestAssociatedPersonValidation()
		{
			var person = Factory.New<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "George";
			contact.OC_PER = person.PK;

			person.Validation.ValidatePER_DriversLicenseNumber();
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);

			person.Validation.ValidatePER_MobilePhone();
			AssertNoErrors(person.PER_MobilePhoneInfo);

			person.Validation.ValidatePER_EmailAddress();
			AssertNoErrors(person.PER_EmailAddressInfo);

			person.Validation.ValidatePER_Passport();
			AssertNoErrors(person.PER_PassportInfo);
		}

		public void TestStandalonePersonValidation()
		{
			var person = Factory.New<GlbPerson>();

			person.Validation.ValidatePER_FullName();
			AssertHasErrorContaining(person.PER_FullNameInfo, "Please enter a Full Name.");
			person.PER_FullName = "George Lucas";
			AssertNoErrors(person.PER_FullNameInfo);

			person.Validation.ValidatePER_DriversLicenseNumber();
			person.Validation.ValidatePER_MobilePhone();
			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_Passport();
			person.Validation.ValidatePER_MobilePhone_Formatted();
			AssertHasErrorContaining(person.PER_DriversLicenseNumberInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
			AssertHasErrorContaining(person.PER_MobilePhoneInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
			AssertHasErrorContaining(person.PER_EmailAddressInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
			AssertHasErrorContaining(person.PER_PassportInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
			AssertHasErrorContaining(person.PER_MobilePhone_FormattedInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");

			person.PER_DriversLicenseNumber = "1A";

			person.Validation.ValidatePER_DriversLicenseNumber();
			person.Validation.ValidatePER_MobilePhone();
			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_Passport();
			person.Validation.ValidatePER_MobilePhone_Formatted();
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);
			AssertNoErrors(person.PER_MobilePhoneInfo);
			AssertNoErrors(person.PER_EmailAddressInfo);
			AssertNoErrors(person.PER_PassportInfo);
			AssertNoErrors(person.PER_MobilePhone_FormattedInfo);

			person.PER_DriversLicenseNumber = ZString.Empty;
			person.PER_MobilePhone = "+61 2 1234 5678";

			person.Validation.ValidatePER_DriversLicenseNumber();
			person.Validation.ValidatePER_MobilePhone();
			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_Passport();
			person.Validation.ValidatePER_MobilePhone_Formatted();
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);
			AssertNoErrors(person.PER_MobilePhoneInfo);
			AssertNoErrors(person.PER_EmailAddressInfo);
			AssertNoErrors(person.PER_PassportInfo);
			AssertNoErrors(person.PER_MobilePhone_FormattedInfo);

			person.PER_MobilePhone = ZString.Empty;
			person.PER_EmailAddress = "george.lucas@heights.com";

			person.Validation.ValidatePER_DriversLicenseNumber();
			person.Validation.ValidatePER_MobilePhone();
			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_Passport();
			person.Validation.ValidatePER_MobilePhone_Formatted();
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);
			AssertNoErrors(person.PER_MobilePhoneInfo);
			AssertNoErrors(person.PER_EmailAddressInfo);
			AssertNoErrors(person.PER_PassportInfo);
			AssertNoErrors(person.PER_MobilePhone_FormattedInfo);

			person.PER_EmailAddress = ZString.Empty;
			person.PER_Passport = "A1111111";

			person.Validation.ValidatePER_DriversLicenseNumber();
			person.Validation.ValidatePER_MobilePhone();
			person.Validation.ValidatePER_EmailAddress();
			person.Validation.ValidatePER_Passport();
			person.Validation.ValidatePER_MobilePhone_Formatted();
			AssertNoErrors(person.PER_DriversLicenseNumberInfo);
			AssertNoErrors(person.PER_MobilePhoneInfo);
			AssertNoErrors(person.PER_EmailAddressInfo);
			AssertNoErrors(person.PER_PassportInfo);
			AssertNoErrors(person.PER_MobilePhone_FormattedInfo);
		}

		[TestDate(2018, 7, 10, 17, 31, 55)]
		public void TestCheckPER_PassportExpiryDateIsValidZDateRange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var person = Factory.New<GlbPerson>();
				person.PER_PassportExpiryDate = ZDate.Today.AddYears(2);
				AssertNoWarnings(person.PER_PassportExpiryDateInfo);

				person.PER_PassportExpiryDate = ZDate.Today.AddYears(6);
				AssertNoErrors(person.PER_PassportExpiryDateInfo);

				person.PER_PassportExpiryDate = ZDate.Today.AddYears(-2);
				AssertHasWarningContaining(person.PER_PassportExpiryDateInfo, "is more than 1 year old.");

				person.PER_PassportExpiryDate = ZDate.Today.AddYears(-11);
				AssertHasErrorContaining(person.PER_PassportExpiryDateInfo, "is more than 10 years old and thus is not valid.");
			}
		}

		public void TestCheckWebAccessEnabled()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_EmailAddress = "1@a.com";
			person1.PER_WebAccessEnabled = true;
			person1.PER_IsActive = true;

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_EmailAddress = "2@a.com";
			person2.PER_WebAccessEnabled = false;
			person2.PER_IsActive = true;

			var person3 = Factory.NewWithValidTestData<GlbPerson>();
			person3.PER_EmailAddress = "3@a.com";
			person3.PER_WebAccessEnabled = true;
			person3.PER_IsActive = false;

			Factory.Save();

			var person4 = Factory.NewWithValidTestData<GlbPerson>();
			person4.PER_EmailAddress = "1@a.com";
			person4.PER_WebAccessEnabled = true;
			person4.PER_IsActive = true;

			var errorExpected = "This email is already in use by another active Person with web access.";
			person4.RunPreSaveValidation();
			AssertHasError(person4.PER_EmailAddressInfo, errorExpected);
			AssertHasError(person4.PER_IsActiveInfo, errorExpected);
			AssertHasError(person4.PER_WebAccessEnabledInfo, errorExpected);

			person4.PER_EmailAddress = "1@a.com";
			person4.PER_WebAccessEnabled = false;
			person4.PER_IsActive = true;
			person4.RunPreSaveValidation();
			AssertNoErrors(person4.PER_EmailAddressInfo);
			AssertNoErrors(person4.PER_IsActiveInfo);
			AssertNoErrors(person4.PER_WebAccessEnabledInfo);

			person4.PER_EmailAddress = "1@a.com";
			person4.PER_WebAccessEnabled = true;
			person4.PER_IsActive = false;
			person4.RunPreSaveValidation();
			AssertNoErrors(person4.PER_EmailAddressInfo);
			AssertNoErrors(person4.PER_IsActiveInfo);
			AssertNoErrors(person4.PER_WebAccessEnabledInfo);

			person4.PER_EmailAddress = "2@a.com";
			person4.PER_WebAccessEnabled = true;
			person4.PER_IsActive = true;
			person4.RunPreSaveValidation();
			AssertNoErrors(person4.PER_EmailAddressInfo);
			AssertNoErrors(person4.PER_IsActiveInfo);
			AssertNoErrors(person4.PER_WebAccessEnabledInfo);

			person4.PER_EmailAddress = "3@a.com";
			person4.PER_WebAccessEnabled = true;
			person4.PER_IsActive = true;
			person4.RunPreSaveValidation();
			AssertNoErrors(person4.PER_EmailAddressInfo);
			AssertNoErrors(person4.PER_IsActiveInfo);
			AssertNoErrors(person4.PER_WebAccessEnabledInfo);

			person4.PER_EmailAddress = "";
			person4.PER_WebAccessEnabled = true;
			person4.PER_IsActive = true;
			person4.RunPreSaveValidation();
			AssertHasError(person4.PER_EmailAddressInfo, "Email is required when web access is enabled.");
			AssertNoErrors(person4.PER_IsActiveInfo);
			AssertNoErrors(person4.PER_WebAccessEnabledInfo);
		}

		public void TestValidatePrimaryRelationship()
		{
			var person = Factory.New<GlbPerson>();
			person.Validation.ValidatePrimaryRelationship();
			AssertNoRowError(person, "Please set the person's primary workplace.");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;

			person.ContactCollection.Load();
			person.Validation.ValidatePrimaryRelationship();
			AssertHasRowError(person, "Please set the person's primary workplace.");

			person.SetPrimaryRelationship(contact);
			person.Validation.ValidatePrimaryRelationship();
			AssertNoRowError(person, "Please set the person's primary workplace.");

			person.RemovePrimaryRelationship();
			person.ContactCollection.RemoveAndDeleteAll();
			person.Validation.ValidatePrimaryRelationship();
			AssertNoRowError(person, "Please set the person's primary workplace.");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			person.Validation.ValidatePrimaryRelationship();
			AssertHasRowError(person, "Please set the person's primary workplace.");

			person.IsMovingFromAnotherPerson = true;
			person.Validation.ValidatePrimaryRelationship();
			AssertNoRowError(person, "Please set the person's primary workplace.");

			person.IsMovingFromAnotherPerson = false;
			person.Validation.ValidatePrimaryRelationship();
			AssertHasRowError(person, "Please set the person's primary workplace.");

			person.SetPrimaryRelationship(staff);
			person.Validation.ValidatePrimaryRelationship();
			AssertNoRowError(person, "Please set the person's primary workplace.");
		}
	}
}
