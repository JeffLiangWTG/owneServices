using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TransistionRulesToTouchCollection))]
	sealed class TransistionRulesToTouchCollectionTest : ActiveBusinessObjectCollectionTestCase<TransistionRulesToTouchCollection>
	{
		public void TestDefaults()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();

			var collection = new TransistionRulesToTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(touch.PK, item.GCD_G0_NextTouch);
		}

		public void TestDefaults_NextTouch_ParentHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 2;

			var collection = new TransistionRulesToTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(touch.PK, item.GCD_G0_NextTouch);
			AssertEquals((byte)1, item.GCD_ParentHorizontalId);
			AssertEquals(ZGuid.Empty, item.GCD_G0_ParentTouch);
		}

		public void TestDefaults_NextTouch_FirstHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 1;

			var collection = new TransistionRulesToTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(touch.PK, item.GCD_G0_NextTouch);
			AssertEquals((byte)0, item.GCD_ParentHorizontalId);
			AssertEquals(master.PK, item.GCD_G0_ParentTouch);
		}

		public void TestDefaults_NextGroup_ParentHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = ZGuid.NewZGuid().ToString();
			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 2;
			touch.CurrentGroupColor = 1;
			touch.G0_CampaignName = ZGuid.NewZGuid().ToString();

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch, Factory);

			Factory.Save();

			var collection = new TransistionRulesToTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(ZGuid.Empty, item.GCD_G0_NextTouch);
			AssertEquals(touch.G0_GCG_Group, item.GCD_GCG_Group);
			AssertEquals((byte)1, item.GCD_ParentHorizontalId);
			AssertEquals(ZGuid.Empty, item.GCD_G0_ParentTouch);
		}

		public void TestDefaults_NextGroup_FirstHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = ZGuid.NewZGuid().ToString();
			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 1;
			touch.CurrentGroupColor = 1;
			touch.G0_CampaignName = ZGuid.NewZGuid().ToString();

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch, Factory);

			Factory.Save();

			var collection = new TransistionRulesToTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(ZGuid.Empty, item.GCD_G0_NextTouch);
			AssertEquals(touch.G0_GCG_Group, item.GCD_GCG_Group);
			AssertEquals((byte)0, item.GCD_ParentHorizontalId);
			AssertEquals(master.PK, item.GCD_G0_ParentTouch);
		}

		public void TestCompanyCampaignDripMarketing_ParentTouchHorizontal()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			master.AllTouches.Add(touch1A);

			var touch1B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";
			master.AllTouches.Add(touch1B);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			master.AllTouches.Add(touch2A);
			Factory.Save();

			var glbCompanyCampaignDripMarketing = Factory.LoadTop1<GlbCompanyCampaignDripMarketing>(new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, touch1A.PK));
			AssertNotNull(glbCompanyCampaignDripMarketing);
		}

		public void TestFilter()
		{
			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();
			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var touch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2.G0_GCG_Group = group.PK;

			var rule1 = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			rule1.GCD_G0_NextTouch = touch1.PK;

			var rule2 = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			rule2.GCD_GCG_Group = group.PK;

			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1, Factory, true);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2, Factory, true);

			Factory.Save();

			var collection = new TransistionRulesToTouchCollection(touch1);
			AssertEquals(1, collection.Count);
			AssertEquals(rule1, collection[0]);

			collection = new TransistionRulesToTouchCollection(touch2);
			AssertEquals(1, collection.Count);
			AssertEquals(rule2, collection[0]);
		}

		public void TestAllowNew()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			var collection = new TransistionRulesToTouchCollection(touch);

			AssertEquals(false, ((IBindingList)collection).AllowNew);

			touch.G0_HorizontalId = 1;
			AssertEquals(true, ((IBindingList)collection).AllowNew);
		}

		protected override TransistionRulesToTouchCollection GetCollectionToTest()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			return new TransistionRulesToTouchCollection(touch);
		}
	}
}
