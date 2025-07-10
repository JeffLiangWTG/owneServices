using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	public static class DomesticTaxRatesProgram
	{
		public static void Run(string outputPath)
		{
			var domesticTaxRatesPublicationDate = DateTime.ParseExact(ApplicationConfig.DomesticTaxRatesPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

			new DomesticTaxRatesParser(ApplicationConfig.DomesticTaxRatesConfigFileInputPath, ApplicationConfig.DomesticTaxRatesDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.DomesticTaxRates), domesticTaxRatesPublicationDate);
		}
	}
}
