using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.Freight.LocalCartage.Module.Test.Cartage
{
	class CartageFilterControlTest : TestCaseWithFactory
	{
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new CartageFilterControl(new ModuleCartageCollection(Factory), new CartageFilterBusinessObject()))
			{
				var holdReason = nameof(CommonCartage.Job) + "+" + nameof(CommonCartage.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = new CartageFilterControl(new ModuleCartageCollection(Factory), new CartageFilterBusinessObject()))
			{
				var profitLossReason = nameof(CommonCartage.Job) + "+" + nameof(CommonCartage.Job.JH_ProfitLossReasonCode);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == profitLossReason && !col.IsVisible);

				Assert("Profit/Loss Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = new CartageFilterControl(new ModuleCartageCollection(Factory), new CartageFilterBusinessObject()))
			{
				var margin = nameof(CommonCartage.Job) + "+" + nameof(CommonCartage.Job.JH_TotalProfitRevenueMargin);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == margin && !col.IsVisible);

				Assert("Margin% column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}
	}
}
