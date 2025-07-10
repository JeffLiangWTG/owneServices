using System.IO;
using System.Xml;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class DownloadExchangeRates
	{
		public static XmlDocument Download(string url)
		{
			var xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(url);
			}
			catch (FileNotFoundException)
			{
				throw new ExchangeRatesException($"Unable to Load ES Exchange Rate XML from the following URL: {url}");
			}
			return xmlDocument;
		}
	}
}
