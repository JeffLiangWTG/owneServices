using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			var xslDirectory = Path.GetDirectoryName(ApplicationConfig.Instance.FullDataXLSFilepath);
			var pdfDirectory = Path.GetDirectoryName(ApplicationConfig.Instance.MandatoryCodesListPDFFilepath);
			var zipDirectory = Path.GetDirectoryName(ApplicationConfig.Instance.FullDataZipFilepath);
			Directory.CreateDirectory(xslDirectory);
			Directory.CreateDirectory(pdfDirectory);
			Directory.CreateDirectory(zipDirectory);

			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var outputFullPath = ApplicationConfig.Instance.ExportXMLFilepath;

			var client = new HttpClientHelper();
			var parser = new USSIMDataPopulator(fileDownloaderWrapper,
				ApplicationConfig.Instance.MandatoryCodesListPDFFilepath,
				ApplicationConfig.Instance.FullDataXLSFilepath,
				ApplicationConfig.Instance.FullDataZipFilepath,
				ApplicationConfig.Instance.FullDataUrl,
				ApplicationConfig.Instance.MandatoryCodesListUrl,
				client);
			parser.Parse(outputFullPath);

			Directory.Delete(xslDirectory, true);
			Directory.Delete(pdfDirectory, true);
			Directory.Delete(zipDirectory, true);
		}
	}
}
