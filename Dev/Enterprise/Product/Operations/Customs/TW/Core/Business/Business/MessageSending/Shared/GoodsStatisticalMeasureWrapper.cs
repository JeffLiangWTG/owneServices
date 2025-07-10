using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class GoodsStatisticalMeasureWrapper : IGoodsStatisticalMeasure
	{
		readonly ZDecimal customsQuantity;
		readonly ZString customsUnitQty;

		public GoodsStatisticalMeasureWrapper(ZDecimal customsQuantity, ZString customsUnitQty)
		{
			this.customsQuantity = customsQuantity;
			this.customsUnitQty = customsUnitQty;
		}

		ZString IGoodsStatisticalMeasure.StatisticalUnitCode => customsUnitQty;

		ZDecimal IGoodsStatisticalMeasure.TariffQuantity => customsQuantity;
	}
}
