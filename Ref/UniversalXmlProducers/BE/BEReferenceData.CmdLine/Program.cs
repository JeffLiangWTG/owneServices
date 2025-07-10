using System;
using System.Globalization;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	public class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			if (args.Length != 0)
			{
				var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.ExchangeRates:
						ExchangeRatesProgram.Run();
						break;
					case Constants.ProgramFunctions.AdditionalInfo:
						AdditionalInfoProgram.Run();
						break;
					case Constants.ProgramFunctions.LocationCodes:
						LocationCodesProgram.Run();
						break;
					case Constants.ProgramFunctions.NctsCodeLists:
						NCTSCodeListProgram.Run();
						break;
					case Constants.ProgramFunctions.TariffData:
						TariffDataProgram.Run();
						break;
					case Constants.ProgramFunctions.EUCodeLists:
						EUCodeListProgram.Run();
						break;
					case Constants.ProgramFunctions.ImportCodeLists:
						var processManager = new UCCImportCodeListProcessManager();
						processManager.RunProducers();
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}
