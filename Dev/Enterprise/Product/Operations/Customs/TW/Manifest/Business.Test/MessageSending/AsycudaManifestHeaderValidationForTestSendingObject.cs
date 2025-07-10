using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderValidationForTestSendingObject : ZValidation
	{
		public AsycudaManifestHeaderValidationForTestSendingObject(AsycudaManifestHeaderForTestSendingObject parent) : base(parent)
		{
			validationInternals = this;
		}
		readonly IValidationInternals validationInternals;

		public override Type AutoValidationType => typeof(AsycudaManifestHeaderValidationForTestSendingObject);

		public AsycudaManifestHeaderForTestSendingObject Parent => (AsycudaManifestHeaderForTestSendingObject)ParentFilter;

		public override void ValidateAll() => ValidateAMA_TransportMode();

		public void ValidateAMA_TransportMode() => validationInternals.Validate(Parent.AMA_TransportModeInfo, new RunValidationInvoker(AMA_TransportModeValidationInvoker));

		void AMA_TransportModeValidationInvoker() => CheckAMA_TransportMode();

		protected void CheckAMA_TransportMode() => Parent.MockValidationMessage?.Invoke(Parent.AMA_TransportModeInfo);
	}
}
