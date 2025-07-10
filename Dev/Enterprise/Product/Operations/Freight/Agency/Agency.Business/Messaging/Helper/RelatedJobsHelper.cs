using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public static class RelatedJobsHelper
	{
		#region BillOfLading

		public static ZQuery GetRelatedBillOfLadingFilter(BusinessObjectFactory factory, JobVoyage voyage, ZString port, ZString direction)
		{
			var voyages = new List<JobVoyage>();
			voyages.Add(voyage);
			voyages.AddRange(GetSlotVoyages(factory, voyage));

			var sailingPKs = voyages.SelectMany(v => GetRelatedSailings(v, port, direction)).Select(s => s.PK).ToList();

			var transportsFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportsFilter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Constants.TransportParentTypes.AgencyShipment);
			transportsFilter.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);
			transportsFilter.AddToFilter(JobConsolTransportSchema.JW_JX, sailingPKs);

			var sailingRelationship = new ZDBOnlyQuery(typeof(AgencyShipment));
			sailingRelationship.AddToFilter(JobShipmentSchema.JS_JX, sailingPKs);
			sailingRelationship.AddSubQuery(transportsFilter, JoinCondition.Or);

			var result = new ZQuery();

			result.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			result.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
			result.AddToFilter(sailingRelationship);

			return result;
		}

		static IEnumerable<JobSailing> GetRelatedSailings(JobVoyage voyage, ZString port, ZString direction)
		{
			if (direction.IsEmpty)
			{
				return voyage.Sailings.Cast<JobSailing>().Where(s => s.JX_JA_RL_NKPortOfLoading == port || s.JX_JB_RL_NKPortOfDischarge == port).ToList();
			}

			if (direction == Constants.PortDirection.Transit)
			{
				var transitPortDateTimes = new List<ZDateTime>();

				var origins = voyage.Origins.Cast<VoyageOrigin>();
				var origin = origins.FirstOrDefault(x => x.JA_RL_NKPortOfLoading == port);
				if (origin != null)
				{
					transitPortDateTimes.Add(origin.JA_E_DEP);
				}

				var destinations = voyage.Destinations.Cast<VoyageDestination>();
				var destination = destinations.FirstOrDefault(x => x.JB_RL_NKPortOfDischarge == port);
				if (destination != null)
				{
					transitPortDateTimes.Add(destination.JB_E_ARV);
				}

				if (!transitPortDateTimes.Any())
				{
					return Enumerable.Empty<JobSailing>();
				}

				return voyage.Sailings.Cast<JobSailing>().Where(s =>
				{
					if (s.JX_JA_RL_NKPortOfLoading == port || s.JX_JB_RL_NKPortOfDischarge == port)
					{
						return false;
					}

					var etd = origins.FirstOrDefault(x => x.JA_RL_NKPortOfLoading == s.JX_JA_RL_NKPortOfLoading)?.JA_E_DEP ?? ZDateTime.Empty;
					var eta = destinations.FirstOrDefault(x => x.JB_RL_NKPortOfDischarge == s.JX_JB_RL_NKPortOfDischarge)?.JB_E_ARV ?? ZDateTime.Empty;

					foreach (var transitPortDateTime in transitPortDateTimes)
					{
						if (etd <= transitPortDateTime && eta >= transitPortDateTime)
						{
							return true;
						}
					}

					return false;
				});
			}

			var sailings = direction == Constants.PortDirection.Load
				? voyage.Sailings.Cast<JobSailing>().Where(s => s.JX_JA_RL_NKPortOfLoading == port)
				: voyage.Sailings.Cast<JobSailing>().Where(s => s.JX_JB_RL_NKPortOfDischarge == port);

			return sailings.ToList();
		}

		static IEnumerable<JobVoyage> GetSlotVoyages(BusinessObjectFactory factory, JobVoyage voyage)
		{
			var voyages = new List<JobVoyage>();

			var query = new ZQuery();
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, voyage.JV_AirSeaRoad);
			query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, voyage.JV_RV_NKVessel);
			query.AddToFilter(JobVoyageSchema.JV_VoyageFlight, voyage.JV_VoyageFlight);
			factory.AddFetchHint(JobVoyageSchema.Instance, query);

			var originalVoyage = factory.Load<JobVoyage>(voyage.PK);
			if (originalVoyage != null)
			{
				voyages.AddRange(originalVoyage.FindOtherVoyagesWithSameVesselVoyageCombination());
			}

			return voyages;
		}

		#endregion
	}
}
