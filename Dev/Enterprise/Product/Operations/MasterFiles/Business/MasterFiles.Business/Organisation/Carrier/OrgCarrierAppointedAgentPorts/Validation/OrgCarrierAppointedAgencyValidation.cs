using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgencyValidation : OrgCarrierAppointedAgentPortsValidation
	{
		public OrgCarrierAppointedAgencyValidation(OrgCarrierAppointedAgentPorts parent)
			: base(parent) { }

		protected override void CheckO5_PortOrCountry()
		{
			base.CheckO5_PortOrCountry();
			if (Parent.Header != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.O5_PortOrCountryInfo, Parent.Header.CarrierAppointedAgentPorts_Agency);
			}
		}

		protected override string DuplicateRelatedPartyErrorMessage => Res.GetString("1f9b8ad1-9f30-452f-b084-1d53f2e047bd", "{0} already has an Agency with this address.", Parent.O5_PortOrCountry);
	}
}
