using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This class performs group validations on RateLines where we need to validate each rateLine with all other ratelines in the group.
	/// It is designed to utilize precalculated values used during each RateLine validation. So, we don't calculate value on every rate line validation.
	/// </summary>
	class RateLineGroupValidation
	{
		public RateLineGroupValidation()
		{
			rateLineChargeCodeLookup = new Dictionary<ZGuid, RateLineGroupValidationInfo>();
		}

		readonly Dictionary<ZGuid, RateLineGroupValidationInfo> rateLineChargeCodeLookup;

		public void AddItem(RateLine rateLine)
		{
			if (rateLine != null)
			{
				var chargeCode = rateLine.TL_AC;
				RateLineGroupValidationInfo value;

				if (!rateLineChargeCodeLookup.TryGetValue(chargeCode, out value))
				{
					value = new RateLineGroupValidationInfo();
					rateLineChargeCodeLookup.Add(chargeCode, value);
				}

				value.Prepare(rateLine);
			}
		}

		public bool HasSameChargeCodeWithOverlappingCurrencies(RateLine rateLine)
		{
			if (rateLine != null)
			{
				var chargeCode = rateLine.TL_AC;
				RateLineGroupValidationInfo value;

				if (rateLineChargeCodeLookup.TryGetValue(chargeCode, out value))
				{
					return value.HasOverlappingCurrencies(rateLine);
				}
			}

			return false;
		}

		public bool IsJobLevelAvailableIsConsistent(RateLine rateLine)
		{
			if (rateLine != null)
			{
				var chargeCode = rateLine.TL_AC;
				RateLineGroupValidationInfo value;

				if (rateLineChargeCodeLookup.TryGetValue(chargeCode, out value))
				{
					return value.IsJobLevelAvailableIsConsistent;
				}
			}

			return true;
		}

		class RateLineGroupValidationInfo
		{
			readonly HashSet<ZString> currency = new HashSet<ZString>();
			readonly List<RateLine> rateLines = new List<RateLine>();
			bool isWhsJobLevelCharge;
			bool isNotWhsJobLevelCharge;

			public void Prepare(RateLine rateLine)
			{
				rateLines.Add(rateLine);

				if (!rateLine.TL_RX_NKCurrency.IsEmpty)
				{
					currency.Add(rateLine.TL_RX_NKCurrency);
				}

				if (rateLine.IsJobLevelAvailable)
				{
					if (rateLine.TL_IsWhsJobLevelCharge)
					{
						isWhsJobLevelCharge = true;
					}
					else
					{
						isNotWhsJobLevelCharge = true;
					}
				}
			}

			public bool HasOverlappingCurrencies(RateLine rateLine)
			{
				if (currency.Count < 2)
				{
					return false;
				}

				return rateLines.Any(l => l.TL_RX_NKCurrency != rateLine.TL_RX_NKCurrency && DatesOverlap(rateLine, l));

				bool DatesOverlap(RateLine rateLine1, RateLine rateLine2)
				{
					var startDate1 = rateLine1.TL_RateStartDate.IsEmpty ? new ZDate(ZDateTime.MinSmallDateTimeValue) : rateLine1.TL_RateStartDate;
					var endDate1 = rateLine1.TL_RateEndDate.IsEmpty ? new ZDate(ZDateTime.MaxSmallDateTimeValue) : rateLine1.TL_RateEndDate;

					var startDate2 = rateLine2.TL_RateStartDate.IsEmpty ? new ZDate(ZDateTime.MinSmallDateTimeValue) : rateLine2.TL_RateStartDate;
					var endDate2 = rateLine2.TL_RateEndDate.IsEmpty ? new ZDate(ZDateTime.MaxSmallDateTimeValue) : rateLine2.TL_RateEndDate;

					return startDate1 <= endDate2 && startDate2 <= endDate1;
				}
			}

			public bool IsJobLevelAvailableIsConsistent => !(isWhsJobLevelCharge && isNotWhsJobLevelCharge);
		}
	}
}
