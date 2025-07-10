using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgClientAssignedStaffModuleFilter))]
	public class OrgClientAssignedStaffModuleFilterTest : ModuleTextFilterTest
	{
		public void TestClearAndIsEmpty()
		{
			var branch = Factory.New<GlbBranch>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "FRD";
			staff.GS_LoginName = "Fred.Bloggs";
			staff.GS_FullName = "Fred Bloggs";

			ClientAssignedStaffFilter.ClientType = "CNE";
			ClientAssignedStaffFilter.StaffRole = "SAL";
			ClientAssignedStaffFilter.AssignedStaff = staff.GS_Code;
			ClientAssignedStaffFilter.Department = "FES";
			ClientAssignedStaffFilter.ControllingBranch = branch.PK;
			AssertEquals(false, ClientAssignedStaffFilter.IsEmpty);
			AssertEquals(false, ClientAssignedStaffFilter.Query.IsEmpty);

			ClientAssignedStaffFilter.Clear();
			AssertEquals(true, ClientAssignedStaffFilter.IsEmpty);
			AssertEquals(true, ClientAssignedStaffFilter.Query.IsEmpty);

			AssertEquals("", ClientAssignedStaffFilter.ClientType);
			AssertEquals("", ClientAssignedStaffFilter.StaffRole);
			AssertEquals("", ClientAssignedStaffFilter.AssignedStaff);
			AssertEquals("", ClientAssignedStaffFilter.Department);
			AssertEquals(ZGuid.Empty, ClientAssignedStaffFilter.ControllingBranch);
		}

		public void TestClientTypeList()
		{
			string expected = GetExpectedClientTypeList();
			AssertEquals(expected, ClientAssignedStaffFilter.ClientTypeList.ElementsAsString);
		}

		protected virtual string GetExpectedClientTypeList()
		{
			return "CNR - Consignor\r\n" +
				"CNE - Consignee\r\n" +
				"LOC - Local Client\r\n" +
				"CPY - Controlling Customer";
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");
			var filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);

			var branch = Factory.New<GlbBranch>();

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.ClientType = "CNR";
			filter.StaffRole = "CON";
			filter.AssignedStaff = "XYZ";
			filter.Department = "FRT";
			filter.ControllingBranch = branch.PK;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (OrgClientAssignedStaffModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Client Type", "CNR", loadedFilter.ClientType);
			AssertEquals("Staff Role", "CON", loadedFilter.StaffRole);
			AssertEquals("Assigned Staff", "XYZ", loadedFilter.AssignedStaff);
			AssertEquals("Department", "FRT", loadedFilter.Department);
			AssertEquals("ControllingBranch", branch.PK, loadedFilter.ControllingBranch);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml_WhenSwitchUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var branch = Factory.New<GlbBranch>();
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.ClientType = "CNR";
			filter.StaffRole = "CON";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser;
			filter.Department = "FRT";
			filter.ControllingBranch = branch.PK;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				strip.FilterDescription = "";
				strip.Delete();

				var loadedFilter = (OrgClientAssignedStaffModuleFilter)filterStripBizO[filter.Description];
				filterStripBizO.LoadLayout(savedLayout);

				AssertEquals("Client Type", "CNR", loadedFilter.ClientType);
				AssertEquals("Staff Role", "CON", loadedFilter.StaffRole);
				AssertEquals("Assigned Staff", GlbStaff.CurrentUser.GS_Code, loadedFilter.AssignedStaff);
				AssertEquals("Department", "FRT", loadedFilter.Department);
				AssertEquals("ControllingBranch", branch.PK, loadedFilter.ControllingBranch);
			}
		}

		public void TestAssignedStaff_WhenSwitchUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.ClientType = "CNR";
			filter.StaffRole = "CON";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser;
			filter.AssignedStaff = GlbStaff.CurrentUser.GS_Code;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals(GlbStaff.CurrentUser.GS_Code, staff.GS_Code);

				filterStripBizO.LoadLayout(savedLayout);
				var filterStrips = filterStripBizO.FilterStrips;
				AssertEquals(1, filterStrips.Count);

				var savedFilter = (OrgClientAssignedStaffModuleFilter)filterStrips[0].CurrentModuleFilter;

				AssertEquals("Client Type", "CNR", savedFilter.ClientType);
				AssertEquals("Staff Role", "CON", savedFilter.StaffRole);
				AssertEquals("Comparison Operator", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser, savedFilter.ComparisonOperator);
				AssertEquals("Assigned Staff", GlbStaff.CurrentUser.GS_Code, savedFilter.AssignedStaff);
			}
		}

		#region Test Assigned Staff Comparison Operator

		public void TestAssignedStaff_WhenUsingComparisonOperator_AssignedStaffInfoMayBeReadOnly()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals("assigned staff should be editable when using the 'exact' operator", false, filter.AssignedStaffInfo.ReadOnly);
			filter.AssignedStaff = "abc";
			AssertEquals("abc", filter.AssignedStaff);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("assigned staff should be editable when using the 'not equal' operator", false, filter.AssignedStaffInfo.ReadOnly);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals(ZString.Empty, filter.AssignedStaff);
			AssertEquals("assigned staff should be read only when using the 'is blank' operator", true, filter.AssignedStaffInfo.ReadOnly);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals(ZString.Empty, filter.AssignedStaff);
			AssertEquals("assigned staff should be read only when using the 'is not blank' operator", true, filter.AssignedStaffInfo.ReadOnly);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
			AssertNotEquals(ZString.Empty, filter.AssignedStaff);
			AssertEquals("assigned staff should be read only when using the 'current user' operator", true, filter.AssignedStaffInfo.ReadOnly);

			filter.SupportsFiltersMatchComparisonOperator = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			AssertNotEquals(ZString.Empty, filter.AssignedStaff);
			AssertEquals("assigned staff should be editable when using the 'filters match' operator", false, filter.AssignedStaffInfo.ReadOnly);
		}

		public void TestAssignedStaffComparisonOperator_DefaultValue()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");

			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
		}

		public void TestAssignedStaff_WhenIsBlankOrIsNotBlankComparison_AssignedStaffIsEmpty()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Client Assigned Staff");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.AssignedStaff = "abc";
			AssertEquals("Precondition: AssignedStaff is not an empty string", "abc", filter.AssignedStaff);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("When using the IsBlank comparison operator, AssignedStaff is an empty string", ZString.Empty, filter.AssignedStaff);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.AssignedStaff = "abc";
			AssertEquals("Precondition: AssignedStaff is not an empty string", "abc", filter.AssignedStaff);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals("When using the IsNotBlank comparison operator, AssignedStaff is an empty string", ZString.Empty, filter.AssignedStaff);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgClientAssignedStaffModuleFilter("Test");
		}

		OrgClientAssignedStaffModuleFilter ClientAssignedStaffFilter
		{
			get
			{
				if (clientAssignedStaffFilter == null)
				{
					clientAssignedStaffFilter = (OrgClientAssignedStaffModuleFilter)GetNewBusinessObject();
				}

				return clientAssignedStaffFilter;
			}
		}
		OrgClientAssignedStaffModuleFilter clientAssignedStaffFilter;

		#endregion
	}
}
