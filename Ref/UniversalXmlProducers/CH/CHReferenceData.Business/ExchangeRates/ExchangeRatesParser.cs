using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public class ExchangeRatesParser
	{
		readonly DownloadResult download;

		public ExchangeRatesParser(DownloadResult download)
		{
			this.download = download;
		}

		public string ConvertToRefXML(string outputFilePath)
		{
			ErrorBuilder.Clear();
			var writerConfiguration = GetRefExchangeRateWriterConfiguration();
			var inputDoc = Helper.DeserializeXML<wechselkurse>(download.Content);
			var outputRates = new List<RefExchangeRateZZ>();

			var validDates = ParseValidDates(inputDoc.gueltigkeit);

			foreach (var record in inputDoc.devise)
			{
				var currency = record.code?.ToUpperInvariant();
				var publishedRate = record.kurs;
				var scale = record.waehrung.Split(' ').FirstOrDefault();
				if (ValidRecord(record, currency, publishedRate, scale))
				{
					var rate = (decimal)(publishedRate / int.Parse(scale, CultureInfo.InvariantCulture));
					foreach (var startDate in validDates)
					{
						outputRates.Add(new RefExchangeRateZZ
						{
							ZZN_RX_NKExCurrency = currency,
							ZZN_StartDate = startDate,
							ZZN_EndDate = startDate.EndOfDay(),
							ZZN_Rate = rate,
							ZZN_AsPublished = publishedRate.ToString("0.########", CultureInfo.InvariantCulture).Truncate(10)
						});
					}
				}
			}
			var published = ParseCustomsDateTime(inputDoc.datum);
			Helper.ExportToXMLFile("CH Exchange Rate", outputFilePath, writerConfiguration, published, outputRates, UpdateType.Partial);
			return ErrorBuilder.ToString();
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "CH");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_AsPublished, false);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		static IEnumerable<DateTime> ParseValidDates(string dmyDateList)
		{
			foreach (var dmyDate in dmyDateList.Split(','))
			{
				yield return DateTime.ParseExact(dmyDate, "d'.'M'.'yyyy", CultureInfo.InvariantCulture);
			}
		}

		static DateTime ParseCustomsDateTime(string ymdDate) => DateTime.ParseExact(ymdDate, "dd'.'MM'.'yyyy", CultureInfo.InvariantCulture);

		bool ValidRecord(wechselkurseDevise record, string currency, double publishedRate, string scale)
		{
			var result = true;
			if (string.IsNullOrEmpty(currency) || publishedRate < 0 || !int.TryParse(scale, out _))
			{
				result = false;
				ErrorBuilder.AppendLine("DETAILS:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency (Code): {record.code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description (English): {record.land_en}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency (Waehrung): {record.waehrung}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate (Kurs): {record.kurs}");
			}
			return result;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
