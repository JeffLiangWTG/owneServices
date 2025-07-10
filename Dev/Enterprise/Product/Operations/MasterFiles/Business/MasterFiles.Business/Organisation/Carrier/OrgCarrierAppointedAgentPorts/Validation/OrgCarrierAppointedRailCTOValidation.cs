
namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedRailCTOValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedRailCTOValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsRailHead)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("0a323b32-c060-49e5-bc45-a83cf5cd286a", "Selected Organization should be Rail Head/Depot"));
			}
		}

		protected override string DuplicateRelatedPartyErrorMessage => Res.GetString("173889f5-19fa-4eac-aefb-1e238ef04f05", "{0} already has a Rail CTO with this address.", Parent.O5_PortOrCountry);
	}
}
