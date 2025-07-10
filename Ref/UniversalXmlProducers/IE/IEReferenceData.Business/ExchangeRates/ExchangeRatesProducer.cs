using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Business;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Business
{
	public class ExchangeRatesProducer
	{
		public string ConvertToXmlFile(Dictionary<string, decimal> htmlExchangeRates, IDateTimeProvider dateTimeProvider, string exchangeRateDate, string outputPathAndFile)
			=> ConvertToXmlFile(htmlExchangeRates, dateTimeProvider, ParseAndValidateDate(exchangeRateDate, new[] { "dd/MM/yyyy", "dd/MM/yy" }, "Exchange Rate"), outputPathAndFile);

		public string ConvertToXmlFile(Dictionary<string, decimal> htmlExchangeRates, IDateTimeProvider dateTimeProvider, DateTime exchangeRateStartDate, string outputPathAndFile, bool clearLog = true)
		{
			if (exchangeRateStartDate != DateTime.MinValue)
			{
				var currentYear = exchangeRateStartDate.Year;
				var currentMonth = exchangeRateStartDate.Month;

				var startDate = exchangeRateStartDate;
				var endDate = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));
				var currencyDescriptionToCode = IrishCurrencyCodes.CurrencyCodes;

				var exchangeRates = new List<RefExchangeRateZZ>();
				var invalidRecords = new List<KeyValuePair<string, decimal>>();
				foreach (var record in htmlExchangeRates)
				{
					if (ValidateData(record, currencyDescriptionToCode, out var currencyCode))
					{
						exchangeRates.Add(new RefExchangeRateZZ()
						{
							ZZN_RX_NKExCurrency = currencyCode,
							ZZN_Rate = record.Value
						});
					}
					else
					{
						invalidRecords.Add(record);
					}
				}
				if (invalidRecords.Count > 0)
				{
					AppendInvalidDataErrorDetails(invalidRecords);
				}
				Helper.ExportToXmlFile($"{Constants.IECountryCode} Exchange Rate", outputPathAndFile, GetWriterConfiguration(startDate, endDate), dateTimeProvider.CurrentLocalDate, exchangeRates);
			}
			return ErrorBuilder.ToString();
		}

		static XmlWriterConfiguration GetWriterConfiguration(DateTime startDate, DateTime endDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, endDate);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "IE");

			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}

		static bool ValidateData(KeyValuePair<string, decimal> record, IReadOnlyDictionary<string, string> currencyDescriptionToCode, out string currencyCode)
		{
			var success = false;
			if (record.Key.Length == 3 && CurrencyCodeRegex.IsMatch(record.Key))
			{
				success = true;
				currencyCode = record.Key;
			}
			else if (currencyDescriptionToCode.TryGetValue(record.Key.ToUpperInvariant(), out currencyCode))
			{
				success = true;
			}
			return success && record.Value != decimal.Zero;
		}
		static readonly Regex CurrencyCodeRegex = new Regex(@"^[A-Z]{3}$");

		void AppendInvalidDataErrorDetails(List<KeyValuePair<string, decimal>> invalidRecords)
		{
			ErrorBuilder.AppendLine("Unable to import Exchange Rate due to invalid Description or empty Rate.");
			ErrorBuilder.AppendLine("DETAILS:");
			invalidRecords.ForEach(record => ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{record.Key} - {record.Value}"));
		}

		DateTime ParseAndValidateDate(string dateToParse, string[] formats, string description)
		{
			var dateDetails = DateTime.MinValue;
			foreach (var format in formats)
			{
				dateDetails = dateToParse.GetDateTime(format);
				if (dateDetails != DateTime.MinValue)
				{
					break;
				}
			}
			if (dateDetails == DateTime.MinValue)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Invalid format of {description} Date: {dateToParse}");
			}
			return dateDetails;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
