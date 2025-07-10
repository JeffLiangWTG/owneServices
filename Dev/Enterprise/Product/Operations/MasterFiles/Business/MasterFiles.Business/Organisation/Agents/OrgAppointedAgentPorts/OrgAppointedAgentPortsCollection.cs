using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAppointedAgentPortsDependentCollection : DependentBusinessObjectCollection<OrgAppointedAgentPorts, OrgHeader>
	{
		public OrgAppointedAgentPortsDependentCollection(OrgHeader parentHeader, ZString agentType)
			: base(parentHeader)
		{
			this.AgentType = agentType;
		}

		public readonly ZString AgentType;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, AgentType);
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgAppointedAgentPorts)child).O5_SeaAirCarrierOrForwarderType = AgentType;
		}
	}

	public class OrgAppointedAgentPortsCollection : ActiveBusinessObjectCollection<OrgAppointedAgentPorts>
	{
		public OrgAppointedAgentPortsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
