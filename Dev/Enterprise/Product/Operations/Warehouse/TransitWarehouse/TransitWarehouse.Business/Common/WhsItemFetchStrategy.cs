using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public abstract class WhsItemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsItemFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected abstract string JobFieldName { get; }

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var jobStatus = JobFieldName + "+" + nameof(JobHeader.JH_Status);
			var holdReason = JobFieldName + "+" + nameof(JobHeader.JH_HoldReason);
			var profitLossReason = JobFieldName + "+" + nameof(JobHeader.JH_ProfitLossReasonCode);
			var margin = JobFieldName + "+" + nameof(JobHeader.JH_TotalProfitRevenueMargin);

			if (columns.Any(column =>
				column.ColumnName == jobStatus ||
				column.ColumnName == holdReason ||
				column.ColumnName == profitLossReason ||
				column.ColumnName == margin))
			{
				AddFetchHintJobHeader();
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, BusinessObject.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}
	}
}
