using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationHRJobApplicantTest : TestCaseWithFactory
	{
		public void TestCertificateCollectionsInitializedAsEmptyCollections()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();

			var dedupeApplicant = new DeduplicationHRJobApplicant(applicant, null);
			AssertEquals("Certificate collection should initially be empty", 0, dedupeApplicant.CertificateOrAccreditations.Count);
		}

		public void TestCertificatesCollectionPopulatedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();

			var cert1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var cert2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert1.XZ_ParentID = applicant.PK;
			cert1.XZ_ParentTableCode = "HA";
			cert2.XZ_ParentID = applicant.PK;
			cert2.XZ_ParentTableCode = "HA";

			var dedupeApplicant = new DeduplicationHRJobApplicant(applicant, null);

			AssertContainsExactElementsInAnyOrder("Certificates should match", new[] { cert1, cert2 }.Select(x => x.PK), dedupeApplicant.CertificateOrAccreditations.Select(x => x.XZ_PK));
		}

		public void TestIPersonPropertiesImplementedCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			var dedupeApplicant = new DeduplicationHRJobApplicant(applicant, null);

			AssertEquals("AddressFull", dedupeApplicant.AddressFull.ColName);
			AssertEquals("HA_Birthdate", dedupeApplicant.BirthDate.ColName);

			AssertEquals(2, dedupeApplicant.Address.Count);
			AssertEquals("HA_UserAddress1", dedupeApplicant.Address[0].ColName);
			AssertEquals("HA_UserAddress2", dedupeApplicant.Address[1].ColName);

			AssertEquals(1, dedupeApplicant.EmailAddresses.Count);
			AssertEquals("HA_EmailAddress", dedupeApplicant.EmailAddresses[0].ColName);

			AssertEquals("HA_FullName", dedupeApplicant.FullName.ColName);

			AssertEquals(4, dedupeApplicant.PhoneNumbers.Count);
			AssertEquals("HA_FaxNum", dedupeApplicant.PhoneNumbers[0].ColName);
			AssertEquals("HA_HomePhone", dedupeApplicant.PhoneNumbers[1].ColName);
			AssertEquals("HA_MobilePhone", dedupeApplicant.PhoneNumbers[2].ColName);
			AssertEquals("HA_WorkPhone", dedupeApplicant.PhoneNumbers[3].ColName);

			AssertEquals("CargoWise.Glow.Model.Interfaces.IHRJobApplicant", dedupeApplicant.GlowType.FullName);
		}

		public void TestConstructorSetRawNameCorrectly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = (IHRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_FullName = "ABC";
			var dedupApplicant = new DeduplicationHRJobApplicant(applicant, new DeduplicationGlbPerson(person));
			AssertEquals("ABC", dedupApplicant.HA_FullName);
			AssertEquals("ABC", dedupApplicant.RawName);
		}
	}
}
