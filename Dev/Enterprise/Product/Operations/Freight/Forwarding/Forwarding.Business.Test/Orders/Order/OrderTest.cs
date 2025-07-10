using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	public class OrderTest : BaseFreightTest
	{
		public void TestSetOrderToDelivered()
		{
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Delivered);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Delivered);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Delivered, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Confirmed, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Confirmed);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(Constants.OrderStatus.Cancelled, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Cancelled, Constants.OrderStatus.Delivered);
			TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(string.Empty, string.Empty, string.Empty, Constants.OrderStatus.Confirmed);
		}

		void TestSetOrderToDeliveredIfOrderLinesIsNotEmpty(string line1Status, string line2Status, string line3Status, string expectedOrderStatus)
		{
			var order = Factory.New<Order>();
			order.JD_OrderStatus = Constants.OrderStatus.Confirmed;

			foreach (var lineStatus in new string[] { line1Status, line2Status, line3Status }.Where(status => !string.IsNullOrEmpty(status)))
			{
				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_LineStatus = lineStatus;
			}

			order.SetOrderToDeliveredIfOrderLinesDelivered();
			AssertEquals(expectedOrderStatus, order.JD_OrderStatus);
		}

		public void TestCanAttachOrderToShipmentWithNonMatchingControllingCustomer()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var order = Factory.New<Order>();
				var shipment = Factory.New<ForwardingShipment>();
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_Code = "CCM";
				var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
				controllingCustomerAddress.OA_OH = controllingCustomer.PK;
				controllingCustomerAddress.OA_Code = "CCM";

				Assert(order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				shipment.ControllingCustomerAddress.E2_OA_Address = ZGuid.Empty;
				Assert(!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
				Assert(order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = ZGuid.Empty;
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
				Assert(!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				order.ControllingCustomerDocAddress.E2_AddressOverride = true;
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
				Assert(!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				order.ControllingCustomerDocAddress.E2_AddressOverride = true;
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
				shipment.ControllingCustomerAddress.E2_AddressOverride = true;
				Assert(!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));

				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				order.ControllingCustomerDocAddress.E2_AddressOverride = false;
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
				shipment.ControllingCustomerAddress.E2_AddressOverride = true;
				Assert(!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out _));
			}
		}

		public void TestRequiredDocumentAddedForImportDirection()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var task = BO.WorkflowItems.Milestones.AddNew();
			task.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today);
			using (task.TemporarilyAllowSettingCondition("P9_SE_NKMilestoneEvent"))
			{
				task.P9_SE_NKMilestoneEvent = Events.Departure.Code;
			}

			var validToDate = ZDate.Today.AddDays(5);
			var requiredDocument1 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001", ZGuid.Empty);
			var requiredDocument2 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002", BO.SupplierPK);
			var requiredDocument3 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.DeliveryOrder, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003", orgHeader.PK);

			var requiredDocument4 = AddNewJobRequiredDoc(BO.Supplier, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004", ZGuid.Empty);
			var requiredDocument5 = AddNewJobRequiredDoc(BO.Supplier, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005", BO.SupplierPK);
			var requiredDocument6 = AddNewJobRequiredDoc(BO.Supplier, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0006", orgHeader.PK);
			Factory.Save();

			var agentsInvoiceDocuments = BO.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = BO.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);

			AssertEquals("declaration should have 2 AgentsInvoice required documents added.", 2, agentsInvoiceDocuments.Count());
			AssertEquals("declaration should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("2 AgentsInvoice required documents added were from supplier/importer relationship.", "0001,0002", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from importer.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		public void TestRequiredDocumentAddedForSupplierLink()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var task = BO.WorkflowItems.Milestones.AddNew();
			task.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today);
			using (task.TemporarilyAllowSettingCondition("P9_SE_NKMilestoneEvent"))
			{
				task.P9_SE_NKMilestoneEvent = Events.Departure.Code;
			}

			var validToDate = ZDate.Today.AddDays(5);
			var requiredDocument1 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001", ZGuid.Empty);
			var requiredDocument2 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002", BO.SupplierPK);
			var requiredDocument3 = AddNewJobRequiredDoc(BO.Buyer, Constants.RefDocTypes.DeliveryOrder, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003", orgHeader.PK);

			var supplierLink = BO.Buyer.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = BO.SupplierPK;
			var requiredDocument4 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004", ZGuid.Empty);
			var requiredDocument5 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005", BO.SupplierPK);
			var requiredDocument6 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0006", orgHeader.PK);
			Factory.Save();

			var agentsInvoiceDocuments = BO.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = BO.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);

			AssertEquals("declaration should have 3 AgentsInvoice required documents added.", 3, agentsInvoiceDocuments.Count());
			AssertEquals("declaration should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("3 AgentsInvoice required documents added were from supplier/importer relationship.", "0004,0005,0006", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from importer.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		#region TestCorrectDocumentReceivedEventLogging

		public void TestNoAIDEventLoggingOnOrderWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			BO.JD_RL_NKPortOfLoading = "SGSIN";
			BO.JD_RL_NKPortOfDischarge = "AUBNE";

			var requiredDocument = BO.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = BO.Logs.GetAllLogs();
			AssertNull("AID event should not be logged", BO.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestAIDEventLoggingOnOrder()
		{
			BO.JD_RL_NKPortOfLoading = "SGSIN";
			BO.JD_RL_NKPortOfDischarge = "AUBNE";

			var requiredDocument = BO.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = BO.Logs.GetAllLogs();
			AssertNotNull("AID event should be logged", BO.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestNoAEDEventLoggingOnOrderWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			BO.JD_RL_NKPortOfLoading = "SGSIN";
			BO.JD_RL_NKPortOfDischarge = "AUBNE";

			var requiredDocument = BO.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = BO.Logs.GetAllLogs();
			AssertNull("AED event should not be logged", BO.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestAEDEventLoggingOnOrder()
		{
			BO.JD_RL_NKPortOfLoading = "SGSIN";
			BO.JD_RL_NKPortOfDischarge = "AUBNE";

			var requiredDocument = BO.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = BO.Logs.GetAllLogs();
			AssertNotNull("AED event should be logged", BO.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		#endregion

		JobRequiredDocument AddNewJobRequiredDoc(IHaveRequiredDocuments org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDate date, ZString docNumber, ZGuid documentOwner)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			result.EQ_OH_DocumentOwner = documentOwner;
			return result;
		}

		public void TestPortDefaultingFromBuyerConsignorRelationship()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AUSBUYER";
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "NZSUPPLIER";
			supplier.OH_RL_NKClosestPort = "NZAKL";

			Order order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = supplier.PK;

			AssertEquals("No Supplier/Buyer Link: Load should default to supplier's UNLOCO", "NZAKL", order1.JD_RL_NKPortOfLoading);
			AssertEquals("No Supplier/Buyer Link: Origin should default to supplier's UNLOCO", "NZAKL", order1.JD_RL_NKGoodsAvailableAt);
			AssertEquals("No Supplier/Buyer Link: Discharge should default to buyer's UNLOCO", "AUSYD", order1.JD_RL_NKPortOfDischarge);
			AssertEquals("No Supplier/Buyer Link: Destination should default to buyer's UNLOCO", "AUSYD", order1.JD_RL_NKGoodsDeliveredTo);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKLoadPort = "NZWLG";
			linkTrnMode.PF_RL_NKPlaceOfReceivalPort = "NZCHC";
			linkTrnMode.PF_RL_NKDischargePort = "AUBNE";
			linkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "AUMEL";

			Factory.Save();

			Order order2 = Factory.New<Order>();
			order2.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order2.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order2.BuyerPK = buyer.PK;
			order2.SupplierPK = supplier.PK;

			AssertEquals("Load should default from SupplierBuyerLink", "NZWLG", order2.JD_RL_NKPortOfLoading);
			AssertEquals("Origin should default from SupplierBuyerLink", "NZCHC", order2.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Discharge should default from SupplierBuyerLink", "AUBNE", order2.JD_RL_NKPortOfDischarge);
			AssertEquals("Destination should default from SupplierBuyerLink", "AUMEL", order2.JD_RL_NKGoodsDeliveredTo);
		}

		public void TestGetNextOrderNumber()
		{
			var firstBuyer = Factory.NewWithValidTestData<OrgHeader>();
			firstBuyer.OH_Code = "BUYER";
			firstBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = false;
			firstBuyer.MiscServ.OM_IMLastOrderReference = "ORD";

			var order1 = Factory.New<Order>();
			order1.BuyerPK = firstBuyer.PK;

			Factory.Save();
			AssertEquals("should default to autogenerated order number", "P000001", order1.JD_OrderNumber);

			firstBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			var order2 = Factory.New<Order>();
			order2.BuyerPK = firstBuyer.PK;

			Factory.Save();
			AssertEquals("ORD1", order2.JD_OrderNumber);

			var secondBuyer = Factory.NewWithValidTestData<OrgHeader>();
			secondBuyer.OH_Code = "BUYER2";
			secondBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			secondBuyer.MiscServ.OM_IMLastOrderReference = "ORD1-000000A";

			var order4 = Factory.New<Order>();
			order4.BuyerPK = secondBuyer.PK;
			Factory.Save();
			AssertEquals("ORD1-000001A", order4.JD_OrderNumber);

			var order5 = Factory.New<Order>();
			order5.BuyerPK = secondBuyer.PK;
			Factory.Save();
			AssertEquals("ORD1-000002A", order5.JD_OrderNumber);

			var thirdBuyer = Factory.NewWithValidTestData<OrgHeader>();
			thirdBuyer.OH_Code = "BUYER3";
			thirdBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			thirdBuyer.MiscServ.OM_IMLastOrderReference = "ORD99ER";

			var order6 = Factory.New<Order>();
			order6.BuyerPK = thirdBuyer.PK;
			Factory.Save();
			AssertEquals("ORD100ER", order6.JD_OrderNumber);

			var fourthBuyer = Factory.NewWithValidTestData<OrgHeader>();
			fourthBuyer.OH_Code = "BUYER4";
			fourthBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			fourthBuyer.MiscServ.OM_IMLastOrderReference = "7599999999999999999999999999";

			var order7 = Factory.New<Order>();
			order7.BuyerPK = fourthBuyer.PK;
			Factory.Save();
			AssertEquals("should default to auto generated order number as order is longer than a long", "1", order7.JD_OrderNumber);

			var fifthBuyer = Factory.NewWithValidTestData<OrgHeader>();
			fifthBuyer.OH_Code = "BUYER5";
			fifthBuyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			firstBuyer.MiscServ.OM_IMLastOrderReference = "";

			var order8 = Factory.New<Order>();
			order8.BuyerPK = firstBuyer.PK;

			Factory.Save();
			AssertEquals("P000002", order8.JD_OrderNumber);
		}

		public void TestAttachedShipment()
		{
			var order = Factory.New<Order>();
			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;

			AssertEquals("Shipment should be loaded", shipment, order.Shipment);
			AssertNull("No quoted booking should be loaded", order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", true, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", false, order.IsBookingAttached);

			var shipment2 = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment2.PK;

			AssertEquals("Shipment should be loaded", shipment2, order.Shipment);
			AssertNull("No quoted booking should be loaded", order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", true, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", false, order.IsBookingAttached);
		}

		public void TestAttachedBookingWithQuote()
		{
			var order = Factory.New<Order>();

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var bookingWithQuote = quotedBookingBuilder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			order.JD_JS = bookingWithQuote.ForwardingShipment.PK;

			AssertEquals("Shipment should be loaded", bookingWithQuote.ForwardingShipment, order.Shipment);
			AssertEquals("Quoted booking should be loaded", bookingWithQuote, order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", false, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", true, order.IsBookingAttached);

			var bookingWithQuote2 = quotedBookingBuilder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			order.JD_JS = bookingWithQuote2.ForwardingShipment.PK;

			AssertEquals("Shipment should be loaded", bookingWithQuote2.ForwardingShipment, order.Shipment);
			AssertEquals("Quoted booking should be loaded", bookingWithQuote2, order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", false, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", true, order.IsBookingAttached);
		}

		public void TestAttachedBooking()
		{
			var order = Factory.New<Order>();

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quickBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory);

			order.JD_JS = quickBooking.ForwardingShipment.PK;
			AssertEquals("Shipment should be loaded", quickBooking.ForwardingShipment, order.Shipment);
			AssertEquals("Quoted booking should be loaded", quickBooking, order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", false, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", true, order.IsBookingAttached);

			var quickBooking2 = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory);
			order.JD_JS = quickBooking2.ForwardingShipment.PK;

			AssertEquals("Shipment should be loaded", quickBooking2.ForwardingShipment, order.Shipment);
			AssertEquals("Quoted booking should be loaded", quickBooking2, order.QuotedBooking);
			AssertEquals("IsForwardingShipmentAttached", false, order.IsForwardingShipmentAttached);
			AssertEquals("IsBookingAttached", true, order.IsBookingAttached);
		}

		public void TestJD_VB_ThatAutoUpdatesBookingDetailsFromOrder()
		{
			var order = Factory.New<Order>();

			AssertEquals("no quoted booking attached", ZGuid.Empty, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder);

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quickBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory);

			order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder = quickBooking.ViewPK;

			AssertEquals("JD_VB_ThatAutoUpdatesBookingDetailsFromOrder returns attached quick booking PK", quickBooking.ViewPK, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder);

			order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder = ZGuid.Empty;

			AssertEquals("JD_VB_ThatAutoUpdatesBookingDetailsFromOrder returns empty guid", ZGuid.Empty, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder);
		}

		public void TestAttachedShipmentReadonlyness()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);

			var order = Factory.NewWithValidTestData<Order>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = order.JD_IncoTerm;
			shipment.JS_RS_NKServiceLevel = order.JD_RS_NKServiceLevel_NI;

			Factory.Save();

			AssertEquals("no shipment/booking attached; JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly);

			AssertEquals("no shipment/booking attached; JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly);

			order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			Factory.Save();

			AssertEquals("shipment attached; JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly);

			AssertEquals("shipment attached; JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly should be readonly",
				true, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly);
		}

		public void TestAttachedBookingReadonlyness()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);

			var order = Factory.NewWithValidTestData<Order>();

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quickBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory);
			var shipment = (ForwardingShipment)quickBooking.ForwardingShipment;
			shipment.JS_INCO = order.JD_IncoTerm;
			shipment.JS_RS_NKServiceLevel = order.JD_RS_NKServiceLevel_NI;

			Factory.Save();

			AssertEquals("no shipment/booking attached; JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly);

			AssertEquals("no shipment/booking attached; JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly);

			order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder = quickBooking.ViewPK;
			Factory.Save();

			AssertEquals("booking attached; JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly should be readonly",
				true, order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly);

			AssertEquals("booking attached; JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly should not be readonly",
				false, order.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly);
		}

		public void TestGetNextOrderNumber_AutoGenerateOrderNumber()
		{
			var buyerA = Factory.NewWithValidTestData<OrgHeader>();
			buyerA.OH_Code = "BuyerA";
			buyerA.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			buyerA.MiscServ.OM_IMLastOrderReference = "A1";

			var buyerB = Factory.NewWithValidTestData<OrgHeader>();
			buyerB.OH_Code = "BuyerB";
			buyerB.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = false;
			buyerB.MiscServ.OM_IMLastOrderReference = "B1";

			var buyerC = Factory.NewWithValidTestData<OrgHeader>();
			buyerC.OH_Code = "BuyerC";
			buyerC.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = false;
			buyerC.MiscServ.OM_IMLastOrderReference = "C1";

			var order1 = Factory.New<Order>();
			order1.JD_OA_BuyerAddress = buyerA.MainAddress.PK;
			AssertEquals("order1's 'Order No.' = next order number of the buyer1's 'Last Order No.' (prior to saving the order)", "A2", order1.JD_OrderNumber);
			Factory.Save();
			AssertEquals("order1's 'Order No.' = buyerA's new 'Last Order No.'", order1.JD_OrderNumber, buyerA.MiscServ.OM_IMLastOrderReference);

			var order2 = Factory.New<Order>();
			order2.JD_OA_BuyerAddress = buyerA.MainAddress.PK;
			AssertEquals("order2's 'Order No.' = next order number of the buyer1's 'Last Order No.' (prior to saving the order)", "A3", order2.JD_OrderNumber);
			order2.JD_OA_BuyerAddress = buyerB.MainAddress.PK;
			AssertEquals("order2's 'Order No.' = order number before 'Buyer' is changed", "A3", order2.JD_OrderNumber);

			var order3 = Factory.New<Order>();
			order3.JD_OA_BuyerAddress = buyerB.MainAddress.PK;
			Factory.Save();
			AssertEquals("order3's 'Order No.' = autogenerated order number", "P000001", order3.JD_OrderNumber);
			order3.JD_OA_BuyerAddress = buyerC.MainAddress.PK;
			AssertEquals("order3's 'Order No.' = autogenerated order number", "P000001", order3.JD_OrderNumber);

			var order4 = Factory.New<Order>();
			order4.JD_OrderNumber = "INEEDTOSTAY";
			order4.JD_OA_BuyerAddress = buyerA.MainAddress.PK;
			AssertEquals("order's 'Order No.' = INEEDTOSTAY", "INEEDTOSTAY", order4.JD_OrderNumber);
		}

		public void TestAppendOrdersLineToinvoice()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "BUYER";

			var order = Factory.NewWithValidTestData<Order>();
			var line1 = order.OrderLines.AddNew();
			var line2 = order.OrderLines.AddNew();

			order.JD_InvoiceNumber = "AAA";
			order.JD_OA_SupplierAddress = supplier.MainAddress.PK;
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			order.JD_InvoiceDate = ZDateTime.Today;

			line1.JO_Quantity = 20m;
			line1.JO_LinePrice = 1000m;
			line1.JO_QtyReceived = 1m;

			line2.JO_Quantity = 40m;
			line2.JO_QtyReceived = 1m;
			line2.JO_LinePrice = 2000m;

			var invoiceHeader = Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "AAA";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			var invoiceLine1 = invoiceHeader.AddNewInvoiceLine();
			var invoiceLine2 = invoiceHeader.AddNewInvoiceLine();

			Factory.Save();

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();
			notifier.NextSelectedItemPK = invoiceHeader.PK;

			order.AppendOrdersLineToInvoice(notifier);

			AssertEquals("JI_JZ is set to InvoiceHeader's PK", invoiceHeader.PK, invoiceLine1.JI_JZ);
			AssertEquals("JI_JZ is set to InvoiceHeader's PK", invoiceHeader.PK, invoiceLine2.JI_JZ);
			AssertEquals("JZ_InvoiceAmount is set", 3000m, invoiceHeader.JZ_InvoiceAmount);

			ZQuery query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, invoiceHeader.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, order.PK);
			AssertEquals("InvoiceRelatedOrderGenPivots should be created", 1, Factory.Load<GenPivot>(query).Length);
		}

		#region Detaching PreAdvice

		public void TestDetachingPreAdviceWillDetachShipment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();

			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;
			order.JD_EF_ShipmentPrePlanning = preadvice.PK;

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotNull("Default behaviour: shipment wasn't detached", order.Shipment);

			order.JD_JS = shipment.PK;
			order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = false; };

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotNull("shipment will not be detached", order.Shipment);

			order.JD_JS = shipment.PK;
			order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = true; };

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotNull("Cancelled by user: shipment wasn't detached", order.Shipment);
		}

		public void TestDetachingPreAdviceWillDetachDeclaration()
		{
			var preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JE = declaration.PK;

			order.JD_EF_ShipmentPrePlanning = preadvice.PK;

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotNull("Default behaviour: declaration wasn't detached", order.Declaration);

			order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = false; };

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNull("Confirmed by user: declaration was detached as well", order.Declaration);

			order.JD_JE = declaration.PK;
			order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = true; };

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotNull("Cancelled by user: declaration wasn't detached", order.Declaration);
		}

		#endregion

		#region Detaching Shipment

		public void TestDetachingShipmentWillUpdateOrderStatusIfNecessary()
		{
			Order order = Factory.NewWithValidTestData<Order>();

			Action<string, string, string> assertStatusAfterDetachingShipment = (message, initialStatus, expectedStatus) =>
				{
					ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

					order.JD_JS = shipment.PK;
					order.JD_OrderStatus = initialStatus;
					AssertEquals("Precondition", initialStatus, order.JD_OrderStatus);

					order.JD_JS = ZGuid.Empty;
					Factory.Save();
					AssertEquals(message, expectedStatus, order.JD_OrderStatus);
				};

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			assertStatusAfterDetachingShipment("Registry is OFF, status not changed", Constants.OrderStatus.Delivered, Constants.OrderStatus.Delivered);

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			assertStatusAfterDetachingShipment("Registry is ON, status changed to INCOMPLETE", Constants.OrderStatus.Delivered, Constants.OrderStatus.Incomplete);
			assertStatusAfterDetachingShipment("Registry is ON, status changed to INCOMPLETE", Constants.OrderStatus.Shipped, Constants.OrderStatus.Incomplete);

			assertStatusAfterDetachingShipment("Status not changed", Constants.OrderStatus.PartDelivered, Constants.OrderStatus.PartDelivered);
			assertStatusAfterDetachingShipment("Status not changed", Constants.OrderStatus.Confirmed, Constants.OrderStatus.Confirmed);

			order.JD_BookingConfRef = "HELLO";
			assertStatusAfterDetachingShipment("Status changed to INCOMPLETE and then to CONFIRMED on saving", Constants.OrderStatus.Delivered, Constants.OrderStatus.Confirmed);
			assertStatusAfterDetachingShipment("Status changed to INCOMPLETE and then to CONFIRMED on saving", Constants.OrderStatus.Shipped, Constants.OrderStatus.Confirmed);
		}

		public void TestOrderLineLinksToPackProductsClearOnDetachingOrder()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			var line1 = order1.OrderLines.AddNew();
			var line2 = order1.OrderLines.AddNew();

			var order2 = Factory.NewWithValidTestData<Order>();
			var line3 = order2.OrderLines.AddNew();

			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			shipment.OnOrderLineToPackLineConversion += (sender, e) => e.ShouldCreatePacklines = true;
			shipment.AttachedOrders.AddRange(new[] { order1, order2 });
			shipment.CreatePackLinesFromOrderLines(new[] { order1, order2 });

			var product1 = shipment.OuterPackLines[0].Products.AddNew();
			var product2 = shipment.OuterPackLines[1].Products.AddNew();
			var product3 = shipment.OuterPackLines[2].Products.AddNew();

			product1.D2_JO = line1.PK;
			product2.D2_JO = line2.PK;
			product3.D2_JO = line3.PK;

			Assert(!product1.D2_JO.IsEmpty);
			Assert(!product2.D2_JO.IsEmpty);
			Assert(!product3.D2_JO.IsEmpty);

			shipment.AttachedOrders.RemoveFromRelationship(order1);

			Assert(product1.D2_JO.IsEmpty);
			Assert(product2.D2_JO.IsEmpty);
			Assert(!product3.D2_JO.IsEmpty);
		}

		public void TestUpdateTriggerDoesNotThrowException()
		{
			Order order = Factory.New<Order>();

			var trigger = order.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKLoadPort = "CNSZX";
			shipment.JS_RL_NKDischargePort = "AUSYD";
			shipment.Transports.AddNew();

			var atd = new ZDateTime(2020, 2, 2);
			shipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = atd;
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			order.JD_JE = declaration.PK;
			order.JD_JS = ZGuid.Empty;

			order.JD_JE = ZGuid.Empty;
			AssertNoExceptionThrown(() => order.JD_JS = shipment.PK);
		}

		public void TestProxyDatesWhenShipmentDetached()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBB";
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var transport = shipment.TransportsIncludingRelated.AddNew();

			transport.JW_ATD = new ZDateTime(2013, 06, 09);
			transport.JW_ATA = new ZDateTime(2013, 06, 12);
			transport.JW_ETD = new ZDateTime(2013, 06, 09);
			transport.JW_ETA = new ZDateTime(2013, 06, 11);

			var currentDate = ZDateTime.SmallDateTimeNow.ToOffset();
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "1";
			order1.SupplierPK = org2.PK;
			order1.BuyerPK = org1.PK;
			order1.UpdateEvent(Events.Arrival, currentDate);
			order1.UpdateEvent(Events.Departure, currentDate);
			order1.UpdateEventEstimate(Events.Arrival, currentDate);
			order1.UpdateEventEstimate(Events.Departure, currentDate);
			order1.UpdateEvent(Events.CustomsCommenced, currentDate);
			order1.UpdateEvent(Events.CustomsCleared, currentDate);
			order1.UpdateEvent(Events.CargoAvailable, currentDate);
			order1.UpdateEvent(Events.DeliveryCartageAdvised, currentDate);
			order1.UpdateEvent(Events.DeliveryCartageCompleteFinalised, currentDate);
			order1.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, currentDate);
			order1.JD_JS = shipment.PK;
			Factory.Save();

			order1.JD_JS = ZGuid.Empty;

			AssertEquals(ZDateTimeOffset.Empty, order1.WorkflowItems.Milestones["DEP"].P9_ScheduledDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, order1.WorkflowItems.Milestones["ARV"].P9_ScheduledDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["DCF"].P9_ScheduledDateForBinding);

			AssertEquals(ZDateTimeOffset.Empty, order1.WorkflowItems.Milestones["DEP"].P9_ActualDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, order1.WorkflowItems.Milestones["ARV"].P9_ActualDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["DCF"].P9_ActualDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["CAV"].P9_ActualDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["DCA"].P9_ActualDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["DCF"].P9_ActualDateForBinding);
			AssertEquals(currentDate, order1.WorkflowItems.Milestones["CCC"].P9_ActualDateForBinding);
		}

		public void TestIsAllowedToDetachShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";

			var order = Factory.NewWithValidTestData<Order>();
			shipment1.AttachedOrders.Add(order);
			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);

			Assert(!order.IsAllowedToDetachShipment);

			Factory.Save();
			Assert(order.IsAllowedToDetachShipment);

			shipment1.JS_GoodsDescription = "BOOKS";
			Assert(!order.IsAllowedToDetachShipment);

			Factory.Save();
			Assert(order.IsAllowedToDetachShipment);
		}

		public void TestJD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";

			var order = Factory.NewWithValidTestData<Order>();
			Assert(!order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrderInfo.ReadOnly);

			shipment1.AttachedOrders.Add(order);

			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);

			Assert(!order.IsAllowedToDetachShipment);
			Assert(order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrderInfo.ReadOnly);

			Factory.Save();
			Assert(order.IsAllowedToDetachShipment);
			Assert(!order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrderInfo.ReadOnly);
		}

		public void TestValidateShipmentWhenPreSave()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipment1.PK;

			var order = Factory.NewWithValidTestData<Order>();
			shipment1.AttachedOrders.Add(order);

			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);

			var shipmentNotifications = order.Shipment.Notifications;
			Assert(!shipmentNotifications.Contains("Warning - JS_ActualVolume: You have not entered a Shipment Volume."));

			order.RunPreSaveValidation();
			Assert(shipmentNotifications.Contains("Warning - JS_ActualVolume: You have not entered a Shipment Volume."));
		}

		public void TestDetachingPreAdviceWillDetachShipment_WithIsAllowedCheck()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";

			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment1.PK;
			order.JD_EF_ShipmentPrePlanning = preAdvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = false; };
			shipment1.AttachedOrders.Add(order);
			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);
			Assert(!order.IsAllowedToDetachShipment);

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotEquals("Shipment will not be detached", ZGuid.Empty, order.JD_JS);

			order.JD_JS = shipment1.PK;
			order.JD_EF_ShipmentPrePlanning = preAdvice.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = false; };
			Factory.Save();
			Assert(order.IsAllowedToDetachShipment);

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			Assert(!order.IsAllowedToDetachShipment);
			AssertNull("Pre-advice was detached", order.PreAdvice);
			AssertNotEquals("Shipment will not be detached", ZGuid.Empty, order.JD_JS);
		}

		public void TestJD_JS_ShouldNotThrow_WhenDetachingShipmentDefinedByInvalidNumber()
		{
			var order = Factory.NewWithValidTestData<Order>();

			AssertNoExceptionThrown(() =>
			{
				order.JD_JS = ZGuid.Invalid;
				order.JD_JS = ZGuid.Empty;
			});
		}

		#endregion

		#region Detaching Declaration

		public void TestDetachingDeclarationWouldRemoveItFromPreAdvice()
		{
			var preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JE = declaration.PK;

			order.JD_EF_ShipmentPrePlanning = preadvice.PK;

			order.JD_JE = Guid.Empty;
			AssertEquals("Removing an Order from a Declaration should also remove it from the attached Pre Advice.", Guid.Empty, order.JD_EF_ShipmentPrePlanning);
		}

		public void TestDetachingDeclarationRemovesOrderLineReferencesFromInvoiceLines()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var invoice = Factory.New<Enterprise.Integration.Customs.AU.IJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine1 = Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine1.JI_JZ = invoice.PK;
			var invoiceLine2 = Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine2.JI_JZ = invoice.PK;

			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			var orderLine2 = order.OrderLines.AddNew();

			order.JD_JE = declaration.PK;
			invoiceLine1.JI_JO = orderLine1.PK;
			invoiceLine2.JI_JO = orderLine2.PK;

			order.JD_JE = ZGuid.Empty;
			Assert("Order Line reference is removed from Invoice Line 1", invoiceLine1.JI_JO.IsEmpty);
			Assert("Order Line reference is removed from Invoice Line 2", invoiceLine2.JI_JO.IsEmpty);
		}

		#endregion

		public void TestNonCancelledOrderLines()
		{
			Order order = Factory.New<Order>();

			OrderLine orderLine1 = order.OrderLines.AddNew();
			Assert(order.NonCancelledOrderLines.Contains(orderLine1));

			orderLine1.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;
			Assert(!order.NonCancelledOrderLines.Contains(orderLine1));

			OrderLine orderLine2 = order.OrderLines.AddNew();
			Assert(order.NonCancelledOrderLines.Contains(orderLine2));
		}

		#region GetPossibleOrdersForAttachment_List

		public void TestGetPossibleOrdersForAttachment_List()
		{
			Order[] existingOrders = Factory.Load<Order>(new ZQuery());
			foreach (Order order in existingOrders)
			{
				order.Delete();
			}

			Func<bool, string, ZGuid, Order> createOrder = (isCancelled, transportMode, shipmentPK) =>
				{
					Order order = Factory.New<Order>();
					order.JD_IsCancelled = isCancelled;
					order.JD_TransportMode = transportMode;
					if (!shipmentPK.IsEmpty)
					{
						order.JD_JS = shipmentPK;
					}
					return order;
				};

			ForwardingShipment someShipment = Factory.New<ForwardingShipment>();

			Order airOrder1 = createOrder(false, Constants.TransportModes.Air, ZGuid.Empty);
			Order airOrder2 = createOrder(false, Constants.TransportModes.Air, ZGuid.Empty);
			Order attachedAirOrder = createOrder(false, Constants.TransportModes.Air, someShipment.PK);
			Order cancelledAirOrder = createOrder(true, Constants.TransportModes.Air, ZGuid.Empty);

			Order seaOrder = createOrder(false, Constants.TransportModes.Sea, ZGuid.Empty);
			Order attachedSeaOrder = createOrder(false, Constants.TransportModes.Sea, someShipment.PK);
			Order cancelledSeaOrder = createOrder(true, Constants.TransportModes.Sea, ZGuid.Empty);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "";
			AssertContainsExactElementsInAnyOrder(new Order[] { airOrder1, airOrder2, seaOrder }, Order.GetPossibleOrdersForAttachment_List(shipment));

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertContainsExactElementsInAnyOrder(new Order[] { airOrder1, airOrder2 }, Order.GetPossibleOrdersForAttachment_List(shipment));

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder(new Order[] { seaOrder }, Order.GetPossibleOrdersForAttachment_List(shipment));

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertContainsExactElementsInAnyOrder(Array.Empty<Order>(), Order.GetPossibleOrdersForAttachment_List(shipment));

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.New<OrgHeader>().PK;

			OrderCollection collection = Order.GetPossibleOrdersForAttachment_List(shipment);
			AssertHasDefault(collection, OrdersFilterProvider.FilterNames.TransportMode, "Property", (ZString)Constants.TransportModes.Air);
			AssertHasDefault(collection, OrdersFilterProvider.FilterNames.BuyerSupplier, "Property1", shipment.ConsigneePK);
			AssertHasDefault(collection, OrdersFilterProvider.FilterNames.BuyerSupplier, "Property2", shipment.ConsignorPK);
			AssertHasDefault(collection, OrdersFilterProvider.FilterNames.AttachedUnattachedOrders, "Property", (ZString)"All");

			AssertNoExceptionThrown(delegate
			{
				AssertNull("Passing null? Getting null, thank me it's not an exception thrown in your face.", Order.GetPossibleOrdersForAttachment_List(null));
			});
		}

		public void TestGetPossibleOrdersForAttachment_ListRelationshipFilter()
		{
			Order order1 = BO;
			order1.JD_IsCancelled = false;
			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_IsCancelled = true;

			Factory.Save();

			var orderParent = new Mock<IAttachOrders>();

			orderParent.Setup(m => m.Factory).Returns(Factory);//.Repeat.Any();
			orderParent.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Air);//.Repeat.Any();
			orderParent
				.SetupSequence(m => m.ConsigneeDocumentaryAddress)
				.Returns(Factory.New<JobDocAddress>())
				.Returns(Factory.New<JobDocAddress>())
				.CallBase();

			var collection = Order.GetPossibleOrdersForAttachment_List(orderParent.Object);
			AssertContainsExactElementsInAnyOrder(new[] { order1 }, Factory.Load<Order>(collection.Relationship.RelationshipFilter));
			orderParent.VerifyAll();
		}

		public void TestAllowAttachOrdersWithNonMatchingControllingCustomer()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var order = Factory.New<Order>();
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_Code = "CCM";
				var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
				controllingCustomerAddress.OA_OH = controllingCustomer.PK;
				controllingCustomerAddress.OA_Code = "CCM";
				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;

				var collection = Order.GetPossibleOrdersForAttachment_List(shipment);
				AssertContainsFilterNotMetError(collection, order, "You do not have security rights to allow order and its shipment have non-matching Controlling Customers.");
			}
		}

		public void TestGetPossibleOrdersForAttachment_ListNotificationWhenAdditionalFilterNotMet()
		{
			var orderParent = new Mock<IAttachOrders>();

			orderParent.Setup(m => m.Factory).Returns(Factory);
			orderParent.Setup(m => m.TransportMode).Returns(ZString.Empty);
			orderParent.Setup(m => m.TableName);
			orderParent.Setup(m => m.ConsigneeDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			orderParent.Setup(m => m.ConsignorDocumentaryAddress).Returns(Factory.New<JobDocAddress>());

			var collection = Order.GetPossibleOrdersForAttachment_List(orderParent.Object);
			var order = Factory.New<Order>();
			order.JD_JS = ZGuid.NewZGuid();
			AssertContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as it is already attached to a Shipment.");

			orderParent.Verify(m => m.TableName, Times.Never);
			orderParent.Verify(m => m.ConsigneeDocumentaryAddress, Times.Once);

			orderParent.Setup(m => m.Factory).Returns(Factory);
			orderParent.Setup(m => m.TransportMode).Returns(Core.Constants.TransportModes.Air);
			orderParent.Setup(m => m.TableName).Returns("Panani");
			orderParent.Setup(m => m.ConsigneeDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			orderParent.Setup(m => m.ConsignorDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			collection = Order.GetPossibleOrdersForAttachment_List(orderParent.Object);
			order = Factory.New<Order>();
			order.JD_JS = ZGuid.NewZGuid();
			AssertContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as it is already attached to a Shipment.");

			orderParent.Verify(m => m.ConsigneeDocumentaryAddress, Times.Exactly(2));

			order.JD_JS = ZGuid.Empty;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as the Transport Mode is different to that of the Panani.");

			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertNotContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as the Transport Mode is different to that of the Panani.");
			orderParent.Setup(m => m.Factory).Returns(Factory);
			orderParent.Setup(m => m.TransportMode).Returns(ZString.Empty);
			orderParent.Setup(m => m.TableName);
			orderParent.Setup(m => m.ConsigneeDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			orderParent.Setup(m => m.ConsignorDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			collection = Order.GetPossibleOrdersForAttachment_List(orderParent.Object);
			order = Factory.New<Order>();
			order.JD_OrderStatus = Constants.OrderStatus.Cancelled;
			AssertContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as it has been canceled.");

			orderParent.Verify(m => m.TableName, Times.Exactly(2));
			orderParent.Verify(m => m.ConsigneeDocumentaryAddress, Times.Exactly(3));

			order.JD_OrderStatus = Constants.OrderStatus.Open;
			AssertNotContainsFilterNotMetError(collection, order, "This Order cannot be chosen here as it has been canceled.");
			orderParent.Setup(m => m.Factory).Returns(Factory);
			orderParent.Setup(m => m.TransportMode).Returns(ZString.Empty);
			orderParent.Setup(m => m.TableName);
			orderParent.Setup(m => m.ConsigneeDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			orderParent.Setup(m => m.ConsignorDocumentaryAddress).Returns(Factory.New<JobDocAddress>());
			var errorMessage = @"This Order is already linked to an active Supplier Booking. Orders in use in the Supplier Bookings module cannot be linked to Shipments directly.
Please cancel all active Supplier Bookings before attaching this Order directly, or proceed to the Supplier Booking and Container Load List process to create a new Shipment from this Order.";
			collection = Order.GetPossibleOrdersForAttachment_List(orderParent.Object);
			order = Factory.NewWithValidTestData<Order>();
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.JSB_Status = Constants.SupplierBookingStatus.Approved;
			Assert(order.IsAttachedToSupplierBooking);
			AssertContainsFilterNotMetError(collection, order, errorMessage);

			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			Assert(!order.IsAttachedToSupplierBooking);

			AssertNotContainsFilterNotMetError(collection, order, errorMessage);

			orderParent.Verify(m => m.TableName, Times.Exactly(2));
			orderParent.Verify(m => m.ConsignorDocumentaryAddress, Times.Exactly(4));

			booking.JSB_Status = Constants.SupplierBookingStatus.Converted;
			AssertContainsFilterNotMetError(collection, order, errorMessage);
		}

		void AssertContainsFilterNotMetError(OrderCollection collection, Order order, ZString expectedError)
		{
			AssertContains(expectedError, ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(order));
		}

		void AssertNotContainsFilterNotMetError(OrderCollection collection, Order order, ZString expectedError)
		{
			AssertNotContains(expectedError, ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(order));
		}

		public void TestReasonNotToBeAbleToDetach()
		{
			var errorMessage = @"This Order is linked to an active Supplier Booking. Orders linked to a Supplier Booking cannot be detached from Shipments directly.
Please detach the order through the supplier booking function.";
			var order = Factory.NewWithValidTestData<Order>();
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.JSB_Status = Constants.SupplierBookingStatus.Approved;

			Assert(!order.CanDetach);
			AssertEquals(errorMessage, order.ReasonNotToBeAbleToDetach);

			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			Assert(order.CanDetach);
			AssertEquals(String.Empty, order.ReasonNotToBeAbleToDetach);
		}

		#endregion

		#region OnSaving

		#region TestOnSavingCountryRequiredDocuments

		public void TestOnSavingCountryRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInstruction, "", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ArrivalNotice, aUS, "", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc4 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BankDraft, "", "", JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc5 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BillOfEntry, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Rail, true);
			RefCountryRequiredDocument requiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, false);
			RefCountryRequiredDocument requiredDoc7 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ChargeSheet, "US", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc8 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DelayAlert, aUS, "US", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc9 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);

			Order order = Factory.New<Order>();
			order.SupplierPK = LocalConsignor.PK;
			order.BuyerPK = OverseasConsignee.PK;
			order.JD_RL_NKPortOfLoading = "AUBNE";
			order.JD_RL_NKPortOfDischarge = "NZAKL";
			order.JD_OrderNumber = "1234";
			order.JD_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			AssertNull("Doesn't match, shouldn't be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BillOfEntry));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));

			AssertEquals(4, order.RequiredDocuments.Count);

			JobRequiredDocument rdAgentInvoice = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice);
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", rdAgentInvoice.EQ_DocUsage);

			JobRequiredDocument rdAgentsInstruction = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInstruction);
			AssertNotNull("Should be in the list", rdAgentsInstruction);
			AssertEquals("DocUsage should be IMP", "IMP", rdAgentsInstruction.EQ_DocUsage);

			JobRequiredDocument rdArrivalNotice = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ArrivalNotice);
			AssertNotNull("Should be in the list", rdArrivalNotice);
			AssertEquals("DocUsage should be EXP", "EXP", rdArrivalNotice.EQ_DocUsage);

			JobRequiredDocument rdBankDraft = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BankDraft);
			AssertNotNull("Should be in the list", rdBankDraft);
			AssertEquals("DocUsage should be BTH", "BTH", rdBankDraft.EQ_DocUsage);

			RefCountry countryAU2 = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			RefCountryRequiredDocument requiredDoc10 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.AgentsInvoice, aUS, aUS, JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc11 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.ChargeSheet, aUS, aUS, JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc12 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.DangerousGoodsForm, "", "", JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc13 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EFTRequest, aUS, aUS, JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc14 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EntryPrint, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc15 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.HouseBill, aUS, "CY", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc16 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.MasterHouse, aUS, "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);

			Order order2 = Factory.New<Order>();
			order2.SupplierPK = LocalConsignor.PK;
			order2.BuyerPK = LocalConsignee.PK;
			order2.JD_RL_NKPortOfLoading = "AUSYD";
			order2.JD_RL_NKPortOfDischarge = "AUBNE";
			order2.JD_OrderNumber = "1235";
			order2.JD_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			AssertEquals(5, order2.RequiredDocuments.Count);

			AssertNull("Doesn't match, shouldn't be in the list", order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			JobRequiredDocument rdAgentInvoice2 = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice2);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", rdAgentInvoice2.EQ_DocUsage);

			JobRequiredDocument dangerousGoodsForm = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm);
			AssertNotNull("AgentInvoice: Should be in the list", dangerousGoodsForm);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", dangerousGoodsForm.EQ_DocUsage);

			JobRequiredDocument entryPrint = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EntryPrint);
			AssertNotNull("AgentInvoice: Should be in the list", entryPrint);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", entryPrint.EQ_DocUsage);

			JobRequiredDocument eFTRequest = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EFTRequest);
			AssertNotNull("AgentInvoice: Should be in the list", eFTRequest);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", eFTRequest.EQ_DocUsage);

			JobRequiredDocument masterHouse = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterHouse);
			AssertNotNull("AgentInvoice: Should be in the list", masterHouse);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", masterHouse.EQ_DocUsage);
		}

		#endregion

		#region TestOnSavingOrganisationRequiredDocuments

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingOrganisationRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);
			RefCountryRequiredDocument testRequiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.VetinaryCertificate, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");
			JobRequiredDocument requiredDoc4 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");

			Order order = Factory.New<Order>();
			order.SupplierPK = consignor.PK;
			OverseasConsignee.OH_RL_NKClosestPort = "NZAKL";
			order.BuyerPK = OverseasConsignee.PK;
			order.JD_RL_NKPortOfLoading = "AUBNE";
			order.JD_RL_NKPortOfDischarge = "NZAKL";
			order.JD_OrderNumber = "1234";
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Today.AddDays(1));
			Factory.Save();

			JobRequiredDocument rdSanitaryCertificate = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate);
			AssertNotNull("SanitaryCertificate: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertEquals("SanitaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdSanitaryCertificate.EQ_DocPeriod);

			JobRequiredDocument rdAgentInvoice = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));

			JobRequiredDocument rdVetinaryCertificate = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertEquals("VetinaryCertificate: Period should be Period", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);

			AssertNotNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertNotNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
		}
		#endregion

		#region TestOnSavingBuyerSupplierRequiredDocuments

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingBuyerSupplierRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUBNE";
			consignee.OH_RL_NKClosestPort = "NZAKL";

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");

			OrgSupplierBuyerLink buyerSupplierLink1 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink1.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.NewZealand;

			JobRequiredDocument requiredDoc4 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");
			JobRequiredDocument requiredDoc8 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0006");

			OrgSupplierBuyerLink buyerSupplierLink2 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			JobRequiredDocument requiredDoc9 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink2, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-3), ZDateTime.Today.Date.AddDays(3), "0007");

			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.SupplierPK = consignor.PK;
			order1.BuyerPK = consignee.PK;
			order1.JD_RL_NKPortOfLoading = "AUBNE";
			order1.JD_RL_NKGoodsDeliveredTo = "SGSIN";
			order1.JD_OrderNumber = "1234";
			order1.JD_TransportMode = Constants.TransportModes.Sea;
			order1.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Today.AddDays(1));
			Factory.Save();

			JobRequiredDocument rdVetinaryCertificate = order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", rdVetinaryCertificate);
			AssertEquals("VetinaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Today.AddDays(-2), rdVetinaryCertificate.EQ_DateReceived);

			JobRequiredDocument rdBeneficiaryCertificate = order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate);
			AssertNotNull("BeneficiaryCertificate: Should be in the list", order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));

			JobRequiredDocument rdAgentsInvoice = order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: Period should be Period", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdAgentsInvoice.EQ_DocPeriod);

			AssertNotNull(order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNotNull(order1.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));

			Order order2 = Factory.NewWithValidTestData<Order>();
			order2.SupplierPK = consignor.PK;
			order2.BuyerPK = consignee.PK;
			order2.JD_RL_NKPortOfLoading = "AUBNE";
			order2.JD_RL_NKGoodsDeliveredTo = "USLAX";
			order2.JD_OrderNumber = "5678";
			order2.JD_TransportMode = Constants.TransportModes.Sea;
			order2.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Today.AddDays(1));
			Factory.Save();

			rdVetinaryCertificate = order2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", rdVetinaryCertificate);
			AssertEquals("VetinaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Today.AddDays(-3), rdVetinaryCertificate.EQ_DateReceived);
		}
		#endregion

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isOrder)
		{
			RefCountryRequiredDocument result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnOrder = isOrder;
			return result;
		}

		JobRequiredDocument GetNewJobRequiredDoc(OrgHeader org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		JobRequiredDocument GetNewBuyerSupplierRequiredDoc(OrgSupplierBuyerLink buyerSupplierLink, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			JobRequiredDocument result = buyerSupplierLink.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		public void TestOnSaving_ShouldCreateStatusUpdatedEvent_IfReleasedFlagIsToggled()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.OH_Code = "ORGASYD";
			buyer.Addresses[0].OA_Address1 = "Buyer address";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "DMC-ORDER";
			order.JD_OrderNumberSplit = 1;
			order.BuyerPK = buyer.PK;
			Factory.Save();

			var log = order.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNull("Should not create STU event if new record", log);

			order.JD_IsReleased = true;
			Factory.Save();

			log = order.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertEquals(
				"Should create STU event with relevant human-readable values",
				"|NEW=Released|OLD=On hold|RFN=DMC-ORDER~1~ORGASYD",
				log.Reference);

			order.JD_IsReleased = false;
			Factory.Save();

			log = order.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertEquals(
				"Should create STU event with relevant human-readable values",
				"|NEW=On hold|OLD=Released|RFN=DMC-ORDER~1~ORGASYD",
				log.Reference);
		}

		#endregion

		#region Order Status Events

		public void TestJD_OrderStatusRaisesEvents()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderStatus = Constants.OrderStatus.Open;
			AssertNotNull("Event raised when JD_OrderStatus is set to Open", order.Logs.MostRecentLogByEventTime(Events.OrderPlacedFinalised));
		}

		#endregion

		#region Pre-Advice

		public void TestCreateAndLinkPreAdviceToOrder()
		{
			var order = Factory.New<Order>();
			order.JD_MasterWaybill = "MASTER";
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.JD_OH_Carrier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.JD_OH_ReceivingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.JD_OH_SendingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.JD_RL_NKPortOfDischarge = "USLAX";
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			order.JD_RV_NKDepartureVessel = "Gudrun Maersk";
			order.JD_DepartureVoyage = "GM1234";

			order.JD_RV_NKIntermediateVessel = "Intermediate";
			order.JD_IntermediateVoyage = "IM1234";

			order.JD_RV_NKArrivalVessel = "Arrival";
			order.JD_ArrivalVoyage = "AR1234";

			order.JD_ActualWeight = 100m;
			order.JD_UnitOfWeight = Constants.Weight.Grams;
			order.JD_ActualVolume = 200m;
			order.JD_UnitOfVolume = Constants.Volume.CubicDecimetres;
			order.JD_Packs = 300;
			order.JD_F3_NKPackType = Constants.PkgUnit.Piece;

			order.UpdateEventEstimate(Events.Departure, new ZDateTimeOffset(2008, 5, 5));
			order.UpdateEventEstimate(Events.Arrival, new ZDateTimeOffset(2008, 6, 6));
			order.UpdateEvent(Events.Departure, new ZDateTimeOffset(2008, 5, 15));
			order.UpdateEvent(Events.Arrival, new ZDateTimeOffset(2008, 6, 16));

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();
			order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertEquals(ZGuid.Empty, order.JD_EF_ShipmentPrePlanning);
			AssertEquals("Please save your Order before attempting any Actions.", notifier.LastMessage);

			Factory.Save();
			notifier.LastMessage = "";
			order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertNotEquals(ZGuid.Empty, order.JD_EF_ShipmentPrePlanning);
			AssertEquals(order.JD_MasterWaybill, order.PreAdvice.EF_MasterBill);
			AssertEquals(order.JD_OA_BuyerAddress, order.PreAdvice.EF_OA_BuyerAddress);
			AssertEquals(order.JD_OC_BuyerContact, order.PreAdvice.EF_OC_BuyerContact);
			AssertEquals(order.JD_OH_Carrier, order.PreAdvice.EF_OH_Carrier);
			AssertEquals(order.JD_OH_ReceivingAgent, order.PreAdvice.EF_OH_ReceivingAgent);
			AssertEquals(order.JD_OH_SendingAgent, order.PreAdvice.EF_OH_SendingAgent);
			AssertEquals(order.JD_RL_NKPortOfDischarge, order.PreAdvice.EF_RL_NKPortDisch);
			AssertEquals(order.JD_RL_NKPortOfLoading, order.PreAdvice.EF_RL_NKPortLoad);

			AssertEquals(order.JD_TransportMode, order.PreAdvice.PreAdviceTransports[0].JW_TransportMode);
			AssertEquals(order.JD_RV_NKDepartureVessel, order.PreAdvice.PreAdviceTransports[0].JW_Vessel);
			AssertEquals(order.JD_DepartureVoyage, order.PreAdvice.PreAdviceTransports[0].JW_VoyageFlight);
			AssertEquals(order.JD_OH_Carrier, order.PreAdvice.PreAdviceTransports[0].CarrierPK);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime(), order.PreAdvice.PreAdviceTransports[0].JW_ETD);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime(), order.PreAdvice.PreAdviceTransports[0].JW_ETA);
			AssertEquals(order.GetMilestoneActualDate(Events.Departure).ToZDateTime(), order.PreAdvice.PreAdviceTransports[0].JW_ATD);
			AssertEquals(order.GetMilestoneActualDate(Events.Arrival).ToZDateTime(), order.PreAdvice.PreAdviceTransports[0].JW_ATA);

			AssertEquals("Gudrun Maersk", order.JD_RV_NKDepartureVessel);
			AssertEquals("GM1234", order.JD_DepartureVoyage);

			AssertEquals("Intermediate", order.JD_RV_NKIntermediateVessel);
			AssertEquals("IM1234", order.JD_IntermediateVoyage);

			AssertEquals("Arrival", order.JD_RV_NKArrivalVessel);
			AssertEquals("AR1234", order.JD_ArrivalVoyage);

			AssertEquals("EF_ActualWeight", 100m, order.PreAdvice.EF_ActualWeight);
			AssertEquals("EF_UnitOfWeight", Constants.Weight.Grams, order.PreAdvice.EF_UnitOfWeight);
			AssertEquals("EF_ActualVolume", 200m, order.PreAdvice.EF_ActualVolume);
			AssertEquals("EF_UnitOfVolume", Constants.Volume.CubicDecimetres, order.PreAdvice.EF_UnitOfVolume);
			AssertEquals("EF_Packs", 300, order.PreAdvice.EF_Packs);
			AssertEquals("EF_F3_NKPackType", Constants.PkgUnit.Piece, order.PreAdvice.EF_F3_NKPackType);

			AssertEquals(1, order.PreAdvice.Orders.Count);
			AssertCollectionContains(order, order.PreAdvice.Orders);
			AssertEquals(false, order.HasChanges);

			Factory.Save();

			var expectedMessage = "This order is already part of a Shipment Pre Advice.";
			notifier.LastMessage = "";
			var advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertEquals(expectedMessage, notifier.LastMessage);
			AssertEquals(null, advice);

			expectedMessage = "This order is attached to the PA00000001 pre-advice. The created Declaration will be for that pre-advice and not just this one order.\r\n\r\nDo you want to continue?";
			notifier.LastMessage = "";
			advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Declaration);
			AssertEquals(expectedMessage, notifier.LastMessage);
			AssertEquals(null, advice);

			expectedMessage = "This order is attached to the PA00000001 pre-advice. The created Shipment will be for that pre-advice and not just this one order.\r\n\r\nDo you want to continue?";
			notifier.LastMessage = "";
			advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Shipment);
			AssertEquals(expectedMessage, notifier.LastMessage);
			AssertEquals(null, advice);

			expectedMessage = "This order is attached to the PA00000001 pre-advice. The created Shipment will be for that pre-advice and not just this one order.\r\n\r\nDo you want to continue?";
			notifier.LastMessage = "";
			notifier.NextQueryUserResult = true;
			advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Shipment);
			AssertEquals(expectedMessage, notifier.LastMessage);
			AssertEquals(order.PreAdvice, advice);

			order.PreAdvice.Orders.RemoveFromRelationship(order);
			order.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Empty);
			order.UpdateEventEstimate(Events.Arrival, ZDateTimeOffset.Empty);
			order.UpdateEvent(Events.Departure, ZDateTimeOffset.Empty);
			order.UpdateEvent(Events.Arrival, ZDateTimeOffset.Empty);
			Factory.Save();

			order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertEquals(new ZDateTime(2008, 5, 5), order.PreAdvice.PreAdviceTransports[0].JW_ETD);
			AssertEquals(new ZDateTime(2008, 6, 6), order.PreAdvice.PreAdviceTransports[0].JW_ETA);
			AssertEquals(new ZDateTime(2008, 5, 15), order.PreAdvice.PreAdviceTransports[0].JW_ATD);
			AssertEquals(new ZDateTime(2008, 6, 16), order.PreAdvice.PreAdviceTransports[0].JW_ATA);
		}

		public void TestCreateAndLinkPreAdviceToOrder_DoNothing()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.AttachedOrders.Add(order);

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 2;
			orderLine.JO_QtyReceived = 1;
			Factory.Save();

			var advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Shipment);
			AssertEquals("Now has no new order", 1, shipment.AttachedOrders.Count);
			AssertEquals("Now has no split", 0, order.OrderSplitSiblings.Count);
		}

		public void TestCreateAndLinkPreAdviceToOrder_CreateNewOrder()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.AttachedOrders.Add(order);

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 2;
			orderLine.JO_QtyReceived = 1;
			Factory.Save();

			var advice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Shipment, GetCreateNewOrder);
			AssertEquals("Now has a new order", 2, shipment.AttachedOrders.Count);
			AssertEquals("Now has no split", 0, order.OrderSplitSiblings.Count);
		}

		OrderSplitDialogResult GetCreateNewOrder()
		{
			return OrderSplitDialogResult.CreateNewOrder;
		}

		public void TestCreateAndLinkPreAdviceToOrder_GetSplitOrder()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			var order1 = Factory.NewWithValidTestData<Order>();
			shipment.AttachedOrders.Add(order1);

			var order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_Quantity = 10m;
			order1Line1.JO_QtyReceived = 0m;

			var order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_Quantity = 13m;
			order1Line2.JO_QtyReceived = 13m;

			var order1Line3 = order1.OrderLines.AddNew();
			order1Line3.JO_Quantity = 8m;
			order1Line3.JO_QtyReceived = 5m;
			Factory.Save();

			var advice = order1.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.Shipment, GetSplitOrder);
			AssertEquals("Now has no new order", 1, shipment.AttachedOrders.Count);
			AssertEquals("Now has a split", 1, order1.OrderSplitSiblings.Count);

			AssertEquals("Split is not on this pre-advice", ZGuid.Empty, order1.OrderSplitSiblings[0].JD_EF_ShipmentPrePlanning);
			AssertEquals("2 lines on split", 2, order1.OrderSplitSiblings[0].OrderLines.Count);
			AssertEquals("Expected Quantity", 10m, order1.OrderSplitSiblings[0].OrderLines[0].JO_Quantity);
			AssertEquals("Qty Received", 0m, order1.OrderSplitSiblings[0].OrderLines[0].JO_QtyReceived);
			AssertEquals("Line Split Number 0 as no qty received", ZShort.Zero, order1.OrderSplitSiblings[0].OrderLines[0].JO_LineSplitNumber);
			AssertEquals("Expected Quantity", 3m, order1.OrderSplitSiblings[0].OrderLines[1].JO_Quantity);
			AssertEquals("Qty Received", 0m, order1.OrderSplitSiblings[0].OrderLines[1].JO_QtyReceived);
			AssertEquals("Line Split Number 1 as some qty received", (ZShort)1, order1.OrderSplitSiblings[0].OrderLines[1].JO_LineSplitNumber);
		}

		OrderSplitDialogResult GetSplitOrder()
		{
			return OrderSplitDialogResult.SplitOrder;
		}

		public void TestJD_EF_ShipmentPrePlanning_RaisesPreadviceEvent()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_EF_ShipmentPrePlanning = Factory.NewWithValidTestData<JobShipmentPreplanning>().PK;
			Factory.Save();
			AssertNotNull("Show Attached event when ShipmentPrePlanning was attached", order.Logs.MostRecentLogByEventTime(Events.Attached));
			AssertEquals("PA00000001|TYP=Shipment Pre Advice", order.Logs.MostRecentLogByEventTime(Events.Attached).SL_Reference);

			order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;
			Factory.Save();
			AssertNotNull("Show Detached event when ShipmentPrePlanning was detached", order.Logs.MostRecentLogByEventTime(Events.Detached));
			AssertEquals("PA00000001|TYP=Shipment Pre Advice", order.Logs.MostRecentLogByEventTime(Events.Detached).SL_Reference);
		}

		public void TestJD_JS_RaisesOrderShippedEvent()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			AssertNotNull("Event raised when foreign key set", order.Logs.MostRecentLogByEventTime(Events.OrderShipped));

			order.JD_JS = ZGuid.Empty;
			AssertNull("Event deleted when foreign key null", order.Logs.MostRecentLogByEventTime(Events.OrderShipped));
		}

		public void TestJD_JE_RaisesOrderShippedEvent()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_JE = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration))).PK;
			AssertNotNull("Event raised when foreign key set", order.Logs.MostRecentLogByEventTime(Events.OrderShipped));

			order.JD_JE = ZGuid.Empty;
			AssertNull("Event deleted when foreign key null", order.Logs.MostRecentLogByEventTime(Events.OrderShipped));
		}

		#endregion

		#region DontDefaultTransferModeWhenVesselDetailsAreEntered

		public void TestDontDefaultTransferModeWhenVesselDetailsAreEntered()
		{
			BO = Factory.New<Order>();
			BO.JD_TransportMode = "SEA";
			AssertEquals("No msg should be shown", "SEA", BO.JD_TransportMode);

			// set up the buyer/suppler and their link
			OrgHeader buyer = CreateNewOrg(BO.Factory, "buy");
			OrgHeader supplier = CreateNewOrg(BO.Factory, "supp");
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = buyer.PK;

			OrgHeader aIRCarrier = CreateNewOrg(BO.Factory, "carr");
			OrgHeader fCLCarrier = CreateNewOrg(BO.Factory, "carr");
			ZString transportMode = Constants.TransportModes.Air;
			ZString containerMode = Constants.ContainerModes.Loose;

			// set the defaults on the link and supplier/misc serv
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;

			// make sure the defaults come from the link appropriately
			BO.SupplierPK = supplier.PK;
			BO.BuyerPK = buyer.PK;

			BO.JD_TransportMode = "SEA";
			BO.JD_ContainerMode = "FCL";
			BO.SupplierPK = ZGuid.Empty;
			BO.BuyerPK = ZGuid.Empty;
			BO.JD_RV_NKArrivalVessel = "AVessel";
			BO.JD_RV_NKIntermediateVessel = "BVessel";
			BO.JD_RV_NKDepartureVessel = "CVessel";

			BO.SupplierPK = supplier.PK;
			BO.BuyerPK = buyer.PK;
			AssertEquals("TransportMode shouldn't default to Air from buyer supplier link", "SEA", BO.JD_TransportMode);
			AssertEquals("ContainerMode shouldn't default to loose", "FCL", BO.JD_ContainerMode);
			AssertEquals("Vessel stays", "AVessel", BO.JD_RV_NKArrivalVessel);
			AssertEquals("Vessel Cleared", "BVessel", BO.JD_RV_NKIntermediateVessel);
			AssertEquals("Vessel Cleared", "CVessel", BO.JD_RV_NKDepartureVessel);
		}

		#endregion

		#region TestSavingWithNullBuyer

		public void TestSavingWithNullBuyer()
		{
			bool valueBefore = OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value;
			try
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Order ord1 = Factory.New<Order>();
				ord1.JD_OrderNumber = "1";
				ord1.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				ord1.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
				OnFactorySaving(ord1);

				AssertEquals("Should have updated Order Status", Constants.OrderStatus.Delivered, ord1.JD_OrderStatus);
			}
			finally
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, valueBefore);
			}
		}

		#endregion

		#region Related Business Objects

		public void TestRelatedWarehouseReceive()
		{
			var order = Factory.New<Order>();
			AssertNull(order.RelatedWarehouseReceive);

			var warehouseReceive1 = Factory.New<IWhsReceive>();
			var orderReceivePivot1 = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_ParentId] = ZGuid.NewZGuid();
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive1.PK;
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive1.WD_DocketType;

			var warehouseReceive2 = Factory.New<IWhsReceive>();
			var orderReceivePivot2 = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_ParentId] = order.PK;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive2.PK;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive2.WD_DocketType;

			AssertEquals((BusinessObject)warehouseReceive2, order.RelatedWarehouseReceive);
		}

		public void TestWarehouseDocAddress()
		{
			var order = Factory.New<Order>();
			AssertEquals(0, order.DocAddresses.Count);
			AssertNotNull(order.WarehouseDocAddress);
			AssertEquals(DocAddressType.Warehouse, order.WarehouseDocAddress.DocAddressType);
			AssertEquals("Check DocAddress returns the same instance once created", order.WarehouseDocAddress, order.WarehouseDocAddress);
			AssertEquals(1, order.DocAddresses.Count);
		}

		public void TestWarehouseAddress()
		{
			var order = Factory.New<Order>();
			AssertNull(order.WarehouseAddress);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			order.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(orgHeader.MainAddress, order.WarehouseAddress);
		}

		public void TestWarehouse()
		{
			var order = Factory.New<Order>();
			AssertNull(order.Warehouse);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			order.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(orgHeader, order.Warehouse);
		}

		public void TestNotifyPartyDocAddress()
		{
			var order = Factory.New<Order>();
			AssertEquals(0, order.DocAddresses.Count);
			AssertNotNull(order.NotifyPartyDocAddress);
			AssertNotNull(order.NotifyParty2DocAddress);
			AssertNotNull(order.NotifyParty3DocAddress);
			AssertEquals(DocAddressType.NotifyParty, order.NotifyPartyDocAddress.DocAddressType);
			AssertEquals(DocAddressType.NotifyParty2, order.NotifyParty2DocAddress.DocAddressType);
			AssertEquals(DocAddressType.NotifyParty3, order.NotifyParty3DocAddress.DocAddressType);
			AssertEquals("Check DocAddress returns the same instance once created", order.NotifyPartyDocAddress, order.NotifyPartyDocAddress);
			AssertEquals("Check DocAddress returns the same instance once created", order.NotifyParty2DocAddress, order.NotifyParty2DocAddress);
			AssertEquals("Check DocAddress returns the same instance once created", order.NotifyParty3DocAddress, order.NotifyParty3DocAddress);
			AssertEquals(3, order.DocAddresses.Count);
		}

		public void TestPlannedContainers()
		{
			AssertEquals("PlannedContainers", 0, BO.PlannedContainers.Count);
			BO.PlannedContainers.AddNew();
			AssertEquals("PlannedContainers", 1, BO.PlannedContainers.Count);

			BO.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("PlannedContainers", 1, BO.PlannedContainers.Count);

			BO.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("PlannedContainers", 1, BO.PlannedContainers.Count);

			BO.JD_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("PlannedContainers cleared", 0, BO.PlannedContainers.Count);
		}

		#endregion

		#region Business Object Overrides

		public void TestClone()
		{
			BO.JD_OrderNumber = "order";
			BO.JD_OrderNumberSplit = 2;
			BO.JD_OrderStatus = "xxx";

			OrderLine line = BO.OrderLines.AddNew();
			line.JO_LineNo = 2;
			OrderLineDelivery delivery = line.Deliveries.AddNew();
			delivery.J4_Allocated = 1;
			OrderLineDeliverContainer deliverContainer = delivery.Containers.AddNew();
			deliverContainer.J5_ContainerNum = "XXXXXXX2";
			OrderContainer container = BO.PlannedContainers.AddNew();
			container.J1_ContainerCount = 2;

			Order clonedOrder = (Order)BO.Clone();
			AssertEquals("Order number empty", ZString.Empty, clonedOrder.JD_OrderNumber);
			AssertEquals("Order number split empty", new ZByte(0), clonedOrder.JD_OrderNumberSplit);

			AssertEquals("Order Cloned", "xxx", clonedOrder.JD_OrderStatus);
			AssertEquals("OrderLine Cloned", 2, clonedOrder.OrderLines[0].JO_LineNo);
			AssertEquals("OrderLineDelivery Cloned", new ZDecimal(1), clonedOrder.OrderLines[0].Deliveries[0].J4_Allocated);
			AssertEquals("OrderLineDeliverContainer Cloned", "XXXXXXX2", clonedOrder.OrderLines[0].Deliveries[0].Containers[0].J5_ContainerNum);
			AssertEquals("OrderContainer Cloned", new ZShort((short)2), clonedOrder.PlannedContainers[0].J1_ContainerCount);
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			BO.JD_OrderNumber = "x";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;
			Order split1 = BO.SplitOrder(CreateOrderType.Split);
			Order split2 = split1.SplitOrder(CreateOrderType.Split);

			OrderLine line = split2.OrderLines.AddNew();
			OrderLineDelivery delivery = line.Deliveries.AddNew();
			OrderLineDeliverContainer container = delivery.Containers.AddNew();

			BO.Factory.Save();
			split1.Delete();
			OrgHeader configOrg = ((ICustomLabelsConfigOrgProvider)split1).ConfigOrg;
			AssertNull("This should not have thrown a RowNotInTableException", configOrg);
			AssertEquals("Order with split number less than item being deleted should not be deleted", false, BO.IsDeleted);
			AssertEquals("Order should be deleted", true, split1.IsDeleted);
			AssertEquals("Order split should also be deleted", true, split2.IsDeleted);
			BO.Factory.Save();
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var orderLine = BO.OrderLines.AddNew();
			BO.JD_JS = Shipment.PK;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { orderLine, Consol, Shipment }, BO.BusinessObjectsWithRelatedEvents);

			BO.JD_JE = Declaration.PK;
			BO.JD_EF_ShipmentPrePlanning = ShipmentPreadvise.PK;
			var note = BO.Notes.AddNew(true, "Test Note", "Test Note Description");
			var warehouseReceive = Factory.New<IWhsReceive>();
			var orderReceivePivot = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentId] = BO.PK;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = BO.TablePrefix;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive.PK;
			orderReceivePivot[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive.WD_DocketType;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { orderLine, Declaration, ShipmentPreadvise, note, (BusinessObject)warehouseReceive }, BO.BusinessObjectsWithRelatedEvents);
		}

		JobShipmentPreplanning ShipmentPreadvise
		{
			get { return shipmentPreadvise ?? (shipmentPreadvise = Factory.New<JobShipmentPreplanning>()); }
		}
		JobShipmentPreplanning shipmentPreadvise;

		#endregion

		#region FillWithValidTestData

		public void TestFillWithValidTestData_DoesPopulatePickupDeliveryLists()
		{
			var order = Factory.NewWithValidTestData<TestOrderWithExceptionThrownOnAddNewForPickDeliverAddressLists>(TestBusinessObjectKind.All);
			AssertEquals("Should be populating dependent collections", true, order.OrderLines.Count >= 1);
		}

		class TestOrderWithExceptionThrownOnAddNewForPickDeliverAddressLists : Order
		{
			public TestOrderWithExceptionThrownOnAddNewForPickDeliverAddressLists(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion

		#region Events

		public void TestOrderStatusChangeEvent()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "A Test Order";
			order1.BuyerPK = org.PK;
			order1.SupplierPK = org.PK;
			order1.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			Factory.Save();

			order1.JD_OrderStatus = Constants.OrderStatus.Delivered;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, order1.PK);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, Constants.OrderStatus.Delivered);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, Constants.OrderStatus.Incomplete);
			StmALog[] eventsFound = (StmALog[])Factory.Load(typeof(StmALog), filter);
			AssertEquals("Should have one edit event with Order Status in it", 1, eventsFound.Length);

			Factory.Save();
			eventsFound = (StmALog[])Factory.Load(typeof(StmALog), filter);
			AssertEquals("Saving again should not add an additional event with Order Status in it", 1, eventsFound.Length);

			ZQuery filter2 = new ZQuery(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, order1.PK);
			filter2.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);
			eventsFound = (StmALog[])Factory.Load(typeof(StmALog), filter2);
			int countOfEventsBefore = eventsFound.Length;

			order1.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			order1.JD_OrderStatus = Constants.OrderStatus.Cancelled;
			order1.JD_OrderStatus = Constants.OrderStatus.PartDelivered;
			order1.JD_OrderStatus = Constants.OrderStatus.Delivered;

			eventsFound = (StmALog[])Factory.Load(typeof(StmALog), filter2);
			AssertEquals("The events should only come from when the factory is saved", countOfEventsBefore, eventsFound.Length);
		}

		#endregion

		#region WorkflowProviderLinkage

		[GuiTest]
		public void TestWorkflowProviderLinkageServiceForJDJSAndShipment()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.CreateSystem(Factory, "ORD", "SHP");
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "ORD";
			var templateJobHeader1 = template1.ProcessHeaders[0];

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SHP";
			var templateJobHeader2 = template2.ProcessHeaders[0];

			var templateLink1 = (IProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			templateLink1.FP_FH_HeaderFrom = templateJobHeader1.PK;
			templateLink1.FP_FH_HeaderTo = templateJobHeader2.PK;
			templateLink1.FP_LinkType = "DEP";
			templateLink1.FromWorkflowExternalTemplatePK = template2.PK;
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "CN1";

			var order = Factory.NewWithValidTestData<Order>();

			Factory.Save();

			var orderJobHeader = ProcessJobHeaderProvider.GetForParent(order, Factory);
			var shipmentJobHeader = ProcessJobHeaderProvider.GetForParent(shipment, Factory);

			Factory.Save();

			AssertNotNull(orderJobHeader);
			AssertNotNull(shipmentJobHeader);

			order.JD_JS = shipment.PK;
			testHelper.AssertIsPrerequisite(orderJobHeader, shipmentJobHeader);

			order.JD_JS = ZGuid.Empty;
			testHelper.AssertIsNotPrerequisite(orderJobHeader, shipmentJobHeader);
		}

		#endregion

		#region TestJD_JSAttachedLog

		public void TestJD_JSAttachOrDetachLog()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "CN1";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;
			AssertEquals(1, order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());

			order.JD_JS = ZGuid.Empty;
			AssertEquals("unsaved Attach event is removed when detach", 0, order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());
			AssertEquals("Detach event is NOT logged", 0, order.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Count());

			order.JD_JS = shipment.PK;
			Factory.Save();

			AssertEquals("CN1|TYP=Shipment", order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);

			order.JD_JS = ZGuid.Empty;
			AssertEquals("CN1|TYP=Shipment", order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);
			AssertEquals("CN1|TYP=Shipment", order.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Single().SL_Reference);
		}

		public void TestUpdateReferenceNumberForATCEvent()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "CN1";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;
			AssertEquals(string.Format("{0}|TYP=Shipment", shipment.PK), order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);

			Factory.Save();
			AssertEquals("CN1|TYP=Shipment", order.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);
		}

		#endregion

		#region Buyer/Supplier Relationship

		public void TestDefaultFromBuyerSupplierLinkSetsTransportMode()
		{
			// set up the buyer/suppler and their link
			OrgHeader buyer = CreateNewOrg(BO.Factory, "buy");
			OrgHeader supplier = CreateNewOrg(BO.Factory, "supp");
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = buyer.PK;

			// set up some values for the defaults
			RefCurrency currency1 = RefCurrency.LoadFromCurrencyCode(BO.Factory, "AUD");
			RefCurrency currency2 = BO.Factory.New<RefCurrency>();
			RefServiceLevel serviceLevel = BO.Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "GGG";

			OrgHeader aIRCarrier = CreateNewOrg(BO.Factory, "carr");
			OrgHeader fCLCarrier = CreateNewOrg(BO.Factory, "carr");
			ZString transportMode = Constants.TransportModes.Courier;
			ZString containerMode = Constants.ContainerModes.FCL;
			ZString inco = Constants.IncoTerms.FreeOnBoard;

			// set the defaults on the link and supplier/misc serv
			link.OL_RX_NKDefaultCurrency = currency1.RX_Code;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = currency2.RX_Code;
			buyer.MiscServ.OM_RS_NKIMDefaultServiceLevel = serviceLevel.RS_Code;
			link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = aIRCarrier.PK;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;
			link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = inco;
			link.OrgSupBuyLinkTrnModes[0].PF_RS_NKDefaultServiceLevel = serviceLevel.RS_Code;

			// make sure the defaults come from the link appropriately
			((ISupportDataImporting)BO).IsImportingData = true;
			BO.SupplierPK = supplier.PK;
			BO.BuyerPK = buyer.PK;
			AssertNotEquals("TransportMode shouldn default to Constants.TransportModes.Courier from buyer supplier link", link.OrgSupBuyLinkTrnModes[0].PF_TransportMode, BO.JD_TransportMode);
		}

		public void TestDefaultFromBuyerSupplierLink()
		{
			// set up the buyer/suppler and their link
			OrgHeader buyer = CreateNewOrg(BO.Factory, "buy");
			OrgHeader supplier = CreateNewOrg(BO.Factory, "supp");
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(buyer);
			link.OL_RN_NKImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			// set up some values for the defaults
			RefCurrency currency1 = RefCurrency.LoadFromCurrencyCode(BO.Factory, "AUD");
			RefCurrency currency2 = BO.Factory.NewWithValidTestData<RefCurrency>();
			RefServiceLevel serviceLevel = BO.Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "GGG";

			OrgHeader aIRCarrier = CreateNewOrg(BO.Factory, "carr");
			OrgHeader fCLCarrier = CreateNewOrg(BO.Factory, "carr");
			ZString transportMode = Constants.TransportModes.All;
			ZString containerMode = Constants.ContainerModes.FCL;
			ZString inco = Constants.IncoTerms.FreeOnBoard;

			// set the defaults on the link and supplier/misc serv
			link.OL_RX_NKDefaultCurrency = currency1.RX_Code;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = currency2.RX_Code;
			buyer.MiscServ.OM_RS_NKIMDefaultServiceLevel = serviceLevel.RS_Code;
			link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = aIRCarrier.PK;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;
			link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = inco;
			link.OrgSupBuyLinkTrnModes[0].PF_RS_NKDefaultServiceLevel = serviceLevel.RS_Code;

			// make sure the defaults come from the link appropriately
			((ISupportDataImporting)BO).IsImportingData = true;
			BO.JD_RL_NKGoodsDeliveredTo = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			BO.SupplierPK = supplier.PK;
			BO.BuyerPK = buyer.PK;
			AssertEquals("ServiceLevel", serviceLevel.PK, BO.ServiceLevel_NI.PK);
			AssertNotEquals("TransportMode shouldn't default to ALL from buyer supplier link", link.OrgSupBuyLinkTrnModes[0].PF_TransportMode, BO.JD_TransportMode);
			AssertEquals("ContainerMode", containerMode, BO.JD_ContainerMode);
			AssertEquals("IncoTerm", inco, BO.JD_IncoTerm);

			// ensure the currency defaults from the correct place
			BO.JD_RX_NKOrderCurrency = ZString.Empty;
			BO.SupplierPK = ZGuid.Empty;
			BO.SupplierPK = supplier.PK;
			BO.JD_TransportMode = "";
			BO.JD_TransportMode = transportMode;
			AssertEquals("Currency from Link", currency1.RX_Code, BO.JD_RX_NKOrderCurrency);
			AssertEquals("Rate from Link", 1.000m, BO.JD_EstimatedExchangeRate);

			link.OL_RX_NKDefaultCurrency = ZString.Empty;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = currency2.RX_Code;
			BO.JD_RX_NKOrderCurrency = ZString.Empty;
			BO.JD_TransportMode = "";
			BO.JD_TransportMode = transportMode;
			AssertEquals("Currency from MiscServ", currency2.RX_Code, BO.JD_RX_NKOrderCurrency);

			// ensure the carrier defaults from the correct place
			link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = ZGuid.Empty;
			BO.JD_ContainerMode = Constants.ContainerModes.LCL;
			BO.JD_OH_Carrier = ZGuid.Empty;
			BO.SupplierPK = ZGuid.Empty;
			BO.SupplierPK = supplier.PK;
			AssertEquals("no Carrier", ZGuid.Empty, BO.JD_OH_Carrier);

			link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = aIRCarrier.PK;
			BO.JD_OH_Carrier = ZGuid.Empty;
			BO.JD_ContainerMode = Constants.ContainerModes.AIR;
			AssertEquals("AIR Carrier", aIRCarrier.PK, BO.JD_OH_Carrier);

			link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = fCLCarrier.PK;
			BO.JD_OH_Carrier = ZGuid.Empty;
			BO.JD_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("FCL Carrier", fCLCarrier.PK, BO.JD_OH_Carrier);
		}

		public void TestOriginDestinationWillNotChangeOnImport()
		{
			Order order = Factory.New<Order>();

			((ISupportDataImporting)order).IsImportingData = true;
			((IBuyerSupplierRelationshipConsumer)order).Origin = "AUSYD";
			((IBuyerSupplierRelationshipConsumer)order).Destination = "USLAX";
			AssertEquals("AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("USLAX", order.JD_RL_NKGoodsDeliveredTo);

			((ISupportDataImporting)order).IsImportingData = false;
			((IBuyerSupplierRelationshipConsumer)order).Origin = "SGSIN";
			((IBuyerSupplierRelationshipConsumer)order).Destination = "HKHKG";
			AssertEquals("SGSIN", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("HKHKG", order.JD_RL_NKGoodsDeliveredTo);

			((ISupportDataImporting)order).IsImportingData = true;
			((IBuyerSupplierRelationshipConsumer)order).Origin = "AUSYD";
			((IBuyerSupplierRelationshipConsumer)order).Destination = "USLAX";
			AssertEquals("SGSIN", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("HKHKG", order.JD_RL_NKGoodsDeliveredTo);
		}

		public void TestDefaultBuyerFromSupplierUsingLink()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			AssertEquals("Supplier should be populated", supplier.PK, order.SupplierPK);
		}

		public void TestDefaultSupplierFromBuyerUsingLink()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			var order = Factory.New<Order>();
			order.SupplierPK = supplier.PK;
			AssertEquals("Supplier should be populated", buyer.PK, order.BuyerPK);
		}

		public void TestFilterDefaultsOn_SupplierList()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			Order order = Factory.New<Order>();
			order.SupplierPK = supplier.PK;
			AssertEquals(true, order.SupplierList.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestFilterDefaultsOn_BuyerList()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			AssertEquals(true, order.BuyerList.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestJD_OA_SupplierAddressSet_ShouldDeleteControllingCustomerAddress_WhenBuyerDoesNotHaveRelatedParty()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerAddress = buyer.Addresses.AddNewMainAddress();
			buyerAddress.Address1 = "16 Power Place Menai";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNewMainAddress();
			supplierAddress.Address1 = "4 Bindon Place";

			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = buyer.PK;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			AssertNotNull(order.ControllingCustomerDocAddress.Address);

			order.SupplierPK = supplier.PK;
			AssertEquals(Guid.Empty, order.ControllingCustomerDocAddress.OrganisationPK);
		}

		public void TestJD_OA_BuyerAddressSet_ShouldDeleteControllingCustomerAddress_WhenBuyerDoesNotHaveRelatedParty()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerAddress = buyer.Addresses.AddNewMainAddress();
			buyerAddress.Address1 = "16 Power Place Menai";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNewMainAddress();
			supplierAddress.Address1 = "4 Bindon Place";

			var order = Factory.NewWithValidTestData<Order>();
			order.SupplierPK = supplier.PK;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			AssertNotNull(order.ControllingCustomerDocAddress.Address);

			order.BuyerPK = buyer.PK;
			AssertEquals(Guid.Empty, order.ControllingCustomerDocAddress.OrganisationPK);
		}

		#endregion

		#region Test Defaults

		public void TestWarehouseDocAddressDefaultsFromWarehouseRelatedPartyOnBuyer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			AssertNull(order.WarehouseAddress);

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyRecord = buyer.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			relatedPartyRecord.PR_OH_RelatedParty = warehouse.PK;
			Factory.Save();

			order.BuyerPK = ZGuid.Empty;
			order.BuyerPK = buyer.PK;
			AssertEquals(warehouse.MainAddress, order.WarehouseAddress);
		}

		[TestDate(2006, 07, 07, 07, 49, 25)]
		public void TestSetDefaultValues()
		{
			Order testOrder = Factory.New<Order>();

			AssertEquals("JD_OrderDate", new ZDateTime(2006, 07, 07, 07, 49, 25), testOrder.JD_OrderDate);
			AssertEquals("JD_TransportMode", Core.Constants.TransportModes.Sea, testOrder.JD_TransportMode);
			AssertEquals("JD_ContainerMode", Constants.ContainerModes.FCL, testOrder.JD_ContainerMode);
			AssertEquals("JD_OrderStatus", Constants.OrderStatus.Incomplete, testOrder.JD_OrderStatus);
			AssertEquals("JD_RX_NKOrderCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testOrder.JD_RX_NKOrderCurrency);
			AssertEquals("JD_EstimatedExchangeRate", 1m, testOrder.JD_EstimatedExchangeRate);

			GlbDepartment.CurrentDepartment.GE_Sea = false;
			GlbDepartment.CurrentDepartment.GE_Air = true;
			testOrder = Factory.New<Order>();
			AssertEquals("JD_TransportMode", Core.Constants.TransportModes.Air, testOrder.JD_TransportMode);
		}

		public void TestSetContainerModeFromTransportMode()
		{
			BO.JD_TransportMode = Constants.TransportModes.Air;
			Assert(BO.JD_ContainerMode == Constants.ContainerModes.Loose);

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			Assert(BO.JD_ContainerMode == Constants.ContainerModes.FCL);
		}

		public void TestGoodsReceivedAt_DeliveredTo_PopulationFromBuyerSupplier()
		{
			Order bO = Factory.New<Order>();

			OrgHeader org = GetAOrg();
			bO.SupplierPK = org.PK;
			AssertEquals(org.OH_RL_NKClosestPort, bO.JD_RL_NKPortOfLoading);
			bO.SupplierPK = ZGuid.Empty;
			AssertEquals(org.OH_RL_NKClosestPort, bO.JD_RL_NKPortOfLoading);

			bO.BuyerPK = org.PK;
			AssertEquals(org.OH_RL_NKClosestPort, bO.JD_RL_NKPortOfDischarge);
			bO.BuyerPK = ZGuid.Empty;
			AssertEquals(org.OH_RL_NKClosestPort, bO.JD_RL_NKPortOfDischarge);
		}

		public void TestSettingSupplierAndBuyerDefaultsDestinationAndOrigin()
		{
			Order bO = Factory.New<Order>();

			OrgHeader org = GetAOrg();
			ZString port = "AAAAA";
			org.OH_RL_NKClosestPort = port;
			bO.SupplierPK = org.PK;
			AssertEquals("Expecting Origin to default from Buyer UNLOCO", org.OH_RL_NKClosestPort, bO.JD_RL_NKGoodsAvailableAt);
			bO.BuyerPK = org.PK;
			AssertEquals("Expecting Destination to default from Supplier UNLOCO", org.OH_RL_NKClosestPort, bO.JD_RL_NKGoodsDeliveredTo);
			bO.BuyerPK = ZGuid.Empty;
			bO.SupplierPK = ZGuid.Empty;
			AssertEquals("Destination Should remain even though buyer gone", port, bO.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("Origin Should remain even though Supplier gone", port, bO.JD_RL_NKGoodsAvailableAt);
			OrgHeader org2 = GetAOrg();
			org2.OH_RL_NKClosestPort = "BBBBB";
			bO.BuyerPK = org2.PK;
			bO.SupplierPK = org2.PK;
			AssertEquals("Destination Should remain even though Buyer different", port, bO.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("Origin Should remain even though Supplier Different", port, bO.JD_RL_NKGoodsAvailableAt);
		}

		public void TestSettingSendingReceivingAgent()
		{
			#region Setup Orgs

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.OH_Code = "BUYAU";
			buyer.Addresses[0].OA_Address1 = "Buyer";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Supplier";
			supplier.OH_Code = "SUPAU";
			supplier.Addresses[0].OA_Address1 = "Supplier";

			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "Test Sending Agent";
			sendingAgent.OH_Code = "TSAAU";
			sendingAgent.Addresses[0].OA_Address1 = "SendingAgent";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "Test Receiving Agent";
			receivingAgent.OH_Code = "TRAAU";
			receivingAgent.Addresses[0].OA_Address1 = "ReceivingAgent";

			OrgHeader appointedAgent = Factory.New<OrgHeader>();
			appointedAgent.OH_FullName = "Test Appointed Agent";
			appointedAgent.OH_Code = "TAAAU";
			appointedAgent.Addresses[0].OA_Address1 = "AppointedAgent";

			OrgHeader publishedAgent = Factory.New<OrgHeader>();
			publishedAgent.OH_FullName = "Test Published Agent";
			publishedAgent.OH_Code = "TPAAU";
			publishedAgent.Addresses[0].OA_Address1 = "PublishedAgent";

			ZString localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			buyer.OH_RL_NKClosestPort = localPort;
			supplier.OH_RL_NKClosestPort = localPort;

			OrgAppointedAgentPorts newAppointedAgentPort = appointedAgent.AppointedAgentPorts.AddNew();
			newAppointedAgentPort.O5_SeaAgentStatus = "APP";
			newAppointedAgentPort.O5_AirAgentStatus = "APP";
			newAppointedAgentPort.O5_PortOrCountry = localPort;
			newAppointedAgentPort.O5_OA_AgentOfficeAddress = appointedAgent.MainAddress.PK;
			appointedAgent.OH_IsForwarder = true;
			appointedAgent.OH_RL_NKClosestPort = localPort;

			publishedAgent.OH_IsForwarder = true;
			OrgAppointedAgentPorts newPubAgentPort = publishedAgent.AppointedAgentPorts.AddNew();
			newPubAgentPort.O5_SeaAgentStatus = "PUB";
			newPubAgentPort.O5_AirAgentStatus = "PUB";
			newPubAgentPort.O5_PortOrCountry = localPort;
			newPubAgentPort.O5_OA_AgentOfficeAddress = publishedAgent.MainAddress.PK;
			publishedAgent.OH_RL_NKClosestPort = localPort;

			OrgSupplierBuyerLink supplierLink = buyer.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = supplier.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_OH_ReceivingAgent = receivingAgent.PK;
			supplier.BuyerLinks[0].OrgSupBuyLinkTrnModes[0].PF_OH_SendingAgent = sendingAgent.PK;

			Factory.Save();

			#endregion

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "123213123";
			order1.BuyerPK = buyer.PK;
			AssertEquals("Should set Supplier from Supplier Link", supplier.PK, order1.SupplierPK);
			AssertEquals("Should set Receiving Agent from Supplier Link", receivingAgent.PK, order1.JD_OH_ReceivingAgent);
			AssertEquals("Should set Sending Agent from Buyer Link", sendingAgent.PK, order1.JD_OH_SendingAgent);

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "01420214";
			buyer.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_OH_ReceivingAgent = ZGuid.Empty;
			supplier.BuyerLinks[0].OrgSupBuyLinkTrnModes[0].PF_OH_SendingAgent = ZGuid.Empty;
			order2.BuyerPK = buyer.PK;
			AssertEquals("Should default from the port's published agent", publishedAgent.PK, order2.JD_OH_ReceivingAgent);

			publishedAgent.OH_IsForwarder = false;
			Factory.Save();

			Order order3 = Factory.New<Order>();
			order3.JD_OrderNumber = "3214234";
			order3.BuyerPK = buyer.PK;

			AssertEquals("Should default from the Appointed agent if published agent isn't marked as forwarder anymore", appointedAgent.PK, order3.JD_OH_ReceivingAgent);

			order2.JD_OH_ReceivingAgent = ZGuid.Empty;
			order2.JD_OH_SendingAgent = ZGuid.Empty;
			publishedAgent.Delete();

			Factory.Save();
			Order order4 = Factory.New<Order>();
			order4.BuyerPK = buyer.PK;
			AssertEquals("should default from the port's appointed agent", appointedAgent.PK, order4.JD_OH_ReceivingAgent);
		}

		public void TestSettingSendingAndReceivingAgentsFromTransport()
		{
			#region Setup Orgs

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.OH_Code = "BUYAU";
			buyer.Addresses[0].OA_Address1 = "Buyer";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Supplier";
			supplier.OH_Code = "SUPAU";
			supplier.Addresses[0].OA_Address1 = "Supplier";

			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "Test Sending Agent";
			sendingAgent.OH_Code = "TSAAU";
			sendingAgent.Addresses[0].OA_Address1 = "SendingAgent";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "Test Receiving Agent";
			receivingAgent.OH_Code = "TRAAU";
			receivingAgent.Addresses[0].OA_Address1 = "ReceivingAgent";

			OrgHeader publishedAgent = Factory.New<OrgHeader>();
			publishedAgent.OH_FullName = "Test Published Agent";
			publishedAgent.OH_Code = "TPAAU";
			publishedAgent.Addresses[0].OA_Address1 = "PublishedAgent";

			ZString localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			buyer.OH_RL_NKClosestPort = localPort;
			supplier.OH_RL_NKClosestPort = localPort;

			OrgAppointedAgentPorts newAppAgPorts = publishedAgent.AppointedAgentPorts.AddNew();
			newAppAgPorts.O5_PortOrCountry = localPort;
			newAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			newAppAgPorts.O5_OA_AgentOfficeAddress = publishedAgent.MainAddress.PK;
			publishedAgent.OH_IsForwarder = true;
			publishedAgent.OH_RL_NKClosestPort = localPort;

			Factory.Save();

			#endregion

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "123213123";
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = supplier.PK;
			order1.JD_OH_SendingAgent = sendingAgent.PK;
			order1.JD_OH_ReceivingAgent = receivingAgent.PK;
			Factory.Save();

			AssertEquals("Shouldn't default to Published Air Agent", sendingAgent.PK, order1.SendingAgent.PK);
			AssertEquals("Shouldn't default to Published Air Agent", receivingAgent.PK, order1.ReceivingAgent.PK);

			order1.JD_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Should default to Published Air Agent", publishedAgent.OH_FullName, order1.SendingAgent.OH_FullName);
			AssertEquals("Should default to Published Air Agent", publishedAgent.OH_FullName, order1.ReceivingAgent.OH_FullName);

			OrgSupplierBuyerLink link = buyer.SupplierLinks.AddNew(supplier);
			link.OrgSupBuyLinkTrnModes[0].PF_OH_SendingAgent = sendingAgent.PK;
			link.OrgSupBuyLinkTrnModes[0].PF_OH_ReceivingAgent = receivingAgent.PK;
			Factory.Save();

			Order order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			AssertEquals("Supplier should be defaulted to buyer/supplier relationship", supplier.PK, order2.SupplierPK);
			AssertEquals("Sending Agent should default to buyer/supplier relationship", sendingAgent.PK, order2.JD_OH_SendingAgent);
			AssertEquals("Receiving Agent should default to buyer/supplier relationship", receivingAgent.PK, order2.JD_OH_ReceivingAgent);
		}

		#endregion

		#region ITemplateCopy

		public void TestDateTimesClearedOnTemplateCopy()
		{
			BO.JD_BookingConfDate = ZDateTime.Now;
			BO.JD_OrderDate = new ZDateTime(2005, 11, 26);
			Order copiedOrder = (Order)BO.TemplateCopy();
			AssertEquals("DateTimes Cleared on Copy", ZDateTime.Empty, copiedOrder.JD_BookingConfDate);
			AssertZDatesWithin5Minutes("Order date not cleared and reset to current date/time", ZDateTime.Now, copiedOrder.JD_OrderDate);
		}

		public void TestForeignKeysClearedOnTemplateCopy()
		{
			BO.JD_JS = ZGuid.NewZGuid();
			BO.JD_JE = ZGuid.NewZGuid();
			BO.JD_EF_ShipmentPrePlanning = ZGuid.NewZGuid();

			Order copiedOrder = (Order)BO.TemplateCopy();
			AssertEquals("FKs to Shipment, Dec and Pre-Advice cleared", ZGuid.Empty, copiedOrder.JD_JS);
			AssertEquals("FKs to Shipment, Dec and Pre-Advice cleared", ZGuid.Empty, copiedOrder.JD_JE);
			AssertEquals("FKs to Shipment, Dec and Pre-Advice cleared", ZGuid.Empty, copiedOrder.JD_EF_ShipmentPrePlanning);
		}

		public void TestCopyingOrderDetails()
		{
			BO.JD_InvoiceNumber = "1234";
			BO.JD_InvoiceDate = ZDateTime.Today;

			BO.JD_Waybill = "WB12345";
			BO.JD_MasterWaybill = "MWB12345";

			BO.JD_RV_NKArrivalVessel = "Arrival Vessel";
			BO.JD_RV_NKIntermediateVessel = "Itermediate Vessel";
			BO.JD_RV_NKDepartureVessel = "Departure Vessel";

			BO.JD_ArrivalVoyage = "ArrVoyage";
			BO.JD_IntermediateVoyage = "IterVoyage";
			BO.JD_DepartureVoyage = "DepVoyage";

			BO.JD_RL_NKPortOfLoading = "AUSYD";
			BO.JD_RL_NKPortOfDischarge = "AUBNE";

			BO.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			BO.JD_RL_NKGoodsAvailableAt = "AUBNE";

			BO.JD_OH_Carrier = GlbCompany.CurrentCompany.OrgProxy.PK;

			BO.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);

			OrderLine orderLine = BO.OrderLines.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			orderLine.ManufacturerNameOrPK = org.PK.ToString();
			orderLine.JO_CommercialInvoiceNo = "123";
			Factory.Save();

			OrderLineDelivery orderLineDelivery = orderLine.Deliveries.AddNew();
			orderLineDelivery.J4_RL_NKDestinationPort = "AUSYD";
			orderLineDelivery.J4_OA_NKDeliveryPoint = "Point";

			ProcessTask milestone = BO.WorkflowItems.Milestones.AddNew();
			milestone.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddDays(2));
			milestone.SetMilestoneActualDateForTest(ZDateTimeOffset.Now.AddDays(1));

			Order copiedOrder = (Order)BO.TemplateCopy();

			AssertEquals(ZGuid.Empty, copiedOrder.JD_OH_Carrier);

			AssertEquals(ZString.Empty, copiedOrder.JD_InvoiceNumber);
			AssertEquals(ZDateTime.Empty, copiedOrder.JD_InvoiceDate);

			AssertEquals(ZString.Empty, copiedOrder.JD_Waybill);
			AssertEquals(ZString.Empty, copiedOrder.JD_MasterWaybill);

			AssertEquals(ZString.Empty, copiedOrder.JD_RV_NKArrivalVessel);
			AssertEquals(ZString.Empty, copiedOrder.JD_RV_NKIntermediateVessel);
			AssertEquals(ZString.Empty, copiedOrder.JD_RV_NKDepartureVessel);

			AssertEquals(ZString.Empty, copiedOrder.JD_ArrivalVoyage);
			AssertEquals(ZString.Empty, copiedOrder.JD_IntermediateVoyage);
			AssertEquals(ZString.Empty, copiedOrder.JD_DepartureVoyage);

			ProcessTask copiedMilestone = copiedOrder.WorkflowItems.Milestones[Events.OrderConfirmed];
			AssertEquals(ZDateTime.Empty, copiedMilestone.P9_ScheduledDate.ToZDateTime());
			AssertEquals(ZDateTime.Empty, copiedMilestone.P9_ActualDate.ToZDateTime());

			AssertEquals(1, copiedOrder.OrderLines.Count);
			AssertEquals(ZString.Empty, copiedOrder.OrderLines[0].JO_CommercialInvoiceNo);

			AssertEquals(1, copiedOrder.OrderLines[0].Deliveries.Count);
			AssertEquals("AUSYD", copiedOrder.OrderLines[0].Deliveries[0].J4_RL_NKDestinationPort);
			AssertEquals("Point", copiedOrder.OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint);

			AssertEquals("AUSYD", copiedOrder.JD_RL_NKPortOfLoading);
			AssertEquals("AUBNE", copiedOrder.JD_RL_NKPortOfDischarge);

			AssertEquals("AUSYD", copiedOrder.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("AUBNE", copiedOrder.JD_RL_NKGoodsAvailableAt);

			var orderManufacturer = BO.DocAddresses.FindDocAddressesByType(DocAddressType.Manufacturer)[0];
			var copiedOrderManufacturer = copiedOrder.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);

			CombineAssertions(() =>
			{
				Assert(copiedOrderManufacturer != null);
				AssertEquals(orderManufacturer.CompanyName, copiedOrderManufacturer.CompanyName);
				AssertEquals(orderManufacturer.Address, copiedOrderManufacturer.Address);
			});
		}

		public void TestTemplateCopy_BuyerSpecificOrderNumber()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			org.MiscServ.OM_IMLastOrderReference = "13";

			Order order = Factory.New<Order>();
			AssertEquals(ZString.Empty, order.JD_OrderNumber);
			order.BuyerPK = org.PK;
			AssertEquals("14", order.JD_OrderNumber);
			Factory.Save();

			Order newOrder = (Order)order.TemplateCopy();
			AssertEquals("15", newOrder.JD_OrderNumber);
		}

		#endregion

		public void TestGetNameOfRelatedItem_CombinedTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var consol3 = shipment.Consols.AddNew();
			var consol4 = Factory.New<ForwardingConsol>();

			var transport = shipment.Transports.AddNew();
			transport.JW_ETD = ZDateTime.Now.AddDays(1);
			transport.JW_ETA = ZDateTime.Now.AddDays(2);

			var order = Factory.New<Order>();
			order.JD_JS = shipment.PK;

			var relatedItemsNameProvider = (IRelatedItemsNameProvider)order;
			AssertEquals(ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(order));
			AssertEquals("Master", relatedItemsNameProvider.GetNameOfRelatedItem(shipment));
			AssertEquals("Master, Departure Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol1));
			AssertEquals("Master, Transship. Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol2));
			AssertEquals("Master, Arrival Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol3));
			AssertEquals(ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(consol4));
			AssertEquals("No related item name for transport (for now)", ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(transport));

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var order2 = Factory.New<Order>();
			order2.JD_JE = declaration.PK;

			relatedItemsNameProvider = order2;
			AssertEquals(ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(order2));
			AssertEquals("Master", relatedItemsNameProvider.GetNameOfRelatedItem(shipment));
			AssertEquals("Master, Departure Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol1));
			AssertEquals("Master, Transship. Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol2));
			AssertEquals("Master, Arrival Consol", relatedItemsNameProvider.GetNameOfRelatedItem(consol3));
			AssertEquals(ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(consol4));
			AssertEquals("No related item name for transport (for now)", ZString.Empty, relatedItemsNameProvider.GetNameOfRelatedItem(transport));
		}

		#region Testing Dates

		public void TestUnusedVoyageDatesAreCleared()
		{
			BO.JD_DepartureVoyage = "A123";
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_E_ARV_1stIntermediate = new ZDateTime(DateTime.Now.AddDays(10));
			BO.JD_IntermediateVoyage = "B123";
			BO.JD_E_DEP_2 = new ZDateTime(DateTime.Now.AddDays(30));
			BO.JD_E_ARV_2ndIntermediate = new ZDateTime(DateTime.Now.AddDays(45));
			BO.JD_ArrivalVoyage = "C123";
			BO.JD_E_DEP_3 = new ZDateTime(DateTime.Now.AddDays(50));
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddDays(60);

			Assert("JD_E_ARV_1stIntermediate is in use", !BO.JD_E_ARV_1stIntermediate.IsEmpty);
			Assert("JD_E_DEP_2 is in use", !BO.JD_E_DEP_2.IsEmpty);
			Assert("JD_E_ARV_2ndIntermediate is in use", !BO.JD_E_ARV_2ndIntermediate.IsEmpty);
			Assert("JD_E_DEP_3 is in use", !BO.JD_E_DEP_3.IsEmpty);

			BO.JD_IntermediateVoyage = ZString.Empty;

			Assert("JD_E_ARV_1stIntermediate is in use", !BO.JD_E_ARV_1stIntermediate.IsEmpty);
			Assert("JD_E_DEP_2 is not in use", BO.JD_E_DEP_2.IsEmpty);
			Assert("JD_E_ARV_2ndIntermediate is not in use", BO.JD_E_ARV_2ndIntermediate.IsEmpty);
			Assert("JD_E_DEP_3 is in use", !BO.JD_E_DEP_3.IsEmpty);

			BO.JD_ArrivalVoyage = ZString.Empty;

			Assert("JD_E_ARV_1stIntermediate is not in use", BO.JD_E_ARV_1stIntermediate.IsEmpty);
			Assert("JD_E_DEP_2 is not in use", BO.JD_E_DEP_2.IsEmpty);
			Assert("JD_E_ARV_2ndIntermediate is not in use", BO.JD_E_ARV_2ndIntermediate.IsEmpty);
			Assert("JD_E_DEP_3 is not in use", BO.JD_E_DEP_3.IsEmpty);
		}

		public void TestJD_Milestone_E_ARV()
		{
			Order order = Factory.New<Order>();
			var now = ZDateTime.SmallDateTimeNow;
			order.JD_Milestone_E_ARV = now;
			AssertEquals("JD_Milestone_E_ARV should be CurrentDate", now, order.JD_Milestone_E_ARV);
		}

		public void TestJD_Milestone_E_DEP()
		{
			Order order = Factory.New<Order>();
			var now = ZDateTime.SmallDateTimeNow;
			order.JD_Milestone_E_DEP = now;
			AssertEquals("JD_Milestone_E_DEP should be CurrentDate", now, order.JD_Milestone_E_DEP);
		}

		public void TestProxiedDates()
		{
			Order order = Factory.New<Order>();
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_EXW);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_EXW);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_GIW);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_GIW);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_CCC);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_CCC);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_CLR);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_CLR);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_CAV);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_CAV);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_DCA);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_DCA);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_E_DCF);
			AssertEquals(ZDateTime.Empty, order.JD_Milestone_A_DCF);
			var currentDate = ZDateTime.SmallDateTimeNow;
			var currentDateOffset = currentDate.ToOffset();
			order.UpdateEventEstimate(Events.ExWorks, currentDateOffset);
			order.UpdateEvent(Events.ExWorks, currentDateOffset.AddDays(1));
			order.UpdateEventEstimate(Events.GateIn, currentDateOffset.AddDays(2));
			order.UpdateEvent(Events.GateIn, currentDateOffset.AddDays(3));
			order.UpdateEventEstimate(Events.CustomsCommenced, currentDateOffset.AddDays(4));
			order.UpdateEvent(Events.CustomsCommenced, currentDateOffset.AddDays(5));
			order.UpdateEventEstimate(Events.CustomsCleared, currentDateOffset.AddDays(6));
			order.UpdateEvent(Events.CustomsCleared, currentDateOffset.AddDays(7));
			order.UpdateEventEstimate(Events.CargoAvailable, currentDateOffset.AddDays(8));
			order.UpdateEvent(Events.CargoAvailable, currentDateOffset.AddDays(9));
			order.UpdateEventEstimate(Events.DeliveryCartageAdvised, currentDateOffset.AddDays(10));
			order.UpdateEvent(Events.DeliveryCartageAdvised, currentDateOffset.AddDays(11));
			order.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, currentDateOffset.AddDays(12));
			order.UpdateEvent(Events.DeliveryCartageCompleteFinalised, currentDateOffset.AddDays(13));
			AssertEquals(currentDate, order.JD_Milestone_E_EXW);
			AssertEquals(currentDate.AddDays(1), order.JD_Milestone_A_EXW);
			AssertEquals(currentDate.AddDays(2), order.JD_Milestone_E_GIW);
			AssertEquals(currentDate.AddDays(3), order.JD_Milestone_A_GIW);
			AssertEquals(currentDate.AddDays(4), order.JD_Milestone_E_CCC);
			AssertEquals(currentDate.AddDays(5), order.JD_Milestone_A_CCC);
			AssertEquals(currentDate.AddDays(6), order.JD_Milestone_E_CLR);
			AssertEquals(currentDate.AddDays(7), order.JD_Milestone_A_CLR);
			AssertEquals(currentDate.AddDays(8), order.JD_Milestone_E_CAV);
			AssertEquals(currentDate.AddDays(9), order.JD_Milestone_A_CAV);
			AssertEquals(currentDate.AddDays(10), order.JD_Milestone_E_DCA);
			AssertEquals(currentDate.AddDays(11), order.JD_Milestone_A_DCA);
			AssertEquals(currentDate.AddDays(12), order.JD_Milestone_E_DCF);
			AssertEquals(currentDate.AddDays(13), order.JD_Milestone_A_DCF);
			order.JD_Milestone_E_EXW = currentDate.AddDays(14);
			order.JD_Milestone_A_EXW = currentDate.AddDays(15);
			order.JD_Milestone_E_GIW = currentDate.AddDays(16);
			order.JD_Milestone_A_GIW = currentDate.AddDays(17);
			order.JD_Milestone_E_CCC = currentDate.AddDays(18);
			order.JD_Milestone_A_CCC = currentDate.AddDays(19);
			order.JD_Milestone_E_CLR = currentDate.AddDays(20);
			order.JD_Milestone_A_CLR = currentDate.AddDays(21);
			order.JD_Milestone_E_CAV = currentDate.AddDays(22);
			order.JD_Milestone_A_CAV = currentDate.AddDays(23);
			order.JD_Milestone_E_DCA = currentDate.AddDays(24);
			order.JD_Milestone_A_DCA = currentDate.AddDays(25);
			order.JD_Milestone_E_DCF = currentDate.AddDays(26);
			order.JD_Milestone_A_DCF = currentDate.AddDays(27);
			AssertEquals(currentDate.AddDays(14), order.JD_Milestone_E_EXW);
			AssertEquals(currentDate.AddDays(15), order.JD_Milestone_A_EXW);
			AssertEquals(currentDate.AddDays(16), order.JD_Milestone_E_GIW);
			AssertEquals(currentDate.AddDays(17), order.JD_Milestone_A_GIW);
			AssertEquals(currentDate.AddDays(18), order.JD_Milestone_E_CCC);
			AssertEquals(currentDate.AddDays(19), order.JD_Milestone_A_CCC);
			AssertEquals(currentDate.AddDays(20), order.JD_Milestone_E_CLR);
			AssertEquals(currentDate.AddDays(21), order.JD_Milestone_A_CLR);
			AssertEquals(currentDate.AddDays(22), order.JD_Milestone_E_CAV);
			AssertEquals(currentDate.AddDays(23), order.JD_Milestone_A_CAV);
			AssertEquals(currentDate.AddDays(24), order.JD_Milestone_E_DCA);
			AssertEquals(currentDate.AddDays(25), order.JD_Milestone_A_DCA);
			AssertEquals(currentDate.AddDays(26), order.JD_Milestone_E_DCF);
			AssertEquals(currentDate.AddDays(27), order.JD_Milestone_A_DCF);
		}

		public void TestEstimatedDates()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = HomePort;
			shipment1.JS_RL_NKDestination = OverseasPort;

			ZDateTime currentDate = ZDateTime.SmallDateTimeNow;

			Order order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_JS = shipment1.PK;

			CommonConsol consol1 = shipment1.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = HomePort;
			consol1.JK_RL_NKDischargePort = OverseasPort;

			Factory.Save();

			Transport transport1a = consol1.Transports[0];
			transport1a.JW_RL_NKLoadPort = HomePort;
			transport1a.JW_RL_NKDiscPort = "";
			transport1a.JW_ETD = currentDate.AddDays(1);
			transport1a.JW_ETA = currentDate.AddDays(4);

			Transport transport1b = consol1.Transports.AddNew();
			transport1b.JW_RL_NKLoadPort = "";
			transport1b.JW_RL_NKDiscPort = OverseasPort;
			transport1b.JW_ETD = currentDate.AddDays(6);
			transport1b.JW_ETA = currentDate.AddDays(19);

			Factory.Save();

			AssertEquals("ETD should come from Consol", transport1a.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("ETA have come from Consol", transport1b.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			consol1.JK_RL_NKDischargePort = OverseasPort2;
			transport1b.JW_ETA = currentDate.AddDays(9);

			CommonConsol consol2 = shipment1.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = OverseasPort2;
			consol2.JK_RL_NKDischargePort = OverseasPort;

			Transport transport2a = consol2.Transports[0];
			transport2a.JW_RL_NKLoadPort = OverseasPort2;
			transport2a.JW_RL_NKDiscPort = "";
			transport2a.JW_ETD = currentDate.AddDays(11);
			transport2a.JW_ETA = ZDateTime.Now.AddDays(14);

			Transport transport2b = consol2.Transports.AddNew();
			transport2b.JW_RL_NKLoadPort = "";
			transport2b.JW_RL_NKDiscPort = OverseasPort;
			transport2b.JW_ETD = currentDate.AddDays(16);
			transport2b.JW_ETA = currentDate.AddDays(19);

			Factory.Save();

			AssertEquals("Should be Shipments Arrival Consol", shipment1.ArrivalConsol, consol2);
			AssertEquals("Should be Shipments Departure Consol", shipment1.DepartureConsol, consol1);
			AssertEquals("Should be from Departure Consol", transport1a.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("Should be from Arrival Consol", transport2b.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			Transport transport0 = shipment1.Transports.AddNew();
			transport0.JW_RL_NKLoadPort = HomePort;
			transport0.JW_RL_NKDiscPort = "";
			transport0.JW_ETD = currentDate.AddDays(-1);
			transport0.JW_ETA = currentDate;

			Transport transport3 = shipment1.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "";
			transport3.JW_RL_NKDiscPort = OverseasPort;
			transport3.JW_ETD = currentDate.AddDays(20);
			transport3.JW_ETA = currentDate.AddDays(21);

			Factory.Save();

			AssertEquals("Should be from first shipment routing leg", transport0.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("Should be from last shipment routing leg", transport3.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
		}

		public void TestActualDates()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = HomePort;
			shipment1.JS_RL_NKDestination = OverseasPort;

			ZDateTime currentDate = ZDateTime.Now;

			Order order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_JS = shipment1.PK;

			CommonConsol consol1 = shipment1.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = HomePort;
			consol1.JK_RL_NKDischargePort = OverseasPort;

			Factory.Save();

			Transport transport1a = consol1.Transports[0];
			transport1a.JW_RL_NKLoadPort = HomePort;
			transport1a.JW_RL_NKDiscPort = "";
			transport1a.JW_ATD = currentDate.AddDays(1);
			transport1a.JW_ATA = currentDate.AddDays(4);

			Transport transport1b = consol1.Transports.AddNew();
			transport1b.JW_RL_NKLoadPort = "";
			transport1b.JW_RL_NKDiscPort = OverseasPort;
			transport1b.JW_ATD = currentDate.AddDays(6);
			transport1b.JW_ATA = currentDate.AddDays(19);

			Factory.Save();

			AssertEquals("ATD should come from Consol", transport1a.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("ATA have come from Consol", transport1b.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			consol1.JK_RL_NKDischargePort = OverseasPort2;
			transport1b.JW_RL_NKDiscPort = OverseasPort2;
			transport1b.JW_ATA = currentDate.AddDays(9);

			CommonConsol consol2 = shipment1.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = OverseasPort2;
			consol2.JK_RL_NKDischargePort = OverseasPort;

			Transport transport2a = consol2.Transports[0];
			transport2a.JW_RL_NKLoadPort = OverseasPort2;
			transport2a.JW_RL_NKDiscPort = "";
			transport2a.JW_ATD = currentDate.AddDays(11);
			transport2a.JW_ATA = currentDate.AddDays(14);

			Transport transport2b = consol2.Transports.AddNew();
			transport2b.JW_RL_NKLoadPort = "";
			transport2b.JW_RL_NKDiscPort = OverseasPort;
			transport2b.JW_ATD = currentDate.AddDays(16);
			transport2b.JW_ATA = currentDate.AddDays(19);

			Factory.Save();

			AssertEquals("Should be Shipments Arrival Consol", shipment1.ArrivalConsol, consol2);
			AssertEquals("Should be Shipments Departure Consol", shipment1.DepartureConsol, consol1);
			AssertEquals("Should be from Departure Consol", transport1a.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("Should be from Arrival Consol", transport2b.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			Transport transport0 = shipment1.Transports.AddNew();
			transport0.JW_RL_NKLoadPort = HomePort;
			transport0.JW_RL_NKDiscPort = "";
			transport0.JW_ATD = currentDate.AddDays(-1);
			transport0.JW_ATA = currentDate;

			Transport transport3 = shipment1.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "";
			transport3.JW_RL_NKDiscPort = OverseasPort;
			transport3.JW_ATD = currentDate.AddDays(20);
			transport3.JW_ATA = currentDate.AddDays(21);

			Factory.Save();

			AssertEquals("Should be from original shipment routing leg", transport1a.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("Should be from original shipment routing leg", transport2b.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
		}

		public void TestEventReferenceParameters()
		{
			var order = Factory.New<Order>();
			order.JD_RL_NKPortOfLoading = HomePort;
			order.JD_RL_NKPortOfDischarge = OverseasPort4;

			order.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Now);
			AssertEquals("Should be load port from order", HomePort, order.GetLogs().MostRecentLogByEventTime(Events.Departure).Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			order.UpdateEventEstimate(Events.Arrival, ZDateTimeOffset.Now);
			AssertEquals("Should be discharge port from order", OverseasPort4, order.GetLogs().MostRecentLogByEventTime(Events.Arrival).Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort3;
			order.JD_JS = shipment.PK;

			order.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Now);
			AssertEquals("Does not contain location because no departure consol", false, order.GetLogs().MostRecentLogByEventTime(Events.Departure).Parameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.Location));
			order.UpdateEventEstimate(Events.Arrival, ZDateTimeOffset.Now);
			AssertEquals("Does not contain location because no arrival consol", false, order.GetLogs().MostRecentLogByEventTime(Events.Arrival).Parameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.Location));

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = OverseasPort;
			departureConsol.JK_RL_NKDischargePort = OverseasPort2;

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = OverseasPort2;
			arrivalConsol.JK_RL_NKDischargePort = OverseasPort3;

			order.UpdateEventEstimate(Events.Departure, ZDateTimeOffset.Now);
			AssertEquals("Should be load port from departure consol", OverseasPort, order.GetLogs().MostRecentLogByEventTime(Events.Departure).Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			order.UpdateEventEstimate(Events.Arrival, ZDateTimeOffset.Now);
			AssertEquals("Should be discharge port from arrival consol", OverseasPort3, order.GetLogs().MostRecentLogByEventTime(Events.Arrival).Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			order.UpdateEventEstimate(Events.GateIn, ZDateTimeOffset.Now);
			AssertEquals("Should be terminal", EventConstants.Facilities.Code.Terminal, order.GetLogs().MostRecentLogByEventTime(Events.GateIn).Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		public void TestDatesWhenDeclerationAttached()
		{
			var currentDate = ZDateTime.SmallDateTimeNow;
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			Order order1 = Factory.New<Order>();
			order1.UpdateEventEstimate(Events.Arrival, currentDate.AddDays(10).ToOffset());
			order1.UpdateEventEstimate(Events.Departure, currentDate.AddDays(10).ToOffset());
			declaration[JobDeclarationSchema.JE_RL_NKOrigin.Name] = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			declaration[JobDeclarationSchema.JE_MessageType.Name] = "EXP";
			var transport = ((IRoutingSupport)declaration).Transports.AddNew();
			transport.JW_ATD = currentDate;
			transport.JW_ATA = currentDate;

			//Arrival Date
			declaration[JobDeclarationSchema.JE_DateAtFinalDestination.Name] = currentDate;
			//Departure Date
			declaration[JobDeclarationSchema.JE_DateAtOrigin.Name] = currentDate;
			//Delivery Date
			((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_DeliveryCartageCompleted = currentDate;
			((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_EstimatedDelivery = currentDate;
			//Cartage Advised
			((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_DeliveryCartageAdvised = currentDate;
			order1.JD_JE = declaration.PK;
			var bob = order1.GetMilestoneActualDate(Events.DeliveryCartageAdvised);
			AssertEquals("Departure Date should default from the order when decleration attached", currentDate, order1.GetMilestoneActualDate(Events.Departure).ToZDateTime());

			AssertEquals("Delivered Date shouldn't default to the order when an export decleration attached", true, currentDate != order1.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());

			AssertEquals("Delivered Date shouldn't default to the order when an export decleration attached", true, currentDate != order1.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());

			AssertEquals("Arrival Date should default to the order when declaration attached", currentDate, order1.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			AssertEquals("Cartage Advised Date should not default to the order when an Export declaration attached", true, currentDate != order1.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime());

			AssertEquals("JD_E_ARV Should be Dec Final Arrival Date", currentDate, order1.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			AssertEquals("JD_E_DEP Should be Dec Final Arrival Date", currentDate, order1.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());

			declaration[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
			order1.JD_JE = ZGuid.Empty;
			order1.JD_JE = declaration.PK;

			AssertEquals("Delivered Date should default to the order when an import decleration attached", currentDate, order1.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());

			AssertEquals("Estimated Delivered Date should default to the order when an import decleration attached", currentDate, order1.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());

			AssertEquals("Cartage Advised Date should default to the order when an Import Declaration attached", currentDate, order1.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime());
		}

		public void TestProxyDatesWhenShipmentAttached()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBB";
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var transport = shipment.TransportsIncludingRelated.AddNew();
			transport.JW_ATD = new ZDateTime(2013, 06, 09);
			transport.JW_ATA = new ZDateTime(2013, 06, 12);
			transport.JW_ETD = new ZDateTime(2013, 06, 09);
			transport.JW_ETA = new ZDateTime(2013, 06, 11);

			var currentDate = ZDateTimeOffset.Now;
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "1";
			order1.SupplierPK = org2.PK;
			order1.BuyerPK = org1.PK;
			order1.UpdateEvent(Events.Arrival, currentDate);
			order1.UpdateEvent(Events.Departure, currentDate);
			order1.UpdateEventEstimate(Events.Arrival, currentDate);
			order1.UpdateEventEstimate(Events.Departure, currentDate);
			order1.JD_JS = shipment.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var order2 = newFactory.Load<Order>(order1.PK);

			AssertEquals("Date ETA should come from Shipment", new ZDateTime(2013, 06, 11), order2.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("Date ETD should come from Shipment", new ZDateTime(2013, 06, 09), order2.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("Date ATA should come from Shipment", new ZDateTime(2013, 06, 12), order2.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals("Date ATD should come from Shipment", new ZDateTime(2013, 06, 09), order2.GetMilestoneActualDate(Events.Departure).ToZDateTime());
		}

		public void TestCustomsDatesProxyFromShipmentOrDeclarationCorrectly()
		{
			TestCustomsDatesProxyFromShipmentOrDeclarationCorrectly(Events.CustomsCommenced, Events.CustomsCleared);
		}

		public void TestCustomsDatesProxyFromShipmentOrDeclarationCorrectly_ForExportDeclaration()
		{
			TestCustomsDatesProxyFromShipmentOrDeclarationCorrectly(Events.ExportCustomsCommenced, Events.ExportCustomsCleared);
		}

		// TODO: Is it possible to seperate into 2 test case?
		void TestCustomsDatesProxyFromShipmentOrDeclarationCorrectly(Event customsCommencedEvent, Event customsClearedEvent)
		{
			var currentTime = new ZDateTimeOffset(2013, 07, 16, 5, 4, 1);

			var shipment = CommonShipment.New(Factory);
			var log1 = shipment.GetLogs().AddNew(new EventValue(customsCommencedEvent, eventTime: currentTime));
			var log2 = shipment.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(1)));
			var log3 = shipment.GetLogs().AddNew(new EventValue(customsCommencedEvent, eventTime: currentTime.AddDays(3)));
			var log4 = shipment.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(5), isEstimate: ZBool.True));
			var log5 = shipment.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(10)));
			log5.Cancel();

			var order1 = Factory.New<Order>();
			order1.JD_JS = shipment.PK;
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var expectedCustomsCommencedDate = log3.SL_EventTime.AddSeconds(-log3.SL_EventTime.Second).AddMilliseconds(-log3.SL_EventTime.Millisecond);
			var expectedCustomsClearedDate = log2.SL_EventTime.AddSeconds(-log2.SL_EventTime.Second).AddMilliseconds(-log2.SL_EventTime.Millisecond);

			AssertEquals("Customs Clearance Commenced date should be the most recent event from the shipment not an estimate or cancelled", expectedCustomsCommencedDate, order1.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Cleared Customs date should come from the shipment event", expectedCustomsClearedDate, order1.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			log1 = declaration.GetLogs().AddNew(new EventValue(customsCommencedEvent, eventTime: currentTime));
			log2 = declaration.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(1)));
			log3 = declaration.GetLogs().AddNew(new EventValue(customsCommencedEvent, eventTime: currentTime.AddDays(3)));
			log4 = declaration.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(5), isEstimate: ZBool.True));
			log5 = declaration.GetLogs().AddNew(new EventValue(customsClearedEvent, eventTime: currentTime.AddDays(10)));
			log5.Cancel();

			order1.JD_JS = ZGuid.Empty;
			order1.JD_JE = declaration.PK;

			AssertEquals("Customs Clearance Commenced date should be the most recent event from the declaration", expectedCustomsCommencedDate, order1.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Cleared Customs date should come from the declaration event", expectedCustomsClearedDate, order1.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());
		}

		public void TestCustomsDatesWhenViewedFromNZ()
		{
			ZString currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);

			try
			{
				var currentTime = ZDateTimeOffset.Now;
				var ship1 = CommonShipment.New(Factory);
				var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime);
				ship1.GetLogs().AddNew(Events.CustomsCleared, Core.Constants.CountryCodes.Australia, currentTime.AddDays(1));
				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime.AddDays(3));
				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime.AddDays(5), ZBool.True);

				Order order1 = Factory.New<Order>();
				order1.JD_JS = ship1.PK;
				Assert("Customs Clearance Commenced date should be empty because it is from AU", order1.GetMilestoneActualDate(Events.CustomsCommenced).IsEmpty);
				Assert("Cleared Customs date should be empty because it is from AU", order1.GetMilestoneActualDate(Events.CustomsCleared).IsEmpty);

				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime);
				declaration.GetLogs().AddNew(Events.CustomsCleared, Core.Constants.CountryCodes.Australia, currentTime.AddDays(1));
				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime.AddDays(3));
				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.Australia, currentTime.AddDays(5), ZBool.True);
				order1.JD_JS = ZGuid.Empty;
				order1.JD_JE = declaration.PK;
				order1.UpdateEvent(Events.CustomsCleared, ZDateTimeOffset.Empty);
				order1.UpdateEvent(Events.CustomsCommenced, ZDateTimeOffset.Empty);

				Assert("Customs Clearance Commenced date should be empty because it is from AU", order1.GetMilestoneActualDate(Events.CustomsCommenced).IsEmpty);
				Assert("Cleared Customs date should be empty because it is from AU", order1.GetMilestoneActualDate(Events.CustomsCleared).IsEmpty);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCompanyCountryCode;
			}
		}

		public void TestCustomsDatesWhenViewedFromAU()
		{
			ZString currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			try
			{
				var currentTime = ZDateTimeOffset.Now;
				var ship1 = CommonShipment.New(Factory);
				var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime);
				ship1.GetLogs().AddNew(Events.CustomsCleared, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(1));
				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(3));
				ship1.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(5), ZBool.True);

				var order1 = Factory.New<Order>();
				order1.JD_JS = ship1.PK;
				Assert("Customs Clearance Commenced date should now be empty as it was in NZ", order1.GetMilestoneActualDate(Events.CustomsCommenced).IsEmpty);
				Assert("Cleared Customs date should come from the shipment event", order1.GetMilestoneActualDate(Events.CustomsCleared).IsEmpty);

				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime);
				declaration.GetLogs().AddNew(Events.CustomsCleared, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(1));
				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(3));
				declaration.GetLogs().AddNew(Events.CustomsCommenced, Core.Constants.CountryCodes.NewZealand, currentTime.AddDays(5), ZBool.True);
				order1.JD_JS = ZGuid.Empty;
				order1.JD_JE = declaration.PK;
				order1.UpdateEvent(Events.CustomsCleared, ZDateTimeOffset.Empty);
				order1.UpdateEvent(Events.CustomsCommenced, ZDateTimeOffset.Empty);

				Assert("Customs Clearance Commenced date should be empty because we are in AU", order1.GetMilestoneActualDate(Events.CustomsCommenced).IsEmpty);
				Assert("Cleared Customs date should be empty because we are in AU", order1.GetMilestoneActualDate(Events.CustomsCleared).IsEmpty);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCompanyCountryCode;
			}
		}

		[TestDate(2009, 1, 1)]
		public void TestOrderConfLogDateIsNotChangedWhenBookingConfIsUpdated()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_BookingConfDate = ZDateTime.Now;
			order.JD_BookingConfRef = "XYZ";

			ZDateTime eventDateTime = order.Logs.MostRecentLogByEventTime(Events.OrderConfirmed).SL_EventTime;

			TestDateAttribute.Date = new DateTime(2009, 2, 1);

			order.JD_BookingConfRef = "ABC";
			AssertEquals("Event date is not updated when the Booking Conf Ref is changed", eventDateTime, order.Logs.MostRecentLogByEventTime(Events.OrderConfirmed).SL_EventTime);
		}

		public void TestOrderLineConfirmationDateNotAfterOrderHeaderConfirmationDate()
		{
			var thisYear = ZDateTime.Now.Year;

			ZDateTime date1 = new ZDateTime(thisYear, 1, 2, 3, 4, 5);
			ZDateTime date2 = new ZDateTime(thisYear, 1, 2, 3, 2, 1);
			ZDateTime date3 = new ZDateTime(thisYear, 2, 4, 6, 8, 10);

			Order order = Factory.NewWithValidTestData<Order>();
			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();

			order.JD_BookingConfDate = date1;
			order.RunPreSaveValidation();
			AssertNoWarnings("No warnings as line dates are not present", order.JD_BookingConfDateInfo);

			line1.JO_ConfirmationDate = date2;
			order.RunPreSaveValidation();
			AssertNoWarnings("No warnings as line dates are not present, or before order date", order.JD_BookingConfDateInfo);

			line2.JO_ConfirmationDate = date3;
			order.RunPreSaveValidation();
			AssertHasWarning("Order line date is after order date", order.JD_BookingConfDateInfo, "One or more Order Lines have a Confirmation Date after the Order Confirmation Date.");
		}

		#endregion

		#region Order Splitting

		public void TestOrderSplitSiblings()
		{
			BO.JD_OrderNumber = "xxx";
			AssertEquals(0, BO.OrderSplitSiblings.Count);

			Order split1 = BO.Factory.New<Order>();
			split1.BuyerPK = BO.BuyerPK;
			split1.JD_OrderNumber = "xxx";
			split1.JD_OrderNumberSplit = 1;
			BO.RefreshOrderSplitSiblings();
			AssertEquals(1, BO.OrderSplitSiblings.Count);

			Order split2 = BO.Factory.New<Order>();
			split2.BuyerPK = BO.BuyerPK;
			split2.JD_OrderNumber = "xxx";
			split2.JD_OrderNumberSplit = 2;
			BO.RefreshOrderSplitSiblings();
			AssertEquals(2, BO.OrderSplitSiblings.Count);

			split2.BuyerPK = Factory.New<OrgHeader>().PK;
			BO.RefreshOrderSplitSiblings();
			AssertEquals(1, BO.OrderSplitSiblings.Count);
		}

		public void TestSplitOrderDoesntSetQtyInvoiced()
		{
			BO.JD_OrderNumber = "A";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;
			AssertEquals("Pre-Condition, no order lines", 0, BO.OrderLines.Count);
			OrderLine line = BO.OrderLines.AddNew();
			line.JO_Quantity = 10;
			line.JO_QtyInvoiced = 5;

			Order split = BO.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split Order should have no invoice Qty", (ZDecimal)0, split.OrderLines[0].JO_QtyInvoiced);
		}

		public void TestSplitOrderCopiesDeliveryLines()
		{
			BO.JD_OrderNumber = "A";
			OrderLine line = BO.OrderLines.AddNew();
			line.JO_Quantity = 10;
			line.JO_QtyInvoiced = 5;
			line.Deliveries.AddNew();

			Order split = BO.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split Order should have 1 order line with 1 delivery line", 1, split.OrderLines[0].Deliveries.Count);
		}

		public void TestSplitOrderIncludesAddresses()
		{
			OrgHeader randomOrg = Factory.NewWithValidTestData<OrgHeader>();

			BO.JD_OrderNumber = "A";
			BO.ControllingCustomerDocAddress.E2_AddressOverride = true;
			BO.ControllingCustomerDocAddress.E2_CompanyName = "Blah";

			Order split = BO.SplitOrder(CreateOrderType.Split);
			AssertEquals("Controlling Customer Override", true, split.ControllingCustomerDocAddress.E2_AddressOverride);
			AssertEquals("Controlling Customer Name", "Blah", split.ControllingCustomerDocAddress.E2_CompanyName);
			AssertEquals("Controlling Customer Doc Address Has Correct Parent", split.PK, split.ControllingCustomerDocAddress.E2_ParentID);

			AssertEquals("Old Controlling Customer Not Changed", BO.PK, BO.ControllingCustomerDocAddress.E2_ParentID);
		}

		public void TestOrderDoesntGoReadOnlyOnSplit()
		{
			BO.JD_OrderNumber = "x";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;
			OrderLine line = BO.OrderLines.AddNew();

			// create a couple of splits
			Order split = BO.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split order should not go read-only", false, split.ReadOnly);
			AssertEquals("Split order should not go read-only", false, split.JD_OrderStatusInfo.ReadOnly);
		}

		public void TestIsOrderPartiallyCompleteAndIsNotAlreadySplit()
		{
			BO.JD_OrderNumber = "x";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			// create a couple of splits
			var split1 = BO.SplitOrder(CreateOrderType.Split);
			var splitLine1 = split1.OrderLines.AddNew();
			splitLine1.JO_Quantity = 2;
			splitLine1.JO_QtyReceived = 1;
			var split2 = split1.SplitOrder(CreateOrderType.Split);
			var splitLine2 = split2.OrderLines[0];
			splitLine2.JO_Quantity = 2;
			splitLine2.JO_QtyReceived = 1;

			// make sure IsSplittable works according to JO_QtyReceived < JO_Quantity
			AssertEquals("Already split", false, split1.IsOrderPartiallyCompleteAndIsNotAlreadySplit);
			AssertEquals("Order incomplete and not already split", true, split2.IsOrderPartiallyCompleteAndIsNotAlreadySplit);
			splitLine2.JO_QtyReceived = 2;
			AssertEquals("Order no longer incomplete", false, split2.IsOrderPartiallyCompleteAndIsNotAlreadySplit);
		}

		[ExpectNoExceptions]
		public void TestIsOrderPartiallyCompleteAndIsNotAlreadySplit_NoOverflowException()
		{
			BO.JD_OrderNumber = "x";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			var line = BO.OrderLines.AddNew();
			line.JO_Quantity = 270;
			line.JO_QtyInvoiced = 1;

			var split1 = BO.SplitOrder(CreateOrderType.Split);
			for (var i = 0; i < 260; i++)
			{
				split1.OrderLines[0].JO_QtyReceived = 1;
				if (split1.IsOrderPartiallyCompleteAndIsNotAlreadySplit)
				{
					split1 = split1.SplitOrder(CreateOrderType.Split);
				}
			}

			AssertEquals(byte.MaxValue, split1.JD_OrderNumberSplit);
		}

		public void TestSplitOrderCopiesINCOCurrency()
		{
			BO.JD_OrderNumber = "1010";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader org2 = BO.Factory.NewWithValidTestData<OrgHeader>();
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org2.PK;

			var supLink = org.SupplierLinks.AddNew();
			supLink.OL_OH_Supplier = org2.PK;
			supLink.OL_RX_NKDefaultCurrency = Constants.CurrencyCodes.Angola;
			var mode = supLink.OrgSupBuyLinkTrnModes.AddNew();
			mode.PF_TransportMode = Constants.TransportModes.Sea;
			mode.PF_IncoTerm = Constants.IncoTerms.LandedIntoStore;

			BO.JD_IncoTerm = Constants.IncoTerms.UnpackedAtFactory;
			BO.JD_RX_NKOrderCurrency = Constants.CurrencyCodes.Albania;

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals(Constants.IncoTerms.UnpackedAtFactory, split.JD_IncoTerm);
			AssertEquals(Constants.CurrencyCodes.Albania, split.JD_RX_NKOrderCurrency);
		}

		public void TestSplitOrder_GetNewOrderNumber()
		{
			BO.JD_OrderNumber = "A";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "Buyer";
			buyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			buyer.MiscServ.OM_IMLastOrderReference = "A1";

			BO.BuyerPK = buyer.PK;
			BO.SupplierPK = buyer.PK;

			Order newOrder = BO.SplitOrder(CreateOrderType.New);
			Factory.Save();

			AssertEquals("Order should default to buyer's configuration setting", "A2", newOrder.JD_OrderNumber);
			AssertEquals("Buyer's Last Order Reference", "A2", buyer.MiscServ.OM_IMLastOrderReference);
		}

		public void TestSplitOrderLines()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org = factory.LoadTop1<OrgHeader>(new ZQuery());

			JobShipmentPreplanning preAdvice = factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = org.PK;

			Order newOrder = factory.New<Order>();
			newOrder.JD_OrderNumber = "xxx";
			newOrder.BuyerPK = org.PK;
			newOrder.SupplierPK = org.PK;
			newOrder.JD_EF_ShipmentPrePlanning = preAdvice.PK;
			factory.Save();
			AssertEquals("Should have no Order lines before split", 0, newOrder.OrderLines.Count);

			Order split1 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have no Order lines", 0, split1.OrderLines.Count);
			AssertEquals("Split should have no link to pre-advice", ZGuid.Empty, split1.JD_EF_ShipmentPrePlanning);

			split1.Delete();
			OrderLine line1 = newOrder.OrderLines.AddNew();
			line1.JO_Quantity = 0;
			line1.JO_QtyReceived = 0;
			factory.Save();

			Order split2 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have no order lines", 0, split2.OrderLines.Count);

			split2.Delete();
			line1.JO_Quantity = 5;
			line1.JO_CommercialInvoiceNo = "INVOICE";
			line1.JO_ContainerNumber = "CONTAINER";
			factory.Save();

			Order split3 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should copy incomplete order lines to new Order", 1, split3.OrderLines.Count);
			AssertEquals("Split should NOT increment line split as no qty was received", ZShort.Zero, split3.OrderLines[0].JO_LineSplitNumber);
			AssertEquals("Split should copy incomplete order lines to new Order", line1.JO_Quantity, split3.OrderLines[0].JO_Quantity);
			AssertEquals("Split line should have no invoice number", "", split3.OrderLines[0].JO_CommercialInvoiceNo);
			AssertEquals("Split line should have no container number", "", split3.OrderLines[0].JO_ContainerNumber);

			split3.Delete();
			line1.JO_QtyReceived = 5;
			factory.Save();

			Order split4 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split shouldn't have an order line", 0, split4.OrderLines.Count);

			split4.Delete();
			OrderLine line2 = newOrder.OrderLines.AddNew();
			line2.JO_Quantity = 10;
			factory.Save();

			Order split5 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have 1 Order line", 1, split5.OrderLines.Count);
			AssertEquals("Split should have copy Order line", line2.JO_Quantity, split5.OrderLines[0].JO_Quantity);

			split5.Delete();
			line2.JO_QtyReceived = 7;
			factory.Save();

			Order split6 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have remaining qty copied", line2.JO_Quantity - line2.JO_QtyReceived, split6.OrderLines[0].JO_Quantity);
			AssertEquals("Split should increment line split number as some qty is remaining", (ZShort)1, split6.OrderLines[0].JO_LineSplitNumber);
		}

		[ExpectNoExceptions]
		public void TestSplitOrdersForOrderLineVolumeDoNotThrowException()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_OrderNumber = "someorder";

			var line1 = Factory.NewWithValidTestData<OrderLine>();

			order.OrderLines.Add(line1);

			line1.JO_Quantity = 700m;
			line1.JO_OuterPacks = 10m;
			line1.JO_OuterPackLength = 999999m;
			line1.JO_OuterPackWidth = 999999m;
			line1.JO_OuterPackHeight = 999999m;
			line1.JO_OuterPackUnitOfDimension = Core.Constants.Length.Metres;
			line1.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			line1.JO_ActualVolume = 8;

			line1.JO_QtyReceived = 5m;
			line1.JO_QtyInvoiced = 10m;

			order.SplitOrder(CreateOrderType.Split);

			Factory.Save();
		}

		public void TestSplitUsesRegistrySetting()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org = factory.LoadTop1<OrgHeader>(new ZQuery());

			JobShipmentPreplanning preAdvice = factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = org.PK;

			Order newOrder = factory.New<Order>();
			newOrder.JD_OrderNumber = "xxx";
			newOrder.BuyerPK = org.PK;
			newOrder.SupplierPK = org.PK;
			newOrder.JD_EF_ShipmentPrePlanning = preAdvice.PK;
			factory.Save();
			AssertEquals("Should have no Order lines before split", 0, newOrder.OrderLines.Count);

			OrderLine line1 = newOrder.OrderLines.AddNew();
			line1.JO_CommercialInvoiceNo = "INVOICE";
			line1.JO_ContainerNumber = "CONTAINER";
			line1.JO_Quantity = 5;
			line1.JO_QtyReceived = 4;
			factory.Save();

			factory.Save();

			Order split1 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have 1 Order line", 1, split1.OrderLines.Count);
			AssertEquals("Split should have remaining quantity", 1m, split1.OrderLines[0].JO_Quantity);
			split1.Delete();

			line1.JO_QtyInvoiced = 3;
			line1.JO_QtyReceived = 5;
			factory.Save();

			Order split2 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have 0 Order lines", 0, split2.OrderLines.Count);
			split2.Delete();

			OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Order split3 = newOrder.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split should have 1 Order line", 1, split3.OrderLines.Count);
			AssertEquals("Split should have remaining quantity", 2m, split3.OrderLines[0].JO_Quantity);
		}

		[ExpectNoExceptions]
		public void TestOpeningOrderSplits()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Order orderSplit = order.SplitOrder(CreateOrderType.Split);
			Factory.Save();

			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			Order retrievedOrder = retrievingFactory.Load<Order>(order.PK);
		}

		public void TestOrderSplitSiblingsCorrectAfterSplitOrder()
		{
			var order = Factory.NewWithValidTestData<Order>();
			AssertEquals("Should be no order split siblings initially", 0, order.OrderSplitSiblings.Count);
			Order orderSplit = order.SplitOrder(CreateOrderType.Split);
			AssertEquals("Should be 1 split sibling after split", 1, order.OrderSplitSiblings.Count);
			AssertEquals("Should be 1 split sibling after split", 1, orderSplit.OrderSplitSiblings.Count);
		}

		public void TestSplitOrderDoesntCopyShipment()
		{
			BO.JD_OrderNumber = "1010";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BO.JD_JS = shipment.PK;

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals("Split Order should have no Shipment", ZGuid.Empty, split.JD_JS);
			AssertEquals("Order should have Shipment still", shipment.PK, BO.JD_JS);
		}

		public void TestSplitOrderDoesntCopyDeclaration()
		{
			BO.JD_OrderNumber = "1010";
			OrgHeader org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BO.JD_JE = declaration.PK;

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals("Split Order should have no Declaration", ZGuid.Empty, split.JD_JE);
			AssertEquals("Order should have Declaration still", declaration.PK, BO.JD_JE);
		}

		public void TestSplitOrderDoesntCopyPlannedContainers()
		{
			BO.JD_OrderNumber = "1010";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			BO.PlannedContainers.AddNew();
			BO.PlannedContainers.AddNew();

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals("Split Order should have no PlannedContainers", 0, split.PlannedContainers.Count);
			AssertEquals("Order should have 2 PlannedContainers", 2, BO.PlannedContainers.Count);
		}

		public void TestSplitOrderDoesntCopySailingDetails()
		{
			BO.JD_OrderNumber = "1010";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			ZDateTime now = ZDateTime.SmallDateTimeNow;
			BO.JD_DepartureVesselCutoffDate = now;
			BO.JD_RV_NKDepartureVessel = "2";
			BO.JD_RV_NKIntermediateVessel = "3";
			BO.JD_RV_NKArrivalVessel = "4";
			BO.JD_DepartureVoyage = "5";
			BO.JD_IntermediateVoyage = "6";
			BO.JD_ArrivalVoyage = "8";
			BO.JD_Milestone_E_DEP = now;
			BO.JD_E_DEP_2 = now;
			BO.JD_E_DEP_3 = now;
			BO.JD_E_ARV_1stIntermediate = now;
			BO.JD_E_ARV_2ndIntermediate = now;
			BO.JD_Milestone_E_ARV = now;
			BO.JD_Waybill = "9";
			BO.JD_MasterWaybill = "10";

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals("Split Order should have cleared JD_DepartureVesselCutoffDate", ZDateTime.Empty, split.JD_DepartureVesselCutoffDate);
			AssertEquals("Split Order should have cleared JD_RV_NKDepartureVessel", "", split.JD_RV_NKDepartureVessel);
			AssertEquals("Split Order should have cleared JD_RV_NKIntermediateVessel", "", split.JD_RV_NKIntermediateVessel);
			AssertEquals("Split Order should have cleared JD_RV_NKArrivalVessel", "", split.JD_RV_NKArrivalVessel);
			AssertEquals("Split Order should have cleared JD_DepartureVoyage", "", split.JD_DepartureVoyage);
			AssertEquals("Split Order should have cleared JD_IntermediateVoyage", "", split.JD_IntermediateVoyage);
			AssertEquals("Split Order should have cleared JD_ArrivalVoyage", "", split.JD_ArrivalVoyage);
			AssertEquals("Split Order should have cleared JD_Milestone_E_DEP", ZDateTime.Empty, split.JD_Milestone_E_DEP);
			AssertEquals("Split Order should have cleared JD_E_DEP_2", ZDateTime.Empty, split.JD_E_DEP_2);
			AssertEquals("Split Order should have cleared JD_E_DEP_3", ZDateTime.Empty, split.JD_E_DEP_3);
			AssertEquals("Split Order should have cleared JD_E_ARV_1stIntermediate", ZDateTime.Empty, split.JD_E_ARV_1stIntermediate);
			AssertEquals("Split Order should have cleared JD_E_ARV_2ndIntermediate", ZDateTime.Empty, split.JD_E_ARV_2ndIntermediate);
			AssertEquals("Split Order should have cleared JD_Milestone_E_ARV", ZDateTime.Empty, split.JD_Milestone_E_ARV);
			AssertEquals("Split Order should have cleared JD_Waybill", "", split.JD_Waybill);
			AssertEquals("Split Order should have cleared JD_MasterWaybill", "", split.JD_MasterWaybill);

			AssertEquals("Order should have JD_DepartureVesselCutoffDate", now, BO.JD_DepartureVesselCutoffDate);
			AssertEquals("Order should have JD_RV_NKDepartureVessel", "2", BO.JD_RV_NKDepartureVessel);
			AssertEquals("Order should have JD_RV_NKIntermediateVessel", "3", BO.JD_RV_NKIntermediateVessel);
			AssertEquals("Order should have JD_RV_NKArrivalVessel", "4", BO.JD_RV_NKArrivalVessel);
			AssertEquals("Order should have JD_DepartureVoyage", "5", BO.JD_DepartureVoyage);
			AssertEquals("Order should have JD_IntermediateVoyage", "6", BO.JD_IntermediateVoyage);
			AssertEquals("Order should have JD_ArrivalVoyage", "8", BO.JD_ArrivalVoyage);
			AssertEquals("Order should have JD_Milestone_E_DEP", now, BO.JD_Milestone_E_DEP);
			AssertEquals("Order should have JD_E_DEP_2", now, BO.JD_E_DEP_2);
			AssertEquals("Order should have JD_E_DEP_3", now, BO.JD_E_DEP_3);
			AssertEquals("Order should have JD_E_ARV_1stIntermediate", now, BO.JD_E_ARV_1stIntermediate);
			AssertEquals("Order should have JD_E_ARV_2ndIntermediate", now, BO.JD_E_ARV_2ndIntermediate);
			AssertEquals("Order should have JD_Milestone_E_ARV", now, BO.JD_Milestone_E_ARV);
			AssertEquals("Order should have JD_Waybill", "9", BO.JD_Waybill);
			AssertEquals("Order should have JD_MasterWaybill", "10", BO.JD_MasterWaybill);
		}

		public void TestSplitOrderDoesntCopyTrackingDates()
		{
			BO.JD_OrderNumber = "1010";
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			ZDateTime now = ZDateTime.SmallDateTimeNow;
			var nowOffset = new ZDateTimeOffset(now);
			BO.JD_ActualUserDate2 = now;
			BO.JD_EstimateUserDate2 = now;
			BO.JD_ActualUserDate1 = now;
			BO.JD_EstimateUserDate1 = now;
			BO.JD_ActualUserDate4 = now;
			BO.JD_EstimateUserDate4 = now;
			BO.JD_ActualUserDate3 = now;
			BO.JD_EstimateUserDate3 = now;
			BO.UpdateEventEstimate(Events.GateIn, nowOffset);
			BO.UpdateEventEstimate(Events.DeliveryCartageAdvised, nowOffset);
			BO.UpdateEventEstimate(Events.CargoAvailable, nowOffset);
			BO.UpdateEventEstimate(Events.CustomsCleared, nowOffset);
			BO.UpdateEventEstimate(Events.CustomsCommenced, nowOffset);
			BO.UpdateEventEstimate(Events.Arrival, nowOffset);
			BO.UpdateEventEstimate(Events.Departure, nowOffset);
			BO.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, nowOffset);
			BO.UpdateEventEstimate(Events.ExWorks, nowOffset);
			BO.UpdateEvent(Events.GateIn, nowOffset);
			BO.UpdateEvent(Events.DeliveryCartageAdvised, nowOffset);
			BO.UpdateEvent(Events.CargoAvailable, nowOffset);
			BO.UpdateEvent(Events.CustomsCleared, nowOffset);
			BO.UpdateEvent(Events.CustomsCommenced, nowOffset);
			BO.UpdateEvent(Events.Arrival, nowOffset);
			BO.UpdateEvent(Events.Departure, nowOffset);
			BO.UpdateEvent(Events.DeliveryCartageCompleteFinalised, nowOffset);
			BO.UpdateEvent(Events.ExWorks, nowOffset);

			Order split = BO.SplitOrder(CreateOrderType.Split);

			AssertEquals("Split Order should have cleared JD_ActualUserDate2", ZDateTime.Empty, split.JD_ActualUserDate2);
			AssertEquals("Split Order should have cleared JD_EstimateUserDate2", ZDateTime.Empty, split.JD_EstimateUserDate2);
			AssertEquals("Split Order should have cleared JD_ActualUserDate1", ZDateTime.Empty, split.JD_ActualUserDate1);
			AssertEquals("Split Order should have cleared JD_EstimateUserDate1", ZDateTime.Empty, split.JD_EstimateUserDate1);
			AssertEquals("Split Order should have cleared JD_ActualUserDate4", ZDateTime.Empty, split.JD_ActualUserDate4);
			AssertEquals("Split Order should have cleared JD_EstimateUserDate4", ZDateTime.Empty, split.JD_EstimateUserDate4);
			AssertEquals("Split Order should have cleared JD_ActualUserDate3", ZDateTime.Empty, split.JD_ActualUserDate3);
			AssertEquals("Split Order should have cleared JD_EstimateUserDate3", ZDateTime.Empty, split.JD_EstimateUserDate3);
			AssertEquals("Split Order should have cleared E GateIn", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime());
			AssertEquals("Split Order should have cleared E DeliveryCartageAdvised", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("Split Order should have cleared E CargoAvailable", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("Split Order should have cleared E CustomsCleared", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("Split Order should have cleared E CustomsCommenced", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Split Order should have cleared E Arrival", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("Split Order should have cleared E Departure", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("Split Order should have cleared E DeliveryCartageCompleteFinalised", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("Split Order should have cleared E ExWorks", ZDateTime.Empty, split.GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime());
			AssertEquals("Split Order should have cleared A GateIn", ZDateTime.Empty, split.GetMilestoneActualDate(Events.GateIn).ToZDateTime());
			AssertEquals("Split Order should have cleared A DeliveryCartageAdvised", ZDateTime.Empty, split.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("Split Order should have cleared A CargoAvailable", ZDateTime.Empty, split.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("Split Order should have cleared A CustomsCleared", ZDateTime.Empty, split.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("Split Order should have cleared A CustomsCommenced", ZDateTime.Empty, split.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Split Order should have cleared A Arrival", ZDateTime.Empty, split.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals("Split Order should have cleared A Departure", ZDateTime.Empty, split.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("Split Order should have cleared A DeliveryCartageCompleteFinalised", ZDateTime.Empty, split.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("Split Order should have cleared A ExWorks", ZDateTime.Empty, split.GetMilestoneActualDate(Events.ExWorks).ToZDateTime());

			AssertEquals("Order should have JD_ActualUserDate2", now, BO.JD_ActualUserDate2);
			AssertEquals("Order should have JD_EstimateUserDate2", now, BO.JD_EstimateUserDate2);
			AssertEquals("Order should have JD_ActualUserDate1", now, BO.JD_ActualUserDate1);
			AssertEquals("Order should have JD_EstimateUserDate1", now, BO.JD_EstimateUserDate1);
			AssertEquals("Order should have JD_ActualUserDate4", now, BO.JD_ActualUserDate4);
			AssertEquals("Order should have JD_EstimateUserDate4", now, BO.JD_EstimateUserDate4);
			AssertEquals("Order should have JD_ActualUserDate3", now, BO.JD_ActualUserDate3);
			AssertEquals("Order should have JD_EstimateUserDate3", now, BO.JD_EstimateUserDate3);
			AssertEquals("Order should have E GateIn", now, BO.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime());
			AssertEquals("Order should have E DeliveryCartageAdvised", now, BO.GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("Order should have E CargoAvailable", now, BO.GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("Order should have E CustomsCleared", now, BO.GetMilestoneEstimatedDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("Order should have E CustomsCommenced", now, BO.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Order should have E Arrival", now, BO.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("Order should have E Departure", now, BO.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("Order should have E DeliveryCartageCompleteFinalised", now, BO.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("Order should have E ExWorks", now, BO.GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime());
			AssertEquals("Order should have A GateIn", now, BO.GetMilestoneActualDate(Events.GateIn).ToZDateTime());
			AssertEquals("Order should have A DeliveryCartageAdvised", now, BO.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("Order should have A CargoAvailable", now, BO.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("Order should have A CustomsCleared", now, BO.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("Order should have A CustomsCommenced", now, BO.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("Order should have A Arrival", now, BO.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals("Order should have A Departure", now, BO.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("Order should have A DeliveryCartageCompleteFinalised", now, BO.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("Order should have A ExWorks", now, BO.GetMilestoneActualDate(Events.ExWorks).ToZDateTime());
		}

		public void TestSplitOrderDoesntCopyNonSystemDeefinedTrackingDates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsSystem = false;
			template.P0_IsActive = true;
			template.P0_ProcessType = "ORD";

			var arv = template.WorkflowItems.AddNew();
			arv.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			arv.P9_Sequence = 1;
			arv.P9_Type = "MIL";

			var giy = template.WorkflowItems.AddNew();
			giy.TriggerConditions.TriggerEventCode = Events.GateInCode;
			giy.P9_Sequence = 2;
			giy.P9_Type = "MIL";

			var now = ZDateTime.SmallDateTimeNow;
			var nowOffset = new ZDateTimeOffset(now);
			giy.P9_EstimatedDefaultFromPredecessor = 1;
			giy.P9_EstimatedDefaultTimeDelta = new ZDateTime(now.Year, 1, 1, 1, 0, 0);

			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.UpdateEventEstimate(Events.Arrival, nowOffset);

			AssertEquals("Order should have A Arrival", now, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("Order should have A GateInContainerParkYard", now.AddHours(1), order.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime());

			var newOrder = order.SplitOrder(CreateOrderType.Split);
			AssertEquals("Split Order should have cleared A Arrival", ZDateTime.Empty, newOrder.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("Split Order should have cleared A GateInContainerParkYard", ZDateTime.Empty, newOrder.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime());
		}

		#endregion

		#region Lists

		[ExpectNoExceptions]
		public void TestJD_OH_SendingAgents_List()
		{
			int x = BO.JD_OH_SendingAgents_List.Count;
			int y = BO.JD_OH_ReceivingAgents_List.Count;

			BO.JD_RL_NKPortOfLoading = GetAOrg().OH_RL_NKClosestPort;
			BO.JD_RL_NKPortOfDischarge = GetAOrg().OH_RL_NKClosestPort;
			x = BO.JD_OH_SendingAgents_List.Count;
			y = BO.JD_OH_ReceivingAgents_List.Count;
		}

		public void TestContactsList()
		{
			OrgHeader buyer = CreateNewOrg(BO.Factory, "az");
			OrgHeader supplier = CreateNewOrg(BO.Factory, "bz");
			OrgHeader sendingAgent = CreateNewOrg(BO.Factory, "cz");
			OrgHeader receivingAgent = CreateNewOrg(BO.Factory, "dz");

			buyer.Contacts.AddNew().OC_ContactName = "ConsigneeContact";
			supplier.Contacts.AddNew().OC_ContactName = "ConsignorContact";

			BO.BuyerPK = buyer.PK;
			BO.Contacts.Load();
			AssertEquals(1, BO.Contacts.Count);
			AssertEquals(true, BO.Contacts.Cast<OrgContact>().Any(contact => contact.OC_ContactName == "ConsigneeContact"));

			BO.SupplierPK = supplier.PK;
			BO.Contacts.Load();
			AssertEquals(2, BO.Contacts.Count);
			AssertEquals(true, BO.Contacts.Cast<OrgContact>().Any(contact => contact.OC_ContactName == "ConsignorContact"));

			BO.JD_TransportMode = ZString.Empty;
			BO.JD_OH_SendingAgent = sendingAgent.PK;
			BO.Contacts.Load();
			AssertEquals(2, BO.Contacts.Count);

			BO.JD_TransportMode = ZString.Empty;
			BO.JD_OH_ReceivingAgent = receivingAgent.PK;
			BO.Contacts.Load();
			AssertEquals(2, BO.Contacts.Count);
		}

		public void TestJD_OrderStatus_List()
		{
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = NewOrder();
			CodeDescriptionPairList testOrderCodeDescPairList = new CodeDescriptionPairList();
			testOrderCodeDescPairList.AddPair("REG", "Reg Status Description");
			Env.Registry.OrderHeaderStatusList = testOrderCodeDescPairList;
			order.BuyerPK = buyer.PK;

			CodeDescriptionPairList sampleCodeDescPairList = new CodeDescriptionPairList();
			sampleCodeDescPairList.AddPair("XXX", "Description");

			buyer.MiscServ.OrderStatusList = sampleCodeDescPairList.ToXMLByteArray();
			testOrderCodeDescPairList.Clear();
			testOrderCodeDescPairList.AddPair("POX", "Proxy Org Status Description");
			GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList = testOrderCodeDescPairList.ToXMLByteArray();
			AssertEquals("Status List should include proxy organisation codes", true, order.JD_OrderStatus_List.ContainsCode("POX"));
			AssertEquals("Status List should include the default list", true, order.JD_OrderStatus_List.ContainsCode(Constants.OrderStatus.Incomplete));
			AssertEquals("Status List should include the list from the buyer", true, order.JD_OrderStatus_List.ContainsCode("XXX"));
			AssertEquals("Status List should include registry codes", true, order.JD_OrderStatus_List.ContainsCode("REG"));
		}

		public void TestJD_Shipment_List()
		{
			string transportModeKey = "Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			string containerModeKey = "Container Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var ship = CommonShipment.New(Factory);
			ship.JS_TransportMode = Constants.TransportModes.Sea;
			ship.JS_PackingMode = Constants.ContainerModes.FCL;
			ship.JS_IsCancelled = false;

			var order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "12312";
			order1.BuyerPK = org.PK;
			order1.SupplierPK = org.PK;
			order1.JD_TransportMode = Constants.TransportModes.Air;
			order1.JD_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var collection = order1.JD_Shipment_List;
			collection.Load();
			AssertEquals("Order Shipment List shouldn't contain Ship with different transport mode", false, collection.Contains(ship.PK));

			order1.JD_TransportMode = Constants.TransportModes.Sea;
			order1.JD_ContainerMode = Constants.ContainerModes.FCL;
			Factory.Save();

			collection = order1.JD_Shipment_List;
			collection.Load();
			AssertEquals("Order Shipment List should contain Ship with same transport mode", true, collection.Contains(ship.PK));

			AssertEquals("Correct filterbusinessobject defaults", Constants.TransportModes.Sea, collection.FilterBusinessObjectDefaults[transportModeKey].Value);
			AssertEquals("Correct filterbusinessobject defaults", Constants.ContainerModes.FCL, collection.FilterBusinessObjectDefaults[containerModeKey].Value);
		}

		public void TestJD_Shipment_List_IsForwardingModuleShipmentCollection()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var shipmentList = order.JD_Shipment_List;
			AssertEquals("Shipment Type is ForwardingModuleShipment", typeof(ForwardingModuleShipment), shipmentList.TypeOfElements);
		}

		#endregion

		#region Fields Going ReadOnly

		public void TestOrderNumberReadOnly()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var order = factory.NewWithValidTestData<Order>();
			BusinessObjectFactory factoryForShipment = new BusinessObjectFactory();
			ForwardingShipment shipmentInDB = factoryForShipment.New<ForwardingShipment>();
			factoryForShipment.Save();

			AssertEquals(false, BO.JD_OrderNumberInfo.ReadOnly);
			order.JD_JS = shipmentInDB.PK;
			AssertEquals("Cannot be in database for test", false, order.IsInDatabase);
			AssertEquals("Not in db, therefore can modify number", false, order.JD_OrderNumberInfo.ReadOnly);

			factory.Save();
			BusinessObjectFactory factoryToLoadFrom = new BusinessObjectFactory();
			Order loadedOrder = factoryToLoadFrom.Load<Order>(order.PK);
			loadedOrder.OnLoaded();
			loadedOrder.JD_OrderNumber = "ooo";
			AssertEquals("Shipment attached and in db - order number still editable", false, loadedOrder.JD_OrderNumberInfo.ReadOnly);
			loadedOrder.JD_JS = ZGuid.Empty;
			AssertEquals("Shipment not attached", false, loadedOrder.JD_OrderNumberInfo.ReadOnly);
		}

		public void TestDuplicateOrderNumReadOnly()
		{
			ZString duplicateOrderNumber = "123";
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Factory.LoadTop1<Order>(new ZQuery());
			order1.JD_OrderNumber = duplicateOrderNumber;
			order1.BuyerPK = buyer.PK;
			Factory.Save();
			Order duplicateOrder = Factory.New<Order>();
			duplicateOrder.BuyerPK = buyer.PK;
			duplicateOrder.JD_OrderNumber = duplicateOrderNumber;
			AssertEquals("Duplicate Order should have errors", true, duplicateOrder.JD_OrderNumberInfo.HasErrors());
			AssertEquals("Duplicate Order number shouldn't be readonly", false, duplicateOrder.JD_OrderNumberInfo.ReadOnly);
		}

		public void TestShipmentAndConsolsReadOnly()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BO.JD_JS = shipment.PK;
			shipment.Consols.AddNew();

			AssertEquals("Shipment must NOT be read only as otherwise new order will be readonly", false, BO.Shipment.ReadOnly);
		}

		public void TestDontGoReadOnlyWhenAttachingShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "1234";
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			Factory.Save();

			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			Order retrievedOrder = retrievingFactory.Load<Order>(order.PK);
			retrievedOrder.JD_JS = shipment.PK;
			object x = retrievedOrder.Shipment;
			object y = retrievedOrder.ConsolsForBinding;

			AssertEquals("Order should not be read-only", false, retrievedOrder.ReadOnly);
		}

		#endregion

		#region New Properties

		public void TestHasBeenExported()
		{
			Order order = Factory.New<Order>();
			AssertEquals(false, order.HasBeenExported);

			order.Logs.AddNew(Events.DataExport, "Testing Only");
			AssertEquals(true, order.HasBeenExported);
		}

		public void TestPlanningVoyageStates()
		{
			AssertEquals(PlanningVoyageState.OneVoyage, BO.PlanningVoyageState);
			BO.JD_ArrivalVoyage = "x";
			AssertEquals(PlanningVoyageState.OneVoyage, BO.PlanningVoyageState);
			BO.JD_ArrivalVoyage = "";
			BO.JD_DepartureVoyage = "x";
			AssertEquals(PlanningVoyageState.OneVoyage, BO.PlanningVoyageState);
			BO.JD_ArrivalVoyage = "x";
			BO.JD_DepartureVoyage = "x";
			AssertEquals(PlanningVoyageState.OneVoyage, BO.PlanningVoyageState);

			BO.JD_ArrivalVoyage = "x";
			BO.JD_DepartureVoyage = "y";
			AssertEquals(PlanningVoyageState.TwoVoyage, BO.PlanningVoyageState);

			BO.JD_DepartureVoyage = "x";
			BO.JD_IntermediateVoyage = "x";
			BO.JD_ArrivalVoyage = "x";
			AssertEquals(PlanningVoyageState.ThreeVoyage, BO.PlanningVoyageState);
		}

		public void TestJD_PlannedContainersVisible()
		{
			BO.JD_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("JD_PlannedContainersVisible", true, BO.JD_PlannedContainersVisible);
			BO.JD_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals("JD_PlannedContainersVisible", true, BO.JD_PlannedContainersVisible);
			BO.JD_ContainerMode = Constants.ContainerModes.Loose;
			AssertEquals("JD_PlannedContainersVisible", false, BO.JD_PlannedContainersVisible);
		}

		public void TestCalcBuyerSupplierCodes()
		{
			OrgHeader newBuyer = BO.Factory.New<OrgHeader>();

			newBuyer.OH_FullName = "BUYER ORGHEADER";
			newBuyer.OH_Code = "BUYER";
			newBuyer.OH_IsConsignee = true;

			OrgHeader newSupplier = BO.Factory.New<OrgHeader>();

			newSupplier.OH_FullName = "SUPPLIER ORGHEADER";
			newSupplier.OH_Code = "SUPPLIER";
			newSupplier.OH_IsConsignor = true;

			BO.BuyerPK = newBuyer.PK;
			BO.SupplierPK = newSupplier.PK;
			AssertEquals("Buyer Code.", "BUYER", BO.JD_Calc_BuyerCode);
			AssertEquals("Supplier Code.", "SUPPLIER", BO.JD_Calc_SupplierCode);
		}

		public void TestDontAccessOrderLinesOnLoad()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.OrderLines.AddNew();
			order.OrderLines.AddNew();

			AssertEquals("Precondition", 2, order.OrderLines.Count);
			Factory.Save();

			Order retrievedOrder = new BusinessObjectFactory().Load<Order>(order.PK);

			FieldInfo orderLinesField = typeof(Order).GetField("orderLines", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNull("OrderLines shouldn't load until they are absolutely needed", orderLinesField.GetValue(retrievedOrder));
		}

		public void TestJD_CalcShipmentBrokerageNumber()
		{
			CommonShipment ship = CommonShipment.New(Factory);
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			ZString brokerageNumber = "B00001111";
			ZString shipmentNumber = "S00001111";
			declaration[JobDeclarationSchema.JE_DeclarationReference.Name] = brokerageNumber;
			ship.JS_UniqueConsignRef = shipmentNumber;

			Order order1 = Factory.New<Order>();
			AssertEquals("Shipment Brokerage number should be blank", true, order1.JD_CalcShipmentBrokerageNumber.IsEmpty);

			order1.JD_JS = ship.PK;
			AssertEquals("Shipment Brokerage number should be Shipment No", shipmentNumber, order1.JD_CalcShipmentBrokerageNumber);

			order1.JD_JE = declaration.PK;
			AssertEquals("Order IsShipmentAttached should be false since declaration attached", false, order1.IsShipmentAttached);
			AssertEquals("Shipment Brokerage number should be Brokerage No", brokerageNumber, order1.JD_CalcShipmentBrokerageNumber);
		}

		public void TestJD_Calc_HouseBill()
		{
			ZString houseBillNumber1 = "1234";
			ZString houseBillNumber2 = "5678";

			Shipment.JS_HouseBill = houseBillNumber1;
			BO.JD_Waybill = houseBillNumber2;

			AssertEquals("House Bill of Order should be taken from internal field", houseBillNumber2, BO.JD_Calc_HouseBill);

			BO.JD_JS = Shipment.PK;
			AssertEquals("House Bill of Order should be taken from attached shipment", houseBillNumber1, BO.JD_Calc_HouseBill);
		}

		public void TestJD_Calc_MasterBill()
		{
			ZString masterBillNumber1 = "1234";
			ZString masterBillNumber2 = "5678";

			Consol.JK_MasterBillNum = masterBillNumber1;
			BO.JD_MasterWaybill = masterBillNumber2;

			AssertEquals("Master Bill of Order should be taken from internal field", masterBillNumber2, BO.JD_Calc_MasterBill);

			CommonShipment ship = CommonShipment.New(Factory);

			BO.JD_JS = ship.PK;
			AssertEquals("Master Bill of Order should be taken from internal field still", masterBillNumber2, BO.JD_Calc_MasterBill);

			BO.JD_JS = Shipment.PK;
			AssertEquals("Master Bill of Order should be taken from consol which holds attached shipment", masterBillNumber1, BO.JD_Calc_MasterBill);
		}

		public void TestEffectiveCCCommenced_OrdersFromDeclaration()
		{
			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();
			IBusinessObjectCollection orders = (IBusinessObjectCollection)declaration["AttachedOrders"];
			Order order = (Order)orders.AddNew();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Preconditions; Declaration.SystemCreateTime should not be empty", false, ((ZDateTime)declaration[JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc]).IsEmpty);
			AssertEquals("Order.JD_E_CCC was expected to be Declaration.JE_SystemCreateTimeUtc.", ((ZDateTime)declaration[JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc]).ToSmallDateTimeFloor(), order.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime().ToSmallDateTimeFloor());
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BusinessObject sameDeclaration = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			IBusinessObjectCollection sameOrders = (IBusinessObjectCollection)sameDeclaration["AttachedOrders"];
			Order sameOrder = (Order)sameOrders.ToArray()[0];
			AssertEquals("Preconditions; Declaration.SystemCreateTime should not be empty", false, ((ZDateTime)sameDeclaration[JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc]).IsEmpty);
			AssertEquals("Commenced date is what it was saved before.", ((ZDateTime)sameDeclaration[JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc]).ToSmallDateTimeFloor(), sameOrder.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime().ToSmallDateTimeFloor());

			sameOrder.UpdateEventEstimate(Events.CustomsCommenced, new ZDateTimeOffset(2010, 10, 10));
			AssertEquals("JD_E_CCC should be what it was set to.", new ZDateTime(2010, 10, 10), sameOrder.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
			newFactory.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			BusinessObject sameDeclaration2 = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			IBusinessObjectCollection sameOrders2 = (IBusinessObjectCollection)sameDeclaration["AttachedOrders"];
			Order sameOrder2 = (Order)sameOrders2.ToArray()[0];
			AssertEquals("Commenced date is what it was saved before.", new ZDateTime(2010, 10, 10), sameOrder2.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
		}

		public void TestEffectiveCCCommenced_OrdersFromShipment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			Order order = shipment.AttachedOrders.AddNew();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Order.JD_E_CCC is expected to be empty since shipment has no declarations.", ZDateTime.Empty, order.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());

			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			order = shipment.AttachedOrders.AddNew();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Order.JD_E_CCC is expected to be SET since shipment has 1 declaration.", declaration[JobDeclarationSchema.JE_SystemCreateTimeUtc], order.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
		}

		#endregion

		#region TestOrderUpdateHistoryNote

		public void TestOrderUpdateHistoryStmNoteIsReadOnly()
		{
			AssertEquals(true, Factory.New<Order.OrderUpdateHistoryStmNote>().ReadOnly);
		}

		public void TestOrderUpdateHistoryStmNoteDescription()
		{
			AssertEquals(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description, Factory.New<Order.OrderUpdateHistoryStmNote>().ST_Description);
		}

		public void TestOrderUpdateHistoryStmNoteDescriptionMultilingual()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.EnableRot13();

				var order = Factory.New<Order>();
				order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Code, "Note 1");
				order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Code, "Note 2");
				var orderUpdateNotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Code);
				AssertEquals(1, orderUpdateNotes.Length);
				AssertEquals(PredefinedNoteTypes.Instance.OrderUpdateHistory.Code, orderUpdateNotes[0].ST_DescriptionInDatabase);
				AssertEquals(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description, orderUpdateNotes[0].ST_Description);
				AssertNoErrors(orderUpdateNotes[0].ST_DescriptionInfo);
			}
		}

		public void TestOrderUpdateHistoryStmNoteIsSavedByFactoryIsFalse()
		{
			AssertEquals(false, Factory.New<Order.OrderUpdateHistoryStmNote>().IsSavedByFactory);
		}

		public void TestOrderUpdateHistoryStmNoteHasChangesIsAlwaysFalse()
		{
			Order.OrderUpdateHistoryStmNote note = Factory.New<Order.OrderUpdateHistoryStmNote>();
			AssertEquals(false, note.HasChanges);

			note.ST_NoteDataAsText = "Odin's Ravens";
			AssertEquals(false, note.HasChanges);

			note.HasChanges = true;
			AssertEquals(false, note.HasChanges);
		}

		public void TestOrderUpdateHistoryStmNoteIsNotValidated()
		{
			OrgHeader bizo = Factory.New<OrgHeader>();

			Order.OrderUpdateHistoryStmNote note = Factory.New<Order.OrderUpdateHistoryStmNote>();
			bizo.Notes.Add(note);
			AssertEquals("Precondition - a new order update history note should not have any notifications.", false, note.HasNotifications());

			note.Validation.ValidateAll();
			AssertEquals(false, note.HasNotifications());

			note.ST_Description = "Matt is a stupid-head.";
			note.ST_IsCustomDescription = false;
			note.Validation.ValidateST_Description();
			AssertEquals(false, note.ST_DescriptionInfo.HasNotifications());

			note.ST_NoteText = "";
			note.Validation.ValidateST_NoteText();
			AssertEquals(false, note.ST_NoteTextInfo.HasNotifications());
		}

		public void TestOrderUpdateHistoryNote()
		{
			Order order = Factory.NewWithValidTestData<Order>();

			Action<string, StmNote[]> assertOrderNotesInDatabase = (message, expectedNotes) =>
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmNote));
					query.AddToFilter(StmNoteSchema.ST_ParentID, order.PK);
					query.AddToFilter(StmNoteSchema.ST_Table, order.TableName);

					StmNote[] actualNotes = Factory.Load<StmNote>(query);
					AssertContainsExactElementsInAnyOrder(message, expectedNotes, actualNotes);
				};

			Factory.Save();
			assertOrderNotesInDatabase("No notes in database", Array.Empty<StmNote>());

			StmNote randomNote1 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "Internal Work Notes Text");
			AssertEquals("Not creating OrderUpdateHistoryNote when there is no OrderManagementUpdateNotes", 0, order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description).Length);

			string managementUpdateNoteText1 = "Order Management Update Text 1";
			string managementUpdateNoteText2 = "Order Management Update Text 2";

			StmNote managementUpdateNote1 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, managementUpdateNoteText1);
			AssertEquals("OrderUpdateHistoryNote was created", 1, order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description).Length);

			Factory.Save();
			assertOrderNotesInDatabase("There are 2 notes in database", new StmNote[] { randomNote1, managementUpdateNote1 });

			StmNote managementUpdateNote2 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, managementUpdateNoteText2);
			Thread.Sleep(5);
			Factory.Save();

			StmNote updateHistoryNote = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description)[0];
			AssertEquals("OrderUpdateHistory note must be Readonly", true, updateHistoryNote.ReadOnly);
			AssertEquals("OrderUpdateHistory.ST_NoteTextInfo must be Readonly", true, updateHistoryNote.ST_NoteTextInfo.ReadOnly);
			AssertEquals("OrderUpdateHistory.ST_DescriptionInfo must be valid", false, updateHistoryNote.ST_DescriptionInfo.HasErrors());

			AssertEquals("The order must have 1 OrderUpdateHistoryNote yet", 1, order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description).Length);
			AssertEquals("The OrderUpdateHistoryNote description must be sum of OrderUpdateNote",
				managementUpdateNote2.ST_CreatedByUserName + "\t" + managementUpdateNote2.ST_CreatedDateUtc.ToLongTimeString() + System.Environment.NewLine + managementUpdateNoteText2 + System.Environment.NewLine +
				managementUpdateNote1.ST_CreatedByUserName + "\t" + managementUpdateNote1.ST_CreatedDateUtc.ToLongTimeString() + System.Environment.NewLine + managementUpdateNoteText1 + System.Environment.NewLine,
				updateHistoryNote.ST_NoteText);

			StmNote randomNote2 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special Instructions Text");

			Factory.Save();
			assertOrderNotesInDatabase("OrderUpdateHistoryNote is never saved to database", new StmNote[] { randomNote1, managementUpdateNote1, managementUpdateNote2, randomNote2 });
		}

		public void TestTruncateOrderManagementUpdateWhenExceeding_MaxLength()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var note = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, "blah");
			var managementUpdateNoteText = String.Concat(Enumerable.Repeat("X", note.ST_NoteTextInfo.MaxLength));
			note.ST_NoteText = managementUpdateNoteText;
			AssertEquals("OrderUpdateHistoryNote was created", 1, order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description).Length);
		}

		public void TestOrderUpdateHistoryNoteInternalOrdering()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			string noteText1 = "Update Managment 1";
			string noteText2 = "Update Managment 2";
			string noteText3 = "Update Managment 3";

			StmNote note1 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, noteText1);
			Thread.Sleep(5);
			Factory.Save();

			StmNote note2 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, noteText2);
			Thread.Sleep(5);
			Factory.Save();

			StmNote note3 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, noteText3);
			Thread.Sleep(5);
			Factory.Save();

			String historyNoteText = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description)[0].ST_NoteText;

			Assert("Precondition : They must be inserted in order (first rectent)", note1.ST_CreatedDateUtc < note2.ST_CreatedDateUtc);
			Assert("They must be inserted in order (first rectent)", note2.ST_CreatedDateUtc < note3.ST_CreatedDateUtc);

			AssertEquals("If Note1 is added before Note2 we expect it's Note2 string position be less than Note1",
				true, historyNoteText.IndexOf(noteText1) > historyNoteText.IndexOf(noteText2));

			AssertEquals("If Note2 is added before Note3 we expect it's Note3 string position be less than Note2",
				true, historyNoteText.IndexOf(noteText2) > historyNoteText.IndexOf(noteText3));
		}

		public void TestEnsureOrderUpdateHistoryNoteCannotBeDeleted()
		{
			Order order = Factory.NewWithValidTestData<Order>();

			StmNote note1 = order.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, "Update Managment 1");
			Factory.Save();

			StmNote historyNote = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description)[0];
			AssertNotNull(historyNote);

			order.Notes.VisibleNotes.RemoveAndDelete(historyNote);
			Assert("History Note cannot be deleted", !historyNote.IsDeleted);
		}

		public void TestDoNotCreateOrderUpdateHistoryNoteWhenIsRefreshingByDataRefreshBus()
		{
			_ = BO.Notes.VisibleNotes;
			BO.Factory.Save();

			var boInNewFactory = new BusinessObjectFactory().Load<Order>(BO.PK);
			boInNewFactory.Notes.AddNew(false, "Order Management Update", "Order Management Update");
			AssertNoExceptionThrown(() => boInNewFactory.Factory.Save());
		}

		#endregion

		#region TestAutoIncreaseOrderNumber

		public void TestOrderNumberIsTakenFromNumberFountain()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "codvxxc";
			org.OrganisationTypes = OrganisationTypes.Consignee;
			org.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = false;

			Order order = Order.New(Factory);
			AssertNotNull("Order.New(Factory) returned null.", order);
			order.BuyerPK = org.PK;
			Factory.Save();
			AssertEquals("P000001", order.JD_OrderNumber);

			org = CreateOrgWithAutoIncreaseNumberOption();
			org.MiscServ.OM_IMLastOrderReference = "ORDER123";
			order.JD_OrderNumber = "";
			order.BuyerPK = org.PK;
			AssertEquals("ORDER124", order.JD_OrderNumber);
		}

		public void TestOrderNumberIsTakenFromMiscServReference()
		{
			OrgHeader org = CreateOrgWithAutoIncreaseNumberOption();
			AssertNotNull("createOrgWithAutoIncreaseNumberOption returned null.", org);
			org.MiscServ.OM_IMLastOrderReference = "ORDER123";

			Order order = Order.New(Factory);
			AssertNotNull("Order.New(Factory) returned null.", order);
			order.BuyerPK = org.PK;
			AssertEquals("ORDER124", order.JD_OrderNumber);

			Factory.Save();
			AssertEquals("ORDER124", org.MiscServ.OM_IMLastOrderReference);

			order = Order.New(Factory);
			order.BuyerPK = org.PK;

			Factory.Save();
			AssertEquals("ORDER125", org.MiscServ.OM_IMLastOrderReference);

			org.MiscServ.OM_IMLastOrderReference = "neworder";
			order = Order.New(Factory);
			order.BuyerPK = org.PK;
			AssertEquals("neworder1", order.JD_OrderNumber);

			Factory.Save();
			order = Order.New(Factory);
			order.BuyerPK = org.PK;
			AssertEquals("neworder2", order.JD_OrderNumber);
		}

		public void TestOverflowingOrderNumberFromMiscServTruncates()
		{
			Order order = Factory.New<Order>();

			OrgHeader org = CreateOrgWithAutoIncreaseNumberOption();
			org.MiscServ.OM_IMLastOrderReference = "THIRTYFIVECHARACTERSWILLOVERFLOW999";
			AssertNoExceptionThrown(() => order.BuyerPK = org.PK);
			AssertEquals("THIRTYFIVECHARACTERSWILLOVERFLOW100", order.JD_OrderNumber);

			order.JD_OrderNumber = ZString.Empty;

			org = CreateOrgWithAutoIncreaseNumberOption();
			org.MiscServ.OM_IMLastOrderReference = "LASTORDERREFONLYCONTAININGALPHACHAR";
			AssertNoExceptionThrown(() => order.BuyerPK = org.PK);
			AssertEquals("LASTORDERREFONLYCONTAININGALPHACHAR", order.JD_OrderNumber);
		}

		public void TestManualOrderNumberDoesntChangeMiscServReference()
		{
			OrgHeader org = CreateOrgWithAutoIncreaseNumberOption();
			AssertNotNull("createOrgWithAutoIncreaseNumberOption returned null.", org);
			org.MiscServ.OM_IMLastOrderReference = "ORDER123";

			Order order = Order.New(Factory);
			AssertNotNull("Order.New(Factory) returned null.", order);
			order.BuyerPK = org.PK;
			AssertEquals("ORDER124", order.JD_OrderNumber);

			Factory.Save();
			AssertEquals("ORDER124", org.MiscServ.OM_IMLastOrderReference);

			order = Order.New(Factory);
			order.BuyerPK = org.PK;

			Factory.Save();
			AssertEquals("ORDER125", org.MiscServ.OM_IMLastOrderReference);

			order = Order.New(Factory);
			order.BuyerPK = org.PK;
			order.JD_OrderNumber = "ManualOrderNo1";
			Factory.Save();
			AssertEquals("ORDER125", org.MiscServ.OM_IMLastOrderReference);
		}

		public void TestAutoIncreaseOrderNumber()
		{
			OrgHeader org = CreateOrgWithAutoIncreaseNumberOption();
			AssertNotNull("createOrgWithAutoIncreaseNumberOption returned null.", org);
			org.MiscServ.OM_IMLastOrderReference = "ORDER123";
			Order order = Order.New(Factory);
			AssertNotNull("Order.New(Factory) returned null.", order);
			order.BuyerPK = org.PK;
			AssertEquals("ORDER124", order.JD_OrderNumber);

			order.JD_OrderNumber = ZString.Empty;
			order.BuyerPK = ZGuid.NewZGuid();
			org.MiscServ.OM_IMLastOrderReference = "C41B142ASD19";
			order.BuyerPK = org.PK;
			AssertEquals("C41B142ASD20", order.JD_OrderNumber);

			order.JD_OrderNumber = ZString.Empty;
			order.BuyerPK = ZGuid.NewZGuid();
			org.MiscServ.OM_IMLastOrderReference = "123";
			order.BuyerPK = org.PK;
			AssertEquals("124", order.JD_OrderNumber);

			order.JD_OrderNumber = ZString.Empty;
			order.BuyerPK = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, order.JD_OrderNumber);
			org.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = false;
			org.MiscServ.OM_IMLastOrderReference = "ORDER1";
			order.BuyerPK = org.PK;
			Factory.Save();
			AssertEquals("P000001", order.JD_OrderNumber);
		}

		OrgHeader CreateOrgWithAutoIncreaseNumberOption()
		{
			OrgHeader org = CreateNewOrg(Factory, "CODE");
			org.OrganisationTypes = OrganisationTypes.Consignee;
			org.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			return org;
		}

		#endregion

		#region TestRetrieveSailing

		public void TestRetrieveSailing()
		{
			PopulateNewVoyageWithTestOriginDestinationTimes();

			BO.JD_DepartureVoyage = "";
			BO.JD_IntermediateVoyage = "";
			BO.JD_ArrivalVoyage = "";
			TestRetrieveSailing(
				Order.Schema.JD_RV_NKDepartureVessel, Order.Schema.JD_DepartureVoyage,
				Order.Schema.JD_Milestone_E_DEP, Order.Schema.JD_Milestone_E_ARV);

			BO.JD_DepartureVoyage = "";
			BO.JD_IntermediateVoyage = "";
			BO.JD_ArrivalVoyage = "";
			TestRetrieveSailing(
				Order.Schema.JD_RV_NKArrivalVessel, Order.Schema.JD_ArrivalVoyage,
				Order.Schema.JD_Milestone_E_DEP, Order.Schema.JD_Milestone_E_ARV);

			BO.JD_DepartureVoyage = "";
			BO.JD_IntermediateVoyage = "3vessls";
			BO.JD_ArrivalVoyage = "";
			TestRetrieveSailing(
				Order.Schema.JD_RV_NKDepartureVessel, Order.Schema.JD_DepartureVoyage,
				Order.Schema.JD_Milestone_E_DEP, Order.Schema.JD_E_ARV_1stIntermediate);

			BO.JD_DepartureVoyage = "";
			BO.JD_IntermediateVoyage = "3vessls";
			BO.JD_ArrivalVoyage = "";
			TestRetrieveSailing(
				Order.Schema.JD_RV_NKIntermediateVessel, Order.Schema.JD_IntermediateVoyage,
				Order.Schema.JD_E_DEP_2, Order.Schema.JD_E_ARV_2ndIntermediate);

			BO.JD_DepartureVoyage = "";
			BO.JD_IntermediateVoyage = "3vessls";
			BO.JD_ArrivalVoyage = "";
			TestRetrieveSailing(
				Order.Schema.JD_RV_NKArrivalVessel, Order.Schema.JD_ArrivalVoyage,
				Order.Schema.JD_E_DEP_3, Order.Schema.JD_Milestone_E_ARV);
		}

		ZDateTime fE_VoyageOriginTime;
		ZDateTime fA_VoyageOriginTime;
		ZDateTime fE_VoyageDestinationTime;
		void PopulateNewVoyageWithTestOriginDestinationTimes()
		{
			fE_VoyageOriginTime = ZDateTime.SmallDateTimeNow.AddDays(3);
			fA_VoyageOriginTime = ZDateTime.Now.AddDays(4);
			fE_VoyageDestinationTime = ZDateTime.SmallDateTimeNow.AddDays(1);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "avessel";

			JobVoyage voyage = BO.Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "aflight";
			BO.BuyerPK = GetAOrg().PK;
			BO.SupplierPK = GetAOrg().PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = GetAOrg().OH_RL_NKClosestPort;
			origin.JA_E_DEP = fE_VoyageDestinationTime;
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = GetAOrg().OH_RL_NKClosestPort;
			destination.JB_E_ARV = fE_VoyageOriginTime;
			destination.JB_A_ARV = fA_VoyageOriginTime;
		}

		void TestRetrieveSailing(
			string vesselProp, string voyageProp,
			string e_DEP_Prop, string e_ARV_Prop)
		{
			BO[e_DEP_Prop] = ZDateTime.Empty;
			BO[e_ARV_Prop] = ZDateTime.Empty;

			BO[voyageProp] = ""; // expect no exception

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO[vesselProp] = "avessel";
			BO[voyageProp] = "aflight";
			Assert("Time shouldn't be populated for air", (ZDateTime)BO[e_DEP_Prop] != fE_VoyageDestinationTime);
			Assert("Time shouldn't be populated for air", ((ZDateTime)BO[e_ARV_Prop]) != fE_VoyageOriginTime);

			BO[vesselProp] = "avessel2";
			BO.JD_TransportMode = Constants.TransportModes.Sea;
			BO[vesselProp] = "avessel";
			BO[voyageProp] = "aflight";
			AssertEquals("Estimated Destination time from departure", ((ZDateTime)BO[e_DEP_Prop]), fE_VoyageDestinationTime);
			AssertEquals("Estimated Arrival time from origin", ((ZDateTime)BO[e_ARV_Prop]), fE_VoyageOriginTime);
		}

		#endregion

		#region Test Populate Shipment

		public void TestPopulateShipment_WeightVolumeInnersOuters()
		{
			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_ActualWeight = 10m;
			order1.JD_ActualVolume = 10m;
			order1.JD_Packs = 10;
			order1.JD_F3_NKPackType = Constants.PkgUnit.Skid;

			OrderLine order1_Line1 = order1.OrderLines.AddNew();
			OrderLine order1_Line2 = order1.OrderLines.AddNew();
			order1_Line1.JO_InnerPacks = 5;
			order1_Line1.JO_InnerPacksUQ = Constants.PkgUnit.Keg;
			order1_Line2.JO_InnerPacks = 5;
			order1_Line2.JO_InnerPacksUQ = Constants.PkgUnit.Keg;

			Order order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_ActualWeight = 15m;
			order2.JD_ActualVolume = 15m;
			order2.JD_Packs = 15;
			order2.JD_F3_NKPackType = Constants.PkgUnit.Pallet;

			OrderLine order2_Line1 = order2.OrderLines.AddNew();
			order2_Line1.JO_InnerPacks = 10;
			order2_Line1.JO_F3_NKPackType = Constants.PkgUnit.Drum;
			OrderLine order2_Line2 = order2.OrderLines.AddNew();
			order2_Line2.JO_InnerPacks = 5;
			order2_Line2.JO_F3_NKPackType = Constants.PkgUnit.Drum;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_F3_NKPackType = "AAA";
			shipment.JS_F3_NKTotalCountPackType = "BBB";

			AssertEquals("Precondition: no packs", 0, shipment.JS_OuterPacks);
			AssertEquals("Precondition", "AAA", shipment.JS_F3_NKPackType);
			AssertEquals("Precondition: no packs", 0, shipment.JS_TotalPackageCount);
			AssertEquals("Precondition", "BBB", shipment.JS_F3_NKTotalCountPackType);

			order1.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals(10m, order1.Shipment.JS_ActualWeight);
			AssertEquals(10m, order1.Shipment.JS_ActualVolume);
			AssertEquals(10, order1.Shipment.JS_OuterPacks);
			AssertEquals("Pack type defaulted from order", Constants.PkgUnit.Skid, order1.Shipment.JS_F3_NKPackType);
			AssertEquals(10, order1.Shipment.JS_TotalPackageCount);
			AssertEquals("Pack type defaulted from order", Constants.PkgUnit.Keg, order1.Shipment.JS_F3_NKTotalCountPackType);

			Order orderWithNoPacks = Factory.NewWithValidTestData<Order>();
			orderWithNoPacks.JD_Packs = 0;
			orderWithNoPacks.JD_F3_NKPackType = Constants.PkgUnit.Bottle;
			orderWithNoPacks.OrderLines.AddNew().JO_InnerPacks = 0;
			orderWithNoPacks.OrderLines[0].JO_F3_NKPackType = Constants.PkgUnit.Bottle;

			orderWithNoPacks.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Pack type not changed on shipment", Constants.PkgUnit.Skid, shipment.JS_F3_NKPackType);
			AssertEquals("Pack type not changed on shipment", Constants.PkgUnit.Keg, shipment.JS_F3_NKTotalCountPackType);

			order2.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals(25m, order2.Shipment.JS_ActualWeight);
			AssertEquals(25m, order2.Shipment.JS_ActualVolume);
			AssertEquals(25, order2.Shipment.JS_OuterPacks);
			AssertEquals("Pack type set to generic type of Package", Constants.PkgUnit.Package, order2.Shipment.JS_F3_NKPackType);
			AssertEquals(25, order2.Shipment.JS_TotalPackageCount);
			AssertEquals("Pack type set to generic type of Package", Constants.PkgUnit.Package, order2.Shipment.JS_F3_NKTotalCountPackType);

			Order order3 = Factory.NewWithValidTestData<Order>();
			order3.JD_ActualWeight = 2m;
			order3.JD_ActualVolume = 2m;
			order3.JD_Packs = 2;
			order3.JD_F3_NKPackType = Constants.PkgUnit.Pallet;

			OrderLine order3_Line1 = order3.OrderLines.AddNew();
			OrderLine order3_Line2 = order3.OrderLines.AddNew();
			order3_Line1.JO_InnerPacks = 1;
			order3_Line1.JO_InnerPacksUQ = Constants.PkgUnit.Drum;
			order3_Line2.JO_InnerPacks = 1;
			order3_Line2.JO_InnerPacksUQ = Constants.PkgUnit.Drum;

			shipment.JS_ActualWeight = 6m;
			shipment.JS_ActualVolume = 6m;
			shipment.JS_OuterPacks = 6;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Skid;
			shipment.JS_TotalPackageCount = 6;
			shipment.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Keg;

			order3.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Don't update weight, as total doesn't equal the total of the orders", 6m, order3.Shipment.JS_ActualWeight);
			AssertEquals("Don't update volume, as total doesn't equal the total of the orders", 6m, order3.Shipment.JS_ActualVolume);
			AssertEquals("Don't update outers, as total doesn't equal the total of the orders", 6, order3.Shipment.JS_OuterPacks);
			AssertEquals("Don't update outer pack type", Constants.PkgUnit.Skid, order3.Shipment.JS_F3_NKPackType);
			AssertEquals("Don't update inners, as total doesn't equal the total of the orders", 6, order3.Shipment.JS_TotalPackageCount);
			AssertEquals("Don't update inner pack type", Constants.PkgUnit.Keg, order3.Shipment.JS_F3_NKTotalCountPackType);
		}

		public void TestPopulateShipment_GoodsDescription()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderGoodsDescription = "Order 1 Goods Description 1";

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderGoodsDescription = "Order 2 Goods Description 2";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Should be no note", 0, shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			Assert("Should be empty", shipment.JS_GoodsDescription.IsEmpty);

			order1.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Should be no note", 0, shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			AssertEquals("Shipment Goods description should be order 1 goods description", "Order 1 Goods Description 1", shipment.JS_GoodsDescription);

			shipment.JS_GoodsDescription = string.Empty;

			order2.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Should be no note", 0, shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			Assert("Should be empty because the attached order isn't first order", shipment.JS_GoodsDescription.IsEmpty);
		}

		public void TestPopulateShipment_IncoTerm()
		{
			Order order1 = Factory.New<Order>();
			order1.JD_IncoTerm = "AAA";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = "";

			order1.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Shipment's INCO set from order", "AAA", shipment.JS_INCO);

			Order order2 = Factory.New<Order>();
			order2.JD_IncoTerm = "BBB";

			order2.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Shipment's INCO wasn't empty => not changed", "AAA", shipment.JS_INCO);
		}

		public void TestPopulateShipment_ServiceLevel()
		{
			RefServiceLevel serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "AAA";

			RefServiceLevel serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "BBB";

			Order order1 = Factory.New<Order>();
			order1.JD_RS_NKServiceLevel_NI = serviceLevel1.RS_Code;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RS_NKServiceLevel = "";

			order1.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Shipment's service level set from order", "AAA", shipment.JS_RS_NKServiceLevel);

			Order order2 = Factory.New<Order>();
			order2.JD_RS_NKServiceLevel_NI = serviceLevel2.RS_Code;

			order2.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;
			AssertEquals("Shipment's service level wasn't empty => not changed", "AAA", shipment.JS_RS_NKServiceLevel);
		}

		#endregion

		#region Consols for Binding

		public void TestConsolsForBinding()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			shipment1.AttachedOrders.Add(order);

			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);

			order.JD_JS = ZGuid.Empty;

			AssertEquals("All consols removed", 0, order.ConsolsForBinding.Count);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment2.Consols.Add(consol2);
			Factory.Save();

			order.JD_JS = shipment2.PK;

			AssertContainsExactElementsInAnyOrder(new[] { consol2 }, order.ConsolsForBinding);
		}

		#endregion

		#region IncoTerm_List

		[TestDate(2010, 7, 25)]
		public void TestJD_IncoTerm_List2010()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			AssertIncotermLookupCodes(order, Core.Constants.IncoTerms.Incoterms2000);

			Factory.Save();
			AssertIncotermLookupCodes(order, Core.Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 7, 25)]
		public void TestJD_IncoTerm_List2011()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			AssertIncotermLookupCodes(order, Core.Constants.IncoTerms.Incoterms2010);

			Factory.Save();
			AssertIncotermLookupCodes(order, Core.Constants.IncoTerms.Incoterms2010);
		}

		void AssertIncotermLookupCodes(Order order, IEnumerable<string> expectedCodes)
		{
			string[] actualCodes = (from code in order.JD_IncoTerm_List.Cast<CodeDescriptionPair>() select code.Code).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		#endregion

		#region Calc_InnerPacks

		public void TestCalc_InnerPacks()
		{
			var order = Factory.NewWithValidTestData<Order>();
			AssertEquals("No Order Lines: InnerPacks should be 0", 0, order.JD_Calc_InnerPacks);

			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();
			line1.JO_InnerPacks = 6;
			line1.JO_InnerPacksUQ = Constants.PkgUnit.Keg;
			line2.JO_InnerPacks = 7;
			line2.JO_InnerPacksUQ = "";

			AssertEquals("Order InnerPacks should be 13", 13, order.JD_Calc_InnerPacks);
			AssertEquals("Order InnerPacks type should be KEG", Constants.PkgUnit.Keg, order.JD_Calc_InnerPackType);

			line2.JO_InnerPacksUQ = Constants.PkgUnit.Keg;
			AssertEquals("Order InnerPacks type should be KEG", Constants.PkgUnit.Keg, order.JD_Calc_InnerPackType);

			line2.JO_InnerPacksUQ = Constants.PkgUnit.Drum;
			AssertEquals("Order InnerPacks type should be PKG", Constants.PkgUnit.Package, order.JD_Calc_InnerPackType);
		}

		#endregion

		#region TestOrderLineTotals

		public void TestOrderLineTotals()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			AssertEquals("No Order Lines: Line Count should be 0", 0, order.JD_Calc_LineCount);
			AssertEquals("No Order Lines: Inner Packs should be 0", 0, order.JD_Calc_InnerPacks);
			AssertEquals("No Order Lines: Outer Packs should be 0", 0, order.JD_Calc_OuterPacks);
			AssertEquals("No Order Lines: Quantity should be 0", 0M, order.JD_Calc_TotalQuantity);
			AssertEquals("No Order Lines: Quantity Invoiced should be 0", 0M, order.JD_Calc_TotalQuantityInvoiced);
			AssertEquals("No Order Lines: Quantity Received should be 0", 0M, order.JD_Calc_TotalQuantityReceived);
			AssertEquals("No Order Lines: Quantity Remaining should be 0", 0M, order.JD_Calc_TotalQuantityRemaining);

			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();
			line1.JO_InnerPacks = 29;
			line1.JO_OuterPacks = 23;
			line1.JO_Quantity = 19;
			line1.JO_QtyInvoiced = 17;
			line1.JO_QtyReceived = 13;
			line2.JO_InnerPacks = 11;
			line2.JO_OuterPacks = 7;
			line2.JO_Quantity = 5;
			line2.JO_QtyInvoiced = 3;
			line2.JO_QtyReceived = 2;

			AssertEquals("Order Line Count", 2, order.JD_Calc_LineCount);
			AssertEquals("Order Inner Packs", 40, order.JD_Calc_InnerPacks);
			AssertEquals("Order Outer Packs", 30, order.JD_Calc_OuterPacks);
			AssertEquals("Order Quantity", 24M, order.JD_Calc_TotalQuantity);
			AssertEquals("Order Quantity Invoiced", 20M, order.JD_Calc_TotalQuantityInvoiced);
			AssertEquals("Order Quantity Received", 15M, order.JD_Calc_TotalQuantityReceived);
			AssertEquals("Order Quantity Remaining", 9M, order.JD_Calc_TotalQuantityRemaining);
		}

		#endregion

		#region TestOrderNumberChangeAttemptWhileSplitsExist

		public void TestOrderNumberChangeAttemptWhileSplitsExist()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "1234";
			AssertEquals("Order number can be changed when no splits exist", "1234", order.JD_OrderNumber);

			Order orderSplit = order.SplitOrder(CreateOrderType.Split);
			Factory.Save();

			AssertEquals("Split number on primary order for the test", (byte)0, order.JD_OrderNumberSplit);
			AssertEquals("Split number on order split for the test", (byte)1, orderSplit.JD_OrderNumberSplit);

			orderSplit.OrderNumberChangeAttemptedWhileSplitsExist += new EventHandler(OnOrder_OrderNumberChangeAttemptedWhileSplitsExist);
			orderSplit.JD_OrderNumber = "4321";
			Assert("Order number cannot be changed while split siblings exist", orderSplit.JD_OrderNumber != "4321");
			AssertEquals("Event should be raised indicating the illegal change attempt to the order number", true, OnOrder_OrderNumberChangeAttemptedWhileSplitsExistCalled);
		}

		bool OnOrder_OrderNumberChangeAttemptedWhileSplitsExistCalled;
		void OnOrder_OrderNumberChangeAttemptedWhileSplitsExist(object sender, EventArgs e)
		{
			OnOrder_OrderNumberChangeAttemptedWhileSplitsExistCalled = true;
		}

		#endregion

		#region TestOrderSplitSiblingsNotAccessedOnLoadOrSave

		public void TestOrderSplitSiblingsNotAccessedOnLoad()
		{
			var savedOrder = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestOrderWithOrderSplitSiblingsAccessCheck loadedOrder = newFactory.Load<TestOrderWithOrderSplitSiblingsAccessCheck>(savedOrder.PK);

			// poke the object a bit for the test
			object notUsed;
			notUsed = loadedOrder.JD_OrderNumber;
			notUsed = loadedOrder.JD_OrderNumberInfo;
			notUsed = loadedOrder.JD_OrderNumberSplit;
			notUsed = loadedOrder.JD_OrderNumberSplitInfo;

			AssertEquals("Should not access order split siblings when reading a new order", false, loadedOrder.OrderSplitSiblingsAccessed);
			loadedOrder.JD_OrderDate = new ZDateTime(2005, 1, 1);
			newFactory.Save();
			AssertEquals("Should not access order split siblings when saving a change to the order", false, loadedOrder.OrderSplitSiblingsAccessed);
		}

		class TestOrderWithOrderSplitSiblingsAccessCheck : Order
		{
			public TestOrderWithOrderSplitSiblingsAccessCheck(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool OrderSplitSiblingsAccessed;

			public override OrderCollection OrderSplitSiblings
			{
				get
				{
					OrderSplitSiblingsAccessed = true;
					return base.OrderSplitSiblings;
				}
			}
		}

		#endregion

		#region TestNoBusinessObjectsLoadedOnSaveIfNoChangesMade

		public void TestNoBusinessObjectsLoadedOnSaveIfNoChangesMade()
		{
			var order = Factory.NewWithValidTestData<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.Deliveries.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Order loadedOrder = newFactory.Load<Order>(order.PK);
			newFactory.Save();
			AssertBusinessObjectTypesNotCreatedOrLoaded(newFactory, typeof(OrderLine), typeof(OrderLineDelivery));
		}

		#endregion

		#region IRelatedJob

		public void TestControllerId()
		{
			IRelatedJob order = Factory.New<Order>();
			AssertEquals(ControllerIDs.Orders, order.ControllerID);
		}

		public void TestJobDescription()
		{
			IRelatedJob order = Factory.New<Order>();
			AssertEquals("Forwarding Order", order.JobDescription);
		}

		public void TestJobNo()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "ORDERME";
			order.JD_OrderNumberSplit = new ZByte(2);
			IRelatedJob iRelatedJob = order;
			AssertEquals("ORDERME-2", iRelatedJob.JobNumber);
		}

		public void TestJobStatus()
		{
			var order = Factory.New<Order>();
			order.JD_OrderStatus = "INC";
			IRelatedJob iRelatedJob = order;
			AssertEquals("INC", iRelatedJob.JobStatus);
		}

		#endregion

		#region IDocAddresses

		public void TestDocAddresses()
		{
			var order = Factory.New<Order>();
			AssertEquals(typeof(JobDocAddressDependentCollection), order.DocAddresses.GetType());

			var docAddresses = order as IDocAddresses;
			var addressType = docAddresses.GetDocAddressRequirement(DocAddressType.GoodsAvailableAt).DefaultDocAddressType;
			AssertEquals(DocAddressType.GoodsAvailableAt, addressType);

			addressType = docAddresses.GetDocAddressRequirement(DocAddressType.GoodsDeliveredTo).DefaultDocAddressType;
			AssertEquals(DocAddressType.GoodsDeliveredTo, addressType);
		}

		public void TestSupportedAddressTypes()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				IDocAddresses order = Factory.New<Order>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.ControllingCustomer,
					DocAddressType.Warehouse,
					DocAddressType.ControllingAgent,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
					DocAddressType.GoodsAvailableAt,
					DocAddressType.GoodsDeliveredTo,
					DocAddressType.Manufacturer,
					DocAddressType.ConsigneeDocumentaryAddress,
				}, order.SupportedAddressTypes);
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				IDocAddresses order = Factory.New<Order>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.ControllingCustomer,
					DocAddressType.Warehouse,
					DocAddressType.ControllingAgent,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
					DocAddressType.GoodsAvailableAt,
					DocAddressType.GoodsDeliveredTo,
					DocAddressType.Manufacturer,
				}, order.SupportedAddressTypes);
			});
		}

		public void TestGetOrgHeaderList()
		{
			IDocAddresses order = Factory.New<Order>();
			AssertNull(order.GetOrgHeaderList(DocAddressType.ControllingCustomer));

			var orgHeaderList = order.GetOrgHeaderList(DocAddressType.Warehouse);
			AssertNotNull(orgHeaderList);

			var organisationTypeDefault = new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True);
			var cfsDefault = new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)OrgConstants.FilterControl.SecondaryOrgType.Depot);
			AssertContainsExactElementsInAnyOrder(new[] { organisationTypeDefault, cfsDefault }, orgHeaderList.FilterBusinessObjectDefaults);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			IDocAddresses order = Factory.New<Order>();
			AssertEquals(Env.Security.OrderManager, order.GetCanOverrideCheckpoint(null));
		}

		public void TestCanDeleteAddress_WhenDocAddressIsControllingCustomer_OrderLineAttachedToSupplierBooking()
		{
			var supplierBookingStatuses = typeof(Constants.SupplierBookingStatus).GetFields();

			foreach (var status in supplierBookingStatuses)
			{
				var supplierBookingStatus = status.GetValue(null) as string;
				if (supplierBookingStatus == Constants.SupplierBookingStatus.Converted)
				{
					continue;
				}

				Factory.Save();

				if (supplierBookingStatus == Constants.SupplierBookingStatus.Cancelled)
				{
					AssertCanDeleteAddress(
						$"{supplierBookingStatus} - Controlling Customer can be deleted when SBK is CAN status and order lines are attached.",
						DocAddressType.ControllingCustomer,
						supplierBookingStatus,
						expectedOutput: true,
						attachOrderLine: true
					);
				}
				else
				{
					AssertCanDeleteAddress(
						$"{supplierBookingStatus} - Controlling Customer cannot be deleted when SBK is not CAN status and order lines are attached.",
						DocAddressType.ControllingCustomer,
						supplierBookingStatus,
						expectedOutput: false,
						attachOrderLine: true
					);
				}
			}
		}

		public void TestCanDeleteAddress_WhenDocAddressIsControllingCustomer_NoOrderLineAttachedToSupplierBooking()
		{
			AssertCanDeleteAddress(
				"Controlling Customer can be deleted regardless of SBK status when no order lines are attached.",
				DocAddressType.ControllingCustomer,
				supplierBookingStatus: null,
				expectedOutput: true,
				attachOrderLine: false
			);
		}

		#endregion

		#region IModuleToModule

		public void TestCanExportData()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			IModuleToModule iModuleToModule = order;
			ZString errorMessage;
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("The Buyer is not flagged as a Warehouse Client.", errorMessage);

			buyer.OH_IsWarehouseClient = true;
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("No Warehouse is specified.", errorMessage);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			order.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(true, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("", errorMessage);
		}

		public void TestGetRelatedObject()
		{
			var order = Factory.New<Order>();
			IModuleToModule iModuleToModule = order;
			AssertNull(iModuleToModule.GetRelatedObject());

			var warehouseReceive1 = Factory.New<IWhsReceive>();
			var orderReceivePivot1 = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_ParentId] = ZGuid.NewZGuid();
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive1.PK;
			orderReceivePivot1[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive1.WD_DocketType;

			var warehouseReceive2 = Factory.New<IWhsReceive>();
			var orderReceivePivot2 = (BusinessObject)Factory.New<IWhsDocketJobPivot>();
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_ParentId] = order.PK;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_ParentTableCode] = order.TablePrefix;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_WD_Docket] = warehouseReceive2.PK;
			orderReceivePivot2[WhsDocketJobPivotSchema.WV_DocketType] = warehouseReceive2.WD_DocketType;
			AssertEquals((BusinessObject)warehouseReceive2, iModuleToModule.GetRelatedObject());
		}

		public void TestRecipientOrganisation()
		{
			var order = Factory.New<Order>();
			IModuleToModule iModuleToModule = order;
			AssertNull(iModuleToModule.RecipientOrganisation);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			order.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(orgHeader, iModuleToModule.RecipientOrganisation);
		}

		public void TestCanExportDataDoesNotThrowNRE()
		{
			ZString errorMessage;
			var order = Factory.New<Order>();
			var iModuleToModule = order as IModuleToModule;

			AssertNoExceptionThrown("should handle the NRE exception", () => iModuleToModule.CanExportData(out errorMessage));
		}
		#endregion

		#region IAttachedOrder

		public void TestIAttachedOrder()
		{
			var order = Factory.New<Order>();
			var line1 = order.OrderLines.AddNew();
			IAttachedOrder attachedOrder = order;

			line1.JO_ActualWeight = 100m;
			line1.JO_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(100m, attachedOrder.TotalWeight);
			AssertEquals("KG", attachedOrder.WeightUnit);

			line1.JO_ActualVolume = 1m;
			line1.JO_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals(1m, attachedOrder.TotalVolume);
			AssertEquals("M3", attachedOrder.VolumeUnit);

			order.JD_OrderGoodsDescription = "Cakes";
			AssertEquals("Cakes", attachedOrder.GoodsDescription);

			order.JD_OrderDate = new ZDateTime(2012, 1, 1);
			AssertEquals(new ZDateTime(2012, 1, 1), attachedOrder.JobDate);

			order.JD_OrderNumber = "12345";
			AssertEquals("12345", attachedOrder.JobNo);

			order.JD_OrderStatus = "INV";
			AssertEquals("INV", attachedOrder.JobStatus);

			AssertEquals(Order.AttachedOrderType, attachedOrder.JobType);
			AssertEquals("Order Manager", attachedOrder.JobDescription);
			AssertEquals(ModuleIDs.Orders, attachedOrder.ModuleID);
			AssertEquals(ControllerIDs.Orders, attachedOrder.ControllerID);
		}

		public void TestIAttachedOrder_TotalWeight_CalculationsAreSafe()
		{
			var order = Factory.New<Order>();
			order.JD_UnitOfWeight = "xx";

			var line1 = order.OrderLines.AddNew();
			line1.JO_ActualWeight = 10;
			line1.JO_UnitOfWeight = Constants.Weight.Kilograms;

			var line2 = order.OrderLines.AddNew();
			line2.JO_ActualWeight = 20;
			line2.JO_UnitOfWeight = "XX";

			var attachedOrder = order as IAttachedOrder;

			AssertEquals(0m, attachedOrder.TotalWeight);
			AssertEquals("", attachedOrder.WeightUnit);
		}

		public void TestIAttachedOrder_TotalWeight()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_UnitOfWeight = Constants.Weight.Pounds;
			order.JD_ActualWeight = 50;

			var line1 = order.OrderLines.AddNew();
			var line2 = order.OrderLines.AddNew();
			var line3 = order.OrderLines.AddNew();
			line1.JO_ActualWeight = 10;
			line1.JO_UnitOfWeight = Constants.Weight.Kilograms;
			line2.JO_ActualWeight = 20000;
			line2.JO_UnitOfWeight = Constants.Weight.Grams;
			line3.JO_ActualWeight = 0.03;
			line3.JO_UnitOfWeight = Constants.Weight.Tonnes;

			var attachedOrder = order as IAttachedOrder;

			AssertEquals("Total weight should be 132 pounds", 132.277m, attachedOrder.TotalWeight);
			AssertEquals("Total weight should be 132 pounds", 132.277m, order.JD_Calc_TotalWeight);
			AssertEquals("Total weight unit should be pounds", Constants.Weight.Pounds, attachedOrder.WeightUnit);

			line1.JO_ActualWeight = 10;
			line1.JO_UnitOfWeight = Constants.Weight.Kilograms;
			line2.JO_ActualWeight = 20;
			line2.JO_UnitOfWeight = Constants.Weight.Kilograms;
			line3.JO_ActualWeight = 30;
			line3.JO_UnitOfWeight = Constants.Weight.Kilograms;

			AssertEquals("Total weight should be 60 Kilograms", 60m, attachedOrder.TotalWeight);
			AssertEquals("Total weight should be 60 Kilograms", 60m, order.JD_Calc_TotalWeight);
			AssertEquals("Total weight unit should be pounds", Constants.Weight.Kilograms, attachedOrder.WeightUnit);
		}

		public void TestIAttachedOrder_TotalVolume_CalculationsAreSafe()
		{
			var order = Factory.New<Order>();
			order.JD_UnitOfVolume = "xx";

			var line1 = order.OrderLines.AddNew();
			line1.JO_ActualVolume = 1;
			line1.JO_UnitOfVolume = Constants.Volume.CubicMetres;

			var line2 = order.OrderLines.AddNew();
			line2.JO_ActualVolume = 2;
			line2.JO_UnitOfVolume = "XX";

			var attachedOrder = order as IAttachedOrder;

			AssertEquals(0m, attachedOrder.TotalVolume);
			AssertEquals("", attachedOrder.VolumeUnit);
		}

		public void TestIAttachedOrder_TotalVolume()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_UnitOfVolume = Constants.Volume.CubicMetres;
			order.JD_ActualVolume = 5;

			var line1 = order.OrderLines.AddNew();
			var line2 = order.OrderLines.AddNew();
			var line3 = order.OrderLines.AddNew();
			line1.JO_ActualVolume = 1000;
			line1.JO_UnitOfVolume = Constants.Volume.CubicDecimetres;
			line2.JO_ActualVolume = 2000000;
			line2.JO_UnitOfVolume = Constants.Volume.CubicCentimeters;
			line3.JO_ActualVolume = 30000;
			line3.JO_UnitOfVolume = Constants.Volume.CubicInches;

			var attachedOrder = order as IAttachedOrder;

			AssertEquals("Total volume should be 3 cubicmeters", 3.492m, attachedOrder.TotalVolume);
			AssertEquals("Total volume should be 3 cubicmeters", 3.492m, order.JD_Calc_TotalVolume);
			AssertEquals("Total volume unit should be cubicmeters", Constants.Volume.CubicMetres, attachedOrder.VolumeUnit);

			line1.JO_ActualVolume = 1;
			line1.JO_UnitOfVolume = Constants.Volume.CubicCentimeters;
			line2.JO_ActualVolume = 2;
			line2.JO_UnitOfVolume = Constants.Volume.CubicCentimeters;
			line3.JO_ActualVolume = 3;
			line3.JO_UnitOfVolume = Constants.Volume.CubicCentimeters;

			AssertEquals("Total volume should be 6 CubicCentimeters", 6m, attachedOrder.TotalVolume);
			AssertEquals("Total volume should be 6 CubicCentimeters", 6m, order.JD_Calc_TotalVolume);
			AssertEquals("Total volume unit should be CubicCentimeters", Constants.Volume.CubicCentimeters, attachedOrder.VolumeUnit);
		}

		public void TestIAttachedOrder_TotalPacks()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_F3_NKPackType = Constants.PkgUnit.Bundle;

			var line1 = order.OrderLines.AddNew();
			var line2 = order.OrderLines.AddNew();
			var line3 = order.OrderLines.AddNew();
			line1.JO_OuterPacksUQ = Constants.PkgUnit.Bag;
			line1.JO_OuterPacks = 1;
			line2.JO_OuterPacksUQ = Constants.PkgUnit.Basket;
			line2.JO_OuterPacks = 2;
			line3.JO_OuterPacksUQ = Constants.PkgUnit.Box;
			line3.JO_OuterPacks = 3;

			var attachedOrder = order as IAttachedOrder;

			AssertEquals("Total packs should be 6", 6, order.JD_Calc_OuterPacks);
			AssertEquals("Total packs should be 6", 6, attachedOrder.TotalPacks);
			AssertEquals("Total pack type should be package", Constants.PkgUnit.Package, order.JD_Calc_OuterPacksType);

			line1.JO_OuterPacksUQ = Constants.PkgUnit.Envelope;
			line1.JO_OuterPacks = 1;
			line2.JO_OuterPacksUQ = Constants.PkgUnit.Envelope;
			line2.JO_OuterPacks = 2;
			line3.JO_OuterPacksUQ = Constants.PkgUnit.Envelope;
			line3.JO_OuterPacks = 3;

			AssertEquals("Total packs should be 6", 6, order.JD_Calc_OuterPacks);
			AssertEquals("Total packs should be 6", 6, attachedOrder.TotalPacks);
			AssertEquals("Total pack type should be Envelope", Constants.PkgUnit.Envelope, order.JD_Calc_OuterPacksType);
		}

		#endregion

		#region ICancelable

		public void TestCanCancel_BuyerIsValidAndNoAttachments_ReturnNull()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var isErrorMessageGenerated = !string.IsNullOrWhiteSpace(order.CanCancel());
			AssertEquals("Error message is generated", false, isErrorMessageGenerated);
		}

		public void TestCanCancel_BuyerIsInvalid_ReturnErrorMessage()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Guid.Empty;

			var isErrorMessageGenerated = !string.IsNullOrWhiteSpace(order.CanCancel());
			AssertEquals("Error message is generated", true, isErrorMessageGenerated);
		}

		public void TestCanCancel_ShipmentIsAttached_ReturnErrorMessage()
		{
			var buyer = Factory.New<OrgHeader>();
			var shipment = CommonShipment.New(Factory);

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_JS = shipment.PK;

			var isErrorMessageGenerated = !string.IsNullOrWhiteSpace(order.CanCancel());
			AssertEquals("Error message is generated", true, isErrorMessageGenerated);
		}

		public void TestCanCancel_DeclarationIsAttached_ReturnErrorMessage()
		{
			var buyer = Factory.New<OrgHeader>();
			var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_JE = declaration.PK;

			var isErrorMessageGenerated = !string.IsNullOrWhiteSpace(order.CanCancel());
			AssertEquals("Error message is generated", true, isErrorMessageGenerated);
		}

		public void TestCanReactivate()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order1 = Factory.NewWithValidTestData<Order>();
			var order2 = Factory.NewWithValidTestData<Order>();
			var order3 = Factory.NewWithValidTestData<Order>();

			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "1";
			order1.JD_OrderNumberSplit = (ZByte)0;

			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "1";
			order2.JD_OrderNumberSplit = (ZByte)0;
			order2.JD_IsCancelled = true;

			order3.BuyerPK = buyer.PK;
			order3.JD_OrderNumber = "2";
			order3.JD_OrderNumberSplit = (ZByte)0;
			order3.JD_IsCancelled = true;

			Factory.Save();

			AssertEquals("An active order already exists for the buyer with this order number and the order cannot be reactivated.", order2.CanReactivate());
			AssertNull(order3.CanReactivate());
		}

		public void TestReadOnlySetWhenCancelled()
		{
			Order order1 = Factory.New<Order>();

			order1.JD_IsCancelled = true;
			foreach (ZPropertyInfo property in order1.ZPropertyInfoHash)
			{
				AssertEquals("All Properties should be read-only, no exception", true, property.ReadOnly);
			}
		}

		public void TestLoadingCancelledOrdersFieldsReadOnly()
		{
			var order1 = Factory.NewWithValidTestData<Order>();

			order1.JD_IsCancelled = true;
			foreach (ZPropertyInfo property in order1.ZPropertyInfoHash)
			{
				AssertEquals("All Properties should be read-only, no exception", true, property.ReadOnly);
			}

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Order order2 = factory2.Load<Order>(order1.PK);

			foreach (ZPropertyInfo property in order2.ZPropertyInfoHash)
			{
				AssertEquals("All Properties should be read-only on loaded order, no exception", true, property.ReadOnly);
			}
		}

		#endregion

		#region IJobNumber

		public void TestJobNumber()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "OrderNumber";
			order.JD_OrderNumberSplit = 2;
			AssertEquals("OrderNumber-2", ((IJobNumber)order).JobNumber);
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerInfo()
		{
			IDocManagerSupport order = Factory.New<Order>();
			AssertEquals("Code should be ORD. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "ORD", order.DocManagerInfo.DocManagerCode);
			AssertEquals(typeof(OrderDocManagerInfo), order.DocManagerInfo.GetType());
		}

		#endregion

		#region IDocumentSupportable Tests

		public void TestGetDocBusinessObject()
		{
			Order order = Factory.New<Order>();
			BusinessObject shipmentBizO = order.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.Shipment), null)[0].ParentBusinessObject;
			AssertEquals(order.PK, shipmentBizO.PK);
			BusinessObject orderBizO = order.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.Order), null)[0].ParentBusinessObject;
			AssertEquals(order.PK, orderBizO.PK);
			AssertEquals(0, order.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.Consol), null).Length);
		}

		public void TestSupportedDataContext()
		{
			Order order = Factory.New<Order>();
			AssertEquals("Constants.DataContext.Shipment is Supported", true, order.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Shipment)));
			AssertEquals("Constants.DataContext.Order is Supported", true, order.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Order)));
			AssertEquals("Constants.DataContext.PreAlert is Supported", true, order.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.PreAlert)));
		}

		#endregion

		#region IDocumentAutoDelivery Tests

		public void TestTransportMode()
		{
			Order order = Factory.New<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("IDocumentSupportable TransportMode", order.JD_TransportMode, order.DocumentSupporter.TransportMode);
		}

		public void TestIsImport()
		{
			Order order = Factory.New<Order>();

			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";
			AssertEquals("IDocumentAutoDelivery IsImport", false, order.DocumentSupporter.IsImport);

			order.JD_RL_NKPortOfLoading = "USLAX";
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("IDocumentAutoDelivery IsImport", true, order.DocumentSupporter.IsImport);
		}

		public void TestLocalPort()
		{
			Order order = Factory.New<Order>();
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";

			AssertEquals("IDocumentAutoDelivery LocalPort for CNR", "AUSYD", order.DocumentSupporter.LocalPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery LocalPort for CNE", "USLAX", order.DocumentSupporter.LocalPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery LocalPort for other contact type", "", order.DocumentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestForeignPort()
		{
			Order order = Factory.New<Order>();
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";

			AssertEquals("IDocumentAutoDelivery ForeignPort for CNR", "USLAX", order.DocumentSupporter.ForeignPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery ForeignPort for CNE", "AUSYD", order.DocumentSupporter.ForeignPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery ForeignPort for other contact type", "", order.DocumentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestGetContactOrganisation()
		{
			var order = Factory.New<Order>();
			AssertNull(order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ARV));

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYERCODE";
			order.BuyerPK = buyer.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPCODE";
			order.SupplierPK = supplier.PK;

			AssertEquals("BUYERCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ARV).OrgHeader.Code);

			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			AssertEquals("BUYERCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ARV).OrgHeader.Code);

			shipment.ConsigneePK = supplier.PK;
			AssertEquals("SUPPCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ARV).OrgHeader.Code);

			order.JD_JS = ZGuid.Empty;
			AssertEquals("SUPPCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.DEP).OrgHeader.Code);

			order.JD_JS = shipment.PK;
			AssertEquals("SUPPCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.DEP).OrgHeader.Code);

			shipment.ConsignorPK = buyer.PK;
			AssertEquals("BUYERCODE", order.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.DEP).OrgHeader.Code);

			order.JD_JS = ZGuid.Empty;
			AssertNull(order.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ARV));
			AssertNull("Should be null", order.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.DEP));

			order.JD_OH_ReceivingAgent = buyer.PK;
			order.JD_OH_SendingAgent = supplier.PK;
			AssertEquals("Should be receiving Agent", buyer.OH_Code, order.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ARV).OrgHeader.Code);
			AssertEquals("Should be Sending Agent", supplier.OH_Code, order.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.DEP).OrgHeader.Code);

			order.JD_JS = shipment.PK;
			AssertNull("Should be null", order.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ARV));
			AssertNull("Should be null", order.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.DEP));

			var consol = order.Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = order.Shipment.JS_RL_NKDestination;

			consol.JK_OA_SendingForwarderAddress = buyer.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = supplier.MainAddress.PK;

			AssertEquals("Should be receiving Agent", supplier.OH_Code, order.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ARV).OrgHeader.Code);
			AssertEquals("Should be Sending Agent", buyer.OH_Code, order.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.DEP).OrgHeader.Code);
		}

		#endregion

		#region ICalenderReminder Tests

		public void TestCalendarReminder()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Some Buyers Organisation";
			buyer.OH_Code = "SOMEBUY1";

			OrgContact contact = buyer.Contacts.AddNew();
			contact.OC_ContactName = "Roger Moore";
			contact.OC_Email = "roger.moore@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";

			OrgDocument buyDoc = contact.Documents.AddNew();
			buyDoc.OD_DocumentGroup = "CNE";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Some Supplier Organisation";
			supplier.OH_Code = "SOMESUP1";

			OrgContact supContact = supplier.Contacts.AddNew();
			supContact.OC_ContactName = "Dr No";
			supContact.OC_Email = "dr.no@edi.com.au";
			supContact.OC_Fax = "+61 2 9099 0777";
			supContact.OC_Phone = "+61 2 9025 1180";

			OrgDocument supDoc = supContact.Documents.AddNew();
			supDoc.OD_DocumentGroup = "CNR";

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_OrderNumber = "10013";
			order.JD_FollowUpDate = ZDateTime.Now;

			string expectedSubject = "Followup Order: 10013, Ordered By: Some Buyers Organisation";
			string expectedBody = @"Follow up Order 10013 Ordered By: Some Buyers Organisation

