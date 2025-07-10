using System;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
{
	public static class TariffsProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();

			var processManagerTariffs = new TariffProcessManager(errorCollector);
			processManagerTariffs.RunProcess();

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
