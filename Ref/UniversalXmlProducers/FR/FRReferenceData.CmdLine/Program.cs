using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] cmdLineArguments)
		{
			MainAsync(cmdLineArguments).GetAwaiter().GetResult();
		}

		static async Task MainAsync(string[] cmdLineArguments)
		{
			if (cmdLineArguments.Length != 0)
			{
				CreateDirectoryIfNeeded(ApplicationConfig.Instance.DownloadDirectory);

				var functionToRun = cmdLineArguments[0].ToUpper(CultureInfo.InvariantCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.TariffUpdate:
						SetProcessStartDate(cmdLineArguments);
						await TariffUpdater.Run();
						break;
					case Constants.ProgramFunctions.TariffCreation:
						await TariffCreator.Run(true);
						break;
					case Constants.ProgramFunctions.TariffRegen:
						await TariffCreator.Run(false);
						break;
					case Constants.ProgramFunctions.OnDemand:
						OnDemandCollectorProgram.Run();
						break;
					default:
						RunDataCollectorPrograms(functionToRun);
						break;
				}
			}
			else
			{
				throw new ArgumentException("No command line argument found.");
			}
		}

		static void RunDataCollectorPrograms(string functionToRun)
		{
			var dataCollectorPrograms = GetDataCollectorPrograms().ToArray();
			var foundProgram = dataCollectorPrograms.FirstOrDefault(x => x.CommandArgument == functionToRun);
			if (foundProgram != null)
			{
				foundProgram.Run();
			}
			else
			{
				throw new ArgumentException($"Invalid argument entered: {functionToRun}");
			}
		}

		static IEnumerable<DataCollectorProgram> GetDataCollectorPrograms()
		{
			yield return new AdditionalCodesCollectorProgram();
			yield return new DeltaIECodesCollectorProgram();
			yield return new AirportIATACollectorProgram();
			yield return new DeltaGCustomsProcedureCollectorProgram();
			yield return new DeltaGCustomsDestinationCodesCollectorProgram();
			yield return new DeltaIECustomsProcedureCollectorProgram();
			yield return new ExchangeRateCollectorProgram();
			yield return new MonthlyDataCollectorProgram();
			yield return new RateTypeCollectorProgram();
			yield return new TradeGroupCollectorProgram();
			yield return new PNTSCodeListsCollectorProgram();
		}

		static void SetProcessStartDate(string[] cmdLineArguments)
		{
			var result = Resumer.GetProcessStartDate();
			if (cmdLineArguments.Length > 1 && DateTime.TryParseExact(cmdLineArguments[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				Resumer.StoreProcessStartDate(result);
			}
		}

		static void CreateDirectoryIfNeeded(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}
