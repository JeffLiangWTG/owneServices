using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class OGAProgram
	{
		public class Import
		{
			public static void Run(string outputPath)
			{
				var ogaPublicationDate = DateTime.ParseExact(ApplicationConfig.OGAPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
				new OGAParser(ApplicationConfig.OGAImportConfigFileInputPath, ApplicationConfig.OGAImportDataFileInputPath)
				.ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.OGAImport), ogaPublicationDate, Constants.Suffix.Import);
			}
		}
		public class Export
		{
			public static void Run(string outputPath)
			{
				var ogaPublicationDate = DateTime.ParseExact(ApplicationConfig.OGAPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
				new OGAParser(ApplicationConfig.OGAExportConfigFileInputPath, ApplicationConfig.OGAExportDataFileInputPath)
				.ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.OGAExport), ogaPublicationDate, Constants.Suffix.Export);
			}
		}
	}
}
