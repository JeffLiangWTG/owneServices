using System;
using System.Globalization;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
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
					case Constants.ProgramFunctions.CIQOfficeCode:
						CIQOfficeCodeProgram.Run(args);
						break;
					case Constants.ProgramFunctions.Tariffs:
						new TariffsUpdateProgram().Run(args);
						break;
					case Constants.ProgramFunctions.TariffsFromExcel:
						TariffsFromExcelProgram.Run();
						break;
					case Constants.ProgramFunctions.DecTpAccess:
						DecTpAccessProgram.Run(args);
						break;
					case Constants.ProgramFunctions.ExchangeRate:
						ExchangeRateProgram.Run();
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
