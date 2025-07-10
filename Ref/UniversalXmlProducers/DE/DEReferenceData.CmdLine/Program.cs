using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Business;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			await ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static async Task ProduceXml(string[] args)
		{
			if (args.Length != 0)
			{
				var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.ExchangeRates:
						ExchangeRatesProgram.Run(ApplicationConfig.OutputPath);
						break;
					case Constants.ProgramFunctions.DeTariffs:
						await DeTariffsProgram.Run(ApplicationConfig.OutputPath);
						break;
					case Constants.ProgramFunctions.CodeLists:
						CodeListsProgram.Run(args.Skip(1).ToArray(), ApplicationConfig.OutputPath);
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

		public static void PrintErrorMessage(string errorString)
		{
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.Error.WriteLine(errorString);
			}
		}
	}
}
