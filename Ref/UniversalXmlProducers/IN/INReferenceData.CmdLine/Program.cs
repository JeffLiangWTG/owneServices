using System;
using System.Globalization;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Constants = CargoWise.RefDbRepo.INReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
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

			var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
			switch (functionToRun)
			{
				case Constants.ProgramFunctions.DBKTariff:
					DBKTariffProgram.Run(args);
					break;
				case Constants.ProgramFunctions.EDILocation:
					EDILocationProgram.Run();
					break;
				case Constants.ProgramFunctions.ExchangeRate:
					ExchangeRateProgram.Run();
					break;
				case Constants.ProgramFunctions.Tariff:
					TariffProgram.Run();
					break;
				case Constants.ProgramFunctions.ErrorCodes:
					ErrorCodesProgram.Run();
					break;
				case Constants.ProgramFunctions.WarehouseCode:
					WarehouseCodeProgram.Run();
					break;
				default:
					throw new ArgumentException($"Invalid argument entered: {functionToRun}");
			}
		}
	}
}
