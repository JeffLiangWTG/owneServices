using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Events = Enterprise.ZArchitecture.Business.Events;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	[TestedType(typeof(QuotedBookingValueObjectDataAdapter))]
	sealed class QuotedBookingValueObjectDataAdapterTest : ValueObjectDataAdapterTest<QuotedBooking, ShipmentBooking>
	{
		QuotedBooking CreateQuotedBooking(string uniqueNumber, string houseBill, ZDateTime etd, string origin, string destination)
		{
			QuotedBooking quotedBooking = CreateQuotedBooking(uniqueNumber, houseBill);
			quotedBooking.Booking.JS_E_DEP = etd;
			quotedBooking.Booking.JS_RL_NKOrigin = origin;
			quotedBooking.Booking.JS_RL_NKDestination = destination;
			return quotedBooking;
		}

		QuotedBooking CreateQuotedBooking(string uniqueNumber, string houseBill)
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_UniqueConsignRef = uniqueNumber;
			quotedBooking.Booking.JS_HouseBill = houseBill;
			return quotedBooking;
		}

		public void TestFindBusinessObject()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking("number1", "housebill1");
			QuotedBooking quotedBooking2 = CreateQuotedBooking("number2", "");
			QuotedBooking quotedBooking3 = CreateQuotedBooking("", "housebill3");
			QuotedBooking quotedBooking4 = CreateQuotedBooking("", "houseBill3", new ZDateTime(2010, 1, 10), "AUSYD", "USLAX");
			QuotedBooking quotedBooking5 = CreateQuotedBooking("", "houseBill3", new ZDateTime(2010, 2, 10), "AUBNE", "USNYC");
			QuotedBooking quotedBooking6 = CreateQuotedBooking("", "houseBill3", new ZDateTime(2010, 3, 10), "CNSHA", "AUMEL");

			Factory.Save();

			ShipmentBooking shipmentBookingValue = new ShipmentBooking();
			shipmentBookingValue.Shipment = new Shipment();
			shipmentBookingValue.Shipment.ShipmentIdentifier = new ShipmentIdentifierCollection();
			ShipmentIdentifier housebillIdentifier = shipmentBookingValue.Shipment.ShipmentIdentifier.AddNew();
			housebillIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			Action<string, QuotedBooking> assertFind = (importMatching, expectedQuotedBooking) =>
			{
				SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importMatching);

				var adapter = new QuotedBookingValueObjectDataAdapterForTesting();
				var context = new ValueObjectImportContext(Factory, Xsd.XmlInterchange.Empty, new NotificationBuffer());

				QuotedBooking foundQuotedBooking = adapter.FindBusinessObject(shipmentBookingValue, context);
				if (expectedQuotedBooking != null)
				{
					AssertEquals(expectedQuotedBooking.Booking.PK, foundQuotedBooking.Booking.PK);
				}
				else
				{
					AssertNull(foundQuotedBooking);
				}
			};

			housebillIdentifier.Value = "random";
			shipmentBookingValue.Shipment.ShipmentDetails.AgentReference = "shrandom";
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, null);
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill, null);
			assertFind(Constants.ShipmentNumberImportTypes.Code.HouseBill, null);

			housebillIdentifier.Value = "housebill1";
			shipmentBookingValue.Shipment.ShipmentDetails.AgentReference = "";
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, null);
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill, quotedBooking1);
			assertFind(Constants.ShipmentNumberImportTypes.Code.HouseBill, quotedBooking1);

			housebillIdentifier.Value = "housebill3";
			shipmentBookingValue.Shipment.ShipmentDetails.AgentReference = "number2";
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, quotedBooking2);
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill, quotedBooking2);
			assertFind(Constants.ShipmentNumberImportTypes.Code.HouseBill, quotedBooking3);

			housebillIdentifier.Value = "";
			shipmentBookingValue.Shipment.ShipmentDetails.AgentReference = "number2";
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, quotedBooking2);
			assertFind(Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill, quotedBooking2);
			assertFind(Constants.ShipmentNumberImportTypes.Code.HouseBill, null);

			housebillIdentifier.Value = "housebill3";
			shipmentBookingValue.Shipment.ShipmentDetails.PortOfOrigin = new Movement { Port = UNLOCO.FromPortCode(Factory, "CNSHA"), EstimatedDateTime = new ZDateTime(2010, 1, 10) };
			shipmentBookingValue.Shipment.ShipmentDetails.PortofDestination = new Movement { Port = UNLOCO.FromPortCode(Factory, "USLAX") };

			var dataAdapter = new QuotedBookingValueObjectDataAdapterForTesting();
			var importContext = new ValueObjectImportContext(Factory, Xsd.XmlInterchange.Empty, new NotificationBuffer());

			QuotedBooking qb = dataAdapter.FindBusinessObject(shipmentBookingValue, importContext);
			AssertEquals(quotedBooking6.Booking.PK, qb.Booking.PK);

			shipmentBookingValue.Shipment.ShipmentDetails.PortOfOrigin = new Movement { Port = UNLOCO.FromPortCode(Factory, "UAIEV"), EstimatedDateTime = new ZDateTime(2010, 1, 10) };
			shipmentBookingValue.Shipment.ShipmentDetails.PortofDestination = new Movement { Port = UNLOCO.FromPortCode(Factory, "USNYC") };

			qb = dataAdapter.FindBusinessObject(shipmentBookingValue, importContext);
			AssertEquals(quotedBooking5.Booking.PK, qb.Booking.PK);

			shipmentBookingValue.Shipment.ShipmentDetails.PortOfOrigin = new Movement { Port = UNLOCO.FromPortCode(Factory, "USLAX"), EstimatedDateTime = new ZDateTime(2010, 1, 10) };
			shipmentBookingValue.Shipment.ShipmentDetails.PortofDestination = new Movement { Port = UNLOCO.FromPortCode(Factory, "UAIEV") };

			qb = dataAdapter.FindBusinessObject(shipmentBookingValue, importContext);
			AssertEquals(quotedBooking4.Booking.PK, qb.Booking.PK);

			shipmentBookingValue.Shipment.ShipmentDetails.PortOfOrigin = new Movement { Port = UNLOCO.FromPortCode(Factory, "USLAX"), EstimatedDateTime = new ZDateTime(2010, 4, 10) };
			shipmentBookingValue.Shipment.ShipmentDetails.PortofDestination = new Movement { Port = UNLOCO.FromPortCode(Factory, "UAIEV") };

			qb = dataAdapter.FindBusinessObject(shipmentBookingValue, importContext);
			AssertEquals(quotedBooking3.Booking.PK, qb.Booking.PK);
		}

		protected override QuotedBooking NewBusinessObject()
		{
			return QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
		}

		public void TestImportXMLVoyageChecksTheRightNo()
		{
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voyage1";
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			var etd = ZDateTime.Now;
			var eta = ZDateTime.Now.AddDays(1);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OrganisationTypes = OrganisationTypes.Carrier;
			carrier.OH_Code = "CARRIER2";
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = etd;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "MYPKG";
			destination.JB_E_ARV = eta;

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			voyage2.JV_VoyageFlight = "voyage2";
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_OH_Line = carrier.PK;

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = etd;

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "MYPKG";
			destination2.JB_E_ARV = eta;

			Factory.Save();

			var adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			var notification = new NotificationBuffer();
			var importContext = new ValueObjectImportContext(Factory, notification);
			var bookingValue = new ShipmentBooking();

			bookingValue.ShipmentBookingDetail.Item = new SailingWithVesselVoyage();
			((SailingWithVesselVoyage)bookingValue.ShipmentBookingDetail.Item).VesselName = "ADMIRALENGRACHT";
			((SailingWithVesselVoyage)bookingValue.ShipmentBookingDetail.Item).VoyageNo = "voyage2";
			bookingValue.ShipmentBookingDetail.Carrier = new Organisation() { EDICode = "CARRIER2" };
			bookingValue.ShipmentBookingDetail.Item.ETA = eta;
			bookingValue.ShipmentBookingDetail.Item.ETD = etd;
			bookingValue.ShipmentBookingDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			bookingValue.ShipmentBookingDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "MYPKG");
			bookingValue.Shipment.ShipmentDetails.TransportMode = TransportMode.SEA;
			var quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);

			AssertEquals("voyage2", quotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight);
		}

		public void TestAgentReferencePopulated()
		{
			QuotedBookingValueObjectDataAdapter adapter = new QuotedBookingValueObjectDataAdapter();

			QuotedBooking quotedBooking = NewBusinessObject();
			quotedBooking.Booking.JS_UniqueConsignRef = "uniqueref";
			ShipmentBooking shipmentValue = adapter.ExportToValueObject(quotedBooking, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("AgentReference should be populated from the unique consign ref", "uniqueref", shipmentValue.Shipment.ShipmentDetails.AgentReference);
		}

		public void TestCustomIsNotSpecified()
		{
			QuotedBookingValueObjectDataAdapter adapter = new QuotedBookingValueObjectDataAdapter();
			QuotedBooking quotedBooking = NewBusinessObject();
			quotedBooking.Booking.JS_UniqueConsignRef = "uniqueref";
			ShipmentBooking shipmentValue = adapter.ExportToValueObject(quotedBooking, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("ShipmentDetails.Custom should NOT be specified", false, shipmentValue.Shipment.ShipmentDetails.Custom.IsSpecified);
		}

		public void TestUpdatedOrCreatedNotificationNotShown()
		{
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			ShipmentBooking shipmentValue = new ShipmentBooking();

			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifications);

			adapter.CreateOrUpdateFromValueObject(shipmentValue, importContext);
			INotification[] createdOrUpdatedNotifications = notifications.GetEventsByType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated);
			AssertEquals("Should contain only 1 'created or updated' message", 1, createdOrUpdatedNotifications.Length);
		}

		public void TestExportOrderReferences()
		{
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			QuotedBooking quotedBooking = NewBusinessObject();
			OrderItem item = quotedBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "Order";
			item = quotedBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "Reference";
			item = quotedBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "Example";

			NotificationBuffer notification = new NotificationBuffer();
			ShipmentBooking shipmentValue = adapter.ExportToValueObject(quotedBooking, new ValueObjectExportContext(notification));
			AssertEquals("Should be 3 order references", 3, shipmentValue.Shipment.ShipmentDetails.OrderReferences.Length);
			AssertEquals("Should be 'Order'", "Order", shipmentValue.Shipment.ShipmentDetails.OrderReferences[0]);
			AssertEquals("Should be 'Reference'", "Reference", shipmentValue.Shipment.ShipmentDetails.OrderReferences[1]);
			AssertEquals("Should be 'Example'", "Example", shipmentValue.Shipment.ShipmentDetails.OrderReferences[2]);
		}

		public void TestImportOrderReferences()
		{
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notification = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notification);

			ShipmentBooking bookingValue = new ShipmentBooking();
			bookingValue.Shipment.ShipmentDetails.OrderReferences = new string[] { "Order1", "Order2", "Reference" };

			QuotedBooking quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);
			AssertEquals("Should be 3 order references", 3, quotedBooking.Booking.DocsAndCartage.OrderItems.Count);
			AssertEquals("Should be 'Order1'", "Order1", quotedBooking.Booking.DocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("Should be 'Order2'", "Order2", quotedBooking.Booking.DocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals("Should be 'Reference'", "Reference", quotedBooking.Booking.DocsAndCartage.OrderItems[2].JT_OrderReference);
		}

		public void TestUpdateOrdersAndOrderReferences()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "AAA";
			consignee.OH_FullName = "Alfred";
			consignee.MainAddress.OA_Address1 = "AAA Address";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "BBB";
			consignor.OH_FullName = "Bob";
			consignor.MainAddress.OA_Address1 = "BBB Address";

			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			ForwardingShipment shipment = quotedBooking.Booking;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HBL12345";
			shipment.JS_A_BKD = new ZDateTime(2010, 1, 1);

			Order order1 = shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "TESTORDER1";
			order1.JD_OrderGoodsDescription = "TESTORDER1 DESC";
			order1.JD_OrderDate = new ZDateTime(2010, 2, 1);

			Order order2 = shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "TESTORDER2";
			order2.JD_OrderGoodsDescription = "TESTORDER2 DESC";
			order2.JD_OrderDate = new ZDateTime(2010, 2, 1);

			Factory.Save();

			OrderItem orderReference1 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderReference1.JT_OrderReference = "REF1";

			OrderItem orderReference2 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderReference2.JT_OrderReference = "REF2";

			var adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			var notification = new NotificationBuffer();

			ShipmentBooking quotedBookingValue = adapter.ExportToValueObject(quotedBooking, new ValueObjectExportContext(notification));

			quotedBookingValue.Shipment.Orders[0].OrderDetail.Description = "TESTORDER1 DESC UPDATED";
			quotedBookingValue.Shipment.Orders[1].OrderDetail.Description = "TESTORDER2 DESC UPDATED";

			quotedBookingValue.Shipment.ShipmentDetails.OrderReferences[0] = "UPDATEDREF1";
			quotedBookingValue.Shipment.ShipmentDetails.OrderReferences[1] = "UPDATEDREF2";

			adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			QuotedBooking quotedBookingImported = adapter.CreateOrUpdateFromValueObject(quotedBookingValue, new ValueObjectImportContext(Factory, notification));

			AssertContains("Order TESTORDER1 updated", notification.AsString);
			AssertContains("Order TESTORDER2 updated", notification.AsString);
			AssertNotContains("Warning: Order TESTORDER1 is already attached and cannot be updated", notification.AsString);
			AssertNotContains("Warning: Order TESTORDER2 is already attached and cannot be updated", notification.AsString);
			AssertNotContains("Order TESTORDER1 has been attached to booking", notification.AsString);
			AssertNotContains("Order TESTORDER2 has been attached to booking", notification.AsString);

			AssertEquals("JD_OrderGoodsDescription should have been updated", "TESTORDER1 DESC UPDATED", quotedBookingImported.Booking.AttachedOrders[0].JD_OrderGoodsDescription);
			AssertEquals("JD_OrderGoodsDescription should have been updated", "TESTORDER2 DESC UPDATED", quotedBookingImported.Booking.AttachedOrders[1].JD_OrderGoodsDescription);

			AssertEquals("JD_OrderGoodsDescription should have been updated", "UPDATEDREF1", quotedBookingImported.Booking.DocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("JD_OrderGoodsDescription should have been updated", "UPDATEDREF2", quotedBookingImported.Booking.DocsAndCartage.OrderItems[1].JT_OrderReference);

			Factory.Save();

			quotedBookingValue.Shipment.Orders[0].OrderDetail.Description = "TESTORDER1 DESC UPDATED AGAIN";
			quotedBookingValue.Shipment.Orders[1].OrderDetail.Description = "TESTORDER2 DESC UPDATED AGAIN";

			quotedBookingValue.Shipment.ShipmentDetails.OrderReferences[0] = "UPDATEDREF1 AGAIN";
			quotedBookingValue.Shipment.ShipmentDetails.OrderReferences[1] = "UPDATEDREF2 AGAIN";

			notification.Clear();

			adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			quotedBookingImported = adapter.CreateOrUpdateFromValueObject(quotedBookingValue, new ValueObjectImportContext(Factory, notification));

			AssertContains("Order TESTORDER1 updated", notification.AsString);
			AssertContains("Order TESTORDER2 updated", notification.AsString);
			AssertNotContains("Order TESTORDER1 has been attached to booking", notification.AsString);
			AssertNotContains("Order TESTORDER2 has been attached to booking", notification.AsString);

			AssertEquals("JD_OrderGoodsDescription updated", "TESTORDER1 DESC UPDATED AGAIN", quotedBookingImported.Booking.AttachedOrders[0].JD_OrderGoodsDescription);
			AssertEquals("JD_OrderGoodsDescription updated", "TESTORDER2 DESC UPDATED AGAIN", quotedBookingImported.Booking.AttachedOrders[1].JD_OrderGoodsDescription);

			AssertEquals("JD_OrderGoodsDescription updated", "UPDATEDREF1 AGAIN", quotedBookingImported.Booking.DocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals("JD_OrderGoodsDescription updated", "UPDATEDREF2 AGAIN", quotedBookingImported.Booking.DocsAndCartage.OrderItems[1].JT_OrderReference);
		}

		public void TestImportSailing()
		{
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			NotificationBuffer notification = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notification);

			ShipmentBooking bookingValue = new ShipmentBooking();
			bookingValue.ShipmentBookingDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			bookingValue.ShipmentBookingDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");

			QuotedBooking quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);
			AssertNull("Not enough info to create a sailing", quotedBooking.ScheduleChooser.Sailing);

			bookingValue.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
			bookingValue.ShipmentBookingDetail.Item = new FlightWithFlightNumber();
			((FlightWithFlightNumber)bookingValue.ShipmentBookingDetail.Item).FlightNoJourneyNoTruckRegNo = "QF123";
			((FlightWithFlightNumber)bookingValue.ShipmentBookingDetail.Item).ETD = DateTime.Today;
			quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);
			AssertNotNull("Sailing Created - enough info for Air", quotedBooking.ScheduleChooser.Sailing);

			bookingValue.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			bookingValue.ShipmentBookingDetail.Item = new SailingWithVesselVoyage();
			((SailingWithVesselVoyage)bookingValue.ShipmentBookingDetail.Item).VoyageNo = "ABC";
			quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);
			AssertNull("Sailing not created - not enough info for Sea", quotedBooking.ScheduleChooser.Sailing);

			((SailingWithVesselVoyage)bookingValue.ShipmentBookingDetail.Item).VesselName = "ENTERPRISE VESSEL";
			quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, importContext);
			AssertNotNull("Sailing created - enough info for Sea", quotedBooking.ScheduleChooser.Sailing);
		}

		public void TestImportSailing_Sea_CarrierIsUsedForMatching()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER1";
			carrier1.OH_FullName = "CarrierONE";
			carrier1.OrganisationTypes = OrganisationTypes.Carrier;

			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CARRIER2";
			carrier2.OH_FullName = "CarrierTWO";
			carrier2.OrganisationTypes = OrganisationTypes.Carrier;

			Factory.Save();

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "NZAKL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK, "AUSYD", "NZAKL");

			var bookingValue = new ShipmentBooking();
			bookingValue.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			bookingValue.ShipmentBookingDetail.PortOfLoading = new Movement()
			{
				Port = new UNLOCO() { Value = "AUSYD" }
			};
			bookingValue.ShipmentBookingDetail.PortOfDischarge = new Movement()
			{
				Port = new UNLOCO() { Value = "NZAKL" }
			};

			var sailingWithVesselVoyage = new SailingWithVesselVoyage();
			sailingWithVesselVoyage.VoyageNo = "123";
			sailingWithVesselVoyage.VesselName = "Visund";
			sailingWithVesselVoyage.Carrier = new Organisation() { EDICode = "CARRIER2" };
			bookingValue.ShipmentBookingDetail.Item = sailingWithVesselVoyage;

			var adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var quotedBooking = adapter.CreateOrUpdateFromValueObject(bookingValue, context);

			var expectedSailingToBeLinkedTo = voyage2.Sailings[0];
			AssertEquals("Carrier used in matching, linked to correct sailing", expectedSailingToBeLinkedTo.PK, quotedBooking.Booking.JS_JX);
		}

		public void TestExportSailing_Sea_CarrierIsExported()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "MAERSK";

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("OLIVIA", "123", carrier.PK);

			var quotedBooking = NewBusinessObject();
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			quotedBooking.Booking.JS_JX = voyage.Sailings[0].PK;

			var bookingValue = new ShipmentBooking();
			var adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			adapter.ExportToValueObject(quotedBooking, bookingValue, new ValueObjectExportContext(new NotificationBuffer()));

			var sailingXSD = bookingValue.ShipmentBookingDetail.Item as SailingWithVesselVoyage;
			AssertEquals("MAERSK", sailingXSD.Carrier.EDICode);
		}

		public void TestClientRequestedETA()
		{
			ShipmentBooking xmlShipmentBooking = new ShipmentBooking();
			xmlShipmentBooking.ShipmentBookingDetailSpecified = true;
			xmlShipmentBooking.ShipmentBookingDetail.ClientRequestedETA = new ZDateTime(2011, 1, 6);

			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			QuotedBooking quotedBooking = adapter.CreateOrUpdateFromValueObject(xmlShipmentBooking, context);
			adapter.ImportFromValueObject(quotedBooking, xmlShipmentBooking, context);

			AssertEquals(xmlShipmentBooking.ShipmentBookingDetail.ClientRequestedETA, quotedBooking.Booking.JS_ClientRequestedETA);

			xmlShipmentBooking.ShipmentBookingDetail.ClientRequestedETA = ZDateTime.Empty;
			quotedBooking.Booking.JS_ClientRequestedETA = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), quotedBooking.Booking.JS_ClientRequestedETA);
		}

		public void TestCarrierOnInport()
		{
			ShipmentBooking xmlShipmentBooking = new ShipmentBooking();
			xmlShipmentBooking.ShipmentBookingDetailSpecified = true;
			xmlShipmentBooking.ShipmentBookingDetail.Carrier = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Carrier", OrganisationTypes.Carrier);

			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			QuotedBooking quotedBooking = adapter.CreateOrUpdateFromValueObject(xmlShipmentBooking, context);
			AssertEquals("Carrier", "Carrier", quotedBooking.Booking.BookedShippingLine.OH_FullName);
			AssertEquals("Carrier is Carrier", true, quotedBooking.Booking.BookedShippingLine.OH_IsShippingProvider);
		}

		public void TestExportOrders()
		{
			QuotedBooking quotedBooking = NewBusinessObject();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.Booking.ConsignorPK = consignor.PK;
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "TestOrder";
			order.JD_JS = quotedBooking.Booking.PK;
			order.BuyerPK = quotedBooking.Booking.ConsignorPK;

			ShipmentBooking bookingValue = new ShipmentBooking();
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			adapter.ExportToValueObject(quotedBooking, bookingValue, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Should have one order", 1, bookingValue.Shipment.Orders.Count);
			AssertEquals(order.JD_OrderNumber, bookingValue.Shipment.Orders[0].OrderIdentifier.OrderNumber);
		}

		public void TestExportOneOffQuote()
		{
			var quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			AssertNull("Precondition: Booking should be null", quotedBooking.Booking);

			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";
			quotedBooking.PaymentTerms = "FOB";
			quotedBooking.AdditionalTerms = "Gotcha Terms";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "EDICUS";
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var bookingValue = new ShipmentBooking();
			var adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			AssertEquals("Precondition: QuotedBooking should have the correct Object State", QuotedBookingState.QuoteOnly, quotedBooking.ObjectState);
			AssertExceptionThrown<NotSupportedException>("Not available for Spot Quotes.", () => adapter.ExportToValueObject(quotedBooking, bookingValue, new ValueObjectExportContext(new NotificationBuffer())));
		}

		public void TestImportOrders()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			ShipmentBooking bookingValue = new ShipmentBooking();
			bookingValue.ShipmentBookingDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			bookingValue.ShipmentBookingDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			bookingValue.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

			Organisation consigneeValue = new Organisation();
			consigneeValue.EDICode = consignee.OH_Code;
			consigneeValue.OrganisationDetails = new OrganisationDetail();
			consigneeValue.OrganisationDetails.Name = consignee.OH_FullName;
			consigneeValue.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			consigneeValue.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address1 = consigneeValue.OrganisationDetails.Addresses.AddNew();
			address1.AddressLine1 = "Test1";

			Organisation consignorValue = new Organisation();
			consignorValue.EDICode = consignor.OH_Code;
			consignorValue.OrganisationDetails = new OrganisationDetail();
			consignorValue.OrganisationDetails.Name = consignor.OH_FullName;
			consignorValue.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");
			consignorValue.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address2 = consignorValue.OrganisationDetails.Addresses.AddNew();
			address2.AddressLine1 = "Test2";
			address2.AddressLine2 = "Test3";

			Xsd.Order orderValue = bookingValue.Shipment.Orders.AddNew();
			orderValue.OrderIdentifier.OrderNumber = "TESTORDER";
			OrderOrderDetail orderDetailValue = new OrderOrderDetail();
			orderDetailValue.Buyer = consigneeValue;
			orderDetailValue.Supplier = consignorValue;
			orderDetailValue.ConfirmNumber = "Confirmation2";
			orderDetailValue.OrderStatus = Core.Constants.OrderStatus.Incomplete;
			orderDetailValue.OrderStatusSpecified = true;
			orderValue.OrderDetail = orderDetailValue;
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notificationBuffer);
			QuotedBookingValueObjectDataAdapter adapter = (QuotedBookingValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			adapter.CreateOrUpdateFromValueObject(bookingValue, context);

			AssertNotContains("Order TESTORDER updated", notificationBuffer.AsString);
			AssertNotContains("Warning: Order TESTORDER is already attached and cannot be updated", notificationBuffer.AsString);
			AssertContains("Order TESTORDER has been attached to booking", notificationBuffer.AsString);

			Factory.Save();

			var filter = new ZQuery(JobShipmentSchema.JS_RL_NKOrigin, bookingValue.ShipmentBookingDetail.PortOfLoading.Port.Value);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_RL_NKDestination, bookingValue.ShipmentBookingDetail.PortOfDischarge.Port.Value));
			var shipment = Factory.LoadTop1<ForwardingShipment>(filter);

			AssertEquals(1, shipment.AttachedOrders.Count);
			AssertEquals(orderValue.OrderIdentifier.OrderNumber, shipment.AttachedOrders[0].JD_OrderNumber);
		}

		#region Implementation

		protected override ValueObjectDataAdapter<QuotedBooking, ShipmentBooking> GetNewBizObjXmlDataAdapter()
		{
			return new QuotedBookingValueObjectDataAdapterForTesting();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "ShipmentBookings"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "ShipmentBooking"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var quotedBooking = CreateQuotedBooking();
			quotedBooking.Booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			quotedBooking.Booking.JS_PackingMode = ZString.Empty;
			quotedBooking.Booking.JS_A_BKD = new ZDateTime(2009, 6, 1);

			var container = Factory.New<ForwardingContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);

			currentTestQuotedBookings.Add(quotedBooking);
			var emptyShipmentBookingXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.QuotedBookings.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.EmptyShipmentBooking.xml");
			return new BusinessObjectAndExpectedOutputFileName(quotedBooking, emptyShipmentBookingXmlPath, ValidationKind.None, "Empty Shipment Booking");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var airShipmentBookingXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.QuotedBookings.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.AirShipmentBooking.xml");
			return new BusinessObjectAndExpectedOutputFileName(NewTestShipmentBookingWithOrders(Core.Constants.TransportModes.Air, false, false), airShipmentBookingXmlPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Air Shipment Booking");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var seaShipmentBookingXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.QuotedBookings.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.SeaShipmentBooking.xml");
			var seaShipmentBookingWithOrdersXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.QuotedBookings.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.SeaShipmentBookingWithOrders.xml");
			var airShipmentBookingWithOrdersXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.QuotedBookings.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.AirShipmentBookingWithOrders.xml");

			var result = new BusinessObjectAndExpectedOutputFileName[]
			{
				new BusinessObjectAndExpectedOutputFileName(NewTestShipmentBookingWithOrders(Constants.TransportModes.Sea, false, true), seaShipmentBookingXmlPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Sea Shipment Booking"),
				new BusinessObjectAndExpectedOutputFileName(NewTestShipmentBookingWithOrders(Constants.TransportModes.Sea, true, true, "pfx1"), seaShipmentBookingWithOrdersXmlPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Sea Shipment Booking With Orders"),
				new BusinessObjectAndExpectedOutputFileName(NewTestShipmentBookingWithOrders(Constants.TransportModes.Air, true, false, "pfx2"), airShipmentBookingWithOrdersXmlPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Air Shipment Booking With Orders")
			};

			return result;
		}

		QuotedBooking NewTestShipmentBookingWithOrders(string transportMode, bool createOrders, bool createContainers, string orderNoUniquePrefix = "")
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ZUBIN";
			consignee.OH_FullName = "ZUBIN";
			consignee.MainAddress.OA_Address1 = "ZUBIN STREET";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "ZASHA";
			consignor.OH_FullName = "ZASHA";
			consignor.MainAddress.OA_Address1 = "ZASHA STREET";

			var goodAvailableAtAddress = Factory.NewWithValidTestData<MasterFiles.Business.OrgAddress>();
			goodAvailableAtAddress.OA_Code = "ZASHA";

			var goodDeliveredToAddress = Factory.NewWithValidTestData<MasterFiles.Business.OrgAddress>();
			goodDeliveredToAddress.OA_Code = "ZUBIN";

			QuotedBooking quotedBooking = CreateQuotedBooking();
			ForwardingShipment shipment = quotedBooking.Booking;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "HB_Testing";
			shipment.JS_A_BKD = new ZDateTime(2009, 6, 1);

			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voyage";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			origin.JA_E_DEP = new ZDateTime(2005, 1, 1);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "MYPKG";
			destination.JB_E_ARV = new ZDateTime(2005, 2, 2);

			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(GlbBranch.CurrentBranch.GB_RL_NKHomePort, "MYPKG");
			(quotedBooking as ISailingChooserParent).SailingJX = sailing.PK;

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "DIVYA";
			carrier.OH_FullName = "DIVYA";
			carrier.MainAddress.OA_Address1 = "DIVYA STREET";

			shipment.JS_ClientRequestedETA = new ZDateTime(2011, 1, 6);
			shipment.JS_CFSReference = "CFS_Reference";
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			if (createOrders)
			{
				Order order1 = shipment.AttachedOrders.AddNew();
				order1.JD_OrderNumber = orderNoUniquePrefix + "attached1";
				order1.JD_OrderDate = new ZDateTime(2005, 1, 1);
				order1.GoodsAvailableAtAddress.E2_OA_Address = goodAvailableAtAddress.PK;
				order1.GoodsDeliveredToAddress.E2_OA_Address = goodDeliveredToAddress.PK;

				Order order2 = shipment.AttachedOrders.AddNew();
				order2.JD_OrderNumber = orderNoUniquePrefix + "attached2";
				order2.JD_OrderDate = new ZDateTime(2005, 1, 1);
				order2.GoodsAvailableAtAddress.E2_OA_Address = goodAvailableAtAddress.PK;
				order2.GoodsDeliveredToAddress.E2_OA_Address = goodDeliveredToAddress.PK;

				OrderItem orderReference1 = shipment.DocsAndCartage.OrderItems.AddNew();
				orderReference1.JT_OrderReference = "order_ref1";

				OrderItem orderReference2 = shipment.DocsAndCartage.OrderItems.AddNew();
				orderReference2.JT_OrderReference = "order_ref2";

				shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bundle;
			}
			else
			{
				OrderItem item = shipment.DocsAndCartage.OrderItems.AddNew();
				item.JT_OrderReference = "OrderReference";
			}

			if (createContainers)
			{
				ForwardingContainer container1 = Factory.New<ForwardingContainer>();
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
				container1.JC_ContainerNum = "ABCD000001";
				container1.JC_ReleaseNum = "cont123";
				container1.JC_SealNum = "sealnum1";
				container1.JC_RH_NKContainerCommodityCode = "GEN";
				quotedBooking.QuotedBookingContainers.Add(container1);

				ForwardingContainer container2 = Factory.New<ForwardingContainer>();
				container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
				container2.JC_ContainerNum = "WXYZ000001";
				container2.JC_ReleaseNum = "cont321";
				container2.JC_SealNum = "sealnum2";
				container2.JC_RH_NKContainerCommodityCode = "CABB";
				quotedBooking.QuotedBookingContainers.Add(container2);
			}

			quotedBooking.Logs.CancelAll();
			quotedBooking.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2005, 3, 3));

			currentTestQuotedBookings.Add(quotedBooking);

			return quotedBooking;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// handled by other data adapters
					"ShipmentBookingDetail/Voyage",
					"Shipment",
					"ShipmentBookingDetail/Item/DepartureCTO/Organisation",
					"ShipmentBookingDetail/Item/ArrivalCTO/Organisation",
					"ShipmentBookingDetail/Item/ETD",
					"ShipmentBookingDetail/Item/ETA",
					"ShipmentBookingDetail/Item/DepartureBerth",
					"ShipmentBookingDetail/Item/ArrivalBerth",
					"ShipmentBookingDetail/Item/DocCutOffDate",
					"ShipmentBookingDetail/Item/IsTranshipment",
					"ShipmentBookingDetail/Item/IsPublished",
					"ShipmentBookingDetail/PortOfDischarge/Port/City",
					"ShipmentBookingDetail/PortOfDischarge/Port/Country",
					"ShipmentBookingDetail/Custom",
					"ShipmentBookingDetail/Item/ATD",
					"ShipmentBookingDetail/Item/ATA",
					"ShipmentBookingDetail/Item/LoadPortETA",
					"ShipmentBookingDetail/Item/LoadPortATA",
					"ShipmentBookingDetail/IsDirectBooking",

					"ShipmentBookingDetail/Containers/EstimatedDelivery",
					"ShipmentBookingDetail/Containers/DeliveryMode",
					"ShipmentBookingDetail/Containers/LCLAvailable",
					"ShipmentBookingDetail/Containers/FCLAvailable",
					"ShipmentBookingDetail/Containers/ImportProcess",
					"ShipmentBookingDetail/Containers/ExportProcess",
					"ShipmentBookingDetail/Containers/Custom",
					"ShipmentBookingDetail/Containers/SetPointTemperature",
					"ShipmentBookingDetail/Containers/SetPointTemperatureUnit",
					"ShipmentBookingDetail/Containers/HumidityPercent",
					"ShipmentBookingDetail/Containers/AirVentFlow",
					"ShipmentBookingDetail/Containers/AirVentFlowRateUnit",
					"ShipmentBookingDetail/Containers/BookingReference",
					"ShipmentBookingDetail/Containers/Seal2",
					"ShipmentBookingDetail/Containers/Seal3",
					"ShipmentBookingDetail/Containers/IsShipperOwnedContainer",
					"ShipmentBookingDetail/Containers/IsArrivingAtCTOByRail",
					"ShipmentBookingDetail/Containers/IsEmptyContainer",
					"ShipmentBookingDetail/Containers/IsDamaged",

					"ShipmentBookingDetail/Carrier",

					// these shouldnt be used ever
					"ShipmentBookingDetail/PortOfLoading/EstimatedDateTime",
					"ShipmentBookingDetail/PortOfLoading/ActualDateTime",
					"ShipmentBookingDetail/PortOfDischarge/EstimatedDateTime",
					"ShipmentBookingDetail/PortOfDischarge/ActualDateTime",
				};
			}
		}

		QuotedBooking CreateQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ZUBIN";
			consignee.OH_FullName = "ZUBIN";
			consignee.MainAddress.OA_Address1 = "ZUBIN STREET";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "ZASHA";
			consignor.OH_FullName = "ZASHA";
			consignor.MainAddress.OA_Address1 = "ZASHA STREET";

			QuotedBooking result = QuotedBooking.New(quote.PK, booking.PK, Factory);
			result.ClientPK = consignee.PK;
			result.Mode = Core.Constants.RateMode.FCL;
			result.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			result.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			return result;
		}

		class QuotedBookingValueObjectDataAdapterForTesting : QuotedBookingValueObjectDataAdapter
		{
			public QuotedBookingValueObjectDataAdapterForTesting()
				: base()
			{
			}

			public new QuotedBooking FindBusinessObject(ShipmentBooking value, IValueObjectImportContext context)
			{
				return base.FindBusinessObject(value, context);
			}

			protected override bool ShouldPopulatedAgentReference()
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			currentTestQuotedBookings = new List<QuotedBooking>();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			DisposeCurrentTestQuotedBookingJobHeaderMutexes();
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		void DisposeCurrentTestQuotedBookingJobHeaderMutexes()
		{
			foreach (QuotedBooking quotedBooking in currentTestQuotedBookings)
			{
				if (quotedBooking != null && quotedBooking.Job != null)
				{
					quotedBooking.Job.Dispose();
				}
			}
		}

		List<QuotedBooking> currentTestQuotedBookings;

		#endregion
	}
}
