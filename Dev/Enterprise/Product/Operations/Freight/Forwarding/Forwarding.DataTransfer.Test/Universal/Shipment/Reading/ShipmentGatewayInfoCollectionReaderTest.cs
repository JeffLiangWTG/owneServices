using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentGatewayInfoCollectionReader))]
	public class ShipmentGatewayInfoCollectionReaderTest : DataObjectCollectionReaderTest
	{
		readonly OrganizationAddress addressDataObject1 = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ArrivalCFSAddress);
		readonly OrganizationAddress addressDataObject2 = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ArrivalCFSAddress);
		readonly OrganizationAddress addressDataObject3 = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ArrivalCFSAddress);

		OrgAddress orgAddress1, orgAddress2, orgAddress3;

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress1 = new OrganisationDataObjectReader(addressDataObject1, new DummyLogger(), Factory).GetMatchedOrNewForTesting();
			orgAddress2 = new OrganisationDataObjectReader(addressDataObject2, new DummyLogger(), Factory).GetMatchedOrNewForTesting();
			orgAddress3 = new OrganisationDataObjectReader(addressDataObject3, new DummyLogger(), Factory).GetMatchedOrNewForTesting();

			AssertNotEquals(orgAddress1.PK, orgAddress2.PK);
			AssertNotEquals(orgAddress2.PK, orgAddress3.PK);
		}

		public override void TestReadIntoCollection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var gatewayInfos = new[] { CreateGatewayInfo(addressDataObject1, 0), CreateGatewayInfo(addressDataObject2, 1) };
			var reader = new ShipmentGatewayInfoCollectionReader(gatewayInfos, shipment, new DummyLogger(), Factory);

			reader.ReadIntoCollection();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals((ZByte)0, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress1.PK).JSG_Sequence);
			AssertEquals((ZByte)1, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress2.PK).JSG_Sequence);
		}

		public void TestReadIntoCollectionWithDeletionAndReordering()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			{
				var gatewayInfos = new[] { CreateGatewayInfo(addressDataObject1, 0), CreateGatewayInfo(addressDataObject2, 1), CreateGatewayInfo(addressDataObject3, 2) };
				var reader = new ShipmentGatewayInfoCollectionReader(gatewayInfos, shipment, new DummyLogger(), Factory);
				reader.ReadIntoCollection();

				AssertEquals(3, shipment.Gateways.Count);
				AssertEquals((ZByte)0, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress1.PK).JSG_Sequence);
				AssertEquals((ZByte)1, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress2.PK).JSG_Sequence);
				AssertEquals((ZByte)2, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress3.PK).JSG_Sequence);
			}

			{
				var newGatewayInfos = new[] { CreateGatewayInfo(addressDataObject2, 0), CreateGatewayInfo(addressDataObject1, 1) };
				var newReader = new ShipmentGatewayInfoCollectionReader(newGatewayInfos, shipment, new DummyLogger(), Factory);
				newReader.ReadIntoCollection();

				AssertEquals("addr3 was removed", 2, shipment.Gateways.Count);
				AssertEquals("addr2 has changed to index 0", (ZByte)0, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress2.PK).JSG_Sequence);
				AssertEquals("addr1 has changed to index 1", (ZByte)1, shipment.Gateways.First(shipmentGateway => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress1.PK).JSG_Sequence);
			}
		}

		public void TestRemoveFromCollection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var gateway = shipment.Gateways.AddNew();
			gateway.JSG_OA_ForwarderAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Address1, "1 GATEWAY DRIVE")).PK;
			Factory.SaveForTesting();

			var gatewayInfos = new[] { CreateGatewayInfo(addressDataObject1, 2), CreateGatewayInfo(addressDataObject2, 3) };
			var reader = new ShipmentGatewayInfoCollectionReader(gatewayInfos, shipment, new DummyLogger(), Factory);
			reader.ReadIntoCollection();

			AssertNoExceptionThrown("Removed Gateway should not throw ZSaveException", () => Factory.SaveForTesting());

			AssertEquals("Gateway Sequence should match XML", (byte)2, shipment.Gateways[0].JSG_Sequence);
			AssertEquals("Gateway Sequence should not differ from XML", (byte)3, shipment.Gateways[1].JSG_Sequence);
			AssertEquals(2, shipment.Gateways.Count);
		}

		GatewayInfo CreateGatewayInfo(OrganizationAddress forwarder, ZByte order)
		{
			return new GatewayInfo
			{
				Order = order,
				Forwarder = forwarder
			};
		}
	}
}
