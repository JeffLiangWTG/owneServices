using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class CustomsDepartmentProgram
	{
		public static void Run(string outputPath)
		{
			var customsOfficeDepartmentPublicationDate = DateTime.ParseExact(ApplicationConfig.CustomsOfficeDepartmentPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			new CustomsDepartmentParser(ApplicationConfig.CustomsDepartmentConfigFilePath, ApplicationConfig.CustomsDepartmentDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.CustomsDepartment), customsOfficeDepartmentPublicationDate);
		}
	}
}
