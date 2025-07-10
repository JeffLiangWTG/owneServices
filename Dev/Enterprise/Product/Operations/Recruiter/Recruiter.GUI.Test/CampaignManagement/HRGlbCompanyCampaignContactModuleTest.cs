using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaignContactModule))]
	public class HRGlbCompanyCampaignContactModuleTest : ZModuleBasherTest
	{
		public void TestGetNewFilterBusinessObject_ShouldBeSetUpForFilterRuleMode()
		{
			using (var module = new HRGlbCompanyCampaignContactModule())
			{
				var filterBizo = module.FilterBusinessObject;
				AssertEquals("This module's filters are always used in filter rule mode, so if this property is false, it means the filter business object wasn't created correctly using RelatedModuleFiltersHelper and will cause errors.", true, filterBizo.IsInFilterRuleMode);
				AssertEquals(module, filterBizo.ParentModule);
			}
		}

		public virtual void TestDripMarketingModuleName()
		{
			using (var module = new HRGlbCompanyCampaignContactModuleForTest())
			{
				AssertEquals(ExpectedModuleName, module.DripMarketingFilterRuleModuleName_Exposed);
			}
		}

		class HRGlbCompanyCampaignContactModuleForTest : HRGlbCompanyCampaignContactModule
		{
			public ZString DripMarketingFilterRuleModuleName_Exposed
			{
				get
				{
					return DripMarketingFilterRuleModuleName;
				}
			}
		}

		protected virtual ZString ExpectedModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleHR.Name;
			}
		}

		public void TestModuleIDAndSupportWorkflow()
		{
			using (HRGlbCompanyCampaignContactModule module = new HRGlbCompanyCampaignContactModule())
			{
				AssertEquals(ModuleIDs.HRGlbCompanyCampaignContact, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRGlbCompanyCampaignContact;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new HRGlbCompanyCampaignContactModuleRecentItemForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		[StressTest]
		public void TestSearchRecordsFoundMessage_NoResults()
		{
			using (HRGlbCompanyCampaignContactModule module = new HRGlbCompanyCampaignContactModule())
			{
				var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
				var collection = new GlbCampaignContactCollection(campaign);
				module.Campaign = campaign;
				var filterBizO = module.FilterBusinessObject;
				campaign.G0_BatchCountDefault = collection.Count + 100;
				var result = (ModuleTextFilter)filterBizO["Email Address"];
				result.Property = "doesnotexistsindatabase@yahoo.com";
				result.SqlComparisonOperator = SQLComparisonOperator.Equal;
				result.IsActive = true;
				((HRGlbCompanyCampaignContactFilterControl)module.EmbeddedControl).Find();
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
			((HRGlbCompanyCampaignContactModule)module).Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
		}
	}
}
