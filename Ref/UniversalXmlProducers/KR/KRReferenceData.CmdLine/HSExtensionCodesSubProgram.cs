using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class HSExtensionCodesSubProgram
	{
		public static void Run(string outputPath)
		{
			var extensionCodePublicationDate = DateTime.ParseExact(ApplicationConfig.HSExtensionCodePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new HSExtensionCodesSubCategoryParser(ApplicationConfig.HSExtensionCodeConfigFileSubInputPath, ApplicationConfig.HSExtensionCodeDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.HSExtensionCodesSub), extensionCodePublicationDate);
		}
	}
}
