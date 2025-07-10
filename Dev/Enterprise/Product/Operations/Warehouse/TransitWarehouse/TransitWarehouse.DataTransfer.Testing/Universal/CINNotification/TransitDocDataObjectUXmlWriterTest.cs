using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.Warehouse.Transit.DataTransfer;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class TransitDocDataObjectUXmlWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObject_CIN750In()
		{
			var notification = new CIN750InNotification("TransitReceive", "RC0000001");
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.Reference
			};
			notification.Goods = new List<DocPackingLine>();

			var writer = new TransitDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(notification, TransitDocDataContext.CIN750WarehouseIn), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_CIN750Out()
		{
			var notification = new CIN750OutNotification("TransitReceive", "DC0000001");
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.Reference
			};
			notification.Goods = new List<DocPackingLine>();

			var writer = new TransitDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(notification, TransitDocDataContext.CIN750WarehouseOut), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_CIN750Cor()
		{
			var notification = new CIN750CorNotification("TransitReceive", "RC0000001");
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.Reference
			};
			notification.Goods = new List<DocPackingLine>();

			var writer = new TransitDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(notification, TransitDocDataContext.CIN750WarehouseCor), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_CIN750Decons()
		{
			var notification = new CIN750DeconsNotification("TransitDispatch", "DC0000001");

			notification.GoodsPairs = new List<Tuple<DocPackingLine, DocPackingLine>>();
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.HouseAirWaybill
			};

			var writer = new TransitDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(notification, TransitDocDataContext.CIN750WarehouseDecons), MessageType.Unspecified) as UniversalShipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		public void TestGetDataObject_CIN750Cons()
		{
			var notification = new CIN750ConsNotification("TransitDispatch", "DC0000001");

			notification.FromGoods = new List<DocPackingLine>();
			notification.ToGoods = new DocPackingLine(ZGuid.NewZGuid());
			notification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = CIN750RefTypes.Codes.HouseAirWaybill
			};

			var writer = new TransitDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(notification, TransitDocDataContext.CIN750WarehouseCons), MessageType.Unspecified) as UniversalShipment;

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
