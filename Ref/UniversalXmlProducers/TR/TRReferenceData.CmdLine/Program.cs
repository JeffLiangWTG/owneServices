using System;
using System.Globalization;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
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
			if (args.Length != 0)
			{
				var func = args[0].ToUpper(CultureInfo.InvariantCulture);

				switch (func)
				{
					case Constants.ProgramFunctions.ExchangeRate:
						ExchangeRateProgram.Run();
						break;
					case Constants.ProgramFunctions.BanderolTariff:
						BanderolTariffProgram.Run();
						break;
					case Constants.ProgramFunctions.HarmonizedTariff:
						HarmonizedTariffProgram.Run();
						break;
					case Constants.ProgramFunctions.TradeGroup:
						TradeGroupProgram.Run();
						break;
					case Constants.ProgramFunctions.ExportUnionTariff:
						ExportUnionTariffProgram.Run();
						break;
					case Constants.ProgramFunctions.ETradeExemptionCodes:
						ETradeExemptionCodesProgram.Run();
						break;
					case Constants.ProgramFunctions.CodeLists:
						TRCustomsCodeListsProgram.Run();
						break;
					case Constants.ProgramFunctions.MeursingRates:
						MeursingRateProgram.Run();
						break;
					case Constants.ProgramFunctions.TRStampDuty:
						RefCusTaxOrFeeProgram.Run();
						break;
					case Constants.ProgramFunctions.TRDutyCodes:
						DutyCodesProgram.Run();
						break;
					case Constants.ProgramFunctions.TRRefCusProcedureCodes:
						RefCusProcedureCodesProgram.Run();
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {func}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}

		public static void PrintErrorMessage(string errorString)
		{
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.Error.WriteLine(errorString);
			}
		}
	}
}
