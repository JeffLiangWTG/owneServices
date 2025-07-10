using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ForwardingShipmentDeliveryOrderProviderTest : DataProviderTestCase<IDeliveryOrderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ForwardingShipmentDeliveryOrderProvider(null));
			AssertNotNull(Provider);
		}

		public void TestMessageReferenceNumber()
		{
			AssertEquals("100000007", Provider.MessageReferenceNumber);
			Provider.MessageReferenceNumber = ZString.Empty;
			AssertNullOrEmpty(shipment.JS_DLO);
		}

		public void TestBusinessObject()
		{
			AssertType<ForwardingShipment>("BusinessObject must be of the expected type", Provider.BusinessObject);
			AssertSame("Shipment must be the same as the original one", shipment, Provider.BusinessObject);
		}

		public void TestFactory()
		{
			AssertType<BusinessObjectFactory>("Factory must be of the expected type", Provider.Factory);
			AssertSame("Factory must be the same as of the original shipment", shipment.Factory, Provider.Factory);
		}

		public void TestMessages()
		{
			AssertType<EDIMessageCollection>("Messages must be of the expected type", Provider.Messages);
			AssertSame("Messages must be the same as of the original shipment", shipment.Messages, Provider.Messages);
		}

		public void TestDeliveryOrderProvider()
		{
			AssertType<ForwardingShipmentDeliveryOrderProvider>("DeliveryOrderProvider type is ForwardingShipmentDeliveryOrderProvider", shipment.DeliveryOrderProvider);
		}

		protected override IDeliveryOrderProvider GetProvider() => new ForwardingShipmentDeliveryOrderProvider(shipment);

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_DLO = "100000007";
		}

		ForwardingShipment shipment;
	}
}
