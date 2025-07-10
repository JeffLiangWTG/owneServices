using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using GMI = CargoWise.Glow.Model.Interfaces;
using ZDateTime = CargoWise.Types.ZDateTime;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPerson))]
	sealed class GlbPersonTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMacroIgnore_Password()
		{
			var passwordHashInfo = typeof(GlbPerson).GetProperty("PER_PasswordHash");
			var passwordSaltInfo = typeof(GlbPerson).GetProperty("PER_PasswordSalt");

			Assert("PER_PasswordHash should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordHashInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("PER_PasswordSalt should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordSaltInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
		}

		public void TestCountryCodeIsObsoleteAndMacroIgnore()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(GlbPerson).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestGetEmailAddressesFromTriggerParty()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = GlbPerson.CreateFromStaff(Factory, staff);
			person.PER_EmailAddress = "personal@email.com";

			Assert(person is IEmailAddressGetterForTrigger);
			AssertEquals("personal@email.com", person.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonalEmail).Single());

			staff.GS_EmailAddress = "work@email.com";
			AssertEquals("work@email.com", person.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail).Single());

			person.PER_EmailAddress = "";
			AssertEquals("work@email.com", person.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail).Single());
		}

		GlbStaff staff;
		IHRJobApplicant applicant;
		OrgContact contact;

		void SetupPropagationTest(bool addStaff, bool staffActive = false)
		{
			person = Factory.New<GlbPerson>();
			person.PER_HomeAddress1 = "1";
			person.PER_HomeAddress2 = "2";
			person.PER_City = "3";
			person.PER_State = "4";
			person.PER_Postcode = "5";
			person.PER_LegalName = "legal";
			person.PER_BirthDate = new ZDate(2000, 1, 1);
			person.PER_EmailAddress = "email@test.com";
			person.PER_EmailAddress2 = "email2@test.com";
			person.PER_Gender = "M";
			person.PER_MobilePhone = "0499702888";
			person.PER_MobilePhone2 = "0499702893";
			person.PER_HomePhone = "0280012200";
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_PersonalInfo = "personal";
			person.PER_Picture = new ZBlob(new byte[] { 1, 2, 3 });
			person.PER_Passport = "A0123456";
			person.PER_PassportExpiryDate = ZDate.BrettsBirthday;
			person.PER_PassportPlaceOfIssue = "UA";
			person.PER_DriversLicenseNumber = "A01234567";

			if (addStaff)
			{
				staff = Factory.New<GlbStaff>();
				staff.GS_FullName = "name staff";
				staff.GS_Birthdate = new ZDate(2001, 1, 1);
				staff.GS_Gender = "M";
				staff.GS_HomePhone = "0280012211";
				staff.GS_MobilePhone = "0499702888";
				staff.GS_RN_NKNationalityCode = "AU";
				staff.GS_IsActive = staffActive;
				staff.GS_PER = person.PK;
			}

			applicant = Factory.New<IHRJobApplicant>();
			applicant.HA_FullName = "name app";
			applicant.HA_Birthdate = new ZDate(2002, 1, 1);
			applicant.HA_Gender = "M";
			applicant.HA_HomePhone = "0280012222";
			applicant.HA_MobilePhone = "0499702888";
			applicant.HA_PER = person.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			org.OH_FullName = "name org";

			contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "name contact";
			contact.OC_OH = org.PK;
			contact.OC_PER = person.PK;
			contact.OC_Birthday = new ZDate(2001, 1, 1);
			contact.OC_Gender = "M";
			contact.OC_HomePhone = "0280012233";
			contact.OC_Mobile = "0499702888";
			contact.OC_RN_NKNationality = "GB";
			contact.OC_PER = person.PK;

			Factory.Save();
			person.ApplicantCollection.Load();
		}

		void AssertPerson(GlbPerson person, ZString name, ZDate birthdate, ZString gender, ZString homePhone, ZString mobilePhone, ZString nationality)
		{
			AssertEquals(name, person.PER_FullName);
			AssertEquals(birthdate, person.PER_BirthDate);
			AssertEquals(gender, person.PER_Gender);
			AssertEquals(homePhone, person.PER_HomePhone);
			AssertEquals(mobilePhone, person.PER_MobilePhone);
			AssertEquals(nationality, person.PER_RN_NKNationalityCodeISO);
		}

		void AssertStaff(GlbStaff staff, ZString name, ZDate birthdate, ZString gender, ZString homePhone, ZString mobilePhone, ZString nationality)
		{
			AssertEquals(name, staff.GS_FullName);
			AssertEquals(birthdate, staff.GS_Birthdate);
			AssertEquals(gender, staff.GS_Gender);
			AssertEquals(homePhone, staff.GS_HomePhone);
			AssertEquals(mobilePhone, staff.GS_MobilePhone);
			AssertEquals(nationality, staff.GS_RN_NKNationalityCode);
		}

		void AssertApplicant(IHRJobApplicant applicant, ZString name, ZDate birthdate, ZString gender, ZString homePhone, ZString mobilePhone)
		{
			AssertEquals(name, applicant.HA_FullName);
			AssertEquals(birthdate, applicant.HA_Birthdate);
			AssertEquals(gender, applicant.HA_Gender);
			AssertEquals(homePhone, applicant.HA_HomePhone);
			AssertEquals(mobilePhone, applicant.HA_MobilePhone);
		}

		void AssertContact(OrgContact contact, ZString name, ZDate birthdate, ZString gender, ZString homePhone, ZString mobilePhone, ZString nationality)
		{
			AssertEquals(name, contact.OC_ContactName);
			AssertEquals(birthdate, contact.OC_Birthday);
			AssertEquals(gender, contact.OC_Gender);
			AssertEquals(homePhone, contact.OC_HomePhone);
			AssertEquals(mobilePhone, contact.OC_Mobile);
			AssertEquals(nationality, contact.OC_RN_NKNationality);
		}

		public void TestPropagation_ActiveStaff()
		{
			SetupPropagationTest(true, true);

			person.PER_FullName = "name 1";
			person.PER_BirthDate = new ZDate(1900, 1, 1);
			person.PER_Gender = "F";
			person.PER_HomePhone = "0211111111";
			person.PER_MobilePhone = "0411111111";
			person.PER_RN_NKNationalityCodeISO = "RU";
			Factory.Save();

			AssertPerson(person, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");
			AssertStaff(staff, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");
			AssertApplicant(applicant, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111");
			AssertContact(contact, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");

			staff.GS_FullName = "name 2";
			staff.GS_Birthdate = new ZDate(1901, 1, 1);
			staff.GS_Gender = "A";
			staff.GS_HomePhone = "0211111222";
			staff.GS_MobilePhone = "0411111222";
			staff.GS_RN_NKNationalityCode = "UA";
			Factory.Save();

			AssertPerson(person, "name 2", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222", "UA");
			AssertStaff(staff, "name 2", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222", "UA");
			AssertApplicant(applicant, "name 2", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222");
			AssertContact(contact, "name 2", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");

			contact.OC_ContactName = "name 3";
			contact.OC_Birthday = new ZDate(1902, 1, 1);
			contact.OC_Gender = "N";
			contact.OC_HomePhone = "0211111333";
			contact.OC_Mobile = "0411111333";
			contact.OC_RN_NKNationality = "AU";
			Factory.Save();

			AssertPerson(person, "name 3", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222", "UA");
			AssertStaff(staff, "name 3", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222", "UA");
			AssertApplicant(applicant, "name 3", new ZDate(1901, 1, 1), "A", "0211111222", "0411111222");
			AssertContact(contact, "name 3", new ZDate(1902, 1, 1), "N", "0211111333", "0411111333", "AU");

			applicant.HA_FullName = "name 4";
			applicant.HA_Birthdate = new ZDate(1903, 1, 1);
			applicant.HA_Gender = "M";
			applicant.HA_HomePhone = "0211111444";
			applicant.HA_MobilePhone = "0411111444";
			Factory.Save();

			AssertPerson(person, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "UA");
			AssertStaff(staff, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "UA");
			AssertApplicant(applicant, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444");
			AssertContact(contact, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111333", "UA"); // not updated because contact's #0411111333 is different from person's #0411111222 from last test
		}

		public void TestPropagation_InactiveStaff()
		{
			SetupPropagationTest(true, false);

			person.PER_FullName = "name 1";
			person.PER_BirthDate = new ZDate(1900, 1, 1);
			person.PER_Gender = "F";
			person.PER_HomePhone = "0211111111";
			person.PER_MobilePhone = "0411111111";
			person.PER_RN_NKNationalityCodeISO = "RU";
			Factory.Save();

			AssertPerson(person, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");
			AssertStaff(staff, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");
			AssertApplicant(applicant, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111");
			AssertContact(contact, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");

			staff.GS_FullName = "name 2";
			staff.GS_Birthdate = new ZDate(1901, 1, 1);
			staff.GS_Gender = "N";
			staff.GS_HomePhone = "0211111222";
			staff.GS_MobilePhone = "0411111222";
			staff.GS_RN_NKNationalityCode = "UA";
			Factory.Save();

			AssertPerson(person, "name 2", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");
			AssertStaff(staff, "name 2", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");
			AssertApplicant(applicant, "name 2", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222");
			AssertContact(contact, "name 2", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");

			contact.OC_ContactName = "name 3";
			contact.OC_Birthday = new ZDate(1902, 1, 1);
			contact.OC_Gender = "N";
			contact.OC_HomePhone = "0211111333";
			contact.OC_Mobile = "0411111333";
			contact.OC_RN_NKNationality = "AU";
			Factory.Save();

			AssertPerson(person, "name 3", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");
			AssertStaff(staff, "name 3", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222", "UA");
			AssertApplicant(applicant, "name 3", new ZDate(1901, 1, 1), "N", "0211111222", "0411111222");
			AssertContact(contact, "name 3", new ZDate(1902, 1, 1), "N", "0211111333", "0411111333", "AU");

			applicant.HA_FullName = "name 4";
			applicant.HA_Birthdate = new ZDate(1903, 1, 1);
			applicant.HA_Gender = "M";
			applicant.HA_HomePhone = "0211111444";
			applicant.HA_MobilePhone = "0411111444";
			Factory.Save();

			AssertPerson(person, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "UA");
			AssertStaff(staff, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "UA");
			AssertApplicant(applicant, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444");
			AssertContact(contact, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111333", "UA"); // not updated because contact's #0411111333 is different from person's #0411111222 from last test
		}

		public void TestPropagation_NoStaff()
		{
			SetupPropagationTest(false);

			person.PER_FullName = "name 1";
			person.PER_BirthDate = new ZDate(1900, 1, 1);
			person.PER_Gender = "F";
			person.PER_HomePhone = "0211111111";
			person.PER_MobilePhone = "0411111111";
			person.PER_RN_NKNationalityCodeISO = "RU";
			Factory.Save();

			AssertPerson(person, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");
			AssertApplicant(applicant, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111");
			AssertContact(contact, "name 1", new ZDate(1900, 1, 1), "F", "0211111111", "0411111111", "RU");

			contact.OC_ContactName = "name 3";
			contact.OC_Birthday = new ZDate(1902, 1, 1);
			contact.OC_Gender = "N";
			contact.OC_HomePhone = "0211111333";
			contact.OC_Mobile = "0411111333";
			contact.OC_RN_NKNationality = "AU";
			Factory.Save();

			AssertPerson(person, "name 3", new ZDate(1902, 1, 1), "N", "0211111333", "0411111333", "AU");
			AssertApplicant(applicant, "name 3", new ZDate(1902, 1, 1), "N", "0211111333", "0411111333");
			AssertContact(contact, "name 3", new ZDate(1902, 1, 1), "N", "0211111333", "0411111333", "AU");

			applicant.HA_FullName = "name 4";
			applicant.HA_Birthdate = new ZDate(1903, 1, 1);
			applicant.HA_Gender = "M";
			applicant.HA_HomePhone = "0211111444";
			applicant.HA_MobilePhone = "0411111444";
			Factory.Save();

			AssertPerson(person, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "AU");
			AssertApplicant(applicant, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444");
			AssertContact(contact, "name 4", new ZDate(1903, 1, 1), "M", "0211111444", "0411111444", "AU");
		}

		public void TestHumanReadableNameCore()
		{
			var person = Factory.New<GlbPerson>();

			AssertEquals("Person", person.HumanReadableName);

			person.PER_FullName = "full name";
			AssertEquals("Person - full name", person.HumanReadableName);
		}

		public void TestInternalPropertiesWithoutSecurity()
		{
			SetupSecurity(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					AssertEquals("legal", personLoaded.PER_LegalNameInternal);
					AssertEquals(ZDate.BrettsBirthday, personLoaded.PER_BirthDateInternal);
					AssertEquals("full", personLoaded.PER_FullNameInternal);
					AssertEquals("M", personLoaded.PER_GenderInternal);
					AssertEquals("0499702888", personLoaded.PER_MobilePhoneInternal);
					AssertEquals("0499702893", personLoaded.PER_MobilePhone2Internal);
					AssertEquals("0280012200", personLoaded.PER_HomePhoneInternal);
					AssertEquals("0280012201", personLoaded.PER_FaxNumberInternal);
					AssertEquals("AU", personLoaded.PER_RN_NKCountryInternal);
					AssertEquals("UA", personLoaded.PER_RN_NKNationalityCodeISOInternal);
					AssertEquals("personal", personLoaded.PER_PersonalInfoInternal);
					AssertEquals("1", personLoaded.PER_HomeAddress1Internal);
					AssertEquals("2", personLoaded.PER_HomeAddress2Internal);
					AssertEquals("3", personLoaded.PER_CityInternal);
					AssertEquals("4", personLoaded.PER_StateInternal);
					AssertEquals("5", personLoaded.PER_PostcodeInternal);
					AssertEquals("email@test.com", personLoaded.PER_EmailAddressInternal);
					AssertEquals("email2@test.com", personLoaded.PER_EmailAddress2Internal);
					AssertEquals("A01234567", personLoaded.PER_DriversLicenseNumberInternal);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_LegalName);
					AssertEquals(ZDate.Empty, personLoaded.PER_BirthDate);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_EmailAddress);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_EmailAddress2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_Gender);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomePhone);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_RN_NKNationalityCodeISO);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_PersonalInfo);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomeAddress1);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomeAddress2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_City);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_State);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_Postcode);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone_Formatted);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomePhone_Formatted);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_DriversLicenseNumber);

					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PrimaryWorkplace);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PrimaryWorkplaceCode);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PrimaryUNLOCO);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PrimaryJobTitle);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PrimaryEmail);

					Assert(personLoaded.PER_LegalNameInfo.ReadOnly);
					Assert(personLoaded.PER_BirthDateInfo.ReadOnly);
					Assert(personLoaded.PER_EmailAddressInfo.ReadOnly);
					Assert(personLoaded.PER_EmailAddress2Info.ReadOnly);
					Assert(personLoaded.PER_GenderInfo.ReadOnly);
					Assert(personLoaded.PER_MobilePhoneInfo.ReadOnly);
					Assert(personLoaded.PER_MobilePhone2Info.ReadOnly);
					Assert(personLoaded.PER_HomePhoneInfo.ReadOnly);
					Assert(personLoaded.PER_RN_NKNationalityCodeISOInfo.ReadOnly);
					Assert(personLoaded.PER_PersonalInfoInfo.ReadOnly);
					Assert(personLoaded.PER_PictureInfo.ReadOnly);
					Assert(personLoaded.PER_HomeAddress1Info.ReadOnly);
					Assert(personLoaded.PER_HomeAddress2Info.ReadOnly);
					Assert(personLoaded.PER_CityInfo.ReadOnly);
					Assert(personLoaded.PER_StateInfo.ReadOnly);
					Assert(personLoaded.PER_PostcodeInfo.ReadOnly);
					Assert(personLoaded.PER_MobilePhone_FormattedInfo.ReadOnly);
					Assert(personLoaded.PER_HomePhone_FormattedInfo.ReadOnly);
					Assert(personLoaded.PER_DriversLicenseNumberInfo.ReadOnly);
				}
			}
		}

		public void TestGetLastAttempt()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var accred = Factory.New<IGlbAccreditation>();
			accred.HAC_Code = "ZZZ";
			var attempt1 = person.AccreditationAttemptCollection.AddNew() as IGlbAccreditationAttempt;
			attempt1.HAA_CommencementDate = ZDate.Today.AddDays(-100);
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(-90);
			attempt1.HAA_HAC = accred.PK;

			Factory.Save();
			AssertEquals(attempt1, person.GetLastAttempt(accred.PK));

			var attempt2 = person.AccreditationAttemptCollection.AddNew() as IGlbAccreditationAttempt;
			attempt2.HAA_CommencementDate = ZDate.Today.AddDays(-80);
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(-60);
			attempt2.HAA_CompletionDate = ZDate.Today.AddDays(-60);
			attempt2.HAA_HAC = accred.PK;

			Factory.Save();
			AssertEquals(attempt2, person.GetLastAttempt(accred.PK));

			var attempt3 = person.AccreditationAttemptCollection.AddNew() as IGlbAccreditationAttempt;
			attempt3.HAA_CommencementDate = ZDate.Today.AddDays(-10);
			attempt3.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt3.HAA_HAC = accred.PK;

			Factory.Save();
			AssertEquals(attempt3, person.GetLastAttempt(accred.PK));

			var attempt4 = person.AccreditationAttemptCollection.AddNew() as IGlbAccreditationAttempt;
			attempt4.HAA_CommencementDate = ZDate.Today.AddDays(-30);
			attempt4.HAA_CompletionDueDate = ZDate.Today.AddDays(-20);
			attempt4.HAA_HAC = accred.PK;

			Factory.Save();
			AssertEquals(attempt3, person.GetLastAttempt(accred.PK));

			AssertEquals(attempt4, person.GetLastAttempt(accred.PK, attempt3.PK));

			AssertEquals(attempt2, person.GetLastAttempt(accred.PK, true));
		}

		public void TestGetAttempt()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var accred = Factory.New<IGlbAccreditation>();
			accred.HAC_Code = "ZZZ";
			var attemptBizO = person.AccreditationAttemptCollection.AddNew();
			var attempt = attemptBizO as IGlbAccreditationAttempt;
			attempt.HAA_CommencementDate = ZDate.Today.AddDays(-1);
			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt.HAA_HAC = accred.PK;

			Factory.Save();

			AssertNull(person.GetCurrentAttempt(ZGuid.NewZGuid(), ZDate.Today));

			AssertEquals(attempt, person.GetCurrentAttempt(accred.PK, ZDate.Today));
		}

		public void TestDeleteAttempts()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = person.AccreditationAttemptCollection.AddNew();
			var attempt2 = person.AccreditationAttemptCollection.AddNew();
			var attempt3 = person.AccreditationAttemptCollection.AddNew();

			person.DeleteAttempts(new BusinessObject[] { attempt1, attempt2 });

			AssertEquals(false, person.AccreditationAttemptCollection.Contains(attempt1));
			AssertEquals(false, person.AccreditationAttemptCollection.Contains(attempt2));
			AssertEquals(true, person.AccreditationAttemptCollection.Contains(attempt3));
		}

		#region Test Securities

		GlbStaff staffDenied;
		GlbStaff staffGranted;
		GlbPerson person;

		void SetupEditSecurity(bool legal, bool dob, bool email, bool gender, bool mobile, bool homePhone, bool address, bool nationality, bool personal, bool image, bool passport, bool licence)
		{
			staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "1";
			person.PER_HomeAddress2 = "2";
			person.PER_City = "3";
			person.PER_State = "4";
			person.PER_Postcode = "5";
			person.PER_LegalName = "legal";
			person.PER_BirthDate = ZDate.BrettsBirthday;
			person.PER_EmailAddress = "email@test.com";
			person.PER_EmailAddress2 = "email2@test.com";
			person.PER_Gender = "M";
			person.PER_MobilePhone = "0499702888";
			person.PER_MobilePhone2 = "0499702893";
			person.PER_HomePhone = "0280012200";
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_PersonalInfo = "personal";
			person.PER_Picture = new ZBlob(new byte[] { 1, 2, 3 });
			person.PER_Passport = "A0123456";
			person.PER_PassportExpiryDate = ZDate.BrettsBirthday;
			person.PER_PassportPlaceOfIssue = "UA";
			person.PER_DriversLicenseNumber = "A01234567";

			if (legal)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceLegalName);
			}

			if (dob)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceDateOfBirth);
			}

			if (email)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceEmail);
			}

			if (gender)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceGender);
			}

			if (mobile)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceMobile);
			}

			if (homePhone)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceHomePhone);
			}

			if (address)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceHomeAddress);
			}

			if (nationality)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceNationality);
			}

			if (personal)
			{
				AddEditSecurity(Env.Security.PersonIntelligencePersonalInformation);
			}

			if (image)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceImage);
			}

			if (passport)
			{
				AddEditSecurity(Env.Security.PersonIntelligencePassport);
			}

			if (licence)
			{
				AddEditSecurity(Env.Security.PersonIntelligenceDriversLicenseNumber);
			}

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceView, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceView, true, staffDenied.PK));

			Factory.Save();
		}

		void AddEditSecurity(SecurityCheckpoint checkpoint)
		{
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, true, staffGranted.PK));
		}

		void SetupSecurity(bool legal, bool dob, bool email, bool gender, bool mobile, bool homePhone, bool address, bool nationality, bool personal, bool image, bool passport, bool licence, bool primaryWorkplace, bool challengePhrase, bool fax)
		{
			staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "1";
			person.PER_HomeAddress2 = "2";
			person.PER_City = "3";
			person.PER_State = "4";
			person.PER_Postcode = "5";
			person.PER_LegalName = "legal";
			person.PER_FullName = "full";
			person.PER_BirthDate = ZDate.BrettsBirthday;
			person.PER_EmailAddress = "email@test.com";
			person.PER_EmailAddress2 = "email2@test.com";
			person.PER_Gender = "M";
			person.PER_MobilePhone = "0499702888";
			person.PER_MobilePhone2 = "0499702893";
			person.PER_FaxNumber = "0280012201";
			person.PER_HomePhone = "0280012200";
			person.PER_HomePhone_IsManuallyVerified = true;
			person.PER_RN_NKNationalityCodeISO = "UA";
			person.PER_RN_NKCountry = "AU";
			person.PER_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			person.PER_PersonalInfo = "personal";
			person.PER_Picture = new ZBlob(new byte[] { 1, 2, 3 });
			person.PER_Passport = "A0123456";
			person.PER_PassportExpiryDate = ZDate.BrettsBirthday;
			person.PER_PassportPlaceOfIssue = "UA";
			person.PER_DriversLicenseNumber = "A01234567";
			person.PER_ChallengePhrase = "01";
			person.PER_ChallengePhraseType = "02";

			if (legal)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewLegalName);
			}

			if (dob)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewDateOfBirth);
			}

			if (email)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewEmail);
			}

			if (gender)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewGender);
			}

			if (mobile)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewMobile);
			}

			if (homePhone)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewHomePhone);
			}

			if (address)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewHomeAddress);
			}

			if (nationality)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewNationality);
			}

			if (personal)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewPersonalInformation);
			}

			if (image)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewImage);
			}

			if (passport)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewPassport);
			}

			if (licence)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewDriversLicenseNumber);
			}

			if (primaryWorkplace)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewPrimaryWorkplace);
			}

			if (challengePhrase)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewChallengePhrase);
			}

			if (fax)
			{
				AddSecurity(Env.Security.PersonIntelligenceViewFaxNumber);
			}

			Factory.Save();
		}

		void AddSecurity(SecurityCheckpoint checkpoint)
		{
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(checkpoint, true, staffGranted.PK));

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));
		}

		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		public void TestLegalNameSecurity()
		{
			AssertSecurity(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestDateOfBirthSecurity()
		{
			AssertSecurity(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestEmailSecurity()
		{
			AssertSecurity(false, false, true, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestGenderSecurity()
		{
			AssertSecurity(false, false, false, true, false, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestMobileSecurity()
		{
			AssertSecurity(false, false, false, false, true, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestHomePhoneSecurity()
		{
			AssertSecurity(false, false, false, false, false, true, false, false, false, false, false, false, false, false, false);
		}

		public void TestNationalitySecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, true, false, false, false, false, false, false, false);
		}

		public void TestPersonalInfoSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, true, false, false, false, false, false, false);
		}

		public void TestImageSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, true, false, false, false, false, false);
		}

		public void TestViewHomeAddressSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, true, false, false, false, false, false, false, false, false);
		}

		public void TestPassportSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false);
		}

		public void TestDriversLicenceSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false);
		}

		public void TestPrimaryWorkplaceSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, false, false, true, false, false);
		}

		public void TestPrimaryWorkplaceCodeSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, false, false, true, false, false);
		}

		public void TestChallengePhraseSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, false, false, false, true, false);
		}

		public void TestFaxNumberSecurity()
		{
			AssertSecurity(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true);
		}

		public void TestLegalNameSecurityEdit()
		{
			AssertEditSecurity(true, false, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestDateOfBirthSecurityEdit()
		{
			AssertEditSecurity(false, true, false, false, false, false, false, false, false, false, false, false);
		}

		public void TestEmailSecurityEdit()
		{
			AssertEditSecurity(false, false, true, false, false, false, false, false, false, false, false, false);
		}

		public void TestGenderSecurityEdit()
		{
			AssertEditSecurity(false, false, false, true, false, false, false, false, false, false, false, false);
		}

		public void TestMobileSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, true, false, false, false, false, false, false, false);
		}

		public void TestHomePhoneSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, true, false, false, false, false, false, false);
		}

		public void TestNationalitySecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, true, false, false, false, false);
		}

		public void TestPersonalInfoSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, true, false, false, false);
		}

		public void TestImageSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, false, true, false, false);
		}

		public void TestViewHomeAddressSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, true, false, false, false, false, false);
		}

		public void TestPassportSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, false, false, true, false);
		}

		public void TestDriversLicenceSecurityEdit()
		{
			AssertEditSecurity(false, false, false, false, false, false, false, false, false, false, false, true);
		}

		void AssertSecurity(bool legal, bool dob, bool email, bool gender, bool mobile, bool homePhone, bool address, bool nationality, bool personal, bool image, bool passport, bool licence, bool primaryWorkplace, bool challengePhrase, bool fax)
		{
			SetupSecurity(legal, dob, email, gender, mobile, homePhone, address, nationality, personal, image, passport, licence, primaryWorkplace, challengePhrase, fax);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					AssertEquals(legal, personLoaded.ViewDeniedMessage == personLoaded.PER_LegalName);
					AssertEquals(dob, ZDate.Empty == personLoaded.PER_BirthDate);
					AssertEquals(dob, -1 == personLoaded.PER_Age);
					AssertEquals(dob, personLoaded.ViewDeniedMessage == personLoaded.PER_BirthDateAndAge_Formatted);
					AssertEquals(email, personLoaded.ViewDeniedMessage == personLoaded.PER_EmailAddress);
					AssertEquals(email, personLoaded.ViewDeniedMessage == personLoaded.PER_EmailAddress2);
					AssertEquals(gender, personLoaded.ViewDeniedMessage == personLoaded.PER_Gender);
					AssertEquals(mobile, personLoaded.ViewDeniedMessage == personLoaded.PER_MobilePhone);
					AssertEquals(mobile, personLoaded.ViewDeniedMessage == personLoaded.PER_MobilePhone2);
					AssertEquals(mobile, personLoaded.ViewDeniedMessage == personLoaded.PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(homePhone, personLoaded.ViewDeniedMessage == personLoaded.PER_HomePhone);
					AssertEquals(homePhone, personLoaded.ViewDeniedMessage == personLoaded.PER_HomePhone_Formatted);
					AssertEquals(homePhone, personLoaded.ViewDeniedMessage == personLoaded.PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(homePhone, false == personLoaded.PER_HomePhone_IsManuallyVerified);
					AssertEquals(nationality, personLoaded.ViewDeniedMessage == personLoaded.PER_RN_NKNationalityCodeISO);
					AssertEquals(personal, personLoaded.ViewDeniedMessage == personLoaded.PER_PersonalInfo);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_HomeAddress1);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_HomeAddress2);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_City);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_State);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_Postcode);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.AddressMap);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.PER_ValidationStatus);
					AssertEquals(address, personLoaded.ViewDeniedMessage == personLoaded.State);
					AssertEquals(passport, personLoaded.ViewDeniedMessage == personLoaded.PER_Passport);
					AssertEquals(passport, ZDate.Empty == personLoaded.PER_PassportExpiryDate);
					AssertEquals(passport, personLoaded.ViewDeniedMessage == personLoaded.PER_PassportPlaceOfIssue);
					AssertEquals(licence, personLoaded.ViewDeniedMessage == personLoaded.PER_DriversLicenseNumber);
					AssertEquals(challengePhrase, personLoaded.ViewDeniedMessage == personLoaded.PER_ChallengePhrase);
					AssertEquals(challengePhrase, personLoaded.ViewDeniedMessage == personLoaded.PER_ChallengePhraseType);
					AssertEquals(fax, personLoaded.ViewDeniedMessage == personLoaded.PER_FaxNumber);
					AssertEquals(fax, personLoaded.ViewDeniedMessage == personLoaded.PER_FaxNum_Formatted);
					AssertEquals(fax, personLoaded.ViewDeniedMessage == personLoaded.PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry);

					AssertEquals(primaryWorkplace, personLoaded.ViewDeniedMessage == personLoaded.PrimaryWorkplace);
					AssertEquals(primaryWorkplace, personLoaded.ViewDeniedMessage == personLoaded.PrimaryWorkplaceCode);
					AssertEquals(primaryWorkplace, personLoaded.ViewDeniedMessage == personLoaded.PrimaryUNLOCO);
					AssertEquals(primaryWorkplace, personLoaded.ViewDeniedMessage == personLoaded.PrimaryJobTitle);
					AssertEquals(primaryWorkplace, personLoaded.ViewDeniedMessage == personLoaded.PrimaryEmail);

					AssertEquals(legal, personLoaded.PER_LegalNameInfo.ReadOnly);
					AssertEquals(dob, personLoaded.PER_BirthDateInfo.ReadOnly);
					AssertEquals(email, personLoaded.PER_EmailAddressInfo.ReadOnly);
					AssertEquals(email, personLoaded.PER_EmailAddress2Info.ReadOnly);
					AssertEquals(gender, personLoaded.PER_GenderInfo.ReadOnly);
					AssertEquals(mobile, personLoaded.PER_MobilePhoneInfo.ReadOnly);
					AssertEquals(mobile, personLoaded.PER_MobilePhone2Info.ReadOnly);
					AssertEquals(homePhone, personLoaded.PER_HomePhoneInfo.ReadOnly);
					AssertEquals(nationality, personLoaded.PER_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(personal, personLoaded.PER_PersonalInfoInfo.ReadOnly);
					AssertEquals(image, personLoaded.PER_PictureInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_HomeAddress1Info.ReadOnly);
					AssertEquals(address, personLoaded.PER_HomeAddress2Info.ReadOnly);
					AssertEquals(address, personLoaded.PER_CityInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_StateInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_PostcodeInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportExpiryDateInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportPlaceOfIssueInfo.ReadOnly);
					AssertEquals(licence, personLoaded.PER_DriversLicenseNumberInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var personLoaded2 = factoryForLoading2.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));
					var now = ZDateTime.Now;
					var brettsBirthday = ZDate.BrettsBirthday;
					ZInt brettAge;
					if (now.DayOfYear < brettsBirthday.DayOfYear)
					{
						brettAge = now.Year - brettsBirthday.Year - 1;
					}
					else
					{
						brettAge = now.Year - brettsBirthday.Year;
					}
					var formattedBirthDateAndAge = FormattableString.Invariant($"{brettsBirthday.ToShortDateString()} ({brettAge} yrs)");

					AssertEquals(legal, "legal" == personLoaded2.PER_LegalName);
					AssertEquals(dob, ZDate.BrettsBirthday == personLoaded2.PER_BirthDate);
					AssertEquals(dob, brettAge == personLoaded2.PER_Age);
					AssertEquals(dob, formattedBirthDateAndAge == personLoaded2.PER_BirthDateAndAge_Formatted);
					AssertEquals(email, "email@test.com" == personLoaded2.PER_EmailAddress);
					AssertEquals(email, "email2@test.com" == personLoaded2.PER_EmailAddress2);
					AssertEquals(gender, "M" == personLoaded2.PER_Gender);
					AssertEquals(mobile, "0499702888" == personLoaded2.PER_MobilePhone);
					AssertEquals(mobile, "0499702893" == personLoaded2.PER_MobilePhone2);
					AssertEquals(mobile, string.Empty == personLoaded2.PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(homePhone, "0280012200" == personLoaded2.PER_HomePhone);
					AssertEquals(homePhone, "0280012200" == personLoaded2.PER_HomePhone_Formatted);
					AssertEquals(homePhone, string.Empty == personLoaded2.PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
					AssertEquals(homePhone, true == personLoaded2.PER_HomePhone_IsManuallyVerified);
					AssertEquals(nationality, "UA" == personLoaded2.PER_RN_NKNationalityCodeISO);
					AssertEquals(personal, "personal" == personLoaded2.PER_PersonalInfo);
					AssertEquals(address, "1" == personLoaded2.PER_HomeAddress1);
					AssertEquals(address, "2" == personLoaded2.PER_HomeAddress2);
					AssertEquals(address, "3" == personLoaded2.PER_City);
					AssertEquals(address, "4" == personLoaded2.PER_State);
					AssertEquals(address, "5" == personLoaded2.PER_Postcode);
					AssertEquals(address, string.Empty == personLoaded2.AddressMap);
					AssertEquals(address, AddressValidationStatus.ManuallyVerified == personLoaded2.PER_ValidationStatus);
					AssertEquals(address, string.Empty == personLoaded2.State);
					AssertEquals(passport, "A0123456" == personLoaded2.PER_Passport);
					AssertEquals(passport, ZDate.BrettsBirthday == personLoaded2.PER_PassportExpiryDate);
					AssertEquals(passport, "UA" == personLoaded2.PER_PassportPlaceOfIssue);
					AssertEquals(licence, "A01234567" == personLoaded2.PER_DriversLicenseNumber);
					AssertEquals(challengePhrase, "01" == personLoaded2.PER_ChallengePhrase);
					AssertEquals(challengePhrase, "02" == personLoaded2.PER_ChallengePhraseType);
					AssertEquals(fax, "0280012201" == personLoaded2.PER_FaxNumber);
					AssertEquals(fax, "0280012201" == personLoaded2.PER_FaxNum_Formatted);
					AssertEquals(fax, string.Empty == personLoaded2.PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry);

					AssertEquals(primaryWorkplace, ZString.Empty == personLoaded2.PrimaryWorkplace);
					AssertEquals(primaryWorkplace, ZString.Empty == personLoaded2.PrimaryWorkplaceCode);
					AssertEquals(primaryWorkplace, ZString.Empty == personLoaded2.PrimaryUNLOCO);
					AssertEquals(primaryWorkplace, ZString.Empty == personLoaded2.PrimaryJobTitle);
					AssertEquals(primaryWorkplace, ZString.Empty == personLoaded2.PrimaryEmail);

					AssertEquals(legal, !personLoaded2.PER_LegalNameInfo.ReadOnly);
					AssertEquals(dob, !personLoaded2.PER_BirthDateInfo.ReadOnly);
					AssertEquals(email, !personLoaded2.PER_EmailAddressInfo.ReadOnly);
					AssertEquals(email, !personLoaded2.PER_EmailAddress2Info.ReadOnly);
					AssertEquals(gender, !personLoaded2.PER_GenderInfo.ReadOnly);
					AssertEquals(mobile, !personLoaded2.PER_MobilePhoneInfo.ReadOnly);
					AssertEquals(mobile, !personLoaded2.PER_MobilePhone2Info.ReadOnly);
					AssertEquals(homePhone, !personLoaded2.PER_HomePhoneInfo.ReadOnly);
					AssertEquals(nationality, !personLoaded2.PER_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(personal, !personLoaded2.PER_PersonalInfoInfo.ReadOnly);
					AssertEquals(image, !personLoaded2.PER_PictureInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_HomeAddress1Info.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_HomeAddress2Info.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_CityInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_StateInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_PostcodeInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportExpiryDateInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportPlaceOfIssueInfo.ReadOnly);
					AssertEquals(licence, !personLoaded2.PER_DriversLicenseNumberInfo.ReadOnly);
				}
			}
		}

		void AssertEditSecurity(bool legal, bool dob, bool email, bool gender, bool mobile, bool homePhone, bool address, bool nationality, bool personal, bool image, bool passport, bool licence)
		{
			SetupEditSecurity(legal, dob, email, gender, mobile, homePhone, address, nationality, personal, image, passport, licence);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					AssertEquals(legal, personLoaded.PER_LegalNameInfo.ReadOnly);
					AssertEquals(dob, personLoaded.PER_BirthDateInfo.ReadOnly);
					AssertEquals(email, personLoaded.PER_EmailAddressInfo.ReadOnly);
					AssertEquals(email, personLoaded.PER_EmailAddress2Info.ReadOnly);
					AssertEquals(gender, personLoaded.PER_GenderInfo.ReadOnly);
					AssertEquals(mobile, personLoaded.PER_MobilePhoneInfo.ReadOnly);
					AssertEquals(mobile, personLoaded.PER_MobilePhone2Info.ReadOnly);
					AssertEquals(homePhone, personLoaded.PER_HomePhoneInfo.ReadOnly);
					AssertEquals(nationality, personLoaded.PER_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(personal, personLoaded.PER_PersonalInfoInfo.ReadOnly);
					AssertEquals(image, personLoaded.PER_PictureInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_HomeAddress1Info.ReadOnly);
					AssertEquals(address, personLoaded.PER_HomeAddress2Info.ReadOnly);
					AssertEquals(address, personLoaded.PER_CityInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_StateInfo.ReadOnly);
					AssertEquals(address, personLoaded.PER_PostcodeInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportExpiryDateInfo.ReadOnly);
					AssertEquals(passport, personLoaded.PER_PassportPlaceOfIssueInfo.ReadOnly);
					AssertEquals(licence, personLoaded.PER_DriversLicenseNumberInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var personLoaded2 = factoryForLoading2.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					AssertEquals(legal, !personLoaded2.PER_LegalNameInfo.ReadOnly);
					AssertEquals(dob, !personLoaded2.PER_BirthDateInfo.ReadOnly);
					AssertEquals(email, !personLoaded2.PER_EmailAddressInfo.ReadOnly);
					AssertEquals(email, !personLoaded2.PER_EmailAddress2Info.ReadOnly);
					AssertEquals(gender, !personLoaded2.PER_GenderInfo.ReadOnly);
					AssertEquals(mobile, !personLoaded2.PER_MobilePhoneInfo.ReadOnly);
					AssertEquals(mobile, !personLoaded2.PER_MobilePhone2Info.ReadOnly);
					AssertEquals(homePhone, !personLoaded2.PER_HomePhoneInfo.ReadOnly);
					AssertEquals(nationality, !personLoaded2.PER_RN_NKNationalityCodeISOInfo.ReadOnly);
					AssertEquals(personal, !personLoaded2.PER_PersonalInfoInfo.ReadOnly);
					AssertEquals(image, !personLoaded2.PER_PictureInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_HomeAddress1Info.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_HomeAddress2Info.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_CityInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_StateInfo.ReadOnly);
					AssertEquals(address, !personLoaded2.PER_PostcodeInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportExpiryDateInfo.ReadOnly);
					AssertEquals(passport, !personLoaded2.PER_PassportPlaceOfIssueInfo.ReadOnly);
					AssertEquals(licence, !personLoaded2.PER_DriversLicenseNumberInfo.ReadOnly);
				}
			}
		}

		#region Validation Security

		public void TestStandalonePersonValidationWhenPassportSecurityDenied()
		{
			SetupSecurity(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false);

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewMobile, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewMobile, true, staffGranted.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, true, staffGranted.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewDriversLicenseNumber, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewDriversLicenseNumber, true, staffGranted.PK));

			person.PER_MobilePhone = ZString.Empty;
			person.PER_EmailAddress = ZString.Empty;
			person.PER_Passport = ZString.Empty;
			person.PER_DriversLicenseNumber = ZString.Empty;

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					personLoaded.Validation.ValidatePER_Passport();
					AssertNoErrors("Should not check security since access rights are denied", personLoaded.PER_PassportInfo);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var personLoaded2 = factoryForLoading2.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					personLoaded2.Validation.ValidatePER_Passport();
					AssertHasError("Should check security since access rights are enabled", personLoaded2.PER_PassportInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
				}
			}
		}

		public void TestStandalonePersonValidationWhenDriversLicenseNumberSecurityDenied()
		{
			SetupSecurity(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false);

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewMobile, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewMobile, true, staffGranted.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, true, staffGranted.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewPassport, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewPassport, true, staffGranted.PK));

			person.PER_MobilePhone = ZString.Empty;
			person.PER_EmailAddress = ZString.Empty;
			person.PER_Passport = ZString.Empty;
			person.PER_DriversLicenseNumber = ZString.Empty;

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					personLoaded.Validation.ValidatePER_DriversLicenseNumber();
					AssertNoErrors("Should not check security since access rights are denied", personLoaded.PER_DriversLicenseNumberInfo);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffGranted.PK, Env.CurrentUser.PK);

					var factoryForLoading2 = new BusinessObjectFactory();
					var personLoaded2 = factoryForLoading2.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					personLoaded2.Validation.ValidatePER_DriversLicenseNumber();
					AssertHasError("Should check security since access rights are enabled", personLoaded2.PER_DriversLicenseNumberInfo, "Please enter at least a driver's license number, email address, mobile phone number or passport number.");
				}
			}
		}

		#endregion

		#endregion

		public void TestPrimaryRelationshipDeleted()
		{
			var contact = GetContact();
			var person = GlbPerson.CreateFromContact(Factory, contact);
			Factory.Save();
			Assert(person.IsInDatabase);
			Assert(person.PrimaryRelationship.IsInDatabase);

			var otherContact = GetContact();
			contact.OC_PER = GlbPerson.CreateFromContact(Factory, otherContact).PK;
			person.Delete();

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNull(person.PrimaryRelationship);
			AssertNull(Factory.LoadTop1<GlbPersonPrimaryRelationship>(new ZQuery(GlbPersonPrimaryRelationshipSchema.PPR_PER, person.PK) { FetchOnlyFromLocalCache = false }));
		}

		public void TestPrimaryPosition_Contact()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var contact1 = GetContact();

			contact1.OC_PER = person.PK;
			contact1.OC_Title = "";

			Factory.Save();

			var contact2 = GetContact();
			var contact3 = GetContact();

			contact2.OC_PER = person.PK;
			contact2.OC_Title = "boss";
			contact2.OC_IsActive = false;

			contact3.OC_PER = person.PK;
			contact3.OC_Title = "secretary";

			var primaryRelationshipPivot = Factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationshipPivot.PPR_PER = person.PK;
			primaryRelationshipPivot.Primary = contact2;

			Factory.Save();

			AssertEquals("boss", person.PrimaryJobTitle);
		}

		public void TestPrimaryWorkingAddressStaff()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_PER = newPerson.PK;
			glbStaff.GS_Title = "developer";

			glbStaff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			glbStaff.HomeBranch.GB_RL_NKHomePort = "AUSYD";
			glbStaff.HomeBranch.GB_City = "Sydney";
			glbStaff.HomeBranch.GB_State = "NSW";

			newPerson.SetPrimaryRelationship(glbStaff);

			AssertEquals("Address details should be from the branch.", glbStaff.HomeBranch.GB_City, newPerson.PrimarySource.City);
			AssertEquals("Address details should be from the branch.", glbStaff.HomeBranch.GB_State, newPerson.PrimarySource.State);
			AssertEquals("Address details should be from the branch.", glbStaff.HomeBranch.Country.RN_Desc, newPerson.PrimarySource.Country);
			AssertEquals("Address details should be from the branch.", glbStaff.HomeBranch.GB_RL_NKHomePort, newPerson.PrimarySource.UNLOCO);
		}

		public void TestPrimaryWorkingAddressStaffNoHomeBranch()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_PER = newPerson.PK;
			glbStaff.GS_Title = "developer";
			glbStaff.ResetBranchAndDepartment();

			glbStaff.GS_GB_LastLogonBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			glbStaff.LastLogonBranch.GB_RL_NKHomePort = "AUSYD";
			glbStaff.LastLogonBranch.GB_City = "Sydney";
			glbStaff.LastLogonBranch.GB_State = "NSW";

			newPerson.SetPrimaryRelationship(glbStaff);

			AssertEquals("Address details should be from the branch.", glbStaff.LastLogonBranch.City, newPerson.PrimarySource.City);
			AssertEquals("Address details should be from the branch.", glbStaff.LastLogonBranch.GB_State, newPerson.PrimarySource.State);
			AssertEquals("Address details should be from the branch.", glbStaff.LastLogonBranch.Country.RN_Desc, newPerson.PrimarySource.Country);
			AssertEquals("Address details should be from the branch.", glbStaff.LastLogonBranch.GB_RL_NKHomePort, newPerson.PrimarySource.UNLOCO);
		}

		public void TestPrimaryWorkingAddressContact()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var contact = GetContact();
			contact.OC_PER = newPerson.PK;
			contact.OC_Title = "manager";
			var mainAddress = contact.ParentOrg.MainAddress;
			mainAddress.OA_City = "Sydney";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			contact.OC_OA_OrgAddress = mainAddress.PK;

			newPerson.SetPrimaryRelationship(contact);

			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", contact.ParentOrg.MainAddress.City, newPerson.PrimarySource.City);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", contact.ParentOrg.MainAddress.OA_State, newPerson.PrimarySource.State);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", contact.ParentOrg.MainAddress.EffectiveRelatedPortCode.Country.RN_Desc, newPerson.PrimarySource.Country);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", contact.ParentOrg.MainAddress.OA_RL_NKRelatedPortCode, newPerson.PrimarySource.UNLOCO);
		}

		public void TestPrimaryWorkingAddressContactNoOC_OA_OrgAddress()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var contact = GetContact();
			contact.OC_PER = newPerson.PK;
			contact.OC_Title = "manager";

			contact.ParentOrg.OH_RL_NKClosestPort = "AUSYD";

			AssertEquals(1, contact.ParentOrg.Addresses.Count);
			var address1 = contact.ParentOrg.Addresses.Cast<OrgAddress>().FirstOrDefault();
			AssertEquals("AUSYD", address1.OA_RL_NKRelatedPortCode);

			var address2 = contact.ParentOrg.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "AUBNE";

			newPerson.SetPrimaryRelationship(contact);

			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", address1.City, newPerson.PrimarySource.City);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", address1.OA_State, newPerson.PrimarySource.State);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", address1.EffectiveRelatedPortCode?.Country?.RN_Desc, newPerson.PrimarySource.Country);
			AssertEquals("Address details should be populated from the contact's OC_OA_OrgAddress.", address1.OA_RL_NKRelatedPortCode, newPerson.PrimarySource.UNLOCO);
		}

		public void TestCompanyNameWithPrimaryContact()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var contact = GetContact();
			contact.OC_PER = newPerson.PK;
			contact.OC_Title = "manager";

			contact.ParentOrg.OH_RL_NKClosestPort = "AUSYD";
			contact.ParentOrg.OH_FullName = "TARS";

			AssertEquals(1, contact.ParentOrg.Addresses.Count);
			var address1 = contact.ParentOrg.Addresses.Cast<OrgAddress>().FirstOrDefault();
			AssertEquals("AUSYD", address1.OA_RL_NKRelatedPortCode);
			address1.OA_CompanyNameOverride = "CMON TARS";

			newPerson.SetPrimaryRelationship(contact);

			AssertEquals("Company name should be retrieved from contact primary working address.", address1.OA_CompanyNameOverride, newPerson.CompanyName);
		}

		public void TestCompanyNameWithPrimaryStaff()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var nonPrimaryGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			nonPrimaryGlbStaff.GS_PER = newPerson.PK;
			nonPrimaryGlbStaff.GS_Title = "nobody";

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_PER = newPerson.PK;
			glbStaff.GS_Title = "developer";

			glbStaff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			glbStaff.HomeBranch.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			glbStaff.HomeBranch.Company.GC_Name = "Matthew McConaughey";

			newPerson.SetPrimaryRelationship(glbStaff);

			AssertEquals("OrgAddress should be populated from the first address matching the OH_RL_NKClosestPort of staff branch org proxy.", glbStaff.HomeBranch.Company.GC_Name, newPerson.CompanyName);
		}

		public void TestPrimaryWorkplaceCode_Org()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var contact = GetContact();
			contact.OC_PER = newPerson.PK;
			contact.OC_Title = "manager";

			contact.ParentOrg.OH_RL_NKClosestPort = "AUSYD";
			contact.ParentOrg.OH_FullName = "TARS";

			AssertEquals(1, contact.ParentOrg.Addresses.Count);
			var address1 = contact.ParentOrg.Addresses.Cast<OrgAddress>().FirstOrDefault();
			AssertEquals("AUSYD", address1.OA_RL_NKRelatedPortCode);
			address1.OA_CompanyNameOverride = "CMON TARS";

			newPerson.SetPrimaryRelationship(contact);

			AssertEquals("Org Code should be retrieved from the parent org", contact.ParentOrg.OH_Code, newPerson.PrimaryWorkplaceCode);
		}

		public void TestPrimaryWorkplaceCode_Staff()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();

			var nonPrimaryGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			nonPrimaryGlbStaff.GS_PER = newPerson.PK;
			nonPrimaryGlbStaff.GS_Title = "nobody";

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_PER = newPerson.PK;
			glbStaff.GS_Title = "developer";

			glbStaff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			glbStaff.HomeBranch.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			glbStaff.HomeBranch.Company.GC_Name = "Matthew McConaughey";

			newPerson.SetPrimaryRelationship(glbStaff);

			AssertEquals("Code should be populated from the staff code.", glbStaff.GS_Code, newPerson.PrimaryWorkplaceCode);
		}

		public void TestPersonFromApplicantWithRelatedContact()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = GetContact();

			contact.OC_PER = person.PK;

			var collection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IHRJobApplicantCollection>(), Factory, ZQuery.NoResultQuery);
			var applicant = collection.AddNew();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = applicant.PK;
				log.SL_Table = HRJobApplicantSchema.Constants.TableName;
				log.SL_Reference = $"Related Contact: (blabla) | ID: {contact.PK}";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}

			Factory.Save();

			AssertEquals(person.PK, applicant[HRJobApplicantSchema.HA_PER]);
		}

		public void TestContactsCollection()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();

			var contact1 = GetContact();
			var contact2 = GetContact();
			var contact3 = GetContact();
			var contact4 = GetContact();
			var contact5 = GetContact();

			contact1.OC_PER = person1.PK;
			contact2.OC_PER = person1.PK;
			contact3.OC_PER = person1.PK;
			contact4.OC_PER = person2.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { contact1, contact2, contact3 }, person1.ContactCollection);
			AssertContainsExactElementsInAnyOrder(new[] { contact4 }, person2.ContactCollection);
		}

		public void TestApplicantsCollection()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();

			var collection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IHRJobApplicantCollection>(), Factory, ZQuery.NoResultQuery);

			var applicant1 = collection.AddNew() as IHRJobApplicant;
			var applicant2 = collection.AddNew() as IHRJobApplicant;
			var applicant3 = collection.AddNew() as IHRJobApplicant;
			var applicant4 = collection.AddNew() as IHRJobApplicant;
			var applicant5 = collection.AddNew() as IHRJobApplicant;

			applicant1.HA_PER = person1.PK;
			applicant2.HA_PER = person1.PK;
			applicant3.HA_PER = person1.PK;
			applicant4.HA_PER = person2.PK;

			applicant1.HA_FullName = "name1";
			applicant2.HA_FullName = "name2";
			applicant3.HA_FullName = "name3";
			applicant4.HA_FullName = "name4";
			applicant5.HA_FullName = "name5";

			applicant1.HA_EmailAddress = "email1@gmail.com";
			applicant2.HA_EmailAddress = "email2@gmail.com";
			applicant3.HA_EmailAddress = "email3@gmail.com";
			applicant4.HA_EmailAddress = "email4@gmail.com";
			applicant5.HA_EmailAddress = "email5@gmail.com";

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2, applicant3 }, person1.ApplicantCollection);
			AssertContainsExactElementsInAnyOrder(new[] { applicant4 }, person2.ApplicantCollection);
		}

		public void TestStaffCollection()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();

			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff4 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff5 = Factory.NewWithValidTestData<GlbStaff>();

			glbStaff1.GS_PER = person1.PK;
			glbStaff2.GS_PER = person1.PK;
			glbStaff3.GS_PER = person1.PK;
			glbStaff4.GS_PER = person2.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { glbStaff1, glbStaff2, glbStaff3 }, person1.StaffCollection);
			AssertContainsExactElementsInAnyOrder(new[] { glbStaff4 }, person2.StaffCollection);
		}

		public void TestGetGlbStaffCollection()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff1.GS_PER = person.PK;
			glbStaff1.GS_Code = "BGS";
			glbStaff1.GS_LoginName = "login1";
			Factory.Save();

			AssertEquals("Person has 1 staff", 1, person.StaffCollection.Count);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var staff2 = newFactory.NewWithValidTestData<GlbStaff>();
			staff2.GS_PER = person.PK;
			glbStaff1.GS_Code = "ASR";
			glbStaff1.GS_LoginName = "login2";
			newFactory.Save();

			person.ReloadGlbStaffCollectionFromDb();

			AssertEquals("Person has 2 staff", 2, person.StaffCollection.Count);
		}

		public void TestDeduplicationStartedEventFiresWhenFindingDuplicates()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventStarted = false;
				var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				glbPerson.DeduplicationStarted += (o, e) => eventStarted = true;

				glbPerson.FindDuplicates(null);
				AssertEquals(true, eventStarted);
			}
		}

		public void TestDeduplicationNotStartedWhenPersonContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			var eventStarted = false;
			EventHandler dedupeAction = new EventHandler((o, e) =>
			{
				eventStarted = true;
			});

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				glbPerson.PER_FullName = "ABC";

				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				glbPerson.DeduplicationStarted += dedupeAction;

				glbPerson.PER_FullName = "";
				AssertEquals(false, eventStarted);

				glbPerson.PER_FullName = "ABC";
				AssertEquals(true, eventStarted);
				glbPerson.DeduplicationStarted -= dedupeAction;
			}
		}

		public void TestDeduplicationActionOccurredEvent()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				DeduplicationAction actionInvoked = DeduplicationAction.None;
				GlbPerson org = Factory.NewWithValidTestData<GlbPerson>();
				org.DeduplicationActionOccurred += (o, e) => actionInvoked = e.InvokedAction;

				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.None, actionInvoked);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.Ignore, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Link, null, null, null);
				AssertEquals(DeduplicationAction.Link, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Merge, null, null, null);
				AssertEquals(DeduplicationAction.Merge, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenMaster, null, null, null);
				AssertEquals(DeduplicationAction.OpenMaster, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenTarget, null, null, null);
				AssertEquals(DeduplicationAction.OpenTarget, actionInvoked);
			}
		}

		public void TestPropagateDeduplicationActionOccurred_ExcludingsAreSet()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var excludingInactive = false;
				var excludingOtherCountry = false;
				GlbPerson org = Factory.NewWithValidTestData<GlbPerson>();
				org.DeduplicationActionOccurred += (o, e) =>
				{
					excludingInactive = e.IsExcludingInactiveFromResults;
					excludingOtherCountry = e.IsExcludingOtherCountriesFromResults;
				};

				Assert("PersonsEnableDuplicateDetection is false.", !SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				Assert(!excludingInactive);
				Assert(!excludingOtherCountry);

				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null, true, true);
				Assert(!excludingInactive);
				Assert(!excludingOtherCountry);

				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				Assert(!excludingInactive);
				Assert(!excludingOtherCountry);

				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null, true, true);
				Assert(excludingInactive);
				Assert(excludingOtherCountry);
			}
		}

		public void TestShouldSetIsDeduplicationPropagatedToTrueWhenDeduplicationStarted()
		{
			//Arrange
			var isDeduplicationStarted = false;
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			glbPerson.DeduplicationStarted += (o, e) => { isDeduplicationStarted = true; };
			((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition, IsDeduplicationPropagated flag for testContact is false by default", false, ((IDeduplicatable)glbPerson).IsDeduplicationStarted);

				//Act
				glbPerson.PropagateDeduplicationStarted();

				//Assert
				AssertEquals("Precondition, should fire DeduplicationStarted event", true, isDeduplicationStarted);
				AssertEquals("IsDeduplicationPropagated flag for testContact is true", true, ((IDeduplicatable)glbPerson).IsDeduplicationStarted);
			}
		}

		public void TestResetValidationStatusWhenCurrentCompanyIsNull()
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var person = Factory.New<GlbPerson>();
				AssertNoExceptionThrown(() => person.ResetValidationStatus(null));
			}
		}

		public void TestInitFromContact()
		{
			var contact = GetContact();
			var person = GlbPerson.CreateFromContact(Factory, contact);

			AssertContactPerson(contact, person);
			AssertEquals(true, person.IsCreatedFromStaffOrContactOrApplicant);
			AssertEquals(contact, person.PrimaryRelationship.Primary);
		}

		public void TestUpdateFromContact()
		{
			var contact = GetContact();
			var person = Factory.New<GlbPerson>();
			contact.OC_PER = person.PK;
			person.UpdateFromContact(contact);

			AssertContactPerson(contact, person);
			AssertEquals(false, person.IsCreatedFromStaffOrContactOrApplicant);
			AssertEquals("Contact is added to collection", 1, person.ContactCollection.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Manually insert changes to simulate changes outside CW1")]
		public void TestUpdateFromContact_ForceUpdate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var originalContactName1 = "contact1";
			var originalContactName2 = "contact2";

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = originalContactName1;
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = originalContactName2;
			contact2.OC_Email = "email2@testing.com";
			contact1.OC_Gender = "M";
			contact2.OC_Gender = "M";
			contact2.OC_Birthday = ZDateTime.BrettsBirthday;
			contact2.OC_HomePhone = "123456";
			contact2.OC_RN_NKNationality = "AU";
			contact2.OC_PersonalInfo = "personal";
			Factory.Save();

			AssertEquals("Precondition", contact1.OC_ContactName, contact1.Person.PER_FullName);
			AssertEquals("Precondition", contact2.OC_ContactName, contact2.Person.PER_FullName);
			AssertEquals("Precondition", contact2.OC_Gender, contact2.Person.PER_Gender);
			AssertEquals("Precondition", contact2.OC_BirthdayInternal, contact2.Person.PER_BirthDate);
			AssertEquals("Precondition", contact2.OC_HomePhone, contact2.Person.PER_HomePhone);
			AssertEquals("Precondition", contact2.OC_RN_NKNationality, contact2.Person.PER_RN_NKNationalityCodeISO);
			AssertEquals("Precondition", contact2.OC_PersonalInfoInternal, contact2.Person.PER_PersonalInfo);

			var updatedContactName1 = "extra1";
			var updatedContactName2 = "extra2";

			var sqlText1 =
$@"UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName1}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact1PK;

UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName2}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
,{OrgContactSchema.Constants.OC_Birthday} = '2000-01-01'
,{OrgContactSchema.Constants.OC_HomePhone} = '654321'
,{OrgContactSchema.Constants.OC_RN_NKNationality} = 'NZ'
,{OrgContactSchema.Constants.OC_PersonalInfo} = 'public'
WHERE {OrgContactSchema.Constants.PK} = @contact2PK;
";

			using (var cmd = Db.Connection.Command(sqlText1))
			{
				cmd.AddParameter("@contact1PK", SqlDbType.UniqueIdentifier, contact1.PK.ToGuid());
				cmd.AddParameter("@contact2PK", SqlDbType.UniqueIdentifier, contact2.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Precondition", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Precondition", originalContactName2, contact2.Person.PER_FullName);
			AssertEquals("Precondition", updatedContactName1, contact1.OC_ContactName);
			AssertEquals("Precondition", updatedContactName2, contact2.OC_ContactName);
			AssertEquals("Precondition", "M", contact1.Person.PER_Gender);
			AssertEquals("Precondition", "M", contact2.Person.PER_Gender);
			AssertEquals("Precondition", "F", contact1.OC_Gender);
			AssertEquals("Precondition", "F", contact2.OC_Gender);

			contact1.Person.UpdateFromContact(contact1);
			contact2.Person.UpdateFromContact(contact2, forceUpdate: true);

			AssertEquals("Should remain unchanged", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Should be updated", updatedContactName2, contact2.Person.PER_FullName);
			AssertEquals("Should remain unchanged", "M", contact1.Person.PER_Gender);
			AssertEquals("Should be updated", "F", contact2.Person.PER_Gender);
			AssertEquals("Should be updated", new ZDate(2000, 01, 01), contact2.Person.PER_BirthDate);
			AssertEquals("Should be updated", "654321", contact2.Person.PER_HomePhone);
			AssertEquals("Should be updated", "NZ", contact2.Person.PER_RN_NKNationalityCodeISO);
			AssertEquals("Should be updated", "public", contact2.Person.PER_PersonalInfo);
		}

		void AssertContactPerson(OrgContact contact, GlbPerson person)
		{
			AssertEquals(contact.OC_Birthday, person.PER_BirthDate);
			AssertEquals("", person.PER_City);
			AssertEquals("", person.PER_EmailAddress);
			AssertEquals("", person.PER_FaxNumber);
			AssertEquals(contact.OC_ContactName, person.PER_FullName);
			AssertEquals(contact.OC_Gender, person.PER_Gender);
			AssertEquals(contact.OC_HomePhone, person.PER_HomePhone);
			AssertEquals("", person.PER_Passport);
			AssertEquals("", person.PER_DriversLicenseNumber);
			AssertEquals("", person.PER_Postcode);
			AssertEquals("", person.PER_RN_NKCountry);
			AssertEquals("AU", person.PER_RN_NKNationalityCodeISO);
			AssertEquals("", person.PER_State);
			AssertEquals("", person.PER_NameTitle);
			AssertEquals("", person.PER_HomeAddress1);
			AssertEquals("", person.PER_HomeAddress2);
			AssertEquals(contact.OC_PersonalInfo, person.PER_PersonalInfo);
			AssertEquals(Array.Empty<byte>(), person.PER_Picture);
			AssertEquals("", person.PER_PreferredLanguage);
		}

		public void TestUpdateFromContact_MobileOnlyUpdatedIfSame()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var person = Factory.Load<GlbPerson>(contact.OC_PER);
			Factory.Save();

			var personMobile = "123456789";
			person.PER_MobilePhone = personMobile;
			person.ShouldUpdateMobileOnRelatedRecords = false;
			Factory.Save();
			var contactMobile = "999999999";
			contact.OC_Mobile = contactMobile;
			Factory.Save();
			AssertEquals("Person mobile should not change as it is different to old contact mobile", personMobile, person.PER_MobilePhone);

			person.PER_MobilePhone = contactMobile;
			Factory.Save();
			var newContactMobile = "000000000";
			contact.OC_Mobile = newContactMobile;
			Factory.Save();
			AssertEquals("Person mobile should change as it is the same as old contact mobile", newContactMobile, person.PER_MobilePhone);
		}

		public void TestUpdateFromContactWithoutSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shouldnotchange@test.com";
			contact.OC_PersonalInfo = "personal";
			contact.OC_Gender = "F";
			contact.OC_RN_NKNationality = "AU";
			contact.OC_HomePhone = "94111111";
			contact.OC_Birthday = new ZDateTime(2019, 01, 01);
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					Env.Security.PersonIntelligenceView.IsAllowed = false;
					Env.Security.PersonIntelligenceEdit.IsAllowed = false;
					Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;
					Env.Security.OrgContactViewMobileNumber.IsAllowed = false;
					Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = false;

					var newFactory = new BusinessObjectFactory();
					contact = newFactory.Load<OrgContact>(contact.PK);

					contact.OC_PersonalInfo = "new information";
					contact.OC_Gender = "M";
					contact.OC_RN_NKNationality = "US";
					contact.OC_HomePhone = "94222222";
					contact.OC_Birthday = new ZDateTime(2019, 02, 02);

					AssertEquals("new information", contact.OC_PersonalInfoInternal);
					AssertEquals("M", contact.OC_GenderInternal);
					AssertEquals("US", contact.OC_RN_NKNationalityInternal);
					AssertEquals(new ZDateTime(2019, 02, 02), contact.OC_BirthdayInternal);
					AssertEquals("94222222", (ZString)contact.OC_HomePhoneInfo.Value);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_PersonalInfo);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_Gender);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_RN_NKNationality);
					AssertEquals(ZDateTime.Empty, contact.OC_Birthday);
					AssertEquals(contact.ViewDeniedMessage, contact.OC_HomePhone_Formatted);

					var contactPerson = contact.Person;
					contactPerson.UpdateFromContact(contact);

					AssertEquals("new information", contactPerson.PER_PersonalInfoInternal);
					AssertEquals("M", contactPerson.PER_GenderInternal);
					AssertEquals("US", contactPerson.PER_RN_NKNationalityCodeISOInternal);
					AssertEquals("94222222", contactPerson.PER_HomePhoneInternal);
					AssertEquals(new ZDateTime(2019, 02, 02), contactPerson.PER_BirthDateInternal);
				}
			}
		}

		OrgContact GetContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "addr1";
			org.MainAddress.Address2 = "addr2";
			org.MainAddress.City = "city";
			org.MainAddress.State = "NSW";
			org.MainAddress.OA_RN_NKCountryCode = "NZ";
			org.MainAddress.OA_PostCode = "1234";

			var contact = org.Contacts.AddNew();
			contact.OC_Birthday = ZDateTime.BrettsBirthday;
			contact.OC_ContactName = "name";
			contact.OC_Email = "email@example.com";
			contact.OC_Fax = "123";
			contact.OC_Gender = "M";
			contact.OC_HomePhone = "123456";
			contact.OC_Mobile = "654321";
			contact.OC_PersonalInfo = "personal";
			contact.OC_Phone = "99999";
			contact.OC_ProfilePhoto = new ZBlob(new byte[] { 1, 2, 3 });
			contact.OC_Title = "master";
			contact.OC_RN_NKNationality = "AU";
			contact.OC_Language = Core.SharedConstants.Languages.Russian;

			var cert = contact.Certificates.AddNew();
			cert.XZ_Type = StaffCertificateType.PAS;
			cert.XZ_RefNumber = "passport";
			cert.XZ_RN_NKCountryOfIssuance = "GB";
			cert.XZ_ExpiryOrDueDate = ZDateTime.BrettsBirthday;

			cert = contact.Certificates.AddNew();
			cert.XZ_Type = StaffCertificateType.NID;
			cert.XZ_RefNumber = "drivers";

			return contact;
		}

		public void TestInitFromApplicant()
		{
			var applicant = GetApplicant();
			var person = GlbPerson.CreateFromApplicant(Factory, applicant);

			AssertEquals(applicant.HA_EmailAddress, person.PER_EmailAddress);
			AssertEquals(true, person.IsCreatedFromStaffOrContactOrApplicant);
		}

		public void TestUpdateFromApplicant()
		{
			var applicant = GetApplicant();
			var person = Factory.New<GlbPerson>();
			person.UpdateFromApplicant(applicant);

			AssertEquals(applicant.HA_EmailAddress, person.PER_EmailAddress);
			AssertEquals(false, person.IsCreatedFromStaffOrContactOrApplicant);
		}

		public void TestCreateFromApplicant_LearningCentreMatchContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			Factory.Save();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = ZGuid.Empty;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = applicant.PK;
				log.SL_Table = HRJobApplicantSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = AutoEvents.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Reference = "Related Contact: Confire (blabla) | ID: " + contact.PK;
			}

			AssertEquals("Precondition: Applicant not associated with person yet", true, applicant.HA_PER.IsEmpty);

			var reloadedPerson = GlbPerson.CreateFromApplicant(Factory, applicant);
			AssertEquals("Should have found person from related contact", person.PK, reloadedPerson.PK);
			AssertEquals("Applicant should have been attached to the person", reloadedPerson.PK, applicant.HA_PER);
			AssertEquals("Applicant should have been attached to the person", 1, reloadedPerson.ApplicantCollection.Count);
		}

		public void TestCreateFromApplicant_LearningCentreMatchStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			staff.GS_PER = person.PK;
			Factory.Save();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = ZGuid.Empty;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = applicant.PK;
				log.SL_Table = HRJobApplicantSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = AutoEvents.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Reference = "Related Staff: Confire (blabla) | ID: " + staff.PK;
			}

			AssertEquals("Precondition: Applicant not associated with person yet", true, applicant.HA_PER.IsEmpty);

			var reloadedPerson = GlbPerson.CreateFromApplicant(Factory, applicant);
			AssertEquals("Should have found person from related staff", person.PK, reloadedPerson.PK);
			AssertEquals("Applicant should have been attached to the person", reloadedPerson.PK, applicant.HA_PER);
			AssertEquals("Applicant should have been attached to the person", 1, reloadedPerson.ApplicantCollection.Count);
		}

		public void TestCreateFromApplicant_DetachApplicant()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			Factory.Save();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = person.PK;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = applicant.PK;
				log.SL_Table = HRJobApplicantSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = AutoEvents.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Reference = "Related Contact: Confire (blabla) | ID: " + contact.PK;
			}
			Factory.Save();

			AssertEquals(1, person.ApplicantCollection.Count);
			AssertEquals("Applicant should have a Person", applicant.HA_PER, person.PK);

			var newPerson = GlbPerson.DetachAndCreateFromApplicant(Factory, applicant);
			Assert("Should create a new applicant", !newPerson.IsInDatabase);
			AssertEquals("Applicant should be set to the new Person", applicant.HA_PER, newPerson.PK);
		}

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			GlbPerson glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			IWorkflowProvider workflowProvider = glbPerson;

			AssertNotNull(workflowProvider);
			AssertEquals(glbPerson.PK, workflowProvider.PK);
			AssertEquals(new GlbPersonWorkflowDescriptor().Code, workflowProvider.WorkflowType);
			AssertEquals(typeof(GlbPersonProcessTaskCollection), workflowProvider.WorkflowItems.GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		#endregion

		IHRJobApplicant GetApplicant()
		{
			var applicant = Factory.New<IHRJobApplicant>();

			applicant.HA_Birthdate = ZDateTime.BrettsBirthday.Date;
			applicant.HA_City = "city";
			applicant.HA_DriversLicenseNumber = "123";
			applicant.HA_EmailAddress = "address@example.com";
			applicant.HA_FaxNum = "456";
			applicant.HA_FullName = "Antonio";
			applicant.HA_Gender = "M";
			applicant.HA_HomePhone = "123123123";
			applicant.HA_MobilePhone = "456789";
			applicant.HA_NameSuffix = "--";
			applicant.HA_Passport = "CO422448";
			applicant.HA_Postcode = "2228";
			applicant.HA_RN_NKCountry = "AU";
			applicant.HA_RN_NKNationalityCodeISO = "UA";
			applicant.HA_State = "NSW";
			applicant.HA_Title = "Don";
			applicant.HA_UserAddress1 = "addr1";
			applicant.HA_UserAddress2 = "addr2";

			return applicant;
		}

		public void TestInitFromStaff()
		{
			var staff = GetStaff();
			var person = GlbPerson.CreateFromStaff(Factory, staff);

			AssertStaffPerson(staff, person);
			AssertEquals(true, person.IsCreatedFromStaffOrContactOrApplicant);
			AssertEquals(staff, person.PrimaryRelationship.Primary);
		}

		public void TestUpdateFromStaff()
		{
			var staff = GetStaff();
			var person = Factory.New<GlbPerson>();
			person.UpdateFromStaff(staff);

			Factory.Save();

			AssertStaffPerson(staff, person);
			AssertEquals(false, person.IsCreatedFromStaffOrContactOrApplicant);

			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(newUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.Security.StaffViewHomeAddressDetails.IsAllowed = false;
				person.UpdateFromStaff(staff);
			}
			AssertStaffPerson(staff, person);
		}

		void AssertStaffPerson(GlbStaff staff, GlbPerson person)
		{
			AssertEquals(staff.GS_Birthdate, person.PER_BirthDate);
			AssertEquals("city", person.PER_City);
			AssertEquals("", person.PER_EmailAddress);
			AssertEquals(staff.GS_FaxNum, person.PER_FaxNumber);
			AssertEquals(staff.GS_FullName, person.PER_FullName);
			AssertEquals(staff.GS_Gender, person.PER_Gender);
			AssertEquals(staff.GS_HomePhone, person.PER_HomePhone);
			AssertEquals("passport", person.PER_Passport);
			AssertEquals("drivers", person.PER_DriversLicenseNumber);
			AssertEquals("1234", person.PER_Postcode);
			AssertEquals("NZ", person.PER_RN_NKCountry);
			AssertEquals("AU", person.PER_RN_NKNationalityCodeISO);
			AssertEquals("NSW", person.PER_State);
			AssertEquals("master", person.PER_NameTitle);
			AssertEquals("addr1", person.PER_HomeAddress1);
			AssertEquals("addr2", person.PER_HomeAddress2);
			AssertEquals(staff.GS_ProfilePhoto, person.PER_Picture);
			AssertEquals(Core.SharedConstants.Languages.Russian, person.PER_PreferredLanguage);
			AssertEquals("friendly", person.PER_FriendlyName);
		}

		public void TestUpdateFromStaff_MobileOnlyUpdatedIfSame()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var person = Factory.Load<GlbPerson>(staff.GS_PER);
			Factory.Save();

			var personMobile = "123456789";
			person.PER_MobilePhone = personMobile;
			person.ShouldUpdateMobileOnRelatedRecords = false;
			Factory.Save();
			var staffMobile = "999999999";
			staff.GS_MobilePhone = staffMobile;
			Factory.Save();
			AssertEquals("Person mobile should not change as it is different to old contact mobile", personMobile, person.PER_MobilePhone);

			person.PER_MobilePhone = staffMobile;
			Factory.Save();
			var newStaffMobile = "000000000";
			staff.GS_MobilePhone = newStaffMobile;
			staff.Factory.Save();
			AssertEquals("Person mobile should change as it is the same as old contact mobile", newStaffMobile, person.PER_MobilePhone);
		}

		GlbStaff GetStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Birthdate = ZDateTime.BrettsBirthday.Date;
			staff.GS_FullName = "name";
			staff.GS_EmailAddress = "email@example.com";
			staff.GS_FaxNum = "123";
			staff.GS_Gender = "M";
			staff.GS_HomePhone = "123456";
			staff.GS_MobilePhone = "654321";
			staff.GS_ProfilePhoto = new ZBlob(new byte[] { 1, 2, 3 });
			staff.GS_NameTitle = "master";
			staff.GS_Title = "title";
			staff.GS_RN_NKNationalityCode = "AU";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.Russian;
			staff.GS_FriendlyName = "friendly";
			staff.Address1 = "addr1";
			staff.Address2 = "addr2";
			staff.City = "city";
			staff.State = "NSW";
			staff.GS_RN_NKCountryCode = "NZ";
			staff.Postcode = "1234";

			var cert = staff.Certificates.AddNew();
			cert.XZ_Type = StaffCertificateType.PAS;
			cert.XZ_RefNumber = "passport";
			cert.XZ_RN_NKCountryOfIssuance = "GB";
			cert.XZ_ExpiryOrDueDate = ZDateTime.BrettsBirthday;

			cert = staff.Certificates.AddNew();
			cert.XZ_Type = StaffCertificateType.NID;
			cert.XZ_RefNumber = "drivers";

			return staff;
		}

		public void TestShouldUpdateMobileOnRelatedRecords()
		{
			var initialMobile = "000000000";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_MobilePhone = initialMobile;

			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_IsActive = false;
			staff.GS_MobilePhone = initialMobile;

			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			contact.OC_Mobile = initialMobile;

			Factory.Save();

			var firstMobile = "111111111";
			person.PER_MobilePhone = firstMobile;
			person.ShouldUpdateMobileOnRelatedRecords = true;
			Factory.Save();
			AssertEquals(firstMobile, contact.OC_Mobile);
			AssertEquals(firstMobile, staff.GS_MobilePhone);

			var changedMobile = "999999999";
			person.PER_MobilePhone = changedMobile;
			person.ShouldUpdateMobileOnRelatedRecords = false;
			Factory.Save();
			AssertEquals(firstMobile, contact.OC_Mobile);
			AssertEquals(firstMobile, staff.GS_MobilePhone);

			person.PER_MobilePhone = firstMobile;
			Factory.Save();
			var secondMobile = "222222222";
			staff.GS_MobilePhone = secondMobile;
			person.ShouldUpdateMobileOnRelatedRecords = true;
			Factory.Save();
			AssertEquals(secondMobile, person.PER_MobilePhone);
			AssertEquals(secondMobile, contact.OC_Mobile);

			staff.GS_MobilePhone = changedMobile;
			person.ShouldUpdateMobileOnRelatedRecords = false;
			Factory.Save();
			AssertEquals(changedMobile, person.PER_MobilePhone);
			AssertEquals(secondMobile, contact.OC_Mobile);
		}

		public void TestMobileExistsOnMultipleRelatedRecords()
		{
			var initialMobile = "000000000";
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;

			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			staff.GS_MobilePhone = initialMobile;
			contact.OC_Mobile = initialMobile;
			Factory.Save();
			AssertEquals("multiple values should be initialMobile", true, person.MobileExistsOnMultipleRelatedRecords(initialMobile, "anotherValue"));

			var differentMobile = "999999999";
			person.ShouldUpdateMobileOnRelatedRecords = false;
			staff.GS_MobilePhone = differentMobile;
			contact.OC_Mobile = differentMobile;
			Factory.Save();
			AssertEquals("No values should be initialMobile", false, person.MobileExistsOnMultipleRelatedRecords("anotherValue", initialMobile));
			AssertEquals("multiple values should be differentMobile", true, person.MobileExistsOnMultipleRelatedRecords("anotherValue", differentMobile));

			person.ShouldUpdateMobileOnRelatedRecords = false;
			staff.GS_MobilePhone = initialMobile;
			Factory.Save();
			AssertEquals("Only 1 value should be initialMobile", false, person.MobileExistsOnMultipleRelatedRecords("anotherValue", initialMobile));
			AssertEquals("Multiple values should be differentMobile - Applicant is no longer different to Person", false, person.MobileExistsOnMultipleRelatedRecords("anotherValue", differentMobile));

			person.ShouldUpdateMobileOnRelatedRecords = false;
			staff.GS_MobilePhone = differentMobile;
			contact.OC_Mobile = initialMobile;
			Factory.Save();
			AssertEquals("Only 1 value should be initialMobile", false, person.MobileExistsOnMultipleRelatedRecords(initialMobile, "anotherValue"));
			AssertEquals("Multiple values should be differentMobile", false, person.MobileExistsOnMultipleRelatedRecords(differentMobile, "anotherValue"));

			person.ShouldUpdateMobileOnRelatedRecords = false;
			contact.OC_Mobile = differentMobile;
			Factory.Save();
			AssertEquals("Only 1 value should be initialMobile", false, person.MobileExistsOnMultipleRelatedRecords(initialMobile, "anotherValue"));
			AssertEquals("Multiple values should be differentMobile", true, person.MobileExistsOnMultipleRelatedRecords(differentMobile, "anotherValue"));
		}

		#region Address Validation

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			person.PER_HomeAddress1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, person.PER_ValidationStatus);

			person.PER_HomeAddress2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, person.PER_ValidationStatus);

			person.PER_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, person.PER_ValidationStatus);

			person.PER_Postcode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, person.PER_ValidationStatus);

			person.PER_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, person.PER_ValidationStatus);

			person.PER_RN_NKCountry = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, person.PER_ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<GlbPerson>() as ISupportWebAddressValidation;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += Address_AddressValidationStatusChanged;
			address.ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.Addressee);
		}

		void Address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((GlbPerson)sender).PER_FullName = "It happened";
		}

		public void TestIgnoreContact()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				var target = Factory.NewWithValidTestData<OrgContact>();
				var personForTest = contact.Person;
				var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
				pmt.PMT_MasterPK = personForTest.PK;
				pmt.PMT_TargetPK = target.Person.PK;
				pmt.PMT_MasterTableCode = pmt.PMT_TargetTableCode = contact.Person.TablePrefix;
				pmt.PMT_Status = "TIG";
				pmt.PMT_GS_NKExcludeBy = "E";

				Factory.Save();

				((IDeduplicatable)personForTest).ShouldRunDeduplication = true;

				AssertEquals("target should be ignored", DuplicationResponseMessages.TIGExisted, ObjectFactory.Get<IMasterDataProvider>().GetPersonDuplicationFinderMessageForTest(contact, target));
			}
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<GlbPerson>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.PER_RN_NKCountry = "CN";
			Assert(address.NeedValidation);

			address.PER_RN_NKCountry = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbPerson>();
			address.PER_RN_NKCountry = "AU";
			AssertAddressMap(address, address.PER_HomeAddress1Info);
			AssertAddressMap(address, address.PER_HomeAddress2Info);
			AssertAddressMap(address, address.PER_CityInfo);
			AssertAddressMap(address, address.PER_PostcodeInfo);
			AssertAddressMap(address, address.PER_StateInfo);
			AssertAddressMap(address, address.PER_RN_NKCountryInfo);

			address.PER_RN_NKCountry = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForPerson: true));
			AssertAddressMap(address, address.PER_HomeAddress1Info);
			AssertAddressMap(address, address.PER_HomeAddress2Info);
			AssertAddressMap(address, address.PER_CityInfo);
			AssertAddressMap(address, address.PER_PostcodeInfo);
			AssertAddressMap(address, address.PER_StateInfo);
			AssertAddressMap(address, address.PER_RN_NKCountryInfo);
		}

		void AssertAddressMap(GlbPerson address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(GlbPerson.PER_RN_NKCountry) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.Person, Factory.New<GlbPerson>().ValidationSection);
		}

		#endregion

		#region PER_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilPersonIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "42 FOOBAR STREET";
			person.PER_HomeAddress2 = "FUNPLACE";
			person.PER_Postcode = "0000";
			person.PER_City = "WHITERUN";
			person.PER_State = "TAMRIEL";
			person.PER_RN_NKCountry = "ID";

			// Act.

			person.PER_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			person.PER_HomeAddress1 = "72 O'RIORDAN STREET";
			person.PER_HomeAddress2 = "WISETECH GLOBAL";
			person.PER_Postcode = "2015";
			person.PER_City = "ALEXANDRIA";
			person.PER_State = "NSW";
			person.PER_RN_NKCountry = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, person.PER_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "42 FOOBAR STREET";
			person.PER_HomeAddress2 = "FUNPLACE";
			person.PER_Postcode = "0000";
			person.PER_City = "WHITERUN";
			person.PER_State = "TAMRIEL";
			person.PER_RN_NKCountry = "ID";

			person.PER_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedPerson = new BusinessObjectFactory().Load<GlbPerson>(person.PK);

			// Act.

			reloadedPerson.PER_HomeAddress1 = "72 O'RIORDAN STREET";
			reloadedPerson.PER_HomeAddress2 = "WISETECH GLOBAL";
			reloadedPerson.PER_Postcode = "2015";
			reloadedPerson.PER_City = "ALEXANDRIA";
			reloadedPerson.PER_State = "NSW";
			reloadedPerson.PER_RN_NKCountry = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedPerson.PER_ValidationStatus);
		}

		#endregion

		#region Properties

		public void TestPER_Title()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_NameTitle = "Dr.";

			AssertEquals("Dr.", person.PER_NameTitle);
		}

		public void TestPER_Age()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_BirthDate = new ZDate(2004, 3, 3);
			ZInt expectedAge;
			if (ZDateTime.Now.DayOfYear < person.PER_BirthDate.DayOfYear)
			{
				expectedAge = ZDateTime.Now.Year - person.PER_BirthDate.Year - 1;
			}
			else
			{
				expectedAge = ZDateTime.Now.Year - person.PER_BirthDate.Year;
			}

			AssertEquals(expectedAge, person.PER_Age);
		}

		public void TestPER_BirthDateAndAge_Formatted()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_BirthDate = new ZDate(2004, 3, 3);

			AssertEquals($"03-Mar-04 ({person.PER_Age} yrs)", person.PER_BirthDateAndAge_Formatted);
		}

		public void TestGender()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_Gender = Genders.Man;
			AssertEquals(true, person.IsMale);
			AssertEquals(false, person.IsFemale);

			person.PER_Gender = Genders.Woman;
			AssertEquals(false, person.IsMale);
			AssertEquals(true, person.IsFemale);
		}

		public void TestGenderDefaultWhenSaving()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			AssertEquals("N", person.PER_Gender);
			Factory.Save();
		}

		public void TestGetSupportedDuplicationFinders()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var supportedFinders = person.GetSupportedDuplicationFinders(typeof(GlbPerson), new DeduplicationProxyConfig());

			AssertEquals(1, supportedFinders.Count);
			AssertNotNull(supportedFinders[0]);
		}

		#endregion

		#region Certificates

		public void TestGetAllLinkedCertificates()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var certificate1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var certificate2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();

			certificate1.XZ_ParentID = person.PK;
			certificate1.XZ_ParentTableCode = "PER";
			var certs = person.Certificates;
			AssertContainsExactElementsInAnyOrder("Person should have 1 certificate", new[] { certificate1 }, certs);

			certificate2.XZ_ParentID = person.PK;
			certificate2.XZ_ParentTableCode = "PER";
			AssertContainsExactElementsInAnyOrder("Person should have both certificates", new[] { certificate1, certificate2 }, person.Certificates);
		}

		public void TestCertificatesReturnsEmptyCollection()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			AssertEquals("Person should initially have no certificates", 0, person.Certificates.Count);
		}

		#endregion

		#region Patterns

		public void TestDeletePersonDeletesAllPatternsAndResults()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);
			var results = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, person);

			Factory.Save();
			person.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Person should be deleted", true, person.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Person patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Person pattern results should all be deleted", Array.Empty<BusinessObject>(), results.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeletePersonDoesNotDeletePatternsFromOtherPerson()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();

			var patterns1 = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person1);
			var patterns2 = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person2);

			Factory.Save();
			person1.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Person 1 should be deleted", true, person1.IsDeleted);
				AssertEquals("Person 2 should not deleted", false, person2.IsDeleted);

				AssertContainsExactElementsInAnyOrder("Person 1 patterns should all be deleted", Array.Empty<BusinessObject>(), patterns1.Where(p => !p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Person 2 patterns should not be deleted", Array.Empty<BusinessObject>(), patterns2.Where(p => p.IsDeleted));
			});
		}

		public void TestDeletePersonDoesNotDeleteChildCollections_ButDeletesChildPatterns()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var personPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			var staffPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, staff);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;
			var contactPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, contact);

			var applicant = person.ApplicantCollection.AddNew();
			((IHRJobApplicant)applicant).HA_PER = person.PK;
			var applicantPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, applicant);

			Factory.Save();
			person.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Person should be deleted", true, person.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Person patterns should all be deleted", Array.Empty<BusinessObject>(), personPatterns.Where(p => !p.IsDeleted));

				AssertEquals("Child staff should not be deleted", false, staff.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Staff patterns should all be deleted", Array.Empty<BusinessObject>(), staffPatterns.Where(p => !p.IsDeleted));

				AssertEquals("Child contact should not be deleted", false, contact.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Contact patterns should all be deleted", Array.Empty<BusinessObject>(), contactPatterns.Where(p => !p.IsDeleted));

				AssertEquals("Child applicant should not be deleted", false, applicant.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Applicant patterns should all be deleted", Array.Empty<BusinessObject>(), applicantPatterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestRemovedFromRecentItems_WhenPersonIsDeleted()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var moduleName = nameof(GlbPerson);
			var link = Factory.New<StmLink>();
			link.STL_ModuleID = moduleName;
			link.STL_ItemPK = person.PK;

			var linkWrapper = new LinkWrapper(link);
			RecentItemManager.Instance.AddOrUpdateRecentItems(moduleName, linkWrapper);
			RecentItemManager.Instance.AddOrUpdateRecentItems(string.Empty, linkWrapper);

			var isInModuleRecentItems = RecentItemManager.Instance.IsInRecentItems(moduleName, linkWrapper);
			var isInMainFormRecentItems = RecentItemManager.Instance.IsInRecentItems(string.Empty, linkWrapper);
			AssertEquals("Recent Items in Person Intelligence Module should show person", true, isInModuleRecentItems);
			AssertEquals("Recent Items in Main Form should show person", true, isInMainFormRecentItems);

			person.Delete();
			person.RemoveFromRecentItems();

			isInModuleRecentItems = RecentItemManager.Instance.IsInRecentItems(moduleName, linkWrapper);
			isInMainFormRecentItems = RecentItemManager.Instance.IsInRecentItems(string.Empty, linkWrapper);
			AssertEquals("Recent items in Person Intelligence Module should not show dissolved person", false, isInModuleRecentItems);
			AssertEquals("Recent items in Main Form should not show dissolved person", false, isInMainFormRecentItems);
		}

		#region Certificate Patterns

		public void TestDeletePersonDoesNotDeletCertificates_ButDeletesCertificatePatterns()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = person.PK;
			certificate.XZ_ParentTableCode = person.TablePrefix;

			Factory.Save();
			person.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Person should be deleted", true, person.IsDeleted);
				AssertEquals("Certificate should not be deleted", false, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeletePersonCertificatesDeletesAllCertificatesAndPatterns_ButDoesNotDeletePersonOrPersonPatterns()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var personPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, person);
			var personResults = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, person);

			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var certificatePatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			certificate.XZ_ParentID = person.PK;
			certificate.XZ_ParentTableCode = person.TablePrefix;

			Factory.Save();
			person.Certificates.DeleteAll();

			CombineAssertions(() =>
			{
				AssertEquals("Person should not be deleted", false, person.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Person patterns should not be deleted", Array.Empty<BusinessObject>(), personPatterns.Where(p => p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("No person results should be deleted", Array.Empty<BusinessObject>(), personResults.Where(p => p.IsDeleted));

				AssertEquals("Certificate should be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", Array.Empty<BusinessObject>(), certificatePatterns.Where(p => !p.IsDeleted));
			});
		}

		public void TestDeletePersonDeletesChildCollectionCertificatePatterns_ButDoesNotDeletesChildCollectionCertificates()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			var staffCertificate = staff.Certificates.AddNew();
			var staffCertPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, staffCertificate);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;
			var contactCertificate = contact.Certificates.AddNew();
			var contactCertPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, contactCertificate);

			var applicant = person.ApplicantCollection.AddNew();
			((IHRJobApplicant)applicant).HA_PER = person.PK;
			var applicantCertificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			applicantCertificate.XZ_ParentID = applicant.PK;
			applicantCertificate.XZ_ParentTableCode = "HA";
			var applicantCertPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, applicantCertificate);

			Factory.Save();
			person.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Person should be deleted", true, person.IsDeleted);

				AssertEquals("Staff certificate should not be deleted", false, staffCertificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Staff certificate patterns should all be deleted", Array.Empty<BusinessObject>(), staffCertPatterns.Where(p => !p.IsDeleted));

				AssertEquals("Contact certificate should not be deleted", false, contactCertificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Contact certificate patterns should all be deleted", Array.Empty<BusinessObject>(), contactCertPatterns.Where(p => !p.IsDeleted));

				AssertEquals("Applicant certificate should not be deleted", false, applicantCertificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Applicant certificate patterns should all be deleted", Array.Empty<BusinessObject>(), applicantCertPatterns.Where(p => !p.IsDeleted));
			});
		}

		#endregion

		#endregion

		#region Workflow

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "PER";

			var milestone = workflowTemplate.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_Description = "Test milestone - 001";
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORGABC";
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			var applicant = Factory.New<IHRJobApplicant>();
			applicant.HA_FullName = "name";

			Factory.Save();

			AssertEquals(true, staff.Person.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
			AssertEquals(true, contact.Person.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
			AssertEquals(true, Factory.Load<GlbPerson>(applicant.HA_PER).WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
		}

		#endregion

		#region UniqueIndexFailure

		public void TestUniqueIndexFailure()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_EmailAddress = "1@a.com";
			person1.PER_WebAccessEnabled = true;
			person1.PER_IsActive = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var person2 = factory2.NewWithValidTestData<GlbPerson>();
			person2.PER_EmailAddress = "1@a.com";
			person2.PER_WebAccessEnabled = true;
			person2.PER_IsActive = true;

			try
			{
				factory2.Save();
				Fail("Should throw");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("This email is already in use by another active Person with web access.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Picture

		[ExpectNoExceptions]
		public void TestPER_ProfilePicture_GIF()
		{
			var gifImageBytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0xF0, 0x01, 0x00, 0x00, 0x00,
				0x00, 0xFF, 0xFF, 0xFF, 0x21, 0xF9, 0x04, 0x08, 0x32, 0x00, 0x00, 0x00, 0x21, 0xFF, 0x0B, 0x4E, 0x45, 0x54, 0x53,
				0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30, 0x03, 0x01, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
				0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x21, 0xF9, 0x04, 0x08, 0x32, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00,
				0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x4C, 0x01, 0x00, 0x3B };

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_ProfilePicture = Image.FromStream(new MemoryStream(gifImageBytes));
			Factory.Save();

			var reloadedPerson = new BusinessObjectFactory().Load<GlbPerson>(person.PK);
			var gifImage = reloadedPerson.PER_ProfilePicture;
			var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
			AssertEquals(2, gifImage.GetFrameCount(dimension));
			gifImage.SelectActiveFrame(dimension, 0);
			gifImage.SelectActiveFrame(dimension, 1);
		}

		#endregion

		public void TestGetJobSkillCompletionDate_ExpiredMainCompletedRefresher()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_EmailAddress = "per@so.n";

			var accredMain = Factory.New<IGlbAccreditation>();
			accredMain.HAC_Code = "AC1";
			accredMain.HAC_CertificateCode = "CUS";
			accredMain.HAC_Description = "AC1 desc";

			var accredRef = Factory.New<IGlbAccreditation>();
			accredRef.HAC_Code = "AC2";
			accredRef.HAC_CertificateCode = "CUS";
			accredRef.HAC_Description = "AC2 desc";
			accredRef.HAC_IsRefresher = true;
			accredRef.HAC_RefresherCertificateExpiryType = "ELS";

			var pivot = Factory.New<IGlbAccreditationRequirementPivot>();
			pivot.HAR_HAC = accredRef.PK;
			pivot.HAR_HAC_Parent = accredMain.PK;

			var attempt1 = CreateAttempt(person, accredMain, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 2, 2));
			var attempt2 = CreateAttempt(person, accredRef, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			var attemptRogue = CreateAttempt(person, accredMain, new ZDate(2018, 12, 12), ZDate.Invalid, new ZDate(2020, 1, 1));

			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
		}

		IGlbAccreditationAttempt CreateAttempt(GlbPerson person, IGlbAccreditation accreditation, ZDate commenced, ZDate completed, ZDate expiry)
		{
			var attempt = Factory.New<IGlbAccreditationAttempt>();
			attempt.HAA_HAC = accreditation.PK;
			attempt.HAA_CommencementDate = commenced;
			if (completed.IsValid)
			{
				attempt.HAA_CompletionDate = completed;
			}
			attempt.HAA_CompletionDueDate = completed;
			attempt.HAA_ExpiryDate = expiry;
			attempt.HAA_PER = person.PK;
			return attempt;
		}

		public void TestNoPIILogs()
		{
			SetupSecurity(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals("Precondition: Current user has changed", staffDenied.PK, Env.CurrentUser.PK);

					var factoryForLoading1 = new BusinessObjectFactory();
					var personLoaded = factoryForLoading1.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, person.PK));

					AssertEquals("legal", personLoaded.PER_LegalNameInternal);
					AssertEquals(ZDate.BrettsBirthday, personLoaded.PER_BirthDateInternal);
					AssertEquals("full", personLoaded.PER_FullNameInternal);
					AssertEquals("M", personLoaded.PER_GenderInternal);
					AssertEquals("0499702888", personLoaded.PER_MobilePhoneInternal);
					AssertEquals("0499702893", personLoaded.PER_MobilePhone2Internal);
					AssertEquals("0280012200", personLoaded.PER_HomePhoneInternal);
					AssertEquals("0280012201", personLoaded.PER_FaxNumberInternal);
					AssertEquals("AU", personLoaded.PER_RN_NKCountryInternal);
					AssertEquals("UA", personLoaded.PER_RN_NKNationalityCodeISOInternal);
					AssertEquals("personal", personLoaded.PER_PersonalInfoInternal);
					AssertEquals("1", personLoaded.PER_HomeAddress1Internal);
					AssertEquals("2", personLoaded.PER_HomeAddress2Internal);
					AssertEquals("3", personLoaded.PER_CityInternal);
					AssertEquals("4", personLoaded.PER_StateInternal);
					AssertEquals("5", personLoaded.PER_PostcodeInternal);
					AssertEquals("email@test.com", personLoaded.PER_EmailAddressInternal);
					AssertEquals("email2@test.com", personLoaded.PER_EmailAddress2Internal);
					AssertEquals("A01234567", personLoaded.PER_DriversLicenseNumberInternal);

					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_LegalName);
					AssertEquals(ZDate.Empty, personLoaded.PER_BirthDate);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_EmailAddress);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_EmailAddress2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_Gender);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomePhone);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_RN_NKNationalityCodeISO);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_PersonalInfo);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomeAddress1);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomeAddress2);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_City);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_State);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_Postcode);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_MobilePhone_Formatted);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_HomePhone_Formatted);
					AssertEquals(personLoaded.ViewDeniedMessage, personLoaded.PER_DriversLicenseNumber);

					personLoaded.PER_LegalName = "legalx2";
					personLoaded.PER_BirthDate = ZDate.BrettsBirthday.AddDays(1);
					personLoaded.PER_FullName = "fullx2";
					personLoaded.PER_Gender = "F";
					personLoaded.PER_MobilePhone = "0499702888x2";
					personLoaded.PER_MobilePhone2 = "0499702893x2";
					personLoaded.PER_HomePhone = "0280012200x2";
					personLoaded.PER_FaxNumber = "0280012201x2";
					personLoaded.PER_RN_NKCountry = "NZ";
					personLoaded.PER_RN_NKNationalityCodeISO = "AU";
					personLoaded.PER_PersonalInfo = "personalx2";
					personLoaded.PER_HomeAddress1 = "1x2";
					personLoaded.PER_HomeAddress2 = "2x2";
					personLoaded.PER_City = "3x2";
					personLoaded.PER_State = "4x2";
					personLoaded.PER_Postcode = "5x2";
					personLoaded.PER_EmailAddress = "email@test.comx2";
					personLoaded.PER_EmailAddress2 = "email2@test.comx2";
					personLoaded.PER_DriversLicenseNumber = "A01234567x2";

					factoryForLoading1.Save();

					var logsAsText = string.Join("\r\n", personLoaded.Logs.GetAllLogs().OfType<StmALog>().Where(x => !x.SL_Reference.IsEmpty).Select(x => x.SL_Reference).OrderBy(x => x));
					AssertEquals(@"Changed PER_FullName.", logsAsText);
				}
			}
		}

		public void TestVerifyPassword()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.SetHashedPassword("pswrd");
			Assert(person.VerifyPassword("pswrd"));
			Assert(!person.VerifyPassword("pSwrd"));
			Assert(!person.VerifyPassword("powrd"));

			var iterations = person.PER_PasswordHashIterations;
			person.PER_PasswordHashIterations = 100;
			Assert(!person.VerifyPassword("pswrd"));

			person.PER_PasswordHashIterations = iterations;
			person.PER_PasswordSalt = new byte[] { 0, 0, 1, 1, 0, 0, 1, 1 };
			Assert(!person.VerifyPassword("pswrd"));
		}

		public void TestFindDuplicates_ShouldCallCorrectServices()
		{
			var testMdp = new MasterDataProviderForTest();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<IMasterDataProvider>(testMdp))
			{
				var person = Factory.New<GlbPerson>();
				((IDeduplicatable)person).ShouldRunDeduplication = true;

				person.FindDuplicates();

				CombineAssertions(() =>
				{
					AssertEquals("FindDuplicates call should not be for admin panel", false, testMdp.IsForAdminPanel);
					AssertEquals("FindDuplicates delegate should have been called", 1, testMdp.FindDuplicatesCallCount);
				});
			}
		}

		public void TestFindDuplicatesBypassErrorChecking_ShouldCallCorrectServices()
		{
			var testMdp = new MasterDataProviderForTest();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<IMasterDataProvider>(testMdp))
			{
				var person = Factory.New<GlbPerson>();
				((IDeduplicatable)person).ShouldRunDeduplication = true;

				((IDeduplicatable)person).FindDuplicatesBypassErrorChecking(true);

				CombineAssertions(() =>
				{
					AssertEquals("FindDuplicates call should be for admin panel", true, testMdp.IsForAdminPanel);
					AssertEquals("FindDuplicates delegate should have been called", 1, testMdp.FindDuplicatesCallCount);
				});
			}
		}

		public void TestIsLockedOut()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_LoginDisabledUntilUtc = ZDateTime.Empty;
			Assert("Not locked out if time is empty", !person.IsLockedOut);

			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(-3);
			Assert("Not locked out if time in past", !person.IsLockedOut);

			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(3);
			Assert("Locked out if time in future", person.IsLockedOut);
		}

		public void TestLockOutUntil()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var lockoutTime = ZDateTime.UtcNow;
			person.LockOutUntil(lockoutTime);

			AssertEquals("LoginDisabledUntilUtc is set", lockoutTime, person.PER_LoginDisabledUntilUtc);
		}

		public void TestUnlock()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow;
			person.Unlock();

			AssertEquals("LoginDisabledUntilUtc is empty", ZDateTime.Empty, person.PER_LoginDisabledUntilUtc);
		}

		public void TestLockoutDateTimeLocal()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var lockoutTimeUtc = ZDateTime.UtcNow.AddDays(1);
			person.LockOutUntil(lockoutTimeUtc);

			AssertEquals(lockoutTimeUtc.ToLocalBranchTime(), person.LockoutDateTimeLocal);
		}

		public void TestShouldAddWebAccessPasswordChangedLogOnSavingIfPasswordHashIsRemoved()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.SetHashedPassword("1234");
			Factory.Save();

			var logQuery = new ZQuery(StmALogSchema.SL_Parent, person.PK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.WebAccessPasswordChangedCode);
			AssertEquals("Precondition (password log only added if changed through web or if removed)", 0, Factory.Load<StmALog>(logQuery).Length);

			person.RemovePasswordHash();
			AssertEquals("Precondition", 0, Factory.Load<StmALog>(logQuery).Length);

			Factory.Save();
			AssertEquals("Should add a log for password removal", 1, Factory.Load<StmALog>(logQuery).Length);
		}

		public void TestPersonID()
		{
			var mainPerson = Factory.NewWithValidTestData<GlbPerson>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();

			var identifier1 = Factory.NewWithValidTestData<GlbMergedPerson>();
			identifier1.GMP_PER_Person = mainPerson.PK;
			identifier1.GMP_MergedPerson = person1.PK;
			var identifier2 = Factory.NewWithValidTestData<GlbMergedPerson>();
			identifier2.GMP_PER_Person = mainPerson.PK;
			identifier2.GMP_MergedPerson = person2.PK;
			Factory.Save();

			AssertEquals(3, mainPerson.PersonIDs.Count());
			AssertNotNull("Should contains itself", mainPerson.PersonIDs.FirstOrDefault(p => p == mainPerson.PK));
			AssertNotNull(mainPerson.PersonIDs.FirstOrDefault(p => p == person1.PK));
			AssertNotNull(mainPerson.PersonIDs.FirstOrDefault(p => p == person2.PK));
		}

		public void TestPasswordHistory()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.SetHashedPassword("p1");
			AssertEquals(true, person.HasPasswordBeenUsed("p1"));

			person.SetHashedPassword("p2", "p1");
			AssertEquals(true, person.HasPasswordBeenUsed("p1"));
			AssertEquals(true, person.HasPasswordBeenUsed("p2"));

			person.SetHashedPassword("p3");
			AssertEquals(true, person.HasPasswordBeenUsed("p1"));
			AssertEquals(true, person.HasPasswordBeenUsed("p2"));
			AssertEquals(true, person.HasPasswordBeenUsed("p3"));

			person.SetHashedPassword("p4", "p3");
			AssertEquals(false, person.HasPasswordBeenUsed("p1"));
			AssertEquals(true, person.HasPasswordBeenUsed("p2"));
			AssertEquals(true, person.HasPasswordBeenUsed("p3"));
			AssertEquals(true, person.HasPasswordBeenUsed("p4"));
			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(person).Count);

			person.RemovePasswordHash();
			AssertEquals(true, person.PER_PasswordHash.IsEmpty);
			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(person).Count);
		}

		class MasterDataProviderForTest : IMasterDataProvider
		{
			public MasterDataProviderForTest()
			{
				testSupportDuplicationFinder = new SupportDuplicationFinderForTest();
			}

			public bool IsForAdminPanel { get; set; } = true;

			public int FindDuplicatesCallCount => testSupportDuplicationFinder?.FindDuplicatesCallCount ?? 0;

			public void ComputeIsExcludedFromDeduplication(OrgHeader header, bool value)
			{
				throw new NotImplementedException();
			}

			public void ComputeIsExcludedFromDeduplication(GlbPerson person, bool value)
			{
				throw new NotImplementedException();
			}

			public GMI.IGlbPerson CreateIGlbPerson(GlbPerson person, bool isDummy)
			{
				throw new NotImplementedException();
			}

			public ISupportDuplicationFinder CreateOrgDuplicationFinder(OrgHeader header)
			{
				throw new NotImplementedException();
			}

			public ISupportDuplicationFinder CreatePersonDuplicationFinder(GlbPerson person, bool forAdminPanel)
			{
				IsForAdminPanel = forAdminPanel;
				return testSupportDuplicationFinder;
			}

			public void FindOrgDuplicates(OrgHeader header) { }

			public void FindPersonDuplicates(GlbPerson person) { }

			public IEnumerable<ScoringResult> FindPotentialOrgDuplicatesForAdminPanel(OrgHeader orgHeader)
			{
				return new List<ScoringResult>();
			}

			public IEnumerable<ScoringResult> FindPotentialPersonDuplicatesForAdminPanel(GlbPerson person)
			{
				return new List<ScoringResult>();
			}

			public List<GMI.IOrgContact> GetContactTargetLists(OrgContact target1, OrgContact target2)
			{
				return new List<GMI.IOrgContact>();
			}

			public IDeduplicationMaster GetDeduplicationGlbPerson(GlbPerson person)
			{
				return null;
			}

			public IDeduplicationMaster GetDeduplicationOrgHeader(OrgHeader header)
			{
				return null;
			}

			public IPatternMatchingRegenerator<OrgHeader>[] GetOrganisationPatternMatchingRegenerationEntities(PatternMatchingRecalculator<OrgHeader> recalculator)
			{
				return null;
			}

			public List<ISupportDuplicationFinder> GetOrganisationSupportedDuplicationFinders(OrgHeader header, Type targetType, DeduplicationProxyConfig config)
			{
				return null;
			}

			public string GetPersonDuplicationFinderMessageForTest(OrgContact contact, OrgContact target)
			{
				return string.Empty;
			}

			public IPatternMatchingRegenerator<GlbPerson>[] GetPersonPatternMatchingRegenerationEntities(PatternMatchingRecalculator<GlbPerson> recalculator)
			{
				return null;
			}

			public List<ISupportDuplicationFinder> GetPersonSupportedDuplicationFinders(GlbPerson person, DeduplicationProxyConfig config)
			{
				return null;
			}

			public List<GMI.IOrgHeader> GetTargetLists(OrgHeader orgTarget1, OrgHeader orgTarget2)
			{
				return new List<GMI.IOrgHeader>();
			}

			readonly SupportDuplicationFinderForTest testSupportDuplicationFinder;
		}

		class SupportDuplicationFinderForTest : ISupportDuplicationFinder
		{
			public SupportDuplicationFinderForTest()
			{
				FindDuplicatesCallCount = 0;
			}

			public int FindDuplicatesCallCount { get; set; }

			public Type TargetType => throw new NotImplementedException();

			public bool ShouldStopProcessing { get; set; }

			public List<ScoringResult> ScoringResults => null;

			public DuplicationStatus LastRunStatus { get; set; }

			public Task FindDuplicates()
			{
				FindDuplicatesCallCount++;
				return Task.CompletedTask;
			}

			public void RequestToCancel() { }
		}
	}
}
