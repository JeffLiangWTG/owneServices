using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Moq;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentGatewayInfoDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		readonly OrganizationAddress organizationAddressDataObject = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ArrivalCFSAddress);
		OrgAddress orgAddress;

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress = new OrganisationDataObjectReader(organizationAddressDataObject, new DummyLogger(), Factory).GetMatchedOrNewForTesting();
		}

		public void TestForwarderIsPopulated()
		{
			var mockDataWriter = new Mock<IDataWritingManager>();
			mockDataWriter.SetupGet(t => t.WriterStrategy).Returns(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new ShipmentGatewayInfoDataObjectWriter(mockDataWriter.Object);

			var shipmentGateway = Factory.NewWithValidTestData<ShipmentGateway>();
			shipmentGateway.JSG_OA_ForwarderAddress = orgAddress.PK;
			shipmentGateway.JSG_Sequence = 1;

			var gatewayInfo = writer.GetDataObject(shipmentGateway);
			AssertEquals("GatewayInfo.Forwarder", organizationAddressDataObject.Address1, gatewayInfo.Forwarder.Address1);
			AssertEquals("GatewayInfo.Order", 1, (int)gatewayInfo.Order);
		}

		public void TestForwarderIsNotPopulated_ForwarderAddressIsNull()
		{
			var mockDataWriter = new Mock<IDataWritingManager>();
			mockDataWriter.SetupGet(t => t.WriterStrategy).Returns(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new ShipmentGatewayInfoDataObjectWriter(mockDataWriter.Object);

			var shipmentGateway = Factory.New<ShipmentGateway>();

			var gatewayInfo = writer.GetDataObject(shipmentGateway);
			AssertNull("GatewayInfo should not have been created", gatewayInfo);
		}
	}
}
