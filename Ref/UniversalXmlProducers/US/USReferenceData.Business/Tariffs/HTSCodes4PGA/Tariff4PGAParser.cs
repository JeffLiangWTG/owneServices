using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class Tariff4PGAParser
	{
		enum FileType
		{
			PDF,
			XLSX
		}

		readonly string pageUrl;
		readonly string webHost;
		readonly IDownLoadService downLoadService;
		const string excelHrefText = @"NEW%20MASTER%20DEA%20DRUG%20CODE-HTS-SCHEDULE%20B%20REFERENCE%20TABLE";
		const string excelInnerText = "MASTER DEA DRUG CODE HTS SCHEDULE B REFERENCE TABLE";
		const string pdfHrefText = @"%2FAPPENDIX_X_PGA_";
		const string pdfInnerText = "ACE AESTIR APPENDIX X - HTS CODES FOR PGAS";
		
		public Tariff4PGAParser(IDownLoadService downLoadService)
		{
			Argument.NotNull(downLoadService, nameof(downLoadService));

			this.pageUrl = ApplicationConfig.Instance.ACEHTSCodesForPGAURL;
			this.webHost = ApplicationConfig.Instance.CustomsBorderProtectionGoverment;
			this.downLoadService = downLoadService;
		}

		public (string logMessage, Dictionary<string, Tariff4PGA[]> tariff4PGACollect, List<EV1> ev1List) Parse()
		{
			var now = DateTime.UtcNow.AddHours(-5).Date;
			var logMessage = new StringBuilder();
			var tariff4PGAList = new List<Tariff4PGA>();

			var pdfParserLogMessage = ParseFile(tariff4PGAList, FileType.PDF, now);
			logMessage.AppendLine(pdfParserLogMessage);

			var excelParserLogMessage = ParseFile(tariff4PGAList, FileType.XLSX, now);
			logMessage.AppendLine(excelParserLogMessage);

			(var ev1LogMessage, var ev1List) = new EV1Parser(downLoadService).ParseEV1(now);
			logMessage.AppendLine(ev1LogMessage);

			logMessage.AppendLine(CultureInfo.InvariantCulture, $"Processed {tariff4PGAList.Count} tariffs for PGA codes.");

			return tariff4PGAList.Any() ? (logMessage.ToString(), GroupAndDistinctTariff4PGAList(tariff4PGAList), ev1List) : (logMessage.ToString(), null, ev1List);
		}

		string ParseFile(List<Tariff4PGA> tariff4PGAList, FileType fileType, DateTime dateTime)
		{
			var fileInfo = fileType == FileType.PDF ? GetFileInfo(pdfHrefText, pdfInnerText, fileType.ToString()) : GetFileInfo(excelHrefText, excelInnerText, fileType.ToString());
			var fileUrl = fileInfo.fileUrl.StartsWith(webHost, StringComparison.Ordinal) ? fileInfo.fileUrl : fileInfo.fileUrl.Length > 0 ? webHost + fileInfo.fileUrl : "";
			if (string.IsNullOrWhiteSpace(fileUrl))
			{
				return $"{dateTime:MM-dd-yyyy} : Can not find the {fileType} file for downloading (url: {pageUrl}).";
			}

			if (fileInfo.PublishDate == null)
			{
				return $"{dateTime:MM-dd-yyyy} : Can not find the publish date (url: {pageUrl}).";
			}

			var logMessage = new StringBuilder().AppendLine(CultureInfo.InvariantCulture, $"Download {fileType} file publish date: {fileInfo.PublishDate:MM-dd-yyyy}.");
			var filePath = Path.GetTempFileName();
			if (downLoadService.DownloadFile(fileUrl, filePath))
			{
				if (fileType == FileType.PDF)
				{
					var parserLogMessage = new Tariff4PGAPDFParser(tariff4PGAList, filePath).Parse();
					logMessage.Append(parserLogMessage);
				}
				else
				{
					new Tariff4PGAExcelParser(tariff4PGAList, filePath).Parse();
				}
				
				return logMessage.ToString();
			}

			return logMessage.Append(CultureInfo.InvariantCulture, $"Download {fileType} file from {fileUrl} failed.").ToString();
		}

		(string fileUrl, DateTime? PublishDate) GetFileInfo(string hrefText, string innerText, string fileType)
		{
			bool IsUrlForPGACodeFile(HtmlNode htmlNode)
			{
				var result = false;
				if (htmlNode.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
				{
					var href = GetLinkAddress(htmlNode).ToUpper(CultureInfo.InvariantCulture);
					result = !string.IsNullOrEmpty(href) && (href.Contains(hrefText) || htmlNode.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains(innerText)) && href.EndsWith(fileType, StringComparison.Ordinal);
				}
				return result;
			}

			var node = downLoadService.FindNode(pageUrl, IsUrlForPGACodeFile);
			return node == null ? (string.Empty, null) : (GetLinkAddress(node), GetFilePublishDate(node));
		}

		static Dictionary<string, Tariff4PGA[]> GroupAndDistinctTariff4PGAList(List<Tariff4PGA> tariff4PGAList)
		{
			return tariff4PGAList.Select(s => (TariffCodeDeFormatted: s.TariffCode.Replace(".", string.Empty), Tariff4PGA: s)).GroupBy(g => g.TariffCodeDeFormatted)
				.ToDictionary(k => k.Key, v => v.DistinctBy(d => d.Tariff4PGA.TariffCode + d.Tariff4PGA.PGACode).Select(s => s.Tariff4PGA).ToArray());
		}

		internal static string GetLinkAddress(HtmlNode htmlNode) => htmlNode.Attributes["href"]?.Value?.Trim();

		internal static DateTime? GetFilePublishDate(HtmlNode htmlNode)
		{
			var parentNode = htmlNode.ParentNode?.ParentNode;
			if (parentNode != null)
			{
				var dateRegex = @"\d{1,2}/\d{1,2}/\d{4}";
				var matchNode = parentNode.ChildNodes.Select(s => Regex.Match(s.InnerText, dateRegex)).FirstOrDefault(f => f.Success);
				if (matchNode != null)
				{
					if (DateTime.TryParseExact(matchNode.Value, "M/d/yyyy", DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var result))
					{
						return result;
					}
				}
			}
			return null;
		}
	}
}
