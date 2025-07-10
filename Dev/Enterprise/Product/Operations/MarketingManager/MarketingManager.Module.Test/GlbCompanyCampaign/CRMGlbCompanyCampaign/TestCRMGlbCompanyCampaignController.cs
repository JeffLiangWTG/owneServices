using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(CRMGlbCompanyCampaignController))]
	public class TestCRMGlbCompanyCampaignController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaign;
		}

		#region Showing Forms

		#region TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint

		public void TestShowViewForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint()
		{
			TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint()
		{
			TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowDeleteForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint()
		{
			TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		public void TestShowCopyForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint()
		{
			TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint((controller, bizObj) => controller.ShowTemplateCopyForm(bizObj));
		}

		void TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint(Action<CRMGlbCompanyCampaignController, BusinessObject> showFormDelegate)
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "GC2";
			otherCompany.GC_RN_NKCountryCode = "AU";

			var campaignForSameCompany = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignForSameCompany.G0_GC = Env.CurrentCompany.PK;
			campaignForSameCompany.G0_CampaignID = "TST00001000";
			campaignForSameCompany.G0_CampaignName = "Same Company Campaign";
			var campaignForDifferentCompany = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignForDifferentCompany.G0_GC = otherCompany.PK;
			campaignForDifferentCompany.G0_CampaignID = "TST00001001";
			campaignForDifferentCompany.G0_CampaignName = "Different Company Campaign";

			Factory.Save();

			Env.Security.CampaignAllowSearchOutsideLoginCompany.IsAllowed = true;
			AssertShowFormAllowed(showFormDelegate, campaignForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, campaignForDifferentCompany, true, null, null);

			Env.Security.CampaignAllowSearchOutsideLoginCompany.IsAllowed = false;
			AssertShowFormAllowed(showFormDelegate, campaignForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, campaignForDifferentCompany,
				false,
				string.Format(@"You do not have the appropriate security rights to view {0}. You are only allowed to view campaigns for your login company.

If you require access to this function please login to the relevant company (Australia (GC2)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.CampaignAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight, campaignForDifferentCompany.HumanReadableName),
				"Access Denied: Allow Search of Campaign Outside Login Company");
		}

		void AssertShowFormAllowed(Action<CRMGlbCompanyCampaignController, BusinessObject> showFormDelegate, BusinessObject sourceEntity, bool expectedIsAllowed, string expectedLastMessage, string expectedLastCaption)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var controller = new CRMGlbCompanyCampaignController();
			showFormDelegate(controller, sourceEntity);

			CombineAssertions(() =>
			{
				using (var lastShownForm = controller.LastShownForm)
				{
					if (expectedIsAllowed)
					{
						AssertNotNull("LastShownForm", lastShownForm);
					}
					else
					{
						AssertNull("LastShownForm", lastShownForm);
					}
				}

				AssertMultilineASCIIEquals("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertMultilineASCIIEquals("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			});
		}

		#endregion

		#endregion

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			bizObjWithoutAccess.G0_GS_NKCampaignCoordinator = bizObjWithoutAccess.G0_GS_NKCampaignManager = "U00";
			CRMSecurityProviderTest<GlbCompanyCampaign>.AssertController(new CRMGlbCompanyCampaignController(), bizObjWithoutAccess, Env.Security.CampaignManagementCRMSecurity);
		}

		#endregion

		public void TestUrlsCanBeOpenedByAnyCompany()
		{
			Assert("Campaign hyperlinks should not be restricted to the current company", !Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
