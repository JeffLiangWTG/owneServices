#define USE_CHROME_DRIVER

using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
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
			var saveFilePath = Application.OutputFileFolderPath;
			if (!Directory.Exists(saveFilePath))
				Directory.CreateDirectory(saveFilePath);
			if (args.Length != 0 && args[0] == "?")
			{
				Console.WriteLine("Parameter 1 - Start Date (default current date) format: yyyyMMdd");
				Console.WriteLine("Parameter 1 - Number of days ago - Startdate = current date - parameter 1: end date = current date");
				Console.WriteLine("Parameter 2 - End Date (default current date) format: yyyyMMdd");
			}
			else if (args.Length != 0)
			{
				if (!DateTime.TryParseExact(args[0], Constants.dateTimeFormat, null, DateTimeStyles.None, out DateTime parsedStartDate))
				{
					if (int.TryParse(args[0], out int daysAgo))
					{
						parsedStartDate = DateTime.UtcNow.AddDays(daysAgo * -1);
					}
					else
					{
						parsedStartDate = DateTime.UtcNow;
					}
				}
				DateTime parsedEndDate = DateTime.MaxValue;
				if (args.Length > 1)
				{
					DateTime.TryParseExact(args[1], Constants.dateTimeFormat, null, DateTimeStyles.None, out parsedEndDate);
				}
				if (parsedEndDate == DateTime.MaxValue)
					parsedEndDate = DateTime.UtcNow;

				saveFilePath += @"\EUNDailyTariffs" + parsedStartDate.ToString(Constants.dateTimeFormat)
					+ "-" + parsedEndDate.ToString(Constants.dateTimeFormat) + ".xml";

				using (var client = new WebDriverHelper())
				{
					new EUNTariffWebsiteParser(client, saveFilePath, parsedEndDate, parsedStartDate, parsedEndDate).ProduceXML();
				}
			}
			else
			{
				saveFilePath += @"\EUNDailyTariffs" + DateTime.UtcNow.ToString("yyyyMMdd") + ".xml";
				using (var client = new WebDriverHelper())
				{
					new EUNTariffWebsiteParser(client, saveFilePath, DateTime.UtcNow, DateTime.UtcNow).ProduceXML();
				}
			}
		}
	}
}
