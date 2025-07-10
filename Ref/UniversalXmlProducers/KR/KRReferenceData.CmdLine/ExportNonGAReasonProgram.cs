using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class ExportNonGAReasonProgram
	{
		public static void Run(string outputPath)
		{
			new NonGAReasonParser(ApplicationConfig.NonGAReasonExportConfigFileInputPath, ApplicationConfig.NonGAReasonExportDataFileInputPath, Constants.DataSources.ExportNonGAReasonType).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.ExportNonGAReasonType), DateTime.Today);
		}
	}
}
