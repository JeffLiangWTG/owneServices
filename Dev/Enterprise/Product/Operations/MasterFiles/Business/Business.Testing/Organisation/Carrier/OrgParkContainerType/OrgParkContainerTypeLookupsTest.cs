using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgParkContainerTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganisationContacts()
		{
			var orgContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			orgContainerYard.OH_IsContainerYard = true;
			orgContainerYard.OH_Code = "CY";
			orgContainerYard.OH_FullName = "Container Yard";

			var orgCarrier = Factory.NewWithValidTestData<OrgHeader>();
			orgCarrier.OH_IsShippingProvider = true;
			orgCarrier.OH_FullName = "Carrier";
			orgCarrier.OH_Code = "CRR";

			var orgCarrier1AppointedPort = orgCarrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			orgCarrier1AppointedPort.O5_PortOrCountry = "AUMEL";
			orgCarrier1AppointedPort.OrganisationPK = orgContainerYard.PK;
			orgCarrier1AppointedPort.O5_OA_AgentOfficeAddress = orgContainerYard.MainAddress.PK;

			var containerType = orgCarrier1AppointedPort.ContainerTypes.AddNew();
			var orgContacts = containerType.Lookups.OrganisationContacts;
			orgContacts.Load();
			AssertEquals("No contact", 0, orgContacts.Count);

			orgCarrier.Contacts.AddNew();
			orgContacts.Load();
			AssertEquals("1 contact", 1, orgContacts.Count);
		}
	}
}
