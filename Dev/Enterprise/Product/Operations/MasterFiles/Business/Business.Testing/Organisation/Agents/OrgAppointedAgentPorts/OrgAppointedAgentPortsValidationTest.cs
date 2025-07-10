using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgAppointedAgentPortsValidationTest : BusinessObjectValidationTestCase
	{
		#region Agent Office

		public void TestO5_OA_AgentOfficeAddress()
		{
			OrgHeader header = NewTestHeader();
			var port = GenerateOrgAppointedAgentPorts(header);
			port.O5_OA_AgentOfficeAddress = ZGuid.Empty;
			AssertHasError(port.O5_OA_AgentOfficeAddressInfo, "Please enter an Agent Office Address.");

			port.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			AssertNoNotifications(port.O5_OA_AgentOfficeAddressInfo);
		}

		public void TestO5_OA_AgentOfficeAddressShouldHaveErrorWhenInactive()
		{
			OrgHeader header = NewTestHeader();

			var address = header.Addresses.AddNewMainAddress();
			address.OA_IsActive = true;

			var port = GenerateOrgAppointedAgentPorts(header);
			port.O5_OA_AgentOfficeAddress = address.PK;
			Factory.Save();

			address.OA_IsActive = false;
			port.RunPreSaveValidation();
			AssertHasError(port.O5_OA_AgentOfficeAddressInfo, "Only active address can be set as Agent Office Address");
		}

		OrgAppointedAgentPorts GenerateOrgAppointedAgentPorts(OrgHeader header)
		{
			var result = header.AppointedAgentPorts.AddNew();
			result.O5_SeaAirCarrierOrForwarderType = OrgAppointedAgentPorts.Forwarder;
			result.O5_AgentDirection = AgentDirectionList.Codes.Both;
			result.O5_PortOrCountry = "AU";
			return result;
		}

		#endregion

		#region O5_PortOrCountry

		public void TestO5_PortOrCountry()
		{
			OrgHeader header = NewTestHeader();
			OrgAppointedAgentPorts port = header.AppointedAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			port.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;

			port.O5_PortOrCountry = "BLAH";
			AssertHasError("UNLOCO port should be invalid", port.O5_PortOrCountryInfo, "Enter a valid " + port.O5_PortOrCountryInfo.Description + ".");

			port.O5_PortOrCountry = "AUBNE";
			AssertNoErrors("Should be a valid UNLOCO port", port.O5_PortOrCountryInfo);

			port.O5_PortOrCountry = ZString.Empty;
			AssertHasError("An empty UNLOCO port is invalid", port.O5_PortOrCountryInfo, "Please enter a " + port.O5_PortOrCountryInfo.Description + ".");

			port.O5_PortOrCountry = "AU";
			AssertNoErrors("Should be a valid country", port.O5_PortOrCountryInfo);
		}

		#endregion

		#region Implementation

		public OrgHeader NewTestHeader()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Org 1 2 3";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "26 Myrtle Street";
			header.MainAddress.OA_City = "Prospect";
			header.OH_Code = "TestOrg123";
			header.Factory.Save();
			return header;
		}

		#endregion
	}
}
