using System;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(CRMGlbCompanyCampaignModule))]
	public class CRMGlbCompanyCampaignModule_Test : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (CRMGlbCompanyCampaignModule module = new CRMGlbCompanyCampaignModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaign, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestGridCollection()
		{
			using (CRMGlbCompanyCampaignModule module = (CRMGlbCompanyCampaignModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCompanyCampaign))
			{
				GlbCompanyCampaign included1 = module.GetFactoryInternal().New<GlbCompanyCampaign>();
				included1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				GlbCompanyCampaign included2 = module.GetFactoryInternal().New<GlbCompanyCampaign>();
				included2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				GlbCompanyCampaign included3 = module.GetFactoryInternal().New<GlbCompanyCampaign>();
				included3.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				GlbCompanyCampaign notIncluded = module.GetFactoryInternal().New<GlbCompanyCampaign>();
				notIncluded.G0_BroadcastVoteSurveyExam = Core.Constants.Recruiter.LearningCentreCampaignType;

				((GlbCompanyCampaignCollection)module.GridCollection).Load();
				AssertEquals(3, module.GridCollection.Count);
				AssertCollectionContains(included1, module.GridCollection);
				AssertCollectionContains(included2, module.GridCollection);
				AssertCollectionContains(included3, module.GridCollection);
			}
		}

		protected virtual Type ExpectedCampaignTypeListType
		{
			get { return typeof(CampaignTypeList); }
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaign;
		}
	}
}
