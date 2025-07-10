using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class OtherGovernmentProgram
	{
		public static void Run(string outputPath)
		{
			var otherGovernmentPublicationDate = DateTime.ParseExact(ApplicationConfig.OtherGovernmentPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new OtherGovernmentParser(ApplicationConfig.OtherGovernmentConfigFilePath, ApplicationConfig.OtherGovernmentDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.OtherGovernment), otherGovernmentPublicationDate);
		}
	}
}
