using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingShipmentValueObjectDataAdapter))]
	public class ForwardingShipmentValueObjectDataAdapterTest : ShipmentValueObjectDataAdapterTest<ForwardingShipment, ForwardingConsol>
	{
		#region Import

		public void TestOrdersLimit()
		{
			const string expectedError = @"Error: The number of Orders on a Shipment is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			var xsdShipment = GetXsdShipmentWithOrder();
			AddOrder(xsdShipment, "TESTORDER2");
			AddOrder(xsdShipment, "TESTORDER3");

			var context = new ValueObjectImportContext(Factory, Notify);

			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);

				AssertCollectionContains(expectedError, Notify.GetEventsByType(ErrorType.DataErrorPreventSave).Select(x => x.Message));
			}
		}

		void AddOrder(Xsd.Shipment xsdShipment, string orderNumber)
		{
			var xsdOrder = xsdShipment.Orders.AddNew();
			xsdOrder.OrderIdentifier.OrderNumber = orderNumber;
			var orderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail = orderDetail;
		}

		public void TestAddImportEventForSACShipment()
		{
			Xsd.Shipment xsdShipment = GetXsdShipmentWithOrder();
			xsdShipment.ShipmentDetails.DeclarationStyle = "";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);

			ForwardingShipment shipment = Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertNull("shipment doesn't have any dim event with 'sac' reference", shipment.Logs.MostRecentLogByEventTime(Events.DataImport, "AU Declaration Style: SAC"));

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Shipments.Add(shipment);

			xsdShipment.ShipmentDetails.DeclarationStyle = "SAC";

			Notify.Clear();
			ForwardingShipmentValueObjectDataAdapter dataAdapter = new ForwardingShipmentValueObjectDataAdapter(consol);
			shipment = dataAdapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertNotNull("shipment should have dim event with 'sac' reference", shipment.Logs.MostRecentLogByEventTime(Events.DataImport, "AU Declaration Style: SAC"));

			Factory.Save();
		}

		public void TestImportOrdersFromShipment()
		{
			var filter = new ZDBOnlyQuery(typeof(Order));
			filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, "TESTORDER");

			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, Consignee.PK);

			var loadedOrder = Factory.LoadTop1<Order>(filter);
			AssertNull("Precondition", loadedOrder);

			Xsd.Shipment xsdShipment = GetXsdShipmentWithOrder();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			loadedOrder = Factory.LoadTop1<Order>(filter);
			AssertNotNull("Order should not be null", loadedOrder);

			AssertEquals("Order should be attached to a shipment", true, loadedOrder.IsShipmentAttached);

			var shipmentWithOrder = loadedOrder.Shipment;
			AssertNotNull("Shipment should not be null:", shipmentWithOrder);
			AssertEquals("Shipment's house bill:", "HOUSEBILL", shipmentWithOrder.JS_HouseBill);
			Assert("Notify:", Notify.AsString.IndexOf("Order TESTORDER has been attached to shipment.") > -1);
		}

		public void TestImportingShipmentDoesNotUpdateAlreadyAttachedOrders()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "housebill";
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			Order order1 = Factory.New<Order>();
			order1.BuyerPK = Consignee.PK;
			order1.SupplierPK = Consignor.PK;
			order1.JD_OrderNumber = "TESTORDER";
			order1.JD_BookingConfRef = "Confirmation1";
			order1.JD_JS = shipment.PK;
			OrderLine orderLine = order1.OrderLines.AddNew();
			orderLine.JO_Partno = "BOOKS";

			Factory.Save();

			Xsd.Shipment xsdShipment = GetXsdShipmentWithOrder();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);

			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertEquals("Order:", false, order1.HasChanges);
			AssertEquals("Order's booking confirmation should not have changed:", "Confirmation1", order1.JD_BookingConfRef);
			AssertNotifyOnImport();

			Factory.Save();
		}

		protected virtual void AssertNotifyOnImport()
		{
			Assert("Notify", Notify.AsString.IndexOf("Order TESTORDER is already attached and cannot be updated.") > -1);
		}

		public void TestOrdersWithReferenceNotImported()
		{
			int ordersCount = Factory.GetDatabaseCount(typeof(Order));

			Xsd.Shipment xsdShipment = GetXsdShipmentWithOrder();
			xsdShipment.Orders[0].OrderDetail.ReferenceNumber.Value = "Reference";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			AssertEquals("No order should have been created", ordersCount, Factory.GetDatabaseCount(typeof(Order)));
			AssertEquals("Notify should have errors:", true, Notify.HasErrors);
			AssertEquals("Notify shows:", true, Notify.AsString.IndexOf("Order TESTORDER: Reference Number should only be provided when importing a stand-alone order.") > -1);
		}

		#endregion

		#region Export

		public void TestExportOrders()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "Order";
			order1.JD_JS = shipment.PK;
			order1.BuyerPK = shipment.ConsignorPK;
			Factory.Save();

			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			Adapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Should have one Order", 1, xsdShipment.Orders.Count);
		}

		public void TestExportOnlyRelevantOrderWhenSpecified()
		{
			ForwardingShipment shipment = GetNewShipmentWithConsignor();
			Order order1 = AddNewOrderToShipment(shipment, "ORDER1");
			Order order2 = AddNewOrderToShipment(shipment, "ORDER2");
			Order order3 = AddNewOrderToShipment(shipment, "ORDER3");
			Factory.Save();

			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			Adapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("OrderToExport NOT specified => There should be 3 Orders", 3, xsdShipment.Orders.Count);

			xsdShipment = new Xsd.Shipment();
			ForwardingShipmentValueObjectDataAdapter singleOrderAdapter = new ForwardingShipmentValueObjectDataAdapter(order2);
			singleOrderAdapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("OrderToExport specified => There should be only 1 Order", 1, xsdShipment.Orders.Count);
			AssertEquals("Exported Order ReferenceNumber", order2.JD_OrderNumber, xsdShipment.Orders[0].OrderIdentifier.OrderNumber);
		}

		public void TestExportPreAdviceIdentifier()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			preadvice.EF_JS = shipment.PK;

			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Shipment));
			MemoryStream stream = new MemoryStream();
			serializer.ExportXmlData(stream, Adapter, new BusinessObject[] { shipment }, new ValueObjectExportContext(new NotificationBuffer()));
			stream.Position = 0;
			string xml = new StreamReader(stream).ReadToEnd();
			AssertEquals("Pre-advice identifier included in the xml", true, xml.Contains("<ShipmentIdentifier ShipmentIdentifierType=\"PreadviceIdentifier\" />"));
		}

		public void TestExportARInvoice()
		{
			ForwardingShipment shipment = GetShipmentWithInvoice();
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			ForwardingShipmentValueObjectDataAdapter dataAdapter = new ForwardingShipmentValueObjectDataAdapter();
			NotificationBuffer buffer = new NotificationBuffer();

			AssertEquals("the system registry is set to false", false, SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.Value);
			dataAdapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(buffer));
			AssertEquals("xsdShipment 's ARInvoices is specified", false, xsdShipment.ARInvoices.IsSpecified);

			buffer.Clear();

			SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("the system registry is set to true", true, SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.Value);
			xsdShipment = new Xsd.Shipment();
			dataAdapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(buffer));
			AssertEquals("xsdShipment 's ARInvoices is specified", true, xsdShipment.ARInvoices.IsSpecified);
			AssertEquals("no of arinvoices attached", 1, xsdShipment.ARInvoices.Count);

			buffer.Clear();
			shipment = GetNewShipmentWithConsignor();
			dataAdapter.ExportToValueObject(shipment, xsdShipment, new ValueObjectExportContext(buffer));
			AssertEquals("xsdShipment 's ARInvoices is specified", false, xsdShipment.ARInvoices.IsSpecified);
		}

		public void TestExportAWBHeaderOnlyWorkForExportAirShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			ForwardingShipment shipmentAirExport = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentAirExport.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentAirExport.JS_RL_NKOrigin = "AUSYD";
			shipmentAirExport.JS_RL_NKDestination = "USLAX";
			shipmentAirExport.PopulateAWB();

			ForwardingShipment shipmentSeaExport = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentSeaExport.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentSeaExport.JS_RL_NKOrigin = "AUSYD";
			shipmentSeaExport.JS_RL_NKDestination = "USLAX";
			shipmentSeaExport.PopulateAWB();

			ForwardingShipment shipmentAirImport = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentAirImport.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentAirImport.JS_RL_NKOrigin = "NZAKL";
			shipmentAirImport.JS_RL_NKDestination = "AUSYD";
			shipmentAirImport.PopulateAWB();

			Factory.Save();

			ProcessTaskNotification task = Factory.New<ProcessTaskNotification>();
			task.PQ_P9 = Factory.New<ProcessTask>().PK;
			task.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB;
			EventsWithSourceType triggeredByEvents = new EventsWithSourceType(EventsWithSourceType.SourceType.Shipment, task, null);

			VerifyExportAWBHeader(shipmentAirExport, triggeredByEvents, true);
			VerifyExportAWBHeader(shipmentSeaExport, triggeredByEvents, false);
			VerifyExportAWBHeader(shipmentAirImport, triggeredByEvents, true);

			task.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			VerifyExportAWBHeader(shipmentAirExport, triggeredByEvents, false);
		}

		void VerifyExportAWBHeader(ForwardingShipment shipment, EventsWithSourceType triggeredByEvents, bool expectAWBHeadersExported)
		{
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			ForwardingShipmentValueObjectDataAdapter dataAdapter = new ForwardingShipmentValueObjectDataAdapter(triggeredByEvents);
			var context = new ValueObjectExportContext(new NotificationBuffer());
			dataAdapter.ExportToValueObject(shipment, xsdShipment, context);
			AssertEquals("xsdShipment 's AWBHeader is specified", expectAWBHeadersExported, xsdShipment.AWBHeaders.IsSpecified);
		}

		ForwardingShipment GetShipmentWithInvoice()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			JobHeader jobheader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobheader.JH_ParentID = shipment.PK;
			jobheader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			BusinessObject aRInvoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoice)));
			aRInvoice[AccTransactionHeaderSchema.AH_JH] = jobheader.PK;
			aRInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = shipment.JobNumber;

			BusinessObject aRInvoiceLine = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoiceLine)));
			aRInvoiceLine[AccTransactionLinesSchema.AL_AH] = aRInvoice.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_JH] = jobheader.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_AG] = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			charge.JR_JH = jobheader.PK;

			Factory.Save();
			return shipment;
		}

		ForwardingShipment GetNewShipmentWithConsignor()
		{
			ForwardingShipment newShipment = Factory.New<ForwardingShipment>();
			newShipment.ConsignorPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			return newShipment;
		}

		Order AddNewOrderToShipment(ForwardingShipment shipment, string newOrderNumber)
		{
			Order newOrder = Factory.New<Order>();
			newOrder.JD_OrderNumber = newOrderNumber;
			newOrder.JD_JS = shipment.PK;
			newOrder.BuyerPK = shipment.ConsignorPK;
			return newOrder;
		}

		#endregion

		#region Implementation

		protected override string FullyPopulatedAirSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.AirShipmentForwarding.xml";

		protected override string FullyPopulatedSeaSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.SeaShipmentForwarding.xml";

		protected override string FullyPopulatedRailSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.RailShipmentForwarding.xml";

		protected override string FullyPopulatedEmptyShipmentSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedShipmentWithEmptyFieldsForwarding.xml";

		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter()
		{
			return new ForwardingShipmentValueObjectDataAdapter();
		}

		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter(ForwardingConsol consol)
		{
			return new ForwardingShipmentValueObjectDataAdapter(consol);
		}

		protected override CommonShipment CreateNewPopulatedShipment(string transportMode)
		{
			ForwardingShipment result = (ForwardingShipment)base.CreateNewPopulatedShipment(transportMode);

			Order order1 = result.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "attached1";
			order1.JD_OrderDate = new ZDateTime(2005, 1, 1);

			Order order2 = result.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "attached2";
			order2.JD_OrderDate = new ZDateTime(2005, 1, 1);

			OrderItem orderReference1 = result.DocsAndCartage.OrderItems.AddNew();
			orderReference1.JT_OrderReference = "order_ref1";

			OrderItem orderReference2 = result.DocsAndCartage.OrderItems.AddNew();
			orderReference2.JT_OrderReference = "order_ref2";

			result.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bundle;
			return result;
		}

		protected override ForwardingShipment CreateNewShipmentWithEmptyFields()
		{
			ForwardingShipment result = base.CreateNewShipmentWithEmptyFields();
			result.AttachedOrders.DeleteAll();
			result.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Carton;

			return result;
		}

		#region Consignee

		protected OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
					fConsignee = Factory.LoadTop1<OrgHeader>(filter);
				}
				return fConsignee;
			}
		}
		OrgHeader fConsignee;

		#endregion

		#region Consignor

		protected OrgHeader Consignor
		{
			get
			{
				if (fConsignor == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
					fConsignor = Factory.LoadTop1<OrgHeader>(filter);
				}
				return fConsignor;
			}
		}
		OrgHeader fConsignor;

		#endregion

		#region Notification

		protected NotificationBuffer Notify
		{
			get
			{
				if (fNotify == null)
				{
					fNotify = new NotificationBuffer();
				}
				return fNotify;
			}
		}
		NotificationBuffer fNotify;

		#endregion

		#region GetXsdShipment

		Xsd.Shipment GetXsdShipmentWithOrder()
		{
			Xsd.Shipment result = new Xsd.Shipment();
			result.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			result.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			result.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			result.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			result.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();
			Xsd.ShipmentIdentifier identifier = result.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "housebill";

			Xsd.Organisation xsdConsignee = new Xsd.Organisation();
			xsdConsignee.EDICode = Consignee.OH_Code;
			xsdConsignee.OrganisationDetails = new Xsd.OrganisationDetail();
			xsdConsignee.OrganisationDetails.Name = Consignee.OH_FullName;
			xsdConsignee.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			xsdConsignee.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address = xsdConsignee.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "Test1";

			Xsd.Organisation xsdConsignor = new Xsd.Organisation();
			xsdConsignor.EDICode = Consignor.OH_Code;
			xsdConsignor.OrganisationDetails = new Xsd.OrganisationDetail();
			xsdConsignor.OrganisationDetails.Name = Consignor.OH_FullName;
			xsdConsignor.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");
			xsdConsignor.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address1 = xsdConsignor.OrganisationDetails.Addresses.AddNew();
			address1.AddressLine1 = "Test2";
			address1.AddressLine2 = "Test3";

			result.ShipmentDetails.Consignor = xsdConsignor;
			result.ShipmentDetails.Consignee = xsdConsignee;

			Xsd.Order xsdOrder = result.Orders.AddNew();
			xsdOrder.OrderIdentifier.OrderNumber = "TESTORDER";
			Xsd.OrderOrderDetail orderDetail = new Xsd.OrderOrderDetail();
			orderDetail.Buyer = xsdConsignee;
			orderDetail.Supplier = xsdConsignor;
			orderDetail.ConfirmNumber = "Confirmation2";
			orderDetail.OrderStatus = Core.Constants.OrderStatus.Incomplete;
			orderDetail.OrderStatusSpecified = true;
			xsdOrder.OrderDetail = orderDetail;

			Xsd.OrderOrderLineCollection orderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = orderLines.AddNew();
			xsdOrderLine.OrderLineNo = 1;
			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Product = "BOOKS";
			xsdOrderLine.OrderLineDetail.LinePrice = new Xsd.MonetaryAmount();
			xsdOrderLine.OrderLineDetail.LinePrice.Value = 100.0m;
			xsdOrderLine.OrderLineDetail.InnerPacks.Value = 10;
			xsdOrderLine.OrderLineDetail.OuterPacks.Value = 10;
			xsdOrder.OrderLines = orderLines;

			return result;
		}

		#endregion

		protected ForwardingShipmentValueObjectDataAdapter Adapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = (ForwardingShipmentValueObjectDataAdapter)GetNewShipmentValueObjectDataAdapter();
				}
				return fAdapter;
			}
		}
		ForwardingShipmentValueObjectDataAdapter fAdapter;

		#endregion
	}
}
