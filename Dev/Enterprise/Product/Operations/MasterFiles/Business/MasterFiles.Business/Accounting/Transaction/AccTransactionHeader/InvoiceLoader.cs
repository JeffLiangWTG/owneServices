using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class InvoiceLoader
	{
		public InvoiceLoader(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			IncludeReversedTransaction = true;
			LimitToCurrentCompany = true;
		}

		public bool IncludeReversedTransaction { get; set; }

		public bool LimitToCurrentCompany { get; set; }

		public ZQuery GetInvoicesForUniqueRefQuery(IEnumerable<ZGuid> orgHeadersForInvoices, ZString uniqueRef)
		{
			ZQuery result;

			if (uniqueRef.IsEmpty)
			{
				result = ZQuery.NoResultQuery;
			}
			else
			{
				ZQuery filter = SingleConsolidatedARInvoiceRefFilter(uniqueRef);
				if (orgHeadersForInvoices != null)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgHeadersForInvoices);
				}

				result = filter;
			}

			return result;
		}

		public AccTransactionHeaderCollection GetInvoicesForUniqueRef(IEnumerable<ZGuid> orgHeadersForInvoices, ZString uniqueRef)
		{
			AccTransactionHeaderCollection invoices;
			invoices = new AccTransactionHeaderCollection(Factory, GetInvoicesForUniqueRefQuery(orgHeadersForInvoices, uniqueRef));
			invoices.Load();

			return invoices;
		}

		public AccTransactionHeaderCollection GetInvoicesForUniqueRef(ZString uniqueRef)
		{
			return GetInvoicesForUniqueRef(null, uniqueRef);
		}

		public DocumentWrapper[] GetWrappersForARInvoice(IEnumerable<ZGuid> orgHeadersForInvoices, ZString uniqueRef, bool useDocBuilderInvoice)
		{
			if (orgHeadersForInvoices != null)
			{
				var transactionHeaders = GetInvoicesForUniqueRef(orgHeadersForInvoices, uniqueRef);

				if (transactionHeaders.Count > 0)
				{
					var wrappers = new List<DocumentWrapper>();

					foreach (var transactionHeader in transactionHeaders)
					{
						if (useDocBuilderInvoice)
						{
							var invoiceWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, transactionHeader);

							if (invoiceWrappers != null)
							{
								wrappers.AddRange(invoiceWrappers);
							}
						}
						else
						{
							wrappers.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ARInvoice, transactionHeader));
						}
					}

					return wrappers.Count == 0 ? null : wrappers.ToArray<DocumentWrapper>();
				}
			}

			return null;
		}

		#region OutstandingARAPTransactions

		public AccTransactionHeaderCollection GetOutstandingARAPTransactions(params string[] transactionTypes)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			string[] ledgerTypes = new string[] {
				LedgerTypes.AccountsReceivable,
				LedgerTypes.AccountsPayable
			};

			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledgerTypes);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypes);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, ZDateTime.Empty);

			if (LimitToCurrentCompany)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			}
			else
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			}

			AccTransactionHeaderCollection resultCollection = new AccTransactionHeaderCollection(Factory, filter);
			resultCollection.Load();
			return resultCollection;
		}

		#endregion

		protected ZQuery SingleConsolidatedARInvoiceRefFilter(ZString uniqueRef)
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, uniqueRef);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			if (LimitToCurrentCompany)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK);
			}
			else
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			}

			if (!IncludeReversedTransaction)
			{
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			}
			filter.OrderBy = AccTransactionHeaderSchema.Constants.AH_PostDate;

			return filter;
		}

		protected BusinessObjectFactory Factory;
	}
}
