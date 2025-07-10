using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Spire.Pdf;
using Spire.Pdf.Texts;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class EV1Parser
	{
		readonly string webHost;
		readonly string ev1PageUrl;
		const string ev1FileName = @"APPENDIX U - HTS/SCHEDULE B CLASSIFICATIONS REQUIRING USED VEHICLE REPORTING";
		const string ev1HrefText = "USED_VEHICLE_NUMBERS";
		readonly IDownLoadService downLoadService;

		public EV1Parser(IDownLoadService downLoadService)
		{
			this.webHost = ApplicationConfig.Instance.CustomsBorderProtectionGoverment;
			this.ev1PageUrl = ApplicationConfig.Instance.ACEHTSCodesForEV1URL;
			this.downLoadService = downLoadService;
		}

		public (string, List<EV1>) ParseEV1(DateTime dateTime)
		{
			bool DetermineLink(HtmlNode htmlNode)
			{
				var result = false;
				if (htmlNode.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
				{
					var href = Tariff4PGAParser.GetLinkAddress(htmlNode).ToUpper(CultureInfo.InvariantCulture);
					result = !string.IsNullOrEmpty(href) && (href.Contains(ev1HrefText) && htmlNode.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains(ev1FileName)) && href.EndsWith("PDF", StringComparison.Ordinal);
				}
				return result;
			}

			var fileNode = downLoadService.FindNode(ev1PageUrl, DetermineLink);
			if (fileNode != null)
			{
				var fileURL = Tariff4PGAParser.GetLinkAddress(fileNode);
				var publicationTime = Tariff4PGAParser.GetFilePublishDate(fileNode);

				if (string.IsNullOrWhiteSpace(fileURL))
				{
					return ($"{dateTime:MM-dd-yyyy} : Can not find the EV1 file for downloading (url: {ev1PageUrl}).", null);
				}

				if (publicationTime == null)
				{
					return ($"{dateTime:MM-dd-yyyy} : Can not find the EV1 publish date (url: {ev1PageUrl}).", null);
				}

				var logMessage = new StringBuilder().AppendLine(CultureInfo.InvariantCulture, $"Download EV1 file publish date: {publicationTime:MM-dd-yyyy}.");
				var filePath = Path.GetTempFileName();
				fileURL = fileURL.StartsWith(webHost, StringComparison.Ordinal) ? fileURL : webHost + fileURL;
				if (downLoadService.DownloadFile(fileURL, filePath))
				{
					var ev1List = ReadEV1PDF(filePath);
					logMessage.AppendLine(CultureInfo.InvariantCulture, $"{ev1List.Count} EV1 data downloaded in total, and {ev1List.Where(x => x.IsMandatory).Count()} is mandatory data.");
					return (logMessage.ToString(), ev1List);
				}
				logMessage.AppendLine(CultureInfo.InvariantCulture, $"Download EV1 file from {fileURL} failed.");
				return (logMessage.ToString(), null);
			}

			return ($"{dateTime:MM-dd-yyyy} : Can not find the EV1 link for downloading (url: {ev1PageUrl}).", null);
		}

		public static List<EV1> ReadEV1PDF(string filePath)
		{
			var ev1List = new List<EV1>();
			var excludedTariffs = ApplicationConfig.Instance.EV1ExcludedTariffs.Split(",").ToList();
			var tariffRegex = @"\d{4}\.\d{2}\.\d{4}";
			using (var pdfDocument = new PdfDocument())
			{
				pdfDocument.LoadFromFile(filePath);
				foreach (PdfPageBase page in pdfDocument.Pages)
				{
					using (var finder = new PdfTextFinder(page))
					{
						finder.Options.Parameter = TextFindParameter.Regex;
						var fragments = finder.Find(tariffRegex);
						foreach (var fragment in fragments)
						{
							if (!excludedTariffs.Contains(fragment.Text))
							{
								ev1List.Add(new EV1(fragment.Text.Replace(".", ""), fragment.TextStates[0].IsBold || fragment.TextStates[0].IsSimulateBold));
							}
						}
					}
				}
			}
			return ev1List;
		}
	}

	public class EV1
	{
		public EV1(string tariffCode, bool isMandatory)
		{
			TariffCode = tariffCode;
			IsMandatory = isMandatory;
		}

		public string TariffCode { get; set; }
		public bool IsMandatory { get; set; }
	}
}
