using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlowContactSecurityServiceTest : TestCaseWithFactory
	{
		public void TestNoIdentity()
		{
			var result = securityService.AreRightsGranted(null, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertEquals("An authentication token must be provided.", result);
		}

		public void TestIdentityNotAuthenticated()
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(false);

			var result = securityService.AreRightsGranted(identityMock.Object, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertEquals("An authentication token must be provided.", result);
		}

		public void TestContactNotFound()
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			identityMock.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());

			var result = securityService.AreRightsGranted(identityMock.Object, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertEquals("The user record could not be found.", result);
		}

		public void TestContactHasNoRights()
		{
			var identity = SetupTestContactAndRemoveRights(WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			var result = securityService.AreRightsGranted(identity, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertEquals("You do not have the relevant security rights.", result);
		}

		public void TestContactIfNoRightsChecked()
		{
			var identity = SetupTestContactAndRemoveRights();

			var result = securityService.AreRightsGranted(identity);

			AssertNull(result);
		}

		public void TestContactHasSomeRights()
		{
			var identity = SetupTestContactAndRemoveRights(WebSecurityRightsList.eRequestPortalViewAll);

			var result = securityService.AreRightsGranted(identity, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertEquals("You do not have the relevant security rights.", result);
		}

		public void TestContactHasAllRights()
		{
			var identity = SetupTestContactAndRemoveRights();

			var result = securityService.AreRightsGranted(identity, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertNull(result);
		}

		public void TestStaff_Allowed()
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns(GlbStaffSchema.Constants.Prefix);
			identityMock.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());

			var result = securityService.AreRightsGranted(identityMock.Object, WebSecurityRightsList.eRequestPortalViewAll, WebSecurityRightsList.eRequestPortalViewOwn, WebSecurityRightsList.eRequestPortalSubmit);

			AssertNull(result);
		}

		public void TestGetContactOrganisation_NoIdentity_ShouldReturnNull()
		{
			AssertNull(securityService.GetContactOrganisation(null));
		}

		public void TestGetContactOrganisation_NotAuthenticated_ShouldReturnNull()
		{
			var identity = SetupIdentityMock(Guid.Empty, isAuthenticated: false);
			AssertNull(securityService.GetContactOrganisation(identity));
		}

		public void TestGetContactOrganisation_NotContact_ShouldNotReturnNull()
		{
			var identity = SetupIdentityMock(Guid.Empty, providerType: GlbStaffSchema.Constants.Prefix);
			AssertNull(securityService.GetContactOrganisation(identity));
		}

		public void TestGetContactOrganisation_InvalidKey_ShouldReturnNull()
		{
			var identity = SetupIdentityMock(Guid.Empty);
			AssertNull(securityService.GetContactOrganisation(identity));
		}

		public void TestGetContactOrganisation_ShouldReturnContactOrganisationIfExists()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var identity = SetupIdentityMock(contact.PK.ToGuid());
			AssertEquals(org.PK, securityService.GetContactOrganisation(identity).PK);
		}

		public void TestHasGroupRole_NotAuthenticated()
		{
			var pk = Guid.NewGuid();
			var identity = SetupIdentityMock(pk, isAuthenticated: false);
			SetupGroupRole(pk, "roleName");

			Assert(!securityService.HasGroupRole(identity, "roleName"));
		}

		public void TestHasGroupRole_NotGranted()
		{
			var pk = Guid.NewGuid();
			var identity = SetupIdentityMock(pk);
			SetupGroupRole(pk, "someOtherRoleName");

			Assert(!securityService.HasGroupRole(identity, "roleName"));
		}

		public void TestHasGroupRole_Staff()
		{
			var pk = Guid.NewGuid();
			var identity = SetupIdentityMock(pk, true, GlbStaffSchema.Constants.Prefix);
			SetupGroupRole(pk, "roleName");

			Assert(!securityService.HasGroupRole(identity, "roleName"));
		}

		public void TestHasGroupRole()
		{
			var pk = Guid.NewGuid();
			var identity = SetupIdentityMock(pk);
			SetupGroupRole(pk, "roleName");

			Assert(securityService.HasGroupRole(identity, "roleName"));
		}

		IGlowAuthenticationTicketIdentity SetupIdentityMock(
			Guid providerKey,
			bool isAuthenticated = true,
			string providerType = OrgContactSchema.Constants.Prefix)
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.ProviderKey).Returns(providerKey);
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(isAuthenticated);
			identityMock.SetupGet(x => x.ProviderType).Returns(providerType);

			Factory.Save();

			return identityMock.Object;
		}

		IGlowAuthenticationTicketIdentity SetupTestContactAndRemoveRights(params WebSecurityRight[] rights)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var identityMock = SetupIdentityMock(contact.PK.ToGuid());

			foreach (var right in rights)
			{
				var orgRight = org.SecurityRights.AddNew();
				orgRight.OX_Granted = false;
				orgRight.OX_SecurityItemName = right.SecurityItemName;
			}

			Factory.Save();

			return identityMock;
		}

		void SetupGroupRole(Guid contactPk, string groupRoleName)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;

			var contact = Factory.NewWithPrimaryKey<OrgContact>(contactPk);
			contact.OC_OH = org.PK;

			var group = Factory.New<GlbGroup>();
			group.GG_Type = "ORG";
			group.GG_Code = "ROLES";
			group.GG_Desc = "Some Group";

			var groupRole = Factory.New<GlbGroupRole>();
			groupRole.GGR_GG_Group = group.PK;
			groupRole.GGR_RoleName = groupRoleName;

			var contactLink = Factory.New<GlbGroupOrgContactLink>();
			contactLink.GCK_GG_Group = group.PK;
			contactLink.GCK_OC_Contact = contact.PK;

			Factory.Save();
		}

		protected override void SetUp()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityService = new GlowContactSecurityService();
		}
		GlowContactSecurityService securityService;
	}
}