Buyer Contact Details: Some Buyers Organisation
Contact: Roger Moore
Email: roger.moore@edi.com.au
Fax: +61 2 9025 1199
Phone: +61 2 9025 1180

Supplier Contact Details: Some Supplier Organisation
Contact: Dr No
Email: dr.no@edi.com.au
Fax: +61 2 9099 0777
Phone: +61 2 9025 1180
";

			AssertEquals("Calendar reminder created with correct body", expectedBody, order.FollowUpDateReminder.Body);
			AssertEquals("Calendar reminder created with correct Subject", expectedSubject, order.FollowUpDateReminder.Subject);

			order.SupplierPK = ZGuid.Empty;
			order.JD_FollowUpDate = ZDateTime.Now.AddDays(35);

			expectedBody = @"Follow up Order 10013 Ordered By: Some Buyers Organisation

Buyer Contact Details: Some Buyers Organisation
Contact: Roger Moore
Email: roger.moore@edi.com.au
Fax: +61 2 9025 1199
Phone: +61 2 9025 1180

";
			AssertEquals("Should have created another reminder with Body", expectedBody, order.FollowUpDateReminder.Body);

			contact.Delete();

			expectedBody = @"Follow up Order 10013 Ordered By: Some Buyers Organisation

Buyer Contact Details: Some Buyers Organisation
Contact: The Import Manager
Email: 
Fax: 
Phone: 

