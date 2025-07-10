using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class USActionMethodApplicatorValidation : ZValidation
	{
		public USActionMethodApplicatorValidation(USOperationalActionMethodApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			zValidationInternals = this;
			parentListInternals = parent;
		}

		public void Add(USActionMethodApplicatorValidation validation)
		{
			zValidationInternals.Add(validation);
		}

		public void Remove(USActionMethodApplicatorValidation validation)
		{
			zValidationInternals.Remove(validation);
		}

		public override void ValidateAll()
		{
			var suspender = parentListInternals.SuspendListChanged();
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
			ValidateSendWithMessageErrors();
		}

		public void ValidateSendWithMessageErrors()
		{
			zValidationInternals.Validate(Parent.SendWithMessageErrorsInfo, new RunValidationInvoker(this.SendWithMessageErrorsValidationInvoker));
		}

		void SendWithMessageErrorsValidationInvoker()
		{
			CheckSendWithMessageErrors();
		}

		protected void CheckSendWithMessageErrors()
		{
			if (Parent.SendWithMessageErrors && !Parent.IsSendWithMessageErrorsAllowed)
			{
				Parent.SendWithMessageErrorsInfo.AddError(string.Format(SendWithMessageErrorsSecurity, Parent.MessageDescription));
			}
		}
		internal const string SendWithMessageErrorsSecurity = "You do not have the appropriate security rights to send {0} message with message errors.";

		public override Type AutoValidationType => typeof(USActionMethodApplicatorValidation);

		public USOperationalActionMethodApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => parent;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly USOperationalActionMethodApplicator parent;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		protected IValidationInternals zValidationInternals;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;
	}
}
