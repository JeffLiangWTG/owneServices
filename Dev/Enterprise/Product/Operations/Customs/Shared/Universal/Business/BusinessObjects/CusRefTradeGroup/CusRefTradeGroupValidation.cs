//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTradeGroupValidation
//
//    This class should be used for overriding validation in AutoCusRefTradeGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class CusRefTradeGroupValidation : AutoCusRefTradeGroupValidation
	{
		public CusRefTradeGroupValidation(AutoCusRefTradeGroup parent) : base(parent)
		{
		}

		new CusRefTradeGroup Parent => (CusRefTradeGroup)base.Parent;

		protected override void CheckCR9_RN_NKCountryCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CR9_RN_NKCountryCodeInfo);
		}

		protected override void CheckCR9_TradeGroup()
		{
			MandatoryValidation.CheckEntered(Parent.CR9_TradeGroupInfo);

			if (!Parent.TradeGroupCountries.Any())
			{
				Parent.CR9_TradeGroupInfo.AddError(Res.GetString("3AF70FCD-196C-4988-9E7F-1E4021614A96", "The trade group must have one trade group country at least."));
			}
		}

		protected override void CheckCR9_Description()
		{
			MandatoryValidation.CheckEntered(Parent.CR9_DescriptionInfo);
		}

		protected override void CheckCR9_EndDate()
		{
			if (Parent.CR9_EndDate.IsValid && Parent.CR9_EndDate < Parent.CR9_StartDate)
			{
				Parent.CR9_EndDateInfo.AddError(Res.GetString("EA05F2FF-A68B-48B1-B548-A1F6E4C731BD", "The end date of trade group must not be earlier than its start date."));
			}
		}

		protected override void CheckCR9_EndDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CR9_EndDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears,
				FutureYearsBeforeWarning = DateRangeValidation.MaximumFutureYears,
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = DateRangeValidation.MaximumPastYears
			});
		}

		protected override void CheckCR9_StartDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CR9_StartDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears,
				FutureYearsBeforeWarning = DateRangeValidation.MaximumFutureYears,
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = DateRangeValidation.MaximumPastYears
			});
		}
	}
}
