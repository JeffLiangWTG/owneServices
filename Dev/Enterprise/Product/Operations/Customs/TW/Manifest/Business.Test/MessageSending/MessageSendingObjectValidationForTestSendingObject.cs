using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class MessageSendingObjectValidationForTestSendingObject : ZValidation
	{
		public MessageSendingObjectValidationForTestSendingObject(MessageSendingObjectForTestSendingObject parent) : base(parent)
		{
			validationInternals = this;
		}
		readonly IValidationInternals validationInternals;

		public override Type AutoValidationType => typeof(MessageSendingObjectValidationForTestSendingObject);

		public MessageSendingObjectForTestSendingObject Parent => (MessageSendingObjectForTestSendingObject)ParentFilter;

		public override void ValidateAll() => ValidateShouldSend();

		public void ValidateShouldSend() => validationInternals.Validate(Parent.ShouldSendInfo, new RunValidationInvoker(this.ShouldSendValidationInvoker));

		void ShouldSendValidationInvoker() => CheckShouldSend();

		protected void CheckShouldSend() => Parent.MockValidationMessage?.Invoke(Parent.ShouldSendInfo);
	}
}
