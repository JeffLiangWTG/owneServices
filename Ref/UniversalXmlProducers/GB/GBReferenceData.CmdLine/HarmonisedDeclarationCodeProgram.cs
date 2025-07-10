using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Business.ChiefHarmonisedDeclarationCode;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.ChiefHarmonisedDeclarationCode;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class ChiefHarmonisedDeclarationCodeProgram
	{
		public static void Run()
		{
			var temporaryDownloadFile = Path.Combine(Path.GetTempPath(), "HDCDownloadFile.zip");
			try
			{
				using (var webDriver = new WebDriverHelper())
				{
					var publicationDate = ChiefHarmonisedDeclarationCodeDownloader.Download(new FileDownloaderWrapper(), webDriver, ConfigurationProvider.ChiefHarmonisedDeclarationCodeUrl, temporaryDownloadFile);
					var chiefHarmonisedDeclarationCodeParser = new ChiefHarmonisedDeclarationCodeParser();
					chiefHarmonisedDeclarationCodeParser.DownloadAndConvertToRefCusCodeListXML(ConfigurationProvider.OutputDirectory, temporaryDownloadFile, publicationDate);
				}
			}
			finally
			{
				if (File.Exists(temporaryDownloadFile))
				{
					File.Delete(temporaryDownloadFile);
				}
			}
		}
	}
}
