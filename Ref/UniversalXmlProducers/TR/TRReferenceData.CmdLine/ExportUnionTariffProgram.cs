using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	class ExportUnionTariffProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			var outputFullPath = Path.Combine(outputPath, $"RefTariffZZ_TR_{Business.Constants.TariffType.Code.TREUA}.xml");
			var parser = new ExportUnionTariffParser();
			parser.GenerateUniversalReferenceData(outputFullPath);
			Program.PrintErrorMessage(parser.ErrorMessage);
		}
	}
}
