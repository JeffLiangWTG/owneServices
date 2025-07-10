using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2200:Rethrow to preserve stack details", Justification = "<Pending>")]
	public static class DownloadExchangeRates
	{
		const string xml = "XML";

		public static Tarih_Date Download(string url)
		{
			Tarih_Date result;
			try
			{
				var exchangeRatesXML = new XmlDocument();
				exchangeRatesXML.Load(url);
				result = DeserializeFromString(exchangeRatesXML.OuterXml);
			}
			catch (Exception ex)
			{
				var sb = new StringBuilder();
				sb.AppendLine(CultureInfo.InvariantCulture, $"Unable to Load TR Exchange Rate XML from the following URL: {url}");
				sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.Message}");

				if (ex.InnerException != null)
				{
					sb.AppendLine(CultureInfo.InvariantCulture, $"Inner Exception: {ex.InnerException.Message}");
				}

				if (ex.Data.Contains(xml))
				{
					sb.AppendLine(CultureInfo.InvariantCulture, $"{xml}: {ex.Data[xml]}");
				}

				throw new ExchangeRatesException(sb.ToString());
			}
			return result;
		}

		static Tarih_Date DeserializeFromString(string xmlData)
		{
			Tarih_Date result;
			var serializer = new XmlSerializer(typeof(Tarih_Date));
			using (var stringReader = new StringReader(xmlData))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				try
				{
					result = (Tarih_Date)serializer.Deserialize(xmlReader);
				}
				catch (InvalidOperationException ex)
				{
					ex.Data.Add(xml, xmlData);

					throw;
				}
			}
			return result;
		}
	}
}
