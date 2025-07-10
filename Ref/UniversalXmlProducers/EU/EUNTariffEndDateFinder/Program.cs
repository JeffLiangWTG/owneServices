using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			var workingFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.WorkingFolder);
			var output = Path.Combine(ApplicationConfig.OutputFilePath, "EUNTariffEndDates.xml");
			var safeRepo = new SafeRepository(new Uri(ApplicationConfig.SafeUpdateServiceUri));
			var files = Directory.GetFiles(workingFolder, "*.xlsx");
			var parser = new DeclarableCodeParser();
			var updater = new NomenclatureEndDateUpdater(safeRepo);
			foreach (var file in files)
			{
				var fileName = Path.GetFileNameWithoutExtension(file);
				var fileDateTime = DateTime.ParseExact(fileName.Substring(fileName.Length - 8), "yyyyMMdd", CultureInfo.InvariantCulture);
				var filePath = Path.Combine(workingFolder, file);
				var codes = parser.Parse(filePath).Where(x => x.IsLeaf).Select(x => x.TariffHeader.Substring(0, 10));
				updater.Update(codes, fileDateTime);
				Console.WriteLine($"Dealing with {file}, {codes.Count()} TariffCodes processed.");
			}
			var codesToWrite = updater.GetCodesWithEndDates();
			if (ApplicationConfig.CheckCodesInSafeDb)
			{
				codesToWrite = updater.CheckCodesInSafeDb(DateTime.Today).Result;
			}
			NomenclatureEndDateXmlBuilder.Write(output, codesToWrite);
		}
	}
}
