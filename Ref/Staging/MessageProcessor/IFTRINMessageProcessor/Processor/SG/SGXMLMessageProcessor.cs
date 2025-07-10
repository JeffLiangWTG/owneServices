using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public class SGXMLMessageProcessor : BaseMessageProcessor
	{
		public SGXMLMessageProcessor(string outputPath)
			: base(DataSourceConstants.Country.Singapore, outputPath)
		{
		}

		#region Process

		protected override (bool success, DateTime issueDate) ProcessCore(string messageText)
		{
			var success = false;
			var issueDate = DateTime.MinValue;
			var customsExchangeRate = Deserialize(messageText)?.OutboundMessage?.CustomsExchangeRate;

			if (customsExchangeRate != null)
			{
				issueDate = GetDate(customsExchangeRate.IssueDate.ToString(CultureInfo.InvariantCulture));
				ProcessCustomsExchangeRate(customsExchangeRate);

				success = true;
			}

			return (success, issueDate);
		}

		static TradenetResponse Deserialize(string messageText)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				using (var reader = XmlReader.Create(new StringReader(messageText)))
				{
					var serializer = new XmlSerializer(typeof(TradenetResponse));
					return serializer.Deserialize(reader) as TradenetResponse;
				}
			}
			catch (Exception)
			{
				return null;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		void ProcessCustomsExchangeRate(CustomsExchangeRate customsExchangeRate)
		{
			Argument.NotNull(customsExchangeRate, nameof(customsExchangeRate));

			var startDate = GetDate(customsExchangeRate.EffectivePeriod?.StartDate.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
			var endDate = GetDate(customsExchangeRate.EffectivePeriod?.EndDate.ToString(CultureInfo.InvariantCulture) ?? string.Empty);

			foreach (var rate in customsExchangeRate.ExchangeRate)
			{
				if (rate != null)
				{
					var currencyExchangeRate = rate.CurrencyExchangeRate / Convert.ToDecimal(rate.RateUnit);
					AddExchangeRate(rate.CurrencyCode, currencyExchangeRate, startDate, endDate);
				}
			}
		}

		static DateTime GetDate(string date)
		{
			return string.IsNullOrWhiteSpace(date) || date.Length != 8
				? DateTime.MinValue
				: new DateTime(Convert.ToInt32(date.Substring(0, 4), CultureInfo.InvariantCulture), Convert.ToInt32(date.Substring(4, 2), CultureInfo.InvariantCulture), Convert.ToInt32(date.Substring(6, 2), CultureInfo.InvariantCulture));
		}

		#endregion
	}
}
