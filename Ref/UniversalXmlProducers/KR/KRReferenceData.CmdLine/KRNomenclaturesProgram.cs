using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class KRNomenclaturesProgram
	{
		public static void Run(string outputPath)
		{
			var hsCodePublicationDate = DateTime.ParseExact(ApplicationConfig.HSCodePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new NomenclatureExcelParser(ApplicationConfig.KRNomenclatureConfigFileInputPath, ApplicationConfig.KRNomenclatureDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.KRNomenclature), hsCodePublicationDate);
		}
	}
}
