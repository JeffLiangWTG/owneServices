using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageCartageLegBehaviorStrategy : CommonCartageLegBehaviorStrategy
	{
		public CartageCartageLegBehaviorStrategy()
		{
		}

		public override void BookedMoveCartageLegLinkCreated(CommonCartageLeg cartageLeg)
		{
			DefaultsForNewLeg(cartageLeg);
		}

		public override void BookedMoveCartageLegLinkBroken(CommonCartageLeg cartageLeg)
		{
			cartageLeg.Delete();
		}

		void DefaultsForNewLeg(CommonCartageLeg leg)
		{
			using (leg.SuspendSettingHasChanges())
			{
				DefaultDisplayOrder(leg);
				DefaultDates(leg);
			}
		}

		void DefaultDisplayOrder(CommonCartageLeg leg)
		{
			int legsCount = leg.BookedCtgMove != null ? leg.BookedCtgMove.CartageLegs.Count : 0;
			leg.JU_DisplayOrder = legsCount + 1;
		}

		public void DefaultDates(CommonCartageLeg leg)
		{
			DefaultPlannedPickup(leg);
			DefaultEstimatedDelivery(leg);
		}

		void DefaultPlannedPickup(CommonCartageLeg leg)
		{
			if (leg.PickupDocAddressType != DocAddressType.None && leg.DeliverToDocAddressType != DocAddressType.None)
			{
				var container = leg.Container;
				if (container != null)
				{
					if (leg.JU_PlannedPickupTime.IsEmpty && leg.PickupDocAddressType == DocAddressType.LocalCartageCTO && container.JC_ArrivalSlotDateTime.IsValid)
					{
						leg.JU_PlannedPickupTime = container.JC_ArrivalSlotDateTime;
					}

					if (leg.JU_PlannedPickupTime.IsEmpty && leg.Cartage.IsExportOrOrigin && leg.PickupDocAddressType != DocAddressType.LocalCartageYard && container.JC_DepartureEstimatedPickup.IsValid)
					{
						leg.JU_PlannedPickupTime = container.JC_DepartureEstimatedPickup;
					}

					if (leg.JU_PlannedPickupTime.IsEmpty && leg.Cartage.IsImportOrDestination && leg.DeliverToDocAddressType != DocAddressType.LocalCartageYard && container.JC_ArrivalEstimatedDelivery.IsValid)
					{
						leg.JU_PlannedPickupTime = container.JC_ArrivalEstimatedDelivery;
					}
				}

				if (leg.JU_PlannedPickupTime.IsEmpty && leg.IsFirstFullLeg && leg.Cartage.JJ_EstimatedPickup.IsValid)
				{
					leg.JU_PlannedPickupTime = leg.Cartage.JJ_EstimatedPickup;
				}
			}
		}

		void DefaultEstimatedDelivery(CommonCartageLeg leg)
		{
			if (leg.PickupDocAddressType != DocAddressType.None && leg.DeliverToDocAddressType != DocAddressType.None)
			{
				var container = leg.Container;
				if (container != null)
				{
					if (leg.JU_EstimatedDeliveryTime.IsEmpty)
					{
						if (leg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO && !leg.HasWaitPoint && container.JC_DepartureSlotDateTime.IsValid)
						{
							leg.JU_EstimatedDeliveryTime = container.JC_DepartureSlotDateTime;
						}
						else if (leg.PickupDocAddressType == DocAddressType.LocalCartageYard && container.JC_EmptyRequired.IsValid)
						{
							leg.JU_EstimatedDeliveryTime = container.JC_EmptyRequired;
						}
						else if (leg.DeliverToDocAddressType == DocAddressType.LocalCartageYard && !leg.HasWaitPoint && container.JC_EmptyReturnedBy.IsValid)
						{
							leg.JU_EstimatedDeliveryTime = container.JC_EmptyReturnedBy;
						}
					}
				}

				if (leg.JU_EstimatedDeliveryTime.IsEmpty && leg.IsFirstFullLeg && leg.Cartage.JJ_EstimatedDelivery.IsValid)
				{
					leg.JU_EstimatedDeliveryTime = leg.Cartage.JJ_EstimatedDelivery;
				}
			}
		}

		public override void WorkSheetCartageLegLinkCreated(CommonCartageLeg cartageLeg)
		{
			new RunSheetSequenceHelper(cartageLeg).GiveSequenceNumberToNewLegOnWorkSheet();

			var eventArgs = new WorkSheetLegLinkEventArgs(cartageLeg);

			if (cartageLeg.Cartage != null)
			{
				cartageLeg.Cartage.RaiseWorkSheetLegLinkAdded(eventArgs);
			}

			if (cartageLeg.WorkSheet != null)
			{
				cartageLeg.WorkSheet.RaiseCartageLegAdded(eventArgs);
			}

			cartageLeg.RaiseWorkSheetAdded(eventArgs);
		}

		public override void WorkSheetCartageLegLinkBroken(CommonCartageLeg cartageLeg)
		{
			new RunSheetSequenceHelper(cartageLeg).RemoveSequenceNumberFromLegOnAWorkSheet();
		}

		public override void PickupAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
			if (cartageLeg.JU_E2PickupAddressID != originalValue)
			{
				DefaultDates(cartageLeg);
				cartageLeg.Cartage.MarkAsNeededAddressReorder();
			}
		}

		public override void WaitPointAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
			if (cartageLeg.JU_E2WaitPointAddressID != originalValue)
			{
				DefaultDates(cartageLeg);
				cartageLeg.Cartage.MarkAsNeededAddressReorder();
			}
		}

		public override void DeliveryAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
			if (cartageLeg.JU_E2DeliveryAddressID != originalValue)
			{
				DefaultDates(cartageLeg);
				cartageLeg.Cartage.MarkAsNeededAddressReorder();
			}
		}
	}
}
