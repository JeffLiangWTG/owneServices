using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class JTTValidUNDGExceptedQuantityChecker
	{
		public static bool DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(ForwardingUNDGDataItem undg, UNDGPackType packType, ExceptedQuantityMeasurementType measurementType)
		{
			var substance = undg?.Substance;
			if (substance == null)
			{
				return false;
			}

			var dangerousGoodsValue = GetConvertedDangerousGoodsQuantityValue(undg, measurementType);

			switch (packType)
			{
				case UNDGPackType.MultiUNDGPack:
					var mostRestrictiveCode = GetMostRestrictiveExceptedQuantityCodeInPacks(undg);
					return DoesValueExceedMaximumNetQuantityAllowedPerMultiUNDGPack(mostRestrictiveCode, dangerousGoodsValue);

				case UNDGPackType.SingleUNDGPack:
					return DoesValueExceedMaximumNetQuantityAllowedPerSingleUNDGPack(substance.DG_ExceptedQuantityCode, dangerousGoodsValue);

				default:
					return false;
			}
		}

		static string GetMostRestrictiveExceptedQuantityCodeInPacks(ForwardingUNDGDataItem undgDataItem)
		{
			var parentPackline = undgDataItem.ParentPackLine;
			if (parentPackline == null)
			{
				return string.Empty;
			}

			var allDistinctSubstances = parentPackline
				.UNDGs
				.Select(undg => undg.Substance)
				.Distinct()
				.WhereNotNull();

			return GetMostRestrictiveExceptedQuantityCodeOfSubstances(allDistinctSubstances);
		}

		static decimal GetConvertedDangerousGoodsQuantityValue(UNDGDataItem undg, ExceptedQuantityMeasurementType measurementType)
		{
			switch (measurementType)
			{
				case ExceptedQuantityMeasurementType.Weight:
					return Core.Constants.Weight.ConvertSafe(
						sourceValue: undg.DI_DGWeight,
						sourceUnitCode: undg.DI_UnitOfWeight,
						targetUnitCode: Core.Constants.Weight.Kilograms);

				case ExceptedQuantityMeasurementType.Volume:
					return Core.Constants.Volume.ConvertSafe(
						sourceValue: undg.DI_DGVolume,
						sourceUnitCode: undg.DI_UnitOfVolume,
						targetUnitCode: Core.Constants.Volume.Litre);
			}

			return 0;
		}
	}
}
