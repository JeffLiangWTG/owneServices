using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntitySalesWrapperFetchStrategy : OrgSalesFetchStrategy
	{
		public EntitySalesWrapperFetchStrategy(EntitySalesWrapper entitySales)
			: base(entitySales)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var entitySales = (EntitySalesWrapper)BusinessObject;
			if (columns.Any(col => EstimateValueProperties.Contains(col.ColumnName)))
			{
				Factory.AddFetchHint(OrgTradeDetailSchema.PA_OW, entitySales.PK);
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
