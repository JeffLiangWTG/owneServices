using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CommodityProvider : INCTSCommodity
{
	protected readonly NctsCommonCargoDesc item;
	public CommodityProvider(NctsCommonCargoDesc item)
	{
		this.item = Argument.NotNull(item, nameof(item));
	}

	public virtual string DescriptionOfGoods => item.BY_Description;

	public string CusCode => item.BY_CusC4Number;

	public virtual string HarmonizedSystemSubHeadingCode => item.BY_HarmonisedTariff.Left(6);

	public virtual string CombinedNomenclatureCode => item.BY_HarmonisedTariff.SubstringSafe(6, 2);

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ??= NctsDataRetrieveMethods.GetUNDGDataItems(item).Select((undg, index) => new DangerousGoodsProvider(undg, index + 1)).ToArray<IDangerousGoods>();
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	public decimal? GrossMass => item.GrossMassInKilograms;

	public decimal? NetMass => NctsDataRetrieveMethods.NetMassInKilogramsNullableByPreviousDocument(item);

	public decimal SupplementaryQty => item.BY_CustomsSecondQuantity.Normalize();

	public virtual IGoodsMeasure GoodsMeasure => goodsMeasure ??= new GoodsMeasureProvider(item);
	IGoodsMeasure goodsMeasure;

	public decimal InvoiceLine => 0m;

	public string QuotaOrderNumber => null;

	public string TypeOfGoods => null;
}
