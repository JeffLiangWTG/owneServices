using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.Business
{
	public class SalesDashboardActivityFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SalesDashboardActivityFetchStrategy(SalesDashboardActivity activity)
			: base(activity)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);
		}

		#endregion
	}
}
