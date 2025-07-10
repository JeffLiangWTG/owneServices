using System;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	public class USSIMDataPopulator : ISIMDataPopulator
	{
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;
		readonly string _localPDFFileSavePath;
		readonly string _localXLSFileSavePath;
		readonly string _localZipFileSavePath;
		readonly IXmlWriter _xmlWriter;
		readonly string _fullDataUrl;
		readonly string _mandatoryCodesListUrl;
		readonly IHttpClientHelper _client;

		public USSIMDataPopulator(IFileDownloaderWrapper fileDownloaderWrapper,
			string localPDFFileSavePath,
			string localXLSFileSavePath,
			string localZipFileSavePath,
			string fullDataUrl,
			string mandatoryCodesListUrl,
			IHttpClientHelper client)
		{
			Argument.NotNull(fileDownloaderWrapper, nameof(fileDownloaderWrapper));
			Argument.NotNullOrEmpty(localPDFFileSavePath, nameof(localPDFFileSavePath));
			Argument.NotNullOrEmpty(localXLSFileSavePath, nameof(localXLSFileSavePath));

			_fileDownloaderWrapper = fileDownloaderWrapper;
			_localPDFFileSavePath = localPDFFileSavePath;
			_localXLSFileSavePath = localXLSFileSavePath;
			_localZipFileSavePath = localZipFileSavePath;
			_fullDataUrl = fullDataUrl;
			_mandatoryCodesListUrl = mandatoryCodesListUrl;

			_xmlWriter = new XmlWriter(XmlWriterHelper.GerRefCusCodeListWriterConfiguration());
			_xmlWriter.SetDataSource("US SIM Data");
			_xmlWriter.SetUpdateType(UpdateType.Full);
			_client = client;
		}

		public void Parse(string fullSaveFilePath)
		{
			var isSuccess = DownloadPDF();
			if (isSuccess)
			{
				var mandatoryDataCodes = FileParser.ParsePDF(_localPDFFileSavePath);
				var publishedDate = GetPublishedDateAndSaveExtractFile(_fullDataUrl, _localZipFileSavePath, _localXLSFileSavePath);
				var codelists = FileParser.ParseXLS(_localXLSFileSavePath, mandatoryDataCodes, publishedDate);

				_xmlWriter.SetPublicationTime(publishedDate);
				foreach (var codeList in codelists)
				{
					_xmlWriter.PopulateData(codeList);
				}
				_xmlWriter.SaveXml(fullSaveFilePath);
			}
			else
			{
				throw new InvalidOperationException("Fail to download PDF file");
			}
		}

		bool DownloadPDF()
		{
			var mandatoryCodesListPDFUrl = GetSpecifiedLink(_mandatoryCodesListUrl, "Three alpha codes", ".pdf");
			return _fileDownloaderWrapper.DownloadFile(mandatoryCodesListPDFUrl, _localPDFFileSavePath);
		}

		public string GetSpecifiedLink(string url, string linkText, string endText)
		{
			var result = string.Empty;
			var page = new HtmlAgilityPack.HtmlDocument();
			page.LoadHtml(_client.GetWebPageAsync(url).Result);
			var links = page.DocumentNode.SelectNodes($"//a");
			if (links != null)
			{
				foreach (var link in links)
				{
					if (link.InnerText.Contains(linkText))
					{
						result = link.GetAttributeValue("href", "");
						if (result.EndsWith(endText, StringComparison.InvariantCultureIgnoreCase))
						{
							break;
						}
					}
				}
			}

			return result;
		}

		public DateTime GetPublishedDateAndSaveExtractFile(string fileUrl, string downloadFilePath, string outputFilePath)
		{
			Argument.NotNullOrEmpty(fileUrl, nameof(fileUrl));
			Argument.NotNullOrEmpty(downloadFilePath, nameof(downloadFilePath));
			Argument.NotNullOrEmpty(outputFilePath, nameof(outputFilePath));
			var publishedDate = ApplicationConfig.DefaultPublishedDate;
			_fileDownloaderWrapper.DownloadFile(fileUrl, downloadFilePath);

			using (var zipFile = ZipFile.OpenRead(downloadFilePath))
			{
				if (zipFile.Entries != null)
				{
					var txtFile = zipFile.Entries.FirstOrDefault(x => x.FullName.EndsWith(".txt", StringComparison.InvariantCulture));
					if (txtFile?.FullName != null)
					{
						publishedDate = GetDate(txtFile.FullName);
					}
				}
				var xlsFile = zipFile.Entries.FirstOrDefault(x => x.FullName.EndsWith(".xlsx", StringComparison.InvariantCulture));
				xlsFile?.ExtractToFile(outputFilePath, true);
			}
			return publishedDate;
		}

		public static DateTime GetDate(string dateString)
		{
			Argument.NotNull(dateString, nameof(dateString));

			var date = ApplicationConfig.DefaultPublishedDate;
			var search = @"[a-z]{3}_[0-9]{4}";
			var match = Regex.Match(dateString, search, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				date = DateTime.ParseExact(match.Value, "MMM_yyyy", System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				search = @"_[0-9]{4}";
				match = Regex.Match(dateString, search, RegexOptions.IgnoreCase);
				if (match.Success)
				{
					date = DateTime.ParseExact(match.Value, "_yyyy", System.Globalization.CultureInfo.InvariantCulture);
				}
			}
			return date;
		}
	}
}
