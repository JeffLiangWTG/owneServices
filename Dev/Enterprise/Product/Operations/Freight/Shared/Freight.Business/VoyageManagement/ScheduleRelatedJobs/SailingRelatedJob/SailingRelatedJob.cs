using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class SailingRelatedJob : AutoViewSailingRelatedJob
	{
		public SailingRelatedJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ScheduleRelatedJobType JobType
		{
			get
			{
				if (jobType == null)
				{
					jobType = ScheduleRelatedJobTypes.New()[VJX_JobType];
					if (jobType == null)
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"The selected record has a VJX_JobType of {VJX_JobType} which has not been configured in ScheduleRelatedJobTypes."));
					}
				}

				return jobType;
			}
		}
		ScheduleRelatedJobType jobType;

		public BusinessObject BizObj => bizObj ?? (bizObj = Factory.Load(JobType.BizOType, VJX_RelatedJobID));
		BusinessObject bizObj;

		public bool CanBeEditedByCurrentCompany => VJX_GC.IsEmpty || VJX_GC == GlbCompany.CurrentCompany.PK;
	}
}
