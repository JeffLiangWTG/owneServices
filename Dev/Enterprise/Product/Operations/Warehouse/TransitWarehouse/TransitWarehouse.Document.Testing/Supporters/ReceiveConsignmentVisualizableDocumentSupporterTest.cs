using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Moq;
using NUnit.Framework;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class ReceiveConsignmentVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		#region TestCustomizeFormCheckpoint

		public void TestCustomizeFormCheckpoint()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertEquals(Env.Security.WhsItemReceiveConsignment, supporter.CustomizeFormCheckpoint);
		}

		#endregion

		#region TestGetAdditionalData

		public void TestGetAdditionalData()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNotNull(supporter.GetAdditionalData(null, null));
		}

		#endregion

		#region TestGetCustomCommands

		public void TestGetCustomCommands()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertEquals(0, supporter.GetCustomCommands("").Count());
		}

		#endregion

		#region TestGetEventParent

		public void TestGetEventParent()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNull(supporter.GetEventParent(null));
		}

		#endregion

		#region TestGetLibraries

		public void TestGetLibraries()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertEquals(0, supporter.GetLibraries("").Count());
		}

		#endregion

		#region TestGetMessageEventsProcessor

		public void TestGetMessageEventsProcessor()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNull(supporter.GetMessageEventsProcessor(null));
		}

		#endregion

		#region TestGetMessageEventsProcessor

		public void TestGetMessageLogCreatorByInNotication()
		{
			TestGetMessageLogCreatorCore(TransitDocDataContext.CIN750WarehouseIn, typeof(CIN750InNotificationLogsCreator));
		}

		public void TestGetMessageLogCreatorByCorNotication()
		{
			TestGetMessageLogCreatorCore(TransitDocDataContext.CIN750WarehouseCor, typeof(CIN750CorNotificationLogsCreator));
		}

		public void TestGetMessageLogCreatorCore(string contextType, Type logsCreatorType)
		{
			var rcn = CreateGeneralRCN();
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(contextType);
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			var logCreater = supporter.GetMessageLogCreator(document.Object);
			AssertNotNull(logCreater);
			AssertType(logsCreatorType, logCreater);
		}

		#endregion

		#region TestGetMessageEventsProcessor

		public void TestGetMessagingExtensionsByInNotication()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNull(supporter.GetMessagingExtensions(null, null));

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			TestGetMessagingExtensionsCore(notification, TransitDocDataContext.CIN750WarehouseIn, typeof(CIN750InNotificationExtensions));
		}

		public void TestGetMessagingExtensionsByCorNotication()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNull(supporter.GetMessagingExtensions(null, null));

			var notification = new CIN750CorNotificationBuilder(rcn).Build();
			TestGetMessagingExtensionsCore(notification, TransitDocDataContext.CIN750WarehouseCor, typeof(CIN750CorNotificationExtensions));
		}

		void TestGetMessagingExtensionsCore(CIN750Notification notification, string dataContextType, Type extensionType)
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			AssertNull(supporter.GetMessagingExtensions(null, null));

			var dynamicData = new Mock<IDynamicData>();
			dynamicData.Setup(d => d.Value).Returns(notification);
			var document = new Mock<IDocument>();
			document.Setup(d => d.DataContext).Returns(dataContextType);
			document.Setup(d => d.Data).Returns(dynamicData.Object);

			var documentInstruction = new Mock<IMessageInstructions>();

			var extensions = supporter.GetMessagingExtensions(document.Object, documentInstruction.Object);
			AssertNotNull(extensions);
			AssertType(extensionType, extensions);
		}

		#endregion

		#region GetBusinessObjectInAnotherFactory

		public void TestGetBusinessObjectInAnotherFactory()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var dummBizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			var differentFactory = new BusinessObjectFactory();

			var consignmentInAnotherFactory = (WhsItemReceiveConsignment)supporter.GetBusinessObjectInAnotherFactory(differentFactory, consignment);
			AssertNotEquals("Created different instance of Consignment", consignment, consignmentInAnotherFactory);
			AssertEquals("Created Consignment in another factory", differentFactory, consignmentInAnotherFactory.Factory);
			AssertEquals("Created Consignment PK matches original", consignment.PK, consignmentInAnotherFactory.PK);

			AssertNull("Non Consignment types must return Null.", supporter.GetBusinessObjectInAnotherFactory(differentFactory, dummBizO));
			AssertNull("Passing null parameters should return Null.", supporter.GetBusinessObjectInAnotherFactory(null, null));
		}

		#endregion

		#region TestGetDocDataObject

		public void TestGetDocDataObject()
		{
			var dummyBizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			var docDataObjectForConsignment = supporter.GetDocDataObject(rcn, FRPortsGoodsReceivedCRESA, null);
			AssertNotNull(docDataObjectForConsignment);

			var docDataObjectForNonConsignment = supporter.GetDocDataObject(dummyBizO, FRPortsGoodsReceivedCRESA, null);
			AssertEquals($"DataContext '{FRPortsGoodsReceivedCRESA}' for Object Type {dummyBizO.GetType()} is not supported.", docDataObjectForNonConsignment);
			AssertExceptionThrown<NotSupportedException>("Null parent is not supported.", () => supporter.GetDocDataObject(null, FRPortsGoodsReceivedCRESA, null));
			AssertExceptionThrown<NotSupportedException>("Empty DataContext is not supported.", () => supporter.GetDocDataObject(dummyBizO, "", null));
		}

		#endregion

		#region TestGetUniversalXmlDataObject_InValidParameters

		public void TestGetUniversalXmlDataObject_InValidParameters()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			Assembly assembly = Assembly.LoadFile(Path.Combine(TestCase.ExecutableDirectory, "Enterprise.Freight.Forwarding.Documents.DataObjects.dll"));
			Type cresaType = assembly.GetType("Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Cresa");
			var ctor = cresaType.GetConstructors()[0];

			var cresa = (DocDataObject)ctor.Invoke(new object[] { new ZString("TransitReceive"), new ZString("RCN1") });
			var document = CreateMockDocument(cresa, FRPortsGoodsReceivedCRESA);
			AssertExceptionThrown<NotSupportedException>("Null Writter and Document is not supported.", () => supporter.GetUniversalXmlDataObject(null, null, MessageType.Unspecified));
			AssertExceptionThrown<NotSupportedException>("Null Document is not supported.", () => supporter.GetUniversalXmlDataObject(new DummyWritterStarategy(), null, MessageType.Unspecified));
			AssertExceptionThrown<NotSupportedException>("Null Writter is not supported.", () => supporter.GetUniversalXmlDataObject(null, document, MessageType.Unspecified));
		}

		public void TestGetUniversalXmlDataObject()
		{
			var rcn = CreateGeneralRCN();
			IVisualizableDocumentSupporter supporter = new ReceiveConsignmentVisualizableDocumentSupporter(rcn);
			Assembly assembly = Assembly.LoadFile(Path.Combine(TestCase.ExecutableDirectory, "Enterprise.Freight.Forwarding.Documents.DataObjects.dll"));
			Type cresaType = assembly.GetType("Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Cresa");
			var ctor = cresaType.GetConstructors()[0];
			var cresa = (DocDataObject)ctor.Invoke(new object[] { new ZString("TransitReceive"), new ZString("RCN1") });
			var mockDocument = CreateMockDocument(cresa, FRPortsGoodsReceivedCRESA);
			var writter = new Mock<IForwardingDocDataObjectUXmlWriter>();
			var expectedShipment = new Shipment();
			writter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, mockDocument, MessageType.Unspecified)).Returns(expectedShipment);
			ObjectFactory.Substitute("IForwardingDocDataObjectUXmlWriter", writter.Object);
			var dataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.Instance, mockDocument, MessageType.Unspecified);

			AssertEquals(dataObject, expectedShipment);
		}

		IDocument CreateMockDocument(DocDataObject docDataObject, string dataContext)
		{
			var data = docDataObject.MakeDynamic();
			var document = new Mock<IDocument>();
			document.Setup(d => d.Data).Returns(data);
			document.Setup(d => d.DataContext).Returns(dataContext);

			return document.Object;
		}

		class DummyWritterStarategy : IDataObjectWriterStrategy
		{
			bool IDataObjectWriterStrategy.IsAllowSet(string fieldName)
			{
				return false;
			}
		}

		WhsItemReceiveConsignment CreateGeneralRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;
			return rcn;
		}

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		const string FRPortsGoodsReceivedCRESA = "FRPortsGoodsReceivedCRESA";

		#endregion
	}
}
