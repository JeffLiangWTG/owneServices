using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class AsycudaBillValidationForTestSendingObject : ZValidation
	{
		public AsycudaBillValidationForTestSendingObject(AsycudaBillForTestSendingObject parent) : base(parent)
		{
			validationInternals = this;
		}
		readonly IValidationInternals validationInternals;

		public override Type AutoValidationType => typeof(AsycudaBillValidationForTestSendingObject);

		public AsycudaBillForTestSendingObject Parent => (AsycudaBillForTestSendingObject)ParentFilter;

		public override void ValidateAll() => ValidateABL_UCRNumber();

		public void ValidateABL_UCRNumber() => validationInternals.Validate(Parent.ABL_UCRNumberInfo, new RunValidationInvoker(ABL_UCRNumberInfoValidationInvoker));

		void ABL_UCRNumberInfoValidationInvoker() => CheckABL_UCRNumber();

		protected void CheckABL_UCRNumber() => Parent.MockValidationMessage?.Invoke(Parent.ABL_UCRNumberInfo);
	}
}
