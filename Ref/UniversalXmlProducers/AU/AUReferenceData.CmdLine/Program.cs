using System;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
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
				var outputPath = ApplicationConfig.OutputDirectory;
				var functionToRun = args[0].ToUpperInvariant();
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.NexDocs:
						NexDocsProgram.Run(args.Skip(1).ToArray(), outputPath);
						break;
					case Constants.ProgramFunctions.ExchangeRate:
						new ExchangeRateProgram().Run(outputPath);
						break;
					case Constants.ProgramFunctions.LCTThresholds:
						new LuxuryCarTaxProgram().Run(outputPath);
						break;
					case Constants.ProgramFunctions.Nomenclature:
						new NomenclatureParser().Parse(DateTime.Now);
						break;
					case Constants.ProgramFunctions.AHECC:
						new AHECCProgram().RunFullUpdate();
						break;
					case Constants.ProgramFunctions.AHECCPartial:
						new AHECCProgram().RunPartialUpdate();
						break;
					case Constants.ProgramFunctions.CMRRefDataFull:
						new CMRReferenceDataProgram().Run();
						break;
					case Constants.ProgramFunctions.CMRRefDataPartial:
						new CMRReferenceDataProgram().RunPartial();
						break;
					case Constants.ProgramFunctions.CustomsTariff:
						TariffDataProgram.Run(args);
						break;
					case Constants.ProgramFunctions.CMRRefTestData:
						CMRReferenceTestDataProgram.Run(args);
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
