using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AccComplianceSequencePortugalValidation : AccComplianceSequenceValidation
	{
		static string DateRangeMustBeOneYearMinimum => Res.GetString("6936f6d0-0795-4b15-a38f-0d9355ebd291", @"The validity of this compliance book is less than one year.
Compliance books must be valid for at least one year to comply with Portugal tax regulations.");

		public AccComplianceSequencePortugalValidation(AutoAccComplianceSequence parent) : base(parent)
		{
		}

		protected override void CheckXD_StartDate()
		{
			base.CheckXD_StartDate();

			MandatoryValidation.CheckEntered(Parent.XD_StartDateInfo);

			if (!Parent.XD_StartDateInfo.HasErrors() && CheckValidComplianceSequenceDateRangeForCountryIsOneYearMinimum(Parent))
			{
				Parent.XD_StartDateInfo.AddError(DateRangeMustBeOneYearMinimum);
			}
		}

		protected override void CheckXD_ExpiryDate()
		{
			base.CheckXD_ExpiryDate();

			MandatoryValidation.CheckEntered(Parent.XD_ExpiryDateInfo);

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && CheckValidComplianceSequenceDateRangeForCountryIsOneYearMinimum(Parent))
			{
				Parent.XD_ExpiryDateInfo.AddError(DateRangeMustBeOneYearMinimum);
			}
		}

		static bool CheckValidComplianceSequenceDateRangeForCountryIsOneYearMinimum(AccComplianceSequence complianceSequence)
		{
			if (complianceSequence.XD_ExpiryDate.IsEmpty)
			{
				return false;
			}

			if (complianceSequence.XD_StartDate.IsEmpty)
			{
				return false;
			}

			return complianceSequence.XD_ExpiryDate < complianceSequence.XD_StartDate.AddYears(1).AddDays(-1);
		}
	}
}
