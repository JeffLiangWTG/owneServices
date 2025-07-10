using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class JobDeclarationValidationForTestSendingObject : ZValidation
	{
		public JobDeclarationValidationForTestSendingObject(JobDeclarationForTestSendingObject parent) : base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
		}

		#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(JobDeclarationValidationForTestSendingObject);
			}
		}
		public JobDeclarationForTestSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly JobDeclarationForTestSendingObject parent;
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
			ValidateJE_MessageType();
		}
		#endregion

		public void ValidateJE_MessageType()
		{
			zValidationInternals.Validate(Parent.JE_MessageTypeInfo, new RunValidationInvoker(this.MessageTypeValidationInvoker));
		}
		void MessageTypeValidationInvoker()
		{
			CheckJE_MessageType();
		}

		protected virtual void CheckJE_MessageType()
		{
			Parent.MockValidationMessage?.Invoke(Parent.JE_MessageTypeInfo);
		}
	}
}
