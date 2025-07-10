using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class VoyageRelatedJob : AutoViewVoyageRelatedJob
	{
		public VoyageRelatedJob(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public ScheduleRelatedJobType JobType
		{
			get
			{
				if (jobType == null)
				{
					jobType = ScheduleRelatedJobTypes.New()[VJV_JobType];
					if (jobType == null)
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"The selected record has a VJV_JobType of {VJV_JobType} which has not been configured in ScheduleRelatedJobTypes."));
					}
				}

				return jobType;
			}
		}
		ScheduleRelatedJobType jobType;

		public ZString VJV_JobTypeDescription => JobType != null ? (ZString)JobType.Description : ZString.Empty;

		public BusinessObject BizObj => bizObj ?? (bizObj = Factory.Load(JobType.BizOType, PK));
		BusinessObject bizObj;

		public bool CanBeEditedByCurrentCompany => VJV_GC.IsEmpty || VJV_GC == GlbCompany.CurrentCompany.PK;
	}
}
