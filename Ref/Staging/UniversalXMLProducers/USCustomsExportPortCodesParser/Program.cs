using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			var webSiteUrl = ApplicationConfig.WebSiteUrl;
			var pdfFilePath = ApplicationConfig.DownloadPath;
			var directory = Path.GetDirectoryName(pdfFilePath);

			Directory.CreateDirectory(directory);

			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var httpClientHelper = new HttpClientHelper();

			var outputPath = ApplicationConfig.XmlFileOutputPath;
			var outputFullPath = Path.Combine(outputPath, $"RefCusCodeListZZ_US.xml");

			var parser = new AESExportPortCodesParser(fileDownloaderWrapper, httpClientHelper, webSiteUrl, pdfFilePath);
			parser.Parse(outputFullPath);

			Directory.Delete(Path.GetDirectoryName(pdfFilePath), true);
		}
	}
}
