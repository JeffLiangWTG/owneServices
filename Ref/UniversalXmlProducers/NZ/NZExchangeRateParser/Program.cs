using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser
{
	class Program
	{
		static void Main(string[] args)
		{
			var sourceUrl = ApplicationConfig.SourceURL;
			var dumpPath = ApplicationConfig.OutputFilePath;

			var parser = new XmlRatesParser(sourceUrl);

			parser.ParseAsync(new HttpClientHelper())?.GetAwaiter().GetResult();

			var path = Path.Combine(dumpPath, $"RefExchangeRateZZ_NZ_{parser.PublicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.xml");
			parser.Save(path);
		}
	}
}
