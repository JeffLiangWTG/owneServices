using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageSendingObjectValidation : ControllingMessageSendingObjectValidation
	{
		public LicensingMessageSendingObjectValidation(AutoControllingMessageSendingObject parent) : base(parent)
		{
		}

		public new LicensingMessageSendingObject Parent => base.Parent as LicensingMessageSendingObject;

		protected override void CheckShouldSendWhenTrue()
		{
			base.CheckShouldSendWhenTrue();
			var parent = Parent;
			var header = parent.Header;
			var targetInfo = parent.ShouldSendInfo;

			if (header.VATNumber.IsEmpty)
			{
				targetInfo.AddError(Res.GetString("801E5CB2-7A33-4021-BD0B-88F051160E2A", "The message cannot be sent without a valid declaration TW-VAT number. Go to Declarant > Organization > Details > Config > Registration to create a valid TW-VAT number."));
			}

			if (!(header.IsNX201_07 || header.CountOfLinkedInvoiceLines > 0))
			{
				targetInfo.AddError(Res.GetString("96AF8D93-F0AE-4806-BB7C-ADF2D9CC6C25", "There must be at least one Invoice Line linked to the Licensing Message."));
			}
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateAction();
		}

		public void ValidateAction()
		{
			ValidateCalculatedProperty(Parent.ActionInfo);
		}

		protected virtual void CheckAction()
		{
			var parent = Parent;
			if (parent.ShouldSend)
			{
				var targetInfo = parent.ActionInfo;
				if (parent.Action.IsEmpty)
				{
					targetInfo.AddError(Res.GetString("79B93ABF-1930-43F2-A662-9DAF650DD948", "Action is required if Send is ticked."));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(targetInfo);
				}
			}
		}

		#region ReasonDescription

		public void ValidateReasonDescription()
		{
			(this as IValidationInternals).Validate(Parent.ReasonDescriptionInfo, new RunValidationInvoker(ReasonDescriptionValidationInvoker));
		}

		void ReasonDescriptionValidationInvoker()
		{
			CheckReasonDescriptionIsWesternEuropean();
			CheckReasonDescription();
		}

		protected virtual void CheckReasonDescriptionIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ReasonDescriptionInfo);
		}

		protected virtual void CheckReasonDescription()
		{
		}

		#endregion
	}
}
