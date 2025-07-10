using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class GoodsMeasure : IGoodsMeasure
	{
		public GoodsMeasure(ZDecimal tariffQuantity, ZString unitCode, decimal netWeightMeasure = 0m)
			: this(tariffQuantity, unitCode, ZString.Empty, netWeightMeasure)
		{
		}

		public GoodsMeasure(ZDecimal tariffQuantity, ZString unitCode, ZString customUnitCode, decimal netWeightMeasure = 0m)
		{
			NetWeightMeasure = netWeightMeasure;
			TariffQuantity = tariffQuantity;
			UnitCode = unitCode;
			CustomUnitCode = customUnitCode;
		}

		public ZDecimal NetWeightMeasure { get; }

		public ZDecimal TariffQuantity { get; }

		public ZString UnitCode { get; }

		public ZString CustomUnitCode { get; }
	}
}
