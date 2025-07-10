using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgAppointedAgentPorts : AutoOrgAppointedAgentPorts
	{
		public const string Forwarder = "FWD";
		public const string GatewayAgent = "GTW";

		public OrgAppointedAgentPorts(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoOrgAppointedAgentPorts.Schema
		{
			public const string O5_IsHandlesAirAgent = "O5_IsHandlesAirAgent";
			public const string O5_IsHandlesSeaAgent = "O5_IsHandlesSeaAgent";
			public const string O5_IsHandlesRoadAgent = "O5_IsHandlesRoadAgent";
			public const string O5_IsHandlesRailAgent = "O5_IsHandlesRailAgent";
		}

		#endregion

		#region Validation

		protected override OrgAppointedAgentPortsValidation GetNewValidation()
		{
			switch (O5_SeaAirCarrierOrForwarderType)
			{
				case OrgAppointedAgentPorts.Forwarder:
					return new OrgForwarderAppointedAgentPortsValidation(this);
				case OrgAppointedAgentPorts.GatewayAgent:
					return new OrgGatewayAppointedAgentPortsValidation(this);
				default:
					return new OrgAppointedAgentPortsValidation(this);
			}
		}

		void ValidateAll()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
		}

		#endregion

		#region Properties

		#region O5_PortOrCountry

		[List("Lookups.Locations")]
		public override ZString O5_PortOrCountry
		{
			get { return base.O5_PortOrCountry; }
			set
			{
				base.O5_PortOrCountry = value;
				ValidateAll();
			}
		}

		#endregion

		#region O5_OA_AgentOfficeAddress

		[List("Lookups.ActiveAddresses")]
		public override ZGuid O5_OA_AgentOfficeAddress
		{
			get { return base.O5_OA_AgentOfficeAddress; }
			set
			{
				base.O5_OA_AgentOfficeAddress = value;
				ValidateAll();
			}
		}

		#endregion

		#region O5_TerminalType

		[List("Lookups.StevedoreTypeList")]
		public override ZString O5_TerminalType
		{
			get { return base.O5_TerminalType; }
			set { base.O5_TerminalType = value; }
		}

		#endregion

		#region O5_RoadAgentStatus

		[List("Lookups.AgentStatuses")]
		public override ZString O5_RoadAgentStatus
		{
			get { return base.O5_RoadAgentStatus; }
			set { base.O5_RoadAgentStatus = value; }
		}

		#endregion

		#region O5_RailAgentStatus

		[List("Lookups.AgentStatuses")]
		public override ZString O5_RailAgentStatus
		{
			get { return base.O5_RailAgentStatus; }
			set { base.O5_RailAgentStatus = value; }
		}

		#endregion

		#region O5_SeaAgentStatus

		[List("Lookups.AgentStatuses")]
		public override ZString O5_SeaAgentStatus
		{
			get { return base.O5_SeaAgentStatus; }
			set { base.O5_SeaAgentStatus = value; }
		}
		#endregion

		#region O5_AirAgentStatus

		[List("Lookups.AgentStatuses")]
		public override ZString O5_AirAgentStatus
		{
			get { return base.O5_AirAgentStatus; }
			set { base.O5_AirAgentStatus = value; }
		}

		#endregion

		#region Air Agent

		public ZBool O5_IsPublishedAirAgent
		{
			get { return O5_AirAgentStatus == AgentStatusList.Codes.Published; }
		}

		public ZBool O5_IsAppointedAirAgent
		{
			get
			{
				return ((O5_AirAgentStatus == AgentStatusList.Codes.Appointed) ||
					(O5_AirAgentStatus == AgentStatusList.Codes.Published));
			}
		}

		public ZBool O5_IsHandlesAirAgent
		{
			get
			{
				return ((O5_AirAgentStatus == AgentStatusList.Codes.Published) ||
						(O5_AirAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_AirAgentStatus == AgentStatusList.Codes.Handles));
			}
			set
			{
				O5_AirAgentStatus = value ? AgentStatusList.Codes.Handles : "";
				O5_IsHandlesAirAgentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo O5_IsHandlesAirAgentInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.O5_IsHandlesAirAgent); }
		}

		public ZBool O5_IsGatewayAirAgent
		{
			get
			{
				return ((O5_AirAgentStatus == AgentStatusList.Codes.GatewayAgentWithTariff) ||
						(O5_AirAgentStatus == AgentStatusList.Codes.GatewayAgent));
			}
		}

		#endregion

		#region Sea Agent

		public ZBool O5_IsPublishedSeaAgent
		{
			get { return (O5_SeaAgentStatus == AgentStatusList.Codes.Published); }
		}

		public ZBool O5_IsAppointedSeaAgent
		{
			get
			{
				return ((O5_SeaAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_SeaAgentStatus == AgentStatusList.Codes.Published));
			}
		}

		public ZBool O5_IsHandlesSeaAgent
		{
			get
			{
				return ((O5_SeaAgentStatus == AgentStatusList.Codes.Published) ||
						(O5_SeaAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_SeaAgentStatus == AgentStatusList.Codes.Handles));
			}
			set
			{
				O5_SeaAgentStatus = value ? AgentStatusList.Codes.Handles : "";
				O5_IsHandlesSeaAgentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo O5_IsHandlesSeaAgentInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.O5_IsHandlesSeaAgent); }
		}

		public ZBool O5_IsGatewaySeaAgent
		{
			get
			{
				return ((O5_SeaAgentStatus == AgentStatusList.Codes.GatewayAgentWithTariff) ||
						(O5_SeaAgentStatus == AgentStatusList.Codes.GatewayAgent));
			}
		}

		#endregion

		#region Road Agent

		public ZBool O5_IsPublishedRoadAgent
		{
			get { return (O5_RoadAgentStatus == AgentStatusList.Codes.Published); }
		}

		public ZBool O5_IsAppointedRoadAgent
		{
			get
			{
				return ((O5_RoadAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_RoadAgentStatus == AgentStatusList.Codes.Published));
			}
		}

		public ZBool O5_IsHandlesRoadAgent
		{
			get
			{
				return ((O5_RoadAgentStatus == AgentStatusList.Codes.Published) ||
						(O5_RoadAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_RoadAgentStatus == AgentStatusList.Codes.Handles));
			}
			set
			{
				O5_RoadAgentStatus = value ? AgentStatusList.Codes.Handles : "";
				O5_IsHandlesRoadAgentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo O5_IsHandlesRoadAgentInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.O5_IsHandlesRoadAgent); }
		}

		public ZBool O5_IsGatewayRoadAgent
		{
			get
			{
				return ((O5_RoadAgentStatus == AgentStatusList.Codes.GatewayAgentWithTariff) ||
						(O5_RoadAgentStatus == AgentStatusList.Codes.GatewayAgent));
			}
		}

		#endregion

		#region Rail Agent

		public ZBool O5_IsPublishedRailAgent
		{
			get
			{
				return (O5_RailAgentStatus == AgentStatusList.Codes.Published);
			}
		}

		public ZBool O5_IsAppointedRailAgent
		{
			get
			{
				return ((O5_RailAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_RailAgentStatus == AgentStatusList.Codes.Published));
			}
		}

		public ZBool O5_IsHandlesRailAgent
		{
			get
			{
				return ((O5_RailAgentStatus == AgentStatusList.Codes.Published) ||
						(O5_RailAgentStatus == AgentStatusList.Codes.Appointed) ||
						(O5_RailAgentStatus == AgentStatusList.Codes.Handles));
			}
			set
			{
				O5_RailAgentStatus = value ? AgentStatusList.Codes.Handles : "";
				O5_IsHandlesRailAgentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo O5_IsHandlesRailAgentInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.O5_IsHandlesRailAgent); }
		}

		public ZBool O5_IsGatewayRailAgent
		{
			get
			{
				return ((O5_RailAgentStatus == AgentStatusList.Codes.GatewayAgentWithTariff) ||
						(O5_RailAgentStatus == AgentStatusList.Codes.GatewayAgent));
			}
		}

		#endregion

		#region O5_AgentDirection

		[List("Lookups.AgentDirections")]
		public override ZString O5_AgentDirection
		{
			get { return base.O5_AgentDirection; }
			set
			{
				base.O5_AgentDirection = value;
				ValidateAll();
			}
		}

		#endregion

		#endregion

		#region ExclusiveGatewayServices

		[ChildEditable]
		[ActionFieldFollow]
		public OrgExclusiveGatewayServiceCollection ExclusiveGatewayServices
		{
			get
			{
				if (exclusiveGatewayServices == null)
				{
					exclusiveGatewayServices = new OrgExclusiveGatewayServiceCollection(this);
					RegisterEditableChildObject(exclusiveGatewayServices);
				}

				return exclusiveGatewayServices;
			}
		}

		OrgExclusiveGatewayServiceCollection exclusiveGatewayServices;

		#endregion

		#region IReadOnlySecurity Members

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Header != null && !Header.SecurityProvider.HasModifyForwarderDetailsSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		public override void Delete()
		{
			exclusiveGatewayServices?.DeleteAll();
			base.Delete();
		}
	}
}
