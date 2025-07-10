using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	public static class DutyRatesProgram
	{
		public static void Run(string outputPath)
		{
			var dutyRatesPublicationDate = DateTime.ParseExact(ApplicationConfig.DutyRatePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new DutyRateParser(ApplicationConfig.DutyRateConfigFileInputPath, ApplicationConfig.DutyRateDataFileInputPath, ApplicationConfig.PreferenceDataFileInputPath, ApplicationConfig.PreferenceConfigFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.DutyRates), dutyRatesPublicationDate);
		}
	}
}
