using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class ForwarderIDsProgram
	{
		public static void Run(string outputPath)
		{
			var forwarderIDsPublicationDate = DateTime.ParseExact(ApplicationConfig.ForwarderIDsPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new ForwarderIDsParser(ApplicationConfig.ForwarderIDsConfigFilePath, ApplicationConfig.ForwarderIDsDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.ForwarderIDs), forwarderIDsPublicationDate);
		}
	}
}
