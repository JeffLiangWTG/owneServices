using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	class SundryChargesFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SundryChargesFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(c =>
				c.ColumnName == $"{nameof(SundryCharges.Job)}+{nameof(SundryCharges.Job.JH_ProfitLossReasonCode)}" ||
				c.ColumnName == $"{nameof(SundryCharges.Job)}+{nameof(SundryCharges.Job.JH_TotalProfitRevenueMargin)}"))
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
