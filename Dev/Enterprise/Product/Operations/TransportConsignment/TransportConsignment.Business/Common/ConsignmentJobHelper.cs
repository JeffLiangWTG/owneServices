using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class ConsignmentJobHelper : IConsignmentJobHelper
	{
		#region AttachConsignmentJobsToParentJobCore
		static void AttachLTConsignmentJobsToParentJobCore(JobHeader parentJob, ZGuid parentPK, bool attachEvenIfInDatabase)
		{
			if (attachEvenIfInDatabase || !parentJob.IsInDatabase)
			{
				var consignments = parentJob.Factory.Load<DtbConsignment>(GetLTConsignmentFromParentPK(parentPK));

				foreach (var consignment in consignments)
				{
					AttachConsignmentJobToParentJob(consignment.Job, parentJob.PK);
				}
			}
		}

		#endregion

		public void AttachLTConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK)
		{
			AttachLTConsignmentJobsToParentJobCore((JobHeader)parentJob, parentPK, false);
		}

		public void AttachLTConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK, bool attachEvenIfInDatabase)
		{
			AttachLTConsignmentJobsToParentJobCore((JobHeader)parentJob, parentPK, attachEvenIfInDatabase);
		}

		static ZQuery GetLTConsignmentFromParentPK(ZGuid parentPK)
		{
			var result = new ZDBOnlyQuery(typeof(DtbConsignment));
			result.AddToFilter(DtbConsignmentSchema.LTC_KM_Booking, parentPK);

			// LTConsignment.LTC_KM_Booking --> DtbBooking.KM_KB_Booking --> DtbBookingConsolidation.KB_ParentID --> parent.PK (parentPK) - link to DtbBookingParent from LT Consignment (LTC)
			var consolidationSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.PK);
			consolidationSubFilter.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, parentPK);
			var bookingSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
			bookingSubFilter.AddSubQuery(DtbBookingSchema.KM_KB_Booking, consolidationSubFilter, JoinCondition.And);
			var consignmentSubFilter = new ZDBOnlySubQuery(typeof(DtbConsignment), DtbConsignmentSchema.PK);
			consignmentSubFilter.AddSubQuery(DtbConsignmentSchema.LTC_KM_Booking, bookingSubFilter, JoinCondition.And);

			result.AddSubQuery(consignmentSubFilter, JoinCondition.Or);
			return result;
		}

		#region AttachConsignmentJobToParentJob

		static void AttachConsignmentJobToParentJob(JobHeader consignmentJob, ZGuid parentJobPK)
		{
			if (consignmentJob != null)
			{
				consignmentJob.JH_JH_ParentJob = parentJobPK;
			}
		}

		#endregion
	}
}
