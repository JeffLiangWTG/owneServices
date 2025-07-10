using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public class ExchangeRateParser
	{
		public ExchangeRateParser(Tarih_Date exchangeRates)
		{
			this.exchangeRates = exchangeRates;
		}
		readonly Tarih_Date exchangeRates;

		public string ConvertToXMLFile(string outputFilePath, string exchangeRateType)
		{
			ErrorBuilder.Clear();
			var publicationDateFromCodeList = exchangeRates.Tarih.GetDateTime("dd.MM.yyyy");
			if (publicationDateFromCodeList.SuccessfullyParsed)
			{
				ExchangeDateCalc.AvailableDateRange(publicationDateFromCodeList.DateTime, out DateTime exchangeStartDate, out DateTime exchangeEndDate);
				var isExport = exchangeRateType == Constants.ExchangeRateTypes.Export;
				var writerConfiguration = GetRefExchangeRateWriterConfiguration(exchangeRateType, exchangeStartDate, exchangeEndDate);
				var result = new List<RefExchangeRateZZ>();
				foreach (var rate in exchangeRates.Currency.Where(x => !string.IsNullOrEmpty(x.ForexBuying) && !string.IsNullOrEmpty(x.ForexSelling)))
				{
					result.Add(new RefExchangeRateZZ
					{
						ZZN_Rate = isExport ? GetRate(rate.ForexBuying) : GetRate(rate.ForexSelling),
						ZZN_RX_NKExCurrency = rate.CurrencyCode
					});
				}
				Helper.ExportToXMLFile("TR Exchange Rates " + (isExport ? "Export" : "Import"), outputFilePath, writerConfiguration, exchangeStartDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to parse publication date: {exchangeRates.Tarih}");
			}
			return ErrorBuilder.ToString();
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string exchangeRateType, DateTime exchangeStartDate, DateTime exchangeEndDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, exchangeRateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.CountryCodeTR);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, true, exchangeStartDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, exchangeEndDate.Add(DateTime.MaxValue.TimeOfDay));
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		static decimal GetRate(string rate)
		{
			_ = decimal.TryParse(rate, out var result);
			return result;
		}
	}
}
