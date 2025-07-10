using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class SimpleDrawbackProgram
	{
		public static void Run(string outputPath)
		{
			var simpleDrawbackPublicationDate = DateTime.ParseExact(ApplicationConfig.SimpleDrawbackPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new SimpleDrawbackParser(ApplicationConfig.SimpleDrawbackConfigFileInputPath, ApplicationConfig.SimpleDrawbackDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.SimpleDrawback), simpleDrawbackPublicationDate);
		}
	}
}
