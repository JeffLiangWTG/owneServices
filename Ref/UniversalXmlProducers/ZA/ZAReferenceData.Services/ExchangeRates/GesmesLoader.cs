using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models;
using Enterprise.Edifact.D96B.Messages.GESMES;
using Enterprise.Edifact.D96B.Segments;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates
{
	internal static class GesmesLoader
	{
		public static IReadOnlyCollection<ExchangeRate> PopulateExchange(GESMESMessage message)
		{
			var rates = new List<ExchangeRate>();

			if (message?.UNH != null)
			{
				var effectiveDate = DateTime.ParseExact(message.DTM[0].DateTimePeriod.DateTimePeriod, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
				rates.AddRange(CreateExchangeRates(message.Group11[0].ARR, effectiveDate));
			}

			return rates;
		}

		static IReadOnlyCollection<ExchangeRate> CreateExchangeRates(ARRSegmentMessageSection segMsgSec, DateTime effectiveDate)
		{
			var rates = new List<ExchangeRate>();

			foreach (ARRSegment arr in segMsgSec)
			{
				rates.Add(new ExchangeRate
				{
					Currency = arr.PositionIdentification.HierarchicalIdNumber,
					Rate = decimal.Parse(arr.ArrayCellDetails.ArrayCellInformation, CultureInfo.InvariantCulture),
					StartDate = effectiveDate,
					EndDate = effectiveDate
				});
			}

			return rates;
		}

	}
}
