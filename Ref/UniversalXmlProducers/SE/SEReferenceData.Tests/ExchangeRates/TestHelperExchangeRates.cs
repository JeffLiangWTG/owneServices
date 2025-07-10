using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services.IncrementalExport;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Tests
{
	internal static class TestHelperExchangeRates
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5369:Use XmlReader for 'XmlSerializer.Deserialize()'", Justification = "Internal Tool")]
		internal static monetaryExchangePeriod[] GetExchangeRatesFromEmbeddedResource(string embeddedResourceName)
		{
			monetaryExchangePeriod[] monetaryExchangePeriods = new monetaryExchangePeriod[0];
			using (var inputTestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName))
			{
				var xmlData = new XmlDocument();
				xmlData.Load(inputTestStream);
				var serializer = new XmlSerializer(typeof(export));
				using (var stream = new StringReader(xmlData.OuterXml))
				{
					var records = (export)serializer.Deserialize(stream);
					monetaryExchangePeriods = records.items.Where(x => x.Item is monetaryExchangePeriod period && period.national == 1L).Select(x => x.Item).Cast<monetaryExchangePeriod>().ToArray();
				}
			}
			return monetaryExchangePeriods;
		}
	}
}
