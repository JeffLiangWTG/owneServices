using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	sealed class JobMawbFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestProfitLossReasonColumn()
		{
			using (var filter = GetFilterControl())
			{
				var column = filter.FilteredGrid.GetColumnStyle("Job+JH_ProfitLossReasonCode");
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		[RequiresSTA]
		public void TestMarginColumn()
		{
			using (var filter = GetFilterControl())
			{
				var column = filter.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		JobMawbFilterControl GetFilterControl() => new (new JobMawbCollection(Factory), new JobMawbFilterBusinessObject());
	}
}
