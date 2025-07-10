using System;
using System.Linq;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
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
			ApplicationConfig.ConfigEnvironment();
			var tradeGroupProducer = new TradeGroupProducer();

			if (args.Length != 0)
			{
				switch (args[0].ToUpperInvariant())
				{
					case ApplicationConfig.ProgramFunctions.Nomenclature:
						NomenclatureProgram.Run();
						break;
					case ApplicationConfig.ProgramFunctions.NomenclaturePlusDaily:
						NomenclaturePlusDailyProgram.Run();
						break;
					case ApplicationConfig.ProgramFunctions.TradeGroup:
						tradeGroupProducer.Run();
						break;
					case ApplicationConfig.ProgramFunctions.Tariff:
						new ImportTariffProducer().Run(args.Skip(1), new NomenclatureProducer().Run(false).Tariffs);
						break;
					case ApplicationConfig.ProgramFunctions.Daily:
						var downloadFolderPath = "";
						if (args.Length > 1)
						{
							downloadFolderPath = args[1];
						}
						var nomenclatureProducerForDaily = new NomenclatureProducer();
						var results = nomenclatureProducerForDaily.Run(false);
						var dailyTariffFactory = new DailyTariffFactory(results.Tariffs, downloadFolderPath);
						var dailyTariffProducer = new DailyTariffProducer(dailyTariffFactory);
						dailyTariffProducer.Run();
						break;
					case ApplicationConfig.ProgramFunctions.AdditionalCodes:
						new AdditionalCodesProducer().Run();
						break;
					default:
						throw new ArgumentException($"Invalid params entered: {args[0]}.");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}
