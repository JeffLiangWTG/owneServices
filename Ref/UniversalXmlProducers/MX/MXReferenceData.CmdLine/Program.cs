using System;
using System.Globalization;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Constants = CargoWise.RefDbRepo.MXReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.MXReferenceData.CmdLine
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
					case Constants.ProgramFunctions.CustomsExchangeRate:
						new ExchangeRatesProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsFacilities:
						new CustomsFacilitiesProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffRate:
						new TariffRatesProgram().Run();
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
