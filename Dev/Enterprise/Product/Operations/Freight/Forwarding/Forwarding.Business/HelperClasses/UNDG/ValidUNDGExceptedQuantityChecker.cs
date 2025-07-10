using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ValidUNDGExceptedQuantityChecker
	{
		public static bool DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(UNDGDataItem undg, UNDGPackType packType)
		{
			var substance = undg?.Substance;
			if (substance == null)
			{
				return false;
			}

			decimal dangerousGoodsValue = GetConvertedDangerousGoodsQuantityValue(undg);

			switch (packType)
			{
				case UNDGPackType.MultiUNDGPack:
					return DoesValueExceedMaximumNetQuantityAllowedPerMultiUNDGPack(substance.DG_ExceptedQuantityCode, dangerousGoodsValue);

				case UNDGPackType.SingleUNDGPack:
					return DoesValueExceedMaximumNetQuantityAllowedPerSingleUNDGPack(substance.DG_ExceptedQuantityCode, dangerousGoodsValue);

				default:
					return false;
			}
		}

		#region Implementation 

		public static decimal GetConvertedDangerousGoodsQuantityValue(UNDGDataItem undg)
		{
			var substance = undg.Substance;

			if (IsDangerousSubstanceSolid(substance))
			{
				return Core.Constants.Weight.ConvertSafe(undg.DI_DGWeight, undg.DI_UnitOfWeight, Core.Constants.Weight.Kilograms);
			}
			else if (IsDangerousSubstanceLiquidOrGas(substance))
			{
				return Core.Constants.Volume.ConvertSafe(undg.DI_DGVolume, undg.DI_UnitOfVolume, Core.Constants.Volume.Litre);
			}

			return 0;
		}

		static bool IsDangerousSubstanceSolid(UNDGSubstance substance)
		{
			return Core.Constants.Weight.ContainsCode(substance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant())
				|| Core.Constants.Weight.ContainsCode(substance.DG_CargoMaxAmtUQ.ToUpperInvariant());
		}

		static bool IsDangerousSubstanceLiquidOrGas(UNDGSubstance substance)
		{
			return Core.Constants.Volume.ContainsCode(substance.DG_LQ2OrPaxMaxAmtUQ.ToUpperInvariant())
				|| Core.Constants.Volume.ContainsCode(substance.DG_CargoMaxAmtUQ.ToUpperInvariant());
		}

		#endregion
	}
}
