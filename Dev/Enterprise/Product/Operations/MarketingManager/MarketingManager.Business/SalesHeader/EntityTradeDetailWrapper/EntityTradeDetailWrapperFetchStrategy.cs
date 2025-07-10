using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradeDetailWrapperFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public EntityTradeDetailWrapperFetchStrategy(EntityTradeDetailWrapper entityTradeDetail)
			: base(entityTradeDetail)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var entityTradeDetail = (EntityTradeDetailWrapper)BusinessObject;
			if (columns.Any(col => EstimateValueProperties.Contains(col.ColumnName)))
			{
				Factory.AddFetchHint(OrgTradeProspectSchema.PAP_PA, entityTradeDetail.PK);
				Factory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, entityTradeDetail.PK);
			}
		}

		HashSet<string> EstimateValueProperties
		{
			get
			{
				if (estimateValueProperties == null)
				{
					estimateValueProperties = new HashSet<string>
					{
						EntitySalesWrapper.Schema.TotalEstimatedAnnualValue,
						EntitySalesWrapper.Schema.CommittedAnnualValue,
						EntitySalesWrapper.Schema.CommittedMonthlyValue,
						EntitySalesWrapper.Schema.PipelineValue,
						EntitySalesWrapper.Schema.UnsuccessfulValue
					};
				}

				return estimateValueProperties;
			}
		}
		HashSet<string> estimateValueProperties;

		#endregion
	}
}
