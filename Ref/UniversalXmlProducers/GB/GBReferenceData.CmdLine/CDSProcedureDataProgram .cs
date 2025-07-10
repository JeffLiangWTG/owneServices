using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.Procedure;
using CargoWise.RefDbRepo.GBReferenceData.Services;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class CDSProcedureDataProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var processManager = new WebClientProcessManager(new ProcedureBuilder(errorCollector));
			processManager.RunProcess(ConfigurationProvider.OutputDirectory, errorCollector);
			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
