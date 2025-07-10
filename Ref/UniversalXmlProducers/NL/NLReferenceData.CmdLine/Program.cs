using System;
using System.Globalization;
using CargoWise.RefDbRepo.NLReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
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
					case Constants.ProgramFunctions.CodeLists:
						CodeLists.Run();
						break;
					case Constants.ProgramFunctions.FiscalExchangeRates:
						FiscalExchangeRatesProgram.Run();
						break;
					case Constants.ProgramFunctions.Tariffs:
						TariffsProgram.Run();
						break;
					case Constants.ProgramFunctions.CustomsExchangeRates:
						CustomsExchangeRatesProgram.Run();
						break;
					case Constants.ProgramFunctions.ECCNCodeList:
						ECCNCodeListProgram.Run();
						break;
					default:
						throw new ArgumentException(Invariant($"Invalid argument: {func}"));
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}
