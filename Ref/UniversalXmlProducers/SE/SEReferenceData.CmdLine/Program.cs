using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
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
				var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.ExchangeRates:
						ExchangeRatesProgram.Run(ApplicationConfig.OutputDirectory, new DownloadExchangeRates());
						break;
					case Constants.ProgramFunctions.CodeLists:
						CodeListsProgram.Run(args.Skip(1).ToArray(), ApplicationConfig.OutputDirectory);
						break;
					case Constants.ProgramFunctions.TradeGroups:
						TradeGroupsProgram.Run("RefCusTradeGroup_SE.xml");
						break;
					case Constants.ProgramFunctions.MeasureTypes:
						MeasureTypesProgram.Run("RefCusConditionType_SE.xml");
						break;
					// TODO: re-enable when parsing and importing are implemented
					//case Constants.ProgramFunctions.GoodsNomenclature:
					//	GoodsNomenclatureProgram.Run(ApplicationConfig.OutputDirectory, new DownloadExportXml());
					//	break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
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
