using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public class ItalyDailyExchangeRateParser
	{
		readonly ExchangeRateMetaData _metaData;
		readonly XmlDocument _xmlDoc;

		public ItalyDailyExchangeRateParser(ExchangeRateMetaData metaData)
		{
			_metaData = metaData;
			_xmlDoc = new XmlDocument();
			_xmlDoc.Load(_metaData.DataLocation);
		}

		public ExchangeRateMetaData ParseMeta()
		{
			var allCurrencyNode = _xmlDoc.ChildNodes[1].ChildNodes[2];
			var currencyWithTimeNode = allCurrencyNode.FirstChild;
			var time = currencyWithTimeNode.Attributes["time"].Value;
			var dateTime = DateTime.ParseExact(time, "yyyy-MM-dd", CultureInfo.InvariantCulture);
			_metaData.Version = string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd}", dateTime);
			_metaData.PublicationTime = dateTime;
			return _metaData;
		}

		public IList<RefExchangeRateZZ> Parse()
		{
			var allCurrencyNode = _xmlDoc.ChildNodes[1].ChildNodes[2];
			var currencyWithTimeNode = allCurrencyNode.FirstChild;

			var list = new List<RefExchangeRateZZ>();
			foreach (XmlElement node in currencyWithTimeNode.ChildNodes)
			{
				var attributes = node.Attributes;
				var exchangeRateZZXml = new RefExchangeRateZZ()
				{
					ZZN_EndDate = _metaData.PublicationTime,
					ZZN_StartDate = _metaData.PublicationTime,
					ZZN_RN_NKCountry = Constants.CountryISO,
					ZZN_ExRateType = Constants.RateType,
					ZZN_RX_NKExCurrency = attributes["currency"].Value,
					ZZN_Rate = decimal.Parse(attributes["rate"].Value, CultureInfo.InvariantCulture)
				};

				list.Add(exchangeRateZZXml);
			}
			return list;
		}
	}
}
