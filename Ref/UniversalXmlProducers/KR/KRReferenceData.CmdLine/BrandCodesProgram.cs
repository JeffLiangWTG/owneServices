using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class BrandCodesProgram
	{
		public static void Run(string outputPath)
		{
			var brandCodesPublicationDate = DateTime.ParseExact(ApplicationConfig.BrandCodesPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new BrandCodesParser(ApplicationConfig.BrandCodesConfigFilePath, ApplicationConfig.BrandCodesDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.BrandCodes), brandCodesPublicationDate);
		}
	}
}
