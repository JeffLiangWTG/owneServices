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
	[TestedType(typeof(OrgContactsActiveStatusAndInfoModuleFilter))]
	sealed class OrgContactsActiveStatusAndInfoModuleFilterTest : ModuleTextFilterTest
	{
		public void TestFilter_All()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TEST TEST TEST";
			var orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Person A";
			orgContact1.OC_IsActive = false;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TEST TEST TEST";
			var orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Person B";
			orgContact2.OC_IsActive = true;

			Factory.Save();

			var filter = new OrganisationFilterBusinessObject();

			((ModuleTextFilter)filter["Name"]).Property = "TEST TEST TEST";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var contactFilter = filter["Contact Name"] as OrgContactsActiveStatusAndInfoModuleFilter;
			AssertNotNull("Filter should exist", contactFilter);

			contactFilter.IsActive = true;
			contactFilter.Property = string.Empty;
			contactFilter.ActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusInactive;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals("Contact filter is empty due to property is empty", 2, collection.Count);

			contactFilter.Property = "Person A";
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertCollectionContains("Get the org1 with inactive address", org1.PK, collection.GetPKs());

			contactFilter.Property = "Person B";
			contactFilter.ActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusActive;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertCollectionContains("Get the org2 with active address", org2.PK, collection.GetPKs());

			contactFilter.Property = "Person";
			contactFilter.ActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusAll;
			collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, collection.GetPKs());
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new OrgContactsActiveStatusAndInfoModuleFilter("TEST");

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.ActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusInactive;
			filter.Property = "ABC";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = string.Empty;
			strip.Delete();

			var loadedFilter = (OrgContactsActiveStatusAndInfoModuleFilter)filterStripBizO[filter.Description];
			filterStripBizO.LoadLayout(savedLayout);
			AssertEquals("Active Status", OrgContactsActiveStatusAndInfoModuleFilter.StatusInactive, loadedFilter.ActiveStatus);
			AssertEquals("Contact Name", "ABC", loadedFilter.Property);
		}

		public void TestActiveStatusPairList()
		{
			var codePairList = new CodeDescriptionPairList();
			codePairList.AddPair("Active", "Show Active Only");
			codePairList.AddPair("Inactive", "Show Inactive Only");
			codePairList.AddPair("All", "Show all records");
			var filter = new OrgContactsActiveStatusAndInfoModuleFilter("TEST");
			AssertContainsExactElementsInAnyOrder(codePairList, filter.ActiveStatusList);
		}

		public void TestConstructor_SetsDefaultActiveStatus()
		{
			var filter = new OrgContactsActiveStatusAndInfoModuleFilter("TEST");
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusActive, filter.DefaultActiveStatus);

			var filter2 = new OrgContactsActiveStatusAndInfoModuleFilter("TEST", (SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery());
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusActive, filter2.DefaultActiveStatus);
		}

		public void TestClear_UseDefaultActiveStatus()
		{
			var filter = new OrgContactsActiveStatusAndInfoModuleFilter("TEST");
			filter.DefaultActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusAll;
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusAll, filter.ActiveStatus);
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusAll, filter.DefaultActiveStatus);

			filter.ActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusInactive;
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusInactive, filter.ActiveStatus);
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusAll, filter.DefaultActiveStatus);

			filter.Clear();
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusAll, filter.ActiveStatus);
			AssertEquals(OrgContactsActiveStatusAndInfoModuleFilter.StatusAll, filter.DefaultActiveStatus);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgContactsActiveStatusAndInfoModuleFilter("TEST");
		}

		#endregion
	}
}
