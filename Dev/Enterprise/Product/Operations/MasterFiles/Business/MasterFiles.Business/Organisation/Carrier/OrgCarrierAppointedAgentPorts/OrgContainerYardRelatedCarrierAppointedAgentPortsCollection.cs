using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContainerYardRelatedCarrierAppointedAgentPortsCollection : ActiveBusinessObjectCollection<OrgCarrierAppointedAgentPorts>
	{
		public OrgContainerYardRelatedCarrierAppointedAgentPortsCollection(OrgHeader parent, ZString agentType)
			: base(parent.Factory)
		{
			this.parent = parent;
			this.agentType = agentType;
			AdditionalFilter = GetAdditionalFilterQuery();
		}
		readonly ZString agentType;
		readonly OrgHeader parent;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, agentType);
			return query;
		}

		ZQuery GetAdditionalFilterQuery()
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, parent.PK);

			var result = new ZDBOnlyQuery(typeof(OrgAppointedAgentPorts));
			result.AddSubQuery(OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress, addressSubQuery, JoinCondition.And);
			return result;
		}

		protected override bool AllowNew => false;
	}
}
