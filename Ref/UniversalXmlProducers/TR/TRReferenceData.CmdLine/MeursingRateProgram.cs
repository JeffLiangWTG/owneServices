using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class MeursingRateProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			var outputFullPath = Path.Combine(outputPath, $"RefTariffZZ_TR_{Business.Constants.TariffType.Code.MEU}.xml");
			var parser = new MeursingRatesParser();
			parser.GenerateUniversalReferenceData(outputFullPath);
			Program.PrintErrorMessage(parser.ErrorMessage);
		}
	}
}
