using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CommodityProvider : ICommodity
{
	readonly NctsCommonCargoDesc goodsItem;
	readonly NctsHeader nctsHeader;

	public CommodityProvider(NctsCommonCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		this.nctsHeader = (NctsHeader)Argument.NotNull(goodsItem.Header, $"{nameof(NctsCommonCargoDesc)}.{nameof(NctsCommonCargoDesc.Header)}");
	}

	public string CusCode => goodsItem.BY_CusC4Number;

	public string DescriptionOfGoods => goodsItem.BY_Description;

	public ICommodityCode CommodityCode => commodityCode ?? (commodityCode = new CommodityCodeProvider(goodsItem));
	ICommodityCode commodityCode;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = GetDangerousGoods());
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	public IGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = new GoodsMeasureProvider(goodsItem));
	public IGoodsMeasure goodsMeasure;

	public int DescriptionOfGoodsMaxLength => CachedValueHelper.GetValue(ref descriptionOfGoodsMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 280;
		var maxLengthOutsideTransitionPeriod = 512;
		return nctsHeader.IsInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> descriptionOfGoodsMaxLength;

	IReadOnlyCollection<IDangerousGoods> GetDangerousGoods()
	{
		var undgItems = goodsItem switch
		{
			NctsDepartureCargoDesc nctsDepartureCargoDesc => nctsDepartureCargoDesc.UNDGs,
			NctsArrivalCargoDesc nctsArrivalCargoDesc => nctsArrivalCargoDesc.UNDGs,
			_ => null
		};
		return undgItems is null
			? Array.Empty<IDangerousGoods>()
			: undgItems.OfType<UNDGDataItem>()
				.Where(x => x.UNDGSubstance is not null)
				.Select((x, i) => new DangerousGoodsProvider(i + 1, x.UNDGSubstance)).ToArray();
	}
}
