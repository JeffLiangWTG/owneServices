using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgOpportunityFetchStrategy(OrgOpportunity opportunity)
			: base(opportunity)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(x => x.ColumnName == "SourceCampaign"))
			{
				Factory.AddFetchHint(ViewCampaignChildNodeSchema.Instance, new ZQuery(ViewCampaignChildNodeSchema.CCN_ActivityID, BusinessObject.PK));
			}

			SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);
		}

		#endregion
	}
}
