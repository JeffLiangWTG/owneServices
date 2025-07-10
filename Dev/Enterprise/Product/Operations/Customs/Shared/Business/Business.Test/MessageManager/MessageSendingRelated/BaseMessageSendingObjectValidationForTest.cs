using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseMessageSendingObjectValidationForTest : ZValidation
	{
		public BaseMessageSendingObjectValidationForTest(BaseMessageSendingObjectForTest parent) : base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}

		#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(BaseMessageSendingObjectValidationForTest);
			}
		}
		public BaseMessageSendingObjectForTest Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly BaseMessageSendingObjectForTest parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;
		#endregion

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
			ValidateShouldSend();
		}
		#endregion

		public void ValidateShouldSend()
		{
			zValidationInternals.Validate(Parent.ShouldSendInfo, new RunValidationInvoker(this.ShouldSendValidationInvoker));
		}
		void ShouldSendValidationInvoker()
		{
			CheckShouldSendIsWesternEuropean();
			CheckShouldSend();
		}
		protected virtual void CheckShouldSendIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ShouldSendInfo);
		}
		protected virtual void CheckShouldSend()
		{
			Parent.MockValidationMessage?.Invoke(Parent.ShouldSendInfo);
		}
	}
}
