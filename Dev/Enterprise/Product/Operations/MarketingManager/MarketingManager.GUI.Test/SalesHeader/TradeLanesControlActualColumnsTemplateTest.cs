using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLanesControlActualColumnsTemplateTest : TestCaseWithFactory
	{
		public void TestAddActualColumnStyles()
		{
			using (var grid = new ZGrid())
			{
				var existingColumnInfo = new ZTextBoxColumnStyleInfo();
				grid.ColumnStyles.Add(existingColumnInfo);

				TradeLanesControlActualColumnsTemplate.AddActualColumnStyles(grid);

				var existingColumnInfoIndex = grid.ColumnStyles.IndexOf(existingColumnInfo);
				AssertNotEquals("existingColumnInfo should still exist", -1, existingColumnInfoIndex);

				using (var template = new TradeLanesControlActualColumnsTemplate())
				{
					var expectedPreColumns = template.PreColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					var actualPreColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Take(existingColumnInfoIndex).Select(x => x.ColumnName).ToArray();
					AssertMultilineASCIIEquals("Should have inserted PreColumns before the existingColumns in grid", string.Join(System.Environment.NewLine, expectedPreColumns), string.Join(System.Environment.NewLine, actualPreColumns));

					var expectedPostColumns = template.PostColumnsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					var actualPostColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Skip(existingColumnInfoIndex + 1).Select(x => x.ColumnName).ToArray();
					AssertMultilineASCIIEquals("Should have appended PostColumns after the existingColumns in grid", string.Join(System.Environment.NewLine, expectedPostColumns), string.Join(System.Environment.NewLine, actualPostColumns));
				}
			}
		}
	}
}
