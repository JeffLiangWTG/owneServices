using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class MessageSendingObjectValidationForTest : ZValidation
	{
		public MessageSendingObjectValidationForTest(MessageSendingObjectForTesting parent) : base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
		}

		#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(MessageSendingObjectValidationForTest);
			}
		}

		public MessageSendingObjectForTesting Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly MessageSendingObjectForTesting parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;
		#endregion
		#region ValidateAll
		public override void ValidateAll()
		{
			ValidateAllCore();
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
			CheckShouldSend();
		}

		protected virtual void CheckShouldSend()
		{
			Parent.MockValidationMessage?.Invoke(Parent.ShouldSendInfo);
		}
	}
}
