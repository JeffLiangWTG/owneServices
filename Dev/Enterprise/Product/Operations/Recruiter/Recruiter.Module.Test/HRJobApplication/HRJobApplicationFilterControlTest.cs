using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	public class HRJobApplicationFilterControlTest : TestCase
	{
		public void TestReferringColumns()
		{
			using (var filterControl = new HRJobApplicationFilterControl())
			{
				var grid = filterControl.Grid;
				AssertColumn(grid, "HP_SourceType", "Source", true);
				AssertColumn(grid, "HP_SourceDetails", "Source Details", false);
				AssertColumn(grid, "ReferringOrganisation+OH_Code", "Referring Org. Code", true);
				AssertColumn(grid, "ReferringOrganisation+OH_FullName", "Referring Org. Name", false);
				AssertColumn(grid, "ReferringPerson+PER_FullName", "Referring Person", true);
				AssertColumn(grid, "ReferringStaffCode", "Referring Staff", false);
				AssertColumn(grid, "ParsedDocuments", "Parsed Documents", true);
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string caption, bool visible)
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
			AssertNotNull(columnName + " column should exist", columnStyle);
			AssertEquals(columnName + " Caption", caption, columnStyle.Caption ?? columnStyle.CaptionResourceString?.Caption);
			AssertEquals(columnName + " Visible", visible, columnStyle.IsVisible);
		}
	}
}
