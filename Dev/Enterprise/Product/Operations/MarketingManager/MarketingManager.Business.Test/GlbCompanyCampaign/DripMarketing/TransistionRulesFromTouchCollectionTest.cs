
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TransistionRulesFromTouchCollection))]
	sealed class TransistionRulesFromTouchCollectionTest : ActiveBusinessObjectCollectionTestCase<TransistionRulesFromTouchCollection>
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public void TestDefaults()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();

			var collection = new TransistionRulesFromTouchCollection(touch);
			var item = collection.AddNew();

			AssertEquals(master, item.MasterCampaign);
			AssertEquals(touch.PK, item.GCD_G0_ParentTouch);
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		public void TestFilter()
		{
			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();

			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			helper.Touch2A.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = helper.Touch1B.PK;
			helper.Touch2A.CurrentGroupColor = 1;

			Factory.Save();

			var collection = new TransistionRulesFromTouchCollection(helper.Touch1A);
			AssertEquals(1, collection.Count);

			collection = new TransistionRulesFromTouchCollection(helper.Touch1B);
			AssertEquals(1, collection.Count);
		}

		GlbCompanyCampaign touch;
		protected override TransistionRulesFromTouchCollection GetCollectionToTest()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";

			return new TransistionRulesFromTouchCollection(touch);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var element = base.GetNewElementToAddToTheCollection();
			(element as GlbCompanyCampaignDripMarketing).GCD_G0_ParentTouch = touch.PK;

			var nextTouch = touch.NextTouches.AddNew();
			nextTouch.G0_CampaignName = "next touch";
			(element as GlbCompanyCampaignDripMarketing).GCD_G0_NextTouch = nextTouch.PK;

			Factory.Save();
			return element;
		}
	}
}
