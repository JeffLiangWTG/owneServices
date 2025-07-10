using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class CustomsExchangeRatesSOAPResponseParser
	{
		public CustomsExchangeRatesSOAPResponseParser(string responseData)
		{
			doc = XDocument.Parse(responseData);
		}

		public IEnumerable<ExchangeRate> Parse()
		{
			var exchangeRates = new List<ExchangeRate>();
			var functionCode = string.Empty;

			foreach (var node in doc.Descendants().Where(x => x is XElement element && element.Name == q + "FunctionCode"))
			{
				functionCode = node.Value;
			}

			foreach (var declarationNode in doc.Descendants().Where(x => x is XElement element && element.Name == q + "Declaration"))
			{
				foreach (var currencyExchangeNode in declarationNode.Descendants().Where(x => x is XElement element && element.Name == q + "CurrencyExchange"))
				{
					var exchangeRate = new ExchangeRate();
					exchangeRate.FunctionCode = functionCode;
					exchangeRate.CurrencyTypeCode = currencyExchangeNode.Element(q + "CurrencyTypeCode").Value;
					exchangeRate.RateNumeric = GetDecimalFromElement(currencyExchangeNode, q + "RateNumeric");
					exchangeRates.Add(exchangeRate);
				}
			}

			return exchangeRates;
		}

		static decimal GetDecimalFromElement(XElement element, XName elementName)
		{
			decimal dt = 0m;

			if (element?.Element(elementName)?.Value != null)
			{
				dt = decimal.Parse(element.Element(elementName)?.Value, CultureInfo.InvariantCulture);
			}

			return dt;
		}

		readonly XDocument doc;
		readonly XNamespace q = "urn:wco:datamodel:WCO:ExchangeRateInformation:1";
	}
}
