
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignCollectionStrongyTyped))]
	sealed class GlbCompanyCampaignCollectionStrongyTypedTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignCollectionStrongyTyped>
	{
		GlbStaff manager;
		GlbStaff coordinator;
		GlbCompanyCampaign master;

		protected override GlbCompanyCampaignCollectionStrongyTyped GetCollectionToTest()
		{
			manager = Factory.NewWithValidTestData<GlbStaff>();
			coordinator = Factory.NewWithValidTestData<GlbStaff>();

			master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			master.G0_IsSalesAndMarketing = true;
			master.G0_Category = master.Lookups.MediaCategoryList[0].Code;
			master.G0_Type = master.Lookups.MediaTypesList[0].Code;
			master.G0_GS_NKCampaignManager = manager.GS_Code;
			master.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			master.G0_Stage = master.Lookups.CampaignStages[0].Code;
			master.G0_EstimatedStartedDate = ZDateTime.BrettsBirthday;
			master.G0_EstimatedCompletedDate = ZDateTime.BrettsBirthday.AddDays(1);
			master.G0_ActualStartedDate = ZDateTime.BrettsBirthday.AddDays(2);
			master.G0_ActualCompletedDate = ZDateTime.BrettsBirthday.AddDays(3);

			return new GlbCompanyCampaignCollectionStrongyTyped(master, new ZQuery(), 0);
		}

		public void TestDefaults()
		{
			var collection = Collection;
			var newCampaign = collection.AddNew();

			AssertEquals(true, newCampaign.G0_IsSalesAndMarketing);
			AssertEquals(master.Lookups.MediaCategoryList[0].Code, newCampaign.G0_Category);
			AssertEquals(master.Lookups.MediaTypesList[0].Code, newCampaign.G0_Type);
			AssertEquals(manager.GS_Code, newCampaign.G0_GS_NKCampaignManager);
			AssertEquals(coordinator.GS_Code, newCampaign.G0_GS_NKCampaignCoordinator);
			AssertEquals(master.Lookups.CampaignStages[0].Code, newCampaign.G0_Stage);
			AssertEquals(ZDateTime.BrettsBirthday, newCampaign.G0_EstimatedStartedDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), newCampaign.G0_EstimatedCompletedDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(2), newCampaign.G0_ActualStartedDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(3), newCampaign.G0_ActualCompletedDate);
			AssertEquals((byte)0, newCampaign.G0_HorizontalId);
			AssertEquals("A", newCampaign.G0_VerticalId);

			var horizontal1 = master.AddHorizontal();
			var horizontal2 = master.AddHorizontal();
			horizontal1.AddCampaign(Factory.NewWithValidTestData<GlbCompanyCampaign>());
			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			horizontal2.AddCampaign(touch2a);

			collection = new GlbCompanyCampaignCollectionStrongyTyped(master, new ZQuery(), 2);
			newCampaign = collection.AddNew();

			AssertEquals((byte)2, newCampaign.G0_HorizontalId);
			AssertEquals("B", newCampaign.G0_VerticalId);
		}
	}
}
