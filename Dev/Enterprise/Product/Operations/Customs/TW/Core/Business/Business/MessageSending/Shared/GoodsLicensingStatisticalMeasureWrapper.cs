using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class GoodsLicensingStatisticalMeasureWrapper : IGoodsLicensingStatisticalMeasure
	{
		public GoodsLicensingStatisticalMeasureWrapper(ZDecimal licensingQuantity, ZString statisticalUnitCode)
		{
			this.licensingQuantity = licensingQuantity;
			this.statisticalUnitCode = statisticalUnitCode;
		}

		ZDecimal IGoodsLicensingStatisticalMeasure.LicensingQuantity => licensingQuantity;

		ZString IGoodsLicensingStatisticalMeasure.StatisticalUnitCode => statisticalUnitCode;

		ZString IGoodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency => ZString.Empty;

		readonly ZDecimal licensingQuantity;

		readonly ZString statisticalUnitCode;
	}
}
