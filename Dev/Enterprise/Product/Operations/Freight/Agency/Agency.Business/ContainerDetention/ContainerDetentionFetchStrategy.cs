using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	class ContainerDetentionFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ContainerDetentionFetchStrategy(ContainerDetention containerDetention)
			: base(containerDetention)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(c =>
				c.ColumnName == ContainerDetention.Schema.NC_Calc_Status ||
				c.ColumnName == $"{nameof(ContainerDetention.Job)}+{nameof(ContainerDetention.Job.JH_ProfitLossReasonCode)}" ||
				c.ColumnName == $"{nameof(ContainerDetention.Job)}+{nameof(ContainerDetention.Job.JH_TotalProfitRevenueMargin)}"))
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
