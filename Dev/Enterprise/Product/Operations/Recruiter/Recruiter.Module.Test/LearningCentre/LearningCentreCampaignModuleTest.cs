using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(LearningCentreCampaignModule))]
	public class LearningCentreCampaignModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.LearningCentreCampaign;
		}

		public void TestCheckpoints()
		{
			using (LearningCentreCampaignModule module = new LearningCentreCampaignModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HRJobSkillExamCampaign, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LearningAndDevelopment, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewController()
		{
			using (LearningCentreCampaignModuleForTest module = new LearningCentreCampaignModuleForTest())
			{
				ZController controller = module.GetNewController(Factory.New<LearningCentreCampaign>());
				Assert("Invalid type", controller is LearningCentreCampaignController);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (LearningCentreCampaignModuleForTest module = new LearningCentreCampaignModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is LearningCentreCampaignFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (LearningCentreCampaignModuleForTest module = new LearningCentreCampaignModuleForTest())
			{
				IBusinessObjectCollection campaignCollection = module.NewGridCollection;
				Assert("Invalid type", campaignCollection is LearningCentreCampaignCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (LearningCentreCampaignModuleForTest module = new LearningCentreCampaignModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is LearningCentreCampaignFilterBusinessObject);
			}
		}
		#endregion
	}
}
