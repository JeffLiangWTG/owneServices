using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Utils = CargoWise.RefDbRepo.EUReferenceData.Business.Utils;

namespace CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business
{
	public class CommonXMLProducer
	{
		public CommonXMLProducer(string fileName, string dataSource, XmlWriterConfiguration refCusCodeListConfiguration, UpdateType updateType = UpdateType.Full)
		{
			fFileName = fileName;
			fDataSource = dataSource;
			fRefCusCodeListConfiguration = refCusCodeListConfiguration;
			fUpdateType = updateType;
		}

		public string DownloadAndConvertToXML(IHttpClientHelper httpClientHelper, string outputPath, string downloadUrl)
		{
			var errorBuilder = new StringBuilder();
			var downloadFilePath = Path.GetTempFileName();

			try
			{
				downloadUrl = !string.IsNullOrEmpty(downloadUrl) ? downloadUrl : ApplicationConfig.Instance.DocumentTypeCommonDownloadUrl;
				outputPath = !string.IsNullOrEmpty(outputPath) ? outputPath : ApplicationConfig.Instance.OutputDirectory;

				var successfullyDownloaded = FileDownloader.DownloadFile(httpClientHelper, downloadFilePath, downloadUrl, errorBuilder).GetAwaiter().GetResult();
				if (!successfullyDownloaded)
				{
					return errorBuilder.ToString();
				}

				(var sourceXml, var publicationTime) = Utils.GetXmlFromZipFileAndPublicationTime(downloadFilePath, ".xml");

				var dataParser = DataParser;
				var refCusCodeLists = dataParser.ParseToRefCusCodeLists(sourceXml);
				if (!string.IsNullOrEmpty(dataParser.ErrorStr))
				{
					return dataParser.ErrorStr;
				}

				var outputFile = Path.Combine(outputPath, fFileName);
				XmlWriterHelper.ExportToXMLFile(fDataSource, outputFile, fRefCusCodeListConfiguration, publicationTime, refCusCodeLists, fUpdateType, GetDependencies(RefCusCodeTypeProducer?.PublicationDate));

				if (RefCusCodeTypeProducer != null)
				{
					RefCusCodeTypeProducer.GenerateFile(outputPath);
				}
			}
			finally
			{
				if (File.Exists(downloadFilePath))
				{
					File.Delete(downloadFilePath);
				}
			}
			return errorBuilder.ToString();
		}

		protected virtual CommonDataParser DataParser { get; }

		protected virtual Dependency[] GetDependencies(DateTime? dependencyDateTime) => Array.Empty<Dependency>();

		protected virtual RefCusCodeTypeProducer RefCusCodeTypeProducer => null;

		readonly string fFileName;
		readonly string fDataSource;
		readonly XmlWriterConfiguration fRefCusCodeListConfiguration;
		readonly UpdateType fUpdateType;
	}
}
