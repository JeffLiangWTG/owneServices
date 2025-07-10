using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UnpaidPaymentInAdvanceTransactionFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UnpaidPaymentInAdvanceTransactionFilter(IRelatedJobNumber relatedJobNumber)
			: base(new BusinessObjectFactory())
		{
			fJobNumber = relatedJobNumber.JobNumber;
		}

		readonly string[] fJobNumber;
		AccTransactionHeaderCollection fTransactions;
		const int batchSize = 50;
		public AccTransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new AccTransactionHeaderCollection(Factory);
					var chunks = fJobNumber.Where(x => !string.IsNullOrEmpty(x)).Chunk(batchSize);
					foreach (var jobNumbers in chunks)
					{
						if (jobNumbers.Any())
						{
							var filter = GetJobNumberFilter(jobNumbers);
							ApplyAdditionalFilters(filter);
							var transactions = Factory.Load<AccTransactionHeader>(filter);
							fTransactions.AddRange(transactions);
						}
					}

					fTransactions.SetReadOnlyIncludingChildren(true);
				}
				return fTransactions;
			}
		}

		public void ResetTransactions()
		{
			fTransactions = null;
		}

		public ZBool HasUnPaidPIAInvoices
		{
			get
			{
				return HasUnPaidPIAInvoicesCore(false, ZGuid.Empty);
			}
		}

		public ZBool HasUnPaidPIAInvoicesForOrg(ZGuid orgPK)
		{
			return HasUnPaidPIAInvoicesCore(true, orgPK);
		}

		ZBool HasUnPaidPIAInvoicesCore(bool includeOrgFilter, ZGuid orgPk)
		{
			var chunks = fJobNumber.Where(x => !string.IsNullOrEmpty(x)).Chunk(batchSize);
			foreach (var jobNumbers in chunks)
			{
				if (jobNumbers.Any())
				{
					var filter = GetJobNumberFilter(jobNumbers);
					ApplyAdditionalFilters(filter);
					if (includeOrgFilter)
					{
						filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_OH, orgPk);
					}
					if (Factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, filter))
					{
						return true;
					}
				}
			}

			return false;
		}

		ZQuery GetJobNumberFilter(IEnumerable<string> jobNumbers)
		{
			if (jobNumbers == null)
			{
				throw new ArgumentNullException(nameof(jobNumbers));
			}
			return new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, jobNumbers);
		}

		void ApplyAdditionalFilters(ZQuery filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_FullyPaidDate, null);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_InvoiceTerm, "PIA");
		}
	}
}
