using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	class ContainerPenaltyCalculateHandlerForConsol : IContainerPenaltyCalculateHandler
	{
		readonly CommonContainer container;

		public ContainerPenaltyCalculateHandlerForConsol(CommonContainer container)
		{
			this.container = container;
		}

		public void HandleContainerDateChanging(ContainerPenaltyRelatedDateType dateType)
		{
			if (!container.SupportsContainerPenalties || container.ContainerParent == null)
			{
				return;
			}

			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			switch (dateType)
			{
				case ContainerPenaltyRelatedDateType.EmptyReturnedBy:
					{
						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

						foreach (var penalty in penalties)
						{
							TryCalculateArrivalDetentionPenaltyValues(penalty);
						}

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLWharfGateOut:
					{
						TryCalculateCTOStorageStart();

						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

						foreach (var penalty in penalties)
						{
							TryCalculateArrivalDetentionPenaltyValues(penalty);
							TryCalculateArrivalCarrierStoragePenaltyValues(penalty);
							TryCalculateArrivalCTOStoragePenaltyValues(penalty);
							TryCalculateArrivalMDDPenaltyValues(penalty);
						}

						CalculateArrivalCTOStorageFreeDaysFromRegistry();

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						break;
					}
				case ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn:
					{
						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

						foreach (var penalty in penalties)
						{
							TryCalculateArrivalDetentionPenaltyValues(penalty);
							TryCalculateArrivalMDDPenaltyValues(penalty);
						}

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLAvailable:
					{
						TryCalculateCTOStorageStart(true);

						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

						foreach (var penalty in penalties)
						{
							TryCalculateArrivalDetentionPenaltyValues(penalty);
							TryCalculateArrivalCarrierStoragePenaltyValues(penalty);
							TryCalculateArrivalCTOStoragePenaltyValues(penalty);
							TryCalculateArrivalMDDPenaltyValues(penalty);
						}

						CalculateArrivalCTOStorageFreeDaysFromRegistry();

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						break;
					}
				case ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate:
					{
						using (container.SuspendContainerPenalties())
						{
							var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

							foreach (var penalty in penalties)
							{
								TryCalculateArrivalCarrierStoragePenaltyValues(penalty);
								TryCalculateArrivalCTOStoragePenaltyValues(penalty);
							}

							CalculateArrivalCTOStorageFreeDaysFromRegistry();

							CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						}

						break;
					}
				case ContainerPenaltyRelatedDateType.FCLWharfGateIn:
					{
						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Export);

						foreach (var penalty in penalties)
						{
							TryCalculateDepartureDetentionPenaltyValues(penalty);
							TryCalculateDepartureCarrierStoragePenaltyValues(penalty);
							TryCalculateDepartureCTOStoragePenaltyValues(penalty);
						}

						CalculateDefaultDepartureCTOStorageFreeDaysFromRegistry();

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Export);
						break;
					}
				case ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut:
					{
						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Export);

						foreach (var penalty in penalties)
						{
							TryCalculateDepartureDetentionPenaltyValues(penalty);
							TryCalculateDepartureMDDPenaltyValues(penalty);
						}

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Export);
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLOnBoardVessel:
					{
						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Export);

						foreach (var penalty in penalties)
						{
							TryCalculateDepartureDetentionPenaltyValues(penalty);
							TryCalculateDepartureCarrierStoragePenaltyValues(penalty);
							TryCalculateDepartureCTOStoragePenaltyValues(penalty);
							TryCalculateDepartureMDDPenaltyValues(penalty);
						}

						CalculateDefaultDepartureCTOStorageFreeDaysFromRegistry();

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Export);
						break;
					}
				case ContainerPenaltyRelatedDateType.FCLUnloadFromVessel:
					{
						TryCalculateCTOStorageStart();

						var penalties = CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import);

						foreach (var penalty in penalties)
						{
							TryCalculateArrivalDetentionPenaltyValues(penalty);
							TryCalculateArrivalCarrierStoragePenaltyValues(penalty);
							TryCalculateArrivalCTOStoragePenaltyValues(penalty);
							TryCalculateArrivalMDDPenaltyValues(penalty);
						}

						CalculateArrivalCTOStorageFreeDaysFromRegistry();

						CalculateManualPenalties(penalties, Core.Constants.ContainerPenaltyProcessType.Import);
						break;
					}
				case ContainerPenaltyRelatedDateType.OverrideFCLAvailableStorageSetToFalse:
				case ContainerPenaltyRelatedDateType.TransportDepotStorageDate:
					{
						TryCalculateCTOStorageStart();
						break;
					}
			}
		}

		#region Penalty Calculation Helpers

		void CalculateManualPenalties(IEnumerable<IContainerPenaltyMatchResult> matchedPenalties, ZString processType)
		{
			if (processType == Core.Constants.ContainerPenaltyProcessType.Import)
			{
				foreach (var penalty in container.ImportPenalties.ToArray())
				{
					CalaculateManualPenalty(matchedPenalties, penalty);
				}
			}
			else if (processType == Core.Constants.ContainerPenaltyProcessType.Export)
			{
				foreach (var penalty in container.ExportPenalties.ToArray())
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

		void TryCalculateArrivalDetentionPenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				FindOrCreateArrivalCarrierDetentionPenalty()?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalCarrierStoragePenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
			{
				container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalCTOStoragePenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
			{
				container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateArrivalMDDPenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				container.ImportPenalties.FindOrCreateArrivalMDDPenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureDetentionPenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				FindOrCreateDepartureCarrierDetentionPenalty()?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureCarrierStoragePenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
			{
				container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureCTOStoragePenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
			{
				container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		void TryCalculateDepartureMDDPenaltyValues(IContainerPenaltyMatchResult penalty)
		{
			if (penalty.PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				container.ExportPenalties.FindOrCreateDepartureMDDPenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
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

		ContainerPenalty FindOrCreateArrivalCarrierDetentionPenalty() => container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(container == null || !container.JC_IsShipperOwned, true, true);
		ContainerPenalty FindOrCreateDepartureCarrierDetentionPenalty() => container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(container == null || !container.JC_IsShipperOwned, true, true);

		void CalculateDefaultDepartureCTOStorageFreeDaysFromRegistry()
		{
			if (container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(false, false) == null && container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(false, false) == null)
			{
				var defaultValue = FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.Value.ValidFreeDays;
				if (defaultValue.HasValue)
				{
					var penalty = container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(true, false);
					penalty.FreeTimeAsDays = Convert.ToByte(defaultValue.Value);
					penalty.CalculateDefaultDurationAsDays();
				}
			}
		}

		void CalculateArrivalCTOStorageFreeDaysFromRegistry()
		{
			if (container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false) == null && container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false) == null)
			{
				var freeDays = FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.Value.ValidFreeDays;
				if (freeDays.HasValue && container.JC_FCLAvailable.IsValid)
				{
					var penalty = container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(true, false);
					penalty.FreeTimeAsDays = Convert.ToByte(freeDays.Value);
					penalty.CalculateDefaultDurationAsDays();
				}
				else
				{
					if (container.JC_ArrivalCTOStorageStartDate.IsValid && container.JC_FCLAvailable.IsValid && container.JC_ArrivalCTOStorageStartDate >= container.JC_FCLAvailable)
					{
						container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(true, false).CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
					}
				}
			}
		}

		void TryCalculateCTOStorageStart(bool calculateIfOverriden = false)
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || container.ContainerParent == null
				|| (container.JC_OverrideFCLAvailableStorage && !calculateIfOverriden))
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				container.CalculateJC_ArrivalCTOStorageStartDate();
			}
		}

		void CalculateDefaultImportPenalties()
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || !container.SupportsContainerPenalties)
			{
				return;
			}

			if (!(container.JC_FCLWharfGateOut.IsValid || container.JC_FCLAvailable.IsValid || container.JC_ArrivalCTOStorageStartDate.IsValid
				|| container.JC_FCLWharfGateIn.IsValid || container.JC_FCLOnBoardVessel.IsValid || container.JC_FCLUnloadFromVessel.IsValid
				|| container.JC_ContainerYardEmptyReturnGateIn.IsValid))
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				var penalties = CalculateMatchedPenalties(ContainerPenaltyProcessType.Import);

				foreach (var penalty in penalties)
				{
					TryCalculateArrivalDetentionPenaltyValues(penalty);
					TryCalculateArrivalCarrierStoragePenaltyValues(penalty);
					TryCalculateArrivalCTOStoragePenaltyValues(penalty);
					TryCalculateArrivalMDDPenaltyValues(penalty);
				}

				CalculateArrivalCTOStorageFreeDaysFromRegistry();
			}
		}

		void CalculateDefaultExportPenalties()
		{
			if ((container as ISupportDataImporting).IsImportingData || container.SuspendedContainerPenalties || !container.SupportsContainerPenalties)
			{
				return;
			}

			using (container.SuspendContainerPenalties())
			{
				container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				container.ExportPenalties.FindOrCreateDepartureMDDPenalty(true)?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
				FindOrCreateDepartureCarrierDetentionPenalty()?.CalculateDefaultFreeTimeAsDays().CalculateDefaultDurationAsDays();
			}
		}

		public void HandleContainerAdded()
		{
			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			TryCalculateCTOStorageStart();
			CalculateDefaultImportPenalties();
		}

		public void HandleTransportATAChanged()
		{
			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			TryCalculateCTOStorageStart();
			CalculateDefaultImportPenalties();
		}

		public void HandleTransportATDChanged()
		{
			if (container.SuspendedContainerPenalties)
			{
				return;
			}

			CalculateDefaultExportPenalties();
		}

		public void HandleContainerAllocation(PackLine packLine)
		{
		}

		public void HandleContainerDeallocation(PackLine packLine)
		{
		}
	}
}
