using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class OGARegulationCategoryProgram
	{
		public static void Run(string outputPath)
		{
			var OGARegulationCategoryPublicationDate = DateTime.ParseExact(ApplicationConfig.OGARegulationCategoryPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new OGARegulationCategoryParser(ApplicationConfig.OGARegulationCategoryConfigFilePath, ApplicationConfig.OGARegulationCategoryDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.OGARegulationCategory), OGARegulationCategoryPublicationDate);
		}
	}
}
