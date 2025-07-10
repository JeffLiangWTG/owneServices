using System.Linq;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerDataObjectWriter : DataObjectWriter<CommonContainer, Container>
	{
		public ContainerDataObjectWriter(BindToLists listCache, IDataWritingManager manager)
			: base(manager)
		{
			this.listCache = listCache;
		}

		readonly BindToLists listCache;

		protected override Container PopulateDataObject(CommonContainer containerBO)
		{
			var containerData = new Container(writeManager.WriterStrategy);

			containerData.ArrivalCartageAdvised = containerBO.JC_ArrivalCartageAdvised;
			containerData.ArrivalCartageComplete = containerBO.JC_ArrivalCartageComplete;
			containerData.ArrivalCartageDemurrageCharge = containerBO.FindArrivalTruckWaitPenalty()?.CPY_PerUnitCost;
			containerData.ArrivalTruckWaitCost = containerData.ArrivalCartageDemurrageCharge;
			containerData.ArrivalCartageDemurrageTime = containerBO.ArrivalTruckWaitTime;
			containerData.ArrivalTruckWaitTime = containerBO.ArrivalTruckWaitTime;
			containerData.ArrivalCartageRef = containerBO.JC_ArrivalCartageRef;
			containerData.ArrivalDeliveryRequiredBy = containerBO.JC_ArrivalDeliveryRequiredBy;
			containerData.ArrivalEstimatedDelivery = containerBO.JC_ArrivalEstimatedDelivery;
			containerData.ArrivalPickupByRail = containerBO.JC_ArrivalPickupByRail;
			containerData.ArrivalSlotDateTime = containerBO.JC_ArrivalSlotDateTime;
			containerData.ArrivalSlotReference = containerBO.JC_ArrivalSlotReference;
			containerData.Commodity = ListHelper.GetWithDescription<Commodity>(containerBO.JC_RH_NKContainerCommodityCode, containerBO.ContainerCommodityCode_List);
			containerData.RatingCommodity = ListHelper.GetWithDescription<Commodity>(containerBO.JC_RH_NKRatingCommodityCode, containerBO.ContainerCommodityCode_List);
			containerData.ContainerCount = containerBO.JC_ContainerCount;
			containerData.ContainerImportDORelease = containerBO.JC_ContainerImportDORelease;
			containerData.ContainerNumber = containerBO.JC_ContainerNum;
			containerData.ContainerJobID = containerBO.JC_ContainerJobID;
			containerData.ContainerQuality = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_ContainerQuality, containerBO.Lookups.ContainerQualities);
			containerData.ContainerStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_ContainerStatus, containerBO.Lookups.ContainerStatuses);
			containerData.DeliveryMode = containerBO.JC_DeliveryMode;
			containerData.DeliverySequence = containerBO.JC_DeliverySequence;
			containerData.DepartureCartageAdvised = containerBO.JC_DepartureCartageAdvised;
			containerData.DepartureCartageComplete = containerBO.JC_DepartureCartageComplete;
			containerData.DepartureCartageDemurrageCharge = containerBO.FindDepartureTruckWaitPenalty()?.CPY_PerUnitCost;
			containerData.DepartureTruckWaitCost = containerData.DepartureCartageDemurrageCharge;
			containerData.DepartureCartageDemurrageTime = containerBO.DepartureTruckWaitTime;
			containerData.DepartureTruckWaitTime = containerBO.DepartureTruckWaitTime;
			containerData.DepartureCartageRef = containerBO.JC_DepartureCartageRef;
			containerData.DepartureDeliveryByRail = containerBO.JC_DepartureDeliveryByRail;
			containerData.DepartureDockReceipt = containerBO.JC_DepartureDockReceipt;
			containerData.DepartureEstimatedPickup = containerBO.JC_DepartureEstimatedPickup;
			containerData.DepartureSlotDateTime = containerBO.JC_DepartureSlotDateTime;
			containerData.DepartureSlotReference = containerBO.JC_DepartureSlotReference;
			containerData.DunnageWeight = containerBO.JC_DunnageWeight;
			containerData.ExportDepotCustomsReference = containerBO.JC_ExportDepotCustomsReference;
			containerData.ImportDepotCustomsReference = containerBO.JC_ImportDepotCustomsReference;
			containerData.FCL_LCL_AIR = ListHelper.GetWithDescription<ContainerMode>(containerBO.JC_ContainerMode, containerBO.JC_ContainerMode_List);
			containerData.OverrideFCLAvailableStorage = containerBO.JC_OverrideFCLAvailableStorage;
			containerData.FCLAvailable = containerBO.JC_FCLAvailable;
			containerData.FCLHeldInTransitStaging = containerBO.JC_FCLHeldInTransitStaging;
			containerData.FCLOnBoardVessel = containerBO.JC_FCLOnBoardVessel;
			containerData.FCLStorageArrivedUnderbond = containerBO.JC_FCLStorageArrivedUnderbond;
			containerData.FCLStorageCharge = containerBO.FindArrivalCTOStoragePenalty()?.CPY_PerUnitCost;
			containerData.ArrivalCTOStorageCost = containerData.FCLStorageCharge;
			containerData.FCLStorageCommences = containerBO.JC_ArrivalCTOStorageStartDate;
			containerData.ArrivalCTOStorageStartDate = containerBO.JC_ArrivalCTOStorageStartDate;
			containerData.FCLStorageDays = containerBO.FindArrivalCTOStoragePenalty()?.DurationAsDays;
			containerData.ArrivalCTOStorageDays = containerData.FCLStorageDays;
			containerData.FCLStorageModuleOnlyMaster = containerBO.JC_FCLStorageModuleOnlyMaster;
			containerData.FCLStorageUnderbondCleared = containerBO.JC_FCLStorageUnderbondCleared;
			containerData.FCLUnloadFromVessel = containerBO.JC_FCLUnloadFromVessel;
			containerData.FCLWharfGateIn = containerBO.JC_FCLWharfGateIn;
			containerData.FCLWharfGateOut = containerBO.JC_FCLWharfGateOut;
			containerData.GrossWeight = containerBO.JC_GrossWeight;
			if (containerBO.JC_IsGrossWeightOverridden)
			{
				containerData.IsGrossWeightOverridden = true;
			}
			containerData.GoodsValue = containerBO.JC_GoodsValue;
			containerData.GoodsValueCurrency = ListHelper.GetWithDescription<Currency>(containerBO.JC_RX_NKGoodsCurrency, containerBO.RefCurrency_List);
			containerData.GoodsWeight = containerBO.GoodsWeightForBinding;
			containerData.IsCFSRegistered = containerBO.JC_IsCFSRegistered;
			containerData.IsDamaged = containerBO.JC_IsDamaged;
			containerData.IsSealOk = containerBO.JC_IsSealOk;
			containerData.OverrideLCLAvailableStorage = containerBO.JC_OverrideLCLAvailableStorage;
			containerData.LCLAvailable = containerBO.JC_LCLAvailable;
			containerData.LCLStorageCommences = containerBO.JC_LCLStorageCommences;
			containerData.LCLUnpack = containerBO.JC_LCLUnpack;
			containerData.PackDate = containerBO.JC_PackDate;
			if (containerBO.JC_PivotBreak != 0)
			{
				containerData.PivotBreak = containerBO.JC_PivotBreak;
			}
			containerData.ReleaseNum = containerBO.JC_ReleaseNum;
			containerData.Seal = containerBO.JC_SealNum;
			containerData.SecondSeal = containerBO.JC_AdditionalSealNum;
			containerData.SealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_SealParty, containerBO.Lookups.SealParty_List);
			containerData.SecondSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_AdditionalSealParty, containerBO.Lookups.SealParty_List);
			containerData.StowagePosition = containerBO.JC_StowagePosition;
			containerData.TareWeight = containerBO.JC_TareWeight;
			containerData.ThirdSeal = containerBO.JC_Additional2SealNum;
			containerData.ThirdSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_Additional2SealParty, containerBO.Lookups.SealParty_List);
			containerData.TotalHeight = containerBO.JC_TotalHeight;
			containerData.TotalLength = containerBO.JC_TotalLength;
			containerData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(containerBO.JC_TotalUnitOfMeasure, listCache.DimensionUnits);
			containerData.TotalWidth = containerBO.JC_TotalWidth;
			containerData.TrainWagonNumber = containerBO.JC_TrainWagonNumber;
			containerData.UnpackGang = containerBO.JC_UnpackGang;
			containerData.UnpackShed = containerBO.JC_UnpackShed;
			containerData.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(containerBO.JC_VolumeCapacityUQ, listCache.VolumeUnits);
			containerData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(containerBO.WeightUnitForBinding, listCache.WeightUnits);
			containerData.SetAdditionalServiceCollection(() => ProcessCollection(containerBO.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete));

			containerData.GrossWeightVerificationType = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_GrossWeightVerificationType, containerBO.Lookups.GrossWeightVerificationTypeList);

			if (containerBO.JC_GrossWeightVerificationType != Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified)
			{
				containerData.SetOrganizationAddressCollection(() => ProcessCollection(containerBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
				containerData.GrossWeightVerificationDateTime = containerBO.JC_GrossWeightVerificationDateTime;
			}

			ObjectFactory.Get<IUniversalMilestoneWriter>().PopulateMilestones(containerBO, containerData, writeManager.ShouldPopulateInternalMilestones);

			PopulateContainerModeSpecificData(containerBO, containerData);

			containerData.SetAdditionalReferenceCollection(() => containerBO.AdditionalReferenceNumbers.Count > 0 ? ProcessCollection(containerBO.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete) : null);

			if (containerBO.ImportPenalties.Any() || containerBO.ExportPenalties.Any() || containerBO.DeliveryPenalties.Any() || containerBO.PickupPenalties.Any())
			{
				containerData.SetContainerPenaltyCollection(() => ProcessCollection(containerBO.ImportPenalties.Union(containerBO.ExportPenalties).Union(containerBO.DeliveryPenalties).Union(containerBO.PickupPenalties), new ContainerPenaltyDataObjectWriter(writeManager)));
			}

			containerData.SetAdditionalAddressInfoCollection(() => ProcessCollection(containerBO.JobAddressAdditionalInfoCollection, new JobAddressAdditionalInfoDataObjectWriter(writeManager)));
			return containerData;
		}

		void PopulateContainerModeSpecificData(CommonContainer containerBO, Container containerData)
		{
			if (containerBO.IsContainerised)
			{
				containerData.AirVentFlow = containerBO.JC_AirVentFlow;
				containerData.AirVentFlowRateUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_AirVentFlowRateUnit, listCache.AirVentFlowRateUnits);
				containerData.ContainerDetentionCharge = containerBO.FindArrivalCarrierDetentionPenalty()?.CPY_PerUnitCost;
				containerData.ArrivalCarrierDetentionCost = containerData.ContainerDetentionCharge;
				containerData.ContainerDetentionDays = containerBO.FindArrivalCarrierDetentionPenalty()?.DurationAsDays;
				containerData.ArrivalCarrierDetentionDays = containerData.ContainerDetentionDays;
				containerData.ContainerParkEmptyPickupGateOut = containerBO.JC_ContainerYardEmptyPickupGateOut;
				containerData.ContainerParkEmptyReturnGateIn = containerBO.JC_ContainerYardEmptyReturnGateIn;
				containerData.ContainerType = ContainerType.New(containerBO.RefContainer);
				containerData.NonOperatingReefer = containerBO.JC_IsNonOperativeReefer;

				if (containerBO.JC_IsNonOperativeReefer)
				{
					if (containerData.ContainerType != null)
					{
						containerData.ContainerType.Category = new ContainerTypeCategory
						{
							Code = Constants.ContainerTypes.DryStorage,
							Description = Constants.ContainerTypeDescriptions.DryStorage
						};
					}
				}
				else if (containerBO.RefContainer?.RC_ContainerType.ToString() == Constants.ContainerTypes.DryStorage && containerBO.RefContainer?.RC_ISOType.SubstringSafe(2, 1).ToString() == "R")
				{
					containerData.NonOperatingReefer = true;
				}

				containerData.EmptyReadyForReturn = containerBO.JC_EmptyReadyForReturn;
				containerData.EmptyRequired = containerBO.JC_EmptyRequired;
				containerData.EmptyReturnedBy = containerBO.JC_EmptyReturnedBy;
				containerData.EmptyReturnRef = containerBO.JC_EmptyReturnReference;
				containerData.HumidityPercent = containerBO.JC_HumidityPercent;
				containerData.IsEmptyContainer = containerBO.JC_IsEmptyContainer;
				containerData.IsControlledAtmosphere = containerBO.JC_IsControlledAtmosphere;
				containerData.IsShipperOwned = containerBO.JC_IsShipperOwned;
				containerData.RefrigGeneratorID = containerBO.JC_RefrigGeneratorID;
				containerData.SetPointTemp = containerBO.JC_SetPointTemp;
				containerData.SetPointTempUnit = containerBO.JC_SetPointTempUnit;
				containerData.TempRecorderSerialNo = containerBO.JC_TempRecorderSerialNo;
				containerData.OverhangFront = containerBO.JC_Calc_OverhangFront;
				containerData.OverhangBack = containerBO.JC_OverhangBack;
				containerData.OverhangHeight = containerBO.JC_Calc_OverhangHeight;
				containerData.OverhangLeft = containerBO.JC_Calc_OverhangLeft;
				containerData.OverhangRight = containerBO.JC_OverhangRight;
				containerData.VolumeCapacity = containerBO.JC_VolumeCapacity;
				containerData.WeightCapacity = containerBO.JC_WeightCapacity;
				containerData.AddOrgAddress(writeManager, containerBO.DepartureContainerYardAddress, nameof(DocAddressType.ContainerYardEmptyPickupAddress));
				containerData.AddOrgAddress(writeManager, containerBO.ArrivalContainerYardAddress, nameof(DocAddressType.ContainerYardEmptyReturnAddress));
			}
		}
	}
}
