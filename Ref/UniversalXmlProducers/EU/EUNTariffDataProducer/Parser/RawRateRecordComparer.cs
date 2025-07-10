using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class RawRateRecordComparer
	{
		public static bool AreEqual(IRawRateRecord x, IRawRateRecord y)
		{
			Argument.NotNull(x, nameof(x));
			Argument.NotNull(y, nameof(y));

			return x.TariffHeader == y.TariffHeader
					&& x.TradeGroup == y.TradeGroup
					&& (!string.IsNullOrEmpty(x.AdditionalCode) && x.AdditionalCode == y.AdditionalCode
						|| (string.IsNullOrEmpty(x.AdditionalCode) && string.IsNullOrEmpty(y.AdditionalCode)))
					&& (!string.IsNullOrEmpty(x.OrderNumber) && x.OrderNumber == y.OrderNumber
						|| (string.IsNullOrEmpty(x.OrderNumber) && string.IsNullOrEmpty(y.OrderNumber)))
					&& (!string.IsNullOrEmpty(x.MeasureTypeId) && x.MeasureTypeId == y.MeasureTypeId
						|| (string.IsNullOrEmpty(x.MeasureTypeId) && string.IsNullOrEmpty(y.MeasureTypeId)))
					&& (!string.IsNullOrEmpty(x.RateCode) && x.RateCode == y.RateCode
						|| (string.IsNullOrEmpty(x.RateCode) && string.IsNullOrEmpty(y.RateCode)))
					&& (x.StartDate == y.StartDate);
		}
	}
}
