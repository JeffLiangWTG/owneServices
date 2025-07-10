using System;
using CargoWise.RefDbRepo.NOReferenceData.CmdLine.ExchangeRates;
using CargoWise.RefDbRepo.NOReferenceData.CmdLine.ReferenceCodes;
using CargoWise.RefDbRepo.NOReferenceData.CmdLine.Tariff;
using CargoWise.RefDbRepo.NOReferenceData.CmdLine.TradeGroups;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine
{
	sealed class Program
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
				var functionToRun = args[0].ToUpper();
				switch (functionToRun)
				{
					case "EXCHANGERATES":
						ExchangeRatesProgram.Run();
						break;
					case "REFERENCECODES":
						ReferenceCodesProgram.Run();
						break;
					case "TRADEGROUPS":
						TradeGroupsProgram.Run();
						break;
					case "TARIFF":
						TariffProgram.Run();
						break;
					case "TARIFFSUPPORT":
						TariffSupportProgram.Run();
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

		internal static bool PrintErrorMessage(string moduleMessage, string errorString)
		{
			var result = false;
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.Error.WriteLine(moduleMessage + errorString);
				result = true;
			}
			return result;
		}

		internal static bool PrintErrorMessage(string errorString) => PrintErrorMessage(string.Empty, errorString);
	}
}
