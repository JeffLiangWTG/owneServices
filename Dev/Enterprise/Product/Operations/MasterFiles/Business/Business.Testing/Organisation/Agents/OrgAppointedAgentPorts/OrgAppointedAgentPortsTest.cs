using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAppointedAgentPorts))]
	sealed class OrgAppointedAgentPortsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			#region blank

			OrgAppointedAgentPorts test = Factory.New<OrgAppointedAgentPorts>();
			Assert("when property is blank, all properties are false", !test.O5_IsHandlesAirAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsHandlesRailAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsHandlesRoadAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsHandlesSeaAgent);

			Assert("when property is blank, all properties are false", !test.O5_IsAppointedAirAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsAppointedRailAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsAppointedRoadAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsAppointedSeaAgent);

			Assert("when property is blank, all properties are false", !test.O5_IsPublishedAirAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsPublishedRailAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsPublishedRoadAgent);
			Assert("when property is blank, all properties are false", !test.O5_IsPublishedSeaAgent);

			#endregion

			#region Handles

			test.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.Handles;
			test.O5_RailAgentStatus = AgentStatusList.Codes.Handles;
			Assert("when property is HAN, agent is Handles", test.O5_IsHandlesAirAgent);
			Assert("when property is HAN, agent is Handles", test.O5_IsHandlesRailAgent);
			Assert("when property is HAN, agent is Handles", test.O5_IsHandlesRoadAgent);
			Assert("when property is HAN, agent is Handles", test.O5_IsHandlesSeaAgent);

			Assert("when property is HAN, agent is Handles", !test.O5_IsAppointedAirAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsAppointedRailAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsAppointedRoadAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsAppointedSeaAgent);

			Assert("when property is HAN, agent is Handles", !test.O5_IsPublishedAirAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsPublishedRailAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsPublishedRoadAgent);
			Assert("when property is HAN, agent is Handles", !test.O5_IsPublishedSeaAgent);

			#endregion

			#region appointed

			test.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.Appointed;
			test.O5_RailAgentStatus = AgentStatusList.Codes.Appointed;
			Assert("when property is APP, agent is Handles", test.O5_IsHandlesAirAgent);
			Assert("when property is APP, agent is Handles", test.O5_IsHandlesRailAgent);
			Assert("when property is APP, agent is Handles", test.O5_IsHandlesRoadAgent);
			Assert("when property is APP, agent is Handles", test.O5_IsHandlesSeaAgent);

			Assert("when property is APP, agent is Appointed", test.O5_IsAppointedAirAgent);
			Assert("when property is APP, agent is Appointed", test.O5_IsAppointedRailAgent);
			Assert("when property is APP, agent is Appointed", test.O5_IsAppointedRoadAgent);
			Assert("when property is APP, agent is Appointed", test.O5_IsAppointedSeaAgent);

			Assert("when property is HAN, agent is Handles and Appointed", !test.O5_IsPublishedAirAgent);
			Assert("when property is HAN, agent is Handles and Appointed", !test.O5_IsPublishedRailAgent);
			Assert("when property is HAN, agent is Handles and Appointed", !test.O5_IsPublishedRoadAgent);
			Assert("when property is HAN, agent is Handles and Appointed", !test.O5_IsPublishedSeaAgent);

			#endregion

			#region published

			test.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
			test.O5_RailAgentStatus = AgentStatusList.Codes.Published;
			Assert("when property is PUB, agent is Handles", test.O5_IsHandlesAirAgent);
			Assert("when property is PUB, agent is Handles", test.O5_IsHandlesRailAgent);
			Assert("when property is PUB, agent is Handles", test.O5_IsHandlesRoadAgent);
			Assert("when property is PUB, agent is Handles", test.O5_IsHandlesSeaAgent);

			Assert("when property is PUB, agent is Appointed", test.O5_IsAppointedAirAgent);
			Assert("when property is PUB, agent is Appointed", test.O5_IsAppointedRailAgent);
			Assert("when property is PUB, agent is Appointed", test.O5_IsAppointedRoadAgent);
			Assert("when property is PUB, agent is Appointed", test.O5_IsAppointedSeaAgent);

			Assert("when property is PUB, agent is Published", test.O5_IsPublishedAirAgent);
			Assert("when property is PUB, agent is Published", test.O5_IsPublishedRailAgent);
			Assert("when property is PUB, agent is Published", test.O5_IsPublishedRoadAgent);
			Assert("when property is PUB, agent is Published", test.O5_IsPublishedSeaAgent);

			#endregion

			#region Gateway

			test.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			test.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			Assert("when property is GTT, agent is Gateway", test.O5_IsGatewayAirAgent);
			Assert("when property is GTT, agent is Gateway", test.O5_IsGatewayRailAgent);
			Assert("when property is GTT, agent is Gateway", test.O5_IsGatewayRoadAgent);
			Assert("when property is GTT, agent is Gateway", test.O5_IsGatewaySeaAgent);

			test.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			test.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Assert("when property is GTA, agent is Gateway", test.O5_IsGatewayAirAgent);
			Assert("when property is GTA, agent is Gateway", test.O5_IsGatewayRailAgent);
			Assert("when property is GTA, agent is Gateway", test.O5_IsGatewayRoadAgent);
			Assert("when property is GTA, agent is Gateway", test.O5_IsGatewaySeaAgent);

			#endregion

			#region mixed

			test.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			test.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;
			test.O5_RoadAgentStatus = AgentStatusList.Codes.Handles;
			test.O5_RailAgentStatus = "";
			Assert(test.O5_IsHandlesAirAgent);
			Assert(!test.O5_IsHandlesRailAgent);
			Assert(test.O5_IsHandlesRoadAgent);
			Assert(test.O5_IsHandlesSeaAgent);

			Assert(test.O5_IsAppointedAirAgent);
			Assert(!test.O5_IsAppointedRailAgent);
			Assert(!test.O5_IsAppointedRoadAgent);
			Assert(test.O5_IsAppointedSeaAgent);

			Assert(test.O5_IsPublishedAirAgent);
			Assert(!test.O5_IsPublishedRailAgent);
			Assert(!test.O5_IsPublishedRoadAgent);
			Assert(!test.O5_IsPublishedSeaAgent);

			#endregion
		}

		public void TestReadOnlySecurity()
		{
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;
			bool oldForwarderValue = Env.Security.OrgForwarderModifyDetails.IsAllowed;

			try
			{
				var testAgentPort = OrgInDB.AppointedAgentPorts.AddNew();
				var testGatewayAgentPort = OrgInDB.AppointedGatewayAgentPorts.AddNew();

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Env.Security.OrgForwarderModifyDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testAgentPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);

				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testGatewayAgentPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);

				Env.Security.OrgForwarderModifyDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testAgentPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);

				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testGatewayAgentPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
				Env.Security.OrgForwarderModifyDetails.IsAllowed = oldForwarderValue;
			}
		}

		public void TestO5_AgentDirection()
		{
			ListAttribute attribute = (ListAttribute)Attribute.GetCustomAttribute(typeof(OrgAppointedAgentPorts).GetMember(OrgAppointedAgentPorts.Schema.O5_AgentDirection)[0], typeof(ListAttribute));
			AssertEquals("Lookups.AgentDirections", attribute.ListDataSourceMember);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgInDB.AppointedAgentPorts.AddNew();
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion
	}
}
