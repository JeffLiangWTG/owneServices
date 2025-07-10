using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseMessageSendingObjectForTest : BaseMessageSendingObject
	{
		public BaseMessageSendingObjectForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;

		public BaseMessageSendingObjectValidationForTest Validation
		{
			get { return GetNewValidation(); }
		}
		protected virtual BaseMessageSendingObjectValidationForTest GetNewValidation()
		{
			return new BaseMessageSendingObjectValidationForTest(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}
	}
}
