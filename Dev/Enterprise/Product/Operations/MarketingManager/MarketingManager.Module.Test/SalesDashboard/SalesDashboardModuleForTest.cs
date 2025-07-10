using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Module.Testing
{
	class SalesDashboardModuleForTest : SalesDashboardModule
	{
		public bool ShowRecentExposed
		{
			get { return ShowRecentItems; }
		}

		public ZQuery GetDisplayResultsQueryForTest() => GetDisplayResultsQuery();
	}
}
