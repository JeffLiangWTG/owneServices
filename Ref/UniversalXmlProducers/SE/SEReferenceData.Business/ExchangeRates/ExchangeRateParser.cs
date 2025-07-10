using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Business
{
	public class ExchangeRateParser
	{
		public ExchangeRateParser(monetaryExchangePeriod monetaryExchangePeriod)
		{
			this.monetaryExchangePeriod = monetaryExchangePeriod;
		}
		readonly monetaryExchangePeriod monetaryExchangePeriod;

		public string ConvertToXMLFile(string outputFilePath, IDateTimeProvider dateTimeProvider)
		{
			var errors = new StringBuilder();
			var publicationDate = dateTimeProvider.CurrentLocalDate;
			var startDate = monetaryExchangePeriod.dateStart;
			var endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
			var writerConfiguration = GetRefExchangeRateWriterConfiguration(startDate, endDate);
			var result = new List<RefExchangeRateZZ>();
			foreach (var exchangeRate in monetaryExchangePeriod.monetaryExchangeRate)
			{
				if (CheckDataIsValid(errors, exchangeRate.monetaryConversionRate, exchangeRate.monetaryUnitCode, exchangeRate.calculationUnit))
				{
					result.Add(new RefExchangeRateZZ
					{
						ZZN_Rate = Math.Round(CountMonetaryRate(exchangeRate.monetaryConversionRate, exchangeRate.calculationUnit), 9),
						ZZN_AsPublished = exchangeRate.monetaryConversionRate.ToString(CultureInfo.InvariantCulture),
						ZZN_RX_NKExCurrency = exchangeRate.monetaryUnitCode
					});
				}
			}
			SEReferenceData.Business.Helper.ExportToXMLFile("SE Exchange Rates", outputFilePath, writerConfiguration, publicationDate, result);
			return errors.ToString();
		}

		static bool CheckDataIsValid(StringBuilder errors, decimal exchangeRate, string exchangeRateCurrency, long calculationUnit)
		{
			var result = true;
			if (!(decimal.Compare(exchangeRate, 0.00M) > 0) || string.IsNullOrEmpty(exchangeRateCurrency) || (!calculationUnit.Equals(1L) && !calculationUnit.Equals(100L)))
			{
				errors.AppendLine("Unable to import Exchange Rate due to invalid combination, Rate or Currency Code or Calculation unit. DETAILS:");
				errors.AppendLine(CultureInfo.InvariantCulture, $"Rate: {exchangeRate.ToString(CultureInfo.InvariantCulture)}");
				errors.AppendLine(CultureInfo.InvariantCulture, $"Currency: {exchangeRateCurrency.ToString(CultureInfo.InvariantCulture)}");
				errors.AppendLine(CultureInfo.InvariantCulture, $"Calculation unit: {calculationUnit.ToString(CultureInfo.InvariantCulture)}");
				result = false;
			}
			return result;
		}

		static decimal CountMonetaryRate(decimal rate, decimal calculationUnit) => 1 / (rate * calculationUnit);

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime startDate, DateTime endDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_AsPublished, false);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, endDate);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "SE");
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}
	}
}
