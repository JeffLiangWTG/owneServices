using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CCommodityProvider : CommodityProvider
{
	public CC044CCommodityProvider(NctsCommonCargoDesc item) : base(item)
	{
	}

	public override string DescriptionOfGoods => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent.BY_Description != unloadedItem.BY_Description ? unloadedItem.BY_Description : (item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW ? item.BY_Description : null);

	public override string HarmonizedSystemSubHeadingCode => IsHarmonisedTariffChanged(item) || item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW ? item.BY_HarmonisedTariff.SubstringSafe(0, 6) : null;

	public override string CombinedNomenclatureCode => (IsHarmonisedTariffChanged(item) || item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW) && item.BY_HarmonisedTariff.Length == 8 ? item.BY_HarmonisedTariff.SubstringSafe(6, 2) : null;

	public override IGoodsMeasure GoodsMeasure => goodsMeasure ??= new CC044CGoodsMeasureProvider(item);
	IGoodsMeasure goodsMeasure;

	bool IsHarmonisedTariffChanged(NctsCommonCargoDesc item) => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.BY_HarmonisedTariff != unloadedItem.BY_HarmonisedTariff;
}
