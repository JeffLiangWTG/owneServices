using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates
{
	public static class MessageConversionHelper
	{
		public static ExchangeRateData Convert(SourceDataMessage msg, ILogger logger)
		{
			ExchangeRateData exchange = null;

			var edifactMessage = EdifactLoader.LoadGesmesMessage(msg.Content);

			var validationErrors = new StringBuilder();

			if (GesmesValidator.ValidateMessage(edifactMessage, validationErrors))
			{
				var rates = GesmesLoader.PopulateExchange(edifactMessage);

				exchange = ConvertRates(rates, msg.CreatedDate);
			}
			else
			{
				logger.LogError($"Validation errors for: {msg.ID} {msg.Filename}");
				logger.LogError(validationErrors.ToString());
			}

			return exchange;
		}

		internal static ExchangeRateData ConvertRates(IReadOnlyCollection<ExchangeRate> rawRates, DateTime publishDate)
		{
			var data = new ExchangeRateData
			{
				PublishDate = publishDate
			};

			var rates = new List<ExchangeRate>();

			foreach (var rr in rawRates)
			{
				var ccy = ConvertCurrency(rr.Currency);

				foreach (var country in countryMappings)
				{
					rates.Add(new ExchangeRate
					{
						CountryCode = country,
						RateType = rateType,
						Currency = ccy,
						Rate = rr.Rate,
						StartDate = rr.StartDate,
						EndDate = rr.EndDate
					});
				}
			}

			data.ExchangeRates = rates;

			return data;
		}

		internal static string ConvertCurrency(string currency)
		{
			var ccy = currency;

			if (currencyMapping.TryGetValue(currency, out var mappedCurrency))
			{
				ccy = mappedCurrency;
			}

			return ccy;
		}

		static Dictionary<string, string> currencyMapping = new Dictionary<string, string>
		{
			{ "ZWD","ZWL" }
		};

		static string[] countryMappings = { "ZA", "NA", "LS", "SZ" };
		const string rateType = "CUS";
	}
}
