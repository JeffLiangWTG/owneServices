using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	class ContainerPenaltyCalculateHandlerForShipment : IContainerPenaltyCalculateHandler
	{
		readonly CommonContainer container;

		public ContainerPenaltyCalculateHandlerForShipment(CommonContainer container)
		{
			this.container = container;
		}

		bool IsAllocated => container?.PackLines.Any() ?? false;

		public void HandleContainerDateChanging(ContainerPenaltyRelatedDateType dateType)
		{
			HandleContainerDateChanging(dateType, null);
		}

		internal bool StopHandlingContainerDateChanging()
		{
			return ((container as ISupportDataImporting)?.IsImportingData ?? true)
				|| !container.SupportsContainerPenalties || container.ContainerParent == null
				|| container.SuspendedContainerPenalties
				|| !IsAllocated
				|| (container.Consol != null && container.Consol.TransportMode != Core.Constants.TransportModes.Sea);
		}

		void HandleContainerDateChanging(ContainerPenaltyRelatedDateType dateType, PackLine packLine)
		{
			if (StopHandlingContainerDateChanging())
			{
				return;
			}

			var shipmentsForDelivery = packLine?.Shipment != null ? new CommonShipment[] { packLine.Shipment } : container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery);
			var shipmentsForPickup = packLine?.Shipment != null ? new CommonShipment[] { packLine.Shipment } : container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Pickup);

			switch (dateType)
			{
				case ContainerPenaltyRelatedDateType.EmptyReturnedBy:
					{
						foreach (var shipment in shipmentsForDelivery)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalDetentionPenaltyValues(penalty, shipment);
								TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
							}

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLWharfGateOut:
					{
						foreach (var shipment in shipmentsForDelivery)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalDetentionPenaltyValues(penalty, shipment);
								TryCalculateArrivalCarrierStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalCTOStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
							}

							CalculateArrivalCTOStorageFreeDaysFromContainer(shipment);

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn:
					{
						foreach (var shipment in shipmentsForDelivery)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalDetentionPenaltyValues(penalty, shipment);
								TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
							}

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLAvailable:
					{
						foreach (var shipment in shipmentsForDelivery)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalDetentionPenaltyValues(penalty, shipment);
								TryCalculateArrivalCarrierStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalCTOStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
							}

							CalculateArrivalCTOStorageFreeDaysFromContainer(shipment);

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate:
					{
						using (container.SuspendContainerPenalties())
						{
							foreach (var shipment in shipmentsForDelivery)
							{
								var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

								foreach (var penalty in penalties)
								{
									TryCalculateArrivalCarrierStoragePenaltyValues(penalty, shipment);
									TryCalculateArrivalCTOStoragePenaltyValues(penalty, shipment);
									TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
								}

								CalculateArrivalCTOStorageFreeDaysFromContainer(shipment);

								CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
							}
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLUnloadFromVessel:
					{
						foreach (var shipment in shipmentsForDelivery)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Delivery, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalDetentionPenaltyValues(penalty, shipment);
								TryCalculateArrivalCarrierStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalCTOStoragePenaltyValues(penalty, shipment);
								TryCalculateArrivalMDDPenaltyValues(penalty, shipment);
							}

							CalculateArrivalCTOStorageFreeDaysFromContainer(shipment);

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Delivery);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLWharfGateIn:
					{
						foreach (var shipment in shipmentsForPickup)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Pickup, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateDepartureDetentionPenaltyValues(penalty, shipment);
								TryCalculateDepartureCarrierStoragePenaltyValues(penalty, shipment);
								TryCalculateDepartureCTOStoragePenaltyValues(penalty, shipment);
								TryCalculateDepartureMDDPenaltyValues(penalty, shipment);
							}

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Pickup);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut:
					{
						foreach (var shipment in shipmentsForPickup)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Pickup, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateDepartureDetentionPenaltyValues(penalty, shipment);
								TryCalculateDepartureMDDPenaltyValues(penalty, shipment);
							}

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Pickup);
						}
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLOnBoardVessel:
					{
						foreach (var shipment in shipmentsForPickup)
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Pickup, shipment);

							foreach (var penalty in penalties)
							{
								TryCalculateDepartureDetentionPenaltyValues(penalty, shipment);
								TryCalculateDepartureCarrierStoragePenaltyValues(penalty, shipment);
								TryCalculateDepartureCTOStoragePenaltyValues(penalty, shipment);
								TryCalculateDepartureMDDPenaltyValues(penalty, shipment);
							}

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Pickup);
						}
						break;
					}
			}

			RefreshPickupPenaltiesAndDeliveryPenalties(true, null);
		}

		#region Penalty Calculation Helpers

		void CalculateManualPenalties(IEnumerable<IContainerPenaltyMatchResult> matchedPenalties, ZString processType)
		{
			if (processType == Core.Constants.ContainerPenaltyProcessType.Delivery)
			{
				foreach (var penalty in container.DeliveryPenalties)
				{
					CalaculateManualPenalty(matchedPenalties, penalty);
				}
			}
			else if (processType == Core.Constants.ContainerPenaltyProcessType.Pickup)
			{
				foreach (var penalty in container.PickupPenalties)
				{
					CalaculateManualPenalty(matchedPenalties, penalty);
				}
			}
		}

		void CalaculateManualPenalty(IEnumerable<IContainerPenaltyMatchResult> matchedPenalties, ContainerPenalty penalty)
		{
			var manualPenalty = false;
			if (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage)
			{
				manualPenalty = !matchedPenalties.Any(p => p.PenaltyType == penalty.CPY_PenaltyType && p.CreditorType == penalty.CPY_CreditorType);
			}
			else if (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention
				|| penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				manualPenalty = !matchedPenalties.Any(p => p.PenaltyType == penalty.CPY_PenaltyType);
			}

			if (manualPenalty)
			{
				penalty.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalDetentionPenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				FindOrCreateArrivalCarrierDetentionPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalCarrierStoragePenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
			{
				FindOrCreateArrivalCarrierStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalCTOStoragePenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
			{
				FindOrCreateArrivalCTOStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalMDDPenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				FindOrCreateArrivalMDDPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureDetentionPenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				FindOrCreateDepartureCarrierDetentionPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureCarrierStoragePenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
			{
				FindOrCreateDepartureCarrierStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureCTOStoragePenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
			{
				FindOrCreateDepartureCTOStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureMDDPenaltyValues(IContainerPenaltyMatchResult penalty, CommonShipment shipment)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				FindOrCreateDepartureMDDPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		#endregion

		IEnumerable<IContainerPenaltyMatchResult> CalculateMatchedPenalties(ZString processType, CommonShipment shipment = null)
		{
			var strategy = container.NewContainerDefaultingStrategy();
			if (strategy == null)
			{
				return Enumerable.Empty<IContainerPenaltyMatchResult>();
			}

			return strategy.CalculateMatchedPenalties(processType, shipment);
		}

		ContainerPenalty FindOrCreateArrivalCarrierDetentionPenalty(CommonShipment shipment) => container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(container == null || !container.JC_IsShipperOwned, true, true, shipment);
		ContainerPenalty FindOrCreateArrivalCarrierStoragePenalty(CommonShipment shipment) => container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(true, shipment: shipment);
		ContainerPenalty FindOrCreateArrivalCTOStoragePenalty(CommonShipment shipment) => container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(true, shipment: shipment);
		ContainerPenalty FindOrCreateArrivalMDDPenalty(CommonShipment shipment) => container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(true, shipment: shipment);
		ContainerPenalty FindOrCreateDepartureCarrierDetentionPenalty(CommonShipment shipment) => container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(container == null || !container.JC_IsShipperOwned, true, true, shipment);
		ContainerPenalty FindOrCreateDepartureCarrierStoragePenalty(CommonShipment shipment) => container.PickupPenalties.FindOrCreateDepartureCarrierStoragePenalty(true, shipment: shipment);
		ContainerPenalty FindOrCreateDepartureCTOStoragePenalty(CommonShipment shipment) => container.PickupPenalties.FindOrCreateDepartureCTOStoragePenalty(true, shipment: shipment);
		ContainerPenalty FindOrCreateDepartureMDDPenalty(CommonShipment shipment) => container.PickupPenalties.FindOrCreateDepartureMDDPenalty(true, shipment: shipment);

		void CalculateArrivalCTOStorageFreeDaysFromContainer(CommonShipment shipment)
		{
			if (container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false, shipment: shipment) == null && container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false, shipment: shipment) == null)
			{
				if (container.JC_ArrivalCTOStorageStartDate.IsValid && container.JC_FCLAvailable.IsValid && container.JC_ArrivalCTOStorageStartDate >= container.JC_FCLAvailable)
				{
					container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(true, false, shipment: shipment).CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				}
			}
		}

		void CalculateDefaultStoragePenalty()
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || !container.SupportsContainerPenalties)
			{
				return;
			}

			if (!(container.JC_FCLWharfGateOut.IsValid || container.JC_FCLAvailable.IsValid || container.JC_ArrivalCTOStorageStartDate.IsValid
				|| container.JC_FCLWharfGateIn.IsValid || container.JC_FCLOnBoardVessel.IsValid || container.JC_FCLUnloadFromVessel.IsValid))
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				var shipmentsForDelivery = container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery);

				foreach (var shipment in shipmentsForDelivery)
				{
					FindOrCreateArrivalCarrierStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
					FindOrCreateArrivalCTOStoragePenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
					CalculateArrivalCTOStorageFreeDaysFromContainer(shipment);
					FindOrCreateArrivalMDDPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				}
			}
		}

		public void HandleContainerAdded()
		{
			if ((container as ISupportDataImporting)?.IsImportingData ?? true)
			{
				return;
			}

			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			if (!IsAllocated)
			{
				return;
			}

			this.CalculateDefaultStoragePenalty();

			RefreshPickupPenaltiesAndDeliveryPenalties(true, null);
		}

		public void HandleTransportATAChanged()
		{
			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			CalculateDefaultDeliveryPenalties();
		}

		public void HandleTransportATDChanged()
		{
			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			CalculateDefaultPickupPenalties();
		}

		public void HandleContainerAllocation(PackLine packLine)
		{
			if ((container as ISupportDataImporting)?.IsImportingData ?? true)
			{
				return;
			}

			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			if (container.JC_EmptyReturnedBy.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy, packLine);
			}

			if (container.JC_FCLWharfGateOut.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut, packLine);
			}

			if (container.JC_ContainerYardEmptyReturnGateIn.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn, packLine);
			}

			if (container.JC_FCLAvailable.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable, packLine);
			}

			if (container.JC_ArrivalCTOStorageStartDate.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate, packLine);
			}

			if (container.JC_FCLWharfGateIn.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn, packLine);
			}

			if (container.JC_ContainerYardEmptyPickupGateOut.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut, packLine);
			}

			if (container.JC_FCLOnBoardVessel.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel, packLine);
			}

			if (container.JC_FCLUnloadFromVessel.IsValid)
			{
				HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel, packLine);
			}
		}

		public void HandleContainerDeallocation(PackLine packLine)
		{
			if ((container as ISupportDataImporting)?.IsImportingData ?? true)
			{
				return;
			}

			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			RefreshPickupPenaltiesAndDeliveryPenalties(false, packLine);
		}

		void CalculateDefaultDeliveryPenalties()
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || !container.SupportsContainerPenalties)
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				var shipmentsForDelivery = container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery);
				foreach (var shipment in shipmentsForDelivery)
				{
					FindOrCreateArrivalMDDPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
					FindOrCreateArrivalCarrierDetentionPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				}
			}
		}

		void CalculateDefaultPickupPenalties()
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || !container.SupportsContainerPenalties)
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				var shipmentsForPickup = container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Pickup);
				foreach (var shipment in shipmentsForPickup)
				{
					FindOrCreateDepartureMDDPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
					FindOrCreateDepartureCarrierDetentionPenalty(shipment)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				}
			}
		}

		void RefreshPickupPenaltiesAndDeliveryPenalties(bool isAdd, PackLine packLine)
		{
			if (container.IsDeleted || container.Consol == null || container.Consol.Shipments == null
				|| !(container is ForwardingContainer forwardingContainer))
			{
				return;
			}

			var shipments = packLine?.Shipment == null ? (IList)forwardingContainer.Consol.Shipments : new CommonShipment[] { packLine.Shipment };

			foreach (CommonShipment shipment in shipments)
			{
				if (shipment == null)
				{
					continue;
				}

				if (isAdd)
				{
					foreach (var penalty in container.PickupPenalties)
					{
						if (shipment.PickupPenalties.All(x => x.PK != penalty.PK) && penalty.CPY_JS_Shipment == shipment.PK)
						{
							shipment.PickupPenalties.Add(shipment.Factory.Load<ShipmentContainerPenalty>(penalty.PK));
						}
					}

					foreach (var penalty in container.DeliveryPenalties)
					{
						if (shipment.DeliveryPenalties.All(x => x.PK != penalty.PK) && penalty.CPY_JS_Shipment == shipment.PK)
						{
							shipment.DeliveryPenalties.Add(shipment.Factory.Load<ShipmentContainerPenalty>(penalty.PK));
						}
					}
				}
				else
				{
					if (!shipment.Containers.Any(c => c.PK == container.PK) && packLine != null)
					{
						for (int i = shipment.DeliveryPenalties.Count - 1; i >= 0; i--)
						{
							var penalty = shipment.DeliveryPenalties[i];
							if (penalty.CPY_JC_Container == container.PK)
							{
								shipment.DeliveryPenalties.Delete(penalty);
							}
						}

						for (int i = shipment.PickupPenalties.Count - 1; i >= 0; i--)
						{
							var penalty = shipment.PickupPenalties[i];
							if (penalty.CPY_JC_Container == container.PK)
							{
								shipment.PickupPenalties.Delete(penalty);
							}
						}

						if (!packLine.IsDeleted)
						{
							for (int i = container.DeliveryPenalties.Count - 1; i >= 0; i--)
							{
								var penalty = container.DeliveryPenalties[i];
								if (penalty.CPY_JS_Shipment == packLine.JL_JS)
								{
									container.DeliveryPenalties.Delete(penalty);
								}
							}

							for (int i = container.PickupPenalties.Count - 1; i >= 0; i--)
							{
								var penalty = container.PickupPenalties[i];
								if (penalty.CPY_JS_Shipment == packLine.JL_JS)
								{
									container.PickupPenalties.Delete(penalty);
								}
							}
						}
					}
				}
			}
		}
	}
}
