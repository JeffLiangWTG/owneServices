using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class AdditionalPaymentReasonsProgram
	{
		public static void Run(string outputPath)
		{
			var AdditionalPaymentReasonsDepartmentPublicationDate = DateTime.ParseExact(ApplicationConfig.AdditionalPaymentReasonsPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new AdditonalPaymentReasonsParser(ApplicationConfig.AdditionalPaymentReasonsConfigFilePath, ApplicationConfig.AdditionalPaymentReasonsDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.AdditionalPaymentReasons), AdditionalPaymentReasonsDepartmentPublicationDate);
		}
	}
}
