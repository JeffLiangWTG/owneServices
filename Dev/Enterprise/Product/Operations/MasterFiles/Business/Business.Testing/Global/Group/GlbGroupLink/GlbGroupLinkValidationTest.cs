using System.Collections.Generic;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbGroupLinkValidationTest : BusinessObjectValidationTestCase
	{
		void AssertExistingLinksHaveNoErrors(GlbStaff staff)
		{
			foreach (GlbGroupLink link in Factory.Load<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GS, staff.PK)))
			{
				link.Validation.ValidateAll();
				AssertNoErrors(link);
			}
		}

		public void TestMembershipTypeValidation()
		{
			GlbGroupLink groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			string codeNotInList = "ZZZ";
			if (groupLink.Lookups.StaffMembershipTypesList.ContainsCode(codeNotInList))
			{
				groupLink.Lookups.StaffMembershipTypesList.RemoveCode(codeNotInList);
			}
			groupLink.GK_MembershipType = "ZZZ";
			Assert("Code ZZZ not in list, should have errors", groupLink.GK_MembershipTypeInfo.HasErrors());

			groupLink.GK_MembershipType = MembershipTypeList.Codes.STF;
			Assert("Code is in list, should not have errors", !groupLink.GK_MembershipTypeInfo.HasErrors());
		}

		public void TestCheckGK_GG()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var allStaffGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_IsController = false;

			group1.GG_Code = "GGX";
			group2.GG_Code = "GGY";

			var existingLink = Factory.New<GlbGroupLink>();
			existingLink.GK_GG = group1.PK;
			existingLink.GK_GS = staff1.PK;

			var security1 = staff1.StaffSecurityPermissionsCollection.AddNew();
			security1.GU_SecurityRight = Env.Security.StaffGroups.Code;
			security1.GU_SecurityItemIsAllowed = true;

			var security2 = staff2.StaffSecurityPermissionsCollection.AddNew();
			security2.GU_SecurityRight = Env.Security.StaffGroups.Code;
			security2.GU_SecurityItemIsAllowed = false;

			staff2.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group1.GG_Code);

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff1.GS_LoginName))
			{
				AssertExistingLinksHaveNoErrors(staff1);
				AssertExistingLinksHaveNoErrors(staff2);

				var link1 = Factory.New<GlbGroupLink>();
				link1.GK_GG = group1.PK;
				AssertNoErrors(link1.GK_GGInfo);

				var link2 = Factory.New<GlbGroupLink>();
				link2.GK_GG = group2.PK;
				AssertNoErrors(link2.GK_GGInfo);

				var link3 = Factory.New<GlbGroupLink>();
				link3.GK_GG = allStaffGroup.PK;
				AssertNoErrors(link3.GK_GGInfo);

				var newStaff = Factory.New<GlbStaff>();
				AssertExistingLinksHaveNoErrors(newStaff);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff2.GS_LoginName))
			{
				AssertExistingLinksHaveNoErrors(staff1);
				AssertExistingLinksHaveNoErrors(staff2);

				var link1 = Factory.New<GlbGroupLink>();
				link1.GK_GG = group1.PK;
				AssertNoErrors(link1.GK_GGInfo);

				var link2 = Factory.New<GlbGroupLink>();
				link2.GK_GG = group2.PK;
				AssertHasError(link2.GK_GGInfo, "You do not have security rights to add staff members to the 'GGY' group.");

				var link3 = Factory.New<GlbGroupLink>();
				link3.GK_GG = allStaffGroup.PK;
				AssertNoErrors(link3.GK_GGInfo);

				var newStaff = Factory.New<GlbStaff>();
				AssertExistingLinksHaveNoErrors(newStaff);

				link2.GK_GG = ZGuid.Invalid;
				AssertEquals("Errors.Length", 1, link2.GK_GGInfo.GetErrors().Count());
				AssertHasError(link2.GK_GGInfo, "Enter a valid Group.");
			}
		}

		public void TestCheckGK_SkillLevel()
		{
			GlbGroupLink groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_SkillLevel = 5;
			AssertNoErrors("Valid skill level, should NOT have errors", groupLink.GK_SkillLevelInfo);

			groupLink.GK_SkillLevel = 0;
			AssertNoErrors("Valid skill level, should NOT have errors", groupLink.GK_SkillLevelInfo);

			groupLink.GK_SkillLevel = 11;
			AssertHasErrors("Invalid skill level, should have errors", groupLink.GK_SkillLevelInfo);

			groupLink.GK_SkillLevel = 10;
			AssertNoErrors("Valid skill level, should NOT have errors", groupLink.GK_SkillLevelInfo);
		}

		public void TestCheckGK_GGWithSalesTeamsEditPermission()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupSales = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;
			group.GG_Code = "GGY";

			groupSales.GG_Code = "GGS";
			groupSales.GG_IsSales = true;

			Factory.Save();

			Env.Security.SalesTeamsEdit.IsAllowed = true;
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				var groupLink = Factory.New<GlbGroupLink>();
				groupLink.GK_GG = group.PK;
				AssertHasError(groupLink.GK_GGInfo, "You do not have security rights to add staff members to the 'GGY' group.");

				var groupLinkSales = Factory.New<GlbGroupLink>();
				groupLinkSales.GK_GG = groupSales.PK;
				AssertNoErrors("GK_GG should not have errors.", groupLinkSales.GK_GGInfo);
			}
		}

		public void TestCheckGK_GS()
		{
			var groupLink = Factory.New<GlbGroupLink>();
			var staff = Factory.New<GlbStaff>();
			var group = Factory.New<GlbGroup>();

			staff.GS_Code = "TST";
			staff.GS_FullName = "Test User Name";
			staff.GS_IsActive = false;

			groupLink.GK_GS = staff.PK;

			AssertHasError("The GlbGroupLink has no group, SHOULD have error since staff is not sales rep", groupLink.GK_GSInfo, "Staff member Test User Name (TST) is inactive and cannot be used.");
			AssertNoWarnings("Should NOT have any warnings", groupLink.GK_GSInfo);

			staff.GS_IsSalesRep = true;
			groupLink.Validation.ValidateAll();
			AssertNoErrors("The GlbGroupLink has no group, SHOULD NOT have error since staff is sales rep", groupLink.GK_GSInfo);
			AssertHasWarning("Should have a warning", groupLink.GK_GSInfo, "Staff member Test User Name (TST) is inactive.");

			staff.GS_IsSalesRep = false;
			groupLink.GK_GG = group.PK;
			groupLink.Validation.ValidateAll();
			AssertHasError("Group is not Sales Team, SHOULD have error", groupLink.GK_GSInfo, "Staff member Test User Name (TST) is inactive and cannot be used.");
			AssertNoWarnings("Should NOT have any warnings", groupLink.GK_GSInfo);

			staff.GS_IsSalesRep = true;
			groupLink.Validation.ValidateAll();
			AssertHasError("Group is not Sales Team, SHOULD have error", groupLink.GK_GSInfo, "Staff member Test User Name (TST) is inactive and cannot be used.");
			AssertNoWarnings("Should NOT have any warnings", groupLink.GK_GSInfo);

			group.GG_IsSales = true;
			groupLink.Validation.ValidateAll();
			AssertNoErrors("Staff is inactive, but group is Sales Team. Should NOT have errors", groupLink.GK_GSInfo);
			AssertHasWarning("Should have a warning", groupLink.GK_GSInfo, "Staff member Test User Name (TST) is inactive.");

			staff.GS_IsActive = true;
			groupLink.Validation.ValidateAll();
			AssertNoErrors("Staff is active, should NOT have errors", groupLink.GK_GSInfo);
			AssertNoWarnings("Should NOT have any warnings", groupLink.GK_GSInfo);
		}

		public void TestCheckGK_GG_ValidatesDomainIsTheSameForStaffAndGroup()
		{
			var domainCredentials1 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials1.DomainName = "domain1";
			domainCredentials1.IsDefaultDomain = true;
			var domainCredentials2 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials2.DomainName = "domain2";
			domainCredentials2.IsDefaultDomain = false;

			var adRegistry = ObjectFactory.Get<IADRegistry>();
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domainCredentials1, domainCredentials2 };
			adRegistry.IsIntegrationEnabled = true;
			adRegistry.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			// Substitute IDirectorySearcherProvider to return a mocked directory searcher so staff's and group's DomainCredentialsDomainName will return the default domain and won't be trying to connect to the fake domain
			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(new Mock<IDirectorySearcher>().Object);
			ObjectFactory.Substitute(directorySearcherProviderMock.Object);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_DomainName = "domain1";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_DomainName = string.Empty;

			var validLink = Factory.New<GlbGroupLink>();
			validLink.GK_GS = staff1.PK;
			validLink.GK_GG = group1.PK;

			AssertNoRowWarningContaining(staff1, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");
			AssertNoRowWarningContaining(group1, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");

			staff1.GS_DomainName = "domain1";

			AssertNoRowWarningContaining(staff1, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");
			AssertNoRowWarningContaining(group1, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_DomainName = "domain2";

			var warningLink = Factory.New<GlbGroupLink>();
			warningLink.GK_GS = staff1.PK;
			warningLink.GK_GG = group2.PK;

			AssertHasRowWarning(group2, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");
			AssertHasRowWarning(staff1, "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");
		}
	}
}
