using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.ExchangeRates
{
	public class ExchangeRateBuilder : XmlBuilder<ExchangeRateData, RefExchangeRateZZ>
	{
		public ExchangeRateBuilder(ExchangeRateData sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		protected override string DataSource => "ZAExchangeRates";

		protected override string FilePrefix => "ZA_RefExchangeRate";

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override List<RefExchangeRateZZ> ConvertToRefModels()
		{
			var rates = new List<RefExchangeRateZZ>();

			foreach (var r in sourceData.ExchangeRates)
			{
				rates.Add(new RefExchangeRateZZ
				{
					ZZN_ExRateType = r.RateType,
					ZZN_RN_NKCountry = r.CountryCode,
					ZZN_RX_NKExCurrency = r.Currency,
					ZZN_StartDate = r.StartDate,
					ZZN_EndDate = r.EndDate,
					ZZN_Rate = r.Rate
				});
			}

			return rates;
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var config = new XmlWriterConfiguration();

			var exRate = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exRate.IncludeColumn(x => x.ZZN_ExRateType, true);
			exRate.IncludeColumn(x => x.ZZN_RN_NKCountry, true);
			exRate.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exRate.IncludeColumn(x => x.ZZN_StartDate, false);
			exRate.IncludeColumn(x => x.ZZN_EndDate, false);
			exRate.IncludeColumn(x => x.ZZN_Rate, false);

			config.IncludeEntityTypeConfiguration(exRate);

			return config;
		}
	}
}
