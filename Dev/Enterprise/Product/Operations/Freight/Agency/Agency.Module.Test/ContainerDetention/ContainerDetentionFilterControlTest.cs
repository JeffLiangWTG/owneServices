using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Module.Test.ContainerDetention
{
	sealed class ContainerDetentionFilterControlTest : TestCaseWithFactory
	{
		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_ProfitLossReasonCode");
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		ContainerDetentionFilterControl GetFilterControl() => new (new ContainerDetentionCollection(Factory), new ContainerDetentionFilterStrip());
	}
}
