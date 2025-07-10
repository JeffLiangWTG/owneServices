using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.Business.MeasureTypes;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	public static class MeasureTypesProgram
	{
		public static void Run(string outputPath)
		{
			var downloader = new DownloadExportXml();
			(var xmlData, var modified) = downloader.CombinedDownloadFile<measureType1>(ApplicationConfig.FilePrefix_MeasureType);

			if (modified != null && xmlData != null)
			{
				var parser = new MeasureTypeParser();
				Program.PrintErrorMessage(parser.ConvertToXMLFile(xmlData, modified, Path.Combine(ApplicationConfig.OutputDirectory, outputPath)));
			}
			else
			{
				Program.PrintErrorMessage("Unable to find or download files for Measuretypes.");
			}
		}
	}
}
