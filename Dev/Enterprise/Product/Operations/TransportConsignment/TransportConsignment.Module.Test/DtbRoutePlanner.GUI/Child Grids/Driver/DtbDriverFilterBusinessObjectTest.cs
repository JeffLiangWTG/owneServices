using System;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbDriverFilterBusinessObject))]
	public class DtbDriverFilterBusinessObjectTest : DtbChildFilterBusinessObjectTest
	{
		#region TestDriverBranch

		public void TestDriverBranch()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driver2 = Helper.CreateDriver("JIM", "JIM");
			var driver3 = Helper.CreateDriver("TOM", "TOM");
			var driver4 = Helper.CreateDriver("BEN", "BEN");
			var driversGroup = Helper.CreateDriverGroup("Drivers", driver1, driver2, driver3, driver4);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;

			driver1.GS_GB_HomeBranch = branch1.PK;
			driver2.GS_GB_HomeBranch = branch2.PK;
			driver3.GS_GB_HomeBranch = branch3.PK;
			driver4.GS_GB_HomeBranch = branch3.PK;

			Asserter.AddToScope(driver1);
			Asserter.AddToScope(driver2);
			Asserter.AddToScope(driver3);
			Asserter.AddToScope(driver4);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var branchFilter = (ModuleGuidFilter)filterBizO[DtbDriverFilterBusinessObject.FilterConstants.Branch];
			AssertEquals(DtbDriverFilterBusinessObject.FilterConstants.DriverCategory, branchFilter.Category);

			branchFilter.IsActive = true;
			branchFilter.Property = branch1.PK;
			Asserter.AssertMatches("driver1 has branch of branch1.", filterBizO.Filter, driver1);

			branchFilter.Property = branch2.PK;
			Asserter.AssertMatches("driver2 has branch of branch2.", filterBizO.Filter, driver2);

			branchFilter.Property = branch3.PK;
			Asserter.AssertMatches("driver3, driver4 has branch of branch3.", filterBizO.Filter, driver3, driver4);
		}

		#endregion

		#region TestDriverDepartment

		public void TestDriverDepartment()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driver2 = Helper.CreateDriver("JIM", "JIM");
			var driver3 = Helper.CreateDriver("TOM", "TOM");
			var driver4 = Helper.CreateDriver("BEN", "BEN");
			var driversGroup = Helper.CreateDriverGroup("Drivers", driver1, driver2, driver3, driver4);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();

			driver1.GS_GE_HomeDepartment = department1.PK;
			driver2.GS_GE_HomeDepartment = department2.PK;
			driver3.GS_GE_HomeDepartment = department3.PK;
			driver4.GS_GE_HomeDepartment = department3.PK;

			Asserter.AddToScope(driver1);
			Asserter.AddToScope(driver2);
			Asserter.AddToScope(driver3);
			Asserter.AddToScope(driver4);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var departmentFilter = (ModuleGuidFilter)filterBizO[DtbDriverFilterBusinessObject.FilterConstants.Department];
			AssertEquals(DtbDriverFilterBusinessObject.FilterConstants.DriverCategory, departmentFilter.Category);

			departmentFilter.IsActive = true;
			departmentFilter.Property = department1.PK;
			Asserter.AssertMatches("driver1 has department of department1.", filterBizO.Filter, driver1);

			departmentFilter.Property = department2.PK;
			Asserter.AssertMatches("driver2 has department of department2.", filterBizO.Filter, driver2);

			departmentFilter.Property = department3.PK;
			Asserter.AssertMatches("driver3, driver4 has department of department3.", filterBizO.Filter, driver3, driver4);
		}

		#endregion

		#region TestDriverState

		public void TestDriverState()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driver2 = Helper.CreateDriver("JIM", "JIM");
			var driver3 = Helper.CreateDriver("TOM", "TOM");
			var driver4 = Helper.CreateDriver("BEN", "BEN");
			var driversGroup = Helper.CreateDriverGroup("Drivers", driver1, driver2, driver3, driver4);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;

			driver1.GS_State = "QLD";
			driver2.GS_State = "VIC";
			driver3.GS_State = "NSW";
			driver4.GS_State = "NSW";

			Asserter.AddToScope(driver1);
			Asserter.AddToScope(driver2);
			Asserter.AddToScope(driver3);
			Asserter.AddToScope(driver4);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var stateFilter = (ModuleTextFilter)filterBizO[DtbDriverFilterBusinessObject.FilterConstants.State];
			AssertEquals(DtbDriverFilterBusinessObject.FilterConstants.DriverCategory, stateFilter.Category);

			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			Asserter.AssertMatches("driver1 is in QLD.", filterBizO.Filter, driver1);

			stateFilter.Property = "VIC";
			Asserter.AssertMatches("driver2 is in VIC.", filterBizO.Filter, driver2);

			stateFilter.Property = "NSW";
			Asserter.AssertMatches("driver3, driver4 are in NSW.", filterBizO.Filter, driver3, driver4);
		}

		#endregion

		#region TestOrCategoryFilters

		public void TestOrCategoryFilters()
		{
			var driver1 = Helper.CreateDriver("BOB", "BOB");
			var driver2 = Helper.CreateDriver("JIM", "JIM");
			var driver3 = Helper.CreateDriver("TOM", "TOM");
			var driver4 = Helper.CreateDriver("BEN", "BEN");
			var driversGroup = Helper.CreateDriverGroup("Drivers", driver1, driver2, driver3, driver4);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;
			driver1.GS_State = "QLD";
			driver2.GS_State = "VIC";
			driver3.GS_State = "NSW";
			Asserter.AddToScope(driver1, driver2, driver3);

			Factory.Save();

			var filterBizO = new DtbRoutePlannerFilterBusinessObject();
			var childFilterBizO = new DtbDriverFilterBusinessObject();
			filterBizO.AddChildFilterBusinessObject(childFilterBizO);

			var stateFilter = (ModuleTextFilter)filterBizO[DtbDriverFilterBusinessObject.FilterConstants.State];
			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			stateFilter.OrCategory = FilterOrCategory.Red;

			var stateFilter2Description = stateFilter.Description + " (1) ";
			var stateFilter2 = new ModuleTextFilter(stateFilter2Description, GlbStaffSchema.GS_State);
			stateFilter2.Category = DtbDriverFilterBusinessObject.FilterConstants.DriverCategory;
			stateFilter2.IsActive = true;
			stateFilter2.Property = "VIC";
			stateFilter2.OrCategory = FilterOrCategory.Red;
			filterBizO.ModuleFilters.AddFilter(stateFilter2);

			Asserter.AssertMatches("should return both of carrier1  and carrier2.", childFilterBizO.Filter, driver1, driver2);

			var stateFilter3Description = stateFilter.Description + " (2) ";
			var stateFilter3 = new ModuleTextFilter(stateFilter3Description, GlbStaffSchema.GS_State);
			stateFilter3.Category = DtbDriverFilterBusinessObject.FilterConstants.DriverCategory;
			stateFilter3.IsActive = true;
			stateFilter3.Property = "NSW";
			stateFilter3.OrCategory = FilterOrCategory.Green;
			filterBizO.ModuleFilters.AddFilter(stateFilter3);
			Asserter.AssertMatches("should not return any carrier.", childFilterBizO.Filter);
		}

		#endregion

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		protected FilterStripAsserter<GlbStaff> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<GlbStaff>(Factory, d => d.GS_Code)); }
		}

		FilterStripAsserter<GlbStaff> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbDriverFilterBusinessObject();
		}

		protected override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(GlbStaff); }
		}

		protected override ModuleTextFilter DuplicateFilter(string description)
		{
			var stateFilterDescription = description + " (1) ";
			return new ModuleTextFilter(stateFilterDescription, GlbStaffSchema.GS_State);
		}

		protected override FilterCategory FilterCategoryOfChildFilter
		{
			get { return DtbDriverFilterBusinessObject.FilterConstants.DriverCategory; }
		}

		protected override string ChildFilterName
		{
			get { return DtbDriverFilterBusinessObject.FilterConstants.State; }
		}

		protected override DtbChildFilterBusinessObject GetNewChildFilterBusinessObject
		{
			get { return (DtbChildFilterBusinessObject)GetNewFilterStripBusinessObject(); }
		}

		protected override SchemaColumn ExpectedFieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver; }
		}

		protected override SchemaColumn ExpectedChildBizOPKOrNK
		{
			get { return GlbStaffSchema.GS_Code; }
		}

		#endregion
	}
}
