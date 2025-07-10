using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationProviderTest : TestCaseWithFactory
	{
		readonly string[] phoneNumbers = { "+12015551111", "+12015552222", "+12015553333", "+12015554444", "+12015555555", "+12015556666", "+12015557777", "+12015558888" };

		readonly string[] formattedPhoneNumbers = { "+1 201-555-1111", "+1 201-555-2222", "+1 201-555-3333", "+1 201-555-4444", "+1 201-555-5555", "+1 201-555-6666", "+1 201-555-7777", "+1 201-555-8888" };

		OrgHeader[] GetTestOrgs()
		{
			var masterOrg = Factory.New<OrgHeader>();
			var targetOrg = Factory.New<OrgHeader>();
			masterOrg.OH_Code = "Code";
			targetOrg.OH_Code = "Code1";
			var masterAddress = masterOrg.Addresses.AddNew();
			var targetAddress = targetOrg.Addresses.AddNew();
			masterAddress.OA_Address1 = "Address1";
			masterAddress.OA_Address2 = "Address2";
			masterAddress.OA_Code = "Code1";
			targetAddress.OA_Address1 = "Address1";
			targetAddress.OA_Address2 = "Address2";
			targetAddress.OA_Code = "Code2";
			targetAddress.OA_Phone = masterAddress.OA_Phone = phoneNumbers[0];
			targetAddress.OA_Mobile = masterAddress.OA_Mobile = phoneNumbers[1];
			targetAddress.OA_Fax = masterAddress.OA_Fax = phoneNumbers[2];
			var masterContact = masterOrg.Contacts.AddNew();
			var targetContact = targetOrg.Contacts.AddNew();
			targetContact.OC_Phone = masterContact.OC_Phone = phoneNumbers[3];
			targetContact.OC_Mobile = masterContact.OC_Mobile = phoneNumbers[4];
			targetContact.OC_Fax = masterContact.OC_Fax = phoneNumbers[5];
			targetContact.OC_HomePhone = masterContact.OC_HomePhone = phoneNumbers[6];
			targetContact.OC_OtherPhone = masterContact.OC_OtherPhone = phoneNumbers[7];
			masterAddress.OA_Latitude = 9.51413;
			masterAddress.OA_Longitude = -7.65432;
			targetAddress.OA_Latitude = 3.14422;
			targetAddress.OA_Longitude = 1.23433;

			return new OrgHeader[] { masterOrg, targetOrg };
		}

		GlbPerson[] GetTestPersons()
		{
			var personMaster = Factory.New<GlbPerson>();
			var contactMaster = Factory.NewWithValidTestData<OrgContact>();
			var staffMaster = personMaster.StaffCollection.AddNew();
			var applicantMaster = Factory.New<Integration.Recruiter.IHRJobApplicant>();

			var personTarget = Factory.New<GlbPerson>();
			var contactTarget = Factory.NewWithValidTestData<OrgContact>();
			var staffTarget = personTarget.StaffCollection.AddNew();
			var applicantTarget = Factory.New<Integration.Recruiter.IHRJobApplicant>();

			SetMasterTargetPhoneValuesForPerson(personMaster, personTarget, phoneNumbers[0], phoneNumbers[1], phoneNumbers[2], phoneNumbers[3]);
			SetMasterTargetPhoneValuesForContact(contactMaster, contactTarget, phoneNumbers[0], phoneNumbers[1], phoneNumbers[2], phoneNumbers[3], phoneNumbers[4]);
			SetMasterTargetPhoneValuesForStaff(staffMaster, staffTarget, phoneNumbers[0], phoneNumbers[1], phoneNumbers[2], phoneNumbers[3]);
			SetMasterTargetPhoneValuesForApplicant(applicantMaster, applicantTarget, phoneNumbers[0], phoneNumbers[1], phoneNumbers[2], phoneNumbers[3]);

			contactMaster.OC_PER = staffMaster.GS_PER = applicantMaster.HA_PER = personMaster.PK;
			contactTarget.OC_PER = staffTarget.GS_PER = applicantTarget.HA_PER = personTarget.PK;

			personMaster.ContactCollection.Add(contactMaster);
			personMaster.ApplicantCollection.Add((BusinessObject)applicantMaster);
			personTarget.ContactCollection.Add(contactTarget);
			personTarget.ApplicantCollection.Add((BusinessObject)applicantTarget);

			return new[] { personMaster, personTarget };
		}

		void SetMasterTargetPhoneValuesForApplicant(Integration.Recruiter.IHRJobApplicant master, Integration.Recruiter.IHRJobApplicant target, string fax, string homePhone, string mobile, string workPhone)
		{
			var applicantBizOMaster = (BusinessObject)master;
			var applicantBizOTarget = (BusinessObject)target;

			applicantBizOMaster[HRJobApplicantSchema.Constants.HA_WorkPhone] = applicantBizOTarget[HRJobApplicantSchema.Constants.HA_WorkPhone] = workPhone;
		}

		void SetMasterTargetPhoneValuesForContact(OrgContact master, OrgContact target, string phone, string homePhone, string mobile, string otherPhone, string fax)
		{
			master.OC_Phone = target.OC_Phone = phone;
			master.OC_HomePhone = target.OC_HomePhone = homePhone;
			master.OC_Mobile = target.OC_Mobile = mobile;
			master.OC_OtherPhone = target.OC_OtherPhone = otherPhone;
			master.OC_Fax = target.OC_Fax = fax;
		}

		void SetMasterTargetPhoneValuesForPerson(GlbPerson master, GlbPerson target, string homePhone, string mobile1, string mobile2, string fax)
		{
			master.PER_HomePhone = target.PER_HomePhone = homePhone;
			master.PER_MobilePhone = target.PER_MobilePhone = mobile1;
			master.PER_MobilePhone2 = target.PER_MobilePhone2 = mobile2;
			master.PER_FaxNumber = target.PER_FaxNumber = fax;
		}

		void SetMasterTargetPhoneValuesForStaff(GlbStaff master, GlbStaff target, string fax, string homePhone, string mobile, string otherPhone)
		{
			master.GS_FaxNum = target.GS_FaxNum = fax;
			master.GS_HomePhone = target.GS_HomePhone = homePhone;
			master.GS_MobilePhone = target.GS_MobilePhone = mobile;
			master.GS_WorkPhone = target.GS_WorkPhone = otherPhone;
		}

		public void TestFormatValueForOrg()
		{
			var orgs = GetTestOrgs();
			var masterOrg = orgs[0];
			var targetOrg = orgs[1];
			Factory.Save();

			var masterGlow = new DeduplicationOrgHeader(masterOrg);
			var targetGlow = new DeduplicationOrgHeader(targetOrg);
			var provider = new DeduplicationProvider();
			var expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[OrgAddressSchema.Constants.OA_Phone] = formattedPhoneNumbers[0],
				[OrgAddressSchema.Constants.OA_Mobile] = formattedPhoneNumbers[1],
				[OrgAddressSchema.Constants.OA_Fax] = formattedPhoneNumbers[2]
			};
			AssertPhoneNumberFormattedForOrg(typeof(IOrgAddress), masterOrg.Addresses[1].PK, targetOrg.Addresses[1].PK);

			expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[OrgContactSchema.Constants.OC_Phone] = formattedPhoneNumbers[3],
				[OrgContactSchema.Constants.OC_Mobile] = formattedPhoneNumbers[4],
				[OrgContactSchema.Constants.OC_Fax] = formattedPhoneNumbers[5],
				[OrgContactSchema.Constants.OC_HomePhone] = formattedPhoneNumbers[6],
				[OrgContactSchema.Constants.OC_OtherPhone] = formattedPhoneNumbers[7],
			};
			AssertPhoneNumberFormattedForOrg(typeof(IOrgContact), masterOrg.Contacts.Single().PK, targetOrg.Contacts.Single().PK);

			AssertEquals("9.514 -7.654", provider.GetMasterComparisonValue(masterGlow, masterOrg.Addresses[1].PK.ToGuid(), typeof(IOrgAddress), new[] { "OA_Latitude", "OA_Longitude" }, false));
			AssertEquals("3.144 1.234", provider.GetTargetComparisonValue(new[] { targetGlow }, targetOrg.Addresses[1].PK.ToGuid(), typeof(IOrgAddress), new[] { "OA_Latitude", "OA_Longitude" }, false));

			void AssertPhoneNumberFormattedForOrg(Type glowType, ZGuid masterChildPK, ZGuid targetChildPK)
			{
				CombineAssertions(() =>
				{
					foreach (var phoneNumber in expectedPhoneNumbers)
					{
						AssertEquals(phoneNumber.Value, provider.GetMasterComparisonValue(masterGlow, masterChildPK.ToGuid(), glowType, new[] { phoneNumber.Key }, false));
						AssertEquals(phoneNumber.Value, provider.GetTargetComparisonValue(new[] { targetGlow }, targetChildPK.ToGuid(), glowType, new[] { phoneNumber.Key }, false));
					}
				});
			}
		}

		public void Test_NonStandardPhone_NotStandardizedToEmpty()
		{
			var nonStandardNumber = "1234892422"; //invalid number manually verified by user

			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			var masterContact = masterOrg.Contacts.AddNew();
			masterContact.OC_Phone = nonStandardNumber;
			Factory.Save();
			var provider = new DeduplicationProvider();
			var masterGlow = new DeduplicationOrgHeader(masterOrg);

			AssertEquals(nonStandardNumber, provider.GetMasterComparisonValue(masterGlow, masterOrg.Contacts[0].PK.ToGuid(), typeof(IOrgContact), new[] { OrgContactSchema.Constants.OC_Phone }, false));
		}

		public void Test_EmptyPhone_StandardizedToEmpty()
		{
			var emptyNumber = string.Empty;

			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			var masterContact = masterOrg.Contacts.AddNew();
			masterContact.OC_Phone = emptyNumber;
			var provider = new DeduplicationProvider();
			var masterGlow = new DeduplicationOrgHeader(masterOrg);

			AssertEquals(emptyNumber, provider.GetMasterComparisonValue(masterGlow, masterOrg.Contacts[0].PK.ToGuid(), typeof(IOrgContact), new[] { OrgContactSchema.Constants.OC_Phone }, false));
		}

		public void TestInvalidColumnDoesNotThrowException()
		{
			// Arrange
			var orgs = GetTestOrgs();
			var masterOrg = orgs[0];
			Factory.Save();

			var masterGlow = new DeduplicationOrgHeader(masterOrg);
			var provider = new DeduplicationProvider();

			// Assert
			AssertNoExceptionThrown(() => provider.GetMasterComparisonValue(masterGlow, masterOrg.Addresses[1].PK.ToGuid(), typeof(IOrgAddress), new[] { "INVALID_COLUMN" }, false));
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestBirthdayComparisonValueIsCorrect()
		{
			var master = Factory.NewWithValidTestData<GlbPerson>();
			var target = Factory.NewWithValidTestData<GlbPerson>();
			var masterStaff = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, master);
			var masterApplicant = PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, master);
			var masterContact = PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, master);
			var targetStaff = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, target);
			var targetApplicant = PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, target);
			var targetContact = PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, target);

			var birthday = ZDateTime.BrettsBirthday.ToDateTime();

			master.PER_BirthDate = target.PER_BirthDate = (ZDate)birthday;
			masterStaff.GS_Birthdate = targetStaff.GS_Birthdate = (ZDate)birthday;
			masterApplicant.HA_Birthdate = targetApplicant.HA_Birthdate = (ZDate)birthday;
			masterContact.OC_Birthday = targetContact.OC_Birthday = birthday;

			Factory.Save();

			var provider = new DeduplicationProvider();
			var masterGlow = master.CreateIGlbPerson();
			var targetGlow = target.CreateIGlbPerson();

			var expectedBirthday = birthday.ToShortDateString();

			CombineAssertions(() =>
			{
				AssertEquals("Master Person Birthday Comparison Value is correct", expectedBirthday, provider.GetMasterComparisonValue(masterGlow, ((IDeduplicationGlowObject)masterGlow).PK, typeof(IGlbPerson), new[] { GlbPersonSchema.Constants.PER_BirthDate }, false));
				AssertEquals("Master Contact Birthday Comparison Value is correct", expectedBirthday, provider.GetMasterComparisonValue(masterGlow, masterContact.PK.ToGuid(), typeof(IOrgContact), new[] { OrgContactSchema.Constants.OC_Birthday }, false));
				AssertEquals("Master Staff Birthday Comparison Value is correct", expectedBirthday, provider.GetMasterComparisonValue(masterGlow, masterStaff.PK.ToGuid(), typeof(IGlbStaff), new[] { GlbStaffSchema.Constants.GS_Birthdate }, false));
				AssertEquals("Target Person Birthday Comparison Value is correct", expectedBirthday, provider.GetTargetComparisonValue(new[] { (DeduplicationGlbPerson)targetGlow }, ((IDeduplicationGlowObject)targetGlow).PK, typeof(IGlbPerson), new[] { GlbPersonSchema.Constants.PER_BirthDate }, false));
				AssertEquals("Target Contact Birthday Comparison Value is correct", expectedBirthday, provider.GetTargetComparisonValue(new[] { (DeduplicationGlbPerson)targetGlow }, targetContact.PK.ToGuid(), typeof(IOrgContact), new[] { OrgContactSchema.Constants.OC_Birthday }, false));
				AssertEquals("Target Staff Birthday Comparison Value is correct", expectedBirthday, provider.GetTargetComparisonValue(new[] { (DeduplicationGlbPerson)targetGlow }, targetStaff.PK.ToGuid(), typeof(IGlbStaff), new[] { GlbStaffSchema.Constants.GS_Birthdate }, false));
			});
		}

		public void TestFormatValueForPerson()
		{
			var persons = GetTestPersons();
			var masterPerson = persons[0];
			var targetPerson = persons[1];
			var masterGlow = masterPerson.CreateIGlbPerson();
			var targetGlow = targetPerson.CreateIGlbPerson();
			var provider = new DeduplicationProvider();

			var expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[GlbPersonSchema.Constants.PER_HomePhone] = formattedPhoneNumbers[0],
				[GlbPersonSchema.Constants.PER_MobilePhone] = formattedPhoneNumbers[1],
				[GlbPersonSchema.Constants.PER_MobilePhone2] = formattedPhoneNumbers[2],
				[GlbPersonSchema.Constants.PER_FaxNumber] = formattedPhoneNumbers[3],
			};
			AssertPhoneNumberFormattedForPerson(typeof(IGlbPerson), ((IDeduplicationGlowObject)masterGlow).PK, ((IDeduplicationGlowObject)targetGlow).PK);

			expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[OrgContactSchema.Constants.OC_Phone] = formattedPhoneNumbers[0],
				[OrgContactSchema.Constants.OC_HomePhone] = formattedPhoneNumbers[1],
				[OrgContactSchema.Constants.OC_Mobile] = formattedPhoneNumbers[2],
				[OrgContactSchema.Constants.OC_OtherPhone] = formattedPhoneNumbers[3],
				[OrgContactSchema.Constants.OC_Fax] = formattedPhoneNumbers[4]
			};
			AssertPhoneNumberFormattedForPerson(typeof(IOrgContact), masterPerson.ContactCollection.Single().PK, targetPerson.ContactCollection.Single().PK);

			expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[GlbStaffSchema.Constants.GS_FaxNum] = formattedPhoneNumbers[0],
				[GlbStaffSchema.Constants.GS_HomePhone] = formattedPhoneNumbers[1],
				[GlbStaffSchema.Constants.GS_MobilePhone] = formattedPhoneNumbers[2],
				[GlbStaffSchema.Constants.GS_WorkPhone] = formattedPhoneNumbers[3]
			};
			AssertPhoneNumberFormattedForPerson(typeof(IGlbStaff), masterPerson.StaffCollection.Single().PK, targetPerson.StaffCollection.Single().PK);

			expectedPhoneNumbers = new Dictionary<string, string>()
			{
				[HRJobApplicantSchema.Constants.HA_WorkPhone] = formattedPhoneNumbers[3]
			};
			AssertPhoneNumberFormattedForPerson(typeof(IHRJobApplicant), masterPerson.ApplicantCollection.Single().PK, targetPerson.ApplicantCollection.Single().PK);

			void AssertPhoneNumberFormattedForPerson(Type glowType, ZGuid masterChildPK, ZGuid targetChildPK)
			{
				CombineAssertions(() =>
				{
					foreach (var phoneNumber in expectedPhoneNumbers)
					{
						AssertEquals(phoneNumber.Value, provider.GetMasterComparisonValue(masterGlow, masterChildPK.ToGuid(), glowType, new[] { phoneNumber.Key }, false));
						AssertEquals(phoneNumber.Value, provider.GetTargetComparisonValue(new[] { (DeduplicationGlbPerson)targetGlow }, targetChildPK.ToGuid(), glowType, new[] { phoneNumber.Key }, false));
					}
				});
			}
		}

		public void TestGetDisplayNameForColumns()
		{
			var orgs = GetTestOrgs();
			var masterOrg = orgs[0];
			var targetOrg = orgs[1];

			var masterGlow = new DeduplicationOrgHeader(masterOrg);
			var targetGlow = new DeduplicationOrgHeader(targetOrg);
			var provider = new DeduplicationProvider();

			AssertEquals(DeduplicationProvider.Constants.Name, provider.GetDisplayNameForColumns(new[] { OrgHeaderSchema.Constants.OH_FullName }));
			AssertEquals(DeduplicationProvider.Constants.Name, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_ContactName }));
			AssertEquals(DeduplicationProvider.Constants.Name, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_FullName }));
			AssertEquals(DeduplicationProvider.Constants.Name, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_FullName }));
			AssertEquals(DeduplicationProvider.Constants.Brand, provider.GetDisplayNameForColumns(new[] { OrgBrandOrRelatedNameSchema.Constants.P1_RelatedName }));
			AssertEquals(DeduplicationProvider.Constants.ContactPhone, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_Phone }));
			AssertEquals(DeduplicationProvider.Constants.AddressPhone, provider.GetDisplayNameForColumns(new[] { OrgAddressSchema.Constants.OA_Phone }));
			AssertEquals(DeduplicationProvider.Constants.ContactMobile, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_Mobile }));
			AssertEquals(DeduplicationProvider.Constants.AddressMobile, provider.GetDisplayNameForColumns(new[] { OrgAddressSchema.Constants.OA_Mobile }));
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { OrgAddressSchema.Constants.OA_Email }));
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_Email }));
			AssertEquals(DeduplicationProvider.Constants.OtherPhone, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_OtherPhone }));
			AssertEquals(DeduplicationProvider.Constants.HomePhone, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_HomePhone }));
			AssertEquals(DeduplicationProvider.Constants.AddressFax, provider.GetDisplayNameForColumns(new[] { OrgAddressSchema.Constants.OA_Fax }));
			AssertEquals(DeduplicationProvider.Constants.ContactFax, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_Fax }));
			AssertEquals(DeduplicationProvider.Constants.Website, provider.GetDisplayNameForColumns(new[] { OrgWebURLSchema.Constants.PU_URL }));
			AssertEquals(DeduplicationProvider.Constants.Type, provider.GetDisplayNameForColumns(new[] { OrgCusCodeSchema.Constants.OK_CodeType }));
			AssertEquals(DeduplicationProvider.Constants.Country, provider.GetDisplayNameForColumns(new[] { OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry }));
			AssertEquals(DeduplicationProvider.Constants.CusCodeCustomsRegNo, provider.GetDisplayNameForColumns(new[] { OrgCusCodeSchema.Constants.OK_CustomsRegNo }));
			AssertEquals(DeduplicationProvider.Constants.Birthday, provider.GetDisplayNameForColumns(new[] { OrgContactSchema.Constants.OC_Birthday }));

			var address = new string[]
			{
				OrgAddressSchema.Constants.OA_Address1,
				OrgAddressSchema.Constants.OA_Address2,
				OrgAddressSchema.Constants.OA_City,
				OrgAddressSchema.Constants.OA_State,
				OrgAddressSchema.Constants.OA_PostCode,
				OrgAddressSchema.Constants.OA_RN_NKCountryCode
			};

			AssertEquals(DeduplicationProvider.Constants.Address, provider.GetDisplayNameForColumns(address));
			AssertEquals(DeduplicationProvider.Constants.Coordinates, provider.GetDisplayNameForColumns(new[] { OrgAddressSchema.Constants.OA_GeoLocation }));

			//GlbPerson
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_EmailAddress }));
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_EmailAddress2 }));
			AssertEquals(DeduplicationProvider.Constants.PersonPhone, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_HomePhone }));
			AssertEquals(DeduplicationProvider.Constants.PersonMobile, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_MobilePhone }));
			AssertEquals(DeduplicationProvider.Constants.PersonMobile, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_MobilePhone2 }));
			AssertEquals(DeduplicationProvider.Constants.PersonFax, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_FaxNumber }));
			AssertEquals(DeduplicationProvider.Constants.Birthday, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_BirthDate }));

			//GlbStaff
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_EmailAddress }));
			AssertEquals(DeduplicationProvider.Constants.PersonPhone, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_HomePhone }));
			AssertEquals(DeduplicationProvider.Constants.PersonMobile, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_MobilePhone }));
			AssertEquals(DeduplicationProvider.Constants.PersonWorkPhone, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_WorkPhone }));
			AssertEquals(DeduplicationProvider.Constants.PersonFax, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_FaxNum }));
			AssertEquals(DeduplicationProvider.Constants.Birthday, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_Birthdate }));

			//HRJobApplicant
			AssertEquals(DeduplicationProvider.Constants.Email, provider.GetDisplayNameForColumns(new[] { HRJobApplicantSchema.Constants.HA_EmailAddress }));
			AssertEquals(DeduplicationProvider.Constants.PersonWorkPhone, provider.GetDisplayNameForColumns(new[] { HRJobApplicantSchema.Constants.HA_WorkPhone }));

			//Columns not in the mapping should be returned directly with their field names.
			AssertEquals(GlbStaffSchema.Constants.GS_Passport, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_Passport }));
			AssertEquals(GlbStaffSchema.Constants.GS_EnterpriseCertificationID, provider.GetDisplayNameForColumns(new[] { GlbStaffSchema.Constants.GS_EnterpriseCertificationID }));
			AssertEquals(GlbPersonSchema.Constants.PER_Passport, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_Passport }));
			AssertEquals(GlbPersonSchema.Constants.PER_FriendlyNameAI, provider.GetDisplayNameForColumns(new[] { GlbPersonSchema.Constants.PER_FriendlyNameAI }));
		}

		public void TestGetGroupNameForType()
		{
			var provider = new DeduplicationProvider();
			AssertEquals(DeduplicationProvider.Constants.OrganisationNames, provider.GetGroupNameForType(typeof(MultiSourceCompanyName)));
			AssertEquals(DeduplicationProvider.Constants.PersonNames, provider.GetGroupNameForType(typeof(MultiSourcePersonName)));
			AssertEquals(DeduplicationProvider.Constants.Domains, provider.GetGroupNameForType(typeof(MultiSourceDomain)));
			AssertEquals(DeduplicationProvider.Constants.PhoneNumbers, provider.GetGroupNameForType(typeof(MultiSourcePhoneNumber)));
			AssertEquals(DeduplicationProvider.Constants.Emails, provider.GetGroupNameForType(typeof(MultiSourceEmail)));
			AssertEquals(DeduplicationProvider.Constants.Birthdays, provider.GetGroupNameForType(typeof(MultiSourceBirthday)));
		}
	}
}
