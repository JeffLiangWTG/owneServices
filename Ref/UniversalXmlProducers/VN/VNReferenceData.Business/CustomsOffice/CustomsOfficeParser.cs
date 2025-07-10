using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public class CustomsOfficeParser
	{
		readonly string outputFilePath;
		readonly string downloadFilePath;
		readonly IHttpHandler httpHandler;
		readonly DateTime defaultPublicationTime;

		public CustomsOfficeParser(string downloadFilePath, string outputFilePath, IHttpHandler httpHandler,
			DateTime defaultPublicationTime)
		{
			this.downloadFilePath = downloadFilePath;
			this.outputFilePath = outputFilePath;
			this.httpHandler = httpHandler;
			this.defaultPublicationTime = defaultPublicationTime;
		}

		public CustomsOfficeParser()
		{
			httpHandler = new HttpClientHandler();
			downloadFilePath = Path.GetTempFileName();
			outputFilePath = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				ApplicationConfig.XmlOutputFolder,
				ApplicationConfig.CustomsOfficeOutputFileName);
			defaultPublicationTime = DateTime.UtcNow;
		}

		public async Task DownloadAndExportToXmlFile()
		{
			Console.WriteLine("Downloading CodeList metadata");
			var metadata = await httpHandler.DownloadCodeListMetadata();
			var metadataItem = MetadataHelper.FindMetadataItem(metadata, ApplicationConfig.CustomsOfficeMetadataId);
			Console.WriteLine($"Customs Office metadata: {System.Text.Json.JsonSerializer.Serialize(metadataItem)}");

			var downloadUrl = MetadataHelper.ParseDownloadUrl(metadataItem);
			Console.WriteLine($"Downloading VN Customs office codes from {downloadUrl}");
			await httpHandler.DownloadToFile(downloadUrl, downloadFilePath);

			Console.WriteLine("Parsing VN Customs office codes from Excel file");
			var refCusCodeListData = await CustomsOfficeHelper.ParseExcelData(downloadFilePath);
			Console.WriteLine($"Found {refCusCodeListData.Count} records from the Excel file");

			var publicationTime = MetadataHelper.ParsePublicationTime(metadataItem, defaultPublicationTime);
			Console.WriteLine($"Publication time to use: {publicationTime}");

			var dataSource = "VN Customs Office";
			var codeType = "CUSOF";
			XmlHelper.ExportToXMLFile(outputFilePath, dataSource, codeType, publicationTime,
				refCusCodeListData);
			Console.WriteLine($"Data exported to XML file {outputFilePath}");
		}
	}
}
