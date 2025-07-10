using System;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.ExchangeRates;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.NomenclatureGroups;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine.TradeGroups;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine
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
			if (args.Length == 0)
			{
				throw new ArgumentException("No arguments entered.");
			}
			else
			{
				var functionName = args[0].ToUpperInvariant();
				ProgramFunction function = TryGetProgramFunction(functionName);
				if (function == null)
				{
					throw new ArgumentException($"Invalid argument entered: {functionName}");
				}
				else
				{
					function.Invoke(ApplicationConfig.Instance.OutputPath);
				}
			}
		}

		internal delegate void ProgramFunction(string outputDirectory);

		internal static ProgramFunction TryGetProgramFunction(string function)
		{
			switch (function)
			{
				case "EXCHANGERATES":
					return ExchangeRatesProgram.Run;
				case "TRADEGROUPS":
					return TradeGroupsProgram.Run;
				case "CODELISTS":
					return CodeListsProgram.Run;
				case "CUSTOMSOFFICES":
					return CustomsOfficesProgram.Run;
				case "NOMENCLATUREGROUPS":
					return NomenclatureGroupsProgram.Run;
				case "EXPORTTARIFFS":
					return ExportTariffsProgram.Run;
				case "IMPORTTARIFFS":
					return ImportTariffsProgram.Run;
				case "PERMITITEMDETAILS":
					return PermitItemDetailsProgram.Run;
				case "ADDITIONALTAXESTARIFFS":
					return AdditionalTaxesTariffsProgram.Run;
				case "RATECODES":
					return RateCodesProgram.Run;
				case "PASSARCODELISTS":
					return PassarCodeListsProgram.Run;
				default:
					return null;
			}
		}

		internal static void PrintErrorMessage(string errorString)
		{
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.Error.WriteLine(errorString);
			}
		}
	}
}
