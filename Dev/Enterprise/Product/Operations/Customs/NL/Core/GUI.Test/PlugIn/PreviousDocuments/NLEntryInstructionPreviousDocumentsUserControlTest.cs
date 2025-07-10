using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

public class NLEntryInstructionPreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestColumnCount()
	{
		using (var control = new NLEntryInstructionPreviousDocumentsUserControl())
		{
			var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			AssertEquals("Count", 2, grid.ColumnStyles.Count);
		}
	}

	public void TestColumnVisibility() => CombineAssertions(() =>
	{
		using (var control = new NLEntryInstructionPreviousDocumentsUserControl())
		{
			var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			Assert("The type should be visible.", FindColumnByName(grid, "CSI_Code").IsVisible);
			Assert("The reference should be visible.", FindColumnByName(grid, "CSI_ReferenceNumber").IsVisible);
			AssertNull("The class should not be visible.", FindColumnByName(grid, "CSI_SubType"));
			AssertNull("The date of issue should not be visible.", FindColumnByName(grid, "CSI_DateOfIssue"));
			AssertNull("The line number should not be visible.", FindColumnByName(grid, "CSI_LineNo"));
		}
	});

	ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
}
