using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public abstract class AgencyShipmentVisualizableDocumentSupporterTest<T> : TestCaseWithFactory
		where T : AgencyShipment
	{
		public void TestCustomizeFormCheckpoint()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertEquals(Env.Security.None, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetBusinessObjectInAnotherFactory()
		{
			var shipment = Factory.NewWithValidTestData<T>();
			var dummBizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var supporter = GetVisualizableDocumentSupporter(shipment);
			var differentFactory = new BusinessObjectFactory();

			var shipmentInAnotherFactory = supporter.GetBusinessObjectInAnotherFactory(differentFactory, shipment);
			AssertNotEquals(shipment, shipmentInAnotherFactory);
			AssertType<T>(shipmentInAnotherFactory);
			AssertEquals(differentFactory, ((BusinessObject)shipmentInAnotherFactory).Factory);
			AssertEquals(shipment.PK, shipment.PK);

			AssertNull(supporter.GetBusinessObjectInAnotherFactory(differentFactory, dummBizO));
			AssertNull(supporter.GetBusinessObjectInAnotherFactory(null, null));
		}

		public void TestGetAdditionalData()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			var additionalData = supporter.GetAdditionalData(null, null);
			Assert(additionalData.IsRight);
			AssertNull(additionalData.Right);
		}

		public void TestGetCustomCommands()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertEquals(0, supporter.GetCustomCommands(string.Empty).Count());
		}

		public void TestGetEventParent_Null()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertNull(supporter.GetEventParent(null));
		}

		public void TestGetEventParent()
		{
			var shipment = Factory.NewWithValidTestData<T>();

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.JDD_ParentTableCode] = shipment.TablePrefix;
			documentData[JobDocumentDataSchema.JDD_ParentID] = shipment.PK;
			documentData[JobDocumentDataSchema.JDD_Name] = "Test Document";
			Factory.Save();

			var supporter = GetVisualizableDocumentSupporter(shipment);

			var xmlEventValueObject = new Mock<IXmlEventValueObject>();
			xmlEventValueObject.Setup(x => x.DataContext.DocumentaryOverride.DocumentName).Returns("Test Document");

			AssertEquals(documentData, supporter.GetEventParent(xmlEventValueObject.Object));
		}

		public void TestGetLibraries()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertEquals(0, supporter.GetLibraries(string.Empty).Count());
		}

		public void TestGetLibraries_UXML()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			var libraries = supporter.GetLibraries(DocDataObjects.DataContext.UXML);
			AssertEquals(1, libraries.Count());
			Assert(libraries.First() is FreightLibrary);
		}

		public void TestGetMessageEventsProcessor()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertNull(supporter.GetMessageEventsProcessor(null));
		}

		public void TestGetMessageLogCreator()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertNull(supporter.GetMessageLogCreator(null));
		}

		public void TestGetMessagingExtensions()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			AssertNull(supporter.GetMessagingExtensions(null, null));
		}

		public void TestGetDocDataObject()
		{
			var shipment = Factory.NewWithValidTestData<T>();
			Factory.Save();

			var supporter = GetVisualizableDocumentSupporter(shipment);
			AssertEquals("DataContext '' is not supported.", supporter.GetDocDataObject(shipment, string.Empty, null).Left);
		}

		public void TestGetUniversalXmlDataObject()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);
			var expectedShipment = new Shipment();

			var document = new Mock<IDocument>();
			var writter = new Mock<IAgencyDocDataObjectUXmlWriter>();
			writter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document.Object, MessageType.Unspecified)).Returns(expectedShipment);
			ObjectFactory.Substitute("IAgencyDocDataObjectUXmlWriter", writter.Object);

			var dataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.Instance, document.Object, MessageType.Unspecified);
			AssertEquals(dataObject, expectedShipment);
		}

		public void TestGetUniversalXmlDataObject_Null()
		{
			var shipment = Factory.New<T>();
			var supporter = GetVisualizableDocumentSupporter(shipment);

			var dataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.Instance, null, MessageType.Unspecified);
			AssertEquals("Cannot produce Universal XML for .", dataObject.Left);
		}

		public abstract AgencyShipmentVisualizableDocumentSupporter<T> GetVisualizableDocumentSupporter(T shipment);
	}
}
