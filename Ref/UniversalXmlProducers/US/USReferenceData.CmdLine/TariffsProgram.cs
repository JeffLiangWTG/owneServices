using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class TariffsProgram
	{
		public static void TariffsMain(string outputPath)
		{
			var serviceClient = new DownLoadService();
			var processDate = new DateTimeProvider().GetUTCNow();
			var tariff4PGAPParseResult = new Tariff4PGAParser(serviceClient).Parse();
			Program.PrintLogMessage(tariff4PGAPParseResult.logMessage);
			var functionsToRun = new Dictionary<string, Action>
			{
				{ Constants.TariffTypes.EXP, () => Program.PrintLogMessage(new EXPTariffParser(serviceClient, processDate, tariff4PGAPParseResult.tariff4PGACollect, tariff4PGAPParseResult.ev1List).DownloadAndConvertCodesToXMLFile(outputPath)) },
				{ Constants.TariffTypes.SHB, () => Program.PrintLogMessage(new SHBTariffParser(serviceClient, processDate, tariff4PGAPParseResult.tariff4PGACollect, tariff4PGAPParseResult.ev1List).DownloadAndConvertCodesToXMLFile(outputPath)) },
			};
			Parallel.Invoke(functionsToRun.Select(x => x.Value).ToArray());
		}
	}
}
