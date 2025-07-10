using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public abstract class CBPPDFParser
	{
		protected CBPPDFParser(string rootUrl, string detialUrl, string outputPath, IDownLoadService downLoadService)
		{
			Argument.NotNullOrEmpty(rootUrl, nameof(rootUrl));
			Argument.NotNullOrEmpty(detialUrl, nameof(detialUrl));
			Argument.NotNullOrEmpty(outputPath, nameof(outputPath));
			Argument.NotNull(downLoadService, nameof(downLoadService));

			this.rootUrl = rootUrl;
			this.detialUrl = detialUrl;
			this.outputPath = outputPath;
			this.downLoadService = downLoadService;
		}
		readonly string rootUrl;
		readonly string detialUrl;
		readonly string outputPath;
		readonly IDownLoadService downLoadService;

		const string DateRegex = @"[A-Za-z]+\s\d{1,2},\s\d{4}";
#pragma warning disable CA1051
		public Func<DateTime> GetNowInUnitedStates = () => DateTime.UtcNow.AddHours(-5).Date; // Eastern Time in US
#pragma warning disable CA1051

		public string DownloadPDFAndExportXML(string pdfPath = null)
		{
			var now = GetNowInUnitedStates();
			var preProcessChecker = new LocalFileStorage(DataSource);
			var pdffileUrl = string.Empty;

			try
			{
				var publishDate = GetPDFFilePublishDate();
				if (publishDate == null)
				{
					throw new InvalidOperationException($"{now:MM-dd-yyyy} : Can not find the last modified date (url: {detialUrl}).");
				}
				else if (preProcessChecker.IsLastPublishDateExpired(publishDate).Expired)
				{
					pdffileUrl = GetPDFFileUrl();
					pdffileUrl = pdffileUrl.StartsWith(rootUrl, StringComparison.Ordinal) ? pdffileUrl : pdffileUrl.Length > 0 ? rootUrl + pdffileUrl : "";
					if (string.IsNullOrWhiteSpace(pdffileUrl))
					{
						throw new InvalidOperationException($"{now:MM-dd-yyyy} : Can not find the pdf file (url: {pdffileUrl}) for downloading.");
					}

					pdfPath = pdfPath == null ? Path.GetTempFileName() : pdfPath;
					if (downLoadService.DownloadFile(pdffileUrl, pdfPath))
					{
						var result = ParseToXml(pdfPath, outputPath, (DateTime)publishDate);
						preProcessChecker.Save((DateTime)publishDate, "yyyy-MM-dd");
						return result;
					}
				}
				else
				{
					return $"{DataSource}: Nothing new published since last process. Skip processing this time.";
				}
			}
			catch (Exception)
			{
				preProcessChecker.ClearData();
				throw;
			}
			return $@"Download pdf file from {pdffileUrl} failed.";
		}

		protected abstract bool IsPDFFileUrl(string href, string innerText);

		protected abstract string ParseToXml(string pdfPath, string outputPath, DateTime publishDate);

		protected abstract string DataSource { get; }

		string GetPDFFileUrl()
		{
			string GetLinkAddress(HtmlNode htmlNode) => htmlNode.Attributes["href"]?.Value?.Trim();

			bool IsUrlForDISCodePDFFile(HtmlNode htmlNode)
			{
				var result = false;
				if (htmlNode.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
				{
					var href = GetLinkAddress(htmlNode).ToUpper(CultureInfo.InvariantCulture);
					result = IsPDFFileUrl(href, htmlNode.InnerText.ToUpper(CultureInfo.InvariantCulture));
				}
				return result;
			}

			var pdfNode = downLoadService.FindNode(detialUrl, IsUrlForDISCodePDFFile);

			return pdfNode != null ? GetLinkAddress(pdfNode) : string.Empty;
		}

		DateTime? GetPDFFilePublishDate()
		{
			bool IsPublishDateParentNode(HtmlNode htmlNode)
			{
				return htmlNode.Name.Equals("span", StringComparison.OrdinalIgnoreCase) && htmlNode.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains("LAST MODIFIED:");
			}

			DateTime? GetDefaultPublishDate(HtmlNode htmlNode)
			{
				var parentNode = htmlNode.ParentNode;
				if (parentNode != null)
				{
					foreach (var childNode in parentNode.ChildNodes)
					{
						var match = Regex.Match(childNode.InnerText, DateRegex);
						if (match.Success && DateTime.TryParse(match.Value, out var result))
						{
							return result;
						}
					}
				}
				return null;
			}

			var publishDateNode = downLoadService.FindNode(detialUrl, IsPublishDateParentNode);

			return publishDateNode != null ? GetDefaultPublishDate(publishDateNode) : null;
		}
	}
}
