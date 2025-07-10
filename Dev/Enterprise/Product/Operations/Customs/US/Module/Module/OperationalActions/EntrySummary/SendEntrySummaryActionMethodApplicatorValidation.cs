using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendEntrySummaryActionMethodApplicatorValidation : USActionMethodApplicatorValidation
	{
		public SendEntrySummaryActionMethodApplicatorValidation(SendEntrySummaryActionMethodApplicator parent)
			: base(parent)
		{
		}

		public new SendEntrySummaryActionMethodApplicator Parent => (SendEntrySummaryActionMethodApplicator)base.Parent;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();

			ValidateContactName();
			ValidateContactPhone();
		}

		public void ValidateContactName()
		{
			zValidationInternals.Validate(Parent.ContactNameInfo, new RunValidationInvoker(this.CheckContactName));
		}

		public void ValidateContactPhone()
		{
			zValidationInternals.Validate(Parent.ContactPhoneInfo, new RunValidationInvoker(this.CheckContactPhone));
		}

		protected void CheckContactName()
		{
			if (Parent.ValidationMode == ValidationModes.EntrySummary)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ContactNameInfo);
			}
		}

		protected void CheckContactPhone()
		{
			if (Parent.ValidationMode == ValidationModes.EntrySummary)
			{
				if (!Parent.ContactPhone.IsEmpty)
				{
					if (!Parent.ContactPhone.IsNumbersOnlyOrEmpty)
					{
						Parent.ContactPhoneInfo.AddMessageError(InvalidContactPhoneMessage);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ContactPhoneInfo);
				}
			}
		}
		internal const string InvalidContactPhoneMessage = "Invalid Contact Phone Number format: should be only numerics.";
	}
}
