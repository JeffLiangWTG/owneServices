using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public class CAExchangeRates
	{
		public CAExchangeRates()
		{
		}

		public List<ForeignExchangeRates> ForeignExchangeRates { get; set; }
	}

	public class FromCurrency
	{
		public string Value { get; set; }
	}

	public class ToCurrency
	{
		public string Value { get; set; }
	}

	public class ForeignExchangeRates
	{
		public string ExchangeRateId { get; set; }
		public string Rate { get; set; }
		public string ExchangeRateEffectiveTimestamp { get; set; }
		public string ExchangeRateExpiryTimestamp { get; set; }
		public string ExchangeRateSource { get; set; }
		public FromCurrency FromCurrency { get; set; }
		public string FromCurrencyCSN { get; set; }
		public ToCurrency ToCurrency { get; set; }
		public string ToCurrencyCSN { get; set; }
	}
}
