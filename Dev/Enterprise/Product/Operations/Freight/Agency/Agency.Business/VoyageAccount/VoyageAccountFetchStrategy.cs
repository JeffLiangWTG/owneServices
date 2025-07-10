using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	class VoyageAccountFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public VoyageAccountFetchStrategy(VoyageAccount voyageAccount)
			: base(voyageAccount)
		{
		}

		VoyageAccount VoyageAccount => BusinessObject as VoyageAccount;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var jobVoyageRequired = false;
			var orgHeaderRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case VoyageAccount.Schema.NA_Calc_Vessel:
					case VoyageAccount.Schema.NA_Calc_Voyage:
						jobVoyageRequired = true;
						break;

					case VoyageAccount.Schema.NA_Calc_Description:
						jobVoyageRequired = true;
						orgHeaderRequired = true;
						break;

					case $"{nameof(VoyageAccount.Job)}+{nameof(VoyageAccount.Job.JH_ProfitLossReasonCode)}":
					case $"{nameof(VoyageAccount.Job)}+{nameof(VoyageAccount.Job.JH_TotalProfitRevenueMargin)}":
						AddFetchHintJobHeader();
						break;
				}
			}

			if (jobVoyageRequired)
			{
				Factory.AddFetchHint(JobVoyageSchema.PK, VoyageAccount.NA_JV);
			}

			if (orgHeaderRequired)
			{
				Factory.AddFetchHint(typeof(OrgHeader), VoyageAccount.NA_OH);
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
