using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class BookingContainerBuilder
	{
		public BookingContainer Build(CommonContainer containerBO, IReadOnlyCollection<BookingPackingLine> packingLines = null, bool isFallbackContainerPackingSummariesTotalOutturnedDetails = false)
		{
			if (containerBO == null)
			{
				return null;
			}

			var context = new CommonContext(containerBO.Factory);
			var container = new BookingContainer(containerBO.PK);
			container.PackingLines = packingLines;

			PopulateGeneralInfo(container, containerBO, context, packingLines);
			PopulateSeals(container, containerBO);
			PopulateWeightAndVolume(container, containerBO, context, packingLines, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			PopulateRefrigeration(container, containerBO, context);
			PopulateMeasures(container, containerBO, context);
			PopulateExportInfo(container, containerBO, context);
			PopulateImportInfo(container, containerBO);
			PopulateVerification(container, containerBO, context);
			PopulateCollections(container, containerBO, context);
			PopulateOtherInfos(container, containerBO);
			PopulatePackingSummaries(container, containerBO, isFallbackContainerPackingSummariesTotalOutturnedDetails);

			return container;
		}

		void PopulateGeneralInfo(BookingContainer container, CommonContainer containerBO, IContext context, IReadOnlyCollection<BookingPackingLine> packingLines)
		{
			container.Number = containerBO.JC_ContainerNum;
			container.ContainerCount = containerBO.JC_ContainerCount;
			container.DeliveryMode = containerBO.JC_DeliveryMode;
			container.DeliverySequence = containerBO.JC_DeliverySequence;

			container.PackCount = packingLines?.Sum(p => p.Quantity) ?? 0;
			container.IsEmpty = container.PackCount == 0 || containerBO.JC_IsEmptyContainer;
			container.IsPartOf = ZBool.False;
			container.IsShipperOwned = containerBO.JC_IsShipperOwned;
			container.IsDamaged = containerBO.JC_IsDamaged;
			container.IsSealOk = containerBO.JC_IsSealOk;

			container.Mode = new CodeDescription(containerBO.JC_ContainerMode_List ?? new CodeDescriptionPairList())
			{
				Code = containerBO.JC_ContainerMode
			};

			container.Type = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
			{
				Code = containerBO.RefContainer?.RC_Code ?? ZString.Empty,
				ISOCode = containerBO.RefContainer?.RC_ISOType ?? ZString.Empty,
				Type = new CodeDescription(containerBO?.RefContainer?.Lookups?.ContainerTypes ?? new CodeDescriptionPairList())
				{
					Code = containerBO.RefContainer?.RC_ContainerType ?? ZString.Empty
				}
			};
			container.IsNonOperativeReefer = containerBO.JC_IsNonOperativeReefer;

			container.ContainerMode = new CodeDescription(containerBO.JC_ContainerMode_List ?? new CodeDescriptionPairList())
			{
				Code = containerBO.JC_ContainerMode
			};
			container.ContainerQuality = new CodeDescription(containerBO.Lookups.ContainerQualities)
			{
				Code = containerBO.JC_ContainerQuality
			};

			container.ContainerStatus = new CodeDescription(containerBO.Lookups.ContainerStatuses)
			{
				Code = containerBO.JC_ContainerStatus
			};

			container.Commodity = new CodeDescription(containerBO.ContainerCommodityCode_List)
			{
				Code = containerBO.JC_RH_NKContainerCommodityCode
			};

			container.GoodsValue = new Measurement
			{
				Value = containerBO.JC_GoodsValue,
				Unit = new CodeDescription(containerBO.RefCurrency_List)
				{
					Code = containerBO.JC_RX_NKGoodsCurrency
				}
			};
		}

		void PopulateSeals(BookingContainer container, CommonContainer containerBO)
		{
			container.Seal = containerBO.JC_SealNum;
			container.SealPartyType = new CodeDescription(containerBO.Lookups.SealParty_List)
			{
				Code = containerBO.JC_SealParty
			};

			container.SecondSeal = containerBO.JC_AdditionalSealNum;
			container.SecondSealPartyType = new CodeDescription(containerBO.Lookups.SealParty_List)
			{
				Code = containerBO.JC_AdditionalSealParty
			};

			container.ThirdSeal = containerBO.JC_Additional2SealNum;
			container.ThirdSealPartyType = new CodeDescription(containerBO.Lookups.SealParty_List)
			{
				Code = containerBO.JC_Additional2SealParty
			};
		}

		void PopulateWeightAndVolume(BookingContainer container, CommonContainer containerBO, IContext context, IReadOnlyCollection<BookingPackingLine> packingLines, string unitOfWeight, string unitOfVolume)
		{
			var goodsWeight = packingLines?.Sum(p => Constants.Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, unitOfWeight))
				?? Constants.Weight.Convert(containerBO.GoodsWeightForBinding, containerBO.JC_GrossWeightUQ, unitOfWeight);

			container.GoodsWeight = new Measurement
			{
				Value = goodsWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var tareWeight = Constants.Weight.Convert(containerBO.JC_TareWeight, containerBO.JC_GrossWeightUQ, unitOfWeight);
			container.TareWeight = new Measurement
			{
				Value = tareWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var dunnage = Constants.Weight.Convert(containerBO.JC_DunnageWeight, containerBO.JC_GrossWeightUQ, unitOfWeight);
			container.Dunnage = new Measurement
			{
				Value = dunnage,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var grossWeight = Constants.Weight.Convert(containerBO.JC_GrossWeight, containerBO.JC_GrossWeightUQ, unitOfWeight);
			container.GrossWeight = new Measurement()
			{
				Value = grossWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var netWeight = Constants.Weight.Convert(containerBO.JC_Calc_NetWeight, containerBO.JC_GrossWeightUQ, unitOfWeight);
			container.NetWeight = new Measurement()
			{
				Value = netWeight,
				Unit = new CodeDescription(containerBO.TotalPackagesUnit_List)
				{
					Code = unitOfWeight
				}
			};

			container.Volume = new Measurement()
			{
				Value = Constants.Volume.Convert(containerBO.JC_Calc_TotalVolume, containerBO.JC_Calc_TotalVolumeUnit, unitOfVolume),
				Unit = new CodeDescription(containerBO.TotalVolumeUnit_List)
				{
					Code = unitOfVolume
				}
			};

			var grossVolume = Constants.Volume.Convert(containerBO.JC_GrossVolume, containerBO.JC_GrossVolumeUQ, unitOfVolume);
			container.GrossVolume = new Measurement()
			{
				Value = grossVolume,
				Unit = new CodeDescription(containerBO.TotalVolumeUnit_List)
				{
					Code = unitOfVolume
				}
			};

			container.WeightCapacity = new Measurement
			{
				Value = containerBO.JC_WeightCapacity,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = containerBO.JC_WeightCapacityUQ
				}
			};

			container.VolumeCapacity = new Measurement
			{
				Value = containerBO.JC_VolumeCapacity,
				Unit = new CodeDescription(containerBO.TotalVolumeUnit_List)
				{
					Code = containerBO.JC_VolumeCapacityUQ
				}
			};
		}

		void PopulateRefrigeration(BookingContainer container, CommonContainer containerBO, IContext context)
		{
			container.HasControlledAtmosphere = containerBO.JC_IsControlledAtmosphere;
			container.TemperatureRecorderSerialNumber = containerBO.JC_TempRecorderSerialNo;
			container.RefrigGeneratorID = containerBO.JC_RefrigGeneratorID;

			container.SetTemperature = new Measurement
			{
				Value = containerBO.JC_SetPointTemp,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = containerBO.JC_SetPointTempUnit
				}
			};

			container.Humidity = new Measurement
			{
				Value = (byte)containerBO.JC_HumidityPercent,
				Unit = new CodeDescription(context.Humidity)
				{
					Code = "%"
				}
			};

			container.AirVentFlow = new Measurement
			{
				Value = containerBO.JC_AirVentFlow,
				Unit = new CodeDescription(context.AirVentFlow)
				{
					Code = containerBO.JC_AirVentFlowRateUnit
				}
			};

			container.Genset = containerBO.JC_IsControlledAtmosphere && !containerBO.JC_RefrigGeneratorID.IsEmpty;
		}

		void PopulateMeasures(BookingContainer container, CommonContainer containerBO, IContext context)
		{
			var dimensionUnit = Constants.Length.Centimetres;

			container.OverhangFront = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_Calc_OverhangFront, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangBack = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_OverhangBack, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangLeft = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_Calc_OverhangLeft, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangRight = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_OverhangRight, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangLength = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_Calc_OverhangLength, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangWidth = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_Calc_OverhangWidth, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.OverhangHeight = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_Calc_OverhangHeight, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.TotalHeight = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_TotalHeight, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.TotalWidth = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_TotalWidth, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};

			container.TotalLength = new Measurement()
			{
				Value = Constants.Length.Convert(containerBO.JC_TotalLength, Constants.Length.Feet, dimensionUnit),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = dimensionUnit
				}
			};
		}

		void PopulateExportInfo(BookingContainer container, CommonContainer containerBO, IContext context)
		{
			container.EmptyRequired = containerBO.JC_EmptyRequired;
			container.ReleaseNumber = containerBO.JC_ReleaseNum;
			container.ContainerParkEmptyPickupGateOut = containerBO.JC_ContainerYardEmptyPickupGateOut;

			container.DepartureCartageAdvised = containerBO.JC_DepartureCartageAdvised;
			container.DepartureCartageReference = containerBO.JC_DepartureCartageRef;
			container.DepartureCartageComplete = containerBO.JC_DepartureCartageComplete;
			container.DepartureTruckWaitCost = containerBO.DepartureTruckWaitCost;
			container.DepartureTruckWaitTime = containerBO.DepartureTruckWaitTime;
			container.DepartureDeliveryByRail = containerBO.JC_DepartureDeliveryByRail;
			container.DepartureSlotDateTime = containerBO.JC_DepartureSlotDateTime;
			container.DepartureSlotReference = containerBO.JC_DepartureSlotReference;
			container.DepartureEstimatedPickup = containerBO.JC_DepartureEstimatedPickup;
			container.DepartureContainerYard = AddressBuilder.Create(context, containerBO.DepartureContainerYardAddress);
			container.ExportDepotCustomsReference = containerBO.JC_ExportDepotCustomsReference;

			container.FCLWharfGateIn = containerBO.JC_FCLWharfGateIn;
			container.FCLOnBoardVessel = containerBO.JC_FCLOnBoardVessel;
		}

		void PopulateImportInfo(BookingContainer container, CommonContainer containerBO)
		{
			container.EmptyReadyForReturn = containerBO.JC_EmptyReadyForReturn;
			container.EmptyReturnedBy = containerBO.JC_EmptyReturnedBy;
			container.EmptyReturnReference = containerBO.JC_EmptyReturnReference;

			container.ArrivalPickupByRail = containerBO.JC_ArrivalPickupByRail;
			container.ArrivalSlotDateTime = containerBO.JC_ArrivalSlotDateTime;
			container.ArrivalSlotReference = containerBO.JC_ArrivalSlotReference;
			container.ArrivalCartageAdvised = containerBO.JC_ArrivalCartageAdvised;
			container.ArrivalCartageReference = containerBO.JC_ArrivalCartageRef;
			container.ArrivalCartageComplete = containerBO.JC_ArrivalCartageComplete;
			container.ArrivalTruckWaitCost = containerBO.ArrivalTruckWaitCost;
			container.ArrivalTruckWaitTime = containerBO.ArrivalTruckWaitTime;
			container.ArrivalDeliveryRequiredBy = containerBO.JC_ArrivalDeliveryRequiredBy;
			container.ArrivalEstimatedDelivery = containerBO.JC_ArrivalEstimatedDelivery;

			container.PackDate = containerBO.JC_PackDate;
			container.LCLUnpack = containerBO.JC_LCLUnpack;
			container.LCLAvailable = containerBO.JC_LCLAvailable;
			container.LCLStorageCommences = containerBO.JC_LCLStorageCommences;
			container.ContainerImportDORelease = containerBO.JC_ContainerImportDORelease;
			container.ImportDepotCustomsReference = containerBO.JC_ImportDepotCustomsReference;

			container.ArrivalCTOStorageCost = containerBO.ArrivalCTOStorageCost;
			container.ArrivalCTOStorageDays = containerBO.ArrivalCTOStorageDays;
			container.FCLWharfGateOut = containerBO.JC_FCLWharfGateOut;
			container.FCLAvailable = containerBO.JC_FCLAvailable;
			container.ArrivalCTOStorageStartDate = containerBO.JC_ArrivalCTOStorageStartDate;
			container.FCLUnloadFromVessel = containerBO.JC_FCLUnloadFromVessel;
			container.FCLHeldInTransitStaging = containerBO.JC_FCLHeldInTransitStaging;

			container.ArrivalCarrierDetentionCost = containerBO.ArrivalCarrierDetentionCost;
			container.ArrivalCarrierDetentionDays = containerBO.ArrivalCarrierDetentionDays;
			container.ContainerParkEmptyReturnGateIn = containerBO.JC_ContainerYardEmptyReturnGateIn;
		}

		void PopulateVerification(BookingContainer container, CommonContainer containerBO, IContext context)
		{
			container.VerifiedStatus = new CodeDescription(containerBO.Lookups.VGMStatusList)
			{
				Code = containerBO.JC_GrossWeightVerificationStatus
			};

			container.VerifiedMethod = new CodeDescription(containerBO.Lookups.GrossWeightVerificationTypeList)
			{
				Code = containerBO.JC_GrossWeightVerificationType
			};

			container.VerifiedByAddress = AddressBuilder.Create(context, containerBO.GrossWeightVerifiedByAddress);
			container.VerifiedDate = containerBO.JC_GrossWeightVerificationDateTime;
		}

		void PopulateCollections(BookingContainer container, CommonContainer containerBO, IContext context)
		{
			container.Numbers = ReferenceNumber.Create(context, containerBO.AdditionalReferenceNumbers);
			container.AdditionalServices = AdditionalService.Create(context, containerBO.Services);
			container.Milestones = Milestone.Create(containerBO);

			container.FumigationService = container.AdditionalServices.FirstOrDefault(s => (s.ServiceCode?.Code ?? ZString.Empty) == Constants.FreightServiceType.Codes.Fumigation);

			var fumigationService = containerBO.Services.Cast<JobService>().FirstOrDefault(v => v.ES_ServiceCode == Constants.FreightServiceType.Codes.Fumigation);
			if (fumigationService?.Contractor?.MainAddress != null)
			{
				container.ContractorCI5 = fumigationService.Contractor.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
				container.ContractorSON = fumigationService.Contractor.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			}
		}

		void PopulateOtherInfos(BookingContainer container, CommonContainer containerBO)
		{
			container.HandlingNotes = containerBO.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
			container.Fumigated = container.FumigationService != null;
			container.OversizeContainer = container.OverhangBack?.Value > 0 || container.OverhangRight?.Value > 0 || container.OverhangFront?.Value > 0 || container.OverhangLeft?.Value > 0 || container.OverhangHeight?.Value > 0;
			container.HazardousCargo = container.PackingLines != null && container.PackingLines.Any(p => p.DangerousGoods != null && p.DangerousGoods.Any());
			container.MarinePollutant = container.PackingLines != null && container.PackingLines.Any(p => p.DangerousGoods != null && p.DangerousGoods.Any(d => !(d.MarinePollutant?.Code ?? ZString.Empty).IsEmpty));
		}

		void PopulatePackingSummaries(BookingContainer container, CommonContainer containerBO, bool isFallbackTotalOutturnedDetails)
		{
			var containerPackingSummaries = new List<BookingPackingSummary>();
			var packingGroups = container.PackingLines.GroupBy(p => p.ShipmentID + p.ImportReferenceNumber);

			foreach (var packingGroup in packingGroups)
			{
				if (packingGroup.Any())
				{
					var builder = new BookingPackingSummaryBuilder();
					var firstPackingLine = packingGroup.First();
					containerPackingSummaries.Add(builder.Build(containerBO, firstPackingLine.ShipmentID, firstPackingLine.ImportReferenceNumber, packingGroup.ToList(), isFallbackTotalOutturnedDetails));
				}
			}

			container.PackingSummaries = containerPackingSummaries;
		}
	}
}
