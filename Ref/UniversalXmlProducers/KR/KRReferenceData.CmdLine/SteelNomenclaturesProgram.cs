using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class SteelNomenclaturesProgram
	{
		public static void Run(string outputPath)
		{
			var steelNomenclaturesPublicationDate = DateTime.ParseExact(ApplicationConfig.SteelNomenclaturesPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new SteelNomenclaturesParser(ApplicationConfig.SteelNomenclaturesConfigFileInputPath, ApplicationConfig.SteelNomenclaturesDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.SteelNomenclature), steelNomenclaturesPublicationDate);
		}
	}
}
