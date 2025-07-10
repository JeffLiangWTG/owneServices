using System;
using System.Globalization;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			var outputPath = ApplicationConfig.Instance.OutputPath;
			if (args.Length != 0)
			{
				var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
				switch (args[0].ToUpper(CultureInfo.InvariantCulture))
				{
					case Constants.ProgramFunctions.Tariffs:
						TariffsProgram.TariffsMain(outputPath);
						break;
					case Constants.ProgramFunctions.ExchangeRate:
						ExchangeRateProgram.ExchangeRateProgramMain(outputPath);
						break;
					case Constants.ProgramFunctions.A99Tariffs:
						A99TariffsProgram.TariffsMain(outputPath);
						break;
					case Constants.ProgramFunctions.ACEValidationRules:
						ACECargoReleaseSEInputValidationRulesProgram.ACECargoReleaseSEInputValidationRulesMain(outputPath);
						break;
					case Constants.ProgramFunctions.UnitedNationsStandardProductAndServiceCodes:
						UnitedNationsStandardProductAndServiceCodesProgram.UnitedNationsStandardProductAndServiceCodesMain(outputPath);
						break;
					case Constants.ProgramFunctions.ECCNNumbers:
						ECCNNumbersProgram.ECCNNumbersProgramMain(outputPath);
						break;
					case Constants.ProgramFunctions.NewWatchRules:
						NewWatchTariffProgram.NewWatchTariffMain(outputPath, args);
						break;
					case Constants.ProgramFunctions.USIncomingMessageQuery:
						USIncomingMessageProgram.USIncomingMessageRequest(args);
						break;
					case Constants.ProgramFunctions.USIncomingMessageDownload:
						USIncomingMessageProgram.USIncomingMessageDownload(outputPath);
						break;
					case Constants.ProgramFunctions.DISCodes:
						DISCodesProgram.DISCodesMain(outputPath);
						break;
					case Constants.ProgramFunctions.AESDispostionCodes:
						AESDispostionCodeProgram.AESDispostionCodesMain(outputPath);
						break;
					case Constants.ProgramFunctions.ExemptionNumber:
						ExemptionCodeProgram.ExemptionCodeMain(outputPath);
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

		public static void PrintLogMessage(string errorString)
		{
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.WriteLine(errorString);
			}
		}
	}
}
