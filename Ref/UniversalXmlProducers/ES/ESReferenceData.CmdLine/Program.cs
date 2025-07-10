using System;
using System.Globalization;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
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
				var outputPath = ApplicationConfig.OutputPath;
				var functionToRun = args[0].ToUpper(CultureInfo.CurrentCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.ExchangeRates:
						ExchangeRatesProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.VAT:
						VATProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.IGIC:
						IGICProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.AIEM:
						AIEMProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.ESEXC:
						ESEXCProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.CANEXC:
						CANEXCProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.C44DOC:
						C44DocumentsProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.REA:
						REAProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.LOCATIONS:
						LocationsProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.CPC:
						CPCProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.MEA:
						MEAProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.REAMeasures:
						REAMeasuresProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.Quota:
						QuotaProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.CSVProcessor:
						CSVProcessorProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.TariffOne:
						TariffOneProgram.Run(outputPath);
						break;
					case Constants.ProgramFunctions.TariffOneRates:
						TariffOneRatesProgram.Run(outputPath);
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

		public static void PrintLogMessage(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				Console.Out.WriteLine(message);
			}
		}
	}
}
