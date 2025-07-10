using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class CustomsExchangeRatesBuilder : ExchangeRatesBuilder<ExchangeRate>
	{
		public CustomsExchangeRatesBuilder(StringBuilder errorCollector) : base(errorCollector) { }

		const string ExRateType = "CUS";

		protected override IEnumerable<RefExchangeRateZZ> ConvertToRefExchangeRateZZs(IEnumerable<ExchangeRate> data, DateTime activationDate)
		{
			var results = new List<RefExchangeRateZZ>();
			var commonEndDate = GetEndDate(activationDate);

			foreach (var ad in data)
			{
				var newRefExchangeRateZZ = new RefExchangeRateZZ
				{
					ZZN_StartDate = activationDate,
					ZZN_EndDate = commonEndDate,
					ZZN_Rate = ad.RateNumeric,
					ZZN_RX_NKExCurrency = ad.CurrencyTypeCode
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
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DefaultValues.NLCountryCode);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, false, activationDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, GetEndDate(activationDate));
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		protected override string XMLWriterDataSource => "NL Customs Exchange Rates";

		protected override UpdateType GetUpdateType() => UpdateType.Full;

		static DateTime GetEndDate(DateTime startDate) => new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddMinutes(-1);
	}
}
