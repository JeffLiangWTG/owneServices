using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAddressWithActiveStatusModuleTextFilter))]
	sealed class OrgAddressActiveStatusAndInfoModuleFilterTest : ModuleTextFilterTest
	{
		public void TestIsBlankOrgAddressWithActiveStatusModuleTextFilterShouldIncludeActiveStatusFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TEST TEST";
			org1.MainAddress.Postcode = string.Empty;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TEST TEST";
			org2.MainAddress.OA_PostCode = "DEF";
			var orgAddress = org2.Addresses.AddNew();
			orgAddress.OA_Address1 = "org2 added address";
			orgAddress.OA_PostCode = string.Empty;
			orgAddress.OA_IsActive = false;

			Factory.Save();
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, org1.Addresses.Count);
				AssertEquals(true, org1.Addresses.Any(a => ((OrgAddress)a).OA_PostCode == string.Empty && ((OrgAddress)a).OA_IsActive));

				AssertEquals(2, org2.Addresses.Count);
				AssertEquals(true, org2.Addresses.Any(a => ((OrgAddress)a).OA_PostCode != string.Empty && ((OrgAddress)a).OA_IsActive));
				AssertEquals(true, org2.Addresses.Any(a => ((OrgAddress)a).OA_PostCode == string.Empty && !((OrgAddress)a).OA_IsActive));
			});

			var filter = new OrganisationFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "TEST TEST";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var postcodeFilter = filter["Post Code"] as OrgAddressWithActiveStatusModuleTextFilter;
			postcodeFilter.IsActive = true;
			postcodeFilter.ComparisonOperator = OrgAddressWithActiveStatusModuleTextFilter.ComparisonConstants.IsBlank;
			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(org2.PK, collection.GetPKs().Single());

			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusActive;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(org1.PK, collection.GetPKs().Single());

			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, collection.GetPKs());
		}

		public void TestIsNotBlankOrgAddressWithActiveStatusModuleTextFilterShouldIncludeActiveStatusFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TEST TEST";
			org1.MainAddress.Postcode = "ABC";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TEST TEST";
			org2.MainAddress.OA_PostCode = string.Empty;
			var orgAddress = org2.Addresses.AddNew();
			orgAddress.OA_Address1 = "org2 added address";
			orgAddress.OA_PostCode = "456";
			orgAddress.OA_IsActive = false;

			Factory.Save();
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, org1.Addresses.Count);
				AssertEquals(true, org1.Addresses.Any(a => ((OrgAddress)a).OA_PostCode != string.Empty && ((OrgAddress)a).OA_IsActive));

				AssertEquals(2, org2.Addresses.Count);
				AssertEquals(true, org2.Addresses.Any(a => ((OrgAddress)a).OA_PostCode == string.Empty && ((OrgAddress)a).OA_IsActive));
				AssertEquals(true, org2.Addresses.Any(a => ((OrgAddress)a).OA_PostCode != string.Empty && !((OrgAddress)a).OA_IsActive));
			});

			var filter = new OrganisationFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "TEST TEST";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var postcodeFilter = filter["Post Code"] as OrgAddressWithActiveStatusModuleTextFilter;
			postcodeFilter.IsActive = true;
			postcodeFilter.ComparisonOperator = OrgAddressWithActiveStatusModuleTextFilter.ComparisonConstants.IsNotBlank;
			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(org2.PK, collection.GetPKs().Single());

			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusActive;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals(org1.PK, collection.GetPKs().Single());

			postcodeFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, collection.GetPKs());
		}

		public void TestFilter_All()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TEST TEST TEST";
			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address WoW";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TEST TEST TEST";
			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "AABBCC";
			orgAddress2.OA_Address2 = "Test Address WoW";
			orgAddress2.OA_IsActive = false;

			Factory.Save();

			var filter = new OrganisationFilterBusinessObject();

			((ModuleTextFilter)filter["Name"]).Property = "TEST TEST TEST";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var addressFilter = filter["Address"] as OrgAddressWithActiveStatusModuleTextFilter;
			AssertNotNull("Filter should exist", addressFilter);

			addressFilter.IsActive = true;
			addressFilter.Property = string.Empty;
			addressFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals("Address filter is empty due to property is empty", 2, collection.Count);

			addressFilter.Property = "Test Address WoW";
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertCollectionContains("Get the org2 with inactive address", org2.PK, collection.GetPKs());

			addressFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusActive;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertCollectionContains("Get the org1 with active address", org1.PK, collection.GetPKs());

			addressFilter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, collection.GetPKs());
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new OrgAddressWithActiveStatusModuleTextFilter("TEST");

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			filter.Property = "ABC";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = string.Empty;
			strip.Delete();

			var loadedFilter = (OrgAddressWithActiveStatusModuleTextFilter)filterStripBizO[filter.Description];
			filterStripBizO.LoadLayout(savedLayout);
			AssertEquals("Active Status", OrgAddressWithActiveStatusModuleTextFilter.StatusInactive, loadedFilter.ActiveStatus);
			AssertEquals("Address", "ABC", loadedFilter.Property);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				loadedFilter = (OrgAddressWithActiveStatusModuleTextFilter)filterStripBizO[filter.Description];
				filterStripBizO.LoadLayout(savedLayout);
				AssertEquals("Active Status", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusInactive].Code, loadedFilter.ActiveStatus);
				AssertEquals("Address", "ABC", loadedFilter.Property);
			}
		}

		public void TestActiveStatusFilterQuery()
		{
			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();

			var filterBizo = new OrganisationFilterBusinessObject();
			var addressFilters = filterBizo.ModuleFilters.OfType<OrgAddressWithActiveStatusModuleTextFilter>();
			foreach (var addressFilter in addressFilters)
			{
				allLanguages.ForEach(lan => AssertActiveStatusFilterQuery(filterBizo, addressFilter, lan));
			}
		}

		void AssertActiveStatusFilterQuery(OrganisationFilterBusinessObject filterBizo, OrgAddressWithActiveStatusModuleTextFilter filter, string language)
		{
			filter.IsActive = true;
			filter.Property = "Test";

			using (Res.TemporarilySwitchLanguage(language))
			{
				filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusActive;
				AssertContains("OA_IsActive = 1".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

				filter.ActiveStatus = (ZString)OrgAddressWithActiveStatusModuleTextFilter.StatusActive.GetUnresolvedValue();
				AssertContains("OA_IsActive = 1".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

				filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
				AssertContains("OA_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

				filter.ActiveStatus = (ZString)OrgAddressWithActiveStatusModuleTextFilter.StatusInactive.GetUnresolvedValue();
				AssertContains("OA_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

				filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
				AssertNotContains("OA_IsActive = 1 or OA_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());

				filter.ActiveStatus = (ZString)OrgAddressWithActiveStatusModuleTextFilter.StatusAll.GetUnresolvedValue();
				AssertNotContains("OA_IsActive = 1 or OA_IsActive = 0".ToUpper(), filterBizo.Filter.LiteralTextADO.ToUpper());
			}
		}

		public void TestActiveStatusPairList()
		{
			var codePairList = new CodeDescriptionPairList();
			codePairList.AddPair("Active", "Show Active Only");
			codePairList.AddPair("Inactive", "Show Inactive Only");
			codePairList.AddPair("All", "Show all records");
			var filter = new OrgAddressWithActiveStatusModuleTextFilter("TEST");
			AssertContainsExactElementsInAnyOrder(codePairList, filter.ActiveStatusList);
		}

		public void TestConstructor_SetsDefaultActiveStatus()
		{
			var filter = new OrgAddressWithActiveStatusModuleTextFilter("TEST");
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusActive, filter.DefaultActiveStatus);

			var filter2 = new OrgAddressWithActiveStatusModuleTextFilter("TEST", (SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery());
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusActive, filter2.DefaultActiveStatus);
		}

		public void TestClear_UseDefaultActiveStatus()
		{
			var filter = new OrgAddressWithActiveStatusModuleTextFilter("TEST");
			filter.DefaultActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusAll, filter.ActiveStatus);
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusAll, filter.DefaultActiveStatus);

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusInactive, filter.ActiveStatus);
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusAll, filter.DefaultActiveStatus);

			filter.Clear();
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusAll, filter.ActiveStatus);
			AssertEquals(OrgAddressWithActiveStatusModuleTextFilter.StatusAll, filter.DefaultActiveStatus);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgAddressWithActiveStatusModuleTextFilter("TEST");
		}

		#endregion
	}
}
