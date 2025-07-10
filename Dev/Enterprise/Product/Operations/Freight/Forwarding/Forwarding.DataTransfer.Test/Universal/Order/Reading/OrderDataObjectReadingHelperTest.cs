using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderDataObjectReadingHelperTest : ShipmentDataObjectReadingHelperTest
	{
		public void TestInvalidParentType()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);
			var parent = new Business.Testing.ForwardingDocsAndCartageValidationTest.MockJobDocsAndCartageParentForTesting(Factory.BOFactory);
			AssertExceptionThrown<InvalidOperationException>("Invalid Parent", "parent must be either Shipment or Declaration.", () => new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, parent));
		}

		public void TestShipmentOrdersLimitExceeded()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order1"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order2"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order3"));

			var shipmentBO = Factory.New<ForwardingShipment>();
			var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, shipmentBO);

			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), "The number of Orders on a Shipment is limited for performance and database management reasons to 2 Orders.", () => readerHelper.PopulateOrders(), true);
			}
		}

		public void TestDeclarationOrdersLimitExceeded()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order1"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order2"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order3"));

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, (IAttachOrders)declaration);

			using (CustomsDataRegistry.Instance.OrdersPerDeclarationLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (CustomsDataRegistry.Instance.OrdersPerDeclarationLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), "The number of Orders on a Declaration is limited for performance and database management reasons to 2 Orders.", () => readerHelper.PopulateOrders(), true);
			}
		}

		public void TestShipmentOrdersAreImported()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var shipmentBO = Factory.New<ForwardingShipment>();
			var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, shipmentBO);
			readerHelper.PopulateOrders();

			AssertEquals(1, shipmentBO.AttachedOrders.Count);
			var order = shipmentBO.AttachedOrders[0];
			AssertEquals("ORDER ME", order.JD_OrderNumber);
			AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);

			AssertMultilineASCIIEquals("LogText", @"Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment."
				, Logger.Logs);
		}

		public void TestUseUnmatchedOrgIfOrderEnableIt_OldEngine()
		{
			var unmatchedOrgPK = SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUseUnmatchedOrgRegistry(false, false, true);

			var order = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = order.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);

			order.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			order.SetOrganizationAddressCollection(() => new List<UniversalDataBuss.DataObjects.Universal.OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Orders in the DB yet.", null, new BusinessObjectFactory().LoadTop1<Order>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(order);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Order  from UniversalShipment.
Successfully saved Order P000001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'.
No matching Order found, creating new Order.
Populating Order...
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'. No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: JACK SCHMIDT ENTERPRISES; Address 1: 643 JACK BOULEVARDE; City: SCHMIDTVILLE]'. No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Order  from UniversalShipment.
Successfully saved Order P000001.
".Trim(), logNoteText);

				var orders = new BusinessObjectFactory().Load<Order>(new ZQuery());
				AssertEquals("orders.Count()", 1, orders.Length);
				var orderBO = orders[0];
				AssertEquals("orderBO.JD_OrderGoodsDescription", "RAGING RABID ROGER RABBIT", orderBO.JD_OrderGoodsDescription);
			});
		}

		public void TestDoNotUseUnmatchedOrgIfOrderDisableIt_OldEngine()
		{
			var unmatchedOrgPK = SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUseUnmatchedOrgRegistry(true, false, false);

			var order = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = order.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);

			order.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			order.SetOrganizationAddressCollection(() => new List<UniversalDataBuss.DataObjects.Universal.OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Orders in the DB yet.", null, new BusinessObjectFactory().LoadTop1<Order>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(order);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot Import Order
Unable to match Buyer Address, please make sure the supplied Buyer Address is valid. Details were:
Address1: 153 PHIL BOULEVARDE
City: MCKRACKINVILLE
CompanyName: PHIL MCKRACKIN ENTERPRISES
Country: AU
Email: PHIL@mckrackin.com.au
Fax: 0011 61 2 2153 3215
Phone: 0011 61 2 2153 5321
Port: AUSYD
Postcode: 2153
State: NSW
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'.
No matching Order found, creating new Order.
Populating Order...
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'.
Error - Cannot Import Order
Unable to match Buyer Address, please make sure the supplied Buyer Address is valid. Details were:
Address1: 153 PHIL BOULEVARDE
City: MCKRACKINVILLE
CompanyName: PHIL MCKRACKIN ENTERPRISES
Country: AU
Email: PHIL@mckrackin.com.au
Fax: 0011 61 2 2153 3215
Phone: 0011 61 2 2153 5321
Port: AUSYD
Postcode: 2153
State: NSW
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

				var orders = new BusinessObjectFactory().Load<Order>(new ZQuery());
				AssertEquals("orders.Count()", 0, orders.Length);
			});
		}

		public void TestUseUnmatchedOrgIfOrderEnableIt_NewEngine()
		{
			var unmatchedOrgPK = SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUseUnmatchedOrgRegistry(false, true, true);

			var order = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = order.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);

			order.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			order.SetOrganizationAddressCollection(() => new List<UniversalDataBuss.DataObjects.Universal.OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Orders in the DB yet.", null, new BusinessObjectFactory().LoadTop1<Order>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(order);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Order  from UniversalShipment.
Successfully saved Order P000001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: PHIL MCKRACKIN; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'
No matching Order found, creating new Order.
Populating Order...
Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: PHIL MCKRACKIN; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]' No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsignorDocumentaryAddress':- Unable to match address for '[Company Name: JACK SCHMIDT; Address 1: 643 JACK BOULEVARDE; City: SCHMIDTVILLE]' No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Order  from UniversalShipment.
Successfully saved Order P000001.
".Trim(), logNoteText);

				var orders = new BusinessObjectFactory().Load<Order>(new ZQuery());
				AssertEquals("orders.Count()", 1, orders.Length);
				var orderBO = orders[0];
				AssertEquals("orderBO.JD_OrderGoodsDescription", "RAGING RABID ROGER RABBIT", orderBO.JD_OrderGoodsDescription);
			});
		}

		public void TestDoNotUseUnmatchedOrgIfOrderDisableIt_NewEngine()
		{
			var unmatchedOrgPK = SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUseUnmatchedOrgRegistry(true, true, false);

			var order = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = order.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);

			order.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			order.SetOrganizationAddressCollection(() => new List<UniversalDataBuss.DataObjects.Universal.OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Orders in the DB yet.", null, new BusinessObjectFactory().LoadTop1<Order>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(order);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot Import Order
Unable to match Buyer Address, please make sure the supplied Buyer Address is valid. Details were:
Address1: 153 PHIL BOULEVARDE
City: MCKRACKINVILLE
CompanyName: PHIL MCKRACKIN ENTERPRISES
Country: AU
Email: PHIL@mckrackin.com.au
Fax: 0011 61 2 2153 3215
Phone: 0011 61 2 2153 5321
Port: AUSYD
Postcode: 2153
State: NSW
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: PHIL MCKRACKIN; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'
No matching Order found, creating new Order.
Populating Order...
Matching 'ConsigneeDocumentaryAddress':- Unable to match address for '[Company Name: PHIL MCKRACKIN; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'
Error - Cannot Import Order
Unable to match Buyer Address, please make sure the supplied Buyer Address is valid. Details were:
Address1: 153 PHIL BOULEVARDE
City: MCKRACKINVILLE
CompanyName: PHIL MCKRACKIN ENTERPRISES
Country: AU
Email: PHIL@mckrackin.com.au
Fax: 0011 61 2 2153 3215
Phone: 0011 61 2 2153 5321
Port: AUSYD
Postcode: 2153
State: NSW
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

				var orders = new BusinessObjectFactory().Load<Order>(new ZQuery());
				AssertEquals("orders.Count()", 0, orders.Length);
			});
		}

		public void TestSupplierBookingOrdersAreNotImportedWhileEnableAdvOrmFeature()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "SBK ORDER";
			order.JD_OrderNumberSplit = new ZByte(1);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER";
			order.BuyerPK = buyer.PK;

			var consigneeDocumentary = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDocumentary.Contacts.AddNew().FillWithValidTestData();
			consigneeDocumentary.OH_Code = "CNSGEE";
			order.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentary.PK;
			order.ConsigneeDocumentaryAddress.ContactPK = consigneeDocumentary.Contacts[0].PK;
			order.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentary.MainAddress.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderDataObject1 = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(UniversalDataBuss.Integration.RecipientRoleType.ORP, order))).GetDataObject(order);
			orderDataObject1.DataContext.AddDataTarget(UniversalDataBuss.Integration.DataContextType.OrderManagerOrder, orderDataObject1.DataContext.DataSourceCollection.First().Key);
			shipmentDataObject.RelatedShipmentCollection.Add(orderDataObject1);
			var orderDataObject2 = GetNewOrderDataObject();
			orderDataObject2.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderDataObject2);
			Factory.SaveForTesting();

			Logger.ClearLogs();
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, shipmentBO);
				readerHelper.PopulateOrders();

				AssertEquals(1, shipmentBO.AttachedOrders.Count);

				var attachedOrder = shipmentBO.AttachedOrders[0];
				AssertEquals("ORDER ME", attachedOrder.JD_OrderNumber);
				AssertEquals(new ZByte(2), attachedOrder.JD_OrderNumberSplit);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(true, Logger.HasWarnings);

				AssertMultilineASCIIEquals("LogText", @"Information - Successfully loaded matching Order.
Information - Matching 'BuyerDocumentaryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Populating Order...
Information - Matching 'BuyerDocumentaryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'GoodsDeliveredTo':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CNSGEE' by code, address '#1' (only address).
Information - Successfully loaded matching OrderLine.
Information - Populating OrderLine...
Information - Updated Order SBK ORDER-1 from UniversalShipment.
Warning - The order containing in this shipment XML has associated supplier bookings and cannot be linked via XML.
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment."
					, Logger.Logs);

				Factory.BOFactory.CleanUp();
			});
		}

		public void TestSupplierBookingOrdersAreNotImportedWhileDisableSupplierBooking()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "SBK ORDER";
			order.JD_OrderNumberSplit = new ZByte(1);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER";
			order.BuyerPK = buyer.PK;

			var consigneeDocumentary = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDocumentary.Contacts.AddNew().FillWithValidTestData();
			consigneeDocumentary.OH_Code = "CNSGEE";
			order.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentary.PK;
			order.ConsigneeDocumentaryAddress.ContactPK = consigneeDocumentary.Contacts[0].PK;
			order.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentary.MainAddress.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderDataObject1 = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(UniversalDataBuss.Integration.RecipientRoleType.ORP, order))).GetDataObject(order);
			orderDataObject1.DataContext.AddDataTarget(UniversalDataBuss.Integration.DataContextType.OrderManagerOrder, orderDataObject1.DataContext.DataSourceCollection.First().Key);
			shipmentDataObject.RelatedShipmentCollection.Add(orderDataObject1);
			var orderDataObject2 = GetNewOrderDataObject();
			orderDataObject2.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderDataObject2);
			Factory.SaveForTesting();

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, shipmentBO);
				readerHelper.PopulateOrders();

				AssertEquals(1, shipmentBO.AttachedOrders.Count);

				var attachedOrder = shipmentBO.AttachedOrders[0];
				AssertEquals("ORDER ME", attachedOrder.JD_OrderNumber);
				AssertEquals(new ZByte(2), attachedOrder.JD_OrderNumberSplit);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(true, Logger.HasWarnings);

				AssertMultilineASCIIEquals(@"Information - Successfully loaded matching Order.
Information - Matching 'BuyerDocumentaryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Populating Order...
Information - Matching 'BuyerDocumentaryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'GoodsDeliveredTo':- Matched to 'BUYER' by code, address '#1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CNSGEE' by code, address '#1' (only address).
Warning - Unknown Address Type [ConsigneeDocumentaryAddress] found. Job Document Address not imported.
Information - Successfully loaded matching OrderLine.
Information - Populating OrderLine...
Information - Updated Order SBK ORDER-1 from UniversalShipment.
Warning - The order containing in this shipment XML has associated supplier bookings and cannot be linked via XML.
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment."
					, Logger.Logs);
			});
		}

		public void TestDeclarationOrdersAreImported()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			Logger.ClearLogs();
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var declarationBO = (IAttachOrders)Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, declarationBO);
				readerHelper.PopulateOrders();

				AssertEquals(1, declarationBO.AttachedOrders.Count);
				var order = declarationBO.AttachedOrders[0];
				AssertEquals("ORDER ME", order.JD_OrderNumber);
				AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);

				AssertMultilineASCIIEquals("LogText", @"Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment."
					, Logger.Logs);
			});

			Logger.ClearLogs();
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var declarationBO = (IAttachOrders)Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				var readerHelper = new OrderDataObjectReadingHelper(shipmentDataObject, Logger, Factory, declarationBO);
				readerHelper.PopulateOrders();

				AssertEquals(1, declarationBO.AttachedOrders.Count);
				var order = declarationBO.AttachedOrders[0];
				AssertEquals("ORDER ME", order.JD_OrderNumber);
				AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);

				AssertMultilineASCIIEquals("LogText", @"Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment."
					, Logger.Logs);
			});
		}

		#region Implementation

		void SetUseUnmatchedOrgRegistry(bool enabledUnmatchedOrg_ExceptOrder, bool useNewEngine, bool enabledUnmatchedOrg_Order)
		{
			var unmatchedOrgRegistryItem = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrgRegistryItem.IsEnabled = enabledUnmatchedOrg_ExceptOrder;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrgRegistryItem);
			OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, useNewEngine);
			var registryValue = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.JobTypeCodes.Order, (NoResString)"Order (Forwarding)", enabledUnmatchedOrg_Order },
			};
			OrganisationsDataRegistry.Instance.UnmatchedOrganisationConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}

		#endregion
	}
}
