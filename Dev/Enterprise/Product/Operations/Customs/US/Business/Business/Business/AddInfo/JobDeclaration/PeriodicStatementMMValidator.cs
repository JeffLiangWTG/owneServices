using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	class PeriodicStatementMMValidator
	{
		public void Validate(ZPropertyInfo monthFieldInfo, ZString paymentType, CodeDescriptionPairList list)
		{
			ZString value = (ZString)monthFieldInfo.Value;

			ListValidation.MessageErrorIfInvalidCode(monthFieldInfo, list);

			if (value.IsEmpty && PaymentTypeList.IsPeriodicPayment(paymentType))
			{
				monthFieldInfo.AddMessageError(MonthRequiredForPeriodicPaymentType);
			}
			else if (!value.IsEmpty && !paymentType.IsEmpty && !PaymentTypeList.IsPeriodicPayment(paymentType))
			{
				monthFieldInfo.AddMessageError(MonthNotRequiredForSelectedPaymentType);
			}
		}

		internal void ValidateAgainstCurrentDate(ZPropertyInfo monthFieldInfo)
		{
			ZString value = (ZString)monthFieldInfo.Value;
			if (!value.IsEmpty && value.IsNumbersOnlyOrEmpty)
			{
				if (!IsCurrentMonthOrNextTwo(value))
				{
					monthFieldInfo.AddMessageError(CurrentMonthOrNextTwo);
				}
				else if (ZInt.ParseSafe(value, 0) == ZDateTime.Today.Month && ZDateTime.Today > new AddInfoJobDeclarationWorkingDate().Get11thWorkingDayOfMonth(ZDate.Today))
				{
					monthFieldInfo.AddMessageError(CurrentMonthInvalid);
				}
			}
		}

		internal void ValidateAgainstReleaseDate(ZPropertyInfo monthFieldInfo, ZDateTime releaseDate)
		{
			if (releaseDate.IsValid)
			{
				ZString value = (ZString)monthFieldInfo.Value;
				if (!value.IsEmpty && value.IsNumbersOnlyOrEmpty)
				{
					if (!IsCurrentMonthSameOrNextMonthsFromFlagDate(value, 1, releaseDate))
					{
						monthFieldInfo.AddMessageError(CurrentMonthOrNextOneToRealeaseDate);
					}
					if (ZInt.ParseSafe(value, 0) == releaseDate.Month && ZDateTime.Today < new AddInfoJobDeclarationWorkingDate().Get11thWorkingDayOfMonth(releaseDate.Date))
					{
						monthFieldInfo.AddWarning(MonthTheSameAsReleaseDateMonth);
					}
				}
			}
		}

		internal const string MonthTheSameAsReleaseDateMonth = "Statement month is the same as the release month. A daily statement that has an entry on it in this condition must be authorized prior to the 11th working day of the statement month.";

		internal const string MonthRequiredForPeriodicPaymentType = "A month is required for the selected payment type.";
		internal const string MonthNotRequiredForSelectedPaymentType = "A month is not required for the selected payment type.";

		internal const string CurrentMonthOrNextTwo = "Periodic Statement Month must fall in the current month or subsequent two months.";
		internal const string CurrentMonthInvalid = "Periodic Statement Month can only be the current month if the Periodic Daily Statement Print Date is less than the Periodic Monthly Statement Print Date.";

		internal const string CurrentMonthOrNextOneToRealeaseDate = "Periodic Statement Month must be the month of release or the next month.";

		bool IsCurrentMonthOrNextTwo(ZString periodicStatementMonth)
		{
			bool pSMIsCurrentMonthOrNextTwo = false;
			int pSM = ZInt.ParseSafe(periodicStatementMonth, 0);
			int currentMonth = ZDateTime.Today.Month;

			if (pSM == 1)
			{
				if (currentMonth == 11 || currentMonth == 12 || currentMonth == 1)
				{
					pSMIsCurrentMonthOrNextTwo = true;
				}
			}
			else if (pSM == 2)
			{
				if (currentMonth == 12 || currentMonth == 1 || currentMonth == 2)
				{
					pSMIsCurrentMonthOrNextTwo = true;
				}
			}
			else if (pSM >= currentMonth && pSM <= currentMonth + 2)
			{
				pSMIsCurrentMonthOrNextTwo = true;
			}

			return pSMIsCurrentMonthOrNextTwo;
		}

		bool IsCurrentMonthSameOrNextMonthsFromFlagDate(ZString periodicStatementMonth, ZInt nextMonths, ZDateTime flagDate)
		{
			ZInt periodicStatementMonthNum = 0;
			if (ZInt.TryParse(periodicStatementMonth, out periodicStatementMonthNum))
			{
				return periodicStatementMonthNum == flagDate.Month || periodicStatementMonthNum == flagDate.AddMonths(nextMonths).Month;
			}
			return false;
		}
	}
}
