using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class JobDeclarationForTestSendingObject : JobDeclaration
	{
		public JobDeclarationForTestSendingObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Action<ZPropertyInfo> MockValidationMessage;

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		public new JobDeclarationValidationForTestSendingObject Validation
		{
			get { return GetNewValidation(); }
		}

		protected new JobDeclarationValidationForTestSendingObject GetNewValidation()
		{
			return new JobDeclarationValidationForTestSendingObject(this);
		}
	}
}
