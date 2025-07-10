
namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAirCTOValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedAirCTOValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsAirCTO)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("a81c3fb8-05bc-4647-a926-4154dbb7e50c", "Selected Organization should be Air CTO/Depot"));
			}
		}

		protected override string DuplicateRelatedPartyErrorMessage => Res.GetString("21992953-48a2-40f9-8332-07070d445be4", "{0} already has an Air CTO with this address.", Parent.O5_PortOrCountry);

		protected override bool IsDirectionMandatory => true;
	}
}
