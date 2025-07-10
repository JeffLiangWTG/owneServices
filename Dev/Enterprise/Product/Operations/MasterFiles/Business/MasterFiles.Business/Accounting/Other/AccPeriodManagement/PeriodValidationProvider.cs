using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class PeriodValidationProvider : ValidationProvider
	{
		public PeriodValidationProvider(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void CheckDateFallsIntoValidPeriod(ZPropertyInfo dateTimeInfo)
		{
			ZDateTime dateTime = (ZDateTime)dateTimeInfo.Value;

			if (dateTime.IsValid)
			{
				if (dateTime > ZDateTime.MaxSmallDateTime || dateTime < ZDateTime.MinSmallDateTimeValue)
				{
					dateTimeInfo.AddError(OutOfDateRangeError);
				}
				else
				{
					if (PeriodCalculator.IsPostDateValid(dateTime) == PostDateValidationResult.PeriodNotFound)
					{
						dateTimeInfo.AddError(InvalidDateError);
					}

					LedgerPeriodValidation(dateTimeInfo);
				}
			}
		}

		#region Implementation

		protected virtual void LedgerPeriodValidation(ZPropertyInfo dateTimeInfo)
		{
			ZDateTime dateTime = (ZDateTime)dateTimeInfo.Value;
			if (PeriodCalculator.IsPostDateValid(dateTime) == PostDateValidationResult.SubLedgerPeriodClosed)
			{
				dateTimeInfo.AddError(SubLedgerPeriodClosedError);
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;
		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		public ZString OutOfDateRangeError
		{
			get
			{
				return Res.GetString("41956652-84C3-4AB0-86D8-C22C74DDCBC9", "This date does not fall into a valid date range. ({0} to {1})", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
		}

		public ZString InvalidDateError
		{
			get
			{
				return Res.GetString("469E8ADE-A389-4C33-A52D-D18E9E03B486", "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.");
			}
		}

		public ZString SubLedgerPeriodClosedError
		{
			get
			{
				return Res.GetString("dd103620-7778-4baf-b734-9ce6ffdaa24b", "This date falls into a period where the sub-ledger is closed");
			}
		}

		public ZString GeneralLedgerPeriodClosedError
		{
			get
			{
				return Res.GetString("11cdac7f-cd53-4482-8ba2-94479d5cd4fa", "This date falls into a period where the general ledger is closed");
			}
		}

		#endregion
	}
}
