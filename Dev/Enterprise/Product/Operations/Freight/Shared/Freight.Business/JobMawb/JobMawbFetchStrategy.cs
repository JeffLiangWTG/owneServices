using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobMawbFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobMawbFetchStrategy(JobMawb mawb)
			: base(mawb)
		{
		}

		JobMawb Mawb => BusinessObject as JobMawb;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case JobMawb.Schema.JM_Calc_Airline2LetterCode:
						var filter = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, Mawb.JM_Airline3DigitPrefix);
						Factory.AddFetchHint(typeof(RefAirline), filter);
						break;

					case $"{nameof(CommonConsol.Job)}+{nameof(CommonConsol.Job.JH_ProfitLossReasonCode)}":
					case $"{nameof(CommonConsol.Job)}+{nameof(CommonConsol.Job.JH_TotalProfitRevenueMargin)}":
						AddFetchHintJobHeader();
						break;
				}
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, Mawb.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}
	}
}
