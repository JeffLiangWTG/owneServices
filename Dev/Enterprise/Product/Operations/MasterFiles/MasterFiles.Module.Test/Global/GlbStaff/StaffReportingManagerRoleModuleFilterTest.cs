using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StaffReportingManagerRoleModuleFilter))]
	sealed class StaffReportingManagerRoleModuleFilterTest : ModuleTextFilterTest
	{
		[TestDate(2019, 06, 11)]
		public void TestFilter_Exact()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";

			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_Code = "ONE";
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager2.GS_Code = "TWO";

			StaffManagerTestHelper.AddManager(staff, manager1, "HRM");
			StaffManagerTestHelper.AddManager(staff, manager2, "PRM");

			Factory.Save();

			var filterBizO = new GlbStaffFilterBusinessObject();
			var filter = filterBizO["Reporting Manager"] as StaffReportingManagerRoleModuleFilter;
			filter.IsActive = true;

			filter.ReportingRole = "TRM";

			var query = filterBizO.Filter;
			var collection = new GlbStaffCollection(Factory);
			collection.AdditionalFilter = query;

			AssertEquals("There should be no items in the list", 0, collection.Count);

			filter.ReportingRole = "HRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("NEO", collection[0].GS_Code);

			filter.ReportingRole = "PRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("NEO", collection[0].GS_Code);

			filter.Manager = manager1.PK;
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be no items in the list", 0, collection.Count);

			filter.Manager = manager2.PK;
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("NEO", collection[0].GS_Code);

			filter.ReportingRole = string.Empty;
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("NEO", collection[0].GS_Code);
		}

		[TestDate(2019, 06, 11)]
		public void TestFilter_NotEquals()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "WEO";

			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_Code = "ONE";
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager2.GS_Code = "TWO";

			StaffManagerTestHelper.AddManager(staff, manager1, "HRM");
			StaffManagerTestHelper.AddManager(staff, manager2, "PRM");
			StaffManagerTestHelper.AddManager(staff2, manager2, "PRM");

			Factory.Save();

			var filterBizO = new GlbStaffFilterBusinessObject();
			var filter = filterBizO["Reporting Manager"] as StaffReportingManagerRoleModuleFilter;
			filter.IsActive = true;
			filter.ComparisonOperator = StaffReportingManagerRoleModuleFilter.ComparisonConstants.NotEqual;

			filter.ReportingRole = "TRM";

			var query = filterBizO.Filter;
			var collection = new GlbStaffCollection(Factory);
			collection.AdditionalFilter = query;

			AssertEquals("There should be no items in the list", 0, collection.Count);

			filter.ReportingRole = "HRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("NEO", collection[0].GS_Code);

			filter.Manager = manager1.PK;
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be no items in the list", 0, collection.Count);

			filter.Manager = manager1.PK;
			filter.ReportingRole = "PRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("WEO", collection[0].GS_Code);
		}

		public void TestFilter_WithIsBlankOperator()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";

			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_Code = "ONE";
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager2.GS_Code = "TWO";

			StaffManagerTestHelper.AddManager(staff, manager1, "HRM");
			StaffManagerTestHelper.AddManager(staff, manager2, "PRM");

			Factory.Save();

			var filterBizO = new GlbStaffFilterBusinessObject();
			var filter = filterBizO["Reporting Manager"] as StaffReportingManagerRoleModuleFilter;
			filter.IsActive = true;
			filter.ComparisonOperator = StaffReportingManagerRoleModuleFilter.ComparisonConstants.IsBlank;

			filter.ReportingRole = "TRM";

			((ModuleTextFilter)filterBizO["Login Name"]).Property = "CWSupport";
			((ModuleTextFilter)filterBizO["Login Name"]).IsActive = true;
			((ModuleTextFilter)filterBizO["Login Name"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			var query = filterBizO.Filter;
			var collection = new GlbStaffCollection(Factory);
			collection.AdditionalFilter = query;

			AssertEquals("staff, manager1 and manager2", 3, collection.Count);

			filter.ReportingRole = "HRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("manager1 and manager2", 2, collection.Count);

			filter.ReportingRole = "PRM";
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("manager1 and manager2", 2, collection.Count);

			filter.Manager = manager1.PK;
			query = filterBizO.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("Shouldn't change result", 2, collection.Count);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";

			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_Code = "ONE";
			var filterStripBizO = new DummyFilterStripBusinessObject();

			var filterBizO = new GlbStaffFilterBusinessObject();
			var filter = filterBizO["Reporting Manager"] as StaffReportingManagerRoleModuleFilter;

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.Manager = manager1.PK;
			filter.ReportingRole = "TRM";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (StaffReportingManagerRoleModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Manager", manager1.PK, loadedFilter.Manager);
			AssertEquals("Reporting Role", "TRM", loadedFilter.ReportingRole);
		}

		public void TestStaffRolesList()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var filter = GetNewBusinessObject() as StaffReportingManagerRoleModuleFilter;
			AssertContainsExactElementsInAnyOrder(SystemDataRegistry.Instance.StaffReportingRoles.Value, filter.ReportingRolesList);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new StaffReportingManagerRoleModuleFilter("Test");
		}

		#endregion
	}
}
