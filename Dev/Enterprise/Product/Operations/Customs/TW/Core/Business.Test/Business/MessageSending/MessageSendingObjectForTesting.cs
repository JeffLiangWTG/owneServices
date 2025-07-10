using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class MessageSendingObjectForTesting : MessageSendingObject
	{
		public MessageSendingObjectForTesting(CusEntryHeader header) : base(header)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;
		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		public new MessageSendingObjectValidationForTest Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		protected new MessageSendingObjectValidationForTest GetNewValidation()
		{
			return new MessageSendingObjectValidationForTest(this);
		}

		public override ZString GetMessageOwner() => ZString.Empty;
	}
}
