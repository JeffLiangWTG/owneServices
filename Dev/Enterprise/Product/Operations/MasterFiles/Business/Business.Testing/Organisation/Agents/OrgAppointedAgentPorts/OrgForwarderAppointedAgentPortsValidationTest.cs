using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgForwarderAppointedAgentPortsValidationTest : OrgAppointedAgentPortsValidationTest
	{
		public void TestO5_AgentDirection()
		{
			string expectedMandatory = "Please enter a Direction.";
			string expectedList = "Enter a valid Direction.";

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAppointedAgentPorts port = header.AppointedAgentPorts.AddNew();

			port.O5_AgentDirection = "";
			AssertHasError("Should be NOT Empty", port.O5_AgentDirectionInfo, expectedMandatory);

			port.O5_AgentDirection = "AAA";
			AssertHasError("Should be in the list", port.O5_AgentDirectionInfo, expectedList);

			port.O5_AgentDirection = AgentDirectionList.Codes.Export;
			AssertNoNotifications("Should be NO errors", port.O5_AgentDirectionInfo);
		}

		public void TestCheckPortIsNotDuplicated()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress address1 = CreateAddress(header, "1 Test Street");
			OrgAddress address2 = CreateAddress(header, "2 Test Street");
			OrgAddress address3 = CreateAddress(header, "3 Test Street");

			var importAirPort = CreateOrgAppointedAgentPorts(address1, "AUBNE", "IMP", "HAN", "", "", "");
			var exportAirPort = CreateOrgAppointedAgentPorts(address2, "AUBNE", "EXP", "HAN", "", "", "");
			var importSeaPort = CreateOrgAppointedAgentPorts(address3, "AUBNE", "IMP", "", "PUB", "", "");
			var exportSeaPort = CreateOrgAppointedAgentPorts(address1, "AUBNE", "EXP", "", "APP", "", "");
			var importRailPort = CreateOrgAppointedAgentPorts(address2, "AUBNE", "IMP", "", "", "HAN", "");
			var exportRailPort = CreateOrgAppointedAgentPorts(address3, "AUBNE", "EXP", "", "", "HAN", "");
			var importRoadPort = CreateOrgAppointedAgentPorts(address1, "AUBNE", "IMP", "", "", "", "HAN");
			var exportRoadPort = CreateOrgAppointedAgentPorts(address2, "AUBNE", "EXP", "", "", "", "HAN");

			AssertNoErrors("No duplicate for import air", importAirPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for export air", importAirPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for import sea", importSeaPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for export sea", importSeaPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for import rail", importRailPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for export rail", importRailPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for import road", importRoadPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for export road", importRoadPort.O5_PortOrCountryInfo);

			var duplicatePort = CreateOrgAppointedAgentPorts(address2, "AUBNE", "IMP", "HAN", "", "", "");
			AssertHasError(duplicatePort.O5_PortOrCountryInfo, "This organization already has handling details for AUBNE. You cannot enter more than one per Location, Direction and Transport Mode.");

			header.AppointedAgentPorts.RemoveAndDelete(duplicatePort);

			var melPort = CreateOrgAppointedAgentPorts(address2, "AUMEL", "IMP", "HAN", "", "", "");
			AssertNoErrors("No error as not AUBNE", melPort.O5_PortOrCountryInfo);

			var auPort = CreateOrgAppointedAgentPorts(address3, "AU", "IMP", "HAN", "", "", "");
			AssertNoErrors("No error as not specific port", auPort.O5_PortOrCountryInfo);

			header.AppointedAgentPorts.RemoveAndDelete(auPort);
			header.AppointedAgentPorts.RemoveAndDelete(importSeaPort);
			header.AppointedAgentPorts.RemoveAndDelete(exportSeaPort);

			var importExportSeaPort = CreateOrgAppointedAgentPorts(address1, "AUBNE", "BTH", "", "HAN", "", "");
			AssertNoErrors("No duplicate for Sea", importExportSeaPort.O5_PortOrCountryInfo);

			exportSeaPort = CreateOrgAppointedAgentPorts(address2, "AUBNE", "EXP", "", "HAN", "", "");
			AssertHasError(exportSeaPort.O5_PortOrCountryInfo, "This organization already has handling details for AUBNE. You cannot enter more than one per Location, Direction and Transport Mode.");

			var allExportPort = CreateOrgAppointedAgentPorts(address3, "AUSYD", "EXP", "HAN", "HAN", "HAN", "HAN");
			exportRailPort = CreateOrgAppointedAgentPorts(address1, "AUSYD", "EXP", "", "", "HAN", "");

			AssertHasError(exportRailPort.O5_PortOrCountryInfo, "This organization already has handling details for AUSYD. You cannot enter more than one per Location, Direction and Transport Mode.");

			header.AppointedAgentPorts.RemoveAndDeleteAll();

			exportSeaPort = CreateOrgAppointedAgentPorts(address1, "AUSYD", "EXP", "", "HAN", "", "");
			allExportPort = CreateOrgAppointedAgentPorts(address3, "AUSYD", "EXP", "HAN", "HAN", "HAN", "HAN");

			AssertHasError(allExportPort.O5_PortOrCountryInfo, "This organization already has handling details for AUSYD. You cannot enter more than one per Location, Direction and Transport Mode.");

			header.AppointedAgentPorts.RemoveAndDeleteAll();

			var exportSeaRoadPort = CreateOrgAppointedAgentPorts(address1, "AUSYD", "EXP", "", "HAN", "", "HAN");
			var exportAirRailPort = CreateOrgAppointedAgentPorts(address2, "AUSYD", "EXP", "HAN", "", "HAN", "");
			var importSeaAirPort = CreateOrgAppointedAgentPorts(address3, "AUSYD", "IMP", "HAN", "HAN", "", "");
			var importRailRoadPort = CreateOrgAppointedAgentPorts(address1, "AUSYD", "IMP", "", "", "HAN", "HAN");

			AssertNoErrors("No duplicate: export sea / road", exportSeaRoadPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate: export air / rail", exportAirRailPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate: import sea / air", importSeaAirPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate: import rail / road", importRailRoadPort.O5_PortOrCountryInfo);

			exportAirPort = CreateOrgAppointedAgentPorts(address2, "AUSYD", "EXP", "HAN", "", "", "");

			AssertHasError(allExportPort.O5_PortOrCountryInfo, "This organization already has handling details for AUSYD. You cannot enter more than one per Location, Direction and Transport Mode.");

			header.AppointedAgentPorts.RemoveAndDeleteAll();

			importSeaPort = CreateOrgAppointedAgentPorts(address1, "AUSYD", "IMP", "", "HAN", "", "");
			importRoadPort = CreateOrgAppointedAgentPorts(address2, "AUSYD", "IMP", "", "", "", "HAN");
			exportSeaRoadPort = CreateOrgAppointedAgentPorts(address3, "AUSYD", "EXP", "", "HAN", "", "HAN");

			AssertNoErrors("No duplicate for import sea", importSeaPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for import road", importRoadPort.O5_PortOrCountryInfo);
			AssertNoErrors("No duplicate for export sea / road", exportSeaRoadPort.O5_PortOrCountryInfo);
		}

		#region AgentStatus

		public void TestAgentStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var publishedPort = org.AppointedAgentPorts.AddNew();
			publishedPort.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			publishedPort.O5_PortOrCountry = "AUMEL";
			publishedPort.O5_AgentDirection = AgentDirectionList.Codes.Export;
			SetAllAgentStatuses(publishedPort, AgentStatusList.Codes.Published);
			org.Factory.Save();

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newPort = newOrg.AppointedAgentPorts.AddNew();
			newPort.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			SetAllAgentStatuses(newPort, ZString.Empty);

			newPort.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			AssertCorrectAgentStatus(newPort, "Sea");

			newPort.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			AssertCorrectAgentStatus(newPort, "Air");

			newPort.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
			AssertCorrectAgentStatus(newPort, "Road");

			newPort.O5_RailAgentStatus = AgentStatusList.Codes.Published;
			AssertCorrectAgentStatus(newPort, "Rail");
		}

		void AssertCorrectAgentStatus(OrgAppointedAgentPorts newPort, ZString agentType)
		{
			ZPropertyInfo info = null;
			switch (agentType)
			{
				case "Sea":
					info = newPort.O5_SeaAgentStatusInfo;
					break;
				case "Air":
					info = newPort.O5_AirAgentStatusInfo;
					break;
				case "Road":
					info = newPort.O5_RoadAgentStatusInfo;
					break;
				case "Rail":
					info = newPort.O5_RailAgentStatusInfo;
					break;
				default:
					info = null;
					break;
			}

			newPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			newPort.O5_PortOrCountry = "GBLON";
			AssertNoErrors(newPort.O5_PortOrCountryInfo);
			AssertNoErrors("Published Ports overlaps for different UNLOCO so should not cause an error", info);

			newPort.O5_PortOrCountry = "AUMEL";
			AssertNoErrors(newPort.O5_PortOrCountryInfo);
			AssertHasErrors(info.Description + " Agent should have a warning", info);

			newPort.O5_AgentDirection = AgentDirectionList.Codes.Import;
			AssertNoErrors(newPort.O5_PortOrCountryInfo);
			AssertNoErrors("A UNLOCO may have two published ports if one is export and the other is import", info);

			SetAllAgentStatuses(newPort, AgentStatusList.Codes.Appointed);
			AssertNoErrors(info.Description + " Agent hould not have errors as restrictions only apply for conflicting published ports", info);

			SetAllAgentStatuses(newPort, AgentStatusList.Codes.Handles);
			AssertNoErrors(info.Description + " Agent should not have errors as restrictions only apply for conflicting published ports", info);

			SetAllAgentStatuses(newPort, ZString.Empty);
			AssertNoErrors(info.Description + " Agent should not have errors with blank ports", info);

			SetAllAgentStatuses(newPort, "AAA");
			AssertHasError("Should be in the list", info,
							string.Format(CultureInfo.InvariantCulture, "Enter a valid Agent Type for {0}.", agentType));
		}

		void SetAllAgentStatuses(OrgAppointedAgentPorts port, string code)
		{
			port.O5_SeaAgentStatus = code;
			port.O5_AirAgentStatus = code;
			port.O5_RoadAgentStatus = code;
			port.O5_RailAgentStatus = code;
		}

		#endregion

		#region Implementation

		OrgAppointedAgentPorts CreateOrgAppointedAgentPorts(OrgAddress orgAddress, ZString port, ZString direction, ZString airAgentStatus, ZString seaAgentStatus, ZString railAgentStatus, ZString roadAgentStatus)
		{
			var header = orgAddress.Header;
			var result = header.AppointedAgentPorts.AddNew();
			result.O5_PortOrCountry = port;
			result.O5_AgentDirection = direction;
			result.O5_AirAgentStatus = airAgentStatus;
			result.O5_SeaAgentStatus = seaAgentStatus;
			result.O5_RailAgentStatus = railAgentStatus;
			result.O5_RoadAgentStatus = roadAgentStatus;

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
