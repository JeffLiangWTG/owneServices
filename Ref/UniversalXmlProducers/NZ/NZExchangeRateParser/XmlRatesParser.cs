using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser
{
	public class XmlRatesParser
	{
		readonly string _sourceUrl;
		readonly IXmlWriter _xmlWriter;

		public DateTime PublicationTime { get; set; } = DateTime.Now;

		public XmlRatesParser(string sourceUrl)
		{
			Argument.NotNullOrEmpty(sourceUrl, nameof(sourceUrl));
			_sourceUrl = sourceUrl;
			_xmlWriter = new XmlWriter(XmlWriterHelper.GetRefExchangeRateZZConfiguration());
			_xmlWriter.SetDataSource("Ref Exchange Rate NZ");
			_xmlWriter.SetUpdateType(UpdateType.Partial);
		}

		public async Task ParseAsync(IHttpClientHelper httpClientHelper)
		{
			Argument.NotNull(httpClientHelper, nameof(httpClientHelper));

			using (var file = await httpClientHelper.GetAsync(_sourceUrl))
			{
				var sourceXml = XDocument.Load(file);

				var exchangeRates = sourceXml.Descendants(XName.Get("exchangeRate"));
				_xmlWriter.SetPublicationTime(PublicationTime);
				foreach (var rate in exchangeRates)
				{
					PopulateWriter(rate);
				}
			}
		}

		void PopulateWriter(XElement rate)
		{
			Argument.NotNull(rate, nameof(rate));

			var rates = ConvertFromXmlToRates(rate);

			if (rates != null)
			{
				foreach (var rateZZ in rates)
				{
					_xmlWriter.PopulateData(rateZZ);
				}
			}
		}

		static RefExchangeRateZZ[] ConvertFromXmlToRates(XElement element)
		{
			Argument.NotNull(element, nameof(element));

			DateTime dateNow = DateTime.MinValue, dateFuture = DateTime.MinValue;
			decimal rateNow = decimal.MinValue, rateFuture = decimal.MinValue;

			var isProcess = true && DateTime.TryParseExact(element.GetNode("dateNow")?.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateNow)
				&& DateTime.TryParseExact(element.GetNode("dateFuture")?.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFuture)
				&& decimal.TryParse(element.GetNode("rateNow")?.Value, out rateNow)
				&& decimal.TryParse(element.GetNode("rateFuture")?.Value, out rateFuture);

			var currencyCode = element.GetNode("currencyCode")?.Value;

			if (!isProcess)
			{
				Console.Error.WriteLine($"Incorrect DateTime or Decimal Format for {currencyCode}");
				return null;
			}
			else
			{
				return new RefExchangeRateZZ[2] {
				new RefExchangeRateZZ {
					ZZN_ExRateType = "CUS",
					ZZN_StartDate = dateNow.AddDays(-13),
					ZZN_EndDate = dateNow,
					ZZN_Rate = rateNow,
					ZZN_RX_NKExCurrency = currencyCode,
					ZZN_RN_NKCountry = "NZ"
				},
				new RefExchangeRateZZ {
					ZZN_ExRateType = "CUS",
					ZZN_StartDate = dateFuture.AddDays(-13),
					ZZN_EndDate = dateFuture,
					ZZN_Rate = rateFuture,
					ZZN_RX_NKExCurrency = currencyCode,
					ZZN_RN_NKCountry = "NZ"
				} };
			}
		}

		public void Save(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			_xmlWriter.SaveXml(path);
		}
	}
}
