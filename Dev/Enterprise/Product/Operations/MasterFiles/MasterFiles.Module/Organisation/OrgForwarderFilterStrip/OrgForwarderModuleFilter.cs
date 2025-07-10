using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class OrgForwarderModuleFilter : ModuleTextFilter
	{
		#region Construction

		public OrgForwarderModuleFilter(ZString description)
			: base(description, DummyQuery, ModeList)
		{
			this.SubGroup = new ForwarderModuleSubGroup();
		}

		#endregion

		#region Status

		ZString status;

		[List("Statuses")]
		public ZString Status
		{
			get { return status; }
			set
			{
				status = value;
				StatusInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		public CodeDescriptionPairList Statuses
		{
			get { return GetAgentStatuses(); }
		}

		CodeDescriptionPairList GetAgentStatuses()
		{
			var agentStatuses = new CodeDescriptionPairList();
			agentStatuses.AddPair(AgentStatusList.Codes.Appointed, AgentStatusList.Descriptions.Appointed);
			agentStatuses.AddPair(AgentStatusList.Codes.Handles, AgentStatusList.Descriptions.Handles);
			agentStatuses.AddPair(AgentStatusList.Codes.Published, AgentStatusList.Descriptions.Published);

			return agentStatuses;
		}

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			Status = "";
		}

		#endregion

		#region Modes

		[List("Modes")]
		public override ZString Property
		{
			get { return base.Property; }
			set { base.Property = value; }
		}

		public CodeDescriptionPairList Modes
		{
			get { return ModeList; }
		}

		static CodeDescriptionPairList ModeList
		{
			get { return GetTransportModes(); }
		}

		static CodeDescriptionPairList GetTransportModes()
		{
			var transportModes = new CodeDescriptionPairList();
			transportModes.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
			transportModes.AddPair(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail);
			transportModes.AddPair(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road);
			transportModes.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);

			return transportModes;
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => base.IsEmptyCore && Status.IsEmpty;

		#endregion

		#region Query

		class ForwarderModuleSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsForwarder, "Y");
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		static ZQuery DummyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			ZQuery query = new ZQuery();

			if (!IsEmpty)
			{
				query = GetForwarderAgentStatusFilter();
			}

			return query;
		}

		protected ZQuery GetForwarderAgentStatusFilter()
		{
			var subQuery = new ZQuery();
			if (Property != "")
			{
				switch (Property)
				{
					case Core.Constants.TransportModes.Air:
						subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_AirAgentStatus));
						break;

					case Core.Constants.TransportModes.Sea:
						subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_SeaAgentStatus));
						break;

					case Core.Constants.TransportModes.Road:
						subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_RoadAgentStatus));
						break;

					case Core.Constants.TransportModes.Rail:
						subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_RailAgentStatus));
						break;

					default:
						break;
				}
			}
			else
			{
				subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_AirAgentStatus), JoinCondition.Or);
				subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_SeaAgentStatus), JoinCondition.Or);
				subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_RoadAgentStatus), JoinCondition.Or);
				subQuery.AddToFilter(AgentStatusSubQuery(OrgAppointedAgentPortsSchema.O5_RailAgentStatus), JoinCondition.Or);
			}
			return subQuery;
		}

		ZQuery AgentStatusSubQuery(SchemaColumn statusType)
		{
			ZQuery query = new ZQuery();
			switch (Status)
			{
				case AgentStatusList.Codes.Handles:
					query.AddToFilter(statusType, AgentStatusList.Codes.Handles);
					break;

				case AgentStatusList.Codes.Appointed:
					query.AddToFilter(statusType, AgentStatusList.Codes.Appointed);
					break;

				case AgentStatusList.Codes.Published:
					query.AddToFilter(statusType, AgentStatusList.Codes.Published);
					break;

				default:
					query.AddToFilter(JoinCondition.Or, statusType, AgentStatusList.Codes.Handles);
					query.AddToFilter(JoinCondition.Or, statusType, AgentStatusList.Codes.Appointed);
					query.AddToFilter(JoinCondition.Or, statusType, AgentStatusList.Codes.Published);
					break;
			}
			return query;
		}

		#endregion
	}
}
