using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingContainerMerger : BusinessObjectMerger<ForwardingContainer>
	{
		public ForwardingContainerMerger(IEnumerable<ForwardingContainer> sourceList)
			: base(sourceList)
		{
		}

		public override void DoMerge()
		{
			ForwardingContainer firstContainer = null;

			var totalTareWeight = 0m;
			var totalGrossWeight = 0m;

			foreach (var container in SourceList.ToArray())
			{
				if (container != null)
				{
					if (firstContainer == null)
					{
						firstContainer = container;

						totalTareWeight = SourceList.Sum(c => Core.Constants.Weight.Convert(c.JC_TareWeight, c.JC_GrossWeightUQ, firstContainer.JC_GrossWeightUQ));
						totalGrossWeight = SourceList.Sum(c => Core.Constants.Weight.Convert(c.JC_GrossWeight, c.JC_GrossWeightUQ, firstContainer.JC_GrossWeightUQ));
					}
					else
					{
						MergeContainers(container, firstContainer);
					}
				}
			}

			if (firstContainer != null)
			{
				var packLines = firstContainer.PackLines.Cast<ForwardingPackLine>();
				var helper = new ForwardingPackLineMerger(packLines, MergeOption.MergeAll, firstContainer.Consol);
				helper.DoMerge();

				RecalculateTareWeightAndGrossWeight(firstContainer, totalTareWeight, totalGrossWeight);
			}
		}

		void MergeContainers(ForwardingContainer source, ForwardingContainer target)
		{
			target.JC_ContainerCount += source.JC_ContainerCount;
			target.JC_GoodsValue += source.JC_GoodsValue;
			target.JC_DunnageWeight += source.JC_DunnageWeight;

			target.JC_WeightCapacity += Core.Constants.Weight.Convert(source.JC_WeightCapacity, source.JC_WeightCapacityUQ, target.JC_WeightCapacityUQ);
			target.JC_VolumeCapacity += Core.Constants.Volume.Convert(source.JC_VolumeCapacity, source.JC_VolumeCapacityUQ, target.JC_VolumeCapacityUQ);
			target.JC_GrossVolume += Core.Constants.Volume.Convert(source.JC_GrossVolume, source.JC_GrossVolumeUQ, target.JC_GrossVolumeUQ);

			foreach (var packLine in source.PackLines.Cast<PackLine>().ToArray())
			{
				source.RemovePackLine(packLine);
				target.AddPackLine(packLine);
			}

			source.Consol.Containers.RemoveAndDelete(source);
		}

		void RecalculateTareWeightAndGrossWeight(ForwardingContainer container, decimal totalTareWeight, decimal totalGrossWeight)
		{
			container.JC_TareWeight = totalTareWeight;
			container.JC_GrossWeight = totalGrossWeight;
		}

		#region Columns

		protected override IEnumerable<SchemaColumn> GetComparableColumnsCore()
		{
			return new SchemaColumn[]
			{
				JobContainerSchema.JC_Additional2SealNum,
				JobContainerSchema.JC_Additional2SealParty,
				JobContainerSchema.JC_AdditionalSealNum,
				JobContainerSchema.JC_AdditionalSealParty,
				JobContainerSchema.JC_AirVentFlow,
				JobContainerSchema.JC_AirVentFlowRateUnit,
				JobContainerSchema.JC_ArrivalCartageAdvised,
				JobContainerSchema.JC_ArrivalCartageComplete,
				JobContainerSchema.JC_ArrivalCartageRef,
				JobContainerSchema.JC_ArrivalDeliveryRequiredBy,
				JobContainerSchema.JC_ArrivalEstimatedDelivery,
				JobContainerSchema.JC_ArrivalPickupByRail,
				JobContainerSchema.JC_ArrivalSlotDateTime,
				JobContainerSchema.JC_ArrivalSlotReference,
				JobContainerSchema.JC_ContainerImportDORelease,
				JobContainerSchema.JC_ContainerMode,
				JobContainerSchema.JC_ContainerNotes,
				JobContainerSchema.JC_ContainerNum,
				JobContainerSchema.JC_ContainerQuality,
				JobContainerSchema.JC_ContainerRating,
				JobContainerSchema.JC_ContainerStatus,
				JobContainerSchema.JC_ContainerStorageLocation,
				JobContainerSchema.JC_ContainerYardEmptyPickupGateOut,
				JobContainerSchema.JC_ContainerYardEmptyReturnGateIn,
				JobContainerSchema.JC_CostSpotRate,
				JobContainerSchema.JC_CostSpotRateMode,
				JobContainerSchema.JC_DeliveryMode,
				JobContainerSchema.JC_DeliverySequence,
				JobContainerSchema.JC_DepartureCartageAdvised,
				JobContainerSchema.JC_DepartureCartageComplete,
				JobContainerSchema.JC_DepartureCartageRef,
				JobContainerSchema.JC_DepartureDeliveryByRail,
				JobContainerSchema.JC_DepartureDockReceipt,
				JobContainerSchema.JC_DepartureEstimatedPickup,
				JobContainerSchema.JC_DepartureSlotDateTime,
				JobContainerSchema.JC_DepartureSlotReference,
				JobContainerSchema.JC_Description,
				JobContainerSchema.JC_EmptyReadyForReturn,
				JobContainerSchema.JC_EmptyRequired,
				JobContainerSchema.JC_EmptyReturnedBy,
				JobContainerSchema.JC_EmptyReturnReference,
				JobContainerSchema.JC_ExportDepotCustomsReference,
				JobContainerSchema.JC_F3_NKPackType,
				JobContainerSchema.JC_FCLAvailable,
				JobContainerSchema.JC_FCLHeldInTransitStaging,
				JobContainerSchema.JC_FCLOnBoardVessel,
				JobContainerSchema.JC_FCLStorageArrivedUnderbond,
				JobContainerSchema.JC_ArrivalCTOStorageStartDate,
				JobContainerSchema.JC_FCLStorageModuleOnlyMaster,
				JobContainerSchema.JC_FCLStorageUnderbondCleared,
				JobContainerSchema.JC_FCLUnloadFromVessel,
				JobContainerSchema.JC_FCLWharfGateIn,
				JobContainerSchema.JC_FCLWharfGateOut,
				JobContainerSchema.JC_GatewaySellSpotRate,
				JobContainerSchema.JC_GatewaySellSpotRateMode,
				JobContainerSchema.JC_HarmonisedCode,
				JobContainerSchema.JC_HumidityPercent,
				JobContainerSchema.JC_ImportDepotCustomsReference,
				JobContainerSchema.JC_IsCFSRegistered,
				JobContainerSchema.JC_IsControlledAtmosphere,
				JobContainerSchema.JC_IsDamaged,
				JobContainerSchema.JC_IsNonOperativeReefer,
				JobContainerSchema.JC_IsEmptyContainer,
				JobContainerSchema.JC_IsSealOk,
				JobContainerSchema.JC_IsShipperOwned,
				JobContainerSchema.JC_JK,
				JobContainerSchema.JC_JS_FCLBookingOnlyLink,
				JobContainerSchema.JC_JSB_SupplierBooking,
				JobContainerSchema.JC_JX,
				JobContainerSchema.JC_LCLAvailable,
				JobContainerSchema.JC_LCLStorageCommences,
				JobContainerSchema.JC_LCLUnpack,
				JobContainerSchema.JC_MarksAndNumbers,
				JobContainerSchema.JC_OA_ArrivalContainerYardAddress,
				JobContainerSchema.JC_OA_DepartureContainerYardAddress,
				JobContainerSchema.JC_OH_CFSClient,
				JobContainerSchema.JC_OH_ShippingLine,
				JobContainerSchema.JC_OverhangBack,
				JobContainerSchema.JC_OverhangRight,
				JobContainerSchema.JC_OverrideFCLAvailableStorage,
				JobContainerSchema.JC_OverrideLCLAvailableStorage,
				JobContainerSchema.JC_PackDate,
				JobContainerSchema.JC_PivotBreak,
				JobContainerSchema.JC_Purpose,
				JobContainerSchema.JC_RC,
				JobContainerSchema.JC_RCA_AllocationLine,
				JobContainerSchema.JC_RefrigGeneratorID,
				JobContainerSchema.JC_ReleaseNum,
				JobContainerSchema.JC_RH_NKContainerCommodityCode,
				JobContainerSchema.JC_RH_NKRatingCommodityCode,
				JobContainerSchema.JC_RX_NKGoodsCurrency,
				JobContainerSchema.JC_RX_NKCostSpotRateCurrency,
				JobContainerSchema.JC_RX_NKGatewaySellSpotRateCurrency,
				JobContainerSchema.JC_RX_NKSellSpotRateCurrency,
				JobContainerSchema.JC_SealNum,
				JobContainerSchema.JC_SealParty,
				JobContainerSchema.JC_SellSpotRate,
				JobContainerSchema.JC_SellSpotRateMode,
				JobContainerSchema.JC_SetPointTemp,
				JobContainerSchema.JC_SetPointTempUnit,
				JobContainerSchema.JC_StowagePosition,
				JobContainerSchema.JC_TempRecorderSerialNo,
				JobContainerSchema.JC_TotalHeight,
				JobContainerSchema.JC_TotalLength,
				JobContainerSchema.JC_TotalUnitOfMeasure,
				JobContainerSchema.JC_TotalWidth,
				JobContainerSchema.JC_TrainWagonNumber,
				JobContainerSchema.JC_UnpackGang,
				JobContainerSchema.JC_UnpackShed,
				JobContainerSchema.JC_VehicleColor,
				JobContainerSchema.JC_VehicleMake,
				JobContainerSchema.JC_VehicleModel,
				JobContainerSchema.JC_VehicleNumberOfDoors,
				JobContainerSchema.JC_VehicleTransmission,
				JobContainerSchema.JC_VehicleYear,
				JobContainerSchema.JC_GrossWeightVerificationDateTime,
				JobContainerSchema.JC_GrossWeightVerificationType,
				JobContainerSchema.JC_GrossWeightVerificationStatus,
			};
		}

		protected override IEnumerable<SchemaColumn> GetIgnoredColumnsCore()
		{
			return new SchemaColumn[]
			{
				JobContainerSchema.PK,
				JobContainerSchema.JC_ContainerCount,
				JobContainerSchema.JC_GoodsValue,
				JobContainerSchema.JC_WeightCapacity,
				JobContainerSchema.JC_WeightCapacityUQ,
				JobContainerSchema.JC_VolumeCapacity,
				JobContainerSchema.JC_VolumeCapacityUQ,
				JobContainerSchema.JC_GrossWeight,
				JobContainerSchema.JC_IsGrossWeightOverridden,
				JobContainerSchema.JC_GrossWeightVerificationDateTime,
				JobContainerSchema.JC_GrossWeightVerificationType,
				JobContainerSchema.JC_GrossWeightUQ,
				JobContainerSchema.JC_GrossVolume,
				JobContainerSchema.JC_GrossVolumeUQ,
				JobContainerSchema.JC_DunnageWeight,
				JobContainerSchema.JC_TareWeight,
				JobContainerSchema.JC_ContainerJobID,
				JobContainerSchema.JC_CLH_LoadListPlan,
				JobContainerSchema.JC_IsValid,
				JobContainerSchema.JC_SystemCreateUser,
				JobContainerSchema.JC_SystemCreateBranch,
				JobContainerSchema.JC_SystemCreateDepartment,
				JobContainerSchema.JC_SystemLastEditUser,
				JobContainerSchema.JC_SystemCreateTimeUtc,
				JobContainerSchema.JC_SystemLastEditTimeUtc
			};
		}

		#endregion

		#region Check

		public override string CheckMerger()
		{
			if (SourceList.Count() <= 1)
			{
				return Res.GetString("671a003f-4ae7-4699-91fe-94618b5a6886", "Please choose at least two container lines!");
			}

			foreach (var container in SourceList)
			{
				if (!container.JC_ContainerNum.IsEmpty)
				{
					return Res.GetString("1d1282ab-c018-44a9-af06-cb5e12c81dd5", "Selected containers should not have any container numbers!");
				}

				if (container.Services.Any())
				{
					return Res.GetString("e46cf38a-4a5a-4af6-9264-7d95b73f4cdd", "Selected containers should not have any services!");
				}
			}

			return GetDifferentColumnValuesError();
		}

		#endregion
	}
}
