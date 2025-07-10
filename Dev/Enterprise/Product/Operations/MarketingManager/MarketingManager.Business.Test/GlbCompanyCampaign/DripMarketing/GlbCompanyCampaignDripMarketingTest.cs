using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignDripMarketing))]
	public class GlbCompanyCampaignDripMarketingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestModuleId()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_G0_Master = master.PK;

			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = touch.PK;

			AssertEquals(ExpectedModuleID.Name, pivot.FilterRule.S9_ModuleID);
		}

		protected virtual ModuleIdentifier ExpectedModuleID
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRule;
			}
		}

		public void TestParentHorizontalIdForBinding()
		{
			var pivot = Factory.New<GlbCompanyCampaignDripMarketing>();

			AssertEquals("", pivot.ParentHorizontalIdForBinding);

			pivot.GCD_ParentHorizontalId = 4;
			AssertEquals("4", pivot.ParentHorizontalIdForBinding);

			pivot.GCD_ParentHorizontalId = 0;
			AssertEquals("", pivot.ParentHorizontalIdForBinding);

			pivot.GCD_ParentHorizontalId = 5;
			pivot.ParentHorizontalIdForBinding = "2";
			AssertEquals("2", pivot.ParentHorizontalIdForBinding);
			AssertEquals((byte)2, pivot.GCD_ParentHorizontalId);

			pivot.ParentHorizontalIdForBinding = "";
			AssertEquals("", pivot.ParentHorizontalIdForBinding);
			AssertEquals((byte)0, pivot.GCD_ParentHorizontalId);

			pivot.GCD_ParentHorizontalId = 5;
			pivot.ParentHorizontalIdForBinding = "zzz";
			AssertEquals("5", pivot.ParentHorizontalIdForBinding);
			AssertEquals((byte)5, pivot.GCD_ParentHorizontalId);
		}

		public void TestFilterRule()
		{
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = touch.PK;
			AssertNotNull(pivot.FilterRule);

			Factory.Save();

			var filter = new BusinessObjectFactory().Load<StmModuleFilter>(pivot.FilterRule.PK);

			AssertEquals("FRU", filter.S9_FilterType);
		}

		public void TestParentTouch()
		{
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_ParentTouch = touch.PK;

			AssertEquals(touch, pivot.ParentTouch);
		}

		public void TestNextTouch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);
			touch2a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;

			Factory.Save();

			AssertEquals(1, touch1a.TransitionRulesFromThisCampaign.Count);
			AssertEquals(touch2a, touch1a.TransitionRulesFromThisCampaign[0].NextTouches[0]);
			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK, touch1b.PK }, touch2a.TouchSourceCampaignPKs);
		}

		public void TestNextTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			var touch1c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1c.G0_HorizontalId = 1;
			touch1c.G0_VerticalId = "C";
			master.AllTouches.Add(touch1c);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);
			touch2a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2a.CurrentGroupColor = 1;

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			master.AllTouches.Add(touch2b);
			touch2b.CurrentGroupColor = 1;

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "A";
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch1c.PK;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.CurrentGroupColor = 2;

			var touch2d = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2d.G0_HorizontalId = 2;
			touch2d.G0_VerticalId = "A";
			master.AllTouches.Add(touch2d);
			touch2d.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch1c.PK;
			touch2d.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;

			var touch3a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "A";
			master.AllTouches.Add(touch3a);
			touch3a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;
			touch3a.CurrentGroupColor = 1;

			var touch3b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3b.G0_HorizontalId = 3;
			touch3b.G0_VerticalId = "B";
			master.AllTouches.Add(touch3b);
			touch3b.CurrentGroupColor = 1;

			var touch3c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3c.G0_HorizontalId = 3;
			touch3c.G0_VerticalId = "C";
			master.AllTouches.Add(touch3c);
			touch3c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch2c.PK;
			touch3c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;
			touch3c.CurrentGroupColor = 2;

			var touch3d = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3d.G0_HorizontalId = 3;
			touch3d.G0_VerticalId = "C";
			master.AllTouches.Add(touch3d);
			touch3d.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch2c.PK;
			touch3d.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2d, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3d, Factory);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK }, touch1a.TransitionRulesFromThisCampaign[0].NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK }, touch1b.TransitionRulesFromThisCampaign[0].NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK, touch2c.PK, touch2d.PK }, touch1c.TransitionRulesFromThisCampaign.SelectMany(r => r.NextTouches).Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2a.TransitionRulesFromThisCampaign[0].NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2b.TransitionRulesFromThisCampaign[0].NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK, touch3c.PK, touch3d.PK }, touch2c.TransitionRulesFromThisCampaign.SelectMany(r => r.NextTouches).Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2d.TransitionRulesFromThisCampaign[0].NextTouches.Select(t => t.PK));

			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK, touch1b.PK, touch1c.PK }, touch2a.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK, touch1b.PK, touch1c.PK }, touch2b.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch1c.PK }, touch2c.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch1c.PK }, touch2d.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK, touch2c.PK, touch2d.PK }, touch3a.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK, touch2c.PK, touch2d.PK }, touch3b.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch2c.PK }, touch3c.TouchSourceCampaignPKs);
			AssertContainsExactElementsInAnyOrder(new[] { touch2c.PK }, touch3d.TouchSourceCampaignPKs);
		}

		public void TestGroupResetsNextTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign1.G0_GCG_Group = group.PK;
			campaign2.G0_GCG_Group = group.PK;

			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK }, touch1a.TransitionRulesToThisCampaign[0].NextTouches.Select(t => t.PK));
			touch1a.G0_GCG_Group = group.PK;
			touch1a.TransitionRulesToThisCampaign[0].GCD_GCG_Group = group.PK;

			Factory.Save();

			touch1a = new BusinessObjectFactory().Load<GlbCompanyCampaign>(touch1a.PK);
			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK, campaign1.PK, campaign2.PK }, touch1a.TransitionRulesToThisCampaign[0].NextTouches.Select(t => t.PK));
		}

		public void TestNextTouchResetsGroup()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign1.G0_GCG_Group = group.PK;
			campaign2.G0_GCG_Group = group.PK;

			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK }, touch1a.TransitionRulesToThisCampaign[0].NextTouches.Select(t => t.PK));
			touch1a.G0_GCG_Group = group.PK;
			touch1a.TransitionRulesToThisCampaign[0].GCD_GCG_Group = group.PK;

			Factory.Save();

			touch1a = new BusinessObjectFactory().Load<GlbCompanyCampaign>(touch1a.PK);
			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK, campaign1.PK, campaign2.PK }, touch1a.TransitionRulesToThisCampaign[0].NextTouches.Select(t => t.PK));

			touch1a.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch = touch1a.PK;
			touch1a.G0_GCG_Group = ZGuid.Empty;

			touch1a.Factory.Save();
			touch1a = new BusinessObjectFactory().Load<GlbCompanyCampaign>(touch1a.PK);
			AssertContainsExactElementsInAnyOrder(new[] { touch1a.PK }, touch1a.TransitionRulesToThisCampaign[0].NextTouches.Select(t => t.PK));
		}

		public void TestGCD_G0_ParentTouch_ReadOnly()
		{
			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();

			AssertEquals(true, pivot.GCD_G0_ParentTouch_ReadOnly);

			pivot.GCD_ParentHorizontalId = 1;
			AssertEquals(false, pivot.GCD_G0_ParentTouch_ReadOnly);

			pivot.GCD_ParentHorizontalId = 0;
			AssertEquals(true, pivot.GCD_G0_ParentTouch_ReadOnly);

			pivot.ParentHorizontalIdForBinding = "3";
			AssertEquals(false, pivot.GCD_G0_ParentTouch_ReadOnly);
		}

		public void TestParentHorizontalIdForBinding_ReadOnly()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";

			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = touch.PK;

			Factory.Save();

			AssertEquals(false, pivot.ParentHorizontalIdForBinding_ReadOnly);

			pivot.GCD_G0_ParentTouch = master.PK;
			AssertEquals(true, pivot.ParentHorizontalIdForBinding_ReadOnly);

			pivot.GCD_G0_ParentTouch = ZGuid.NewZGuid();
			AssertEquals(false, pivot.ParentHorizontalIdForBinding_ReadOnly);
		}

		public void TestHumanReadableName()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";

			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = touch.PK;

			AssertEquals(master.HumanReadableName, pivot.HumanReadableName);
		}

		public void TestParentTouchSummary()
		{
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_EstimatedStartedDate = new ZDateTime(2004, 2, 2);
			touch.G0_Category = "Doc";
			touch.G0_Type = "Jam";
			touch.G0_CampaignName = "Name";

			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			AssertEquals("", pivot.ParentTouchSummary);

			pivot.GCD_G0_ParentTouch = touch.PK;

			AssertEquals("040202_Doc_Jam_Name", pivot.ParentTouchSummary);

			touch.G0_G0_Master = ZGuid.NewZGuid();
			touch.G0_HorizontalId = 1;
			AssertEquals("Touch ID attached when parent is a touch campaign.", "040202_Doc_Jam_Name_1", pivot.ParentTouchSummary);

			touch.G0_VerticalId = "A";
			AssertEquals("Touch ID attached when parent is a touch campaign.", "040202_Doc_Jam_Name_1A", pivot.ParentTouchSummary);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = touch.PK;

			return pivot;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
