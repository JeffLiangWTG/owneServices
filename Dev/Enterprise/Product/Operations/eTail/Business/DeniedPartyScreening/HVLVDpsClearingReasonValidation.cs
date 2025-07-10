using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsClearingReasonValidation : ZValidation
	{
		public HVLVDpsClearingReasonValidation(HVLVDpsClearingReason parent) : base(parent)
		{
			Parent = parent;
		}

		public override Type AutoValidationType => typeof(HVLVDpsClearingReason);

		public HVLVDpsClearingReason Parent { get; }

		public override void ValidateAll()
		{
			ValidateClearingReason();
			ValidateClearingReasonText();
		}

		public void ValidateClearingReason()
		{
			ValidateCalculatedProperty(Parent.ClearingReasonInfo);
		}

		protected void CheckClearingReason()
		{
			if (string.IsNullOrWhiteSpace(Parent.ClearingReason))
			{
				Parent.ClearingReasonInfo.AddError(RequireReasonForCLRItem.ClearingReasonValidationText);
			}
		}

		public void ValidateClearingReasonText()
		{
			ValidateCalculatedProperty(Parent.ClearingReasonTextInfo);
		}

		protected void CheckClearingReasonText()
		{
			var message = GetErrorMesage(Parent.ClearingReason, Parent.ClearingReasonText, RequireReasonForCLRItem.ClearingReasonValidationText);

			if (!string.IsNullOrEmpty(message))
			{
				Parent.ClearingReasonTextInfo.AddError(message);
			}
		}

		string GetErrorMesage(string clearingReason, string clearingReasonText, string missingClearingReasonErrorMessage)
		{
			if (!string.IsNullOrWhiteSpace(clearingReasonText))
			{
				return RequireReasonForCLRItem.GetErrorMessage(clearingReasonText);
			}

			if (string.IsNullOrWhiteSpace(clearingReason))
			{
				return missingClearingReasonErrorMessage;
			}
			
			return RequireReasonForCLRItem.ClearingReasonWordAndCharacterLengthValidationText;
		}
	}
}
