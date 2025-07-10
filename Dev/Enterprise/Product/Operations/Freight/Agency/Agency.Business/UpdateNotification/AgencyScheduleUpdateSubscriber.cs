using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyScheduleUpdateSubscriber : IScheduleUpdateSubscriber
	{
		public void ATDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD)
		{
			ZDateTime newDate = origin.JA_A_DEP;

			if (!newDate.IsEmpty && AgencyRegistry.Instance.UpdateShipmentDatesFromSailing.Value)
			{
				newDate = newDate.Date;

				ZQuery dateFilter = new ZQuery();
				dateFilter.DefaultJoinCondition = JoinCondition.Or;
				dateFilter.AddToFilter(JobShipmentSchema.JS_ShippedOnBoardDate, ZDateTime.Empty);
				dateFilter.AddToFilter(JobShipmentSchema.JS_HouseBillIssueDate, ZDateTime.Empty);

				ZQuery shipmentFilter = GetBaseShipmentFilter(origin);
				shipmentFilter.AddToFilter(dateFilter);

				AgencyShipment[] shipments = origin.Factory.Load<AgencyShipment>(shipmentFilter);

				if (shipments.Length > 0 && services.QueryProvider.ShouldUpdateAgencyShipmentDatesFromATD)
				{
					foreach (AgencyShipment shipment in shipments)
					{
						if (shipment.JS_ShippedOnBoardDate.IsEmpty)
						{
							shipment.JS_ShippedOnBoardDate = newDate;
						}

						if (shipment.JS_HouseBillIssueDate.IsEmpty)
						{
							shipment.JS_HouseBillIssueDate = newDate;
						}
					}
				}
			}
		}

		public void ETDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD)
		{
		}

		public void ETAChanged(IScheduleUpdateServices services, VoyageDestination destination, ZDateTime oldETA)
		{
		}

		#region Implementation

		ZQuery GetBaseShipmentFilter(VoyageOrigin origin)
		{
			var originPKs = new List<ZGuid>();
			originPKs.Add(origin.PK);

			if (origin.Voyage != null)
			{
				var anotherVoyages = origin.Voyage.FindOtherVoyagesWithSameVesselVoyageCombination();
				foreach (var anotherVoyage in anotherVoyages)
				{
					var similarOrigin = anotherVoyage.Origins.GetOriginFromLoading(origin.JA_RL_NKPortOfLoading);
					if (similarOrigin != null)
					{
						originPKs.Add(similarOrigin.PK);
					}
				}
			}

			var sailings = origin.Factory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JA, originPKs));

			ZQuery shipmentFilter = new ZQuery();
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_JX, sailings.Select(s => s.PK));

			return shipmentFilter;
		}

		#endregion
	}
}
