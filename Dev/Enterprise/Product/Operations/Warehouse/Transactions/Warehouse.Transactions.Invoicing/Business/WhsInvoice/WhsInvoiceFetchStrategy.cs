using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsInvoiceFetchStrategy(WhsInvoice invoice)
			: base(invoice)
		{
		}

		WhsInvoice Invoice => (WhsInvoice)BusinessObject;

		#region FetchForViewCore

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_ProfitLossReasonCode):
					case nameof(WhsInvoice.JobHeader) + "+" + nameof(WhsInvoice.JobHeader.JH_TotalProfitRevenueMargin):
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

		#region FetchForFactorySaveBeforeTransaction

		protected override void FetchForFactorySaveBeforeTransactionCore()
		{
			base.FetchForFactorySaveBeforeTransactionCore();

			// tested in InvoiceingFormBasherTest.TestPerformance_Save
			if (Invoice.HasChanges)
			{
				var dockets = Invoice.GetAdditionalDocketsWithoutSetJobDefaults();

				if (dockets.Length > 0)
				{
					foreach (var docket in dockets)
					{
						var jobHeader = docket.JobHeader;
						if (jobHeader != null)
						{
							Factory.AddFetchHint(AccTransactionLinesSchema.AL_JH, jobHeader.PK);
						}
					}

					if (!ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate)
					{
						var processTasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, dockets.Select(d => d.PK)));
						foreach (var processTask in processTasks)
						{
							Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, processTask.PK);
						}
					}
				}
			}
		}

		#endregion
	}
}
