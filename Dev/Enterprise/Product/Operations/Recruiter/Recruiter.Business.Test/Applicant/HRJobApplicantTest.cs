using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicant))]
	public class HRJobApplicantTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountryCodeIsObsoleteAndMacroIgnore()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(HRJobApplicant).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestHumanReadableNameCore()
		{
			var applicant = Factory.New<HRJobApplicant>();

			AssertEquals("Applicant", applicant.HumanReadableName);

			applicant.HA_FullName = "full name";
			AssertEquals("Applicant - full name", applicant.HumanReadableName);
		}

		public void TestCreateFromStaff()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var viewDeniedMessage = "** View Denied **";

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "full name";
			person.PER_NameSuffix = "sfx";
			person.PER_NameTitle = "title";
			person.PER_Gender = "M";
			person.PER_HomeAddress1 = "address 1";
			person.PER_HomeAddress2 = "address 2";
			person.PER_City = "city";
			person.PER_Postcode = "12345";
			person.PER_State = "state";
			person.PER_RN_NKCountry = "AU";
			person.PER_MobilePhone = "123456789";
			person.PER_HomePhone = "987654321";
			person.PER_FaxNumber = "111";
			person.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			person.PER_FriendlyName = "friendly";
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_PreferredLanguage = "RSN";
			person.PER_DriversLicenseNumber = "drivers";
			person.PER_EmailAddress = "email@contact.com";
			person.PER_EmailAddress2 = "email@contact.com";
			person.PER_MobilePhone2 = "123456789";
			person.PER_Passport = "passport";

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
				GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var newFactory = new BusinessObjectFactory();
					var personLoaded = newFactory.Load<GlbPerson>(person.PK);

					Assert(viewDeniedMessage == personLoaded.PER_LegalName);
					Assert(ZDate.Empty == personLoaded.PER_BirthDate);
					Assert(viewDeniedMessage == personLoaded.PER_EmailAddress);
					Assert(viewDeniedMessage == personLoaded.PER_EmailAddress2);
					Assert(viewDeniedMessage == personLoaded.PER_Gender);
					Assert(viewDeniedMessage == personLoaded.PER_MobilePhone);
					Assert(viewDeniedMessage == personLoaded.PER_MobilePhone2);
					Assert(viewDeniedMessage == personLoaded.PER_HomePhone);
					Assert(viewDeniedMessage == personLoaded.PER_RN_NKNationalityCodeISO);
					Assert(viewDeniedMessage == personLoaded.PER_PersonalInfo);
					Assert(viewDeniedMessage == personLoaded.PER_HomeAddress1);
					Assert(viewDeniedMessage == personLoaded.PER_HomeAddress2);
					Assert(viewDeniedMessage == personLoaded.PER_City);
					Assert(viewDeniedMessage == personLoaded.PER_State);
					Assert(viewDeniedMessage == personLoaded.PER_Postcode);
					Assert(viewDeniedMessage == personLoaded.PER_Passport);

					var applicant = newFactory.New<HRJobApplicant>();
					applicant.SetFromPerson(personLoaded);
					applicant.HA_PER = personLoaded.PK;

					AssertEquals("email@contact.com", applicant.HA_EmailAddress);
				}
			}
		}

		public void TestPersonIsCreated()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "almost full name";
			Factory.Save();

			AssertNotEquals(ZGuid.Empty, applicant.HA_PER);

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(applicant.HA_PER);
			AssertNotNull(person);
			AssertEquals("almost full name", person.PER_FullName);

			applicant = new BusinessObjectFactory() { RefreshEnabled = false }.Load<HRJobApplicant>(applicant.PK);
			AssertEquals(person.PK, applicant.HA_PER);
		}

		public void TestPersonIsUpdated()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "almost full name";
			Factory.Save();

			var person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(applicant.HA_PER);
			AssertNotNull(person);
			applicant = person.Factory.Load<HRJobApplicant>(applicant.PK);

			applicant.HA_FullName = "new full name";
			applicant.Factory.Save();

			person = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbPerson>(person.PK);
			AssertNotNull(person);
			AssertEquals("new full name", person.PER_FullName);
		}

		public void TestUpdateFromPerson()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();

			var person = Factory.Load<GlbPerson>(applicant.HA_PER);
			AssertNotNull(person);

			person.PER_FullName = "new full name";
			person.PER_Gender = "F";
			person.PER_NameSuffix = "mrs";
			person.PER_NameTitle = "new title";
			person.PER_HomeAddress1 = "new address 1";
			person.PER_HomeAddress2 = "new address 2";
			person.PER_City = "new city";
			person.PER_Postcode = "54321";
			person.PER_State = "new state";
			person.PER_RN_NKCountry = "NZ";
			person.PER_HomePhone = "987654321";
			person.PER_FaxNumber = "222";
			person.PER_BirthDate = ZDateTime.Today.Date;
			person.PER_RN_NKNationalityCodeISO = "ZZ";
			person.PER_DriversLicenseNumber = "new drivers";
			person.PER_EmailAddress = "newemail@contact.com";
			person.PER_EmailAddress2 = "newemail@contact.com";
			person.PER_Passport = "newpassport";
			applicant.UpdateFromPerson(person);

			AssertEquals(person.PER_FullName, applicant.HA_FullName);
			AssertEquals(person.PER_NameSuffix, applicant.HA_NameSuffix);
			AssertEquals(person.PER_NameTitle, applicant.HA_Title);
			AssertEquals(person.PER_Gender, applicant.HA_Gender);
			AssertEquals(person.PER_HomeAddress1, applicant.HA_UserAddress1);
			AssertEquals(person.PER_HomeAddress2, applicant.HA_UserAddress2);
			AssertEquals(person.PER_City, applicant.HA_City);
			AssertEquals(person.PER_Postcode, applicant.HA_Postcode);
			AssertEquals(person.PER_State, applicant.HA_State);
			AssertEquals(person.PER_HomePhone, applicant.HA_HomePhone);
			AssertEquals(person.PER_FaxNumber, applicant.HA_FaxNum);
			AssertEquals(person.PER_BirthDate, applicant.HA_Birthdate);
			AssertEquals(person.PER_RN_NKNationalityCodeISO, applicant.HA_RN_NKNationalityCodeISO);
			AssertEquals(person.PER_RN_NKCountry, applicant.HA_RN_NKCountry);
			AssertEquals(person.PER_EmailAddress, applicant.HA_EmailAddress);
			AssertEquals(person.PER_Passport, applicant.HA_Passport);
			AssertEquals(person.PER_DriversLicenseNumber, applicant.HA_DriversLicenseNumber);
		}

		public GlbStaff CreateStaff(bool allowView, bool allowEdit)
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantEdit, allowEdit, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, allowView, staffDenied.PK));

			return staffDenied;
		}

		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		public void TestUpdateFromPersonWithoutSecurity()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var staffDenied = CreateStaff(true, true);
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					Env.Security.PersonIntelligenceView.IsAllowed = false;
					Env.Security.PersonIntelligenceEdit.IsAllowed = false;

					var newFactory = new BusinessObjectFactory();
					applicant = newFactory.Load<HRJobApplicant>(applicant.PK);
					var person = newFactory.Load<GlbPerson>(applicant.HA_PER);
					AssertNotNull(person);
					newFactory.Save();

					person.PER_FullName = "new full name";
					person.PER_Gender = "F";
					person.PER_NameSuffix = "mrs";
					person.PER_NameTitle = "new title";
					person.PER_HomeAddress1 = "new address 1";
					person.PER_HomeAddress2 = "new address 2";
					person.PER_City = "new city";
					person.PER_Postcode = "54321";
					person.PER_State = "new state";
					person.PER_RN_NKCountry = "NZ";
					person.PER_HomePhone = "987654321";
					person.PER_FaxNumber = "222";
					person.PER_BirthDate = ZDateTime.Today.Date;
					person.PER_RN_NKNationalityCodeISO = "ZZ";
					person.PER_DriversLicenseNumber = "new drivers";
					person.PER_EmailAddress = "newemail@contact.com";
					person.PER_EmailAddress2 = "newemail@contact.com";
					person.PER_Passport = "newpassport";
					applicant.UpdateFromPerson(person);

					AssertEquals(person.PER_FullName, "new full name");
					AssertEquals(person.PER_NameSuffix, applicant.HA_NameSuffix);
					AssertEquals(person.PER_NameTitle, applicant.HA_Title);
					AssertEquals(person.PER_Gender, "** View Denied **");
					AssertEquals(person.PER_HomeAddress1, "** View Denied **");
					AssertEquals(person.PER_HomeAddress2, "** View Denied **");
					AssertEquals(person.PER_City, "** View Denied **");
					AssertEquals(person.PER_Postcode, "** View Denied **");
					AssertEquals(person.PER_State, "** View Denied **");
					AssertEquals(person.PER_HomePhone, "** View Denied **");
					AssertEquals(person.PER_FaxNumber, "** View Denied **");
					AssertEquals(person.PER_BirthDate, ZDate.Empty);
					AssertEquals(person.PER_RN_NKNationalityCodeISO, "** View Denied **");
					AssertEquals(person.PER_RN_NKCountry, "** View Denied **");
					AssertEquals(person.PER_EmailAddress, "** View Denied **");
					AssertEquals(person.PER_Passport, "** View Denied **");
					AssertEquals(person.PER_DriversLicenseNumber, "** View Denied **");

					AssertEquals(person.PER_FullNameInternal, "new full name");
					AssertEquals(person.PER_NameSuffix, "mrs");
					AssertEquals(person.PER_NameTitle, "new title");
					AssertEquals(person.PER_GenderInternal, "F");
					AssertEquals(person.PER_HomeAddress1Internal, "new address 1");
					AssertEquals(person.PER_HomeAddress2Internal, "new address 2");
					AssertEquals(person.PER_CityInternal, "new city");
					AssertEquals(person.PER_PostcodeInternal, "54321");
					AssertEquals(person.PER_StateInternal, "new state");
					AssertEquals(person.PER_HomePhoneInternal, "987654321");
					AssertEquals(person.PER_FaxNumberInternal, "222");
					AssertEquals(person.PER_BirthDateInternal, ZDateTime.Today.Date);
					AssertEquals(person.PER_RN_NKNationalityCodeISOInternal, "ZZ");
					AssertEquals(person.PER_RN_NKCountryInternal, "NZ");
					AssertEquals(person.PER_EmailAddressInternal, "newemail@contact.com");
					AssertEquals(person.PER_PassportInternal, "newpassport");
					AssertEquals(person.PER_DriversLicenseNumberInternal, "new drivers");

					AssertEquals(person.PER_EmailAddressInternal, applicant.HA_EmailAddress);
				}
			}
		}

		public void TestPersonUpdateFromApplicantOnSaving()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			var person = Factory.Load<GlbPerson>(applicant.HA_PER);
			Factory.Save();

			applicant.HA_UserAddress1 = "myAddress";
			Factory.Save();
			AssertEquals("Applicant address should propagate to person", applicant.HA_UserAddress1, person.PER_HomeAddress1);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			applicant.Logs.AddNew(AutoEvents.EditedARecord, "Related Contact");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			applicant.HA_FullName = "him";
			applicant.HA_MobilePhone = "123456789";
			Factory.Save();
			AssertEquals("Applicant name should propagate to person", "him", person.PER_FullName);
			AssertEquals("Applicant mobile should propagate to person", "123456789", person.PER_MobilePhone);
		}

		public void TestContactSetterOnSaving()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();

			GlbPerson.CreateFromApplicant(Factory, applicant);
			applicant.HA_Gender = "F";
			Factory.Save();

			AssertEquals("F", applicant.Person.PER_GenderInternal);
		}

		public void TestIsAutologged()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			Assert("Applicant should contain logs", applicant.Logs.GetAllLogs().Count > 0);
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			AssertEquals("Should be 0 business objects with related notes", 0, applicant.BusinessObjectsWithRelatedNotes.Length);
		}

		public void TestDelete()
		{
			var applicant1 = Factory.New<HRJobApplicant>();
			applicant1.HA_HomePhone_IsManuallyVerified = true;
			applicant1.HA_WorkPhone_IsManuallyVerified = true;

			var applicant2 = Factory.New<HRJobApplicant>();
			applicant2.HA_MobilePhone_IsManuallyVerified = true;

			var acks1 = new GenCustomAddOnRuleAckCollection(applicant1);
			var acks2 = new GenCustomAddOnRuleAckCollection(applicant2);

			AssertEquals("Precondition", 1, acks1.Count);
			AssertEquals("Precondition", 0, acks2.Count);

			var application = applicant1.Applications.AddNew();

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = applicant1.PK;

			applicant1.Delete();

			Assert("Campaign Item should be deleted", campaignItem.IsDeleted);
			Assert("Application should be deleted", application.IsDeleted);
			AssertEquals(0, acks1.Count);
			AssertEquals(0, acks2.Count);
		}

		public void TestImplementsIDocManagerSupport()
		{
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(HRJobApplicant)));
			AssertEquals("HRJobApplicant.DocManagerInfo is HRJobApplicantDocManagerInfo", typeof(HRJobApplicantDocManagerInfo), Applicant.DocManagerInfo.GetType());
			AssertEquals("HRJobApplicant.DocManagerCode = APP", "APP", Applicant.DocManagerInfo.DocManagerCode);
		}

		public void TestApplications()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			application.HP_HA = Applicant.PK;
			Assert("Applications collection should contain application", Applicant.Applications.Contains(application));
		}

		public void TestWorkStatusForCurrentCountry()
		{
			Applicant.HA_RN_NKNationalityCodeISO = "MM";
			AssertEquals("The applicant is not from the current country therefore their work permit status is not ReadOnly.", false, Applicant.HA_WorkPermitStatusInfo.ReadOnly);
			Applicant.HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Student;

			Applicant.HA_RN_NKNationalityCodeISO = GlbBranch.CurrentBranch.Country.RN_Code;
			AssertEquals("The applicant is from the current country therefore their work permit status is RES.", Core.Constants.WorkPermitStatuses.Residence, Applicant.HA_WorkPermitStatus);
			AssertEquals("The applicant is from the current country therefore their work permit status is ReadOnly.", true, Applicant.HA_WorkPermitStatusInfo.ReadOnly);

			Applicant.HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Student;
			Applicant.HA_RN_NKNationalityCodeISO = "MM";
			AssertEquals("The applicant's work permit status should not be changed for non-AU residents.", Core.Constants.WorkPermitStatuses.Student, Applicant.HA_WorkPermitStatus);
			AssertEquals("The applicant is not from the current country therefore their work permit status is not ReadOnly.", false, Applicant.HA_WorkPermitStatusInfo.ReadOnly);
		}

		public void TestFillWithValidTestDataCore()
		{
			var applicantWithApplications = Factory.NewWithValidTestData<HRJobApplicant>(TestBusinessObjectKind.PopulateDependentCollections);
			Assert("Applicant should have at least one application", applicantWithApplications.Applications.Count > 0);
			Assert("Application should be for a valid campaign", applicantWithApplications.Applications[0].HP_HV.IsValid);

			var applicantWithNoApplications = Factory.NewWithValidTestData<HRJobApplicant>(TestBusinessObjectKind.MinimumRequiredToSave);
			AssertEquals("Applicant should not have any applications", 0, applicantWithNoApplications.Applications.Count);
		}

		public void TestIContactableMembers()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "meh@meh.com";
			applicant.HA_MobilePhone = "029283";
			applicant.HA_FullName = "Eric Schmidt";
			AssertEquals("Eric Schmidt", applicant.Name);
			AssertEquals("029283", applicant.Mobile);
			AssertEquals("meh@meh.com", applicant.Email);
			AssertEquals(true, applicant.IsActive);
			AssertNotNull(applicant.GetNestedContacts(null));
		}

		public void TestIGlbCompanyCampaignItemRecipientMembers()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FaxNum = "FAX101";
			applicant.HA_Title = "Ms.";
			applicant.HA_MobilePhone = "0001mobile";
			applicant.HA_FullName = "Annie";

			IGlbCompanyCampaignItemRecipient recipient = applicant;
			AssertEquals("0001mobile", recipient.Phone);
			AssertNull(recipient.Organisation);
			AssertEquals("", recipient.Salutation);
			AssertEquals("Ms.", recipient.Title);
			AssertEquals("FAX101", recipient.Fax);
			AssertEquals("Job Applicant Annie", recipient.RelatedDocName);
		}

		public void TestPhoneNumbers()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_MobilePhone = "Mobile101";
			applicant.HA_HomePhone = "Home101";
			applicant.HA_FaxNum = "Fax101";
			applicant.HA_WorkPhone = "Work101";
			applicant.HA_WorkExtension = "Ext101";

			AssertEquals("Mobile101", applicant.MobilePhoneNumber.FormattedForBinding);
			AssertEquals("Home101", applicant.HomePhoneNumber.FormattedForBinding);
			AssertEquals("Fax101", applicant.FaxNumber.FormattedForBinding);
			AssertEquals("Work101", applicant.WorkPhoneNumber.FormattedForBinding);
			AssertEquals("Ext101", applicant.WorkExtensionNumber.FormattedForBinding);

			AssertEquals("", applicant.MobilePhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", applicant.HomePhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", applicant.FaxNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", applicant.WorkPhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", applicant.WorkExtensionNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestPhoneNumbers_Formatted()
		{
			var invalidPhoneNumber = "XXXX";
			var validLocalAUPhone = "02 0101 0101";
			var validInternantionalAUPhone = "+61 2 0101 0101";
			var expected = "+61201010101";
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_MobilePhone_Formatted = invalidPhoneNumber;
			applicant.HA_HomePhone_Formatted = invalidPhoneNumber;
			applicant.HA_FaxNum_Formatted = invalidPhoneNumber;
			applicant.HA_WorkPhone_Formatted = invalidPhoneNumber;

			AssertEquals(invalidPhoneNumber, applicant.HA_MobilePhone);
			AssertEquals(invalidPhoneNumber, applicant.HA_HomePhone);
			AssertEquals(invalidPhoneNumber, applicant.HA_FaxNum);
			AssertEquals(invalidPhoneNumber, applicant.HA_WorkPhone);

			applicant.HA_MobilePhone_Formatted = validInternantionalAUPhone;
			applicant.HA_HomePhone_Formatted = validInternantionalAUPhone;
			applicant.HA_FaxNum_Formatted = validInternantionalAUPhone;
			applicant.HA_WorkPhone_Formatted = validInternantionalAUPhone;

			AssertEquals(expected, applicant.HA_MobilePhone);
			AssertEquals(expected, applicant.HA_HomePhone);
			AssertEquals(expected, applicant.HA_FaxNum);
			AssertEquals(expected, applicant.HA_WorkPhone);

			applicant.HA_MobilePhone_Formatted = validLocalAUPhone;
			applicant.HA_HomePhone_Formatted = validLocalAUPhone;
			applicant.HA_FaxNum_Formatted = validLocalAUPhone;
			applicant.HA_WorkPhone_Formatted = validLocalAUPhone;

			AssertEquals(validLocalAUPhone, applicant.HA_MobilePhone);
			AssertEquals(validLocalAUPhone, applicant.HA_HomePhone);
			AssertEquals(validLocalAUPhone, applicant.HA_FaxNum);
			AssertEquals(validLocalAUPhone, applicant.HA_WorkPhone);

			applicant.HA_RN_NKCountry = "AU";
			applicant.HA_MobilePhone_Formatted = validLocalAUPhone;
			applicant.HA_HomePhone_Formatted = validLocalAUPhone;
			applicant.HA_FaxNum_Formatted = validLocalAUPhone;
			applicant.HA_WorkPhone_Formatted = validLocalAUPhone;

			AssertEquals(expected, applicant.HA_MobilePhone);
			AssertEquals(expected, applicant.HA_HomePhone);
			AssertEquals(expected, applicant.HA_FaxNum);
			AssertEquals(expected, applicant.HA_WorkPhone);
		}

		public void TestPhoneNumbers_SameNormalizedValue()
		{
			var applicant = Factory.New<HRJobApplicant>();

			var unformattedMobilePhone = "04 12 345 678";
			var unformattedHomePhone = "+61 04 22 345 678";
			var unformattedFaxNum = "+61 4 3234 5678";
			var unformattedWorkPhone = "+61  04 13 345 678";

			var normalizedFormattedMobilePhone = "+61412345678";
			var formattedMobilePhone = "+61 412 345 678";
			var formattedHomePhone = "+61 422 345 678";
			var formattedFaxNum = "+61 432 345 678";
			var formattedWorkPhone = "+61 413 345 678";

			var newFormattedMobilePhone = "+61 412 345 679";
			var newFormattedHomePhone = "+61 422 345 679";
			var newFormattedFaxNum = "+61 432 345 679";
			var newFormattedWorkPhone = "+61 412 345 679";

			var newNormalizedMobilePhone = "+61412345679";
			var newNormalizedHomePhone = "+61422345679";
			var newNormalizedFaxNum = "+61432345679";
			var newNormalizedWorkPhone = "+61412345679";

			applicant.HA_RN_NKCountry = "AU";

			applicant.HA_MobilePhone = unformattedMobilePhone;
			applicant.HA_HomePhone = unformattedHomePhone;
			applicant.HA_FaxNum = unformattedFaxNum;
			applicant.HA_WorkPhone = unformattedWorkPhone;

			applicant.HA_MobilePhone_Formatted = formattedMobilePhone;
			applicant.HA_HomePhone_Formatted = formattedHomePhone;
			applicant.HA_FaxNum_Formatted = formattedFaxNum;
			applicant.HA_WorkPhone_Formatted = formattedWorkPhone;

			AssertEquals(normalizedFormattedMobilePhone, applicant.HA_MobilePhone);
			AssertEquals(unformattedHomePhone, applicant.HA_HomePhone);
			AssertEquals(unformattedFaxNum, applicant.HA_FaxNum);
			AssertEquals(unformattedWorkPhone, applicant.HA_WorkPhone);

			applicant.HA_MobilePhone_Formatted = newFormattedMobilePhone;
			applicant.HA_HomePhone_Formatted = newFormattedHomePhone;
			applicant.HA_FaxNum_Formatted = newFormattedFaxNum;
			applicant.HA_WorkPhone_Formatted = newFormattedWorkPhone;

			AssertEquals(newNormalizedMobilePhone, applicant.HA_MobilePhone);
			AssertEquals(newNormalizedHomePhone, applicant.HA_HomePhone);
			AssertEquals(newNormalizedFaxNum, applicant.HA_FaxNum);
			AssertEquals(newNormalizedWorkPhone, applicant.HA_WorkPhone);
		}

		public void TestCodeDescription()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Some Applicant";
			applicant.HA_EmailAddress = "applicant@cargowise.com";
			AssertEquals("applicant@cargowise.com", ((ICodeDescription)applicant).Description);
			AssertEquals("Some Applicant", ((ICodeDescription)applicant).Code);
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.Applicant, Factory.New<HRJobApplicant>().ValidationSection);
		}

		[TestDate(2015, 11, 9, 10, 20, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestLastSubmittedJobApplicationSubmissionTime()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			application.HP_SubmissionTimeUtc = ZDateTime.UtcNow;
			application.HP_HV = campaign.PK;

			var application1 = applicant.Applications.AddNew();
			application1.HP_SubmissionTimeUtc = ZDateTime.UtcNow.AddDays(1);
			application1.HP_HV = campaign1.PK;

			Factory.Save();

			AssertEquals(application1.SubmissionTimeLocal, applicant.LastSubmittedJobApplicationSubmissionTime);
		}

		#region Patterns

		public void TestDeleteAllPatternsOfJobApplicant()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, applicant);

			Factory.Save();
			applicant.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Applicant should be deleted", true, applicant.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Applicant patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteAllPatternsOfJobApplicant_DoesNotDeletePatternsOrResultsFromPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var personPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);
			var personResults = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, person);

			var applicant = person.ApplicantCollection.AddNew();

			Factory.Save();
			applicant.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Applicant should be deleted", true, applicant.IsDeleted);
				AssertEquals("Person should not deleted", false, person.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Person patterns should not be deleted", Array.Empty<BusinessObject>(), personPatterns.Where(p => p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Person results should not be deleted", Array.Empty<BusinessObject>(), personResults.Where(p => p.IsDeleted));
			});
		}

		#region Certificate Patterns

		public void TestDeleteApplicantDeletesAllCertificatesPatterns_ButDoesNotDeleteCertificates()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = applicant.PK;
			certificate.XZ_ParentTableCode = applicant.TablePrefix;

			Factory.Save();
			applicant.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Applicant should be deleted", true, applicant.IsDeleted);
				AssertEquals("Certificate should not be deleted", false, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeleteApplicantCertificatesDeletesAllCertificatesAndPatterns_ButDoesNotDeleteApplicantOrApplicantPatterns()
		{
			var applicant = Factory.NewWithValidTestData<OrgContact>();
			var applicantPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, applicant);

			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var certificatePatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = applicant.PK;
			certificate.XZ_ParentTableCode = applicant.TablePrefix;

			Factory.Save();
			applicant.Certificates.DeleteAll();

			CombineAssertions(() =>
			{
				AssertEquals("Applicant should not be deleted", false, applicant.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Applicant patterns should not be deleted", Array.Empty<BusinessObject>(), applicantPatterns.Where(p => p.IsDeleted));

				AssertEquals("Certificate should be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), certificatePatterns.Where(p => !p.IsDeleted));
			});
		}

		#endregion

		#endregion

		#region Phone numbers

		public void TestHA_FaxNum_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			jobApplicant.HA_FaxNum_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_FaxNum_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_FaxNum);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			jobApplicant.HA_RN_NKCountry = "AU";
			jobApplicant.HA_FaxNum_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_FaxNum_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_FaxNum);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, jobApplicant.HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry);

			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.France;
			jobApplicant.HA_FaxNum_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_FaxNum_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_FaxNum);
			AssertEquals("The tooltip should be blank", string.Empty, jobApplicant.HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestHA_HomePhone_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			jobApplicant.HA_HomePhone_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_HomePhone_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_HomePhone);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			jobApplicant.HA_RN_NKCountry = "AU";
			jobApplicant.HA_HomePhone_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_HomePhone_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_HomePhone);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, jobApplicant.HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);

			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.France;
			jobApplicant.HA_HomePhone_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_HomePhone_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_HomePhone);
			AssertEquals("The tooltip should be blank", string.Empty, jobApplicant.HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestHA_MobilePhone_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			jobApplicant.HA_MobilePhone_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_MobilePhone_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_MobilePhone);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			jobApplicant.HA_RN_NKCountry = "AU";
			jobApplicant.HA_MobilePhone_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_MobilePhone_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_MobilePhone);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, jobApplicant.HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry);

			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.France;
			jobApplicant.HA_MobilePhone_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_MobilePhone_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_MobilePhone);
			AssertEquals("The tooltip should be blank", string.Empty, jobApplicant.HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestHA_WorkPhone_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			jobApplicant.HA_RN_NKCountry = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			jobApplicant.HA_WorkPhone_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_WorkPhone_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_WorkPhone);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			jobApplicant.HA_RN_NKCountry = "AU";
			jobApplicant.HA_WorkPhone_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_WorkPhone_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_WorkPhone);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, jobApplicant.HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry);

			jobApplicant.HA_RN_NKCountry = Core.Constants.CountryCodes.France;
			jobApplicant.HA_WorkPhone_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, jobApplicant.HA_WorkPhone_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, jobApplicant.HA_WorkPhone);
			AssertEquals("The tooltip should be blank", string.Empty, jobApplicant.HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		#endregion

		#region SupportWebAddressValidationTest

		public class HRJobApplicantSupportWebAddressValidationControlTest : SupportWebAddressValidationTest<HRJobApplicant>
		{
			protected override HRJobApplicant GetBOToTest()
			{
				var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
				applicant.HA_RN_NKCountry = "AU";
				return applicant;
			}

			protected override void AssertCityValidationResultWhenCityIsEmpty(HRJobApplicant testBO)
			{
				Assert(!testBO.CityInfo.HasNotifications());
			}
		}

		#endregion

		public void TestIsLearningCenterUser()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant4 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant5 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			applicant1.Logs.AddNew(AutoEvents.EditedARecord, "Related Contact");
			applicant2.Logs.AddNew(AutoEvents.EditedARecord, "Related Staff");
			applicant3.Logs.AddNew(AutoEvents.EditedARecord, "Created from Contact");
			applicant4.Logs.AddNew(AutoEvents.EditedARecord, "Approved by");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			AssertEquals(true, applicant1.IsLearningCenterUser);
			AssertEquals(true, applicant2.IsLearningCenterUser);
			AssertEquals(true, applicant3.IsLearningCenterUser);
			AssertEquals(true, applicant4.IsLearningCenterUser);
			AssertEquals(false, applicant5.IsLearningCenterUser);
		}

		public void TestHasChanges()
		{
			var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();

			AssertEquals(false, jobApplicant.HasChanges);

			jobApplicant.HA_MobilePhone = "123";
			AssertEquals(true, jobApplicant.HasChanges);

			jobApplicant.ClearHasChanges();
			AssertEquals(false, jobApplicant.HasChanges);

			jobApplicant.HA_FaxNum = "123";
			AssertEquals(true, jobApplicant.HasChanges);

			jobApplicant.ClearHasChanges();
			AssertEquals(false, jobApplicant.HasChanges);

			jobApplicant.HA_HomePhone = "123";
			AssertEquals(true, jobApplicant.HasChanges);
		}

		public void TestPhoneNumberValidationWhenIsWebOrNot()
		{
			var rawValue = Globals.IsWeb;

			try
			{
				var jobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
				Factory.Save();

				Globals.IsWeb = false;
				jobApplicant.HA_MobilePhone = "123";
				AssertNoErrors(jobApplicant.HA_MobilePhoneInfo);

				jobApplicant.HA_FaxNum = "123";
				AssertNoErrors(jobApplicant.HA_FaxNumInfo);

				jobApplicant.HA_HomePhone = "123";
				AssertNoErrors(jobApplicant.HA_HomePhoneInfo);

				Globals.IsWeb = true;
				jobApplicant.HA_MobilePhone = "456";
				AssertHasErrors(jobApplicant.HA_MobilePhoneInfo);

				jobApplicant.HA_FaxNum = "456";
				AssertHasErrors(jobApplicant.HA_FaxNumInfo);

				jobApplicant.HA_HomePhone = "456";
				AssertHasErrors(jobApplicant.HA_HomePhoneInfo);
			}
			finally
			{
				Globals.IsWeb = rawValue;
			}
		}

		#region IHaveRequiredDocuments

		public void TestAddAndDeleteRequiredDocuments()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var doc = applicant.RequiredDocuments.AddNew();
			applicant.RequiredDocuments.AddNew();
			applicant.RequiredDocuments.AddNew();
			Factory.Save();

			AssertEquals("Should have 3 required documents", 3, applicant.RequiredDocuments.Count);
			AssertEquals("Required documents of the object should exist", 3, new BusinessObjectFactory().Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, applicant.PK)).Length);
			AssertNotNull("Specific required document should exist", new BusinessObjectFactory().Load<JobRequiredDocument>(doc.PK));

			applicant.RequiredDocuments.RemoveAndDelete(doc);
			Factory.Save();
			AssertEquals("Should have 2 required documents", 2, applicant.RequiredDocuments.Count);
			AssertNull("Deleted required document should not exist", new BusinessObjectFactory().Load<JobRequiredDocument>(doc.PK));

			applicant.Delete();
			Factory.Save();

			AssertEquals("Required documents of deleted object should not exist", 0, new BusinessObjectFactory().Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, applicant.PK)).Length);
		}

		#endregion

		#region Implementation

		#region Applicant

		HRJobApplicant Applicant
		{
			get
			{
				if (fApplicant == null)
				{
					fApplicant = Factory.New<HRJobApplicant>();
				}
				return fApplicant;
			}
		}
		HRJobApplicant fApplicant;

		#endregion

		#endregion
	}
}
