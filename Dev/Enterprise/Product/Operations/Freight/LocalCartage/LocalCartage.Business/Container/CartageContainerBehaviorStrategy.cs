using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageContainerBehaviorStrategy : CommonContainerBehaviorStrategy
	{
		public override void ArrivalSlotDateOrReferenceChanged(CommonContainer container, ZDateTime originalValue)
		{
			foreach (CommonBookedCtgMove move in GetAllBookedMovesOnContainer(container))
			{
				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					if (leg.PickupDocAddressType == DocAddressType.LocalCartageCTO)
					{
						AddSlotEvent(leg, container.JC_ArrivalSlotDateTime, container.JC_ArrivalSlotReference, leg.PickupFromCity, Core.Constants.EventReferenceParameterReasons.Pickup);

						if (leg.JU_PlannedPickupTime.IsEmpty || leg.JU_PlannedPickupTime == originalValue)
						{
							leg.JU_PlannedPickupTime = container.JC_ArrivalSlotDateTime;
						}
					}
				}
			}
		}

		public override void DepartureSlotDateOrReferenceChanged(CommonContainer container, ZDateTime originalValue)
		{
			foreach (CommonBookedCtgMove move in GetAllBookedMovesOnContainer(container))
			{
				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					if (leg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO)
					{
						AddSlotEvent(leg, container.JC_DepartureSlotDateTime, container.JC_DepartureSlotReference, leg.DeliverToCity, Core.Constants.EventReferenceParameterReasons.Delivery);

						if ((leg.JU_EstimatedDeliveryTime.IsEmpty || leg.JU_EstimatedDeliveryTime == originalValue) && !leg.HasWaitPoint)
						{
							leg.JU_EstimatedDeliveryTime = container.JC_DepartureSlotDateTime;
						}
					}
				}
			}
		}

		void AddSlotEvent(CommonCartageLeg leg, ZDateTime slotTime, ZString slotReference, ZString city, ZString reason)
		{
			if (slotTime.IsValid && !slotReference.IsEmpty)
			{
				leg.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.SlotConfirmed, EstimateActual.Actual, slotTime.ToOffset(), "",
					new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal),
					new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Location, city),
					new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, reason),
					new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.ReferenceNumber, slotReference));
			}
		}

		CommonBookedCtgMoveCollection GetAllBookedMovesOnContainer(CommonContainer container)
		{
			return new CommonBookedCtgMoveCollection(container.Factory, new DependentRelationship(container, typeof(CommonBookedCtgMove), null, JobBookedCtgMoveSchema.EW_JC_Container));
		}

		public override CommonContainerValidation GetNewValidation(CommonContainer container)
		{
			return new CartageContainerValidation(container);
		}
	}
}
