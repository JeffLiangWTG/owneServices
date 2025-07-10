using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyCampaignFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GlbCompanyCampaignFetchStrategy(GlbCompanyCampaign campaign)
			: base(campaign)
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
