using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaBillValidationForRegularBill
	{
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCargoReleaseStatus();
			ValdiateCargoReleaseStatusOtherDescription();
		}

		public void ValidateCargoReleaseStatus()
		{
			ValidateCalculatedProperty(Parent.CargoReleaseStatusInfo);
		}

		protected virtual void CheckCargoReleaseStatus()
		{
			if (Parent.IsCargoReleaseStatusVisible)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CargoReleaseStatusInfo);
			}
		}

		public void ValdiateCargoReleaseStatusOtherDescription()
		{
			ValidateCalculatedProperty(Parent.CargoReleaseStatusOtherDescriptionInfo);
		}

		protected virtual void CheckCargoReleaseStatusOtherDescription()
		{
			if (Parent.IsCargoReleaseStatusOtherDescriptionVisible && Parent.CargoReleaseStatusOtherDescription.IsEmpty)
			{
				Parent.CargoReleaseStatusOtherDescriptionInfo.AddMessageError(Res.GetString("89BFFFCD-2BCB-423D-87A8-82723858682F", "Release Status Description Required when Cargo Release Status is 'Other'."));
			}
		}
	}
}
