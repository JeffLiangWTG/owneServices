using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class AdditionalCodesProducer : BaseProducer
	{
		public AdditionalCodesProducer()
		{
			additionalCodesParser = new AdditionalCodesParser();
			xmlProducer = new AdditionalCodesXmlProducer();
		}

		public override IEnumerable<string> FilesToLocate => new string[] { "Additional code", "Additionnal code" };

		public override string EUNLibraryBasePage => ApplicationConfig.EUReferenceDataBaseUrl;

		public void Run()
		{
			var filesInfo = LocateWebFiles();
			ReportWebFilesException(filesInfo);
			var filesDownload = filesInfo.Where(x => x.Exception == null);
			ReportDownloadException(DownloadFiles(filesDownload));

			foreach (var downloadedFile in filesDownload)
			{
				var additionalCodesData = AdditionalCodesExcelParser.ReadAdditionalCodesXlsxFileIntoResults(Path.Combine(ApplicationConfig.DownloadsFolder, downloadedFile.FileName));
				(var result, var processingErrors) = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData);

				if (!string.IsNullOrWhiteSpace(processingErrors))
				{
					Console.Error.WriteLine(processingErrors);
				}

				if (result.Any())
				{
					xmlProducer.InitializeWriter(PublishTime);
					xmlProducer.ExportToXml(result);
				}
			}
		}

		static void ReportDownloadException(bool filesDownloadedSuccessfully)
		{
			if (!filesDownloadedSuccessfully)
			{
				throw new InvalidOperationException("Failure downloading Additional Codes files.");
			}
		}

		void ReportWebFilesException(IEnumerable<IWebFileInfo> filesInfo)
		{
			var errors = filesInfo.Where(x => x.Exception != null);
			if (errors.Any() && errors.Count() == FilesToLocate.Count())
			{
				var error = errors.First();
				xmlProducer.InitializeWriter(DateTime.Today);
				var errorMessage = error.Exception.Message;
				xmlProducer.ReportDataSourceError(errorMessage);
				throw error.Exception;
			}
		}

		readonly IXmlProducer<RefCusCodeList> xmlProducer;
		readonly AdditionalCodesParser additionalCodesParser;
	}
}
