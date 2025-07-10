namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgentPortsUNLOCOLookups : OrgCarrierAppointedAgentPortsLookups
	{
		public OrgCarrierAppointedAgentPortsUNLOCOLookups(OrgCarrierAppointedAgentPorts parent)
			: base(parent)
		{
		}

		public new RefUNLOCOCollection Locations
		{
			get { return locations ?? (locations = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection locations;
	}
}
