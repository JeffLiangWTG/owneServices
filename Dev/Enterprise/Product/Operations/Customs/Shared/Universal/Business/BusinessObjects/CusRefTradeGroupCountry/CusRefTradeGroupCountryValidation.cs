//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTradeGroupCountryValidation
//
//    This class should be used for overriding validation in AutoCusRefTradeGroupCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Customs.Universal
{
	using CargoWise.EntityFramework;

	public class CusRefTradeGroupCountryValidation : AutoCusRefTradeGroupCountryValidation
	{
		public CusRefTradeGroupCountryValidation(AutoCusRefTradeGroupCountry parent) : base(parent)
		{
		}

		new CusRefTradeGroupCountry Parent => (CusRefTradeGroupCountry)base.Parent;

		protected override void CheckCRA_StartDate()
		{
			var parent = Parent;
			var startDateInfo = parent.CRA_StartDateInfo;
			var startDate = parent.CRA_StartDate;
			var endDate = parent.CRA_EndDate;
			var tradeGroup = parent.TradeGroup;
			if ((endDate.IsValid && startDate > endDate) || (tradeGroup != null && startDate < tradeGroup.CR9_StartDate))
			{
				startDateInfo.AddError(Res.GetString("7066FE19-4814-47F1-972D-3DC644EABC17", "The start date of the trade group country must not be earlier than trade group start date and later than its end date."));
			}
			else if (tradeGroup != null && ExistDuplicateOrDateOverlapTradeGroupCountry(tradeGroup.TradeGroupCountries, parent))
			{
				startDateInfo.AddError(DuplicateTradeGroupCountryError);
			}
		}

		protected override void CheckCRA_StartDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CRA_StartDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears,
				FutureYearsBeforeWarning = DateRangeValidation.MaximumFutureYears,
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = DateRangeValidation.MaximumPastYears
			});
		}

		protected override void CheckCRA_EndDate()
		{
			var parent = Parent;
			var endDateInfo = parent.CRA_EndDateInfo;
			var endDate = parent.CRA_EndDate;
			var tradeGroup = parent.TradeGroup;
			if (endDate < parent.CRA_StartDate || (tradeGroup != null && endDate > tradeGroup.CR9_EndDate))
			{
				endDateInfo.AddError(Res.GetString("CC90144E-23C6-4DB3-8EFD-E3C0342B6C2F", "The end date of the trade group country must not be later than trade group end date and earlier than its start date."));
			}
			else if (tradeGroup != null && ExistDuplicateOrDateOverlapTradeGroupCountry(tradeGroup.TradeGroupCountries, parent))
			{
				endDateInfo.AddError(DuplicateTradeGroupCountryError);
			}
		}

		protected override void CheckCRA_EndDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CRA_EndDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears,
				FutureYearsBeforeWarning = DateRangeValidation.MaximumFutureYears,
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = DateRangeValidation.MaximumPastYears
			});
		}

		protected override void CheckCRA_RN_NKTradeGroupCountryCode()
		{
			var parent = Parent;
			var tradeGroupCountryCodeInfo = parent.CRA_RN_NKTradeGroupCountryCodeInfo;
			MandatoryValidation.CheckEntered(tradeGroupCountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(tradeGroupCountryCodeInfo);

			var tradeGroup = parent.TradeGroup;
			if (tradeGroup != null && ExistDuplicateOrDateOverlapTradeGroupCountry(tradeGroup.TradeGroupCountries, parent))
			{
				tradeGroupCountryCodeInfo.AddError(DuplicateTradeGroupCountryError);
			}
		}

		static string DuplicateTradeGroupCountryError => Res.GetString("EA0B8B05-924B-49B2-A7AC-D2C27F549FF0", "The trade group country should be unique in the trade group within date range.");

		protected bool ExistDuplicateOrDateOverlapTradeGroupCountry(CusRefTradeGroupCountryCollection tradeGroupCountryCollection, CusRefTradeGroupCountry tradeGroupCountry)
		{
			var result = false;
			if (tradeGroupCountryCollection.Count > 0)
			{
				var tradeGroupCountryCode = tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode;
				var startDate = tradeGroupCountry.CRA_StartDate;
				var endDate = tradeGroupCountry.CRA_EndDate;
				if (!tradeGroupCountryCode.IsEmpty && !startDate.IsEmpty && !endDate.IsEmpty)
				{
					result = tradeGroupCountryCollection.Any(x => x.CRA_RN_NKTradeGroupCountryCode == tradeGroupCountryCode && x.PK != tradeGroupCountry.PK
						&& (startDate <= x.CRA_StartDate && endDate >= x.CRA_StartDate || startDate <= x.CRA_EndDate && endDate >= x.CRA_EndDate));
				}
			}
			return result;
		}
	}
}
