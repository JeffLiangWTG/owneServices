using System;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public class AESExportPortCodesParser : IExportPortCodesPdfParser
	{
		readonly string _webSiteUrl;
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;
		readonly string _localPdfFileSavePath;
		readonly IXmlWriter _xmlWriter;
		readonly IHttpClientHelper _httpClientHelper;

		public AESExportPortCodesParser(IFileDownloaderWrapper fileDownloaderWrapper, IHttpClientHelper httpClientHelper, string mainWebSiteUrl, string localPdfFileSavePath)
		{
			Argument.NotNull(fileDownloaderWrapper, nameof(fileDownloaderWrapper));
			Argument.NotNull(httpClientHelper, nameof(httpClientHelper));
			Argument.NotNullOrEmpty(mainWebSiteUrl, nameof(mainWebSiteUrl));
			Argument.NotNullOrEmpty(localPdfFileSavePath, nameof(localPdfFileSavePath));

			_fileDownloaderWrapper = fileDownloaderWrapper;
			_httpClientHelper = httpClientHelper;
			_webSiteUrl = mainWebSiteUrl;
			_localPdfFileSavePath = localPdfFileSavePath;
			_xmlWriter = new XmlWriter(XmlWriterHelper.GetRefCusCodeListWriterConfiguration());
			_xmlWriter.SetDataSource("US Customs Export Port Codes");
			_xmlWriter.SetUpdateType(UpdateType.Partial);
		}

		public void Parse(string fullSaveFilePath)
		{
			Argument.NotNullOrEmpty(fullSaveFilePath, nameof(fullSaveFilePath));
			var webPageScrape = new WebPageScrape(_httpClientHelper, _webSiteUrl);
			var url = webPageScrape.GetFileUrl();
			var publishDate = webPageScrape.GetPublishDate();
			var isSuccess = _fileDownloaderWrapper.DownloadFile(url, _localPdfFileSavePath);

			if (isSuccess)
			{
				var pdfParser = new PDFParser(_localPdfFileSavePath);
				pdfParser.Parse(publishDate ?? pdfParser.PublicationTime);
				_xmlWriter.SetPublicationTime(publishDate ?? pdfParser.PublicationTime);
				foreach (var codeList in pdfParser.RefCusCodeLists)
				{
					_xmlWriter.PopulateData(codeList);
				}

				_xmlWriter.SaveXml(fullSaveFilePath);
			}
			else
			{
				throw new Exception("Fail to download PDF file");
			}
		}
	}
}
