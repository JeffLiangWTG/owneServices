using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PeriodicInvoicingFetchStrategy(PeriodicInvoicing invoice)
			: base(invoice)
		{
		}

		PeriodicInvoicing Invoice => (PeriodicInvoicing)BusinessObject;

		#region FetchForViewCore

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(PeriodicInvoicing.JobHeader) + "+" + nameof(PeriodicInvoicing.JobHeader.JH_ProfitLossReasonCode):
					case nameof(PeriodicInvoicing.JobHeader) + "+" + nameof(PeriodicInvoicing.JobHeader.JH_TotalProfitRevenueMargin):
						AddFetchHintJobHeader();
						break;
				}
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, Invoice.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}

		#endregion
	}
}
