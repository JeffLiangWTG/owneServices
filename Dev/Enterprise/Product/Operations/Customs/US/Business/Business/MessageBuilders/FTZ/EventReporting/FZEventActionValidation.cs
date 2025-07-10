using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZEventActionValidation : ZValidation
	{
		public FZEventActionValidation(FZEventAction parent)
			: base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}
		public void Add(FZEventActionValidation validation)
		{
			zValidationInternals.Add(validation);
		}
		public void Remove(FZEventActionValidation validation)
		{
			zValidationInternals.Remove(validation);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			IDisposable suspender = parentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}

		protected virtual void ValidateAllCore()
		{
			ValidateUS_ActionCode();
			ValidateUS_ReasonCode();
			ValidateUS_FTZContactName();
			ValidateUS_FTZContactPhone();
			ValidateUS_Reasons();
		}

		#endregion

		#region US_ActionCode

		public void ValidateUS_ActionCode()
		{
			zValidationInternals.Validate(Parent.US_ActionCodeInfo, new RunValidationInvoker(this.CheckUS_ActionCode));
		}

		protected void CheckUS_ActionCode()
		{
			if (Parent.EventType != FZEventType.Unconcur)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.US_ActionCodeInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ActionCodeInfo, "Action Code");

				if (!Parent.US_ActionCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_ActionCodeInfo, Parent.Lookups.ActionCodeList);

					if (Parent.EventType == FZEventType.Concur && Parent.IsAC_ActionCode &&
						Parent.EventHeader.DeliveryCode != FTZDeliveryCodeList.Codes.FinalManifestPortionReported)
					{
						Parent.US_ActionCodeInfo.AddMessageError(FinalDeliveryRequired);
					}
				}
			}
		}
		internal const string FinalDeliveryRequired = "Delivery Code should be 'Final'. Partial concurrences are not accepted via this method.";

		#endregion

		#region US_ReasonCode

		public void ValidateUS_ReasonCode()
		{
			zValidationInternals.Validate(Parent.US_ReasonCodeInfo, new RunValidationInvoker(CheckUS_ReasonCode));
		}

		protected void CheckUS_ReasonCode()
		{
			if (Parent.EventType == FZEventType.Unconcur)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.US_ReasonCodeInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReasonCodeInfo, "Reason Code");

				if (!Parent.US_ReasonCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_ReasonCodeInfo, Parent.Lookups.ReasonCodeList);
				}
			}
		}

		#endregion

		#region US_FTZContactName

		public void ValidateUS_FTZContactName()
		{
			zValidationInternals.Validate(Parent.US_FTZContactNameInfo, new RunValidationInvoker(CheckUS_FTZContactName));
		}

		void CheckUS_FTZContactName()
		{
			if (Parent.EventType == FZEventType.Unconcur)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FTZContactNameInfo, "Contact Name");
			}
		}

		#endregion

		#region US_FTZContactPhone

		public void ValidateUS_FTZContactPhone()
		{
			zValidationInternals.Validate(Parent.US_FTZContactPhoneInfo, new RunValidationInvoker(CheckUS_FTZContactPhone));
		}

		void CheckUS_FTZContactPhone()
		{
			if (Parent.EventType == FZEventType.Unconcur)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FTZContactPhoneInfo, "Phone Number");
			}
		}

		#endregion

		#region US_Reasons

		public void ValidateUS_Reasons()
		{
			zValidationInternals.Validate(Parent.US_ReasonsInfo, new RunValidationInvoker(CheckUS_Reasons));
		}

		void CheckUS_Reasons()
		{
			if (Parent.EventType == FZEventType.Unconcur)
			{
				switch (Parent.US_ReasonCode)
				{
					case FTZUnconcurrenceReasonCodeList.Codes._02:
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReasonsInfo, Parent.GetReasonsCaption().Caption);
						ITNumberValidator.ValidateITNumberFormat(Parent.US_ReasonsInfo, Parent.EventHeader.IsAir, true);
						break;
					case FTZUnconcurrenceReasonCodeList.Codes._03:
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReasonsInfo, Parent.GetReasonsCaption().Caption);
						break;
					case FTZUnconcurrenceReasonCodeList.Codes._04:
					case FTZUnconcurrenceReasonCodeList.Codes._05:
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReasonsInfo, Parent.GetReasonsCaption().Caption);
						EntryNumberValidator.ValidateFormatAndCheckDigit(Parent.US_ReasonsInfo, Parent.EventHeader.Branch, ZString.Empty, true);
						break;
				}
			}
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get
			{
				return typeof(FZEventActionValidation);
			}
		}
		public FZEventAction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parent; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly FZEventAction parent;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
