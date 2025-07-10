using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSPortData;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class CDSPortDataProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var refDataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);

			var processManager = new WebClientProcessManager(new PortBuilder(errorCollector), refDataLoader);

			processManager.RunProcess(ConfigurationProvider.OutputDirectory, errorCollector);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
