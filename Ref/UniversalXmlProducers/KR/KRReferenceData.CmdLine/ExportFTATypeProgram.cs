using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class ExportFTATypeProgram
	{
		public static void Run(string outputPath)
		{
			new ExportFTATypeParser(ApplicationConfig.ExportFTATypeConfigFilePath, ApplicationConfig.ExportFTATypeDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.ExportFTAType), DateTime.Today);
		}
	}
}
