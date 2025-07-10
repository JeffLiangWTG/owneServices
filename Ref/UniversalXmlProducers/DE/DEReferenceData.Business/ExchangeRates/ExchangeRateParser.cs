using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.ExchangeRates
{
	public class ExchangeRateParser
	{
		public ExchangeRateParser(DownloadResult downloadResult)
		{
			this.downloadResult = downloadResult;
		}
		readonly DownloadResult downloadResult;

		public string ConvertToXMLFile(string outputFilePath, string dataSource, string kursartValue)
		{
			var result = new List<RefExchangeRateZZ>();
			ErrorBuilder.Clear();

			var xml = Helper.DeserializeFromString<kurse>(downloadResult.Content);
			if (xml.kurs != null)
			{
				var groupedExchangeRates = xml.kurs.GroupBy(x =>
				(
					x.startdatum,
					x.enddatum,
					x.iso3,
					x.kurswert
				));
				foreach (var exchangeRate in groupedExchangeRates)
				{
					var (valid, exchangeRateZZ) = Convert(exchangeRate);
					if (valid)
					{
						result.Add(exchangeRateZZ);
					}
				}
			}

			if (result.Any())
			{
				var writerConfiguration = GetRefExchangeRateWriterConfiguration(kursartValue == DownloadExchangeRates.IATAKursartValue ? IATExRateType : CUSExRateType);
				Helper.ExportToXMLFile(dataSource, outputFilePath, writerConfiguration, downloadResult.LastModified.LocalDateTime, result);
			}

			return ErrorBuilder.ToString();
		}

		(bool valid, RefExchangeRateZZ exchangeRateZZ) Convert(IGrouping<(string startDate, string endDate, string currencyCode, string rate), kurs> exchangeRate)
		{
			var exchangeRateEntry = exchangeRate.Key;

			var (startDateOk, startDate) = exchangeRateEntry.startDate.GetDateTime("dd.MM.yyyy");
			var (endDateOk, endDate) = exchangeRateEntry.endDate.GetDateTime("dd.MM.yyyy");
			var currency = exchangeRateEntry.currencyCode;
			var rateOk = decimal.TryParse(exchangeRateEntry.rate, NumberStyles.Number, new CultureInfo("de-DE"), out var rate);

			if (rateOk)
			{
				if (rate > decimal.Zero && startDateOk && endDateOk && startDate <= endDate && !string.IsNullOrEmpty(currency))
				{
					return (true, new RefExchangeRateZZ
					{
						ZZN_StartDate = startDate,
						ZZN_EndDate = endDate,
						ZZN_Rate = rate,
						ZZN_RX_NKExCurrency = currency
					});
				}
				else
				{
					ErrorBuilder.AppendLine("Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates.");
					ErrorBuilder.AppendLine("DETAILS:");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency: {exchangeRateEntry.currencyCode}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate: {exchangeRateEntry.rate}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {exchangeRateEntry.startDate}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {exchangeRateEntry.endDate}");
				}
			}
			return (false, null);
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string rateType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, rateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "DE");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		const string CUSExRateType = "CUS";
		const string IATExRateType = "IAT";
	}
}
