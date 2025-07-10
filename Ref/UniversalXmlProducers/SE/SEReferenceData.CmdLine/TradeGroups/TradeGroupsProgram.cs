using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.Business.TradeGroups;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	public static class TradeGroupsProgram
	{
		public static void Run(string outputPath)
		{
			var downloader = new DownloadExportXml();
			(var xmlData, var modified) = downloader.CombinedDownloadFile<geographicalArea>(ApplicationConfig.FilePrefix_GeographicalArea);

			if (modified != null && xmlData != null)
			{
				var parser = new TradeGroupParser();
				Program.PrintErrorMessage(parser.ConvertToXMLFile(xmlData, modified, Path.Combine(ApplicationConfig.OutputDirectory, outputPath)));
			}
			else
			{
				Program.PrintErrorMessage("Unable to find or download files for TradeGroups.");
			}
		}
	}
}
