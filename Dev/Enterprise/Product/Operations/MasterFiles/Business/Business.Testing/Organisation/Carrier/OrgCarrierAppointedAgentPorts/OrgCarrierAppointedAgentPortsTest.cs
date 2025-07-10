using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierAppointedAgentPorts))]
	sealed class OrgCarrierAppointedAgentPortsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewValidation_ReturnsCorrectTypes()
		{
			var carrierAppointedPorts = Factory.New<OrgCarrierAppointedAgentPorts>();

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.AirCTO;
			AssertType("AirCTO Validation Type", typeof(OrgCarrierAppointedAirCTOValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Stevedore;
			AssertType("SeaCTO Validation Type", typeof(OrgCarrierAppointedSeaCTOValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.RoadDepotShed;
			AssertType("RoadCTO Validation Type", typeof(OrgCarrierAppointedRoadCTOValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.RailHeadDepot;
			AssertType("RailCTO Validation Type", typeof(OrgCarrierAppointedRailCTOValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.ContainerYard;
			AssertType("ContainerYard Validation Type", typeof(OrgCarrierAppointedCYValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			AssertType("Agency Validation Type", typeof(OrgCarrierAppointedAgencyValidation), carrierAppointedPorts.Validation);

			carrierAppointedPorts.O5_SeaAirCarrierOrForwarderType = ZString.Empty;
			AssertType("Default Validation Type", typeof(OrgCarrierAppointedAgentPortsValidation), carrierAppointedPorts.Validation);
		}

		public void TestDefaults()
		{
			OrgCarrierAppointedAgentPorts carrAppPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			Assert(carrAppPorts.O5_SeaAirCarrierOrForwarderType.IsEmpty);
		}

		public void TestCarrierOrganisation()
		{
			OrgHeader header = NewTestHeaderForTests();
			OrgCarrierAppointedAgentPorts carrAppPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			carrAppPorts.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			AssertEquals(header.PK, carrAppPorts.OrganisationPK);
			AssertEquals(header.OH_FullName, carrAppPorts.OrganisationName);
		}

		public void TestOrgCarrierAppointedPorts()
		{
			OrgHeader headerForTest = NewTestHeaderForTests();
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Org 1 2 3";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "26 Myrtle Street";
			header.MainAddress.OA_City = "Prospect";
			header.OH_Code = "TestOrg123";
			OrgAppointedAgentPorts appAgPorts = header.AppointedAgentPorts.AddNew();
			appAgPorts.O5_PortOrCountry = "USLAX";
			appAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPorts.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			OrgCarrierAppointedAgentPorts carrAppPorts = header.CarrierAppointedAgentPorts_AirCTO.AddNew();
			carrAppPorts.O5_PortOrCountry = "USLAX";

			OrgHeaderCollection organisations = carrAppPorts.Lookups.OrganisationList;
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "Test123");
			organisations.Load(query);
			carrAppPorts.OrganisationPK = organisations[0].PK;
			carrAppPorts.O5_OA_AgentOfficeAddress = organisations[0].MainAddress.PK;

			header.Factory.Save();

			organisations = new OrgHeaderCollection(Factory);
			organisations.Load(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "TestOrg123"));
			Assert(!organisations[0].CarrierAppointedAgentPorts_AirCTO.Contains(appAgPorts.PK));
			Assert(organisations[0].CarrierAppointedAgentPorts_AirCTO.Contains(carrAppPorts.PK));

			AssertEquals("Test Org 1 2 3", organisations[0].CarrierAppointedAgentPorts_AirCTO[0].OrganisationName);
		}

		public void TestAgentOfficeAddressDefaultingOnOrganisationChange()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.MainAddress.OA_Address1 = "123 faze street";

			OrgCarrierAppointedAgentPorts agentPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			agentPorts.OrganisationPK = orgHeader1.PK;

			OrgHeader orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.MainAddress.OA_Address1 = "125 optic street";

			agentPorts.OrganisationPK = orgHeader2.PK;
			AssertEquals(orgHeader2.MainAddress.PK, agentPorts.O5_OA_AgentOfficeAddress);

			agentPorts.OrganisationPK = ZGuid.NewZGuid();
			AssertEquals(ZGuid.Empty, agentPorts.O5_OA_AgentOfficeAddress);

			agentPorts.O5_OA_AgentOfficeAddress = orgHeader1.MainAddress.PK;
			AssertEquals(agentPorts.OrganisationPK, orgHeader1.PK);

			agentPorts.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, agentPorts.O5_OA_AgentOfficeAddress);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;
			bool oldForwarderValue = Env.Security.OrgForwarderModifyDetails.IsAllowed;

			try
			{
				OrgCarrierAppointedAgentPorts testCarrierPort = OrgInDB.CarrierAppointedAgentPorts_AirCTO.AddNew();

				Env.Security.OrgForwarderModifyDetails.IsAllowed = false;
				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCarrierPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_AirAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_IsHandlesAirAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_IsHandlesRailAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_IsHandlesRoadAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_IsHandlesSeaAgentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_OA_AgentOfficeAddressInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_RailAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_RoadAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_SeaAgentStatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCarrierPort.O5_SeaAirCarrierOrForwarderTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
				Env.Security.OrgForwarderModifyDetails.IsAllowed = oldForwarderValue;
			}
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

		public void TestLookups()
		{
			var port = Factory.New<OrgCarrierAppointedAgentPorts>();
			AssertType(typeof(OrgCarrierAppointedAgentPortsUNLOCOLookups), port.Lookups);

			port = Factory.New<OrgCarrierAppointedAgentPorts>();
			port.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			AssertType(typeof(OrgCarrierAppointedAgentPortsLookups), port.Lookups);

			port = Factory.New<OrgCarrierAppointedAgentPorts>();
			port.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.AirCTO;
			AssertType(typeof(OrgCarrierAppointedAgentPortsUNLOCOLookups), port.Lookups);
		}

		#region Implementation

		OrgHeader NewTestHeaderForTests()
		{
			OrgHeader headerForTest = Factory.New<OrgHeader>();
			headerForTest.OH_FullName = "Test Org 1 2 3";
			headerForTest.OH_RL_NKClosestPort = "AUSYD";
			headerForTest.MainAddress.OA_Address1 = "26 Test Street";
			headerForTest.MainAddress.OA_City = "Prospect";
			headerForTest.OH_Code = "Test123";
			headerForTest.OH_IsShippingProvider = ZBool.True;
			headerForTest.OH_IsAirCTO = ZBool.True;
			headerForTest.OH_IsSeaCTO = ZBool.True;
			headerForTest.OH_IsRailHead = ZBool.True;
			headerForTest.OH_IsRoadFreightDepot = ZBool.True;
			headerForTest.Factory.Save();
			return headerForTest;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			var agent = factory.NewWithValidTestData<OrgHeader>();
			var port = header.CarrierAppointedAgentPorts_Agency.AddNew();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;

			return port;
		}

		#endregion
	}
}
