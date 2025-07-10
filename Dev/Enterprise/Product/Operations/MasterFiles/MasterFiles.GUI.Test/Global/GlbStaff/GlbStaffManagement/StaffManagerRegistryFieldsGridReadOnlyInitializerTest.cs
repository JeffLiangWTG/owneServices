using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffManagerRegistryFieldsGridReadOnlyInitializerTest : TestCaseWithFactory
	{
		public void TestAddStaffCustomFieldsColumns()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			using (ZGrid grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });

				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);

				StaffManagerRegistryFieldsGridReadOnlyInitializer.AddStaffCustomFieldsColumns(grid, new GlbStaffCollection(Factory), true);

				AssertEquals(5, grid.ColumnStyles.Count);

				AssertEqualColumn("Abc", "_abc", (ZGridColumnInfo)grid.ColumnStyles[0], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());

				AssertEqualColumn(DefaultStaffReportingRoles.Descriptions.DirectManager, DefaultStaffReportingRoles.Codes.DirectManager, (ZGridColumnInfo)grid.ColumnStyles[1], typeof(StaffManagerCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());

				AssertEqualColumn("Disabled Manager", "TRM", (ZGridColumnInfo)grid.ColumnStyles[2], typeof(StaffManagerCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[2].GetType());

				AssertEqualColumn("Human Resources Manager", "HRM", (ZGridColumnInfo)grid.ColumnStyles[3], typeof(StaffManagerCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[3].GetType());

				AssertEqualColumn("Payroll Manager", "PRM", (ZGridColumnInfo)grid.ColumnStyles[4], typeof(StaffManagerCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[4].GetType());
			}
		}

		public void TestSecurity()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var loginUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(loginUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });

				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);

				Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed = false;
				StaffManagerRegistryFieldsGridReadOnlyInitializer.AddStaffCustomFieldsColumns(grid, new GlbStaffCollection(Factory), true);

				AssertEquals("Security denied", 1, grid.ColumnStyles.Count);

				Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed = true;
				Env.Security.StaffViewOtherDirectReports.IsAllowed = false;
				StaffManagerRegistryFieldsGridReadOnlyInitializer.AddStaffCustomFieldsColumns(grid, new GlbStaffCollection(Factory), true);

				AssertEquals("Security allowed", 5, grid.ColumnStyles.Count);

				Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed = false;
				Env.Security.StaffViewOtherDirectReports.IsAllowed = true;
				StaffManagerRegistryFieldsGridReadOnlyInitializer.AddStaffCustomFieldsColumns(grid, new GlbStaffCollection(Factory), true);

				AssertEquals("Security allowed", 5, grid.ColumnStyles.Count);
			}
		}

		internal static void AssertEqualColumn(string caption, string columnName, ZGridColumnInfo columnStyle, Type propertyDescriptorType)
		{
			AssertEquals(caption, columnStyle.Caption);
			AssertEquals(columnName, columnStyle.ColumnName);
			Assert(columnStyle.GroupName.IsEmpty());

			if (propertyDescriptorType != null)
			{
				AssertEquals(propertyDescriptorType, ((IOverridablePropertyDescriptor)columnStyle).PropertyDescriptor.GetType());
			}
			else
			{
				AssertNull(((IOverridablePropertyDescriptor)columnStyle).PropertyDescriptor);
			}
		}
	}
}
