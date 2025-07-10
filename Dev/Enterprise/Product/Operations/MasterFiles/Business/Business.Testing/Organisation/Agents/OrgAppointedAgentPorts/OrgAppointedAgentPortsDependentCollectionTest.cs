using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAppointedAgentPortsDependentCollection))]
	sealed class OrgAppointedAgentPortsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgAppointedAgentPortsDependentCollection(OrgHeader.New(Factory), OrgAppointedAgentPorts.Forwarder);
		}

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

		public void TestOrgAppointedPortsCollection()
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
			Assert(organisations[0].AppointedAgentPorts.Contains(appAgPorts.PK));
			Assert(!organisations[0].AppointedAgentPorts.Contains(carrAppPorts.PK));
		}
	}
}
