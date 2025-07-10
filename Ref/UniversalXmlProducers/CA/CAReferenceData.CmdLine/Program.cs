using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
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
					case Constants.ProgramFunctions.CAFacilityData:
						CAFacilityData.Run(args.Skip(1).ToArray());
						break;
					case Constants.ProgramFunctions.CATariff:
						CATariffDataRunner.Run();
						break;
					case Constants.ProgramFunctions.CFIAAIRSRegistrationTypes:
						CFIAAIRSRegistrationTypes.Run();
						break;
					case Constants.ProgramFunctions.CFIAAIRSMiscData:
						CFIAAIRSMiscData.Run();
						break;
					case Constants.ProgramFunctions.CBSAErrorCode:
						CBSAErrorCode.Run();
						break;
					case Constants.ProgramFunctions.CAExchangeRate:
						CAExchangeRate.Run();
						break;
					case Constants.ProgramFunctions.CAOfficeCode:
						CAOfficeCode.Run();
						break;
					case Constants.ProgramFunctions.ManualProcessor:
						ManualProcessor.Run();
						break;
					case Constants.ProgramFunctions.CASIMAData:
						CASIMADataRunner.Run();
						break;
					case Constants.ProgramFunctions.CASurtaxData:
						CASurtaxRunner.Run();
						break;
						case Constants.ProgramFunctions.CAGSTCode:
						CAGSTCodeRunner.Run();
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
			Console.Error.WriteLine(errorString);
		}
	}
}
