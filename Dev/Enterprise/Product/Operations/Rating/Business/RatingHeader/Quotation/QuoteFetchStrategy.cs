using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	public class QuoteFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public QuoteFetchStrategy(Quote quote)
			: base(quote)
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

