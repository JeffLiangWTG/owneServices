using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class GoodsMeasureProvider : IGoodsMeasure
{
	protected readonly NctsCommonCargoDesc item;
	public GoodsMeasureProvider(NctsCommonCargoDesc item)
	{
		this.item = Argument.NotNull(item, nameof(item));
	}

	public virtual decimal GrossMassMeasure => item.GrossMassInKilograms.Normalize();

	public virtual decimal NetNetWeightMeasure => item.NetMassInKilograms.Normalize();

	public decimal? TariffQuantity => decimal.Zero;

	public decimal? SupplementaryUnitsQty => item.BY_CustomsSecondQuantity == 0 ? null : item.BY_CustomsSecondQuantity.Normalize();
}
