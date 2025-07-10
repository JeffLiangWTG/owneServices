using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgForwarderAppointedAgentPortsValidation : OrgAppointedAgentPortsValidation
	{
		public OrgForwarderAppointedAgentPortsValidation(OrgAppointedAgentPorts parent)
			: base(parent) { }

		public new OrgAppointedAgentPorts Parent
		{
			get { return base.Parent; }
		}

		#region Validation Overrides

		protected override void CheckO5_AgentDirection()
		{
			base.CheckO5_AgentDirection();

			MandatoryValidation.CheckEntered(Parent.O5_AgentDirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O5_AgentDirectionInfo);
			ValidateO5_AirAgentStatus();
			ValidateO5_RailAgentStatus();
			ValidateO5_RoadAgentStatus();
			ValidateO5_SeaAgentStatus();
		}

		protected override void CheckO5_PortOrCountry()
		{
			base.CheckO5_PortOrCountry();

			CheckForDuplicatePorts();
			ValidateO5_AirAgentStatus();
			ValidateO5_RailAgentStatus();
			ValidateO5_RoadAgentStatus();
			ValidateO5_SeaAgentStatus();
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			Parent.O5_PortOrCountryInfo.RefreshBinding();
		}

		#region Agent Status Validation

		protected override void CheckO5_AirAgentStatus()
		{
			base.CheckO5_AirAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("c1fe5523-a2a8-438b-ba09-046b545476d5", "Enter a valid Agent Type for Air."), Parent.O5_AirAgentStatusInfo);
			ValidateAgentStatus(Parent.O5_AirAgentStatusInfo, OrgAppointedAgentPortsSchema.O5_AirAgentStatus, Res.GetString("194f7650-5bd6-4137-a9dd-0136dea94a92", "Air"));

			ValidateO5_PortOrCountry();
		}

		protected override void CheckO5_RailAgentStatus()
		{
			base.CheckO5_RailAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("10a70c42-24e0-469c-b4cb-ed3d5564e22e", "Enter a valid Agent Type for Rail."), Parent.O5_RailAgentStatusInfo);
			ValidateAgentStatus(Parent.O5_RailAgentStatusInfo, OrgAppointedAgentPortsSchema.O5_RailAgentStatus, Res.GetString("a375455b-926f-4c16-9fdf-104833f08ce7", "Rail"));

			ValidateO5_PortOrCountry();
		}

		protected override void CheckO5_RoadAgentStatus()
		{
			base.CheckO5_RoadAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("94e99c9b-6d74-483d-bac6-c89382c0f24e", "Enter a valid Agent Type for Road."), Parent.O5_RoadAgentStatusInfo);
			ValidateAgentStatus(Parent.O5_RoadAgentStatusInfo, OrgAppointedAgentPortsSchema.O5_RoadAgentStatus, Res.GetString("09ad1edf-896a-4e2d-81e3-fabd7638558f", "Road"));

			ValidateO5_PortOrCountry();
		}

		protected override void CheckO5_SeaAgentStatus()
		{
			base.CheckO5_SeaAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("d35b5fa6-98e2-41ee-b06c-7229dc71dbb9", "Enter a valid Agent Type for Sea."), Parent.O5_SeaAgentStatusInfo);
			ValidateAgentStatus(Parent.O5_SeaAgentStatusInfo, OrgAppointedAgentPortsSchema.O5_SeaAgentStatus, Res.GetString("c994acc9-a397-42d2-b032-ecc88ff355f7", "Sea"));

			ValidateO5_PortOrCountry();
		}

		void ValidateAgentStatus(ZPropertyInfo agentStatusInfo, SchemaStringColumn agentStatusColumn, string agentMode)
		{
			if (Parent.O5_PortOrCountry.IsEmpty && !agentStatusInfo.Value.IsEmpty && !Parent.O5_PortOrCountryInfo.HasErrors())
			{
				ValidateO5_PortOrCountry();
			}

			if (agentStatusInfo.Value.ToString() == AgentStatusList.Codes.Published)
			{
				if (IsDuplicatePort(GetPortsPublishedAgent(agentStatusColumn)))
				{
					ZString message = Res.GetString("880002c5-4a61-455a-aa94-dc07608e82f0", "A published {0} Agent already exists for {1}. You cannot enter more than 1 per Location and Direction.", agentMode, Parent.O5_PortOrCountry);
					agentStatusInfo.AddError(message);
				}
			}
		}

		OrgAppointedAgentPorts[] GetPortsPublishedAgent(SchemaStringColumn o5_AgentStatus)
		{
			var filter = new ZQuery();
			filter.AddToFilter(o5_AgentStatus, AgentStatusList.Codes.Published);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, Parent.O5_PortOrCountry);

			return Parent.Factory.Load<OrgAppointedAgentPorts>(filter);
		}

		#endregion

		#region Duplicate Ports

		void CheckForDuplicatePorts()
		{
			if (IsDuplicatePort(GetForwarderDuplicatePorts()))
			{
				ZString message = Res.GetString("bfb0ea0b-c7ec-4306-b2dc-be137dea297e", "This organization already has handling details for {0}. You cannot enter more than one per Location, Direction and Transport Mode.", Parent.O5_PortOrCountry);
				Parent.O5_PortOrCountryInfo.AddError(message);
			}
		}

		OrgAppointedAgentPorts[] GetForwarderDuplicatePorts()
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, OrgAppointedAgentPorts.Forwarder);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OH, Parent.O5_OH);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, Parent.O5_PortOrCountry);

			if (Parent.O5_AgentDirection != AgentDirectionList.Codes.Both)
			{
				var directionFilter = new ZQuery();
				directionFilter.AddToFilter(OrgAppointedAgentPortsSchema.O5_AgentDirection, AgentDirectionList.Codes.Both);
				directionFilter.AddToFilter(JoinCondition.Or, OrgAppointedAgentPortsSchema.O5_AgentDirection, Parent.O5_AgentDirection);

				filter.AddToFilter(directionFilter);
			}

			var modeFilter = new ZQuery();

			if (!Parent.O5_AirAgentStatus.IsEmpty)
			{
				modeFilter.AddToFilter(JoinCondition.Or, OrgAppointedAgentPortsSchema.O5_AirAgentStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			if (!Parent.O5_SeaAgentStatus.IsEmpty)
			{
				modeFilter.AddToFilter(JoinCondition.Or, OrgAppointedAgentPortsSchema.O5_SeaAgentStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			if (!Parent.O5_RailAgentStatus.IsEmpty)
			{
				modeFilter.AddToFilter(JoinCondition.Or, OrgAppointedAgentPortsSchema.O5_RailAgentStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			if (!Parent.O5_RoadAgentStatus.IsEmpty)
			{
				modeFilter.AddToFilter(JoinCondition.Or, OrgAppointedAgentPortsSchema.O5_RoadAgentStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			filter.AddToFilter(modeFilter);

			return Parent.Factory.Load<OrgAppointedAgentPorts>(filter);
		}

		#endregion

		#endregion
	}
}
