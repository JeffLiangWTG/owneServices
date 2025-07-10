using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SalesEnquiryFetchStrategy(SalesEnquiry inquiry)
			: base(inquiry)
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
