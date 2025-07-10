
namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedCYValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedCYValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsContainerYard)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("ff5c3d7e-9acc-4cab-80ea-ab594ad7b98c", "Selected Organization should be a container yard."));
			}
		}

		protected override string DuplicateRelatedPartyErrorMessage => Res.GetString("20aa52b3-2848-476b-bca0-ea0218fa81d4", "{0} already has a Container Yard/Park with this address.", Parent.O5_PortOrCountry);

		protected override bool IsDirectionMandatory => true;
	}
}
