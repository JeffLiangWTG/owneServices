using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class TaxOfficeProgram
	{
		public static void Run(string outputPath)
		{
			var TaxOfficePublicationDate = DateTime.ParseExact(ApplicationConfig.TaxOfficePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new TaxOfficeParser(ApplicationConfig.TaxOfficeConfigFileInputPath, ApplicationConfig.TaxOfficeDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.TaxOffice), TaxOfficePublicationDate);
		}
	}
}
