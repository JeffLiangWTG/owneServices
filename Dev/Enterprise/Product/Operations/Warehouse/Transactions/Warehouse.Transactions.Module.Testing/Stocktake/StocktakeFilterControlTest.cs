using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	sealed class StocktakeFilterControlTest : TestCaseWithFactory
	{
		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var profitLossReason = nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_ProfitLossReasonCode);
				var column = filterControl.FilteredGrid.GetColumnStyle(profitLossReason);
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filter = GetFilterControl())
			{
				var margin = nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_TotalProfitRevenueMargin);
				var column = filter.FilteredGrid.GetColumnStyle(margin);
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		StocktakeFilterControl GetFilterControl() => new (new WhsStocktakeCollection(Factory), new StocktakeFilterBusinessObject());
	}
}
