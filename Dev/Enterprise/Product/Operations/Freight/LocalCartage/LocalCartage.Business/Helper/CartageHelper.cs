using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageHelper : ICartageHelper
	{
		public void AttachCartageJobsToParentJob(IJobHeader parentJob, ZGuid parentPK)
		{
			AttachCartageJobsToParentJob((JobHeader)parentJob, parentPK);
		}

		public void AttachCartageJobsToParentJob(IJobHeader parentJob, ZGuid parentPK, bool attachEvenIfParentJobIsInDatabase)
		{
			AttachCartageJobsToParentJob((JobHeader)parentJob, parentPK, attachEvenIfParentJobIsInDatabase);
		}

		public static void AttachCartageJobsToParentJob(JobHeader parentJob, ZGuid parentPK, bool attachEvenIfParentJobIsInDatabase = false)
		{
			if (attachEvenIfParentJobIsInDatabase || !parentJob.IsInDatabase)
			{
				CommonCartage[] cartages = parentJob.Factory.Load<CommonCartage>(GetCartagesFromParentPK(parentPK));

				foreach (CommonCartage cartage in cartages)
				{
					AttachCartageJobToParentJob(cartage, cartage.Job, parentJob.PK);
				}
			}
		}

		static ZQuery GetCartagesFromParentPK(ZGuid parentPK)
		{
			// JobCartage.JJ_ParentID --> parentPK (JobShipment.JS_PK or DtbBooking.KM_PK)
			var result = new ZDBOnlyQuery(typeof(CommonCartage));
			result.AddToFilter(JobCartageSchema.JJ_ParentID, parentPK);

			// called by ForwardingShipment.OnJobCreating
			//													KM_KB_Booking							KB_ParentID				
			// JobCartage.JJ_ParentID ------> DtbBooking.KM_PK -------> DtbBookingConsilidation.KB_PK --------> JobShipment.PK (parentPK)
			//
			var dtbConsolidationSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.PK);
			dtbConsolidationSubFilter.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, parentPK);
			var dtbBookingSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
			dtbBookingSubFilter.AddSubQuery(DtbBookingSchema.KM_KB_Booking, dtbConsolidationSubFilter, JoinCondition.And);
			result.AddSubQuery(JobCartageSchema.JJ_ParentID, dtbBookingSubFilter, JoinCondition.Or);

			return result;
		}

		internal static void AttachCartageJobToParentJob(CommonCartage cartage, JobHeader cartageJob, ZGuid parentJobPK)
		{
			if (!cartage.ShouldUseParentJob && cartageJob != null)
			{
				cartageJob.JH_JH_ParentJob = parentJobPK;
			}
		}

		public static CommonCartage FindCartage(CartageType cartageType)
		{
			ZQuery directionQuery = null;
			var directionQueryConditions = cartageType.GetMatchingDirectionCodes().Select(dir => new ZQuery(JobCartageSchema.JJ_Direction, dir));

			foreach (var condition in directionQueryConditions)
			{
				directionQuery = (directionQuery == null) ? condition : new ZQuery(directionQuery, JoinCondition.Or, condition);
			}
			var parentIdQuery = new ZQuery(JobCartageSchema.JJ_ParentID, cartageType.CartageParent.CartageParentID);

			var query = new ZQuery(parentIdQuery, JoinCondition.And, directionQuery);
			query.IgnoreActiveFilter = true;
			var cartages = cartageType.CartageParent.Factory.Load<CommonCartage>(query);

			// on the very rare occasion 2 cartages can be created with the same id, but different branch
			// choose the active cartage over one that matches branch, but obviously choose the one with matching branch over any others.

			CommonCartage result = null;

			if (cartages.Length > 1)
			{
				int highScore = -1;

				foreach (var cartage in cartages)
				{
					var score = !cartage.JJ_IsCancelled ? 2 : 0;
					score += cartage.JJ_GB == GlbBranch.CurrentBranch.PK ? 1 : 0;

					if (score >= highScore)
					{
						highScore = score;
						result = cartage;
					}
				}
			}
			else
			{
				result = cartages.FirstOrDefault();
			}

			return result;
		}
	}
}
