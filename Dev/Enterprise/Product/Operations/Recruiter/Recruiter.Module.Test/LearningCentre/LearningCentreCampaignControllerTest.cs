using System;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(LearningCentreCampaignController))]
	public class LearningCentreCampaignControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.LearningCentreCampaign, Controller.ModuleID);
		}

		public void TestForm()
		{
			using (IZForm form = Controller.ShowNewForm())
			{
				AssertEquals(typeof(LearningCentreCampaignForm), form.GetType());
			}
		}

		public void TestSecurityCheckpoints()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			AssertEquals(Env.Security.HRJobSkillExamCampaignDelete, Controller.GetCheckPointForDelete(campaign));
			AssertEquals(Env.Security.HRJobSkillExamCampaignEdit, Controller.GetCheckPointForEdit(campaign));
			AssertEquals(Env.Security.HRJobSkillExamCampaignNew, Controller.GetCheckPointForNew(campaign));
			AssertEquals(Env.Security.HRJobSkillExamCampaignView, Controller.GetCheckPointForView(campaign));
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(LearningCentreCampaign);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LearningCentreCampaign;
		}
	}
}
