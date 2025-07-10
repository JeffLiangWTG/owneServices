using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;
using Spire.Pdf;
using Spire.Pdf.Utilities;
using Path = System.IO.Path;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DownloadExchangeRates
	{
		public (string Errors, IExchangeRates ExchangeRates, string PDFPath) Download(string url, IHttpClientHelper httpClientHelper)
		{
			ErrorBuilder.Clear();
			IExchangeRates result = null;
			var pdfDownloadUrl = string.Empty;
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var htmlDocument = new HtmlDocument();
				var html = httpClientHelper.GetWebPageAsync(url).GetAwaiter().GetResult();
				htmlDocument.LoadHtml(html);

				pdfDownloadUrl = ExtractPDFFileUrl(htmlDocument);
				result = ParsePDF(pdfDownloadUrl, httpClientHelper);
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to Load JP Exchange Rate Html from the following URL: {url} /r/n {ex.Message}");
			}
#pragma warning restore CA1031 // Do not catch general exception types
			return (ErrorBuilder.ToString(), result, pdfDownloadUrl);
		}

		IExchangeRates ParsePDF(string url, IHttpClientHelper httpClientHelper)
		{
			var list = new List<IExchangeRateDetails>();

			var downloadFilePath = Path.GetTempFileName();
			var successfullyDownloaded = FileDownloader.TryDownload(httpClientHelper, url, downloadFilePath).GetAwaiter().GetResult();
			if (!successfullyDownloaded)
			{
				throw new UnhandledApplicationException(string.Format(CultureInfo.InvariantCulture, "Fail to download file from {0}.", url));
			}

			var downloadAbsolutePath = Path.GetFullPath(downloadFilePath);
			using (var pdfDocument = new PdfDocument(downloadAbsolutePath))
			{
				pdfDocument.LoadFromFile(downloadAbsolutePath);
				using (var extractor = new PdfTableExtractor(pdfDocument))
				{
					for (var pageIndex = 0; pageIndex < pdfDocument.Pages.Count; pageIndex++)
					{
						var table = extractor.ExtractTable(pageIndex)?.FirstOrDefault();
						var rowCount = table?.GetRowCount() ?? 0;

						if (table == null || rowCount <= 1)
						{
							throw new UnhandledApplicationException($"Unexpected fomart of PDF from {url}.");
						}

						for (var rowIndex = 1; rowIndex < rowCount; rowIndex++)
						{
							var code = table.GetText(rowIndex, 3).Trim();

							if (string.IsNullOrWhiteSpace(code) || code == BruneiDollarCurrencyCode)
							{
								continue;
							}

							if (code.Length != 3)
							{
								throw new UnhandledApplicationException($"Find code {code} with unexpected length from {url}.");
							}

							var needDivideByHundred = false;
							var rateText = table.GetText(rowIndex, 4).Trim();

							if (string.IsNullOrWhiteSpace(rateText))
							{
								rateText = table.GetText(rowIndex, 5).Trim();
								needDivideByHundred = true;
							}

							if (decimal.TryParse(rateText, out decimal rate))
							{
								if (needDivideByHundred)
								{
									rate /= 100;
								}
							}
							else
							{
								ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to convert rate for Currency: {code} /r/n Rate: {rateText}");
							}

							list.Add(new ExchangeRateDetailsProvider(code, rate));
						}
					}
				}
			}

			if (list.Count == 0)
			{
				throw new UnhandledApplicationException($"Can't find any exchange rates from {url}.");
			}

			AddBruneiCurrencyCode(list);

			var startDate = string.Empty;
			var endDate = string.Empty;
			var dateDetails = url.Substring(url.IndexOf("kouji-rate-english", StringComparison.Ordinal) + 18, 17).Split('-');
			if (dateDetails.Length == 2)
			{
				startDate = dateDetails[0];
				endDate = dateDetails[1];
			}

			return new ExchangeRatesProvider(startDate, endDate, list);
		}

		static void AddBruneiCurrencyCode(List<IExchangeRateDetails> list)
		{
			var sgd = list.FirstOrDefault(x => x.Code == SingaporeDollarCurrencyCode);
			if (sgd != null)
			{
				list.Add(new ExchangeRateDetailsProvider(BruneiDollarCurrencyCode, sgd.Rate));
			}
		}

		string ExtractPDFFileUrl(HtmlDocument doc)
		{
			var result = string.Empty;
			var text = doc.Text;
			var strRegex = @"kawase(\d{4})/kouji-rate-english(\d{8})-(\d{8}).pdf";
			var regex = new Regex(strRegex);
			var collection = regex.Matches(text);

			foreach (var shortPath in collection)
			{
				var strNew = shortPath.ToString();
				
				if (string.Compare(strNew, result, StringComparison.Ordinal) > 0)
				{
					result = strNew;
				}
			}

			if (!string.IsNullOrEmpty(result))
			{
				result = GetFileHelper().GetFullPDFPath(result);
			}

			return result;
		}

		protected virtual IFileHelper GetFileHelper() => new FileHelper();

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		public const string BruneiDollarCurrencyCode = "BND";
		public const string SingaporeDollarCurrencyCode = "SGD";
	}
}
