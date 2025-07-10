using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class RatesUtils
	{
		public static RefCusRate FeedRefCusTariffRate(string percentValue, string rateCode, string preference = null, DateTime? applicabilityStartDate = null, DateTime? applicabilityEndDate = null)
		{
			string rateFormula;
			if (double.TryParse(percentValue, out var formula))
			{
				rateFormula = formula > 0 ? $"VFD * {(formula / 100):0.00##}" : "0";
			}
			else
			{
				return null;
			}

			var rate = new RefCusRate
			{
				ZZ2_ZY1_NKRateCode = rateCode,
				ZZ2_ZY1_ZZR_NKRateType = GetRateTypeByCode(rateCode),
				ZZ2_RateFormula = rateFormula,
				ZZ2_RateFormulaDerivedFrom = percentValue,
				ZZ2_ZZS_NKPreference = preference,
				ZZ2_ZZS_ZZZ_NKDataGrouping = string.IsNullOrEmpty(preference) ? null : Constants.DataGroupingCodes.Brazil,
				RefCusApplicabilities = FeedRefCusApplicability(applicabilityStartDate, applicabilityEndDate),
			};

			if (applicabilityStartDate != null)
			{
				rate.ZZ2_StartDate = applicabilityStartDate.Value;
			}

			if (applicabilityEndDate != null)
			{
				rate.ZZ2_EndDate = applicabilityEndDate.Value;
			}
			return rate;
		}

		static string GetRateTypeByCode(string code)
		{
			switch (code)
			{
				case Constants.Rates.Codes.COFINS:
					return Constants.Rates.Types.COFINS;
				case Constants.Rates.Codes.PIS:
					return Constants.Rates.Types.PIS;
				case Constants.Rates.Codes.Duty:
					return Constants.Rates.Types.Duty;
				case Constants.Rates.Codes.IPI:
					return Constants.Rates.Types.IPI;
				default:
					return string.Empty;
			}
		}

		static RefCusApplicability[] FeedRefCusApplicability(DateTime? applicabilityStartDate, DateTime? applicabilityEndDate)
		{
			var applicability = new RefCusApplicability
			{
				ZZT_AdditionalCode = string.Empty
			};

			if (applicabilityStartDate != null)
			{
				applicability.ZZT_StartDate = applicabilityStartDate.Value;
			}

			if (applicabilityEndDate != null)
			{
				applicability.ZZT_EndDate = applicabilityEndDate.Value;
			}

			return new RefCusApplicability[] { applicability };
		}

		public static readonly string NcmPattern = @"^(\d{4}\.\d{2}\.\d{2})$";
		public static readonly string AlternativeNcmPattern = @"^(\d{8})$";
	}
}
