using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class ExpressDeliveryServiceIDProgram
	{
		public static void Run(string outputPath)
		{
			var publicationDate = DateTime.Today;
			new ExpressDeliveryServiceIDParser(ApplicationConfig.ExpressDeliveryServiceIDConfigFileInputPath, ApplicationConfig.ExpressDeliveryServiceIDDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.ExpressDeliveryServiceID), publicationDate);
		}
	}
}