";
			AssertEquals("Calendar reminder created with correct body", expectedBody, order.FollowUpDateReminder.Body);
		}

		public void TestFollowUpDateReminder_WhenZDateTimeIsInvalid()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_FollowUpDate = ZDateTime.Invalid;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region NoteTypes

		public void TestNoteTypes()
		{
			var expectedNoteTypes = new PredefinedNoteType[]
			{
				PredefinedNoteTypes.Instance.SpecialInstructions,
				PredefinedNoteTypes.Instance.ExtraOrderDetails,
				PredefinedNoteTypes.Instance.OrderManagementUpdate,
				PredefinedNoteTypes.Instance.OrderManagementNote,
				PredefinedNoteTypes.Instance.InternalWorkNotes,
				PredefinedNoteTypes.Instance.FaxEmailTransmissionLog,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.AgentNotes,
				PredefinedNoteTypes.Instance.HandlingInstructions
			};

			Order order = Factory.New<Order>();

			foreach (PredefinedNoteType expectedNoteType in expectedNoteTypes)
			{
				string message = string.Format("NoteTypes should contain {0} note type", expectedNoteType.Description);
				AssertEquals(message, true, order.NoteTypes.Cast<PredefinedNoteType>().Contains(expectedNoteType));
			}
		}

		#endregion

		#region OrderLines

		public void TestGetOrderLinesCore()
		{
			var mock = Factory.NewMoq<Order>();
			Order parentOrder = mock.Object;
			mock.Protected().Setup<OrderLineCollection>("GetOrderLinesCore").Returns(new OrderLineCollection(parentOrder));
			OrderLineCollection collection = parentOrder.OrderLines;
			AssertNotNull(collection);
			AssertEquals(typeof(OrderLineCollection), collection.GetType());
			mock.VerifyAll();
		}

		public void TestSetDefaultTolerances_WhenTransportModeChanges()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgAddress = CreateOrgHeaderWithAddress();
				var orgHeader = orgAddress.Header;

				var order = Factory.NewWithValidTestData<Order>();
				order.JD_OA_BuyerAddress = orgAddress.PK;
				order.SupplierPK = orgHeader.PK;
				order.JD_TransportMode = Constants.TransportModes.Sea;

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = orgHeader.PK;
				link.OL_OH_Supplier = orgHeader.PK;
				link.OL_RN_NKImporterCountry = orgAddress.Country.Code;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.Air;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				order.OrderLines.AddNew();
				order.OrderLines.AddNew();

				AssertEquals(1, order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
				AssertNotEquals(tolerance.OLT_TransportMode, order.JD_TransportMode);
				CombineAssertions("Tolerances before TransportMode changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
					}
				});

				order.JD_TransportMode = tolerance.OLT_TransportMode;
				CombineAssertions("Tolerances after TransportMode changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, orderLine.JO_LateShipmentLimitDays);
					}
				});
			}
		}

		public void TestSetDefaultTolerances_WhenBuyerChanges()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgAddress = CreateOrgHeaderWithAddress();
				var orgHeader = orgAddress.Header;
				var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var order = Factory.NewWithValidTestData<Order>();
				order.BuyerPK = otherOrgHeader.PK;
				order.SupplierPK = orgHeader.PK;

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = orgHeader.PK;
				link.OL_OH_Supplier = orgHeader.PK;
				link.OL_RN_NKImporterCountry = orgAddress.Country.Code;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				order.OrderLines.AddNew();
				order.OrderLines.AddNew();

				AssertNull(order.SupplierBuyerLinkFromBuyerAddressCountry);
				CombineAssertions("Tolerances before Buyer changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
					}
				});

				order.JD_OA_BuyerAddress = orgAddress.PK;
				AssertEquals(1, order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
				CombineAssertions("Tolerances after Buyer changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, orderLine.JO_LateShipmentLimitDays);
					}
				});
			}
		}

		public void TestSetDefaultTolerances_WhenSupplierChanges()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgAddress = CreateOrgHeaderWithAddress();
				var orgHeader = orgAddress.Header;
				var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var order = Factory.NewWithValidTestData<Order>();
				order.JD_OA_BuyerAddress = orgAddress.PK;
				order.SupplierPK = otherOrgHeader.PK;

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = orgHeader.PK;
				link.OL_OH_Supplier = orgHeader.PK;
				link.OL_RN_NKImporterCountry = orgAddress.Country.Code;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				order.OrderLines.AddNew();
				order.OrderLines.AddNew();

				AssertNull(order.SupplierBuyerLinkFromBuyerAddressCountry);
				CombineAssertions("Tolerances before Supplier changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
					}
				});

				order.SupplierPK = orgHeader.PK;
				AssertEquals(1, order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
				CombineAssertions("Tolerances after Supplier changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, orderLine.JO_LateShipmentLimitDays);
					}
				});
			}
		}

		public void TestSetDefaultTolerances_Performance()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgAddress = CreateOrgHeaderWithAddress();
				var orgHeader = orgAddress.Header;
				var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var order = Factory.NewWithValidTestData<Order>();
				order.JD_OA_BuyerAddress = orgAddress.PK;
				order.SupplierPK = otherOrgHeader.PK;

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = orgHeader.PK;
				link.OL_OH_Supplier = orgHeader.PK;
				link.OL_RN_NKImporterCountry = orgAddress.Country.Code;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				for (var i = 0; i < 100; i++)
				{
					var orderLine = order.OrderLines.AddNew();
					orderLine.JO_Partno = "PartNum" + i;
				}

				AssertNull(order.SupplierBuyerLinkFromBuyerAddressCountry);
				CombineAssertions("Tolerances before Supplier changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
					}
				});

				using (AssertDbHitsForAllFactories(
					"Fetch tolerances for buyer supplier link and transport mode, returning all parts only once.",
					new Dictionary<string, int> { { OrgSupplierBuyerLinkToleranceSchema.Constants.TableName, 1 } },
					true))
				{
					order.SupplierPK = orgHeader.PK;
				}
				AssertEquals(1, order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
				CombineAssertions("Tolerances after Supplier changes.", () =>
				{
					foreach (var orderLine in order.OrderLines)
					{
						AssertEquals(nameof(orderLine.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, orderLine.JO_UnderQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, orderLine.JO_OverQuantityPercentageLimit);
						AssertEquals(nameof(orderLine.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, orderLine.JO_EarlyShipmentLimitDays);
						AssertEquals(nameof(orderLine.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, orderLine.JO_LateShipmentLimitDays);
					}
				});
			}
		}

		OrgAddress CreateOrgHeaderWithAddress()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var address = orgHeader.Addresses.AddNew();
			address.FillWithValidTestData();
			address.OA_RN_NKCountryCode = country.RN_Code;

			return address;
		}

		#endregion

		#region Test New()

		public void TestNew()
		{
			Order order = Order.New(Factory);

			AssertNotNull("Order.New(Factory) returned null.", order);
			AssertEquals("Order.New(Factory) did not return a typeof(Order).", ExpectedOrderType, order.GetType());
		}

		#endregion

		#region Test Contacts Does Not Load Contacts If Filter Is Empty

		public void TestContactsDoesNotLoadContactsIfFilterIsEmpty()
		{
			Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			AssertNotNull("Precondition - ensure there are contacts in the DB.", Factory.LoadTop1(typeof(OrgContact), new ZQuery()));

			BO.BuyerPK = ZGuid.Empty;
			BO.SupplierPK = ZGuid.Empty;
			BO.JD_OH_SendingAgent = ZGuid.Empty;
			BO.JD_OH_ReceivingAgent = ZGuid.Empty;
			AssertEquals("There is no filter on contacts, should not load any contacts.", 0, BO.Contacts.Count);
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource

		public void TestParentWorkflowProviders()
		{
			Order order = Factory.NewWithValidTestData<Order>();

			IWorkflowTriggerFieldChangeSource fieldChangeSource = order;
			AssertEquals("No ParentWorkflowProviders initially", 0, fieldChangeSource.ParentWorkflowProviders.Count);

			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			AssertEquals("ParentWorkflowProviders with Preadvice attached", 1, fieldChangeSource.ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProviders with Preadvice attached", preadvice, fieldChangeSource.ParentWorkflowProviders[0]);
		}

		#endregion

		#region Controlling Customer Doc Address

		public void TestControllingCustomerDocAddress()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Constants.ContainerModes.FCL;

			AssertEquals(0, order.DocAddresses.Count);

			order.BuyerPK = consignor.PK;
			AssertEquals(2, order.DocAddresses.Count);

			order.BuyerPK = consignee.PK;
			AssertEquals(2, order.DocAddresses.Count);

			order.BuyerPK = ZGuid.Empty;
			consignor.AllRelatedParties.SetRelatedParty(consignee, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
			consignor.AllRelatedParties.Load();

			order.BuyerPK = consignor.PK;
			AssertEquals(3, order.DocAddresses.Count);
			AssertEquals(DocAddressTypes.Codes.ControllingCustomer, order.DocAddresses[2].E2_AddressType);
			AssertEquals("Order controlling customer is defaulted from 'controlling customer' of order's importer", consignee.MainAddress.PK, order.ControllingCustomerDocAddress.E2_OA_Address);

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CTL_CUSTOMER";

			var supplierBuyerLink = consignor.SupplierLinks.AddNew(consignee);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_OH_ControllingCustomer = controllingCustomer.PK;

			order.BuyerPK = ZGuid.Empty;
			consignor.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
			consignor.AllRelatedParties.Load();

			order.BuyerPK = consignor.PK;
			AssertEquals(4, order.DocAddresses.Count);
			AssertEquals(DocAddressTypes.Codes.ControllingCustomer, order.DocAddresses[2].E2_AddressType);
			AssertEquals("Order controlling customer is defaulted based on Supplier-Buyer Relation", controllingCustomer.MainAddress.PK, order.ControllingCustomerDocAddress.E2_OA_Address);
		}

		public void TestDefaultControllingCustomerOnCreateOrder()
		{
			var firstControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			firstControllingCustomer.OH_Code = "CTRL_CUS_1";
			var secondControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			secondControllingCustomer.OH_Code = "CTRL_CUS_2";

			var firstSupplier = Factory.NewWithValidTestData<OrgHeader>();
			firstSupplier.OH_Code = "SUPPLIER_1";
			var secondSupplier = Factory.NewWithValidTestData<OrgHeader>();
			secondSupplier.OH_Code = "SUPPLIER_2";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER_1";
			buyer.AllRelatedParties.SetRelatedParty(firstControllingCustomer, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
			buyer.AllRelatedParties.Load();

			var supplierBuyerLink = buyer.SupplierLinks.AddNew(firstSupplier);
			supplierBuyerLink.OL_OH_ControllingCustomer = secondControllingCustomer.PK;
			buyer.SupplierLinks.AddNew(secondSupplier);

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Constants.ContainerModes.FCL;

			order.BuyerPK = buyer.PK;
			order.JD_OA_BuyerAddress = buyer.Addresses[0].PK;

			order.SupplierPK = firstSupplier.PK;
			order.JD_OA_SupplierAddress = firstSupplier.Addresses[0].PK;

			AssertEquals("Controlling customer address doesn't match that of a standard doc address type", DocAddressTypes.Codes.ControllingCustomer, order.DocAddresses[0].E2_AddressType);
			AssertEquals("Order controlling customer is defaulted based on the Supplier-Buyer Relation", secondControllingCustomer.MainAddress.PK, order.ControllingCustomerDocAddress.E2_OA_Address);
		}

		public void TestDefaultControllingCustomerOnCreateOrderFromShipment()
		{
			var firstControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			firstControllingCustomer.OH_Code = "CTRL_CUS_1";
			var secondControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			secondControllingCustomer.OH_Code = "CTRL_CUS_2";

			var firstConsignor = Factory.NewWithValidTestData<OrgHeader>();
			firstConsignor.OH_Code = "CONSIGNOR_1";
			var secondConsignor = Factory.NewWithValidTestData<OrgHeader>();
			secondConsignor.OH_Code = "CONSIGNOR_2";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE_1";
			consignee.AllRelatedParties.SetRelatedParty(firstControllingCustomer, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
			consignee.AllRelatedParties.Load();

			var supplierBuyerLink = consignee.SupplierLinks.AddNew(firstConsignor);
			supplierBuyerLink.OL_OH_ControllingCustomer = secondControllingCustomer.PK;
			consignee.SupplierLinks.AddNew(secondConsignor);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = firstConsignor.PK;

			var order = shipment.AttachedOrders.AddNew();

			AssertNotNull("Order should have a controlling customer set", order.DocAddresses[0].E2_AddressType);
			AssertEquals("Order's controlling customer has defaulted based on the Supplier-Buyer Relation", secondControllingCustomer.MainAddress.PK, order.ControllingCustomerDocAddress.E2_OA_Address);
		}

		public void TestOrderWithRelatedNotes_ControllingCustomer()
		{
			var order = Factory.New<Order>();
			var controllingCustomer = Factory.New<OrgHeader>();

			var controllingCustomerAddress = order.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ControllingCustomer);
			controllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			StmNote testNote = controllingCustomer.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Break everything");

			AssertEquals(true, order.Notes.VisibleNotes.Contains(testNote));
		}

		#endregion

		#region TestOrganisationsFindBoxListDefaultFromUnmatchOrgNotes

		public void TestOrganisationsFindBoxListDefaultFromUnmatchOrgNotes()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var helper = new UnmatchOrgRecordTestHelper(order, BuyerRec, SupplierRec);

			//buyer
			helper.PopulateOrgDefaultsFromUnmatchedNote(order.GetType(), "BuyerList", order.BuyerList);
			AssertEquals("Consignor_list should have defaults from unmatchorgnotes", true, order.BuyerList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(order.BuyerList.DefaultsForNewChild, BuyerRec);

			//supplier
			helper.PopulateOrgDefaultsFromUnmatchedNote(order.GetType(), "SupplierList", order.SupplierList);
			AssertEquals("Consignor_list should have defaults from unmatchorgnotes", true, order.SupplierList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(order.SupplierList.DefaultsForNewChild, SupplierRec);
		}

		public void TestGetBuyerFromUnmatchedOrgNote()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var helper = new UnmatchOrgRecordTestHelper(order, BuyerRec, SupplierRec);

			var businessObjectCollection = (IBusinessObjectCollection)order.BuyerList;
			businessObjectCollection.Parent = order;
			businessObjectCollection.ListPropertyDescriptor = new PropertyDescriptorForTest("Dummy", Array.Empty<Attribute>());

			var buyerFindBoxListProvider = new OrganisationsFindBoxListProvider(order.BuyerList);
			buyerFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter("UNMATCHED");

			helper.AssertOrgFieldDefaults(order.BuyerList.DefaultsForNewChild, BuyerRec);
			AssertEquals("Consignee", OrganisationTypes.Consignee, order.BuyerList.OrganisationType);
		}

		public void TestGetSupplierFromUnmatchedOrgNote()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var helper = new UnmatchOrgRecordTestHelper(order, BuyerRec, SupplierRec);

			var businessObjectCollection = (IBusinessObjectCollection)order.SupplierList;
			businessObjectCollection.Parent = order;
			businessObjectCollection.ListPropertyDescriptor = new PropertyDescriptorForTest("Dummy", Array.Empty<Attribute>());

			var buyerFindBoxListProvider = new OrganisationsFindBoxListProvider(order.SupplierList);
			buyerFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter("UNMATCHED");

			helper.AssertOrgFieldDefaults(order.SupplierList.DefaultsForNewChild, SupplierRec);
			AssertEquals("Consignor", OrganisationTypes.Consignor, order.SupplierList.OrganisationType);
		}

		#endregion

		#region ProductQuantitySummary

		public void TestProductQuantitySummary()
		{
			var order = Factory.New<Order>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			order.SupplierPK = org.PK;

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "111222";
			part1.OP_Desc = "desc";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "111333";
			part2.OP_Desc = "desc 2";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_Quantity = 3.00M;
			orderLine1.JO_QtyInvoiced = 2.00M;
			orderLine1.JO_QtyReceived = 1.00M;
			orderLine1.JO_Partno = part1.OP_PartNum;

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_Quantity = 4.50M;
			orderLine2.JO_QtyInvoiced = 3.50M;
			orderLine2.JO_QtyReceived = 1.50M;
			orderLine2.JO_Partno = part1.OP_PartNum;

			var orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_Quantity = 5M;
			orderLine3.JO_QtyInvoiced = 4M;
			orderLine3.JO_QtyReceived = 3M;
			orderLine3.JO_Partno = part2.OP_PartNum;

			var totals = order.ProductQuantitySummary;
			AssertEquals(2, totals.Count);
			foreach (OrderLinesTotalByProduct total in totals)
			{
				if (total.Product == "111222")
				{
					AssertEquals("desc", total.ProductDescription);
					AssertEquals(7.5M, total.Quantity);
					AssertEquals(5.5M, total.QuantityInvoiced);
					AssertEquals(2.5M, total.QuantityReceived);
					AssertEquals(5.0M, total.QuantityRemaining);
				}
				else if (total.Product == "111333")
				{
					AssertEquals("desc 2", total.ProductDescription);
					AssertEquals(5M, total.Quantity);
					AssertEquals(4M, total.QuantityInvoiced);
					AssertEquals(3M, total.QuantityReceived);
					AssertEquals(2M, total.QuantityRemaining);
				}
				else
				{
					Fail("Incorrect Product Code");
				}
			}
		}

		#endregion

		#region GetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			var company1 = Factory.New<GlbCompany>();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = Factory.New<GlbCompany>();
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company2.PK;

			company1.GC_OH_OrgProxy = LocalConsignor.PK;
			company2.GC_OH_OrgProxy = OverseasConsignee.PK;

			var order = Factory.New<Order>();
			order.JD_OH_SendingAgent = LocalConsignor.PK;
			order.JD_OH_ReceivingAgent = OverseasConsignee.PK;
			order.JD_RL_NKPortOfLoading = "UAIEV";
			order.JD_RL_NKPortOfDischarge = "AUSYD";

			var workflowInformationProvider = (order as IWorkflowProvider).GetWorkflowInformationProvider();

			AssertEquals("Origin", "Kiev", workflowInformationProvider.Origin);
			AssertEquals("Destination", "Sydney", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Order, workflowInformationProvider.BusinessContext);
			AssertContainsExactElementsInAnyOrder("Companies", new[] { company1.PK, company2.PK }, workflowInformationProvider.Companies);
		}

		#endregion

		public void TestMilestonesView()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			MilestoneCollectionView milestones = order.MilestoneView;
			AssertEquals(false, milestones.AllowNew);
			AssertEquals(false, milestones.AllowRemove);

			milestones.AddNew();
			milestones.AddNew();
			SecurityCheckpoint milestoneCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.OrderTracking, SecurityCore.WorkflowMilestonesAutoGeneratedCode);
			milestoneCheckpoint.IsAllowed = true;
			milestones.Rebuild();
			Assert("Milestones should be enabled", milestones.All(m => !m.ReadOnly));

			milestoneCheckpoint.IsAllowed = false;
			milestones.Rebuild();
			Assert("Milestones should be read only", milestones.All(m => m.ReadOnly));
		}

		public void TestMilestonesView_MilestoneSequence()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			MilestoneCollectionView milestones = order.MilestoneView;

			var milestone1 = milestones.AddNew();
			milestone1.P9_Sequence = 3;
			var milestone2 = milestones.AddNew();
			milestone2.P9_Sequence = 2;
			var milestone3 = milestones.AddNew();
			milestone3.P9_Sequence = 1;

			SecurityCheckpoint milestoneCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.OrderTracking, SecurityCore.WorkflowMilestonesAutoGeneratedCode);
			milestones.Rebuild();
			AssertEquals(3, milestones.Count);
			AssertEquals(milestone3, milestones[0]);
			AssertEquals(milestone2, milestones[1]);
			AssertEquals(milestone1, milestones[2]);
		}

		public void TestProcessTasksCreatedAfterClientAndTransportModeAreSet()
		{
			ZDBOnlyQuery orderMilestonesQuery = new ZDBOnlyQuery(typeof(ProcessTask));
			orderMilestonesQuery.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, "P0");
			orderMilestonesQuery.AddToFilter(ProcessTasksSchema.P9_Type, "MIL");
			ZDBOnlySubQuery orderWorkflowQuery = new ZDBOnlySubQuery(typeof(ProcessTaskTemplate), ProcessTaskTemplateSchema.PK);
			orderWorkflowQuery.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.OrderWorkflowDescriptorCode);
			orderMilestonesQuery.AddSubQuery(ProcessTasksSchema.P9_ParentID, orderWorkflowQuery, JoinCondition.And);

			Assert("Prerequisite - order workflow with milestones found", Factory.Load<ProcessTask>(orderMilestonesQuery).Length > 0);

			GlbDepartment.CurrentDepartment.GE_Sea = GlbDepartment.CurrentDepartment.GE_Air = GlbDepartment.CurrentDepartment.GE_Rail = GlbDepartment.CurrentDepartment.GE_Road = false;

			Order order = Factory.New<Order>();
			AssertEquals("Prerequisite", string.Empty, order.JD_TransportMode);
			AssertEquals(0, order.MilestoneView.Count);

			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(0, order.MilestoneView.Count);

			order.JD_TransportMode = "XXX";
			AssertEquals(0, order.MilestoneView.Count);

			((ISupportDataImporting)order).IsImportingData = true;
			order.JD_TransportMode = "SEA";
			AssertEquals(0, order.MilestoneView.Count);

			((ISupportDataImporting)order).IsImportingData = false;
			order.JD_TransportMode = "AIR";
			AssertNotEquals(0, order.MilestoneView.Count);

			GlbDepartment.CurrentDepartment.GE_Sea = true;

			order = Factory.New<Order>();
			AssertEquals("Prerequisite", Core.Constants.TransportModes.Sea, order.JD_TransportMode);

			order.BuyerPK = ZGuid.Invalid;
			AssertEquals(0, order.MilestoneView.Count);

			((ISupportDataImporting)order).IsImportingData = true;
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(0, order.MilestoneView.Count);

			((ISupportDataImporting)order).IsImportingData = false;
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotEquals(0, order.MilestoneView.Count);
		}

		#region Buyer/Supplier Relationship Link

		public void TestBuyerSupplier()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "S1010";

			OrgHeader newConsignor = Factory.New<OrgHeader>();
			newConsignor.OH_Code = "*CNOR*";
			newConsignor.OH_FullName = "Test Consignor";
			newConsignor.MainAddress.OA_Address1 = "Consignor Address";
			newConsignor.OH_IsConsignor = true;
			OrgHeader newConsignee = Factory.New<OrgHeader>();
			newConsignee.OH_Code = "*CNEE*";
			newConsignee.OH_FullName = "Test Consignee";
			newConsignee.MainAddress.OA_Address1 = "Consignee Address";
			newConsignee.OH_IsConsignee = true;
			order.BuyerPK = newConsignee.PK;
			order.SupplierPK = newConsignor.PK;

			OrgHeader noConsignor = Factory.New<OrgHeader>();
			noConsignor.OH_Code = "*NOCNOR*";
			noConsignor.OH_FullName = "NoLink CNOR";
			noConsignor.MainAddress.OA_Address1 = "Consignor Address";
			noConsignor.OH_IsConsignor = true;
			OrgHeader noConsignee = Factory.New<OrgHeader>();
			noConsignee.OH_Code = "*NOCNEE*";
			noConsignee.OH_FullName = "NoLink CNEE";
			noConsignee.MainAddress.OA_Address1 = "Consignor Address";
			noConsignee.OH_IsConsignee = true;

			AssertEquals("Should prompt to save", true, order.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			order.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();

			Factory.Save();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, newConsignee.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, newConsignor.PK);
			BusinessObject[] links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("Supplier Buyer Link should be saved.", 1, links.Length);

			OrgSupplierBuyerLink link = links[0] as OrgSupplierBuyerLink;
			AssertEquals("Buyer supplier link - buyer", newConsignee.PK, link.Buyer.PK);
			AssertEquals("Buyer supplier link - supplier", newConsignor.PK, link.Supplier.PK);

			filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, noConsignee.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, noConsignor.PK);
			links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("Supplier Buyer Link should not be saved.", 0, links.Length);
		}

		#endregion

		#region JS_ActualVolumeImportFromOrder

		public void TestJS_ActualVolumeImportFromOrder()
		{
			var cne = Factory.NewWithValidTestData<OrgHeader>();
			cne.OH_Code = "TESTCNE";

			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			cnr.OH_Code = "TESTCNR";

			var order = Factory.New<Order>();
			order.BuyerPK = cne.PK;
			order.SupplierPK = cnr.PK;
			order.JD_ActualVolume = 10m;
			order.JD_ActualWeight = 1.2m;
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			order.JD_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var shipmentOne = Factory.New<ForwardingShipment>();
			shipmentOne.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentOne.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentOne.ConsignorPK = cnr.PK;
			shipmentOne.ConsigneePK = cne.PK;
			order.JD_JS = shipmentOne.PK;
			shipmentOne.OnOrderAttached(order);

			AssertEquals(order.JD_ActualWeight, shipmentOne.JS_ActualWeight);
			AssertEquals(order.JD_ActualVolume, shipmentOne.JS_ActualVolume);

			var registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
			registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			registryEntry[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryEntry);

			Factory.Save();

			var shipmentTwo = Factory.New<ForwardingShipment>();
			shipmentTwo.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTwo.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentTwo.ConsignorPK = cnr.PK;
			shipmentTwo.ConsigneePK = cne.PK;
			order.JD_JS = shipmentTwo.PK;
			shipmentTwo.OnOrderAttached(order);

			AssertEquals(order.JD_ActualWeight, shipmentTwo.JS_ActualWeight);
			AssertEquals(order.JD_ActualVolume, shipmentTwo.JS_ActualVolume);
		}

		#endregion

		#region ShipmentEditableRegistration

		public void TestShipmentEditableRegistration()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);

			var order = Factory.New<Order>();
			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;

			AssertEquals(true, order.IsRegisteredEditableChildObject(order.Shipment));
		}

		#endregion

		#region Workflow Template Applying

		public void TestWorkflowTemplateTriggerApplyingAfterCustomFieldChanged()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_IsConsignee = true;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = Core.Constants.DocManagerCodes.Order;
			template.P0_IsActive = true;

			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "OrderHeaderField";
			customField1.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var trigger1 = template.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Custom Field Condition";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger1.TemplateConditions.TemplateCondition2Value = "\"<GetCustomField(OrderHeaderField)>\"==\"Y\"";

			Factory.Save();

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			Factory.Save();

			var triggers = order.WorkflowItems.Triggers.Cast<ProcessTask>().ToArray();
			AssertEquals(0, triggers.Length);

			var newFactory = new BusinessObjectFactory();
			var newOrder = newFactory.Load<Order>(order.PK);
			var customBo = ((ICustomFieldProvider)newOrder).GetCustomBusinessObject();
			customBo["__ORDERHEADERFIELD__prop__ZBool"] = true;

			newFactory.Save();

			triggers = order.WorkflowItems.Triggers.Cast<ProcessTask>().ToArray();
			AssertEquals(1, triggers.Length);
			AssertEquals("Custom Field Condition", triggers[0].P9_Description);
		}

		#endregion

		#region GoodsAvailableAtAddress

		public void TestGoodsAvailableAtAddress()
		{
			var header = Factory.New<OrgHeader>();
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var officeAddress = header.Addresses.AddNew();
			officeAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			officeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
			officeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			officeAddress.OA_Address1 = "Address1";

			var pickupAddress2 = header.Addresses.AddNew();
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			pickupAddress2.OA_RL_NKRelatedPortCode = "AUBNE";
			pickupAddress2.OA_Address1 = "Address2";

			var order = Factory.New<Order>();
			order.GoodsAvailableAtAddress.OrganisationPK = header.PK;
			AssertEquals("Address2", order.GoodsAvailableAtAddress.E2_Address1);
		}

		public void TestDefaultGoodsAvailableAtAddress()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			AssertEquals(0, order.DocAddresses.Count);

			order.SupplierPK = supplier.PK;
			var goodsAvailableAtAddress = order.DocAddresses.FindByDocAddressType(DocAddressType.GoodsAvailableAt);
			AssertNotNull(goodsAvailableAtAddress);
			AssertEquals(order.GoodsAvailableAtAddress.PK, goodsAvailableAtAddress.PK);
		}

		#endregion

		#region GoodsDeliveredToAddress

		public void TestGoodsDeliveredToAddress()
		{
			var header = Factory.New<OrgHeader>();
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var officeAddress = header.Addresses.AddNew();
			officeAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			officeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
			officeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			officeAddress.OA_Address1 = "Address1";

			var deliveryAddress2 = header.Addresses.AddNew();
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress2.OA_RL_NKRelatedPortCode = "AUBNE";
			deliveryAddress2.OA_Address1 = "Address2";

			var order = Factory.New<Order>();
			order.GoodsDeliveredToAddress.OrganisationPK = header.PK;
			AssertEquals("Address2", order.GoodsDeliveredToAddress.E2_Address1);
		}

		public void TestDefaultGoodsDeliveredToAddress()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			AssertEquals(0, order.DocAddresses.Count);

			order.BuyerPK = buyer.PK;
			var goodsDeliverToAddress = order.DocAddresses.FindByDocAddressType(DocAddressType.GoodsDeliveredTo);
			AssertNotNull(goodsDeliverToAddress);
			AssertEquals(order.GoodsDeliveredToAddress.PK, goodsDeliverToAddress.PK);

			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var deliverAddress = buyer2.Addresses.AddNew();
			deliverAddress.FillWithValidTestData();
			deliverAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			order.BuyerPK = buyer2.PK;
			AssertEquals("should default to deliver address", order.GoodsDeliveredToAddress.E2_OA_Address, deliverAddress.PK);
		}

		#endregion

		#region OrderEConversations

		public void TestOrderEConversations()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var conversationProvider = order as IConversationProvider;
			Factory.Save();

			var conversation = conversationProvider.eConversation;
			var expectedConversation = JobConversation.GetConversation(order);

			AssertEquals(expectedConversation.PK, conversation.PK);
			AssertEquals(ModuleIDs.Orders, conversationProvider.ParentModule);
			AssertEquals(ControllerIDs.Orders, conversationProvider.ParentController);
			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), conversationProvider.AdditionalParticipants);
			Assert(conversationProvider.SendEmailNotificationsOnSave);
			AssertNull(conversationProvider.EmailSubjectContentOverride);
			AssertNull(conversationProvider.FromAddressOverride);
			AssertNoExceptionThrown(() => conversationProvider.RunConversationUpdateActionBeforeSaving());
		}

		#endregion

		#region CheckPortConsistentWithLinkedSupplierBooking

		const string OriginEventReference = "TYP=Origin|RES=Origin is different to the Origin of at least one of the Orders.";
		const string DestinationEventReference = "TYP=Destination|RES=Destination is different to the Destination of at least one of the Orders.";

		public void TestCheckPortConsistentWithLinkedSupplierBooking_BookingStatus()
		{
			var bookingStatuses = typeof(Constants.SupplierBookingStatus).GetFields();
			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				var isActiveBooking = !(bookingStatus == Constants.SupplierBookingStatus.Cancelled || bookingStatus == Constants.SupplierBookingStatus.Converted);
				CheckPortConsistentWithLinkedSupplierBooking("CNSHA", "NZNAL", bookingStatus, isActiveBooking, isActiveBooking);
			}
		}

		public void TestCheckPortConsistentWithLinkedSupplierBooking_Update()
		{
			foreach (var origin in new string[] { "SGSIN", "CNSHA" })
			{
				foreach (var destination in new string[] { "AUSYD", "NZNAL" })
				{
					CheckPortConsistentWithLinkedSupplierBooking(origin, destination, Constants.SupplierBookingStatus.Incomplete, origin != "SGSIN", destination != "AUSYD");
				}
			}
		}

		void CheckPortConsistentWithLinkedSupplierBooking(string origin, string destination, string bookingStatus, bool expectOriginEventReference, bool expectDestinationEventReference)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKGoodsAvailableAt = "SGSIN";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			var orderLine = order.OrderLines.AddNew();

			var booking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking1.JSB_Status = bookingStatus;
			booking1.JSB_RL_NKOrigin = "SGSIN";
			booking1.JSB_RL_NKDestination = "AUSYD";
			var bookingLine1 = booking1.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine.PK;

			Factory.Save();
			Assert(!booking1.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_Reference == OriginEventReference || log.SL_Reference == DestinationEventReference));

			order.JD_OrderGoodsDescription = "xxxx";
			Factory.Save();
			Assert(!booking1.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_Reference == OriginEventReference || log.SL_Reference == DestinationEventReference));

			order.JD_RL_NKGoodsAvailableAt = origin;
			order.JD_RL_NKGoodsDeliveredTo = destination;
			Factory.Save();

			AssertEquals(booking1.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_Reference == OriginEventReference), expectOriginEventReference);
			AssertEquals(booking1.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_Reference == DestinationEventReference), expectDestinationEventReference);
		}

		#endregion

		public void TestLinkedSupplierBookings()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			var orderLine2 = order.OrderLines.AddNew();
			var supplierBooking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking1.SupplierBookingLines.AddNew().JSL_JO_OrderLine = orderLine1.PK;
			var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking2.SupplierBookingLines.AddNew().JSL_JO_OrderLine = orderLine2.PK;
			Factory.Save();

			AssertEquals(2, order.LinkedSupplierBookings.Count());
			AssertNotNull(order.LinkedSupplierBookings.Single(supplierBooking => supplierBooking.PK == supplierBooking1.PK));
			AssertNotNull(order.LinkedSupplierBookings.Single(supplierBooking => supplierBooking.PK == supplierBooking2.PK));
		}

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "X125";

			AssertEquals("X125", order.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var order = Factory.NewWithValidTestData<Order>();
			AssertEquals(ExternalRequestTypes.Codes.Order, order.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;
			order.JD_OA_SupplierAddress = reviewerOrg.MainAddress.PK;
			order.JD_OC_SupplierContact = reviewerOrg.Contacts[0].PK;

			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(order, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(order, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, order.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion

		#region Implementation

		protected virtual Type ExpectedOrderType
		{
			get { return typeof(Order); }
		}

		ForwardingConsol Consol
		{
			get { return fConsol ?? (fConsol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol fConsol;

		ForwardingShipment Shipment
		{
			get { return fShipment ?? (fShipment = Consol.Shipments.AddNew()); }
		}
		ForwardingShipment fShipment;

		BusinessObject Declaration
		{
			get { return fDeclaration ?? (fDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>()); }
		}
		BusinessObject fDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			BO = Factory.NewWithValidTestData<Order>();
			BO.SupplierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			BO.Supplier.OH_RL_NKClosestPort = "AUSYD";
			BO.Buyer.OH_RL_NKClosestPort = "AUMEL";
		}

		protected virtual Order NewOrder()
		{
			OrgHeader buyer = CreateNewOrg(Factory, "TBY");
			Order result = Factory.New<Order>();
			result.BuyerPK = buyer.PK;
			return result;
		}

		protected OrgHeader CreateNewOrg(BusinessObjectFactory factory, ZString code)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_FullName = "some full name";
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "some place";
			return result;
		}

		OrgHeader GetAOrg()
		{
			return BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		UnmatchOrgRecord BuyerRec
		{
			get
			{
				if (buyerRec == null)
				{
					buyerRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"buyer addr 1",
						"buyer addr 2",
						"buyerName",
						"2222",
						"NSW",
						"sydney",
						"buyer",
						"buyerOwnerCode",
						"");
				}
				return buyerRec;
			}
		}
		UnmatchOrgRecord buyerRec;

		UnmatchOrgRecord SupplierRec
		{
			get
			{
				if (supplierRec == null)
				{
					supplierRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"supplier addr 1",
						"supplier addr 2",
						"supplierName",
						"1111",
						"NSW",
						"city",
						"supplier",
						"SupownerCode",
						"");
				}
				return supplierRec;
			}
		}
		UnmatchOrgRecord supplierRec;

		void OnFactorySaving(Order order)
		{
			typeof(Order).GetMethod("OnFactorySaving", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(order, null);
		}

		Order BO;

		void AssertCanDeleteAddress(string assertMessage, DocAddressType docAddressType, ZString supplierBookingStatus, bool expectedOutput, bool attachOrderLine)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();

			var docAddress = order.DocAddresses.AddNew();
			docAddress.DocAddressType = docAddressType;

			if (attachOrderLine)
			{
				var jobSupplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var jobSupplierBookingLine = jobSupplierBooking.SupplierBookingLines.AddNew();

				jobSupplierBooking.JSB_Status = supplierBookingStatus;
				jobSupplierBookingLine.JSL_JO_OrderLine = orderLine.PK;
			}

			Factory.Save();

			var docAddresses = order as IDocAddresses;
			AssertEquals(assertMessage, expectedOutput, docAddresses.CanDeleteAddress(docAddress));
		}

		#endregion
	}
}
