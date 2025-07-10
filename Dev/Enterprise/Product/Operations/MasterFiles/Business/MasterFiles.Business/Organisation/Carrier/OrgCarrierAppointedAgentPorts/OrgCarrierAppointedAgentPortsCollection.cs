using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgentPortsDependentCollection : DependentBusinessObjectCollection<OrgCarrierAppointedAgentPorts, OrgHeader>
	{
		public OrgCarrierAppointedAgentPortsDependentCollection(OrgHeader parentHeader, ZString agentType) : base(parentHeader)
		{
			this.AgentType = agentType;
		}

		public readonly ZString AgentType;

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, AgentType);
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			((OrgCarrierAppointedAgentPorts)child).O5_SeaAirCarrierOrForwarderType = AgentType;
		}

		#endregion

		#region FindAddress

		public OrgAddress FindAddress(ZString port)
		{
			return FindAddress(port, "", "", "");
		}

		public OrgAddress FindAddress(ZString port, ZString terminalType)
		{
			return FindAddress(port, terminalType, "", "");
		}

		public OrgAddress FindAddress(ZString port, ZString terminalType, ZString direction)
		{
			return FindAddress(port, terminalType, "", direction);
		}

		public OrgAddress FindContainerYardAddress(ZString port, RefContainer container)
		{
			return FindAddress(port, "", container != null ? container.RC_StorageClass : ZString.Empty, "");
		}

		OrgAddress FindAddress(ZString port, ZString terminalType, ZString containerClass, ZString direction)
		{
			if (port.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, AgentType);
			if (!terminalType.IsEmpty)
			{
				query.AddToFilter(OrgAppointedAgentPortsSchema.O5_TerminalType, terminalType);
			}
			var possibleItems = Find(query)
				.Cast<OrgCarrierAppointedAgentPorts>()
				.Where(item => containerClass.IsEmpty || item.ContainerTypes.ContainsContainerClass(containerClass));

			var ranker = new ColumnValueRanker();
			ranker.Add(OrgAppointedAgentPortsSchema.O5_PortOrCountry, port, port.Left(2));
			if (!direction.IsEmpty)
			{
				ranker.Add(OrgAppointedAgentPortsSchema.O5_AgentDirection, direction, (ZString)OrgConstants.CarrierAgentDirections.Code.Both);
			}

			return ranker.GetBestMatch<OrgAppointedAgentPorts>(possibleItems)?.FirstOrDefault()?.AgentOfficeAddress;
		}

		#endregion
	}
}
