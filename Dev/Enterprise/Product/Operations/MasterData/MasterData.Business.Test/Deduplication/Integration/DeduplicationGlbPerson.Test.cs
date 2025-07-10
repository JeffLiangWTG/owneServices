using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationGlbPersonTest : TestCaseWithFactory
	{
		public void TestChildCollectionsInitializedAsEmptyCollections()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var dedupePerson = new DeduplicationGlbPerson(person);

			CombineAssertions(() =>
			{
				AssertEquals("Child staff collection should initially be empty", 0, dedupePerson.GlbStaffs.Count);
				AssertEquals("Child contact collection should initially be empty", 0, dedupePerson.OrgContacts.Count);
				AssertEquals("Child applicant collection should initially be empty", 0, dedupePerson.HRJobApplicants.Count);
			});
		}

		public void TestChildCollectionsPopulatedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_PER = person.PK;
			staff2.GS_PER = person.PK;

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_PER = person.PK;
			contact2.OC_PER = person.PK;

			var applicant1 = (Integration.Recruiter.IHRJobApplicant)person.ApplicantCollection.AddNew();
			var applicant2 = (Integration.Recruiter.IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant1.HA_PER = person.PK;
			applicant2.HA_PER = person.PK;

			var dedupePerson = new DeduplicationGlbPerson(person);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Person should have 2 child staffs", new[] { staff1, staff2 }.Select(x => x.PK), dedupePerson.GlbStaffs.Select(x => x.GS_PK));
				AssertContainsExactElementsInAnyOrder("Person should have 2 child contacts", new[] { contact1, contact2 }.Select(x => x.PK), dedupePerson.OrgContacts.Select(x => x.OC_PK));
				AssertContainsExactElementsInAnyOrder("Person should have 2 child applicants", new[] { applicant1, applicant2 }.Select(x => x.PK), dedupePerson.HRJobApplicants.Select(x => x.HA_PK));
			});
		}

		public void TestOrgAndBrandNamePopulatedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_PER = person.PK;
			contact2.OC_PER = person.PK;
			contact1.OC_OH = orgheader.PK;
			contact2.OC_OH = orgheader.PK;

			var brandName = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			orgheader.BrandsOrRelatedNames.Add(brandName);

			Factory.Save();

			var dedupePerson = new DeduplicationGlbPerson(person);
			AssertContainsExactElementsInAnyOrder("Person should have 2 child contacts", new[] { contact1, contact2 }.Select(x => x.PK), dedupePerson.OrgContacts.Select(x => x.OC_PK));

			var dedupContacts = dedupePerson.OrgContacts.ToArray();
			AssertEquals("Child Contacts should link to the same org object.", dedupContacts[0].OrgHeader, dedupContacts[1].OrgHeader);
			AssertEquals("There should be only one brand name", 1, dedupContacts[1].OrgHeader.OrgBrandOrRelatedNames.Count);
		}

		public void TestCertificateCollectionsInitializedAsEmptyCollections()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var dedupePerson = new DeduplicationGlbPerson(person);

			AssertEquals("Certificate collection should initially be empty", 0, dedupePerson.CertificateOrAccreditations.Count);
		}

		public void TestPersonsTypeInfo()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = person.ContactCollection.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			contact.OC_OH = orgHeader.PK;
			var deDupPerson = new DeduplicationGlbPerson(person);

			AssertEquals("Person should have contact and staff subtypes", ((IDeduplicatable)person).Info, deDupPerson.PersonsTypeInfo);
		}

		public void TestCertificatesCollectionPopulatedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var cert1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var cert2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert1.XZ_ParentID = person.PK;
			cert1.XZ_ParentTableCode = "PER";
			cert2.XZ_ParentID = person.PK;
			cert2.XZ_ParentTableCode = "PER";

			var dedupePerson = new DeduplicationGlbPerson(person);

			AssertContainsExactElementsInAnyOrder("Person should have 2 certificates", new[] { cert1, cert2 }.Select(x => x.PK), dedupePerson.CertificateOrAccreditations.Select(x => x.XZ_PK));
		}

		public void TestIPersonPropertiesImplementedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var dedupePerson = new DeduplicationGlbPerson(person);

			AssertEquals("AddressFull", dedupePerson.AddressFull.ColName);
			AssertEquals("PER_BirthDate", dedupePerson.BirthDate.ColName);

			AssertEquals(2, dedupePerson.Address.Count);
			AssertEquals("PER_HomeAddress1", dedupePerson.Address[0].ColName);
			AssertEquals("PER_HomeAddress2", dedupePerson.Address[1].ColName);

			AssertEquals(2, dedupePerson.EmailAddresses.Count);
			AssertEquals("PER_EmailAddress", dedupePerson.EmailAddresses[0].ColName);
			AssertEquals("PER_EmailAddress2", dedupePerson.EmailAddresses[1].ColName);

			AssertEquals("PER_FullName", dedupePerson.FullName.ColName);

			AssertEquals(4, dedupePerson.PhoneNumbers.Count);
			AssertEquals("PER_FaxNumber", dedupePerson.PhoneNumbers[0].ColName);
			AssertEquals("PER_HomePhone", dedupePerson.PhoneNumbers[1].ColName);
			AssertEquals("PER_MobilePhone", dedupePerson.PhoneNumbers[2].ColName);
			AssertEquals("PER_MobilePhone2", dedupePerson.PhoneNumbers[3].ColName);

			AssertEquals("CargoWise.Glow.Model.Interfaces.IGlbPerson", dedupePerson.GlowType.FullName);
		}

		public void TestChildrenOfOrgContactAreIncluded()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = person.ContactCollection.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			contact.OC_OH = orgHeader.PK;
			var contactItem = contact.ContactItems.AddNew();
			var dedupePerson = new DeduplicationGlbPerson(person);

			AssertEquals(1, dedupePerson.OrgContacts.Count);
			AssertEquals(1, dedupePerson.OrgContacts.First().OrgContactItems.Count);
		}

		public void TestConstructor()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "ABC";
			var dedupePerson = new DeduplicationGlbPerson(person, new DateTime(2018, 1, 1), "Address1", "Address2", "City", "Postcode", "State", "Email1", "Email2", "Mobile1", "Mobile2", "FaxNumber", "HomePhone", "AU", false);

			AssertEquals("ABC", dedupePerson.PER_FullName);
			AssertEquals("ABC", dedupePerson.RawName);
			AssertEquals(new DateTime(2018, 1, 1), dedupePerson.PER_BirthDate);
			AssertEquals("Address1", dedupePerson.PER_HomeAddress1);
			AssertEquals("Address2", dedupePerson.PER_HomeAddress2);
			AssertEquals("City", dedupePerson.PER_City);
			AssertEquals("Postcode", dedupePerson.PER_Postcode);
			AssertEquals("State", dedupePerson.PER_State);
			AssertEquals("Email1", dedupePerson.PER_EmailAddress);
			AssertEquals("Email2", dedupePerson.PER_EmailAddress2);
			AssertEquals("Mobile1", dedupePerson.PER_MobilePhone);
			AssertEquals("Mobile2", dedupePerson.PER_MobilePhone2);
			AssertEquals("FaxNumber", dedupePerson.PER_FaxNumber);
			AssertEquals("HomePhone", dedupePerson.PER_HomePhone);
			AssertEquals("AU", dedupePerson.PER_RN_NKCountry);
		}

		public void TestGetRelatedTo()
		{
			var branch = Factory.New<GlbBranch>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_GB_HomeBranch = branch.PK;
			var staffOrg = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = staffOrg.PK;
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = staffOrg.PK;
			contact.ParentOrg.OH_Code = "TestOH_Code";
			var deduplicationGlbPerson = new DeduplicationGlbPerson(person);
			AssertEquals("TestOH_Code", deduplicationGlbPerson.GetRelatedTo(Factory));
		}
	}
}
