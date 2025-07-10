using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	internal class CurrencyExchangeRateMessageProcessor : USIncomingMessageProcessor
	{
		const string PublishDateFormat = "MMddyy";

		public CurrencyExchangeRateMessageProcessor(string outputPath) : base(outputPath)
		{
		}

		public override string MetaDataPattern => @"%1[A-Z]{5}\d{6} \d{7}[DQ].{58}";

		public override string OutputFileName => "RefExchangeRateZZ_US_CBP_CUS_Message.xml";

		public override string XMLWriterDataSource => "US CBP CUS Exchange Rate";

		public override UpdateType UpdateType => UpdateType.Partial;

		public Func<DateTime> GetNowInUnitedStates = () => DateTime.UtcNow.AddHours(-5).Date;

		protected override void ProcessCore(MatchCollection matchMetaDatas)
		{
			var dictDay2ExchangeRates = GroupData2CurrencyExchangeRateByDate(matchMetaDatas);
			StringBuilder processInfo = new StringBuilder();
			foreach (var day2ExchangeRates in dictDay2ExchangeRates)
			{
				SaveXML(day2ExchangeRates.Value, day2ExchangeRates.Key, () => XmlWriterHelper.GetRefExchangeRateWriterConfiguration(day2ExchangeRates.Key, day2ExchangeRates.Key));
				Console.WriteLine($"Processed {day2ExchangeRates.Value.Count} exchange rates, publish date at {day2ExchangeRates.Key}.");
			}
		}

		static Dictionary<DateTime, List<RefExchangeRateZZ>> GroupData2CurrencyExchangeRateByDate(MatchCollection matchMetaDatas)
		{
			var dictDay2ExchangeRates = new Dictionary<DateTime, List<RefExchangeRateZZ>>();
			foreach (Match match in matchMetaDatas)
			{
				var (date, rate) = ParsePublishDateAndRefExchangeRate(match.Value);
				if (rate != null)
				{
					if (dictDay2ExchangeRates.TryGetValue(date, out var exchangeRates))
					{
						exchangeRates.Add(rate);
					}
					else
					{
						dictDay2ExchangeRates.Add(date, new List<RefExchangeRateZZ>() { rate });
					}
				}
			}
			return dictDay2ExchangeRates;
		}

		static (DateTime, RefExchangeRateZZ) ParsePublishDateAndRefExchangeRate(string messageData)
		{
			try
			{
				var dateString = messageData.Substring(7, 6);
				var rateString = messageData.Substring(14, 7);
				if (decimal.TryParse(rateString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var rate) && rate > 0
					&& DateTime.TryParseExact(dateString, PublishDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
				{
					var countryCode = messageData.Substring(2, 2);
					var currencyCode = messageData.Substring(4, 3);
					var result = new RefExchangeRateZZ()
					{
						ZZN_RN_NKCountry = Constants.USCountryCode,
						ZZN_ExRateType = Constants.ExchangeRate.DefaultRateType,
						ZZN_Rate = rate / 1000000,
						ZZN_RX_NKExCurrency = currencyCode
					};
					return (date, result);
				}
				return (DateTime.MinValue, null);
			}
			catch (ArgumentOutOfRangeException)
			{
				return (DateTime.MinValue, null);
			}
		}
	}
}
