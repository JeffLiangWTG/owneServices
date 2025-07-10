using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignContactModule))]
	public class GlbCompanyCampaignContactModuleTest : ZModuleBasherTest
	{
		public virtual void TestDripMarketingModuleName()
		{
			using (var module = new GlbCompanyCampaignContactModuleForTest())
			{
				AssertEquals(ExpectedModuleName, module.DripMarketingFilterRuleModuleName_Exposed);
			}
		}

		class GlbCompanyCampaignContactModuleForTest : GlbCompanyCampaignContactModule
		{
			public ZString DripMarketingFilterRuleModuleName_Exposed
			{
				get { return DripMarketingFilterRuleModuleName; }
			}
		}

		protected virtual ZString ExpectedModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRule.Name;
			}
		}

		public void TestModuleIDAndSupportWorkflow()
		{
			using (GlbCompanyCampaignContactModule module = new GlbCompanyCampaignContactModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaignContact, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaignContact;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new GlbCompanyCampaignContactModuleRecentItemForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		[StressTest]
		public void TestSearchRecordsFoundMessage_BatchCountLessThanTotal()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = 100;
			campaign.G0_DeDuplicateContacts = false;

			using (GlbCompanyCampaignContactModule module = new GlbCompanyCampaignContactModule())
			{
				GlbCampaignContactCollection collection = new GlbCampaignContactCollection(campaign);
				collection.Load();
				module.Campaign = campaign;

				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(((GlbCompanyCampaignContactFilterControl)module.EmbeddedControl).FilterBusinessObject);
				((GlbCompanyCampaignContactFilterControl)module.EmbeddedControl).Find();
				AssertEquals("Expected Records found message", string.Format(GlbCompanyCampaignContactModule.NotificationConstants.BatchMatchingRecordsMessage, 100), module.SearchRecordsFoundMessage);
			}
		}

		[StressTest]
		public void TestSearchRecordsFoundMessage_NoResults()
		{
			using (GlbCompanyCampaignContactModule module = new GlbCompanyCampaignContactModule())
			{
				GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

				GlbCampaignContactCollection collection = new GlbCampaignContactCollection(campaign);
				module.Campaign = campaign;
				var filterBizO = module.FilterBusinessObject;

				campaign.G0_BatchCountDefault = collection.Count + 100;

				var result = (ModuleTextFilter)filterBizO["Email Address"];
				result.Property = "doesnotexistsindatabase@yahoo.com";
				result.SqlComparisonOperator = SQLComparisonOperator.Equal;
				result.IsActive = true;

				((GlbCompanyCampaignContactFilterControl)module.EmbeddedControl).Find();
				AssertEquals("Filtered contacts should not be loaded", 0, campaign.FilteredContacts.Count);
				campaign.AdditionalFilter = filterBizO.Filter;
				AssertEquals("Expected Records found message", GlbCompanyCampaign.NotificationConstants.NoMatchingRecordsMessage + " Duplicate contacts excluded.", module.SearchRecordsFoundMessage);
			}
		}

		[StressTest]
		public override void TestModuleShowsAndCanSearch()
		{
			base.TestModuleShowsAndCanSearch();
		}

		protected override void PrepareModuleForUserDefinedFilterTest(ZFilterModule module)
		{
			((GlbCompanyCampaignContactModule)module).Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
		}
	}
}
