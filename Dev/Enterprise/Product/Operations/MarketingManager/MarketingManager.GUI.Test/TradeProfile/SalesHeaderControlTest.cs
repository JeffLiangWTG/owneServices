using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class SalesHeaderControlTest : TestCaseWithFactory
	{
		#region Properties

		public void TestShowTradedColumns()
		{
			using (var control = new SalesHeaderControl())
			{
				AssertEquals(true, control.ShowTradedColumns);

				var unavailableColumnNames = new HashSet<string>(control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsUnavailable).Select(x => x.ColumnName));
				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<string>(),
					unavailableColumnNames);

				control.ShowTradedColumns = false;
				unavailableColumnNames = new HashSet<string>(control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsUnavailable).Select(x => x.ColumnName));
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						SalesHeader.Schema.TradedMonthlyAverage,
						SalesHeader.Schema.TradedAnnualTotal,
						SalesHeader.Schema.TradedAnnualTEUTotalQuantity
					},
					unavailableColumnNames);
			}
		}

		#endregion
	}
}
