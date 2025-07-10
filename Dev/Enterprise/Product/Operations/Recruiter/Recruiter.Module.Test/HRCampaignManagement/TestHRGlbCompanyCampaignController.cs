using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaignController))]
	public class TestHRGlbCompanyCampaignController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HRGlbCompanyCampaign;
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

		void TestShowForm_ChecksCampaignAllowSearchOutsideLoginCompanySecurityCheckpoint(Action<HRGlbCompanyCampaignController, BusinessObject> showFormDelegate)
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "GC2";
			otherCompany.GC_RN_NKCountryCode = "AU";
			var campaignForSameCompany = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaignForSameCompany.G0_GC = Env.CurrentCompany.PK;
			var campaignForDifferentCompany = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaignForDifferentCompany.G0_GC = otherCompany.PK;
			Factory.Save();
			Env.Security.HRCampaignAllowSearchOutsideLoginCompany.IsAllowed = true;
			AssertShowFormAllowed(showFormDelegate, campaignForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, campaignForDifferentCompany, true, null, null);
			Env.Security.HRCampaignAllowSearchOutsideLoginCompany.IsAllowed = false;
			AssertShowFormAllowed(showFormDelegate, campaignForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, campaignForDifferentCompany, false, string.Format(@"You do not have the appropriate security rights to view {0}. You are only allowed to view campaigns for your login company.

If you require access to this function please login to the relevant company (Australia (GC2)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.HRCampaignAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight, campaignForDifferentCompany.HumanReadableName), "Access Denied: Allow Search of Campaign Outside Login Company");
		}

		void AssertShowFormAllowed(Action<HRGlbCompanyCampaignController, BusinessObject> showFormDelegate, BusinessObject sourceEntity, bool expectedIsAllowed, string expectedLastMessage, string expectedLastCaption)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = new HRGlbCompanyCampaignController();
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
	}
}
