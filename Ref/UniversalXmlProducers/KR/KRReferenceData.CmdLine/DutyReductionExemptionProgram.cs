using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	public static class DutyReductionExemptionProgram
	{
		public static void Run(string outputPath)
		{
			var dutyReductionExemptionPublicationDate = DateTime.ParseExact(ApplicationConfig.DutyReductionExemptionPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new DutyReductionExemptionParser(ApplicationConfig.DutyReductionExemptionConfigFileInputPath, ApplicationConfig.DutyReductionExemptionDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.DutyReductionExemption), dutyReductionExemptionPublicationDate);
		}
	}
}
