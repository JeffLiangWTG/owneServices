using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class StaffAssignmentsFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new StaffAssignmentsFilterControl(new DummyBusinessObjectCollection(Factory), new StaffAssignmentsFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>();

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "Header+OH_Code", expectedIsVisible: true);
					AssertHasColumn(columns, "Header+OH_FullName", expectedIsVisible: true);
					AssertHasColumn(columns, "O8_Role", expectedIsVisible: true);
					AssertHasColumn(columns, "RoleDescription", expectedIsVisible: true);
					AssertHasColumn(columns, "O8_GS_NKPersonResponsible", expectedIsVisible: true);
					AssertHasColumn(columns, "ResponsiblePersonName", expectedIsVisible: true);
					AssertHasColumn(columns, "ResponsiblePersonLoginName", expectedIsVisible: false);
					AssertHasColumn(columns, "O8_Department", expectedIsVisible: true);
					AssertHasColumn(columns, "O8_GC", expectedIsVisible: true);
				});
			}
		}

		void AssertHasColumn(IEnumerable<ZGridColumnInfo> columns, string columnName, bool expectedIsVisible)
		{
			var column = columns.FirstOrDefault(c => c.ColumnName == columnName);
			if (column == null)
			{
				Fail($"Should have the column - {columnName}");
			}
			else
			{
				AssertEquals($"IsVisible - {columnName}", expectedIsVisible, column.IsVisible);
			}
		}
	}
}
