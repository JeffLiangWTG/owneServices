using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
{
	public void TestAvailableColumns()
	{
		using (var control = new EntryInstructionGridUserControl())
		{
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");

			CombineAssertions(() =>
			{
				AssertEquals("Count", 4, grid.ColumnStyles.Count);
				AssertEquals("CEI_SubStyle", ((ZDropEditColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("CEI_DateForDuty", ((ZDateEditColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("CEI_Procedure", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("CEI_Description", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
			});
		}
	}
}
