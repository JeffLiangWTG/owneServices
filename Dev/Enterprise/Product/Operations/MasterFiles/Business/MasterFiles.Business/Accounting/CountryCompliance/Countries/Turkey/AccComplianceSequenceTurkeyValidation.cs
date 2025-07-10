using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AccComplianceSequenceTurkeyValidation : AccComplianceSequenceValidation
	{
		static string ValidFromAndExpiryDateMustBeInTheSameCalendaryear => Res.GetString("D6195219-9C5C-487D-AFAF-59FE4B5B3149", "Valid From and Expiry Date must be in the same calendar year.");

		public AccComplianceSequenceTurkeyValidation(AutoAccComplianceSequence parent) : base(parent)
		{
		}

		protected override void CheckXD_StartDate()
		{
			base.CheckXD_StartDate();

			if (!Parent.XD_StartDateInfo.HasErrors() && !CheckStartAndExpiryWithinSameCalendarYear())
			{
				Parent.XD_StartDateInfo.AddError(ValidFromAndExpiryDateMustBeInTheSameCalendaryear);
			}
		}

		protected override void CheckXD_ExpiryDate()
		{
			base.CheckXD_ExpiryDate();

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && !CheckStartAndExpiryWithinSameCalendarYear())
			{
				Parent.XD_ExpiryDateInfo.AddError(ValidFromAndExpiryDateMustBeInTheSameCalendaryear);
			}
		}

		bool CheckStartAndExpiryWithinSameCalendarYear() => Parent.XD_StartDate.Year == Parent.XD_ExpiryDate.Year;
	}
}
