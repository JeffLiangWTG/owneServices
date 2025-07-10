using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using CampaignContactFilterCategories = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories;

namespace Enterprise.MarketingManager.Module.Testing
{
	class GlbCompanyCampaignContactFilterModuleStrategyTest : TestCaseWithFactory
	{
		public void TestOrgRelatedPartiesFilter()
		{
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Factory.NewWithValidTestData<GlbCompanyCampaign>());
			GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(campaignFilter);
			var strategy = new GlbCompanyCampaignContactFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(campaignFilter.ModuleFilters, typeof(CampaignContact), Factory);

			ModuleTextFilter textFilter = (ModuleTextFilter)campaignFilter["Organization Name"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = org1.OH_FullName = "AAAAA";
			org2.OH_Code = org2.OH_FullName = "AAAAB";
			org3.OH_Code = org3.OH_FullName = "AAAAC";
			org4.OH_Code = org4.OH_FullName = "AAAAD";
			org5.OH_Code = org5.OH_FullName = "AAAAE";
			org6.OH_Code = org6.OH_FullName = "AAAAF";
			Factory.Save();
			var org1Contact = GetNewContact(org1);
			var org2Contact = GetNewContact(org2);
			var org3Contact = GetNewContact(org3);
			var org4Contact = GetNewContact(org4);
			var org5Contact = GetNewContact(org5);
			var org6Contact = GetNewContact(org6);
			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org5.PK, org6.PK);
			Factory.Save();
			var filter = (OrgRelatedPartiesModuleFilter)campaignFilter.ModuleFilters["Related Parties" + FilterModuleStrategy.UniqueSuffix];
			filter.IsActive = true;
			filter.PartyType = "MNG";
			var collection = new GlbCampaignContactCollection(Factory, campaignFilter.Filter);
			collection.Load();
			AssertEquals("Collection contains 3 contacts", 3, collection.Count);
			AssertEquals("Collection contains org 1 contact", true, collection.Contains(org1Contact));
			AssertEquals("Collection contains org 2 contact", true, collection.Contains(org2Contact));
			AssertEquals("Collection does not contains org 3 contact", false, collection.Contains(org3Contact));
			AssertEquals("Collection does not contains org 4 contact", false, collection.Contains(org4Contact));
			AssertEquals("Collection contains org 5 contact", true, collection.Contains(org5Contact));
			AssertEquals("Collection does not contains org 6 contact", false, collection.Contains(org6Contact));
		}

		OrgRelatedParty SetupRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid)
		{
			var party = Factory.NewWithValidTestData<OrgRelatedParty>();
			party.PR_OH_Parent = parentGuid;
			party.PR_OH_RelatedParty = relatedPartyGuid;
			party.PR_PartyType = "MNG";
			party.PR_SystemCreateTimeUtc = ZDateTime.Now;
			return party;
		}
		OrgContact GetNewContact(OrgHeader org)
		{
			var result = org.Contacts.AddNew();
			result.OC_ContactName = org.OH_Code;
			return result;
		}

		public void TestRunOnFilterControlInitialisation_WithCampaignContact()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripCommonControl(new GlbCompanyCampaignContactFilterBusinessObject(campaign)))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var strategy = new GlbCompanyCampaignContactFilterModuleStrategy();
				strategy.RunOnFilterControlInitialisation(filterControl, new GlbCampaignContactCollection(Factory));

				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<ZString>(),
					filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(info => info.ColumnName));

				var filterStrip = filterControl.AddNewFilterStrip();
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						typeof(SalesRelationActivityFilterHelper.RecentActivityDateFilterControlBuilder),
						typeof(SalesRelationActivityFilterHelper.HasSalesRelationFilterControlBuilder),
						typeof(OrgRelatedPartiesFilterHelper.OrgRelatedPartiesModuleFilterControlBuilder)
					},
					filterStrip.CustomFilterControlsBuilders.Select(builder => builder.GetType()));
			}
		}

		public void TestRunOnFilterControlInitialisation_WithNonCampaignContact()
		{
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripCommonControl(new GlbCompanyCampaignFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var strategy = new GlbCompanyCampaignContactFilterModuleStrategy();
				strategy.RunOnFilterControlInitialisation(filterControl, new GlbCompanyCampaignCollection(Factory));

				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<string>(),
					filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(info => info.ColumnName));

				var filterStrip = filterControl.AddNewFilterStrip();
				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<ZFilterStrip.CustomFilterControlsBuilder>(),
					filterStrip.CustomFilterControlsBuilders.Select(builder => builder.GetType()));
			}
		}

		public void TestRunOnModuleFiltersCreated_WithCampaignContact()
		{
			var moduleFilters = new ModuleFilterCollection();
			var strategy = new GlbCompanyCampaignContactFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(moduleFilters, typeof(CampaignContact), Factory);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					ParseDescriptionAndFilterCategory(SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate + FilterModuleStrategy.UniqueSuffix, CampaignContactFilterCategories.Inquiries),
					ParseDescriptionAndFilterCategory(SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation + FilterModuleStrategy.UniqueSuffix, CampaignContactFilterCategories.Inquiries),
					ParseDescriptionAndFilterCategory("Campaign Tracking_" + SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate + FilterModuleStrategy.UniqueSuffix, CampaignContactFilterCategories.CampaignTracking),
					ParseDescriptionAndFilterCategory("Campaign Tracking_" + SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation + FilterModuleStrategy.UniqueSuffix, CampaignContactFilterCategories.CampaignTracking),
					ParseDescriptionAndFilterCategory("Organization_" + SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate + FilterModuleStrategy.UniqueSuffix, FilterCategories.SalesRelationActivity),
					ParseDescriptionAndFilterCategory("Organization_" + SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation + FilterModuleStrategy.UniqueSuffix, FilterCategories.SalesRelationActivity),
					ParseDescriptionAndFilterCategory("Related Parties" + FilterModuleStrategy.UniqueSuffix, FilterCategories.RelationshipOrgAndStaff),
				},
				moduleFilters.Select(filter => ParseDescriptionAndFilterCategory(filter.Description, filter.Category)));
		}

		static string ParseDescriptionAndFilterCategory(string description, FilterCategory filterCategory)
		{
			return string.Format("{0} {1}", description, filterCategory.Description);
		}

		public void TestRunOnModuleFiltersCreated_WithNonCampaignContact()
		{
			var moduleFilters = new ModuleFilterCollection();
			var strategy = new GlbCompanyCampaignContactFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(moduleFilters, typeof(GlbCompanyCampaign), Factory);

			AssertContainsExactElementsInAnyOrder(
				Enumerable.Empty<string>(),
				moduleFilters.Select(filter => filter.Description));
		}
	}
}
