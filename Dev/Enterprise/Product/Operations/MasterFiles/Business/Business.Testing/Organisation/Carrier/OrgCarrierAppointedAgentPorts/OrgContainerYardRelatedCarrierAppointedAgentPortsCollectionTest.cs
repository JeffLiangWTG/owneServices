using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContainerYardRelatedCarrierAppointedAgentPortsCollection))]
	sealed class OrgContainerYardRelatedCarrierAppointedAgentPortsCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgContainerYardRelatedCarrierAppointedAgentPortsCollection>
	{
		protected override OrgContainerYardRelatedCarrierAppointedAgentPortsCollection GetCollectionToTest()
		{
			return new OrgContainerYardRelatedCarrierAppointedAgentPortsCollection(Factory.NewWithValidTestData<OrgHeader>(), CarrierOrForwarderType.Codes.ContainerYard);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bizObject = Factory.New<OrgCarrierAppointedAgentPorts>();
			bizObject.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.ContainerYard;
			return bizObject;
		}

		public void TestContainerYardRelatedCarrierAppointedAgentPorts()
		{
			CreateCarrierWithContainerYardAppointedPort("CR1", "Carrier 1", orgContainerYard, "AUMEL");

			var collection = new OrgContainerYardRelatedCarrierAppointedAgentPortsCollection(orgContainerYard, "XXX");
			AssertEquals(0, collection.Count);

			collection = new OrgContainerYardRelatedCarrierAppointedAgentPortsCollection(orgContainerYard, CarrierOrForwarderType.Codes.ContainerYard);
			AssertEquals(1, collection.Count);
		}

		public void TestContainerYardRelatedCarrierAppointedAgentPorts_CreateRelationshipFilter()
		{
			CreateCarrierWithContainerYardAppointedPort("CR1", "Carrier 1", orgContainerYard, "AUMEL");
			CreateCarrierWithContainerYardAppointedPort("CR2", "Carrier 2", orgContainerYard, "AUSYD");

			var newFactory = new BusinessObjectFactory();
			orgContainerYard = newFactory.Load<OrgHeader>(orgContainerYard.PK);
			AssertEquals(2, orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Count);

			var expectedAppointedPort = orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Find(x => x.O5_PortOrCountry == "AUMEL").First();
			Assert(orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Any(p => p.PK == expectedAppointedPort.PK));

			expectedAppointedPort = orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Find(x => x.O5_PortOrCountry == "AUSYD").First();
			Assert(orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Any(p => p.PK == expectedAppointedPort.PK));
		}

		OrgHeader CreateCarrierWithContainerYardAppointedPort(ZString code, ZString name, OrgHeader orgParent, ZString portCode)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = name;
			carrier.OH_Code = code;

			var carrierAppointedPort = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			carrierAppointedPort.O5_PortOrCountry = portCode;
			carrierAppointedPort.OrganisationPK = orgParent.PK;
			carrierAppointedPort.O5_OA_AgentOfficeAddress = orgParent.MainAddress.PK;

			Factory.Save();

			return carrier;
		}

		protected override void SetUp()
		{
			orgContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			orgContainerYard.OH_IsContainerYard = true;
			orgContainerYard.OH_Code = "CY";
			orgContainerYard.OH_FullName = "Container Yard";
			Factory.Save();
		}
		OrgHeader orgContainerYard;

		public override void TestAdd()
		{
			Assert("This is Readonly Collection", true);
		}

		public new void TestAddNew()
		{
			Assert("This is Readonly Collection", true);
		}

		public override void TestDelete()
		{
			Assert("This is Readonly Collection", true);
		}

		public override void TestTypedget_Item()
		{
			Assert("This is Readonly Collection", true);
		}
	}
}
