using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationGlbStaffTest : TestCaseWithFactory
	{
		public void TestCertificateCollectionsInitializedAsEmptyCollections()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dedupeStaff = new DeduplicationGlbStaff(staff, null);

			AssertEquals("Certificate collection should initially be empty", 0, dedupeStaff.CertificateOrAccreditations.Count);
		}

		public void TestCertificatesCollectionPopulatedCorrectly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var cert1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var cert2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert1.XZ_ParentID = staff.PK;
			cert1.XZ_ParentTableCode = "GS";
			cert2.XZ_ParentID = staff.PK;
			cert2.XZ_ParentTableCode = "GS";

			var dedupeStaff = new DeduplicationGlbStaff(staff, null);

			AssertContainsExactElementsInAnyOrder("Certificates should match", new[] { cert1, cert2 }.Select(x => x.PK), dedupeStaff.CertificateOrAccreditations.Select(x => x.XZ_PK));
		}

		public void TestIPersonPropertiesImplementedCorrectly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dedupeStaff = new DeduplicationGlbStaff(staff, null);

			AssertEquals("AddressFull", dedupeStaff.AddressFull.ColName);
			AssertEquals("GS_Birthdate", dedupeStaff.BirthDate.ColName);

			AssertEquals(2, dedupeStaff.Address.Count);
			AssertEquals("GS_UserAddress1", dedupeStaff.Address[0].ColName);
			AssertEquals("GS_UserAddress2", dedupeStaff.Address[1].ColName);

			AssertEquals(1, dedupeStaff.EmailAddresses.Count);
			AssertEquals("GS_EmailAddress", dedupeStaff.EmailAddresses[0].ColName);

			AssertEquals("GS_FullName", dedupeStaff.FullName.ColName);

			AssertEquals(5, dedupeStaff.PhoneNumbers.Count);
			AssertEquals("GS_FaxNum", dedupeStaff.PhoneNumbers[0].ColName);
			AssertEquals("GS_HomePhone", dedupeStaff.PhoneNumbers[1].ColName);
			AssertEquals("GS_MobilePhone", dedupeStaff.PhoneNumbers[2].ColName);
			AssertEquals("GS_Pager", dedupeStaff.PhoneNumbers[3].ColName);
			AssertEquals("GS_WorkPhone", dedupeStaff.PhoneNumbers[4].ColName);

			AssertEquals("CargoWise.Glow.Model.Interfaces.IGlbStaff", dedupeStaff.GlowType.FullName);
		}

		public void TestConstructorSetRawNameCorrectly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "ABC";
			var dedupStaff = new DeduplicationGlbStaff(staff, null);
			AssertEquals("ABC", dedupStaff.GS_FullName);
			AssertEquals("ABC", dedupStaff.RawName);
		}

		public void TestConstructor_WithoutAccessRights_SetValueCorrectly()
		{
			var userWithoutSecurity = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_UserAddress1 = "User Address 123";
			staff.GS_UserAddress2 = "User Address 234";
			staff.GS_City = "SYD";
			staff.GS_State = "DUMMY";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_Postcode = "ABC";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(userWithoutSecurity.GS_LoginName))
			{
				AssertEquals("Precondition", false, Env.Security.StaffViewHomeAddressDetails.IsAllowed);

				var dedupStaff = new DeduplicationGlbStaff(staff, null);

				CombineAssertions(() =>
				{
					AssertEquals("User Address 123", dedupStaff.GS_UserAddress1);
					AssertEquals("User Address 234", dedupStaff.GS_UserAddress2);
					AssertEquals("SYD", dedupStaff.GS_City);
					AssertEquals("DUMMY", dedupStaff.GS_State);
					AssertEquals("AU", dedupStaff.GS_RN_NKCountryCode);
					AssertEquals("ABC", dedupStaff.GS_Postcode);
				});
			}
		}
	}
}
