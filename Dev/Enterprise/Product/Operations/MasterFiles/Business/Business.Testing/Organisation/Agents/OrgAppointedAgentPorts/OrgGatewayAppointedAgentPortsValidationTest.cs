using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgGatewayAppointedAgentPortsValidationTest : OrgAppointedAgentPortsValidationTest
	{
		public void TestCheckPortIsNotDuplicated()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var address1 = CreateAddress(header, "1 Test Street");
			var address2 = CreateAddress(header, "2 Test Street");
			var address3 = CreateAddress(header, "3 Test Street");

			var address1AUBNE = CreateOrgGatewayAppointedAgentPorts(address1, "AUBNE");
			var address2AUBNE = CreateOrgGatewayAppointedAgentPorts(address2, "AUBNE");
			var address3AUBNE = CreateOrgGatewayAppointedAgentPorts(address3, "AUBNE");

			AssertNoErrors(address1AUBNE.O5_PortOrCountryInfo);
			AssertNoErrors(address2AUBNE.O5_PortOrCountryInfo);
			AssertNoErrors(address3AUBNE.O5_PortOrCountryInfo);

			var duplicatePort = CreateOrgGatewayAppointedAgentPorts(address2, "AUBNE");
			AssertHasError(duplicatePort.O5_PortOrCountryInfo, "This organization already has Gateway Agent details for AUBNE. You cannot enter more than one Gateway Agent Office Address per Location.");

			header.AppointedGatewayAgentPorts.RemoveAndDelete(duplicatePort);

			var address2AUMEL = CreateOrgGatewayAppointedAgentPorts(address2, "AUMEL");
			AssertNoErrors("No error as not AUBNE", address2AUMEL.O5_PortOrCountryInfo);

			var address3AU = CreateOrgGatewayAppointedAgentPorts(address3, "AU");
			AssertNoErrors("No error as not specific port", address3AU.O5_PortOrCountryInfo);
		}

		public void TestO5_AgentDirection()
		{
			var expectedMandatory = "Please enter a Direction.";
			var expectedList = "Enter a valid Direction.";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var port = header.AppointedGatewayAgentPorts.AddNew();

			port.O5_AgentDirection = string.Empty;
			AssertHasError("Should be NOT Empty", port.O5_AgentDirectionInfo, expectedMandatory);

			port.O5_AgentDirection = "AAA";
			AssertHasError("Should be in the list", port.O5_AgentDirectionInfo, expectedList);

			port.O5_AgentDirection = AgentDirectionList.Codes.Export;
			AssertNoNotifications("Should be NO errors", port.O5_AgentDirectionInfo);

			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			AssertNoNotifications("Should be NO errors", port.O5_AgentDirectionInfo);

			port.O5_AgentDirection = AgentDirectionList.Codes.Import;
			AssertNoNotifications("Should be NO errors", port.O5_AgentDirectionInfo);
		}

		#region Agent Status

		public void TestAgentStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var port = org.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			port.O5_PortOrCountry = "AUMEL";
			port.O5_AgentDirection = AgentDirectionList.Codes.Export;

			AssertAgentStatus(port.O5_SeaAgentStatusInfo, "Sea");
			AssertAgentStatus(port.O5_AirAgentStatusInfo, "Air");
			AssertAgentStatus(port.O5_RailAgentStatusInfo, "Rail");
			AssertAgentStatus(port.O5_RoadAgentStatusInfo, "Road");
		}

		void AssertAgentStatus(ZPropertyInfo propertyInfo, ZString agentStatusType)
		{
			propertyInfo.Value = ZString.Empty;
			AssertNoNotifications("Should be NO errors", propertyInfo);

			propertyInfo.Value = (ZString)AgentStatusList.Codes.GatewayAgent;
			AssertNoNotifications("Should be NO errors", propertyInfo);

			propertyInfo.Value = (ZString)AgentStatusList.Codes.GatewayAgentWithTariff;
			AssertNoNotifications("Should be NO errors", propertyInfo);

			propertyInfo.Value = (ZString)"AAA";
			AssertHasError("Should be in the list", propertyInfo,
							string.Format(CultureInfo.InvariantCulture, "Enter a valid Gateway Type for {0}.", agentStatusType));
		}

		#endregion

		#region Implementation

		OrgAppointedAgentPorts CreateOrgGatewayAppointedAgentPorts(OrgAddress orgAddress, ZString port)
		{
			var header = orgAddress.Header;
			var result = header.AppointedGatewayAgentPorts.AddNew();
			result.O5_PortOrCountry = port;
			result.O5_OA_AgentOfficeAddress = orgAddress.PK;

			return result;
		}

		OrgAddress CreateAddress(OrgHeader orgHeader, ZString addressLine1)
		{
			var result = orgHeader.Addresses.AddNew();
			result.OA_Address1 = addressLine1;

			return result;
		}

		#endregion
	}
}
