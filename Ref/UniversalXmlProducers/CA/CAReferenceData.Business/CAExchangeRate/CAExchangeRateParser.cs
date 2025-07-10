using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public class CAExchangeRateParser : ICAExchangeRateParser
	{
		public bool ParseExchangeRateIntoXML(List<ForeignExchangeRates> exchangeRates, string exportFilePath, DateTime latestUpdateOnDate)
		{
			var nextDatRates = new List<RefExchangeRateZZ>();
			var currentDatRates = new List<RefExchangeRateZZ>();
			var beforeDatRates = new List<RefExchangeRateZZ>();
			foreach (var foreignExchangeRate in exchangeRates)
			{
				var rate = decimal.Parse(foreignExchangeRate.Rate, CultureInfo.InvariantCulture);
				if (rate != 0m)
				{
					var effectiveDate = Convert.ToDateTime(foreignExchangeRate.ExchangeRateEffectiveTimestamp, CultureInfo.InvariantCulture).Date;
					if (effectiveDate == latestUpdateOnDate)
					{
						PopulateRateList(nextDatRates, foreignExchangeRate);
					}
					else if (effectiveDate == latestUpdateOnDate.AddDays(-1))
					{
						PopulateRateList(currentDatRates, foreignExchangeRate);
					}
					else
					{
						PopulateRateList(beforeDatRates, foreignExchangeRate);
					}
				}
			}

			var endDate = latestUpdateOnDate.AddDays(1).AddMinutes(-1);
			if (beforeDatRates.Any())
			{
				PopulateHistoricalData(currentDatRates, beforeDatRates, latestUpdateOnDate.AddDays(-2), endDate.AddDays(-2));
			}
			beforeDatRates.Clear();

			if (currentDatRates.Any())
			{
				PopulateHistoricalData(nextDatRates, currentDatRates, latestUpdateOnDate.AddDays(-1), endDate.AddDays(-1));
			}
			currentDatRates.Clear();

			var hasData = nextDatRates.Any();
			if (hasData)
			{
				ExportToXMLFile(exportFilePath, latestUpdateOnDate, endDate, nextDatRates);
			}
			return hasData;
		}

		static void PopulateHistoricalData(List<RefExchangeRateZZ> exchangeRates, List<RefExchangeRateZZ> previousExchangeRates, DateTime startDate, DateTime endDate)
		{
			foreach (var previousExchangeRate in previousExchangeRates)
			{
				var currency = previousExchangeRate.ZZN_RX_NKExCurrency;
				if (!exchangeRates.Exists(x => x.ZZN_RX_NKExCurrency == currency))
				{
					exchangeRates.Add(new RefExchangeRateZZ()
					{
						ZZN_Rate = previousExchangeRate.ZZN_Rate,
						ZZN_RX_NKExCurrency = previousExchangeRate.ZZN_RX_NKExCurrency
					});
				}
				if (previousExchangeRate.ZZN_StartDate == DateTime.MinValue)
				{
					previousExchangeRate.ZZN_StartDate = startDate;
					previousExchangeRate.ZZN_EndDate = endDate;
				}
				exchangeRates.Add(previousExchangeRate);
			}
		}

		static void PopulateRateList(List<RefExchangeRateZZ> exchangeRates, ForeignExchangeRates foreignExchangeRate)
		{
			var currency = foreignExchangeRate.FromCurrency.Value;
			var rate = exchangeRates.Find(x => x.ZZN_RX_NKExCurrency == currency);
			if (rate == null)
			{
				exchangeRates.Add(new RefExchangeRateZZ()
				{
					ZZN_Rate = decimal.Parse(foreignExchangeRate.Rate, CultureInfo.InvariantCulture),
					ZZN_RX_NKExCurrency = currency
				});
			}
			else
			{
				rate.ZZN_Rate = decimal.Parse(foreignExchangeRate.Rate, CultureInfo.InvariantCulture);
			}
		}

		static void ExportToXMLFile(string exportFilePath, DateTime publicationDateTime, DateTime endDate, List<RefExchangeRateZZ> exchangeRates, UpdateType updateType = UpdateType.Partial)
		{
			var _xmlWriter = new Common.UniversalXmlWriter.XmlWriter(XMLWriterHelper.GetRefExchangeRateWriterConfiguration(publicationDateTime, endDate));
			_xmlWriter.SetDataSource(Constants.DataSource.ExchangeRate);
			_xmlWriter.SetPublicationTime(publicationDateTime);
			_xmlWriter.SetUpdateType(updateType);
			foreach (var rate in exchangeRates)
			{
				_xmlWriter.PopulateData(rate);
			}
			_xmlWriter.SaveXml(exportFilePath);
		}
	}
}
