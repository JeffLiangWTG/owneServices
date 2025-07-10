using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public class CustomsRulesFilterControlTest : ZFilterStripControlTest
	{
		public void TestGridColumns()
		{
			var collection = new CustomsRuleCollection(Factory);
			using (var control = new CustomsRulesFilterControl(collection, new CustomsRulesFilterStripBusinessObject()))
			{
				var grid = control.Grid;
				AssertEquals(15, grid.ColumnStyles.Count);

				var columnStyles = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "PermitHolder+OH_Code"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "PermitHolder+OH_FullName"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "CPH_RN_NKCountryCode"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "CPH_StartDate"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "CPH_EndDate"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "CPH_Type"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "CPH_PermitDescription"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "Company+GC_Code"));
				AssertEquals(true, columnStyles.Any(x => x.ColumnName == "Company+GC_Name"));
			}
		}
	}
}
