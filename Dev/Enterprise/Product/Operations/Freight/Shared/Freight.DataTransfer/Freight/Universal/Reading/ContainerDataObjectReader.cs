using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerDataObjectReader<T> : BaseContainerDataObjectReader<T> where T : CommonContainer
	{
		public ContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<Container, T> containerBizObjProvider = null, Func<Container, T> containerBizObjCreator = null)
			: base(containerDataObject, logger, factory)
		{
			this.containerBizObjProvider = containerBizObjProvider;
			this.containerBizObjCreator = containerBizObjCreator;
		}

		readonly Func<Container, T> containerBizObjProvider;
		readonly Func<Container, T> containerBizObjCreator;

		protected override T GetExistingBusinessObject()
		{
			return containerBizObjProvider != null ? containerBizObjProvider(dataObject) : null;
		}

		protected override T GetNewBusinessObject()
		{
			return containerBizObjCreator != null ? containerBizObjCreator(dataObject) : base.GetNewBusinessObject();
		}

		protected override void PopulateBusinessObject(T container)
		{
			ISupportDataImporting supportDataImporting = container;
			supportDataImporting.IsImportingData = true;

			try
			{
				using (container.SuspendContainerPenalties())
				{
					ImportContainerInfo(container);
					SetValue(container, JobContainerSchema.JC_DeliveryMode, dataObject.DeliveryMode);
					SetValue(container, JobContainerSchema.JC_AirVentFlow, dataObject.AirVentFlow);
					SetValue(container, JobContainerSchema.JC_RH_NKContainerCommodityCode, dataObject.Commodity);
					SetValue(container, JobContainerSchema.JC_RH_NKRatingCommodityCode, dataObject.RatingCommodity);
					SetValue(container, JobContainerSchema.JC_ContainerNum, dataObject.ContainerNumber);
					SetValue(container, JobContainerSchema.JC_DepartureEstimatedPickup, dataObject.DepartureEstimatedPickup);
					SetValue(container, JobContainerSchema.JC_DepartureSlotDateTime, dataObject.DepartureSlotDateTime);
					SetValue(container, JobContainerSchema.JC_DepartureSlotReference, dataObject.DepartureSlotReference);
					SetValue(container, JobContainerSchema.JC_EmptyRequired, dataObject.EmptyRequired);
					SetValue(container, JobContainerSchema.JC_ContainerMode, dataObject.FCL_LCL_AIR);
					SetValue(container, JobContainerSchema.JC_ReleaseNum, dataObject.ReleaseNum);
					SetValue(container, JobContainerSchema.JC_SetPointTemp, dataObject.SetPointTemp);
					SetValue(container, JobContainerSchema.JC_TempRecorderSerialNo, dataObject.TempRecorderSerialNo);
					SetValue(container, JobContainerSchema.JC_AirVentFlowRateUnit, dataObject.AirVentFlowRateUnit);
					SetValue(container, JobContainerSchema.JC_ArrivalCartageAdvised, dataObject.ArrivalCartageAdvised);
					SetValue(container, JobContainerSchema.JC_ArrivalCartageComplete, dataObject.ArrivalCartageComplete);
					SetValue(container, JobContainerSchema.JC_ArrivalCartageRef, dataObject.ArrivalCartageRef);
					SetValue(container, JobContainerSchema.JC_ArrivalDeliveryRequiredBy, dataObject.ArrivalDeliveryRequiredBy);
					SetValue(container, JobContainerSchema.JC_ArrivalEstimatedDelivery, dataObject.ArrivalEstimatedDelivery);
					SetValue(container, JobContainerSchema.JC_ArrivalPickupByRail, dataObject.ArrivalPickupByRail);
					SetValue(container, JobContainerSchema.JC_ArrivalSlotDateTime, dataObject.ArrivalSlotDateTime);
					SetValue(container, JobContainerSchema.JC_ArrivalSlotReference, dataObject.ArrivalSlotReference);
					SetValue(container, JobContainerSchema.JC_ContainerImportDORelease, dataObject.ContainerImportDORelease);
					SetValue(container, JobContainerSchema.JC_ContainerYardEmptyPickupGateOut, dataObject.ContainerParkEmptyPickupGateOut);
					SetValue(container, JobContainerSchema.JC_ContainerYardEmptyReturnGateIn, dataObject.ContainerParkEmptyReturnGateIn);
					SetValue(container, JobContainerSchema.JC_ContainerQuality, dataObject.ContainerQuality);
					SetValue(container, JobContainerSchema.JC_ContainerStatus, dataObject.ContainerStatus);
					SetValue(container, JobContainerSchema.JC_DeliverySequence, dataObject.DeliverySequence);
					SetValue(container, JobContainerSchema.JC_DepartureCartageAdvised, dataObject.DepartureCartageAdvised);
					SetValue(container, JobContainerSchema.JC_DepartureCartageComplete, dataObject.DepartureCartageComplete);
					SetValue(container, JobContainerSchema.JC_DepartureCartageRef, dataObject.DepartureCartageRef);
					SetValue(container, JobContainerSchema.JC_DepartureDeliveryByRail, dataObject.DepartureDeliveryByRail);
					SetValue(container, JobContainerSchema.JC_DepartureDockReceipt, dataObject.DepartureDockReceipt);
					SetValue(container, JobContainerSchema.JC_EmptyReadyForReturn, dataObject.EmptyReadyForReturn);
					SetValue(container, JobContainerSchema.JC_EmptyReturnedBy, dataObject.EmptyReturnedBy);
					SetValue(container, JobContainerSchema.JC_EmptyReturnReference, dataObject.EmptyReturnRef);
					SetValue(container, JobContainerSchema.JC_ExportDepotCustomsReference, dataObject.ExportDepotCustomsReference);
					SetValue(container, JobContainerSchema.JC_ImportDepotCustomsReference, dataObject.ImportDepotCustomsReference);
					SetValue(container, JobContainerSchema.JC_OverrideFCLAvailableStorage, dataObject.OverrideFCLAvailableStorage);
					SetValue(container, JobContainerSchema.JC_FCLAvailable, dataObject.FCLAvailable);
					SetValue(container, JobContainerSchema.JC_FCLHeldInTransitStaging, dataObject.FCLHeldInTransitStaging);
					SetValue(container, JobContainerSchema.JC_FCLOnBoardVessel, dataObject.FCLOnBoardVessel);
					SetValue(container, JobContainerSchema.JC_FCLStorageArrivedUnderbond, dataObject.FCLStorageArrivedUnderbond);
					SetValue(container, JobContainerSchema.JC_ArrivalCTOStorageStartDate, dataObject.ArrivalCTOStorageStartDate ?? dataObject.FCLStorageCommences);
					SetValue(container, JobContainerSchema.JC_FCLStorageModuleOnlyMaster, dataObject.FCLStorageModuleOnlyMaster);
					SetValue(container, JobContainerSchema.JC_FCLStorageUnderbondCleared, dataObject.FCLStorageUnderbondCleared);
					SetValue(container, JobContainerSchema.JC_FCLUnloadFromVessel, dataObject.FCLUnloadFromVessel);
					SetValue(container, JobContainerSchema.JC_FCLWharfGateIn, dataObject.FCLWharfGateIn);
					SetValue(container, JobContainerSchema.JC_FCLWharfGateOut, dataObject.FCLWharfGateOut);
					SetValue(container, JobContainerSchema.JC_HumidityPercent, dataObject.HumidityPercent);
					SetValue(container, JobContainerSchema.JC_IsCFSRegistered, dataObject.IsCFSRegistered);
					SetValue(container, JobContainerSchema.JC_IsControlledAtmosphere, dataObject.IsControlledAtmosphere);
					SetValue(container, JobContainerSchema.JC_IsDamaged, dataObject.IsDamaged);
					SetValue(container, JobContainerSchema.JC_IsEmptyContainer, dataObject.IsEmptyContainer);
					SetValue(container, JobContainerSchema.JC_IsSealOk, dataObject.IsSealOk);
					SetValue(container, JobContainerSchema.JC_IsShipperOwned, dataObject.IsShipperOwned);
					SetValue(container, JobContainerSchema.JC_OverrideLCLAvailableStorage, dataObject.OverrideLCLAvailableStorage);
					SetValue(container, JobContainerSchema.JC_LCLAvailable, dataObject.LCLAvailable);
					SetValue(container, JobContainerSchema.JC_LCLStorageCommences, dataObject.LCLStorageCommences);
					SetValue(container, JobContainerSchema.JC_LCLUnpack, dataObject.LCLUnpack);
					SetValue(container, JobContainerSchema.JC_PackDate, dataObject.PackDate);
					SetValue(container, JobContainerSchema.JC_RefrigGeneratorID, dataObject.RefrigGeneratorID);
					SetValue(container, JobContainerSchema.JC_SealNum, dataObject.Seal);
					SetValue(container, JobContainerSchema.JC_AdditionalSealNum, dataObject.SecondSeal);
					SetValue(container, JobContainerSchema.JC_Additional2SealNum, dataObject.ThirdSeal);
					SetValue(container, JobContainerSchema.JC_SealParty, dataObject.SealPartyType);
					SetValue(container, JobContainerSchema.JC_AdditionalSealParty, dataObject.SecondSealPartyType);
					SetValue(container, JobContainerSchema.JC_Additional2SealParty, dataObject.ThirdSealPartyType);
					SetValue(container, JobContainerSchema.JC_SetPointTempUnit, dataObject.SetPointTempUnit);
					SetValue(container, JobContainerSchema.JC_StowagePosition, dataObject.StowagePosition);
					SetValue(container, JobContainerSchema.JC_TotalHeight, dataObject.TotalHeight);
					SetValue(container, JobContainerSchema.JC_TotalLength, dataObject.TotalLength);
					SetValue(container, JobContainerSchema.JC_TotalUnitOfMeasure, dataObject.LengthUnit);
					SetValue(container, JobContainerSchema.JC_TotalWidth, dataObject.TotalWidth);
					SetValue(container, JobContainerSchema.JC_TrainWagonNumber, dataObject.TrainWagonNumber);
					SetValue(container, JobContainerSchema.JC_UnpackGang, dataObject.UnpackGang);
					SetValue(container, JobContainerSchema.JC_UnpackShed, dataObject.UnpackShed);
					SetValue(container, JobContainerSchema.JC_VolumeCapacity, dataObject.VolumeCapacity);
					SetValue(container, JobContainerSchema.JC_VolumeCapacityUQ, dataObject.VolumeUnit);
					SetValue(container, JobContainerSchema.JC_WeightCapacity, dataObject.WeightCapacity);

					SetValue(container, JobContainerSchema.JC_OverhangBack, dataObject.OverhangBack);
					SetValue(container, JobContainerSchema.JC_OverhangRight, dataObject.OverhangRight);
					SetValue(container, JobContainerSchema.JC_GoodsValue, dataObject.GoodsValue);
					SetValue(container, JobContainerSchema.JC_RX_NKGoodsCurrency, dataObject.GoodsValueCurrency);

					PopulateIsNonOperativeReefer(container);

					if (dataObject.OrganizationAddressCollection != null)
					{
						var departureAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyPickupAddress));
						if (departureAddress != null)
						{
							var departureOrgAddress = new OrganisationDataObjectReader(departureAddress, logger, factory).GetMatched();
							if (departureOrgAddress != null)
							{
								SetValue(container, JobContainerSchema.JC_OA_DepartureContainerYardAddress, departureOrgAddress.PK);
							}
						}

						var arrivalAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyReturnAddress));
						if (arrivalAddress != null)
						{
							var arrivalOrgAddress = new OrganisationDataObjectReader(arrivalAddress, logger, factory).GetMatched();
							if (arrivalOrgAddress != null)
							{
								SetValue(container, JobContainerSchema.JC_OA_ArrivalContainerYardAddress, arrivalOrgAddress.PK);
							}
						}
					}

					if (dataObject.AdditionalServiceCollection != null)
					{
						var additionalservicesCollectionReader = new AdditionalServiceDataObjectCollectionReader(dataObject.AdditionalServiceCollection, logger, factory, container);
						additionalservicesCollectionReader.ReadIntoCollection();
					}

					SetValue(container, JobContainerSchema.JC_GrossVolume, container.JC_GrossVolume); //hack: trigger the SetValue to fix the value and provide logging message

					ImportWeightRelatedInfo(container);

					if (dataObject.AdditionalReferenceCollection != null)
					{
						var addtionalReferenceCollectionReader = new ContainerAdditionalReferenceCollectionReader<CommonContainer>(dataObject.AdditionalReferenceCollection, logger, factory, container);
						addtionalReferenceCollectionReader.ReadIntoCollection();
					}

					PopulateContainerPenalties(container);

					if (dataObject.AdditionalAddressInfoCollection != null)
					{
						foreach (var additionalAddressInfo in dataObject.AdditionalAddressInfoCollection)
						{
							var reader = new JobAddressAdditionalInfoDataObjectReader(additionalAddressInfo, logger, factory, container);
							reader.ReadIntoBusinessObject();
						}
					}
				}
			}
			finally
			{
				supportDataImporting.IsImportingData = false;
			}
		}

		void PopulateIsNonOperativeReefer(T container)
		{
			container[nameof(CommonContainer.JC_IsNonOperativeReefer)] = dataObject.NonOperatingReefer.GetValueOrDefault(false) ||
				dataObject.ContainerType?.Category?.Code?.ToString() == Core.Constants.ContainerTypes.DryStorage && dataObject.ContainerType?.ISOCode?.SubstringSafe(2, 1).ToString() == "R";
		}

		void PopulateContainerPenalties(T container)
		{
			if (dataObject.ContainerPenaltyCollection != null)
			{
				dataObject.ContainerPenaltyCollection.ForEach(dataObject => new ContainerPenaltyDataObjectReader(dataObject, logger, factory, container).ReadIntoBusinessObject());
			}

			var arrivalTruckWaitCost = dataObject.ArrivalTruckWaitCost ?? dataObject.ArrivalCartageDemurrageCharge;
			if (arrivalTruckWaitCost != null && arrivalTruckWaitCost != 0m)
			{
				container[nameof(CommonContainer.ArrivalTruckWaitCost)] = arrivalTruckWaitCost;
			}

			var arrivalCarrierDetentionCost = dataObject.ArrivalCarrierDetentionCost ?? dataObject.ContainerDetentionCharge;
			if (arrivalCarrierDetentionCost != null && arrivalCarrierDetentionCost != 0m)
			{
				container[nameof(CommonContainer.ArrivalCarrierDetentionCost)] = arrivalCarrierDetentionCost;
			}

			var arrivalCTOStorageCost = dataObject.ArrivalCTOStorageCost ?? dataObject.FCLStorageCharge;
			if (arrivalCTOStorageCost != null && arrivalCTOStorageCost != 0m)
			{
				container[nameof(CommonContainer.ArrivalCTOStorageCost)] = arrivalCTOStorageCost;
			}

			var arrivalCTOStorageDays = dataObject.ArrivalCTOStorageDays ?? dataObject.FCLStorageDays;
			if (arrivalCTOStorageDays != null && arrivalCTOStorageDays != 0)
			{
				container[nameof(CommonContainer.ArrivalCTOStorageDays)] = arrivalCTOStorageDays;
			}

			var arrivalTruckWaitTime = dataObject.ArrivalTruckWaitTime ?? dataObject.ArrivalCartageDemurrageTime;
			if (arrivalTruckWaitTime != null && !arrivalTruckWaitTime.Value.IsEmpty)
			{
				container[nameof(CommonContainer.ArrivalTruckWaitTime)] = arrivalTruckWaitTime;
			}

			var arrivalCarrierDetentionDays = dataObject.ArrivalCarrierDetentionDays ?? dataObject.ContainerDetentionDays;
			if (arrivalCarrierDetentionDays != null && arrivalCarrierDetentionDays != 0)
			{
				container[nameof(CommonContainer.ArrivalCarrierDetentionDays)] = arrivalCarrierDetentionDays;
			}

			var departureTruckWaitCost = dataObject.DepartureTruckWaitCost ?? dataObject.DepartureCartageDemurrageCharge;
			if (departureTruckWaitCost != null && departureTruckWaitCost != 0m)
			{
				container[nameof(CommonContainer.DepartureTruckWaitCost)] = departureTruckWaitCost;
			}

			var departureTruckWaitTime = dataObject.DepartureTruckWaitTime ?? dataObject.DepartureCartageDemurrageTime;
			if (departureTruckWaitTime != null && !departureTruckWaitTime.Value.IsEmpty)
			{
				container[nameof(CommonContainer.DepartureTruckWaitTime)] = departureTruckWaitTime;
			}
		}

		protected virtual void ImportWeightRelatedInfo(T container)
		{
			using (container.SuppressSettingRelatedWeightsFromUniversalShipment())
			{
				SetWeightValues(container);
			}

			if (dataObject.GrossWeight != null)
			{
				container.StandAloneCustomsContainer = true; // This stops total values defaulting to the Shipment.
			}
		}

		protected override bool ShouldUpdateBO => eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.Value;

		void ImportContainerInfo(T container)
		{
			if (dataObject.TareWeight != null)
			{
				using (container.SuppressSettingRelatedWeightsFromUniversalShipment())
				{
					ImportReferenceContainerInfo(container);
				}
			}
			else
			{
				ImportReferenceContainerInfo(container);
			}
		}

		protected virtual void ImportReferenceContainerInfo(T container)
		{
			SetValue(container, JobContainerSchema.JC_RC, dataObject.ContainerType);
			SetValue(container, JobContainerSchema.JC_ContainerCount, dataObject.ContainerCount);
		}

		void SetWeightValues(T container)
		{
			SetValue(container, JobContainerSchema.JC_GrossWeightUQ, dataObject.WeightUnit);
			SetValue(container, JobContainerSchema.JC_DunnageWeight, dataObject.DunnageWeight);
			SetValue(container, JobContainerSchema.JC_TareWeight, dataObject.TareWeight);
			SetValue(container, JobContainerSchema.JC_GrossWeight, dataObject.GrossWeight);
			SetValue(container, JobContainerSchema.JC_IsGrossWeightOverridden, dataObject.IsGrossWeightOverridden);
			SetValue(container, JobContainerSchema.JC_PivotBreak, dataObject.PivotBreak);
		}
	}
}
