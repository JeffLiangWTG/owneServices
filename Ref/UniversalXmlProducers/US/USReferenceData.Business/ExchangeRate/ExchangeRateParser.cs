using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public class ExchangeRateParser
	{
		public ExchangeRateParser(string url, string fileUrlPrefix, string outputPath, IDownLoadService downLoadService)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			Argument.NotNullOrEmpty(fileUrlPrefix, nameof(fileUrlPrefix));
			Argument.NotNullOrEmpty(outputPath, nameof(outputPath));
			Argument.NotNull(downLoadService, nameof(downLoadService));

			this.url = url;
			this.fileUrlPrefix = fileUrlPrefix;
			this.outputPath = outputPath;
			this.downLoadService = downLoadService;
		}

		const string XMLWriterDataSource = "US CBP CUS Exchange Rate";
		const string OutputFileName = "RefExchangeRateZZ_US_CBP_CUS_Excel.xml";
		const string DateRegex = @"\d{1,2}-\d{1,2}-\d{4}";

		readonly string url;
		readonly string fileUrlPrefix;
		readonly string outputPath;
		public static readonly string LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + ".log");
		readonly IDownLoadService downLoadService;

#pragma warning disable CA1051
		public Func<DateTime> GetNowInUnitedStates = () => DateTime.UtcNow.AddHours(-5).Date; // Eastern Time in US
#pragma warning disable CA1051

		public string ParseToXml()
		{
			var excelInfo = GetExcelFileInfo();
			var excelFileUrl = excelInfo.Url;

			excelFileUrl = excelFileUrl.StartsWith(fileUrlPrefix, StringComparison.OrdinalIgnoreCase) ? excelFileUrl : excelFileUrl.Length > 0 ? fileUrlPrefix + excelFileUrl : "";

			var now = GetNowInUnitedStates();
			if (string.IsNullOrWhiteSpace(excelFileUrl))
			{
				DownloadWebPage();
				throw new InvalidOperationException($"{now:MM-dd-yyyy} : Can not find the excel file (url: {excelFileUrl}) for downloading.");
			}
			var data = downLoadService.DownloadData(excelFileUrl);
			if (data != null)
			{
				using (var stream = new MemoryStream(data))
				{
					var exchangeRates = ExcelParser.Parse(stream, new ExcelParserConfiguration(), now, out var publishDateFromFile)
						.Select(c => c.ToRefExchangeRate())
						.Where(c => c != null)
						.OrderBy(x => x.ZZN_RX_NKExCurrency)
						.ToList();

					var publishDate = ParserHelper.GetValidPublishDate(now, excelInfo.PublishDateFromUrl, publishDateFromFile);

					if (exchangeRates.Any())
					{
						var needToPublish = true;
						var dateToJson = new ExchangeRateData() { PublishDate = publishDate, ExchangeRates = exchangeRates.ToArray() }.ToJson();

						if (File.Exists(LogFilePath))
						{
							needToPublish = dateToJson != File.ReadAllText(LogFilePath);
						}

						if (needToPublish)
						{
							ExportToXMLFile(now, publishDate, exchangeRates);
							File.WriteAllText(LogFilePath, dateToJson);
							return $"Processed {exchangeRates.Count} exchange rates.";
						}
						else
						{
							return $"No exchange rate data updated for {publishDate:MM-dd-yyyy}.";
						}
					}
					else
					{
						return "No exchange rate data defined in the file.";
					}
				}
			}

			DownloadWebPage();
			return $@"Download excel file from {excelFileUrl} failed.";
		}

		void DownloadWebPage()
		{
			var path = GetOutputFilePath();
			downLoadService.DownloadFile(url, path);
		}

		string GetOutputFilePath()
		{
			var result = outputPath;

			if (string.IsNullOrEmpty(result))
			{
				result = @"..\UxmlFiles";
			}

			if (!Directory.Exists(result))
			{
				Directory.CreateDirectory(result);
			}

			return Path.Combine(result, $"ExchangeRateParserSaved{GetNowInUnitedStates():MM-dd-yyyy HH-mm}.html");
		}

		(DateTime? PublishDateFromUrl, string Url) GetExcelFileInfo()
		{
			string GetLinkAddress(HtmlNode htmlNode) => htmlNode.Attributes["href"]?.Value?.Trim();

			DateTime? GetDefaultPublishDate(HtmlNode htmlNode)
			{
				var match = Regex.Match(htmlNode.Attributes["title"]?.Value.Trim() ?? string.Empty, DateRegex);
				if (!match.Success)
				{
					match = Regex.Match(htmlNode.InnerText ?? string.Empty, DateRegex);
				}

				if (match.Success && DateTime.TryParseExact(match.Value, "M-d-yyyy", DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var result))
				{
					return result;
				}

				return null;
			}

			var excelNodes = downLoadService.FindNodes(url, ParserHelper.IsUrlForExchangeRateExcelFile);
			if (excelNodes != null)
			{
				if (excelNodes.Count() < 2)
				{
					var node = excelNodes.FirstOrDefault();
					return node != null ? (GetDefaultPublishDate(node), GetLinkAddress(node)) : (null, string.Empty);
				}
				else
				{
					throw new InvalidOperationException($"Find two or more xlsx files from the url.");
				}
			}
			else
			{
				return (null, string.Empty);
			}
		}

		void ExportToXMLFile(DateTime now, DateTime publishDate, List<RefExchangeRateZZ> exchangeRates)
		{
			var dayOfWeek = publishDate.DayOfWeek;
			if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
			{
				throw new InvalidOperationException($"US CBP publish exchange rates at {now}({dayOfWeek}), please dont check that Friday exchange rate is set to cover Friday, plus Saturday and Sunday.");
			}

			void ExportRatesIntoSingleDayXMLFile(DateTime fixedPublishDate)
			{
				var outputFilePath = Path.Combine(outputPath, fixedPublishDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_" + OutputFileName);
				var xmlWriterConfiguration = XmlWriterHelper.GetRefExchangeRateWriterConfiguration(fixedPublishDate, fixedPublishDate);

				XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, outputFilePath, xmlWriterConfiguration, fixedPublishDate, exchangeRates, UpdateType.Partial);
			}

			ExportRatesIntoSingleDayXMLFile(publishDate);
			if (dayOfWeek == DayOfWeek.Friday)
			{
				ExportRatesIntoSingleDayXMLFile(publishDate.AddDays(1));
				ExportRatesIntoSingleDayXMLFile(publishDate.AddDays(2));
			}
		}

		class ExchangeRateData
		{
			public DateTime PublishDate;
			public RefExchangeRateZZ[] ExchangeRates;

			public string ToJson()
			{
				return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
				{
					DateTimeZoneHandling = DateTimeZoneHandling.Local,
					DateFormatString = "yyyy-MM-dd HH:mm:ss"
				});
			}
		}
	}
}
