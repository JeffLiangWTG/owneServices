using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.SharedReferenceData.Business;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.ExchangeRates
{
	public class ExchangeRateBuilder
	{
		public ExchangeRateBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector, IWebClientWrapper webClientWrapper)
		{
			DateTimeProvider = dateTimeProvider;
			ErrorCollector = errorCollector;
			WebClientWrapper = webClientWrapper;
		}
		readonly IDateTimeProvider DateTimeProvider;
		readonly StringBuilder ErrorCollector;
		readonly IWebClientWrapper WebClientWrapper;

		public void RunProcess(string outputFolder)
		{
			var loader = new Downloader(DateTimeProvider, WebClientWrapper, ErrorCollector);

			var data = loader.GetExchangeRates();
			if (data.Rates.Any())
			{
				BuildXml(data.PublishDate, data.Rates, outputFolder);
			}
		}

		protected static void BuildXml(DateTime publicationDate, IEnumerable<ExchangeRate> data, string outputPath)
		{
			var content = ConvertToRefModels(data);

			if (content.Any())
			{
				Helper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outputPath, GetOutputFileName(publicationDate)), XmlWriterConfiguration(), publicationDate, UpdateType.Full, content);
			}
		}

		protected static string FilePrefix => "GBExchangeRates";
		protected static string XMLWriterDataSource => "GB Exchange Rates";
		protected static string GetOutputFileName(DateTime publicationDate) => Invariant($"{FilePrefix}_{publicationDate:yyyyMMddHHmmss}.xml");

		protected static XmlWriterConfiguration XmlWriterConfiguration()
		{
			var config = new XmlWriterConfiguration();

			var exRate = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exRate.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, Constants.ExchangeRateValues.CustomsRateType);
			exRate.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DefaultValues.GBDataGrouping);
			exRate.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exRate.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exRate.IncludeColumn(x => x.ZZN_EndDate, false);
			exRate.IncludeColumn(x => x.ZZN_Rate, false);

			config.IncludeEntityTypeConfiguration(exRate);

			return config;
		}

		protected static IEnumerable<RefExchangeRateZZ> ConvertToRefModels(IEnumerable<ExchangeRate> data)
		{
			var results = new List<RefExchangeRateZZ>();

			results.AddRange(data.Select(x => new RefExchangeRateZZ
			{
				ZZN_RX_NKExCurrency = x.Currency,
				ZZN_StartDate = x.StartDate,
				ZZN_EndDate = CommonHelper.CalcMaxDate(x.EndDate),
				ZZN_Rate = x.Rate
			}));

			return results;
		}
	}
}
