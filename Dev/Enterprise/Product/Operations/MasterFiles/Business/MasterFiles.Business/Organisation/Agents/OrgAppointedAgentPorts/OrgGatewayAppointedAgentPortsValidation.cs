using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgGatewayAppointedAgentPortsValidation : OrgAppointedAgentPortsValidation
	{
		public OrgGatewayAppointedAgentPortsValidation(OrgAppointedAgentPorts parent)
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
		}

		protected override void CheckO5_AirAgentStatus()
		{
			base.CheckO5_AirAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("7bcaff4e-9c25-4cef-83b1-5fba788d54c8", "Enter a valid Gateway Type for Air."), Parent.O5_AirAgentStatusInfo);
		}

		protected override void CheckO5_RailAgentStatus()
		{
			base.CheckO5_RailAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("42c9eddb-5096-429c-8621-8581921b2f4f", "Enter a valid Gateway Type for Rail."), Parent.O5_RailAgentStatusInfo);
		}

		protected override void CheckO5_RoadAgentStatus()
		{
			base.CheckO5_RoadAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("6512b77a-88db-4351-b593-eaa762c1e52f", "Enter a valid Gateway Type for Road."), Parent.O5_RoadAgentStatusInfo);
		}

		protected override void CheckO5_SeaAgentStatus()
		{
			base.CheckO5_SeaAgentStatus();

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("c6d89f4f-e1d7-4ec9-badc-407345783f4d", "Enter a valid Gateway Type for Sea."), Parent.O5_SeaAgentStatusInfo);
		}

		protected override void CheckO5_PortOrCountry()
		{
			base.CheckO5_PortOrCountry();
			CheckForDuplicatePorts();
		}

		#region Duplicate Ports

		void CheckForDuplicatePorts()
		{
			if (IsDuplicatePort(GetGatewayAgentDuplicatePorts()))
			{
				var message = Res.GetString("49586f40-403f-407f-9b4f-71cfb7cdc00a", "This organization already has Gateway Agent details for {0}. You cannot enter more than one Gateway Agent Office Address per Location.", Parent.O5_PortOrCountry);
				Parent.O5_PortOrCountryInfo.AddError(message);
			}
		}

		OrgAppointedAgentPorts[] GetGatewayAgentDuplicatePorts()
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType, OrgAppointedAgentPorts.GatewayAgent);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OH, Parent.O5_OH);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, Parent.O5_PortOrCountry);
			filter.AddToFilter(OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress, Parent.O5_OA_AgentOfficeAddress);

			return Parent.Factory.Load<OrgAppointedAgentPorts>(filter);
		}

		#endregion

		#endregion
	}
}
