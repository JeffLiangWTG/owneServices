using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class HarmonizedTariffProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			Program.PrintErrorMessage(new HarmonizedTariffParser().ConvertToXMLFile(outputPath));
		}
	}
}
