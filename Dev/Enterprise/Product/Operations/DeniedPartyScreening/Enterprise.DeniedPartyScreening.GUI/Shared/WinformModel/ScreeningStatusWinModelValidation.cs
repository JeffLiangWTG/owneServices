using System;
using CargoWise.EntityFramework;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class ScreeningStatusWinModelValidation : ZValidation
	{
		public ScreeningStatusWinModelValidation(ScreeningStatusWinModel parent) : base(parent)
		{
			ScreeningStatusWinModel = parent;
		}

		public override Type AutoValidationType => typeof(ScreeningStatusWinModel);

		public ScreeningStatusWinModel ScreeningStatusWinModel { get; }

		public override void ValidateAll()
		{
			ValidateClearingReasonText();
		}

		protected void CheckClearingReasonText()
		{
			var validationText = ScreeningStatusWinModel.ClearingReasonTextValidationText;

			if (!string.IsNullOrEmpty(validationText))
			{
				ScreeningStatusWinModel.ClearingReasonTextInfo.AddError(validationText);
			}
		}

		public void ValidateClearingReasonText()
		{
			ValidateCalculatedProperty(ScreeningStatusWinModel.ClearingReasonTextInfo);
		}
	}
}
