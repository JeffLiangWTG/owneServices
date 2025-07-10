using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGPermissableQuantitiesHelper;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ValidUNDGMaximumQuantityChecker
	{
		static bool IsDGUsingIATAStandard(UNDGDataItem undg)
		{
			return (undg.UNDGSubstance != null && undg.UNDGSubstance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
		}

		public static bool DoesDGWeightExceedLimitedQuantityLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_LQMaxAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return DoesDGWeightExceedPassengerAndCargoLimit(undg);
			}

			if (undg.DI_PackageCount == 0)
			{
				return undg.DI_DGWeight != 0;
			}

			var weightLimitConvertedToTheSameUnitsAsActualWeight = Core.Constants.Weight.ConvertSafe(undg.UNDGSubstance.DG_LQMaxAmt, undg.UNDGSubstance.DG_LQMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfWeight);

			return undg.DI_DGWeight / undg.DI_PackageCount > weightLimitConvertedToTheSameUnitsAsActualWeight;
		}

		public static bool DoesDGWeightExceedPassengerAndCargoLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_LQ2OrPaxMaxAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return false;
			}

			if (undg.DI_PackageCount == 0)
			{
				return undg.DI_DGWeight != 0;
			}

			var weightLimitConvertedToTheSameUnitsAsActualWeight = Core.Constants.Weight.ConvertSafe(undg.UNDGSubstance.DG_LQ2OrPaxMaxAmt, undg.UNDGSubstance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfWeight);

			return undg.DI_DGWeight / undg.DI_PackageCount > weightLimitConvertedToTheSameUnitsAsActualWeight;
		}

		public static bool DoesDGWeightExceedCargoLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_CargoPackAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return false;
			}

			if (undg.DI_PackageCount == 0)
			{	
				return undg.DI_DGWeight != 0;
			}

			var weightLimitConvertedToTheSameUnitsAsActualWeight = Core.Constants.Weight.ConvertSafe(undg.UNDGSubstance.DG_CargoMaxAmt, undg.UNDGSubstance.DG_CargoMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfWeight);
			var undgPermissibleQuantity = UNDGPermissableQuantitiesHelper.GetPermissibleQuantityForUNDGDataItem(undg, true);
			if (undg.Substance.DG_CargoPackIns != LithiumBatteryConstants.RefPackingInstructions.Forbidden && undgPermissibleQuantity != default(UNDGPermissibleQuantity))
			{
				weightLimitConvertedToTheSameUnitsAsActualWeight = Core.Constants.Weight.ConvertSafe(undgPermissibleQuantity.CaoLimit, undgPermissibleQuantity.CaoLimitUnits, undg.DI_UnitOfWeight);
			}

			return undg.DI_DGWeight / undg.DI_PackageCount > weightLimitConvertedToTheSameUnitsAsActualWeight;
		}

		public static bool DoesDGVolumeExceedLimitedQuantityLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_LQMaxAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return DoesDGVolumeExceedPassengerAndCargoLimit(undg);
			}

			if (undg.DI_PackageCount == 0)
			{
				return undg.DI_DGVolume != 0;
			}

			var volumeLimitConvertedToTheSameUnitsAsActualVolume = Core.Constants.Volume.ConvertSafe(undg.UNDGSubstance.DG_LQMaxAmt, undg.UNDGSubstance.DG_LQMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfVolume);

			return undg.DI_DGVolume / undg.DI_PackageCount > volumeLimitConvertedToTheSameUnitsAsActualVolume;
		}

		public static bool DoesDGVolumeExceedPassengerAndCargoLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_LQ2OrPaxMaxAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return false;
			}

			if (undg.DI_PackageCount == 0)
			{
				return undg.DI_DGVolume != 0;
			}

			var volumeLimitConvertedToTheSameUnitsAsActualVolume = Core.Constants.Volume.ConvertSafe(undg.UNDGSubstance.DG_LQ2OrPaxMaxAmt, undg.UNDGSubstance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfVolume);

			return undg.DI_DGVolume / undg.DI_PackageCount > volumeLimitConvertedToTheSameUnitsAsActualVolume;
		}

		public static bool DoesDGVolumeExceedCargoLimit(UNDGDataItem undg)
		{
			if (!IsDGUsingIATAStandard(undg))
			{
				return false;
			}

			if (undg.UNDGSubstance.DG_CargoPackAmtType != UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode)
			{
				return false;
			}

			if (undg.DI_PackageCount == 0)
			{
				return undg.DI_DGVolume != 0;
			}

			var volumeLimitConvertedToTheSameUnitsAsActualVolume = Core.Constants.Volume.ConvertSafe(undg.UNDGSubstance.DG_CargoMaxAmt, undg.UNDGSubstance.DG_CargoMaxAmtUQ.ToUpperInvariant(), undg.DI_UnitOfVolume);

			return undg.DI_DGVolume / undg.DI_PackageCount > volumeLimitConvertedToTheSameUnitsAsActualVolume;
		}

		public static bool ShouldCheckVolume(UNDGSubstance substance)
		{
			return Enterprise.Core.Constants.Volume.ContainsCode(substance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant())
				|| Enterprise.Core.Constants.Volume.ContainsCode(substance.DG_CargoMaxAmtUQ.ToUpperInvariant());
		}

		public static bool ShouldCheckWeight(UNDGSubstance substance)
		{
			return Enterprise.Core.Constants.Weight.ContainsCode(substance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant())
				|| Enterprise.Core.Constants.Weight.ContainsCode(substance.DG_CargoMaxAmtUQ.ToUpperInvariant());
		}
	}
}
