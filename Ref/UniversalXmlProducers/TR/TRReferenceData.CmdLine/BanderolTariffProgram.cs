using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class BanderolTariffProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			var outputFullPath = Path.Combine(outputPath, "RefTariffZZ_TR_ETRBN.xml");
			var parser = new BanderolTariffDataParser();
			parser.GenerateTariffUniversalReferenceData(outputFullPath);
			Program.PrintErrorMessage(parser.ErrorMessage);
		}
	}
}
