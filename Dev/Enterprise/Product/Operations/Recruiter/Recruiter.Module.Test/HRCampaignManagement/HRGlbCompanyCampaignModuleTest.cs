using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module
{
	[TestedType(typeof(HRGlbCompanyCampaignModule))]
	public class HRGlbCompanyCampaignModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new HRGlbCompanyCampaignModule())
			{
				AssertEquals(ModuleIDs.HRGlbCompanyCampaign, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestGridCollection()
		{
			var included1 = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			included1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			var included2 = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			included2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			var included3 = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			included3.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			var notIncluded = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			notIncluded.G0_BroadcastVoteSurveyExam = Core.Constants.Recruiter.LearningCentreCampaignType;
			Factory.Save();
			using (var module = (HRGlbCompanyCampaignModule)ZModuleFactory.Instance.Create(ModuleIDs.HRGlbCompanyCampaign))
			{
				((GlbCompanyCampaignCollection)module.GridCollection).Load();
				var results = module.GridCollection.Cast<BusinessObject>().Select(b => b.PK);
				AssertEquals(3, module.GridCollection.Count);
				AssertCollectionContains(included1.PK, results);
				AssertCollectionContains(included2.PK, results);
				AssertCollectionContains(included3.PK, results);
			}
		}

		protected virtual Type ExpectedCampaignTypeListType
		{
			get
			{
				return typeof(CampaignTypeList);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRGlbCompanyCampaign;
		}
	}
}
