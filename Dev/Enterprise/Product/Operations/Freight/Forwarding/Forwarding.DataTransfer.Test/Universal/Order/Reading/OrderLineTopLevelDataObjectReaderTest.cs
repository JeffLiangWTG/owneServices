using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderLineTopLevelDataObjectReaderTest : OrganizationAddressTestHelper
	{
		OrderLine CreateOrderLineWithLineForRestrictOrderLineImport(OrgHeader buyer, string orderNo)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = orderNo;
			order.JD_OrderNumberSplit = 1;
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			return orderLine;
		}

		static UniversalShipment CreateDataObjectForRestrictOrderLineImport(OrderLine orderLine)
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrderLine,
				string.Format("{0}~{1}~CRAHOLSYD~{2}~{3}", orderLine.Order.JD_OrderNumber, +orderLine.Order.JD_OrderNumberSplit, orderLine.JO_LineNo, orderLine.JO_SubLineNo));

			dataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());

			var orderLineData = new UniversalOrderLine()
			{
				LineNumber = orderLine.JO_LineNo,
				SubLineNumber = orderLine.JO_SubLineNo
			};
			dataObject.Order.OrderLineCollection.Add(orderLineData);
			return dataObject;
		}

		JobSupplierBooking CreateSupplierBookingWithLineFor(string bookingStatus, string bookingId, OrderLine orderLine)
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_Status = bookingStatus;
			supplierBooking.JSB_BookingId = bookingId;
			supplierBooking.SupplierBookingLines.AddNew().JSL_JO_OrderLine = orderLine.PK;

			return supplierBooking;
		}

		public void TestRestrictOrderLineImport_AttachedSupplierBookingWithAllStatus()
		{
			var buyer = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var bookingStatuses = typeof(Core.Constants.SupplierBookingStatus).GetFields();
			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				var bookingId = "SB001" + bookingStatus;

				var orderLine = CreateOrderLineWithLineForRestrictOrderLineImport(buyer, "ORD001" + bookingStatus);
				var supplierBooking = CreateSupplierBookingWithLineFor(bookingStatus, bookingId, orderLine);
				Factory.SaveForTesting();

				var dataObject = CreateDataObjectForRestrictOrderLineImport(orderLine);

				foreach (var allowAttachedOrderXMLUpdate in new[] { false, true })
				{
					buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = allowAttachedOrderXMLUpdate;
					Logger.ClearLogs();
					new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory).ReadIntoBusinessObject();
					AssertNullOrEmpty("Should not have error", Logger.GetErrors());
				}
			}
		}

		public void TestRestrictOrderLineImport_AttachedMultipleSupplierBookingWithAllStatus()
		{
			var buyer = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var bookingStatuses = typeof(Core.Constants.SupplierBookingStatus).GetFields();
			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				var bookingId = "SB001" + bookingStatus;

				var orderLine = CreateOrderLineWithLineForRestrictOrderLineImport(buyer, "ORD001" + bookingStatus);
				var supplierBooking1 = CreateSupplierBookingWithLineFor(bookingStatus, bookingId + "_1", orderLine);
				var supplierBooking2 = CreateSupplierBookingWithLineFor(bookingStatus, bookingId + "_2", orderLine);
				var supplierBooking3 = CreateSupplierBookingWithLineFor(bookingStatus, bookingId + "_3", orderLine);
				var supplierBooking4 = CreateSupplierBookingWithLineFor(Core.Constants.SupplierBookingStatus.Cancelled, bookingId + "_4", orderLine);
				Factory.SaveForTesting();

				var dataObject = CreateDataObjectForRestrictOrderLineImport(orderLine);

				foreach (var allowAttachedOrderXMLUpdate in new[] { false, true })
				{
					buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = allowAttachedOrderXMLUpdate;
					Logger.ClearLogs();
					new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory).ReadIntoBusinessObject();

					AssertNullOrEmpty("Should not have error", Logger.GetErrors());
				}
			}
		}

		public void TestTopLevelOrderLineMapping()
		{
			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			orderBO.JD_OrderNumber = "ORDER1";
			orderBO.JD_OrderNumberSplit = new ZByte(2);

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrderLine, "ORDER1~2~CRAHOLSYD~1~2");

			dataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());

			var orderLineData = new UniversalOrderLine()
			{
				LineNumber = 1,
				SubLineNumber = 2,
				LineComment = "BOOK"
			};
			dataObject.Order.OrderLineCollection.Add(orderLineData);

			var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
			var orderLineBO2 = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLineBO2);
			AssertEquals("BOOK", orderLineBO2.JO_Description);
		}

		public void TestTopLevelOrderLineMapping_WithOrderReferenceMatching()
		{
			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			orderBO.JD_OrderNumber = "ORDER1";
			orderBO.JD_OrderNumberSplit = new ZByte(2);
			orderBO.JD_MasterWaybill = "MASTERWAYBILL";
			orderBO.JD_Waybill = "WAYBILL";
			orderBO.JD_RL_NKGoodsAvailableAt = "AUSYD";
			orderBO.JD_RL_NKGoodsDeliveredTo = "NZAKL";

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrderLine, null);

			dataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order.OrderNumber = "ORDER1";
			dataObject.Order.OrderNumberSplit = 2;
			dataObject.WayBillNumber = "WAYBILL";
			dataObject.TransportMode = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair
			{
				Code = Core.Constants.TransportModes.Sea,
				Description = "Sea"
			};
			dataObject.PortOfOrigin = new UniversalDataBuss.DataObjects.Universal.UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDestination = new UniversalDataBuss.DataObjects.Universal.UNLOCO { Code = "NZAKL", Name = "Auckland" };

			dataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			var orderLineData = new UniversalOrderLine()
			{
				LineNumber = 1,
				SubLineNumber = 2,
				LineComment = "BOOK"
			};
			dataObject.Order.OrderLineCollection.Add(orderLineData);

			var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
			var orderLineBO2 = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLineBO2);
			AssertEquals(orderLineBO.PK, orderLineBO2.PK);
			AssertEquals("BOOK", orderLineBO2.JO_Description);
		}

		public void TestTopLevelOrderLineMapping_WithOrderLineLineReferenceMatching()
		{
			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			orderBO.JD_OrderNumber = "ORDER1";

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 10;
			orderLineBO.JO_LineReference = "LINEREF1234";
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrderLine, null);

			dataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order.OrderNumber = "ORDER1";
			dataObject.TransportMode = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair
			{
				Code = Core.Constants.TransportModes.Sea,
				Description = "Sea"
			};
			dataObject.PortOfOrigin = new UniversalDataBuss.DataObjects.Universal.UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDestination = new UniversalDataBuss.DataObjects.Universal.UNLOCO { Code = "NZAKL", Name = "Auckland" };

			dataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			var orderLineData = new UniversalOrderLine()
			{
				LineNumber = 2,
				SubLineNumber = 20,
				LineReference = "LINEREF1234",
				LineComment = "BOOK",
			};
			dataObject.Order.OrderLineCollection.Add(orderLineData);

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
				var orderLineBO2 = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLineBO2);
				AssertEquals(orderLineBO.PK, orderLineBO2.PK);
				AssertEquals(orderLineBO.JO_LineReference, orderLineBO2.JO_LineReference);
				AssertEquals("BOOK", orderLineBO2.JO_Description);
				AssertEquals(1, orderLineBO2.JO_LineNo);
				AssertEquals(10, orderLineBO2.JO_SubLineNo);
			}

			orderLineData.LineReference = null;
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
				var orderLineBO3 = reader.ReadIntoBusinessObject();

				AssertNull(orderLineBO3);
			}

			orderLineData.LineReference = "";
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
				var orderLineBO4 = reader.ReadIntoBusinessObject();

				AssertNull(orderLineBO4);
			}

			orderLineData.LineNumber = 1;
			orderLineData.SubLineNumber = 10;
			orderLineData.LineReference = "";
			orderLineData.LineComment = "LEMON";

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
				var orderLineBO5 = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLineBO5);
				AssertEquals(orderLineBO.PK, orderLineBO5.PK);
				AssertEquals("", orderLineBO5.JO_LineReference);
				AssertEquals("LEMON", orderLineBO5.JO_Description);
			}

			orderLineData.LineReference = "NotLINEREF1234";
			orderLineData.LineComment = "APPLE";

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderLineTopLevelDataObjectReader(dataObject, Logger, Factory);
				var orderLineBO5 = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLineBO5);
				AssertEquals(orderLineBO.PK, orderLineBO5.PK);
				AssertEquals("APPLE", orderLineBO5.JO_Description);
				AssertEquals("NotLINEREF1234", orderLineBO5.JO_LineReference);
			}
		}
	}
}
