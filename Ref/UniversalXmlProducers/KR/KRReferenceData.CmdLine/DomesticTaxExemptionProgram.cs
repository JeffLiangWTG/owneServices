using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	public static class DomesticTaxExemptionProgram
	{
		public static void Run(string outputPath)
		{
			var domesticTaxExemptionPublicationDate = DateTime.ParseExact(ApplicationConfig.DomesticTaxExemptionPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new DomesticTaxExemptionParser(ApplicationConfig.DomesticTaxExemptionConfigFileInputPath, ApplicationConfig.DomesticTaxExemptionDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.DomesticTaxExemption), domesticTaxExemptionPublicationDate);
		}
	}
}
