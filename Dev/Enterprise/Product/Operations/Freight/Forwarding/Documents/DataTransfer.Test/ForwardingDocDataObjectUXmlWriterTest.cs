using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class ForwardingDocDataObjectUXmlWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObject_HouseBill()
		{
			var houseBill = new HouseBill("ForwardingShipment", "S00012");

			var writer = new ForwardingDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(houseBill, DataContext.HouseBill), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);

			houseBill.IsElectronicBOL = true;
			houseBill.IsDraft = false;
			writer = new ForwardingDocDataObjectUXmlWriter();
			dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(houseBill, DataContext.HouseBill), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_ShippingInstruction()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var carrierWrapper = new ShippingInstructionBuilder(consol).Build();

			var writer = new ForwardingDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(carrierWrapper, DataContext.ShippingInstruction), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_BookingRequest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters();
			var carrierWrapper = new BookingRequestBuilder(consol, parameters).Build();

			var writer = new ForwardingDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(carrierWrapper, DataContext.BookingRequest), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_BookingRequestForShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var parameters = new DummyDocDataObjectParameters();
			var seashipmentBookingrequest = new SeaShipmentBookingRequestBuilder(shipment, parameters).Build();

			var writer = new ForwardingDocDataObjectUXmlWriter();
			var dataObject =
				writer.GetDataObject(
					DefaultDataObjectWriterStrategy.Instance,
					CreateMockDocument(seashipmentBookingrequest, DataContext.BookingRequest),
					MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_DangerousGoodsNotification()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters();
			var carrierWrapper = new DangerousGoodsNotificationBuilder(consol, parameters).Build();

			var writer = new ForwardingDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(carrierWrapper, DataContext.BEDangerousGoodsNotification), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		IDocument CreateMockDocument(DocDataObject docDataObject, string dataContext)
		{
			var data = docDataObject.MakeDynamic();
			var document = new DummyDocument();
			document.Data = data;
			document.DataContext = dataContext;

			return document;
		}
	}
}
