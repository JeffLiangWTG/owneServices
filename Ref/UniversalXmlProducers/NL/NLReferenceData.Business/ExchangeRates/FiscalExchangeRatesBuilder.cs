using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class FiscalExchangeRatesBuilder : ExchangeRatesBuilder<RateNode>
	{
		public FiscalExchangeRatesBuilder(StringBuilder errorCollector) : base(errorCollector) { }

		const string ExRateType = "CUD";

		protected override IEnumerable<RefExchangeRateZZ> ConvertToRefExchangeRateZZs(IEnumerable<RateNode> data, DateTime activationDate)
		{
			var results = new List<RefExchangeRateZZ>();
			foreach (var ad in data)
			{
				var newRefExchangeRateZZ = new RefExchangeRateZZ
				{
					ZZN_StartDate = activationDate,
					ZZN_EndDate = Constants.DefaultValues.MaximumDateTime,
					ZZN_Rate = ad.Rate,
					ZZN_RX_NKExCurrency = ad.Currency
				};
				results.Add(newRefExchangeRateZZ);
			}

			return results;
		}

		protected override XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime activationDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, ExRateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DefaultValues.NLCountryCode);
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, false, activationDate);
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, Constants.DefaultValues.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}
	}
}
