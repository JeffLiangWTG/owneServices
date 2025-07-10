using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class PreferenceProgram
	{
		public static void Run(string outputPath)
		{
			new RefCusPreferenceParser(ApplicationConfig.PreferenceConfigFileInputPath, ApplicationConfig.PreferenceDataFileInputPath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.RefCusPreference), DateTime.Today);
		}
	}
}
