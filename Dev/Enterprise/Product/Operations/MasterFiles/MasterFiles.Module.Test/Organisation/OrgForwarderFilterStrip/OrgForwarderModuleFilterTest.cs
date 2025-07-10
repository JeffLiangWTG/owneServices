using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgForwarderModuleFilter))]
	sealed class OrgForwarderModuleFilterTest : ModuleTextFilterTest
	{
		#region Forwarder AgentStatus Filter

		public void TestForwarderAgentStatusFilter()
		{
			ForwarderAgentStatusTest(Core.Constants.TransportModes.Air, OrgAppointedAgentPortsSchema.O5_AirAgentStatus);
			ForwarderAgentStatusTest(Core.Constants.TransportModes.Sea, OrgAppointedAgentPortsSchema.O5_SeaAgentStatus);
			ForwarderAgentStatusTest(Core.Constants.TransportModes.Rail, OrgAppointedAgentPortsSchema.O5_RailAgentStatus);
			ForwarderAgentStatusTest(Core.Constants.TransportModes.Road, OrgAppointedAgentPortsSchema.O5_RoadAgentStatus);
		}

		void ForwarderAgentStatusTest(ZString agentMode, SchemaColumn statusType)
		{
			OrgHeader org1 = GetNewOrg(OrgHeaderSchema.OH_IsForwarder);
			OrgHeader org2 = GetNewOrg(OrgHeaderSchema.OH_IsForwarder);
			OrgHeader org3 = GetNewOrg(OrgHeaderSchema.OH_IsForwarder);
			OrgHeader org4 = GetNewOrg(OrgHeaderSchema.OH_IsForwarder);
			OrgHeader org5 = GetNewOrg();

			AddAppointedAgentPorts(org1, statusType, AgentStatusList.Codes.Handles);
			AddAppointedAgentPorts(org2, statusType, AgentStatusList.Codes.Appointed);
			AddAppointedAgentPorts(org3, statusType, AgentStatusList.Codes.Published);
			AddAppointedAgentPorts(org4, statusType, "");
			AddAppointedAgentPorts(org5, statusType, AgentStatusList.Codes.Handles);

			Factory.Save();

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgForwarderModuleFilterForTest = (OrgForwarderModuleFilter)filterStripBizO["Forwarder - Agent Status"];
			orgForwarderModuleFilterForTest.Property = agentMode;
			orgForwarderModuleFilterForTest.Status = "";
			orgForwarderModuleFilterForTest.IsActive = true;

			OrgHeaderCollection organisations = new OrgHeaderCollection(Factory);
			AssertEquals("Organisations.Count", 0, organisations.Count);

			organisations.Load(filterStripBizO.Filter);
			Assert("Organisations.Count >= 3", organisations.Count >= 3);
			Assert("Org1 is in collection", organisations.Contains(org1));
			Assert("Org2 is in collection", organisations.Contains(org2));
			Assert("Org3 is in collection", organisations.Contains(org3));
			Assert("Org4 is not in collection", !organisations.Contains(org4));
			Assert("Org5 is not in collection", !organisations.Contains(org5));

			orgForwarderModuleFilterForTest.Status = AgentStatusList.Codes.Appointed;
			organisations.Load(filterStripBizO.Filter);
			Assert("Org1 is not in collection", !organisations.Contains(org1));
			Assert("Org2 is in collection", organisations.Contains(org2));
			Assert("Org3 is not in collection", !organisations.Contains(org3));
			Assert("Org4 is not in collection", !organisations.Contains(org4));
			Assert("Org5 is not in collection", !organisations.Contains(org5));

			orgForwarderModuleFilterForTest.Status = AgentStatusList.Codes.Handles;
			organisations.Load(filterStripBizO.Filter);
			Assert("Org1 is in collection", organisations.Contains(org1));
			Assert("Org2 is not in collection", !organisations.Contains(org2));
			Assert("Org3 is not in collection", !organisations.Contains(org3));
			Assert("Org4 is not in collection", !organisations.Contains(org4));
			Assert("Org5 is not in collection", !organisations.Contains(org5));

			orgForwarderModuleFilterForTest.Status = AgentStatusList.Codes.Published;
			organisations.Load(filterStripBizO.Filter);
			Assert("Org1 is not in collection", !organisations.Contains(org1));
			Assert("Org2 is not in collection", !organisations.Contains(org2));
			Assert("Org3 is in collection", organisations.Contains(org3));
			Assert("Org4 is not in collection", !organisations.Contains(org4));
			Assert("Org5 is not in collection", !organisations.Contains(org5));

			orgForwarderModuleFilterForTest.Property = "";
			organisations.Load(filterStripBizO.Filter);
			Assert("Org1 is not in collection", !organisations.Contains(org1));
			Assert("Org2 is not in collection", !organisations.Contains(org2));
			Assert("Org3 is in collection", organisations.Contains(org3));
			Assert("Org4 is not in collection", !organisations.Contains(org4));
			Assert("Org5 is not in collection", !organisations.Contains(org5));
		}

		public void TestForwarderAgentStatus_OnlyFiltersSeaAirRoadRailModes()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgForwarderModuleFilterForTest = (OrgForwarderModuleFilter)filterStripBizO["Forwarder - Agent Status"];

			var expectedModes = new CodeDescriptionPairList();
			expectedModes.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
			expectedModes.AddPair(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail);
			expectedModes.AddPair(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road);
			expectedModes.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);

			AssertContainsExactElementsInAnyOrder(expectedModes, orgForwarderModuleFilterForTest.Modes);
		}

		public void TestForwarderAgentStatus_OnlyFiltersAppointedHandlesPublishedStatuses()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgForwarderModuleFilterForTest = (OrgForwarderModuleFilter)filterStripBizO["Forwarder - Agent Status"];

			var expectedStatuses = new CodeDescriptionPairList();
			expectedStatuses.AddPair(AgentStatusList.Codes.Appointed, AgentStatusList.Descriptions.Appointed);
			expectedStatuses.AddPair(AgentStatusList.Codes.Handles, AgentStatusList.Descriptions.Handles);
			expectedStatuses.AddPair(AgentStatusList.Codes.Published, AgentStatusList.Descriptions.Published);

			AssertContainsExactElementsInAnyOrder(expectedStatuses, orgForwarderModuleFilterForTest.Statuses);
		}

		#endregion

		#region Empty

		public void TestIsEmpty()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgForwarderModuleFilterForTest = (OrgForwarderModuleFilter)filterStripBizO["Forwarder - Agent Status"];

			AssertEquals("IsEmpty", true, orgForwarderModuleFilterForTest.IsEmpty);

			orgForwarderModuleFilterForTest.Property = "XXX";
			AssertEquals("IsEmpty", false, orgForwarderModuleFilterForTest.IsEmpty);

			orgForwarderModuleFilterForTest.Property = ZString.Empty;
			orgForwarderModuleFilterForTest.Status = "YYY";

			AssertEquals("IsEmpty", false, orgForwarderModuleFilterForTest.IsEmpty);

			orgForwarderModuleFilterForTest.Status = ZString.Empty;
			AssertEquals("IsEmpty", true, orgForwarderModuleFilterForTest.IsEmpty);
		}

		#endregion

		#region Implementation

		#region Create Orgs

		OrgHeader GetNewOrg()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			return org;
		}

		OrgHeader GetNewOrg(SchemaColumn orgType)
		{
			OrgHeader org = GetNewOrg();
			org[orgType] = ZBool.True;
			return org;
		}

		#endregion

		#region AddAppointed Agent Ports

		OrgAppointedAgentPorts AddAppointedAgentPorts(OrgHeader org, SchemaColumn statusType, ZString status)
		{
			OrgAppointedAgentPorts appAgent = org.AppointedAgentPorts.AddNew();

			appAgent.O5_PortOrCountry = "AU";
			appAgent.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			appAgent[statusType] = status;

			return appAgent;
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new OrgForwarderModuleFilter("moo");
		}

		#endregion

		#endregion
	}
}
