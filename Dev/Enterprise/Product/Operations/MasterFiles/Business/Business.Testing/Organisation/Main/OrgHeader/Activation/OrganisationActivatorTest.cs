using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationActivatorTest : TestCaseWithFactory
	{
		public void TestDeactivateWithProxyCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_Code = "CORERULS";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;
			company.GC_Name = "Core Comp";
			Factory.Save();
			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org.PK }, false);
			ReloadOrgs(org);
			Assert("Should be active", org.OH_IsActive);
		}

		public void TestDeactivateUNMATCHEDorMISC()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_IsActive = true;
			org1.OH_Code = "OBEYCORE";
			Factory.Save();

			OrgHeader org2 = OrgHeader.UnmatchOrg(Factory);
			OrgHeader org3 = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;

			INotification notif = null;
			var notificationHeaderAction = new OrganisationActivator.ActivateOrDeactivateNotificationHeaderAction((x) =>
			{
				notif = x;
			});

			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org1.PK, org2.PK, org3.PK }, false, null, null, notificationHeaderAction, null);
			ReloadOrgs(org1, org2, org3);
			Assert("Should be deactivated as it is selected", !org1.OH_IsActive);
			Assert("Should not be deactivated because it is a system org", org2.OH_IsActive);
			Assert("Should not be deactivated because it is a system org", org3.OH_IsActive);

			Assert(notif.Message.Contains("Some of the selected Organizations were not De-activated because you cannot De-activate System Defined Organization."));
		}

		public void TestActivateDeactivateOrganisationOrgCollection()
		{
			bool previousActivateDeactivateOrgSecurity = Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed;

			try
			{
				OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_IsActive = true;
				org2.OH_IsActive = true;
				org3.OH_IsActive = true;
				org1.OH_Code = "WOOLLOOMOOL1";
				org2.OH_Code = "WOOLLOOMOOL2";
				org3.OH_Code = "WOOLLOOMOOL3";
				org1.OH_FullName = "Test 1";
				org1.OH_RL_NKClosestPort = "AUBNE";
				org2.OH_FullName = "Test 2";
				org2.OH_RL_NKClosestPort = "AUBNE";
				org3.OH_FullName = "Test 3";
				org3.OH_RL_NKClosestPort = "AUBNE";
				Factory.Save();

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				OrganisationActivator.ActivateOrDeactivate(System.Array.Empty<ZGuid>(), false);
				ReloadOrgs(org1, org2, org3);
				Assert("Doesn't do anything as no elements selected", org1.OH_IsActive);
				Assert("Doesn't do anything as no elements selected", org2.OH_IsActive);
				Assert("Doesn't do anything as no elements selected", org3.OH_IsActive);

				org1.OH_IsActive = false;
				org2.OH_IsActive = false;
				org3.OH_IsActive = false;
				org1.Factory.Save();

				OrganisationActivator.ActivateOrDeactivate(System.Array.Empty<ZGuid>(), true);
				ReloadOrgs(org1, org2, org3);
				Assert("Doesn't do anything as no elements selected", !org1.OH_IsActive);
				Assert("Doesn't do anything as no elements selected", !org2.OH_IsActive);
				Assert("Doesn't do anything as no elements selected", !org3.OH_IsActive);

				org1.OH_IsActive = true;
				org2.OH_IsActive = true;
				org3.OH_IsActive = true;
				org1.Factory.Save();

				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org2.PK }, false);
				ReloadOrgs(org1, org2, org3);
				Assert("Should not be de-activated org2 is selected", org1.OH_IsActive);
				Assert("Should be de-activated as it is selected", !org2.OH_IsActive);
				Assert("Should not be de-activated org2 is selected", org3.OH_IsActive);

				org1.OH_IsActive = false;
				org3.OH_IsActive = false;
				org1.Factory.Save();

				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org1.PK, org2.PK }, true);
				ReloadOrgs(org1, org2, org3);
				Assert("Should be activated as it is selected", org1.OH_IsActive);
				Assert("Should be activated as it is selected", org2.OH_IsActive);
				Assert("Should not be activated org1 is selected", !org3.OH_IsActive);
			}
			finally
			{
				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = previousActivateDeactivateOrgSecurity;
			}
		}

		void ReloadOrgs(params OrgHeader[] orgs)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			int count = orgs.Length;
			for (int i = 0; i < count; i++)
			{
				orgs[i] = factory.Load<OrgHeader>(orgs[i].PK);
			}
		}

		public void TestActivateDeactivateOrganisationSecurity()
		{
			bool previousActivateDeactivateOrgSecurity = Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed;

			try
			{
				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_IsActive = true;
				org.OH_Code = "WOOLLOOMOOL1";
				Factory.Save();

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				INotification notif = null;
				var notificationHeaderAction = new OrganisationActivator.ActivateOrDeactivateNotificationHeaderAction((x) =>
				{
					notif = x;
				});

				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org.PK }, true, null, null, notificationHeaderAction, null);
				Assert("Message shown about no security", notif.Message.Contains("You do not have rights to activate/deactivate organizations."));
				AssertEquals(CargoWise.EntityFramework.NotificationType.Error, notif.Type);

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org.PK }, true, null, null, notificationHeaderAction, null);
				Assert("Message about successfull Activation should be shown", notif.Message.Contains("Selected Organizations are Activated."));
				AssertEquals(CargoWise.EntityFramework.NotificationType.Information, notif.Type);

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org.PK }, false, null, null, notificationHeaderAction, null);
				Assert("Message shown about no security", notif.Message.Contains("You do not have rights to activate/deactivate organizations."));
				AssertEquals(CargoWise.EntityFramework.NotificationType.Error, notif.Type);

				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org.PK }, false, null, null, notificationHeaderAction, null);
				Assert("Message about successfull Deactivation should be shown", notif.Message.Contains("Selected Organizations are De-activated."));
				AssertEquals(CargoWise.EntityFramework.NotificationType.Information, notif.Type);

				OrganisationActivator.ActivateOrDeactivate(System.Array.Empty<ZGuid>(), true, null, null, notificationHeaderAction, null);
				Assert("Message about none selected", notif.Message.Contains("You have not selected any Organizations."));
				AssertEquals(CargoWise.EntityFramework.NotificationType.Error, notif.Type);
			}
			finally
			{
				Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = previousActivateDeactivateOrgSecurity;
			}
		}
	}
}
