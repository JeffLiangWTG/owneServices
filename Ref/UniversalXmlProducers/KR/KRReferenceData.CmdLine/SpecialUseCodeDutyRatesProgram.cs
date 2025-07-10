using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	public static class SpecialUseCodeDutyRatesProgram
	{
		public static void Run(string outputPath)
		{
			var dutyRatesPublicationDate = DateTime.ParseExact(ApplicationConfig.DutyRatePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new SpecialUseCodeDutyRateParser(ApplicationConfig.SpecialUseCodeDutyRateConfigFileInputPath, ApplicationConfig.DutyRateDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.SpecialUseCodeDutyRates), dutyRatesPublicationDate);
		}
	}
}
