using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManagementRoleWrapper))]
	sealed class GlbStaffManagementRoleWrapperTest : GlbStaffManagementTreeBizObjWrapperBaseTestCase<GlbStaffManagementRoleWrapper>
	{
		public void TestRole()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "HRM", (NoResString)"Human Resources Manager", true, false, true },
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var wrapper = new GlbStaffManagementRoleWrapper(TreeModel, Staff, SystemDataRegistry.Instance.StaffReportingRoles.Value[0]);
			AssertEquals(SystemDataRegistry.Instance.StaffReportingRoles.Value.GetDescriptionFromCode(RoleManager.GSM_ManagerType), wrapper.Role);
		}

		public void TestJobTitle()
		{
			var wrapper = new GlbStaffManagementRoleWrapper(TreeModel, Staff, SystemDataRegistry.Instance.StaffReportingRoles.Value[0]);
			AssertEquals(ZString.Empty, wrapper.JobTitle);
		}

		public void TestEffectiveDate()
		{
			var wrapper = new GlbStaffManagementRoleWrapper(TreeModel, Staff, SystemDataRegistry.Instance.StaffReportingRoles.Value[0]);
			AssertEquals(ZString.Empty, wrapper.EffectiveDate);
		}

		public void TestLocation()
		{
			var wrapper = new GlbStaffManagementRoleWrapper(TreeModel, Staff, SystemDataRegistry.Instance.StaffReportingRoles.Value[0]);
			AssertEquals(ZString.Empty, wrapper.Branch);
		}

		[TestDate(2019, 05, 03)]
		public void TestChildren()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var treeModel = new GlbStaffManagementTreeModel(staff);

			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "PRM", (NoResString)"Payroll Manager", true, false, true },
				{ "FNM", (NoResString)"Fun Manager", true, false, true },
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
			var payrollManager1 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager1.GS_FullName = "PR Manager 1";
			payrollManager1.GS_Title = "Money Launderer";
			StaffManagerTestHelper.AddManager(staff, payrollManager1, "PRM", new ZDateTime(2019, 01, 20), ZDateTime.Now.AddDays(-1));
			var payrollManager2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager2.GS_FullName = "PR Manager 2";
			payrollManager2.GS_Title = "Penny Wise";
			StaffManagerTestHelper.AddManager(staff, payrollManager2, "PRM");

			var funManager1 = Factory.NewWithValidTestData<GlbStaff>();
			funManager1.GS_FullName = "FN Manager 1";
			funManager1.GS_Title = "My Man";
			StaffManagerTestHelper.AddManager(staff, funManager1, "FNM", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			var funManager2 = Factory.NewWithValidTestData<GlbStaff>();
			funManager2.GS_FullName = "FN Manager 2";
			funManager2.GS_Title = "My Man";
			StaffManagerTestHelper.AddManager(staff, funManager2, "FNM");

			var prmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode("PRM"));
			AssertEquals(1, prmWrapper.Children.Length);
			AssertEquals("Penny Wise", prmWrapper.Children[0].JobTitle);

			var fnmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode("FNM"));
			AssertEquals(2, fnmWrapper.Children.Length);
			AssertEquals("My Man", fnmWrapper.Children[0].JobTitle);
			AssertEquals("My Man", fnmWrapper.Children[1].JobTitle);
		}

		public void TestDirectReportChildren()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var treeModel = new GlbStaffManagementTreeModel(staff);

			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "PRM", (NoResString)"Payroll Manager", true, false, true },
				{ "FNM", (NoResString)"Fun Manager", true, false, true },
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
			var payrollDirectReport1 = Factory.NewWithValidTestData<GlbStaff>();
			payrollDirectReport1.GS_FullName = "PR Report 1";
			payrollDirectReport1.GS_Code = "PR1";
			StaffManagerTestHelper.AddManager(payrollDirectReport1, staff, "PRM", new ZDateTime(2019, 01, 20), ZDateTime.Now.AddDays(-1));

			var payrollDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollDirectReport2.GS_FullName = "PR Report 2";
			payrollDirectReport2.GS_Code = "PR2";
			StaffManagerTestHelper.AddManager(payrollDirectReport2, staff, "PRM");

			var funDirectReport1 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport1.GS_FullName = "FN Report 1";
			funDirectReport1.GS_Code = "FN1";
			StaffManagerTestHelper.AddManager(funDirectReport1, staff, "FNM", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			var funDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport2.GS_FullName = "FN Report 2";
			funDirectReport1.GS_Code = "FN2";
			StaffManagerTestHelper.AddManager(funDirectReport2, staff, "FNM");

			var prmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode("PRM"), isManager: true);
			AssertEquals(1, prmWrapper.Children.Length);
			AssertNotEquals(null, prmWrapper.Children.FirstOrDefault(x => x.Role == $"{payrollDirectReport2.GS_FullName} ({payrollDirectReport2.GS_Code})"));

			var fnmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode("FNM"), isManager: true);
			AssertEquals(2, fnmWrapper.Children.Length);
			AssertNotNull(fnmWrapper.Children.FirstOrDefault(x => x.Role == $"{funDirectReport1.GS_FullName} ({funDirectReport1.GS_Code})"));
			AssertNotNull(fnmWrapper.Children.FirstOrDefault(x => x.Role == $"{funDirectReport2.GS_FullName} ({funDirectReport2.GS_Code})"));
		}

		public void TestDirectManagerChildren()
		{
			var roleCollection = new StaffReportingRoleCollection
			{
				{ DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, true, false, false }
			};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roleCollection);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "The child";
			var treeModel = new GlbStaffManagementTreeModel(staff);

			var directManager1 = Factory.NewWithValidTestData<GlbStaff>();
			directManager1.GS_FullName = "Manager 1";
			directManager1.GS_Code = "COD";

			var directManager2 = Factory.NewWithValidTestData<GlbStaff>();
			directManager2.GS_FullName = "Manager 2";
			directManager2.GS_Code = "CO2";

			StaffManagerTestHelper.AddManager(staff, directManager1, "DRM", ZDateTime.Today.AddDays(-1), ZDateTime.Today);
			StaffManagerTestHelper.AddManager(staff, directManager2, "DRM", ZDateTime.Today.AddDays(1), ZDateTime.Empty);

			var drmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(DefaultStaffReportingRoles.Codes.DirectManager));
			AssertEquals(1, drmWrapper.Children.Length);
			AssertNotNull(drmWrapper.Children.FirstOrDefault(x => x.Role == $"{directManager2.GS_FullName} ({directManager2.GS_Code})"));
		}

		public void TestDirectManagerDirectReportChildren()
		{
			var roleCollection = new StaffReportingRoleCollection();
			roleCollection.Add(DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, true, false, false);
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roleCollection);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "The parent";
			var treeModel = new GlbStaffManagementTreeModel(staff);

			var directReport1 = Factory.NewWithValidTestData<GlbStaff>();
			directReport1.GS_FullName = "DirectReport 1";
			directReport1.GS_Code = "COD";

			var directReport2 = Factory.NewWithValidTestData<GlbStaff>();
			directReport2.GS_FullName = "DirectReport 2";
			directReport2.GS_Code = "CO2";

			StaffManagerTestHelper.AddManager(directReport1, staff, "DRM", ZDateTime.Today.AddDays(-1), ZDateTime.Today);
			StaffManagerTestHelper.AddManager(directReport2, staff, "DRM", ZDateTime.Today.AddDays(1), ZDateTime.Empty);

			var drmWrapper = new GlbStaffManagementRoleWrapper(treeModel, staff, (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(DefaultStaffReportingRoles.Codes.DirectManager), isManager: true);
			AssertEquals(1, drmWrapper.Children.Length);
			AssertNotNull(drmWrapper.Children.FirstOrDefault(x => x.Role == $"{directReport2.GS_FullName} ({directReport2.GS_Code})"));
		}

		#region Overrides

		protected override GlbStaffManagementRoleWrapper GetNewWrapper(GlbStaffManagementTreeModel treeModel, IEnumerable<GlbStaffManagementTreeBizObjWrapperBase> children)
		{
			return new GlbStaffManagementRoleWrapper(treeModel, Staff, SystemDataRegistry.Instance.StaffReportingRoles.Value[0]);
		}

		protected override GlbStaffManagementTreeBizObjWrapperBase GetNewChildWrapper(GlbStaffManagementTreeModel treeModel)
		{
			return null;
		}

		#endregion
	}
}
