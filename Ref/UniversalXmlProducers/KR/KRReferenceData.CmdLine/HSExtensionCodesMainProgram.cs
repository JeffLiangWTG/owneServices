using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class HSExtensionCodesMainProgram
	{
		public static void Run(string outputPath)
		{
			var extensionCodePublicationDate = DateTime.ParseExact(ApplicationConfig.HSExtensionCodePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new HSExtensionCodesMainCategoryParser(ApplicationConfig.HSExtensionCodeConfigFileMainInputPath, ApplicationConfig.HSExtensionCodeDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.HSExtensionCodesMain), extensionCodePublicationDate);
		}
	}
}
