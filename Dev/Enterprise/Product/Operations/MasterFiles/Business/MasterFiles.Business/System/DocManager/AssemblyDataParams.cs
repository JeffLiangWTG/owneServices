using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	public class AssemblyDataParams
	{
		public AssemblyDataParams()
		{
		}

		public AssemblyDataParams(bool includeConsignee, bool includeConsignor, ZDateTime etdFrom, ZDateTime etdTo, ZDateTime etaFrom, ZDateTime etaTo, ZDateTime jobClosedFrom, ZDateTime jobClosedTo, ZGuid organisation)
		{
			IncludeConsignee = includeConsignee;
			IncludeConsignor = includeConsignor;
			ETDFrom = etdFrom;
			ETDTo = etdTo;
			ETAFrom = etaFrom;
			ETATo = etaTo;
			JobClosedFrom = jobClosedFrom;
			JobClosedTo = jobClosedTo;
			Organisation = organisation;
		}

		public bool IncludeConsignee { get; set; }
		public bool IncludeConsignor { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public ZDateTime JobClosedFrom { get; set; }
		public ZDateTime JobClosedTo { get; set; }
		public ZGuid Organisation { get; set; }
		public ZString CompanyCode { get; set; }

		public bool IsJobClosedDatesSpecified
		{
			get { return (!JobClosedFrom.IsEmpty || !JobClosedTo.IsEmpty); }
		}

		public bool IsDateConstrained
		{
			get
			{
				return !ETDFrom.IsEmpty ||
					!ETDTo.IsEmpty ||
					!ETAFrom.IsEmpty ||
					!ETATo.IsEmpty;
			}
		}

		public ZDBOnlySubQuery GetJobClosedQuery()
		{
			var jobQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			var logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.JobClose.Code);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);

			if (!JobClosedFrom.IsEmpty)
			{
				logQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, JobClosedFrom);
			}

			if (!JobClosedTo.IsEmpty)
			{
				logQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, JobClosedTo);
			}

			jobQuery.AddSubQuery(logQuery, JoinCondition.And);
			return jobQuery;
		}
	}
}
