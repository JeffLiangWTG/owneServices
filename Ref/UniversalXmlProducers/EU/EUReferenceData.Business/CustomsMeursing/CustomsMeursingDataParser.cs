using System;
using System.Globalization;
using System.IO;
using System.Net;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Services.Circabc;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business
{
	public static class CustomsMeursingDataParser
	{
		public static void DownloadFileAndGenerateXml(IHttpClientHelper httpClientHelper)
		{
			string downloadFileName = @"Additional agricultural duties - Meursing.xlsx";
			string downloadFileRegex = @"[Aa]dditional[\s]+[Aa]gricultural[\s]+[Dd]uties[\s-]+[Mm]eursing[\s.]+[Xx]lsx";
			string geographicalAreaDownloadFileName = @"Geographical areas composition.xlsx";
			string geographicalAreaDownloadFileRegex = @"[Gg]eographical[\s]+[Aa]reas[\s]+[Cc]omposition[\s.]+[Xx][Ll][Ss][Xx]";
			string year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);

			DateTime? publicationTime = null;
			var serviceUrlTemplate = ApplicationConfig.Instance.CircabcServiceUrlTemplate;
			var circabcDownloadUrlTemplate = ApplicationConfig.Instance.CircabcDownloadUrlTemplate;
			var cirabcDutiesAndRelatedRootId = ApplicationConfig.Instance.CirabcDutiesAndRelatedRootId;

			var circabcService = new CircabcService(httpClientHelper, serviceUrlTemplate);

			var downloadDir = ApplicationConfig.Instance.CustomsMeursingDownloadDir;
			var xmlFileOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "EUMeursingData.xml");
			Directory.CreateDirectory(downloadDir);

			try
			{
				var webInfo = ExcelDownloader.GetWebFileInfo(circabcService, cirabcDutiesAndRelatedRootId, circabcDownloadUrlTemplate, downloadFileRegex, year);
				if (webInfo == null)
				{
					throw new WebException(string.Format(CultureInfo.InvariantCulture, "Can not find file {0} in the website", downloadFileName));
				}
				var geographicalAreaWebInfo = ExcelDownloader.GetWebFileInfo(circabcService, cirabcDutiesAndRelatedRootId, circabcDownloadUrlTemplate, geographicalAreaDownloadFileRegex, year);
				if (geographicalAreaWebInfo == null)
				{
					throw new WebException(string.Format(CultureInfo.InvariantCulture, "Can not find file {0} in the website", geographicalAreaDownloadFileName));
				}

				var downloadAbsolutePath = Path.GetFullPath(downloadDir);
				var downloadFileAbsolutePath = Path.Combine(downloadAbsolutePath, webInfo.FileName);
				File.Delete(downloadFileAbsolutePath);
				ExcelDownloader.DownLoadFile(webInfo, downloadAbsolutePath);

				Console.WriteLine("Read From Excel File, Path:" + downloadFileAbsolutePath);
				var meursingDatas = ExcelParser.ReadXlsFileIntoResults(downloadFileAbsolutePath);
				File.Delete(downloadFileAbsolutePath);

				downloadFileAbsolutePath = Path.Combine(downloadAbsolutePath, geographicalAreaWebInfo.FileName);
				File.Delete(downloadFileAbsolutePath);
				ExcelDownloader.DownLoadFile(geographicalAreaWebInfo, downloadAbsolutePath);

				Console.WriteLine("Read From Excel File, Path:" + downloadFileAbsolutePath);
				var geographicalData = ExcelParser.ReadGeographicalXlsFileIntoResults(downloadFileAbsolutePath);
				File.Delete(downloadFileAbsolutePath);

				publicationTime = webInfo.ModifiedDate;
				XMLGeneration.ExportToXMLFile(publicationTime, meursingDatas, geographicalData, xmlFileOutputFile);
				Console.WriteLine("Export To XML File, Path:" + xmlFileOutputFile);
			}
			catch (Exception ex)
			{
				XMLGeneration.ExportToXMLFile(publicationTime, null, null, xmlFileOutputFile, ex);
			}
		}
	}
}
