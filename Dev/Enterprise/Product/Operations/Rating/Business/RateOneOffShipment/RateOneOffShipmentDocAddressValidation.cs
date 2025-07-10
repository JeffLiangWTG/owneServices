using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	class RateOneOffShipmentDocAddressValidation : JobDocAddressValidation
	{
		public RateOneOffShipmentDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		#region Validation

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsActive)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("565afb8c-d1c8-45b4-aa48-eee15ed10083", "This Organization is not active."));
			}
		}

		#endregion
	}
}

