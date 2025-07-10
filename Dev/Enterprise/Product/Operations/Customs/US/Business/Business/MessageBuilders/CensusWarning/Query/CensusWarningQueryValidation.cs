using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CensusWarningQueryValidation : AutoCensusWarningQueryValidation
	{
		public CensusWarningQueryValidation(AutoCensusWarningQuery parent)
			: base(parent)
		{
		}

		public new CensusWarningQuery Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CensusWarningQuery)base.Parent; }
		}

		protected override void CheckEntryFilerCode()
		{
			base.CheckEntryFilerCode();

			MandatoryValidation.CheckEntered(Parent.EntryFilerCodeInfo, "Entry Filer");
		}

		protected override void CheckEntryNumber()
		{
			base.CheckEntryNumber();

			if (Parent.EntryNumber.IsEmpty && Parent.DateFrom.IsEmpty && Parent.DateTo.IsEmpty && Parent.DistrictPortCode.IsEmpty)
			{
				Parent.EntryNumberInfo.AddError(DataIsMandatory);
			}
			ValidateDateFrom();
			ValidateDateTo();
			ValidateDistrictPortCode();
		}
		internal const string DataIsMandatory = "You should enter at least one of the three query criteria.";

		protected override void CheckDateFrom()
		{
			base.CheckDateFrom();

			if (!Parent.DateFrom.IsEmpty)
			{
				if (!Parent.EntryNumber.IsEmpty)
				{
					Parent.DateFromInfo.AddError(SomeDataIsRedundant);
				}

				if (Parent.DateFrom > Parent.DateTo)
				{
					Parent.DateFromInfo.AddError(DateFromCannotBeGreaterThanDateTo);
				}

				if (Parent.DateFrom > ZDateTime.Today)
				{
					Parent.DateFromInfo.AddWarning(DateCannotBeInFuture);
				}
			}
			else if (Parent.DistrictPortCode.IsEmpty && Parent.EntryNumber.IsEmpty)
			{
				Parent.DateFromInfo.AddError(DataIsMandatory);
			}
			ValidateEntryNumber();
			ValidateDistrictPortCode();
		}
		internal const string DateFromCannotBeGreaterThanDateTo = "Date From cannot be greater than Date To.";
		internal const string DateCannotBeInFuture = "You have entered a future date. This date is based on the date entry summary is initially accepted in ACE.";
		internal const string SomeDataIsRedundant = "You have entered this criterion as well as entry number. You do not need to enter other criteria if you have specified entry number.";

		protected override void CheckDateTo()
		{
			base.CheckDateTo();
			if (!Parent.DateTo.IsEmpty)
			{
				if (Parent.DateTo > ZDateTime.Today)
				{
					Parent.DateToInfo.AddWarning(DateCannotBeInFuture);
				}

				if (Parent.DateFrom.IsEmpty)
				{
					Parent.DateToInfo.AddWarning(DateToWillBeIgnored);
				}
				else if (Parent.DateTo > Parent.DateFrom.AddDays(30))
				{
					Parent.DateToInfo.AddMessageError(DateRangeExceeded);
				}
			}
			else if (!Parent.DateFrom.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.DateToInfo, "Date To");
			}

			ValidateDateFrom();
			ValidateEntryNumber();
			ValidateDistrictPortCode();
		}
		internal const string DateRangeExceeded = "The date range cannot exceed 30 days.";
		internal const string DateToWillBeIgnored = "'Date To' will be ignored in query if 'Date From' is empty.";

		protected override void CheckDistrictPortCode()
		{
			base.CheckDistrictPortCode();

			if (Parent.DistrictPortCode.IsEmpty && Parent.EntryNumber.IsEmpty && Parent.DateFrom.IsEmpty && Parent.DateTo.IsEmpty)
			{
				Parent.DistrictPortCodeInfo.AddError(DataIsMandatory);
			}

			if (!Parent.DistrictPortCode.IsEmpty && !Parent.EntryNumber.IsEmpty)
			{
				Parent.DistrictPortCodeInfo.AddError(SomeDataIsRedundant);
			}

			ValidateEntryNumber();
			ValidateDateFrom();
			ValidateDateTo();
		}
	}
}
