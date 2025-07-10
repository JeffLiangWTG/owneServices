using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgSalesCallFetchStrategy(OrgSalesCall orgSalesCall)
			: base(orgSalesCall)
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
