using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsStocktakeFetchStrategy(WhsStocktake whsStocktake)
			: base(whsStocktake)
		{
		}

		#region FetchForViewCore

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_ProfitLossReasonCode):
					case nameof(WhsStocktake.Job) + "+" + nameof(WhsStocktake.Job.JH_TotalProfitRevenueMargin):
						AddFetchHintJobHeader();
						break;
				}
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, WhsStocktake.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}

		#endregion

		#region Implementation

		WhsStocktake WhsStocktake
		{
			get { return (WhsStocktake)BusinessObject; }
		}

		#endregion
	}
}
