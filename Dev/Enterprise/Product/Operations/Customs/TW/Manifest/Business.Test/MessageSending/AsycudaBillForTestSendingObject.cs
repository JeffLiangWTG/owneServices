using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class AsycudaBillForTestSendingObject : AsycudaBill
	{
		public AsycudaBillForTestSendingObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new AsycudaBillValidationForTestSendingObject Validation => GetNewValidation();

		protected new AsycudaBillValidationForTestSendingObject GetNewValidation() => new AsycudaBillValidationForTestSendingObject(this);
	}
}
