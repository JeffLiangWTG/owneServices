using System;
using System.Linq;
using CargoWise.Types;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public static class RateExtensions
	{
		public static ZDate StartDate(this Rate rate)
		{
			return rate.StartDate.ToZDate();
		}

		public static ZDate ExpiryDate(this Rate rate)
		{
			return rate.ExpiryDate.ToZDate();
		}

		public static ZDate ToZDate(this DateTime? time)
		{
			return time.HasValue ? time.Value.ToZDate() : ZDate.Empty;
		}

		public static ZDate ToZDate(this DateTime time)
		{
			return time == default(DateTime) ? ZDate.Empty : CreateZDateFromDateTime(time);
		}

		static ZDate CreateZDateFromDateTime(DateTime time)
		{
			var offset = new ZDateTimeOffset(time);
			return new ZDate(offset.Year, offset.Month, offset.Day);
		}

		public static string ContainerQuality(this Rate wiseRate)
		{
			var containerQuality = wiseRate
				.ProviderCustomFields?
				.FirstOrDefault(x => string.Equals(x?.Code, Rate.CustomFields.CargoSphere.ContainerQuality))?
				.Value;

			return (containerQuality as string) ?? string.Empty;
		}
	}

	public static class ChargeExtensions
	{
		public static bool HasZeroRate(this Charge wiseCharge)
		{
			return (wiseCharge.FlatRate == null || wiseCharge.FlatRate.Value == 0) && (wiseCharge.PerUnitRate == null || wiseCharge.PerUnitRate.Value == 0);
		}
	}
}
