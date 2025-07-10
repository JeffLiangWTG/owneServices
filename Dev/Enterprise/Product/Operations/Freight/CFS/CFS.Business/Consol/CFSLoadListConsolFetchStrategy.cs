using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public sealed class CFSLoadListConsolFetchStrategy : CommonConsolFetchStrategy
	{
		public CFSLoadListConsolFetchStrategy(CommonConsol consol) : base(consol)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_Status):
					case nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_HoldReason):
					case nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_ProfitLossReasonCode):
					case nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_TotalProfitRevenueMargin):
						AddFetchHintJobHeader();
						break;
				}
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
