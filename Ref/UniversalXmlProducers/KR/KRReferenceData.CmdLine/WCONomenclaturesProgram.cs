using System.Globalization;
using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class WCONomenclaturesProgram
	{
		public static void Run(string outputPath)
		{
			var wcoPublicationDate = DateTime.ParseExact(ApplicationConfig.WCOPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			new WCONomenclatureCopier(ApplicationConfig.WCONomenclatureDataFileInputPathPDF,
									ApplicationConfig.WCONomenclatureDataFileInputPathXLS,
									ApplicationConfig.WCONomenclatureConfigFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.WCOCopiedNomenclature), wcoPublicationDate);
		}
	}
}
