using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class ETradeExemptionCodesProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			var outputFullPath = Path.Combine(outputPath, $"RefTariffZZ_TR_{Business.Constants.TariffType.Code.ETR}.xml");
			var parser = new ETradeExemptionCodesParser();
			parser.GenerateUniversalReferenceData(outputFullPath);
			Program.PrintErrorMessage(parser.ErrorMessage);
		}
	}
}
