using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
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
			var requiredDataDate = DateTime.UtcNow;
			if (args.Length > 0)
			{
				if (!DateTime.TryParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out requiredDataDate))
				{
					throw new ArgumentException("Unable to parse date parameter. Expected format is yyyy-MM-dd");
				}
			}
			var safeRepository = new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));
			var xmlProducer = new RefUNLOCOUtcOffsetXmlProducer(requiredDataDate);

			var timeZoneFileDetail = FileFetcher.DownloadTimeZoneDatabase();
			new RefUNLOCOUtcOffsetGenerator(new UnlocoDataHelper(safeRepository), requiredDataDate
				, Convert.ToInt16(ApplicationConfig.RequiredOffsetPeriodInMonths, CultureInfo.InvariantCulture)
				, xmlProducer, timeZoneFileDetail.FileLink).ExportXml();
		}
	}
}
