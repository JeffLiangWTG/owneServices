using System;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OutstandingCashAdvanceFilter
	{
		public static bool HasOutstandingARCashAdvances(string[] jobNumbers) => HasOutstandingARCashAdvancesCore(jobNumbers, ZGuid.Empty);

		public static bool HasOutstandingARCashAdvancesForOrg(string[] jobNumbers, ZGuid orgPK) => HasOutstandingARCashAdvancesCore(jobNumbers, orgPK);

		static bool HasOutstandingARCashAdvancesCore(string[] jobNumbers, ZGuid orgPK)
		{
			const int batchSize = 50;
			var factory = new BusinessObjectFactory();
			var chunks = jobNumbers.Where(x => !string.IsNullOrEmpty(x)).Chunk(batchSize);

			foreach (var jobChunk in chunks)
			{
				var jobs = jobChunk.ToArray();

				if (jobs.Any())
				{
					var requestedOrPartiallyPaidCashAdvanceQuery = CreateRequestedOrPartiallyPaidCashAdvanceQuery(jobs, orgPK);
					if (factory.ExistsInDatabase(AccCashAdvanceRequestHeaderSchema.Constants.TableName, requestedOrPartiallyPaidCashAdvanceQuery))
					{
						return true;
					}

					var pendingCashAdvanceQuery = CreatePendingCashAdvanceQuery(jobs, orgPK);
					if (factory.ExistsInDatabase(JobChargeSchema.Constants.TableName, pendingCashAdvanceQuery))
					{
						return true;
					}
				}
			}

			return false;
		}

		static ZDBOnlyQuery CreateRequestedOrPartiallyPaidCashAdvanceQuery(string[] jobNumbers, ZGuid orgPK)
		{
			var outstandingHeaderStatuses = new[] { CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceStatusCodes.RequestHeader.PartiallyPaid };
			var jobHeaderQuery = CreateJobHeaderQuery(jobNumbers);
			var cashAdvanceHeaderQuery = new ZDBOnlyQuery(typeof(AccCashAdvanceRequestHeader));
			cashAdvanceHeaderQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_GC_Company, GlbCompany.CurrentCompany.PK);
			cashAdvanceHeaderQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_Ledger, LedgerTypes.AccountsReceivable);
			cashAdvanceHeaderQuery.AddSubQuery(AccCashAdvanceRequestHeaderSchema.CAH_JH_Job, jobHeaderQuery, JoinCondition.And);
			if (!orgPK.IsEmpty)
			{
				cashAdvanceHeaderQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_OH_Organization, orgPK);
			}
			cashAdvanceHeaderQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_Status, outstandingHeaderStatuses);
			return cashAdvanceHeaderQuery;
		}

		static ZDBOnlyQuery CreatePendingCashAdvanceQuery(string[] jobNumbers, ZGuid orgPK)
		{
			var jobHeaderQuery = CreateJobHeaderQuery(jobNumbers);

			var jobChargeQuery = new ZDBOnlyQuery(typeof(JobCharge));
			jobChargeQuery.AddSubQuery(JobChargeSchema.JR_JH, jobHeaderQuery, JoinCondition.And);
			jobChargeQuery.AddToFilter(JobChargeSchema.JR_CAL_ARLine, null);
			if (!orgPK.IsEmpty)
			{
				jobChargeQuery.AddToFilter(JobChargeSchema.JR_OH_SellAccount, orgPK);
			}
			jobChargeQuery.AddToFilter(JobChargeSchema.JR_IsARCashAdvance, true);
			return jobChargeQuery;
		}

		static ZDBOnlySubQuery CreateJobHeaderQuery(string[] jobNumbers)
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest && RecordJobNumberInfo_ForTestOnly != null)
			{
				RecordJobNumberInfo_ForTestOnly(jobNumbers);
			}
#endif
			var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_JobNum, jobNumbers);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return jobHeaderQuery;
		}

#if DEBUG
		[ThreadStatic]
		public static Action<string[]> RecordJobNumberInfo_ForTestOnly;
#endif
	}
}
