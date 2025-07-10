using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.MasterFiles.GUI.Test;

public class SimilarOrgsDisplayGridTest : TestCaseWithFactory
{
	public void TestSimilarOrgsDisplayGrid_HasNecessaryColumns()
	{
		using var grid = new SimilarOrgsDisplayGrid();
		var columnStyles = grid.ColumnStyles.OfType<ZGridColumnInfo>();
		CombineAssertions(() =>
		{
			var zGridColumnInfos = columnStyles as ZGridColumnInfo[] ?? columnStyles.ToArray();
			AssertEquals(13, grid.ColumnStyles.Count);
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OS_Rank"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Code"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_FullName"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_Address1"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_Address2"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_City"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_State"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_PostCode"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OS_UNLOCO"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_Phone"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_Fax"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "LocalBusinessNumber"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "OH_Calc_Email"));
		});
	}
}


