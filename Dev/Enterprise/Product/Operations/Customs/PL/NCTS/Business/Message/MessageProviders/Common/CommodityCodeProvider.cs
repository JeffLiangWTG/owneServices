using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CommodityCodeProvider : ICommodityCode
{
	public CommodityCodeProvider(NctsCommonCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		header = (NctsHeader)Argument.NotNull(goodsItem.Header, $"{nameof(NctsCommonCargoDesc)}.{nameof(NctsCommonCargoDesc.Header)}");
	}

	readonly NctsCommonCargoDesc goodsItem;
	readonly NctsHeader header;

	public string HarmonizedSystemSubHeadingCode => goodsItem.BY_HarmonisedTariff.SubstringSafe(0, 6);

	public string CombinedNomenclatureCode => CachedValueHelper.GetValue(ref combinedNomenclatureCode, () => CheckRuleC0821() ? goodsItem.BY_HarmonisedTariff.SubstringSafe(6, 2) : null);
	CachedValue<string> combinedNomenclatureCode;

	bool CheckRuleC0821() => header.IsArrivalMovement || !(header.MovementHeader.DepartureCustomsOffice is NctsPLOfficeCode departureOffice && departureOffice.IsInCL112CountryList);
}
