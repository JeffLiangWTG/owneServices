using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	public static class FetchHintsHelper
	{
		public static void AddFetchHintsForGlbCompanyOrgProxy(BusinessObjectFactory factory, IEnumerable<ZGuid> orgPks)
		{
			foreach (var orgPk in orgPks.Distinct())
			{
				var companyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, orgPk);
				companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
				factory.AddFetchHint(GlbCompanySchema.Instance, companyQuery);
			}
		}

		public static void AddFetchHintsForGlbBranchOrgProxy(BusinessObjectFactory factory, IEnumerable<ZGuid> orgPks)
		{
			foreach (var orgPk in orgPks.Distinct())
			{
				var branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, orgPk);
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				factory.AddFetchHint(GlbBranchSchema.Instance, branchQuery);
			}
		}

		public static ZQuery GetJobDocAddressQuery(string parentTableCode, ZGuid parentPK)
		{
			var jobDocAddressQuery = new ZQuery();
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parentTableCode);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, parentPK);
			return jobDocAddressQuery;
		}

		public static ZQuery GetJobDocAddressQuery(string parentTableCode, ZGuid[] parentPKs)
		{
			var jobDocAddressQuery = new ZQuery();
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parentTableCode);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, parentPKs);
			return jobDocAddressQuery;
		}

		public static ZQuery GetStmALogQuery(ZGuid parentPK, string eventCode)
		{
			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_Parent, parentPK);
			logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			return logQuery;
		}

		public static ZQuery GetProcessTasksQuery(ZGuid parentPK, string eventCode, string[] type, ZString lineTriggerType)
		{
			var processTaskquery = new ZQuery();
			processTaskquery.AddToFilter(ProcessTasksSchema.P9_ParentID, parentPK);
			processTaskquery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, eventCode);
			processTaskquery.AddToFilter(ProcessTasksSchema.P9_Type, type);
			processTaskquery.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, lineTriggerType);
			return processTaskquery;
		}
	}
}
