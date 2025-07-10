using System;
using System.Globalization;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
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
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			var yearToProcess = DateTime.UtcNow.Year;
			var fullXml = false;
			foreach (var arg in args)
			{
				if (arg.Length == 4 && int.TryParse(arg, out int passedYear))
				{
					if (passedYear >= 2000 && passedYear <= DateTime.UtcNow.Year + 20)
					{
						yearToProcess = passedYear;
						continue;
					}
				}
				if (arg.ToUpperInvariant() == "FULL")
				{
					fullXml = true;
					continue;
				}
			}

			var timeZoneFileDetail = FileFetcher.DownloadTimeZoneDatabase();
			if (!string.IsNullOrWhiteSpace(timeZoneFileDetail.FileLink))
			{
				new TimezoneDatabaseParser(timeZoneFileDetail.FileLink, yearToProcess, fullXml).ExportXML(timeZoneFileDetail.FileDateTime);
			}
			else
			{
				throw new ArgumentException("No file found.");
			}
		}
	}
}
