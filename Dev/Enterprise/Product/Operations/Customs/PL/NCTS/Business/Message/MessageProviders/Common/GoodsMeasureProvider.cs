using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class GoodsMeasureProvider : IGoodsMeasure
{
	public GoodsMeasureProvider(NctsCommonCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		this.nctsHeader = (NctsHeader)Argument.NotNull(goodsItem.Header, $"{nameof(NctsCommonCargoDesc)}.{nameof(NctsCommonCargoDesc.Header)}");
	}

	readonly NctsCommonCargoDesc goodsItem;
	readonly NctsHeader nctsHeader;

	public decimal? NetMass => CachedValueHelper.GetValue(ref netMass, () => WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, goodsItem.NetMassInKilograms));
	CachedValue<decimal?> netMass;

	public decimal? SupplementaryUnits => CachedValueHelper.GetValue(ref supplementaryUnits,
		() => goodsItem.BY_CustomsSecondQuantity.IsEmpty ? (ZDecimal?)null : goodsItem.BY_CustomsSecondQuantity);
	CachedValue<decimal?> supplementaryUnits;

	public decimal? GrossMass => CachedValueHelper.GetValue(ref grossMass,
		() => CheckRuleC0837() ? WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, goodsItem.GrossMassInKilograms) : decimal.Zero);
	CachedValue<decimal?> grossMass;

	bool CheckRuleC0837() => !(((NctsCommonMovementHeader)nctsHeader.MovementHeader) ?? nctsHeader.ArrivalMovementHeader).BM_ReducedDatasetIndicator;
}
