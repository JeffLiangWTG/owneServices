using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingUNDGPermissableQuantitiesHelper
	{
		public static (ZString, ZString) GetUNDGPackingInstructionAndPackingInstructionSectionIfNotPermissable(ForwardingUNDGDataItem undgDataItem)
		{
			var shipment = undgDataItem.ParentPackLine?.Shipment;
			if (!AreShipmentUNDGQuantitiesPermissable(shipment))
			{
				var transport = shipment?.MostInterestingTransport;
				if (transport != null)
				{
					var isCargoOnly = transport.JW_IsCargoOnly;
					var packingInstructionSection = undgDataItem?.DI_PackingInstructionSection ?? ZString.Empty;
					var packingInstruction = (isCargoOnly) ? (undgDataItem?.Substance?.DG_CargoPackIns ?? ZString.Empty) : (undgDataItem?.Substance?.DG_PaxPackIns ?? ZString.Empty);

					return (packingInstruction, packingInstructionSection);
				}
			}
			return (ZString.Empty, ZString.Empty);
		}

		public static bool AreRelatedShipmentsLithiumBatteriesPermissibleForPacking(ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return true;
			}

			var consols = shipment?.Consols?.Cast<ForwardingConsol>();
			var allRelatedShipments = consols.Any()
				? consols.SelectMany(relatedConsol => relatedConsol.Shipments).Cast<ForwardingShipment>()
				: new List<ForwardingShipment> { shipment };

			return AreRelatedShipmentsLithiumBatteriesPermissibleForPacking(allRelatedShipments);
		}

		public static bool AreRelatedShipmentsLithiumBatteriesPermissibleForPacking(IEnumerable<ForwardingShipment> shipments)
		{
			var undgQuantitiesArePermissableForAllShipments = shipments
				.All(shipment => AreShipmentUNDGQuantitiesPermissable(shipment));

			return AreUNDGQuantitiesPermissableForShipmentsConsignment(shipments)
				&& AreUNDGQuantitiesPermissableForULD(shipments)
				&& undgQuantitiesArePermissableForAllShipments;
		}

		public static bool AreShipmentUNDGQuantitiesPermissable(ForwardingShipment shipment)
		{
			var transport = shipment?.MostInterestingTransport;
			if (transport == null)
			{
				return true;
			}

			var isCargoOnly = transport.JW_IsCargoOnly;
			var undgDataItems = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Cast<UNDGDataItem>();

			var weightInfoOrUnitOfWeightHasError = undgDataItems.Any(item => item.DI_UnitOfWeightInfo.HasErrors() || item.DI_DGWeightInfo.HasErrors());
			if (weightInfoOrUnitOfWeightHasError)
			{
				return false;
			}

			var conflictingUNDGDataItems = undgDataItems.Where(undgDataItem =>
			{
				var undgPermissibleQuantity = UNDGPermissableQuantitiesHelper.GetPermissibleQuantityForUNDGDataItem(undgDataItem, isCargoOnly);
				if (undgPermissibleQuantity == default(UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity))
				{
					return false;
				}

				return !IsUNDGPerPackageQuantitiesValid(undgDataItem, undgPermissibleQuantity, isCargoOnly);
			});
			return !conflictingUNDGDataItems.Any();
		}

		public static bool AreUNDGQuantitiesPermissableForULD(ForwardingShipment shipment)
		{
			var transport = shipment?.MostInterestingTransport;
			if (transport == null)
			{
				return true;
			}

			var isCargoOnly = transport.JW_IsCargoOnly;
			var undgDataItems = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Cast<ForwardingUNDGDataItem>();

			var conflictingUNDGQuantities = undgDataItems.Where(undgDataItem =>
			{
				var packingInstructionSection = undgDataItem.DI_PackingInstructionSection;
				var packingInstruction = isCargoOnly
					? undgDataItem?.UNDGSubstance?.DG_CargoPackIns ?? ZString.Empty
					: undgDataItem?.UNDGSubstance?.DG_PaxPackIns ?? ZString.Empty;
				var isPI965OrPI968 = packingInstruction == LithiumBatteryConstants.RefPackingInstructions.PI965 || packingInstruction == LithiumBatteryConstants.RefPackingInstructions.PI968;
				var isAttachingToULDContainers = undgDataItem
						.ParentPackLine?
						.Cast<ForwardingPackLine>()
						.SelectMany(packLine => packLine.Containers)
						.Cast<ForwardingContainer>()
						.Any(container => container.JC_ContainerMode == Core.Constants.ContainerModes.ULD) ?? false;

				return packingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII && isPI965OrPI968 && isAttachingToULDContainers;
			});

			return !conflictingUNDGQuantities.Any();
		}

		public static bool AreUNDGQuantitiesPermissableForShipmentsConsignments(ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return true;
			}

			var consols = shipment?.Consols.Cast<ForwardingConsol>();
			var allRelatedShipments = consols.Any()
				? consols.SelectMany(relatedConsol => relatedConsol.Shipments).Cast<ForwardingShipment>()
				: new List<ForwardingShipment> { shipment };

			return AreUNDGQuantitiesPermissableForShipmentsConsignment(allRelatedShipments);
		}

		#region Implementation

		static bool IsUNDGPerPackageQuantitiesValid(UNDGDataItem undgDataItem, UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity undgPermissibleQuantity, bool isCargoOnly)
		{
			var packLineWeight = undgDataItem.DI_DGWeight;
			var packLineWeightUnits = undgDataItem.DI_UnitOfWeight;
			var packageCount = undgDataItem.DI_PackageCount;

			if (packageCount == 0)
			{
				return packLineWeight == 0;
			}

			if (isCargoOnly)
			{
				var packLineWeightWithConvertedUnits = Core.Constants.Weight.Convert(packLineWeight, packLineWeightUnits, undgPermissibleQuantity.CaoLimitUnits);
				return packLineWeightWithConvertedUnits / packageCount <= undgPermissibleQuantity.CaoLimit;
			}
			else
			{
				var packLineWeightWithConvertedUnits = Core.Constants.Weight.Convert(packLineWeight, packLineWeightUnits, undgPermissibleQuantity.PaxLimitUnits);
				return packLineWeightWithConvertedUnits / packageCount <= undgPermissibleQuantity.PaxLimit;
			}
		}

		static bool AreUNDGQuantitiesPermissableForULD(IEnumerable<ForwardingShipment> allRelatedShipments)
		{
			return allRelatedShipments.All(shipment => AreUNDGQuantitiesPermissableForULD(shipment));
		}

		static bool AreUNDGQuantitiesPermissableForShipmentsConsignment(IEnumerable<ForwardingShipment> shipments)
		{
			var consignmentRestrictedLithiumBatteryCodes = UNDGPermissableQuantitiesHelper.GetConsignmentRestrictedLithiumBatteryCodesAndSection();

			var allRelatedPackLines = shipments
				.SelectMany(relatedShipment => relatedShipment.OuterPackLines)
				.Cast<ForwardingPackLine>();

			var allRelatedUNDGS = allRelatedPackLines
				.SelectMany(relatedPackLine => relatedPackLine.UNDGs)
				.Cast<UNDGDataItem>();

			var allRelatedUNDGItems = allRelatedUNDGS
				.Where(relatedUNDG => consignmentRestrictedLithiumBatteryCodes.Contains((relatedUNDG.UNDGSubstance?.DG_UNNO ?? ZString.Empty, relatedUNDG.DI_PackingInstructionSection)));

			var undgItemsGroupedByDG = allRelatedUNDGItems
				.GroupBy(
					undgDataItem => undgDataItem.DI_DG,
					undgDataItem => undgDataItem.DI_PackageCount,
					(key, values) => new
					{
						UNDG = key,
						UNDGPackageCount = values.Sum(val => val)
					}
				);

			var undgsWithExceededQuantities = undgItemsGroupedByDG
				.Where(undg => undg.UNDGPackageCount > 1);

			return !undgsWithExceededQuantities.Any();
		}

		#endregion
	}
}
