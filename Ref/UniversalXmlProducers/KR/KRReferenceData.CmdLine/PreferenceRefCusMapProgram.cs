using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class PreferenceRefCusMapProgram
	{
		public static void Run(string outputPath)
		{
			new PreferenceRefCusMapParser(ApplicationConfig.PreferenceRefCusMapConfigFileInputPath, ApplicationConfig.PreferenceRefCusMapDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.PreferenceRefCusMap), DateTime.Today);
		}
	}
}
