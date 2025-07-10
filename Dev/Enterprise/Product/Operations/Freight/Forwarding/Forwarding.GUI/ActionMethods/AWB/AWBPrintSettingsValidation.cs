using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class AWBPrintSettingsValidation : ZValidation
	{
		public AWBPrintSettingsValidation(AWBPrintSettings parent)
			: base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
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
			ValidateOnErrorMessageError();
		}

		#endregion

		#region ValidateOnErrorMessageError

		public void ValidateOnErrorMessageError()
		{
			zValidationInternals.Validate(Parent.OnErrorMessageErrorInfo, new RunValidationInvoker(this.OnErrorMessageErrorValidationInvoker));
		}

		void OnErrorMessageErrorValidationInvoker()
		{
			CheckOnErrorMessageErrorIsWesternEuropean();
			CheckOnErrorMessageError();
		}

		protected virtual void CheckOnErrorMessageErrorIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.OnErrorMessageErrorInfo);
		}

		protected virtual void CheckOnErrorMessageError()
		{
			MandatoryValidation.CheckEntered(Parent.OnErrorMessageErrorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OnErrorMessageErrorInfo, Parent.OnErrorMessageError_List);
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(AWBPrintSettingsValidation); }
		}

		public AWBPrintSettings Parent
		{
			get { return parent; }
		}
		readonly AWBPrintSettings parent;

		readonly IValidationInternals zValidationInternals;
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
