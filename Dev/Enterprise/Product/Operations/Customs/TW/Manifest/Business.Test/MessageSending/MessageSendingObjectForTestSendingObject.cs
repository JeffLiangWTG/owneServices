using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class MessageSendingObjectForTestSendingObject : MessageSendingObject
	{
		public MessageSendingObjectForTestSendingObject(AsycudaBill bill) : base(bill)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;
		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public MessageSendingObjectValidationForTestSendingObject Validation => GetNewValidation();

		protected MessageSendingObjectValidationForTestSendingObject GetNewValidation() => new MessageSendingObjectValidationForTestSendingObject(this);
	}
}
