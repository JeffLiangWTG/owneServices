using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Test
{
	public class RatingHelperTest : TestCaseWithFactory
	{
		public void TestGetAllowedNamedAccounts()
		{
			var testDate = ZDateTime.BrettsBirthday.ToDateTime();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var org1 = CreateOrgHeaderWithMappedNamedAccounts([("Nike Inc.", null)]);
			var org2 = CreateOrgHeaderWithMappedNamedAccounts([("Nike", null)]);
			var org3 = CreateOrgHeaderWithMappedNamedAccounts([("Test Account", null)]);
			var org4 = CreateOrgHeaderWithMappedNamedAccounts([("Test Account", carrier), ("Nike", carrier)]);
			var org5 = CreateOrgHeaderWithMappedNamedAccounts([]);
			org5.AddRelatedParty(org1.PK, "MNG", "FWD", "", "", GlbCompany.CurrentCompany);
			var org6 = CreateOrgHeaderWithMappedNamedAccounts([]);
			org6.AddRelatedParty(org5.PK, "MNG", "FWD", "", "", GlbCompany.CurrentCompany);

			AssertAllowedNamedAccounts("User with no groups: ", "test1", [], unconditionalAccess: false, [""]);

			AssertAllowedNamedAccounts("User with permit unconditional access: ", "test2", [], unconditionalAccess: true, []);

			AssertAllowedNamedAccounts("User with group with one org, org mapped to one named account: ", "test3", [[org1]], unconditionalAccess: false, ["", "Nike Inc."]);
			AssertAllowedNamedAccounts("User with group with one org, org mapped to multiple named account: ", "test4", [[org4]], unconditionalAccess: false, ["", "Nike", "Test Account"]);
			AssertAllowedNamedAccounts("User with group with one org, org has MNG related party to depth of 1: ", "test5", [[org5]], unconditionalAccess: false, ["", "Nike Inc."]);
			AssertAllowedNamedAccounts("User with group with one org, org has MNG related party to depth of 2: ", "test6", [[org6]], unconditionalAccess: false, ["", "Nike Inc."]);

			AssertAllowedNamedAccounts("User with group containing multiple orgs: ", "test7", [[org2, org3]], unconditionalAccess: false, ["", "Nike", "Test Account"]);

			AssertAllowedNamedAccounts("User with multiple groups containing single org: ", "test8", [[org2], [org3]], unconditionalAccess: false, ["", "Nike", "Test Account"]);
		}

		void AssertAllowedNamedAccounts(
			string message,
			string staffCode,
			OrgHeader[][] orgGroups,
			bool unconditionalAccess,
			string[] expectedNamedAccounts
			)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = staffCode;
			staff.GS_EmailAddress = "support@cargowise.com";

			if (unconditionalAccess)
			{
				var glbSecurity = staff.StaffSecurityPermissionsCollection.AddNew();
				glbSecurity.GU_SecurityRight = "WiseRatesCargoSphereRateSearch.IgnoreOSMG";
				glbSecurity.GU_SecurityItemIsAllowed = true;
			}

			int groupIndex = 0;
			foreach (var orgGroup in orgGroups)
			{
				var group = staff.Groups.AddNew();
				group.GG_Code = $"{staffCode}_group{groupIndex++}";
				foreach (var org in orgGroup)
				{
					group.Organisation.Add(org);
				}
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var result = RatingHelper.GetAllowedNamedAccounts();

				AssertContainsExactElementsInAnyOrder(message, expectedNamedAccounts, result);
			}
		}

		OrgHeader CreateOrgHeaderWithMappedNamedAccounts((string namedAccount, OrgHeader carrier)[] namedAccounts)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			foreach (var account in namedAccounts)
			{
				var cna = org.MappedNamedAccounts.AddNew();
				cna.ONA_ForeignName = account.namedAccount;
				if (account.carrier != null)
				{
					cna.ONA_OH_Carrier = account.carrier.PK;
				}
			}
			return org;
		}
	}
}
