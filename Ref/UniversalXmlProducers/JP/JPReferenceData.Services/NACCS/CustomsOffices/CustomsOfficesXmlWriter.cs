using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class CustomsOfficesXmlWriter
	{
		const string DataSource = "JP Customs Offices";
		const string FileNameWithoutExtension = "RefCusCodeList_JP_CustomsOffices";

		public static string WriteXml(IHttpClientHelper httpClientHelper)
		{
			var errorBuilder = new StringBuilder();
			var downloadFilePath = Path.GetTempFileName();

#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var downloadUrl = AppConfig.NACCS.CodeLists.CustomsOfficesFileDownloadUrl;

				var successfullyDownloaded = FileDownloader.TryDownload(httpClientHelper, downloadUrl, downloadFilePath).GetAwaiter().GetResult();
				if (!successfullyDownloaded)
				{
					return errorBuilder.ToString();
				}

				var publicationDateIsRetrieved = NaccsPublicationDateScraper.TryGetPublicationDate(httpClientHelper, AppConfig.NACCS.CodeLists.BaseUrl, downloadUrl, out var publicationDate);

				if (!publicationDateIsRetrieved)
				{
					errorBuilder.AppendLine("Unable to retrieve publication date.");
					return errorBuilder.ToString();
				}

				var downloadAbsolutePath = Path.GetFullPath(downloadFilePath);
				if (CsvReaderHelper.TryRead(downloadAbsolutePath, out var records))
				{
					var dataParser = new CustomsOfficesDataParser();
					var codeList = dataParser.ParseToRefCusCodeLists(records);

					if (!codeList.Any())
					{
						return dataParser.ErrorStr;
					}

					var xmlWriter = new XmlWriterHelper(CustomsOfficesHelper.GetRefCusCodeListConfiguration(), DataSource, publicationDate, Common.UniversalXmlWriter.UpdateType.Full, FileNameWithoutExtension);
					xmlWriter.PopulateAndSave(codeList);
				}
			}
			catch (Exception ex)
			{
				errorBuilder.AppendLine(ex.StackTrace);
				errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to parse Customs Offices./r/n {ex.Message}");
			}
			finally
			{
				if (File.Exists(downloadFilePath))
				{
					File.Delete(downloadFilePath);
				}
			}
#pragma warning restore CA1031 // Do not catch general exception types
			return errorBuilder.ToString();
		}
	}
}
