
namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedRoadCTOValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedRoadCTOValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsRoadFreightDepot)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("7873d131-dd5e-4c19-a2ec-c4b25089dcce", "Selected Organization should be Road Depot/Transit Shed"));
			}
		}

		protected override string DuplicateRelatedPartyErrorMessage => Res.GetString("7eed589e-da18-4b19-a02b-3f3b51e8b88c", "{0} already has a Road CTO with this address.", Parent.O5_PortOrCountry);
	}
}
