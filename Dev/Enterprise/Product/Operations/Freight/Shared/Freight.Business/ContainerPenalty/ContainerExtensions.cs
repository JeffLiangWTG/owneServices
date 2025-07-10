using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public static class ContainerExtensions
	{
		public static IReadOnlyCollection<CommonShipment> GetRelatedShipmentsForPenaltyDefaulting(this CommonContainer container, ZString processType)
		{
			var shipments = new HashSet<CommonShipment>();
			var consol = container.Consol;

			if (consol == null
				|| !consol.Shipments.Any()
				|| (processType == ContainerPenaltyProcessType.Delivery || processType == ContainerPenaltyProcessType.Pickup)
					&& consol.JK_TransportMode != TransportModes.Sea)
			{
				return Array.Empty<CommonShipment>();
			}

			foreach (PackLine packLine in container.PackLines)
			{
				if (!PackLineHasConsol(packLine, consol))
				{
					break;
				}

				var shipment = packLine.Shipment;
				if (shipment != null && supportsPenalties(shipment))
				{
					shipments.Add(packLine.Shipment);
				}
			}

			if (!shipments.Any())
			{
				return consol.Shipments.Cast<CommonShipment>()
					.Where(supportsPenalties)
					.OrderBy(shipment => shipment.JS_UniqueConsignRef)
					.ToArray();
			}

			return shipments;

			bool supportsPenalties(CommonShipment shipment)
			{
				return processType.ToString() switch
				{
					ContainerPenaltyProcessType.Delivery => shipment.SupportsDeliveryPenalties,
					ContainerPenaltyProcessType.Pickup => shipment.SupportsPickupPenalties,
					_ => true
				};
			}
		}

		static bool PackLineHasConsol(PackLine packLine, CommonConsol consol)
		{
			var query = new ZQuery(JobContainerPackPivotSchema.J6_JL, packLine.PK);
			var pivot = packLine.Factory.LoadTop1<JobContainerPackPivot>(query);
			return pivot?.Container != null && pivot.Container.JC_JK == consol.PK;
		}
	}
}
