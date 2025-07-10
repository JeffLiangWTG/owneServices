using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public class ExchangeRateSchema
	{
		public ExchangeRateSchema()
		{
		}

		[Index("ISOCode", 2)]
		public string ISOCode { get; set; }

		[Index("Country/Union", 3)]
		public string CountryUnion { get; set; }

		[Index("Rate", 4)]
		public string Rate { get; set; }

		[Index("IND", 5)]
		public string IND { get; set; }

		[Index("CUR Code", 6)]
		public string CURCode { get; set; }

		public RefExchangeRateZZ ToRefExchangeRate()
		{
			return !Rate.Equals(Constants.ExchangeRate.InvalidRate, System.StringComparison.OrdinalIgnoreCase) && decimal.TryParse(Rate, out var rate)
					? new RefExchangeRateZZ()
					{
						ZZN_RN_NKCountry = Constants.USCountryCode,
						ZZN_ExRateType = Constants.ExchangeRate.DefaultRateType,

						ZZN_Rate = rate,
						ZZN_RX_NKExCurrency = CURCode
					}
					: null;
		}
	}
}
