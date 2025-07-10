using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgExclusiveGatewayServiceCollection))]
	public class OrgExclusiveGatewayServiceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "26 Myrtle Street";
			header.MainAddress.OA_City = "Prospect";
			var appAgPorts = header.AppointedAgentPorts.AddNew();
			appAgPorts.O5_PortOrCountry = "USLAX";
			appAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPorts.O5_OA_AgentOfficeAddress = header.MainAddress.PK;

			return new OrgExclusiveGatewayServiceCollection(appAgPorts);
		}
	}
}
