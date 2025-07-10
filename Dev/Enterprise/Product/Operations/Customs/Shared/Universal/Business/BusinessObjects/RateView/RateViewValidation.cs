//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateViewValidation
//
//    This class should be used for overriding validation in AutoRateViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class RateViewValidation : AutoRateViewValidation
	{
		public RateViewValidation(AutoRateView parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			if (!Parent.ZZ2_IsSystem)
			{
				base.ValidateAll();
				ValidateAtLeastOneApplicabilityExists();
			}
		}

		protected new RateView Parent => (RateView)base.Parent;

		protected override void CheckZZ2_StartDate()
		{
			base.CheckZZ2_StartDate();

			var parent = Parent;
			var startDate = parent.ZZ2_StartDate;
			var propertyInfo = parent.ZZ2_StartDateInfo;
			if (startDate > parent.ZZ2_EndDate)
			{
				propertyInfo.AddError(Res.GetString("0DE67AA0-9176-40A7-82F1-749111D550C0", "Start Date cannot be later than End Date."));
			}

			var startDateOfTariff = parent.CusTariff?.ZZ1_StartDate;
			if (startDate < startDateOfTariff)
			{
				propertyInfo.AddError(Res.GetString("9EA1DCF1-4371-4D6C-9657-515EEE1742F9", "Start Date cannot be earlier than Tariff's Effective From date."));
			}
		}

		protected override void CheckZZ2_StartDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckZZ2_EndDate()
		{
			base.CheckZZ2_EndDate();

			var parent = Parent;
			var endDate = parent.ZZ2_EndDate;
			var propertyInfo = parent.ZZ2_EndDateInfo;
			if (endDate < parent.ZZ2_StartDate)
			{
				propertyInfo.AddError(Res.GetString("45B00C51-DD80-4368-9469-15F7EC520E12", "End Date must be later than Start Date."));
			}

			var endDateOfTariff = parent.CusTariff?.ZZ1_EndDate;
			if (endDate > endDateOfTariff)
			{
				propertyInfo.AddError(Res.GetString("9801C562-D0EB-4581-B8F1-44C19345F7E8", "End Date cannot be later than Tariff's Effective To date."));
			}
		}

		protected override void CheckZZ2_EndDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckZZ2_ZY1_RateCode()
		{
			base.CheckZZ2_ZY1_RateCode();

			var parent = Parent;
			var propertyInfo = parent.ZZ2_ZY1_RateCodeInfo;
			MandatoryValidation.CheckEntered(propertyInfo);

			var rateCode = parent.ZZ2_ZY1_RateCode;
			if (!rateCode.IsEmpty)
			{
				var cusTariff = parent.CusTariff;
				if (cusTariff != null)
				{
					var startDate = parent.ZZ2_StartDate;
					var endDate = parent.ZZ2_EndDate;
					var preference = parent.ZZ2_ZZS_Preference;
					var parentPK = parent.PK;
					var ratesWithSamePreferenceAndRateCode = cusTariff.Rates.Where(x => x.ZZ2_ZZS_Preference == preference && x.ZZ2_ZY1_RateCode == rateCode && x.PK != parentPK).ToArray();
					if (ratesWithSamePreferenceAndRateCode.Length > 0)
					{
						if (ratesWithSamePreferenceAndRateCode.Any(x => x.ZZ2_StartDate == startDate))
						{
							propertyInfo.AddError(Res.GetString("B22912BE-807F-4133-979B-30E9CD5BCC20", "The Rate with same Start Date, Preference and Rate Code already exists."));
						}
						else if (ratesWithSamePreferenceAndRateCode.Any(x => (startDate < x.ZZ2_StartDate && endDate >= x.ZZ2_StartDate) || (startDate <= x.ZZ2_EndDate && endDate >= x.ZZ2_EndDate)))
						{
							propertyInfo.AddError(Res.GetString("1627B6ED-E33F-470E-8E65-8902D717058B", "The date range of this Rate overlaps with another Rate with same Preference and Rate Code."));
						}
					}
				}
			}
		}

		protected override void CheckZZ2_ZZZ_NKDataGroupingIsNotEmpty()
		{
		}

		protected override void CheckZZ2_RX_NKCurrencyOverrideIsNotEmpty()
		{
		}

		protected override void CheckZZ2_RateFormula()
		{
			var parent = Parent;
			if (!parent.ZZ2_RateFormula.IsEmpty)
			{
				var calculator = new UniversalRateCalculator(parent.ZZ2_RateFormula, parent);
				if (calculator.Errors.Any())
				{
					parent.ZZ2_RateFormulaInfo.AddError(Res.GetString("E3D856DF-0406-4643-968E-0CF37C6A50F9", "The Formula is not valid due to the error(s): {0}.", string.Join("\r\n", calculator.Errors.Select(x => x.ErrorMessage))));
				}
			}
		}

		protected override void CheckZZ2_ZZS_Preference()
		{
			var parent = Parent;

			if (parent.ZZ2_ZZS_Preference.IsEmpty &&
				parent.ZZ2_ZZR_RateTypeCode == Constants.RateTypes.Duty)
			{
				parent.ZZ2_ZZS_PreferenceInfo.AddError(Res.GetString("BC7CBCBF-09A9-4789-8607-BD3244F206DC", "The preference should not be empty when Rate Type is Duty(DTY)."));
			}
		}

		protected void ValidateAtLeastOneApplicabilityExists()
		{
			var parent = Parent;
			if (parent.CusTariff != null && parent.FilteredRateApplicabilities.Count == 0)
			{
				parent.AddRowError(Res.GetString("DDD57328-BA9B-496F-BAF2-A56BE7C5F230", "At least one applicability is needed for this rate."));
			}
		}
	}
}
