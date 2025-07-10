using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class DateValidator : ValidationProvider
	{
		public DateValidator(BusinessObject bizObject, ZPropertyInfo startDateInfo, ZPropertyInfo endDateInfo)
			: base(bizObject)
		{
			this.BizObject = bizObject;
			this.StartDateInfo = startDateInfo;
			this.EndDateInfo = endDateInfo;
		}

		#region Start Date

		readonly BusinessObject BizObject;

		public void ValidateStartDate()
		{
			if (((ZDate)StartDateInfo.Value).IsEmpty)
			{
				MandatoryValidation.CheckEntered(StartDateInfo);
			}
			if (BizObject is RatingHeader && (ZDate)StartDateInfo.Value < ZDate.Today)
			{
				StartDateInfo.AddWarning(ErrorMessages.StartDateInPast);
			}
			else if (BizObject is RateEntry)
			{
				if ((ZDate)StartDateInfo.Value <= ZDate.Today.AddDays(-30))
				{
					StartDateInfo.AddWarning(ErrorMessages.StartDateInPast);
				}
				else if ((ZDate)StartDateInfo.Value < ZDate.Today)
				{
					StartDateInfo.AddWarning(ErrorMessages.StartDateInPast);
				}
			}
			if ((ZDate)StartDateInfo.Value > (ZDate)EndDateInfo.Value)
			{
				StartDateInfo.AddError(ErrorMessages.StartDateAfterExpiryDate);
			}
		}

		#endregion

		#region End Date

		public void ValidateEndDate(bool allowEmptyDates)
		{
			if (!allowEmptyDates && ((ZDate)EndDateInfo.Value).IsEmpty)
			{
				MandatoryValidation.CheckEntered(EndDateInfo);
			}
			else if ((ZDate)EndDateInfo.Value < (ZDate)StartDateInfo.Value)
			{
				EndDateInfo.AddError(ErrorMessages.ExpiryBeforeStartDate);
			}
		}

		#endregion

		#region Implementation

		readonly ZPropertyInfo StartDateInfo;
		readonly ZPropertyInfo EndDateInfo;

		#endregion
	}
}

