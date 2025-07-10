using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ExchangeRates
{
	public static class ExchangeRateParser
	{
		public static string ConvertToXmlFile(omregningKursListe xmlData, string lastUpdated, string outputFileWithPath)
		{
			_ = xmlData ?? throw new ArgumentNullException(nameof(xmlData));
			ErrorBuilder.Clear();
			var writerConfiguration = GetRefExchangeRateWriterConfiguration();
			var result = new List<RefExchangeRateZZ>();

			var groupedExchangeRates = xmlData.ConversionRate.GroupBy(x =>
			(
				x.DateStart,
				x.DateEnd,
				x.CurrencyCode,
				x.CurrencyRate,
				x.Multiplier
			));

			foreach (var exchangeRate in groupedExchangeRates)
			{
				var (valid, exchangeRateZz) = Convert(exchangeRate.Key);
				if (valid)
				{
					result.Add(exchangeRateZz);
				}
			}

			if (result.Any())
			{
				FileHelper.ExportToXMLFile("NO Exchange Rate", outputFileWithPath, writerConfiguration, DataHelpers.GetModifiedDateTime(lastUpdated, ErrorBuilder), result);
			}

			return ErrorBuilder.ToString();
		}

		static (bool Valid, RefExchangeRateZZ ExchangeRateZZ) Convert((string StartDate, string EndDate, string Currency, string Rate, short Factor) exchangeRate)
		{
			var (startDateOk, startDate) = exchangeRate.StartDate.TryParseDateTime();
			var (endDateOk, endDate) = exchangeRate.EndDate.TryParseEndDateTime();
			var currency = exchangeRate.Currency;
			var (rateOk, rate) = ConvertUnits(exchangeRate.Rate, exchangeRate.Factor);

			if (startDateOk && endDateOk && startDate < endDate && currency.Length == 3 && rateOk && rate > decimal.Zero)
			{
				return (true, new RefExchangeRateZZ
				{
					ZZN_StartDate = startDate,
					ZZN_EndDate = endDate,
					ZZN_Rate = rate,
					ZZN_AsPublished = exchangeRate.Rate,
					ZZN_RX_NKExCurrency = currency
				});
			}

			ErrorBuilder.AppendLine("Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates.");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Currency: {0}", exchangeRate.Currency).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Rate: {0}", exchangeRate.Rate).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Factor: {0}", exchangeRate.Factor).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", exchangeRate.StartDate).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", exchangeRate.EndDate).AppendLine();
			return (false, null);
		}

		static (bool Valid, decimal Result) ConvertUnits(string rate, short factor)
		{
			var result = 0m;
			var valid = decimal.TryParse(rate, NumberStyles.Number, new CultureInfo("no-NO"), out var dblRate) && factor > decimal.Zero;
			if (valid)
			{
				result = dblRate / factor;
			}

			return (valid, result);
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, isKeyColumn: true, "CUS");
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, isKeyColumn: true, Constants.CountryCodes.Norway);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, isKeyColumn: false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, isKeyColumn: false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, isKeyColumn: true);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_AsPublished, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}
		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
