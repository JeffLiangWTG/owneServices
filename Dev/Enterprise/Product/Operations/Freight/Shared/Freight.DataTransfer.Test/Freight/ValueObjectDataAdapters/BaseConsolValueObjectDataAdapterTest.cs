using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	public abstract class BaseConsolValueObjectDataAdapterTest<TBusinessObject> : ValueObjectDataAdapterTest<TBusinessObject, Xsd.Consol>
			where TBusinessObject : CommonConsol
	{
		#region Import Shipments

		public void TestImportConsolShipments()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");

			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL1"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL1"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL3"));

			var adapter = GetNewBizObjXmlDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject consol = NewBusinessObject();
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals(2, consol.Shipments.Count);
			AssertEquals("HBL1", consol.Shipments[0].JS_HouseBill);
			AssertEquals("HBL3", consol.Shipments[1].JS_HouseBill);
		}

		#region Importing Master Shipment

		public void TestImportConsolMasterShipmentASM()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.ASM, true);
		}

		public void TestImportConsolMasterShipmentASMLast()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.ASM, false);
		}

		public void TestImportConsolMasterShipmentBCN()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.BCN, true);
		}

		public void TestImportConsolMasterShipmentBCNLast()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.BCN, false);
		}

		public void TestImportConsolMasterShipmentCLD()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.CLD, true);
		}

		public void TestImportConsolMasterShipmentCLDLast()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.CLD, false);
		}

		public void TestImportConsolMasterShipmentSCN()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.SCN, true);
		}

		public void TestImportConsolMasterShipmentSCNLast()
		{
			TestImportConsolMasterShipment(Xsd.ForwardingShipmentType.SCN, false);
		}

		void TestImportConsolMasterShipment(Xsd.ForwardingShipmentType masterShipmentType, bool masterFirst)
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");

			Xsd.Shipment masterShipmentValue;
			if (masterFirst)
			{
				masterShipmentValue = CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUBNE", "SGSIN", "S001");
				masterShipmentValue.ShipmentDetails.ForwardingShipmentType = masterShipmentType;
				consolValue.Shipments.Add(masterShipmentValue);
			}

			Xsd.Shipment shipment2 = CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUBNE", "SGSIN", "S002");
			shipment2.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.CoLoadMaster, "S001");
			consolValue.Shipments.Add(shipment2);

			Xsd.Shipment shipment3 = CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUBNE", "SGSIN", "S003");
			shipment3.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.CoLoadMaster, "S001");
			consolValue.Shipments.Add(shipment3);

			if (!masterFirst)
			{
				masterShipmentValue = CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUBNE", "SGSIN", "S001");
				masterShipmentValue.ShipmentDetails.ForwardingShipmentType = masterShipmentType;
				consolValue.Shipments.Add(masterShipmentValue);
			}

			foreach (Xsd.Shipment shipmentValue in consolValue.Shipments)
			{
				if (shipmentValue.ShipmentDetails.ForwardingShipmentType != Xsd.ForwardingShipmentType.ASM
					&& shipmentValue.ShipmentDetails.ForwardingShipmentType != Xsd.ForwardingShipmentType.CLD)
				{
					shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
					shipmentValue.ShipmentDetails.Packages.AddNew();
					shipmentValue.ShipmentDetails.Packages.AddNew();
				}
			}

			var adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject consol = NewBusinessObject();
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals("Expected 3 shipments", 3, consol.Shipments.Count);

			var masterShipment = consol.Shipments.Cast<CommonShipment>().FirstOrDefault(s => s.JS_ShipmentType != "STD");
			AssertNotNull(masterShipment);

			var standardShipments = consol.Shipments.Cast<CommonShipment>().Where(s => s != masterShipment).ToArray();
			AssertEquals(standardShipments.Length, 2);

			AssertNotNull(string.Format("Expected {0} shipment", masterShipmentType), standardShipments[0].CoLoadMasterShipment);
			AssertEquals(standardShipments[0].CoLoadMasterShipment.PK, masterShipment.PK);

			AssertNotNull(string.Format("Expected {0} shipment", masterShipmentType), standardShipments[1].CoLoadMasterShipment);
			AssertEquals(standardShipments[0].CoLoadMasterShipment.PK, masterShipment.PK);

			if (masterShipmentType == Xsd.ForwardingShipmentType.ASM || masterShipmentType == Xsd.ForwardingShipmentType.CLD)
			{
				AssertEquals(4, masterShipment.OuterPackLines.Count);
			}
		}

		#endregion

		public void TestImportShipmentsIntoMatchedAndConvertedQuickBookings()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port.Value = "AUSYD";
			consolValue.ConsolDetail.PortOfDischarge.Port.Value = "USLAX";

			Xsd.Shipment shipmentValue = CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUSYD", "USLAX", "BookingHouseBill");
			shipmentValue.ShipmentDetails.GoodsDescription = "yahoo!";
			consolValue.Shipments.Add(shipmentValue);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);

			var booking = Factory.New<CommonShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_HouseBill = "BookingHouseBill";
			booking.JS_GoodsDescription = "old goods description";

			var anotherBooking = Factory.New<CommonShipment>();
			anotherBooking.JS_IsBooking = true;
			anotherBooking.JS_IsForwardRegistered = false;
			anotherBooking.JS_HouseBill = "notmatched";

			Factory.Save();

			var adapter = GetNewBizObjXmlDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject consol = NewBusinessObject();
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals("Shipment imported", 1, consol.Shipments.Count);

			if (QuickBookingShouldBeMatchedAndConverted)
			{
				var shipment = consol.Shipments[0];
				AssertEquals("Quick booking was converted", booking.PK, shipment.PK);
				AssertEquals("Quick booking was converted", true, shipment.JS_IsForwardRegistered);
				AssertEquals("Details were updated", "yahoo!", shipment.JS_GoodsDescription);
			}
		}

		protected virtual bool QuickBookingShouldBeMatchedAndConverted
		{
			get { return true; }
		}

		public void TestImportShipmentsOnlyQuickBookingsAreMatched()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port.Value = "AUSYD";
			consolValue.ConsolDetail.PortOfDischarge.Port.Value = "USLAX";

			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, Consignor.OH_Code, Consignee.OH_Code, "AUSYD", "USLAX", "hello"));

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);

			var directBooking = Factory.New<CommonShipment>();
			directBooking.JS_IsBooking = true;
			directBooking.JS_IsDirectBooking = true;
			directBooking.JS_HouseBill = "hello";

			var confirmedBooking = Factory.New<CommonShipment>();
			confirmedBooking.JS_IsBooking = true;
			confirmedBooking.JS_IsForwardRegistered = true;
			confirmedBooking.JS_HouseBill = "hello";

			IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Integration.QuoteBookingType.BookingWithQuote, Factory);
			var bookingWithQuote = (CommonShipment)quotedBooking.ForwardingShipment;
			bookingWithQuote.JS_HouseBill = "hello";

			new JobHeader.Loader(bookingWithQuote).TryLoadOrCreate().JH_GE = GlbDepartment.CurrentDepartment.PK;  // Requirement for Save
			Factory.Save();

			ZQuery sameHouseBillQuery = new ZQuery(JobShipmentSchema.JS_HouseBill, "hello");
			int countBeforeImport = Factory.Load<CommonShipment>(sameHouseBillQuery).Length;
			AssertEquals("Precondition", 3, countBeforeImport);

			var adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject consol = NewBusinessObject();
			adapter.ImportFromValueObject(consol, consolValue, context);

			int countAfterImport = Factory.Load<CommonShipment>(sameHouseBillQuery).Length;
			AssertEquals("No quick bookings found, new shipment was created during import", 1, countAfterImport - countBeforeImport);
		}

		#endregion

		#region SEA Linked Transports

		public void TestExport_Sea_LinkedTransport_CarrierIsExportedToConsolSailing()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "THECARRIER";

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("Visund", "500", carrier.PK);

			var consol = NewBusinessObject();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var adapter = GetNewConsolValueObjectDataAdapter();
			var consolXML = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));

			var sailingXML = consolXML.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;
			AssertEquals("Transport's carrier exported", true, sailingXML.Carrier.IsSpecified);
			AssertEquals("Transport's carrier exported", "THECARRIER", sailingXML.Carrier.EDICode);
		}

		#endregion

		public void TestUseMappingsForUltimateLoadAndDischarge()
		{
			string fakePortCode1 = "TORONTO";
			string fakePortCode2 = "SYDNEY";
			string realPortCode1 = "CATOR";
			string realPortCode2 = "AUSYD";

			OrgHeader orgWithMappings = Factory.NewWithValidTestData<OrgHeader>();

			OrgPatternMatchOverride override1 = OrgPatternMatchOverrideCollectionExtensions.CreatePatternMatchOverrideForTest(orgWithMappings);
			override1.OO_ForeignCode = fakePortCode1;
			override1.OO_Relationship = "PTC";
			override1.OO_LocalGuid = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, realPortCode1).PK;

			OrgPatternMatchOverride override2 = orgWithMappings.CreatePatternMatchOverrideForTest();
			override2.OO_ForeignCode = fakePortCode2;
			override2.OO_Relationship = "PTC";
			override2.OO_LocalGuid = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, realPortCode2).PK;

			Factory.Save();

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.EDICode = orgWithMappings.OH_Code;

			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, fakePortCode1, ZDateTime.Empty, ZDateTime.Empty);
			consolValue.ConsolDetail.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, fakePortCode2, ZDateTime.Empty, ZDateTime.Empty);

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			TBusinessObject consol = Factory.New<TBusinessObject>();
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals("should have mapped the load port", realPortCode1, consol.JK_RL_NKLoadPort);
			AssertEquals("should have mapped the discharge port", realPortCode2, consol.JK_RL_NKDischargePort);
		}

		public void TestOnlyOperationalEventsAreImported()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");

			// Create events (2 operational and 2 admin)
			Xsd.Events xsdEvents = new Xsd.Events();

			// DataExport Event
			Xsd.Event xsdEvent = xsdEvents.Event.AddNew();
			xsdEvent.Code = Events.DataExport.Code;
			xsdEvent.DateTime = ZDateTime.Now;
			// OrderShipped Event
			Xsd.Event bookedEvent = xsdEvents.Event.AddNew();
			bookedEvent.Code = Events.Booked.Code;
			bookedEvent.DateTime = ZDateTime.Now;

			// Add Event
			Xsd.Event addEvent = xsdEvents.Event.AddNew();
			addEvent.Code = Events.AddedARecordToTheSystem.Code;
			addEvent.DateTime = ZDateTime.Now;
			// Edit Event
			Xsd.Event editEvent = xsdEvents.Event.AddNew();
			editEvent.Code = Events.EditedARecord.Code;
			editEvent.DateTime = ZDateTime.Now;

			consolValue.Events = xsdEvents;

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			TBusinessObject consol = NewBusinessObject();
			StmALog log = consol.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNull("Precondition: no DEX event", log);

			adapter.ImportFromValueObject(consol, consolValue, context);

			StmALog[] dataExportLog = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DataExport Event should have been imported", 1, dataExportLog.Length);
			StmALog[] bookedLog = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Booked.Code));
			AssertEquals("Booked Event should have been imported", 1, bookedLog.Length);

			StmALog[] addLog = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code));
			AssertEquals("Add Event should NOT have been imported", 0, addLog.Length);
			StmALog[] editLog = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Edit Event should NOT have been imported", 0, editLog.Length);
		}

		#region Vessel Name / Lloyds number

		public void TestImportVesselForMainTransport_MatchingNotFoundImportImportLooydsNoAsVessel()
		{
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.LloydsNo = "9999999";

			var consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.Item = sailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", "9999999", transport.JW_Vessel);
		}

		public void TestImportVesselForLegTransport_MatchingNotFoundImportImportLooydsNoAsVessel()
		{
			var legSailing = new Xsd.SailingWithVesselVoyage();
			legSailing.LloydsNo = "9999999";

			var consolValue = new Xsd.Consol();
			var leg = consolValue.ConsolDetail.PlannedLegs.AddNew();
			leg.Item = legSailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", "9999999", transport.JW_Vessel);
		}

		public void TestImportVesselForMainTransport_MatchingNotFoundImportImportVesselNameAsVessel()
		{
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VesselName = "Test Vessel Name";

			var consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.Item = sailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", "Test Vessel Name", transport.JW_Vessel);
		}

		public void TestImportVesselForLegTransport_MatchingNotFoundImportImportVesselNameAsVessel()
		{
			var legSailing = new Xsd.SailingWithVesselVoyage();
			legSailing.VesselName = "Test Vessel Name";

			var consolValue = new Xsd.Consol();
			var leg = consolValue.ConsolDetail.PlannedLegs.AddNew();
			leg.Item = legSailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", "Test Vessel Name", transport.JW_Vessel);
		}

		public void TestImportVesselForMainTransport_MatchByLloydsNumberFallbackToVesselName()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.LloydsNo = "9999999";
			sailing.VesselName = existingVessel.RV_Name;

			var consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.Item = sailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", existingVessel.RV_FK, transport.JW_Vessel);
		}

		public void TestImportVesselForLegTransport_MatchByLloydsNumberFallbackToVesselName()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var legSailing = new Xsd.SailingForPlannedLegs();
			legSailing.LloydsNo = "9999999";
			legSailing.VesselName = existingVessel.RV_Name;

			var consolValue = new Xsd.Consol();
			var leg = consolValue.ConsolDetail.PlannedLegs.AddNew();
			leg.Item = legSailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", existingVessel.RV_FK, transport.JW_Vessel);
		}

		public void TestImportVesselForMainTransport_MatchByLloydsNumber()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.LloydsNo = vessel.RV_LloydsNumber;

			var consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.Item = sailing;
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", vessel.RV_FK, transport.JW_Vessel);
		}

		public void TestImportVesselForLegTransport_MatchByLloydsNumber()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var legSailing = new Xsd.SailingForPlannedLegs();
			legSailing.LloydsNo = vessel.RV_LloydsNumber;

			var consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			var leg = consolValue.ConsolDetail.PlannedLegs.AddNew();
			leg.Item = legSailing;

			var consol = NewBusinessObject();
			new ConsolValueObjectDataAdapterForTest<TBusinessObject>().ImportFromValueObject(consol, consolValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			var transport = consol.Transports[0];
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", vessel.RV_FK, transport.JW_Vessel);
		}

		#endregion

		public void TestImportWithoutPlannedLegs()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			RefVessel existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VesselName = existingVessel.RV_Name;
			sailing.VoyageNo = "1234";

			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = now.AddDays(1);
			consolValue.ConsolDetail.PortOfLoading.ActualDateTime = now.AddDays(2);
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");
			consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = now.AddDays(3);
			consolValue.ConsolDetail.PortOfDischarge.ActualDateTime = now.AddDays(4);
			consolValue.ConsolDetail.Item = sailing;

			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");
			var voyage = helper.CreateSeaVoyage(existingVessel.RV_Name, "1234", carrier.PK, "AUBNE", "SGSIN");
			Factory.Save();

			var adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject consol = NewBusinessObject();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals("should have exactly 1 transport", 1, consol.Transports.Count);
			AssertEquals("Consol Transport Mode", Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals("Consol Load", "AUBNE", consol.JK_RL_NKLoadPort);
			AssertEquals("Consol Discharge", "SGSIN", consol.JK_RL_NKDischargePort);

			Transport transport = consol.Transports[0];
			AssertEquals("Transport should be linked: matched to existing sailing", true, transport.JW_IsLinked);
			AssertEquals("Transport Transport Mode", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Vessel", existingVessel.RV_FK, transport.JW_Vessel);
			AssertEquals("Voyage", "1234", transport.JW_VoyageFlight);
			AssertEquals("Carrier", carrier.PK, transport.CarrierPK);
			AssertEquals("Transport Load", "AUBNE", transport.JW_RL_NKLoadPort);
			AssertEquals("Transport Discharge", "SGSIN", transport.JW_RL_NKDiscPort);
			AssertEquals("ETD", now.AddDays(1), transport.JW_ETD);
			AssertEquals("ATD", now.AddDays(2), transport.JW_ATD);
			AssertEquals("ETA", now.AddDays(3), transport.JW_ETA);
			AssertEquals("ATA", now.AddDays(4), transport.JW_ATA);
		}

		public void TestConsol_Export_CreatedDate()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject consol = NewBusinessObject();
			consol.FillWithValidTestData();

			Xsd.Consol xmlConsol = new Xsd.Consol();
			adapter.ExportToValueObject(consol, xmlConsol, new ValueObjectExportContext(notify));
			AssertEquals(DateTime.MinValue, xmlConsol.ConsolDetail.DateCreated);

			Factory.Save();
			Assert("PreCondition: Consol.Logs.CreatedDate is valid", consol.Logs.CreatedDateUtc.IsValid);

			Xsd.Consol xmlConsol1 = new Xsd.Consol();
			adapter.ExportToValueObject(consol, xmlConsol1, new ValueObjectExportContext(notify));
			AssertEquals(consol.Logs.CreatedDateUtc.ToDateTime(), xmlConsol1.ConsolDetail.DateCreated);
		}

		public void TestConsol_Export_CustomsEntryNumber()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject consol = NewBusinessObject();
			CusEntryNumber cen1 = consol.CusEntryNums.AddNew();
			cen1.FillWithValidTestData();
			cen1.CE_ParentID = consol.PK;
			CusEntryNumber cen2 = consol.CusEntryNums.AddNew();
			cen2.FillWithValidTestData();
			cen2.CE_ParentID = consol.PK;
			cen2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Algeria;

			Xsd.Consol xmlConsol = new Xsd.Consol();
			adapter.ExportToValueObject(consol, xmlConsol, new ValueObjectExportContext(notify));
			AssertEquals(2, xmlConsol.ConsolDetail.CustomsEntryNumbers.Count);
		}

		public void TestConsol_Import_CustomsEntryNumber()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject consol = NewBusinessObject();

			Xsd.Consol xmlConsol = new Xsd.Consol();
			Xsd.CustomsEntryNumber cen = xmlConsol.ConsolDetail.CustomsEntryNumbers.AddNew();
			cen.Country = "NZ";
			cen.Number = "123";
			cen.Type = "CAN";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(consol, xmlConsol, context);
			AssertEquals(1, consol.CusEntryNums.Count);
			AssertEquals(consol.PK, consol.CusEntryNums[0].CE_ParentID);
		}

		public void TestImportAgentReferencePutInConsolRef()
		{
			SystemDataRegistry.Instance.ImportConsolNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.Consol xmlConsol = new Xsd.Consol();
			xmlConsol.ConsolDetail.AgentReference = "AgentReference";

			TBusinessObject consol = NewBusinessObject();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(consol, xmlConsol, context);

			AssertEquals("Should populate the unique consign ref with agent reference", "AgentReference", consol.JK_UniqueConsignRef);
		}

		public void TestConsol_Creditor()
		{
			ConsolValueObjectDataAdapter<TBusinessObject, CommonShipment, Xsd.Consol> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject consol = NewBusinessObject();

			Xsd.Consol xmlConsol = new Xsd.Consol();

			Xsd.Organisation creditor = new Xsd.Organisation();
			creditor.EDICode = "ABC";
			creditor.OrganisationDetails.Name = "ABC Company";
			Xsd.OrgAddress orgAddr = creditor.OrganisationDetails.Addresses.AddNew();
			orgAddr.AddressLine1 = "sssss";

			xmlConsol.ConsolDetail.Creditor = creditor;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(consol, xmlConsol, context);

			AssertEquals("Creditor is ABC", "ABC", consol.Creditor.OH_Code);
		}

		public void TestCollectionSchema()
		{
			AssertNotNull("Should have the CollectionSchema specified", new ConsolValueObjectDataAdapterForTest<TBusinessObject>().CollectionSchema);
		}

		public void TestImportNotesExist()
		{
			Xsd.NotesNote note = new Xsd.NotesNote();
			note.NoteData = "you are a wineo";
			note.NoteType = Xsd.NotesNoteNoteType.BookingNotes;

			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.Notes = new Xsd.NotesNoteCollection();
			consolValue.Notes.Add(note);

			TBusinessObject consolBizObj = NewBusinessObject();

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(consolBizObj, consolValue, context);

			AssertEquals(true, consolBizObj.Notes.HasNotes);
		}

		public void TestExportNotesExist()
		{
			Xsd.Consol consol1 = new Xsd.Consol();
			consol1.Notes = new Xsd.NotesNoteCollection();

			TBusinessObject consolBizObj = NewBusinessObject();
			StmNote note1 = consolBizObj.Notes.AddNew();
			note1.ST_NoteType = "PUB";
			note1.ST_NoteText = "Hello Chello";
			note1.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			adapter.ExportToValueObject(consolBizObj, consol1, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, consol1.Notes.Count);
		}

		public void TestUniqueConsignRefPutInAgentReference()
		{
			TBusinessObject consol = NewBusinessObject();
			consol.JK_UniqueConsignRef = "12345";
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol consolValue = (Xsd.Consol)adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Should populate the agent reference with the unique consign ref", "12345", consolValue.ConsolDetail.AgentReference);
		}

		public void TestAgentReferencePutInExternalAgentReference()
		{
			TBusinessObject consol = NewBusinessObject();
			consol.JK_AgentsReference = "987654";
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol consolValue = (Xsd.Consol)adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Should Populate the ExternalAgentReference with the JK_AgentsReference", "987654", consolValue.ConsolDetail.ExternalAgentReference);
		}

		#region Find Tests

		public void TestFindBusinessObject()
		{
			TBusinessObject consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			consol.JK_MasterBillNum = "masterbill";
			CommonConsol consolWithNoMasterBill = GetNewConsolWithValidTestDataPopulatedByFactory();
			consolWithNoMasterBill.JK_MasterBillNum = "";

			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol value = new Xsd.Consol();
			value.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = value.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = "masterbill";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			TBusinessObject foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the consol with the same master bill", foundConsol.PK, consol.PK);
			consol.JK_MasterBillNum = "";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should find no consol", foundConsol);
		}

		public void TestFindBusinessObject_WithAgentReferenceNoMasterBill()
		{
			TBusinessObject consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			consol.JK_MasterBillNum = "";
			consol.JK_AgentsReference = "agentref";

			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol value = new Xsd.Consol();
			value.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = value.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = "";
			value.ConsolDetail = new Xsd.ConsolConsolDetail();
			value.ConsolDetail.AgentReference = "agentref";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the consol with the same agent reference", foundConsol.PK, consol.PK);
			consol.JK_AgentsReference = "";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should find no consol", foundConsol);
		}

		public void TestFindBusinessObject_WithMasterBillOrAgentReference()
		{
			TBusinessObject consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			consol.JK_MasterBillNum = "";
			consol.JK_AgentsReference = "agentref";

			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol value = new Xsd.Consol();
			value.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = value.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			identifier.Value = "masterbill";
			value.ConsolDetail = new Xsd.ConsolConsolDetail();
			value.ConsolDetail.AgentReference = "agentref";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the consol with the same agent reference", foundConsol.PK, consol.PK);
			consol.JK_AgentsReference = "";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should find no consol", foundConsol);

			consol.JK_AgentsReference = "agentref";
			TBusinessObject otherConsol = GetNewConsolWithValidTestDataPopulatedByFactory();
			otherConsol.JK_MasterBillNum = "masterbill";
			otherConsol.JK_AgentsReference = "agentref";

			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the consol with the same master bill", foundConsol.PK, otherConsol.PK);

			otherConsol.JK_AgentsReference = "";

			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the consol with the same master bill again", foundConsol.PK, otherConsol.PK);

			consol.JK_MasterBillNum = "othermaster";
			otherConsol.JK_MasterBillNum = "othermaster1";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should not find consol with same agent reference but different master bill", foundConsol);
		}

		public void TestFindBusinessObject_ForAir()
		{
			CommonConsol consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "GBLON";
			consol.JK_RL_NKDischargePort = "AUSYD";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF109";

			consol.JK_MasterBillNum = "081";

			CommonConsol decoyConsol = GetNewConsolWithValidTestDataPopulatedByFactory();
			decoyConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			decoyConsol.JK_MasterBillNum = "081";
			decoyConsol.JK_RL_NKLoadPort = "NLAMS";
			decoyConsol.JK_RL_NKDischargePort = "AUBNE";

			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol value = new Xsd.Consol();
			value.ConsolDetail = new Xsd.ConsolConsolDetail();
			value.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = value.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			value.ConsolDetail.Item = flight;

			value.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
			value.ConsolDetail.TransportModeSpecified = true;
			identifier.Value = "081";
			flight.FlightNoJourneyNoTruckRegNo = transport.JW_VoyageFlight;
			value.ConsolDetail.PortOfLoading = new Xsd.Movement();
			value.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Now.ToDateTime();
			value.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "GBLON");

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the correct air consol", foundConsol.PK, consol.PK);
			consol.JK_RL_NKLoadPort = "";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should find no consol", foundConsol);
		}

		public void TestFindBusinessObject_ForSea()
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			CommonConsol consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "masterbill";
			consol.JK_RL_NKLoadPort = "NLAMS";
			consol.JK_RL_NKDischargePort = "AUBNE";

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_VoyageFlight = "voyage";

			CommonConsol decoyConsol = GetNewConsolWithValidTestDataPopulatedByFactory();
			decoyConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			decoyConsol.JK_MasterBillNum = "masterbill";
			decoyConsol.JK_RL_NKLoadPort = "SGSIN";
			decoyConsol.JK_RL_NKDischargePort = "AUSYD";

			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Xsd.Consol value = new Xsd.Consol();
			value.ConsolDetail = new Xsd.ConsolConsolDetail();
			value.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier identifier = value.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.ConsolIdentifierTypeSpecified = true;
			Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
			value.ConsolDetail.Item = sailing;

			identifier.Value = "masterbill";
			value.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			value.ConsolDetail.TransportModeSpecified = true;
			sailing.VesselName = transport.JW_Vessel;
			sailing.VoyageNo = transport.JW_VoyageFlight;
			value.ConsolDetail.PortOfLoading = new Xsd.Movement();
			value.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Now.ToDateTime();
			value.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "NLAMS");

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			TBusinessObject foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertEquals("Should find the correct sea consol", foundConsol.PK, consol.PK);
			consol.JK_RL_NKLoadPort = "";
			foundConsol = FindBusinessObjectWithAdapter(adapter, value, importContext);
			AssertNull("Should find no consol", foundConsol);
		}

		protected virtual ConsolValueObjectDataAdapter<TBusinessObject, CommonShipment, Xsd.Consol> GetNewConsolValueObjectDataAdapter()
		{
			return new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
		}

		TBusinessObject FindBusinessObjectWithAdapter(IValueObjectDataAdapter adapter, IValueObject value, ValueObjectImportContext context)
		{
			MethodInfo method = adapter.GetType().GetMethod("FindBusinessObject", BindingFlags.NonPublic | BindingFlags.Instance);
			return (TBusinessObject)method.Invoke(adapter, new object[] { value, context });
		}

		#endregion

		public virtual void TestOrganisationTypeOnImport()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.SendingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "SendingAgent", OrganisationTypes.Forwarder);
			consolValue.ConsolDetail.ReceivingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "ReceivingAgent", OrganisationTypes.Forwarder);
			consolValue.ConsolDetail.Carrier = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Carrier", OrganisationTypes.Carrier);
			consolValue.ConsolDetail.Creditor = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Creditor", OrganisationTypes.Creditor);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			TBusinessObject importedConsol = new ConsolValueObjectDataAdapterForTest<TBusinessObject>().CreateOrUpdateFromValueObject(consolValue, context);
			AssertEquals("SendingAgent", "SendingAgent", importedConsol.SendingForwarder.OH_FullName);
			AssertEquals("SendingAgent is forwarder", true, importedConsol.SendingForwarder.OH_IsForwarder);
			AssertEquals("ReceivingAgent", "ReceivingAgent", importedConsol.ReceivingForwarder.OH_FullName);
			AssertEquals("ReceivingAgent is forwarder", true, importedConsol.ReceivingForwarder.OH_IsForwarder);
			AssertEquals("Carrier", "Carrier", importedConsol.ShippingLine.OH_FullName);
			AssertEquals("Carrier is shipping provider", true, importedConsol.ShippingLine.OH_IsShippingProvider);
			AssertEquals("Creditor", "Creditor", importedConsol.Creditor.OH_FullName);
			AssertEquals("Creditor is Creditor", true, importedConsol.Creditor.OH_IsCreditor);
		}

		public void TestImportMasterbillIssueDate()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.MasterBillIssueDate = ZDateTime.Empty;
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			NotificationBuffer buffer = new NotificationBuffer();
			CommonConsol consol = adapter.CreateOrUpdateFromValueObject(consolValue, new ValueObjectImportContext(Factory, buffer));
			AssertEquals("master bill issue date", ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			buffer.Clear();
			consolValue.ConsolDetail.MasterBillIssueDate = ZDateTime.Now;
			consol = adapter.CreateOrUpdateFromValueObject(consolValue, new ValueObjectImportContext(Factory, buffer));
			AssertEquals("master bill issue date is imported", consolValue.ConsolDetail.MasterBillIssueDate, consol.JK_MasterBillIssueDate);
		}

		public void TestExportMasterbillIssueDate()
		{
			CommonConsol commonConsol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			commonConsol.JK_MasterBillIssueDate = ZDateTime.Now;

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectExportContext context = new ValueObjectExportContext(buffer);
			ConsolValueObjectDataAdapterForTest<CommonConsol> dataAdapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue = dataAdapter.ExportToValueObject(commonConsol, context);

			AssertEquals("Master bill issue date is exported", commonConsol.JK_MasterBillIssueDate, consolValue.ConsolDetail.MasterBillIssueDate);
		}

		public void TestExportCargoCarrierCode()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_RL_NKLoadPort = "USLAX";

			var sailingsHelper = new SailingsForTestClasses(Factory);

			var transportLeg = consol.Transports.AddNew();
			transportLeg.JW_RL_NKLoadPort = "USLAX";
			transportLeg.JW_RL_NKDiscPort = "AUMEL";
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg.JW_IsLinked = true;
			transportLeg.JW_JX = sailingsHelper.LaxMelSailing.PK;

			var vessel = sailingsHelper.LaxMelSailing.Vessel;
			if (vessel.RV_CarrierCode.IsEmpty)
			{
				vessel.RV_CarrierCode = "1234";
			}

			var notify = new NotificationBuffer();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			var consolXSD = new ConsolValueObjectDataAdapterForTest<CommonConsol>().ExportToValueObject(consol, context);

			var sailingXSD = consolXSD.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;
			AssertNotNull("sailing is not null", sailingXSD);
			AssertEquals("Vessel Name", vessel.RV_Name, sailingXSD.VesselName);
			AssertEquals("Cargo Carrier Code", vessel.RV_CarrierCode, sailingXSD.CargoCarrierCode);
		}

		public void TestOrganisationsNotImportedIfRegistryItemNotSet()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.Departure.ContainerYard = CreateDummyAddressReferenceValue("container park");
			consolValue.ConsolDetail.Departure.CTO = CreateDummyAddressReferenceValue("cto");
			consolValue.ConsolDetail.Departure.Depot = CreateDummyAddressReferenceValue("depot");

			SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol importedConsol = adapter.CreateOrUpdateFromValueObject(consolValue, context);
			AssertNull("Should not import the container park when the flag is false", importedConsol.ContainerYardEmptyPickupAddress);
			AssertNull("Should not import the cto when the flag is false", importedConsol.DepartureCTOAddress);
			AssertNull("Should not import the depot when the flag is false", importedConsol.PackDepotAddress);

			SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			importedConsol = adapter.CreateOrUpdateFromValueObject(consolValue, context);
			AssertNotNull("Should import the container park when the flag is true", importedConsol.ContainerYardEmptyPickupAddress);
			AssertNotNull("Should import the cto when the flag is true", importedConsol.DepartureCTOAddress);
			AssertNotNull("Should import the depot when the flag is true", importedConsol.PackDepotAddress);
		}

		public void TestAddImportEvent()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			Xsd.Consol xsdConsol = new Xsd.Consol();
			xsdConsol.ConsolDetail.AgentReference = "12345678";
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol importedConsol = adapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			StmALog[] dataImportEvents = importedConsol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new consol", 1, dataImportEvents.Length);

			Factory.Save();

			xsdConsol.ConsolDetail.ConsolType = Xsd.ConsolType.Other;
			xsdConsol.ConsolDetail.ConsolTypeSpecified = true;
			importedConsol = adapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			dataImportEvents = importedConsol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to existing consol", 2, dataImportEvents.Length);
		}

		public void TestAddExportEvent()
		{
			IValueObjectDataAdapter adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			TBusinessObject consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			StmALog[] dataExportEvents = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to consol", 0, dataExportEvents.Length);

			adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to consol", 1, dataExportEvents.Length);

			adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to consol", 2, dataExportEvents.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporteDocs()
		{
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			Xsd.Consol xSDConsol = new Xsd.Consol();
			Xsd.Document document = xSDConsol.Documents.AddNew();
			document.Data = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			document.DataType = "TIF";
			document.Date = new ZDateTime(2005, 12, 6);
			document.Description = "Masterbill";
			document.DocumentType = "MBL";
			document.IsPublished = Xsd.TrueFalse.@true;

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol consol = adapter.CreateOrUpdateFromValueObject(xSDConsol, importContext);

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IStorageMainForPK documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(Factory);
			IDocumentsView storageMain = documentFactory.GetStorageMain(consol.PK);
			AssertNotNull("Consol should have a StorageMain", storageMain);
			AssertEquals("StorageMain should have 1 StorageDocs", 1, storageMain.DocumentCollectionView.Count);
			BusinessObject storageDoc = ((IBusinessObjectCollectionView)storageMain.DocumentCollectionView).ToArray()[0];
			AssertEquals("Date Type", "TIF", storageDoc[StorageDocsSchema.SC_DataType.Name]);
			AssertEquals("Data", document.Data, storageDoc[StorageDocsSchema.SC_ImageData.Name]);
			AssertEquals("Date", new ZDateTime(2005, 12, 6), storageDoc[StorageDocsSchema.SC_Date.Name]);
			AssertEquals("Description", "Masterbill", storageDoc[StorageDocsSchema.SC_Desc.Name]);
			AssertEquals("Document Type", "MBL", storageDoc[StorageDocsSchema.SC_DocType.Name]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporteDocs_IncludeeDocsIsFalse()
		{
			TBusinessObject consol = SetUpStorageMainAndStorageDocs();
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			SystemDataRegistry.Instance.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Xsd.Consol result = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Documents should NOT be specified", false, result.Documents.IsSpecified);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporteDocs_IncludeeDocsIsTrue()
		{
			TBusinessObject consol = SetUpStorageMainAndStorageDocs();
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			SystemDataRegistry.Instance.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Xsd.Consol result = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Documents count should be 1", 2, result.Documents.Count);
			AssertEquals("DataType", "TIF", result.Documents[0].DataType);
			AssertEquals("Date", new ZDateTime(2005, 10, 11), result.Documents[0].Date);
			AssertEquals("Document Type", "MBL", result.Documents[0].DocumentType);
			AssertEquals("Data", true, result.Documents[0].Data.Length > 1000);
			AssertEquals("Description", "Testing Consol", result.Documents[0].Description);

			AssertEquals("DataType", "PDF", result.Documents[1].DataType);
			AssertEquals("Date", new ZDateTime(2005, 10, 11), result.Documents[1].Date);
			AssertEquals("Document Type", "QUO", result.Documents[1].DocumentType);
			AssertEquals("Data", true, result.Documents[1].Data.Length > 1000);
			AssertEquals("Description", "Testing Consol 2", result.Documents[1].Description);
		}

		public void TestExporteDocs_ConsolHasNoStorageMain()
		{
			TBusinessObject consol = GetNewConsolWithValidTestDataPopulatedByFactory();
			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			SystemDataRegistry.Instance.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Xsd.Consol result = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Documents count should be 0", 0, result.Documents.Count);
		}

		public void TestDropModeDoesNotDefaultOnImport()
		{
			TBusinessObject consol = Factory.New<TBusinessObject>();

			consol.JK_MasterBillNum = "CONSOL1";
			consol.JK_AgentType = "DRT";
			consol.JK_ConsolMode = "FCL";

			CommonShipment shipment = consol.Shipments.AddNew();

			ZQuery orgQuery = new ZQuery();
			shipment.ConsigneePK = Consignee.PK;
			orgQuery.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shipment.ConsigneePK);
			shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(orgQuery).PK;

			OrgAddress pickupAddress = shipment.ConsignorPickupAddress.Address;
			pickupAddress.OA_FCLEquipmentNeeded = "XYZ";

			OrgAddress deliveryAddress = shipment.ConsigneeDeliveryAddress.Address;
			deliveryAddress.OA_FCLEquipmentNeeded = "ABC";

			shipment.JS_HouseBill = "SHIPMENT1";
			shipment.JS_PackingMode = "LCL";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "";
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "";

			Factory.Save();

			ConsolValueObjectDataAdapterForTest<TBusinessObject> adapter = new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
			Xsd.Consol export = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonConsol importConsol = adapter.CreateOrUpdateFromValueObject(export, importContext);
			CommonShipment importShipment = importConsol.Shipments[0];

			AssertEquals("Master Bill", "CONSOL1", importConsol.JK_MasterBillNum);
			AssertEquals("Consol mode", "FCL", importConsol.JK_ConsolMode);

			AssertEquals("House bill", "SHIPMENT1", importShipment.JS_HouseBill);
			AssertEquals("Packing mode", "LCL", importShipment.JS_PackingMode);
			AssertEquals("Pickup Equipment should not default", "", importShipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("Delivery Equipment should not default", "", importShipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		TBusinessObject SetUpStorageMainAndStorageDocs()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(adminConnection, Db.DatabaseName + "_SD001");
			}

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = documentFactoryProvider.GetFactory(Factory);
			var storageDocsFactory = documentFactory.GetFactory(1);
			TBusinessObject consol = ((BusinessObjectFactory)documentFactory).NewWithValidTestData<TBusinessObject>();
			BusinessObject storageMain = (BusinessObject)((BusinessObjectFactory)documentFactory).New<IStorageMain>();
			BusinessObject storageDocs = (BusinessObject)storageDocsFactory.New<IStorageDocs>();
			BusinessObject storageFile = (BusinessObject)storageDocsFactory.New<IStorageFile>();

			storageMain[StorageMainSchema.SM_ParentFK.Name] = consol.PK;
			storageMain[StorageMainSchema.SM_DB.Name] = 1;
			storageDocs[StorageDocsSchema.SC_SM.Name] = storageMain.PK;
			storageFile[StorageDocsSchema.SC_SM.Name] = storageMain.PK;

			storageDocs[StorageDocsSchema.SC_DataType.Name] = "TIF";
			storageFile[StorageDocsSchema.SC_DataType.Name] = "PDF";
			ZDateTime documentDate = new ZDateTime(2005, 10, 11);
			storageDocs[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageDocs[StorageDocsSchema.SC_DocType.Name] = "MBL";
			storageFile[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageFile[StorageDocsSchema.SC_DocType.Name] = "QUO";
			SmallTifFileAsZBlob = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			ZBlob imageData = SmallTifFileAsZBlob;
			storageDocs[StorageDocsSchema.SC_ImageData.Name] = imageData;
			storageDocs[StorageDocsSchema.SC_Desc.Name] = "Testing Consol";
			storageDocs[StorageDocsSchema.SC_IsPublished.Name] = true;

			SmallTifFileAsZBlob = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Sample.PDF"));
			imageData = SmallTifFileAsZBlob;
			storageFile[StorageDocsSchema.SC_ImageData.Name] = imageData;
			storageFile[StorageDocsSchema.SC_Desc.Name] = "Testing Consol 2";
			storageFile[StorageDocsSchema.SC_IsPublished.Name] = true;

			documentFactory.Save();

			return consol;
		}
		ZBlob SmallTifFileAsZBlob;

		Xsd.AddressReference CreateDummyAddressReferenceValue(string description)
		{
			Xsd.AddressReference result = new Xsd.AddressReference();
			result.Organisation = new Xsd.Organisation();
			result.AddressSequenceRef = 1;
			result.Organisation.OrganisationDetails = new Xsd.OrganisationDetail();
			result.Organisation.OrganisationDetails.Name = description;
			result.Organisation.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address = result.Organisation.OrganisationDetails.Addresses.AddNew();
			address.Sequence = 1;
			address.SequenceSpecified = true;
			address.AddressLine1 = description + " - address line1";
			return result;
		}

		protected Xsd.Shipment CreateXsdShipment(Xsd.TransportMode transportMode, ZString consignorCode, ZString consigneeCode, ZString origin, ZString destination, ZString houseBill)
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentDetailsSpecified = true;
			shipment.ShipmentDetails.TransportMode = transportMode;

			shipment.ShipmentDetails.Consignor = new Xsd.Organisation { EDICode = consignorCode };
			shipment.ShipmentDetails.Consignee = new Xsd.Organisation { EDICode = consigneeCode };

			shipment.ShipmentDetails.PortOfOrigin.Port.Value = origin;
			shipment.ShipmentDetails.PortofDestination.Port.Value = destination;
			shipment.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.Housebill, houseBill);

			return shipment;
		}

		protected virtual TBusinessObject GetNewConsolWithValidTestDataPopulatedByFactory()
		{
			return Factory.NewWithValidTestData<TBusinessObject>();
		}

		protected OrgHeader Consignor
		{
			get { return consignor ?? (consignor = Factory.LoadTop1<OrgHeader>(new ZQuery())); }
		}
		OrgHeader consignor;

		protected OrgHeader Consignee
		{
			get { return consignee ?? (consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Consignor.PK))); }
		}
		OrgHeader consignee;

		#endregion

		#region consol specific adapter and element names

		protected override ValueObjectDataAdapter<TBusinessObject, Xsd.Consol> GetNewBizObjXmlDataAdapter()
		{
			return new ConsolValueObjectDataAdapterForTest<TBusinessObject>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Consols"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Consol"; }
		}

		#endregion

		#region populated consols and expected outputs

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			TBusinessObject emptyConsol = NewBusinessObject();
			emptyConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport = emptyConsol.Transports[0];
			transport.JW_VoyageFlight = "voyage";

			emptyConsol.JK_UniqueConsignRef = "";
			emptyConsol.JK_AgentsReference = "";
			var emptyConsolXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyConsol.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyConsol, emptyConsolXmlPath, ValidationKind.None, "Empty consol");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			GlbDepartment.CurrentDepartment.GE_Air = false;
			GlbDepartment.CurrentDepartment.GE_Rail = false;
			GlbDepartment.CurrentDepartment.GE_Road = false;
			GlbDepartment.CurrentDepartment.GE_Sea = false;

			var populatedConsolWithEmptyFields = Factory.NewWithValidTestData<TBusinessObject>(TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply);
			populatedConsolWithEmptyFields.Containers[0].JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20PL").PK;

			populatedConsolWithEmptyFields.Shipments.RemoveAndDeleteAll();
			populatedConsolWithEmptyFields.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = populatedConsolWithEmptyFields.Transports[0];
			transport.JW_VoyageFlight = "voyage";

			populatedConsolWithEmptyFields.JK_UniqueConsignRef = "";
			populatedConsolWithEmptyFields.JK_AgentsReference = "";

			populatedConsolWithEmptyFields.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.SendingForwarder.OH_FullName = "SendingForwarder";
			populatedConsolWithEmptyFields.SendingForwarder.MainAddress.OA_Address1 = "SendingForwarder";

			populatedConsolWithEmptyFields.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.ReceivingForwarder.OH_FullName = "ReceivingForwarder";
			populatedConsolWithEmptyFields.ReceivingForwarder.MainAddress.OA_Address1 = "ReceivingForwarder";

			populatedConsolWithEmptyFields.JK_OA_ShippingLineAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.ShippingLine.OH_FullName = "ShippingLine";
			populatedConsolWithEmptyFields.ShippingLine.MainAddress.OA_Address1 = "ShippingLine";
			populatedConsolWithEmptyFields.ShippingLine.PrimaryRegistrationNumber.Number = "ShippingLine";

			populatedConsolWithEmptyFields.JK_OA_CreditorAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.Creditor.OH_FullName = "Creditor";
			populatedConsolWithEmptyFields.Creditor.MainAddress.OA_Address1 = "Creditor";
			populatedConsolWithEmptyFields.Creditor.PrimaryRegistrationNumber.Number = "Creditor";

			populatedConsolWithEmptyFields.JK_OA_ContainerYardEmptyPickupAddress = populatedConsolWithEmptyFields.ContainerYardEmptyPickupAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ContainerYardEmptyPickupAddress.OA_Address1 = "CNT Yard Address";
			populatedConsolWithEmptyFields.JK_OA_ContainerYardEmptyReturnAddress = populatedConsolWithEmptyFields.ContainerYardEmptyReturnAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ContainerYardEmptyReturnAddress.OA_Address1 = "CNT Yard Address";
			populatedConsolWithEmptyFields.JK_OA_DepartureCTOAddress = populatedConsolWithEmptyFields.DepartureCTOAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.DepartureCTOAddress.OA_Address1 = "Departure CTO Address";
			populatedConsolWithEmptyFields.JK_OA_ArrivalCTOAddress = populatedConsolWithEmptyFields.ArrivalCTOAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ArrivalCTOAddress.OA_Address1 = "Arrival CTO Address";
			populatedConsolWithEmptyFields.JK_OA_UnpackDepotAddress = populatedConsolWithEmptyFields.UnpackDepotAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.UnpackDepotAddress.OA_Address1 = "Unpack Depot Address";
			populatedConsolWithEmptyFields.JK_OA_PackDepotAddress = populatedConsolWithEmptyFields.PackDepotAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.PackDepotAddress.OA_Address1 = "Pack Depot Address";
			populatedConsolWithEmptyFields.Containers[0].JC_OA_DepartureContainerYardAddress = populatedConsolWithEmptyFields.Containers[0].DepartureContainerYardAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.Containers[0].JC_OA_ArrivalContainerYardAddress = populatedConsolWithEmptyFields.Containers[0].ArrivalContainerYardAddress.Header.MainAddress.PK;

			populatedConsolWithEmptyFields.Numbers.RemoveAll();
			return new BusinessObjectAndExpectedOutputFileName(populatedConsolWithEmptyFields, FullyPopulatedConsolWithEmptyFieldsPath, ValidationKind.None, "Populated consol with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var consol = NewConsolWithValidTestData(Constants.TransportModes.Air, Constants.PaymentType.Prepaid);
			return new BusinessObjectAndExpectedOutputFileName(consol, FullyPopulatedAirSamplePath, ValidationKind.Xsd | ValidationKind.FactorySave, "Air");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var result = new ArrayList();
			result.Add(new BusinessObjectAndExpectedOutputFileName(NewConsolWithValidTestData(Constants.TransportModes.Sea, Constants.PaymentType.Prepaid), FullyPopulatedSeaSamplePath, ValidationKind.Xsd | ValidationKind.FactorySave, "Sea"));
			result.Add(new BusinessObjectAndExpectedOutputFileName(NewConsolWithValidTestData(Constants.TransportModes.Rail, Constants.PaymentType.Collect), FullyPopulatedRailSamplePath, ValidationKind.Xsd | ValidationKind.FactorySave, "Rail"));
			return (BusinessObjectAndExpectedOutputFileName[])result.ToArray(typeof(BusinessObjectAndExpectedOutputFileName));
		}

		protected virtual string FullyPopulatedAirSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.AirConsol.xml");

		protected virtual string FullyPopulatedConsolWithEmptyFieldsPath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedConsolWithEmptyFields.xml");

		protected virtual string FullyPopulatedSeaSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.SeaConsol.xml");

		protected virtual string FullyPopulatedRailSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.RailConsol.xml");

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		TBusinessObject NewConsolWithValidTestData(string transportMode, string prepaidCollect)
		{
			var consol = (CommonConsol)NewBusinessObject();
			consol.Logs.AddNew(Events.Booked, "Booked Reference", new ZDateTimeOffset(2005, 3, 3));
			consol.Notes.AddNew(true, "MyNote", "Note Text");

			consol.JK_MasterBillNum = "masterbill";
			consol.JK_TransportMode = transportMode;
			consol.JK_PrepaidCollect = prepaidCollect;
			consol.JK_RL_NKLoadPort = "MYPKG";
			consol.JK_RL_NKDischargePort = "AUPER";

			var transport = consol.Transports[0];
			transport.JW_LegOrder = 1;
			transport.JW_Vessel = "ADMIRALENGRACHT";
			transport.JW_VoyageFlight = "voyageno";
			transport.JW_RL_NKLoadPort = "MYPKG";
			transport.JW_ETD = new ZDateTime(2005, 1, 1);
			transport.JW_ATD = new ZDateTime(2005, 1, 1);
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETA = new ZDateTime(2005, 1, 2);
			transport.JW_ATA = new ZDateTime(2005, 1, 2);

			if (transport.JW_TransportMode == Constants.TransportModes.Sea)
			{
				var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "MAERSK"));
				if (carrier == null)
				{
					carrier = new VoyageTestHelper(Factory).CreateCarrier("MAERSK");
					carrier.MainAddress.OA_Address1 = "MAERSK Address";
				}
				transport.CarrierPK = carrier.PK;
			}

			if (transport.JW_TransportMode != Constants.TransportModes.Rail)
			{
				transport.Sailing.Origin.JA_Berth = "1";
				transport.Sailing.Destination.JB_Berth = "2";
				transport.Sailing.Origin.JA_DocumentaryCutoff = new ZDateTime(2005, 1, 10);
			}

			var seaTransport = consol.Transports.AddNew();
			seaTransport.JW_LegOrder = 3;
			seaTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			seaTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			seaTransport.JW_RL_NKLoadPort = "AUMEL";
			seaTransport.JW_ETD = new ZDateTime(2005, 1, 3);
			seaTransport.JW_RL_NKDiscPort = "AUPER";
			seaTransport.JW_ETA = new ZDateTime(2005, 1, 4);
			seaTransport.JW_Vessel = "ANADYR";
			seaTransport.JW_VoyageFlight = "trans_voy";
			seaTransport.JW_ATA = new ZDateTime(2005, 1, 4);

			var railTransport = consol.Transports.AddNew();
			railTransport.JW_LegOrder = 2;
			railTransport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			railTransport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			railTransport.JW_RL_NKLoadPort = "AUSYD";
			railTransport.JW_ETD = new ZDateTime(2005, 1, 2);
			railTransport.JW_RL_NKDiscPort = "AUMEL";
			railTransport.JW_ETA = new ZDateTime(2005, 1, 3);
			railTransport.JW_Vessel = "ANADYR";
			railTransport.JW_VoyageFlight = "trans_voy";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = "FCL";
			container.JC_ContainerNum = "cont123";
			container.JC_SealNum = "sealnum";
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
			container.JC_ReleaseNum = "BookingRef";
			container.JC_RH_NKContainerCommodityCode = "ABCD";

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.Consignee.OH_Code = "SHPCNE";
			shipment.Consignee.OH_FullName = "Consignee for Shipment";
			shipment.Consignee.MainAddress.OA_Address1 = "Consignee for Shipment";
			shipment.JS_E_DEP = new ZDateTime(2005, 1, 1);
			shipment.JS_E_ARV = new ZDateTime(2005, 2, 3);
			shipment.JS_HouseBill = "housebill";
			shipment.JS_PackingMode = transportMode;
			shipment.JS_HouseBillIssueDate = new ZDateTime(2005, 1, 1);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			if (transportMode == Core.Constants.TransportModes.Air)
			{
				consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			}
			else
			{
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			}

			consol.JK_AgentsReference = "";
			consol.JK_BookingReference = "bookingref";

			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.SendingForwarder.OH_Code = "SNDFOR";
			consol.SendingForwarder.OH_FullName = "SendingForwarder";
			consol.SendingForwarder.MainAddress.OA_Address1 = "SendingForwarder";
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.ReceivingForwarder.OH_Code = "RCVFOR";
			consol.ReceivingForwarder.OH_FullName = "ReceivingForwarder";
			consol.ReceivingForwarder.MainAddress.OA_Address1 = "ReceivingForwarder";

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsCreditor = true;
			shippingLine.OH_Code = "SHPLIN";
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.MainAddress.OA_Address1 = "ShippingLine";

			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			if (transportMode == Constants.TransportModes.Rail)
			{
				consol.JK_OA_CreditorAddress = shippingLine.MainAddress.PK;
			}

			consol.JK_DateFirstForeignPort = new ZDateTime(2005, 2, 1);
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2005, 2, 2);
			consol.JK_DateLastForeignPort = new ZDateTime(2005, 2, 3);
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_RL_NKLastForeignPort = "AUMEL";
			consol.JK_RL_NKFirstForeignPort = "AUBBE";

			consol.JK_OA_ArrivalCTOAddress = NewTestAddress("ArrivalCTOAddress").PK;
			consol.JK_OA_UnpackDepotAddress = NewTestAddress("UnpackDepotAddress").PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = NewTestAddress("ReturnContainerYardEmptyAddress").PK;

			consol.JK_OA_DepartureCTOAddress = NewTestAddress("DepartureCTOAddress").PK;
			consol.JK_OA_PackDepotAddress = NewTestAddress("PackDepotAddress").PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = NewTestAddress("PickupContainerYardEmptyAddress").PK;

			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_NoOriginalBills = 3;
			consol.JK_NoCopyBills = 3;

			CusEntryNumber number = consol.Numbers.AddNew();
			number.CE_EntryType = "AAA";
			number.CE_EntryNum = "1234567";
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Zimbabwe;

			return (TBusinessObject)consol;
		}

		protected OrgAddress NewTestAddress(string addressDescription)
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = addressDescription;
			organisation.MainAddress.OA_Address1 = addressDescription;
			return organisation.MainAddress;
		}

		#endregion

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// covered in other data adapters
					"Events",
					"ConsolDetail/Schedule/Consols",
					"ConsolDetail/Voyage",
					"ConsolDetail/Voyage/Sailings",
					"ConsolDetail/Arrival/CTO/Organisation",
					"ConsolDetail/Arrival/Depot/Organisation",
					"ConsolDetail/Arrival/ContainerYard/Organisation",
					"ConsolDetail/Departure/CTO/Organisation",
					"ConsolDetail/Departure/Depot/Organisation",
					"ConsolDetail/Departure/ContainerYard/Organisation",

					"ConsolDetail/Containers/EstimatedDelivery",
					"ConsolDetail/Containers/IsShipperOwnedContainer",
					"ConsolDetail/Containers/LCLAvailable",
					"ConsolDetail/Containers/FCLAvailable",
					"ConsolDetail/Containers/ImportProcess",
					"ConsolDetail/Containers/ExportProcess",
					"ConsolDetail/Containers/Custom",
					"ConsolDetail/Containers/SetPointTemperature",
					"ConsolDetail/Containers/SetPointTemperatureUnit",
					"ConsolDetail/Containers/HumidityPercent",
					"ConsolDetail/Containers/AirVentFlow",
					"ConsolDetail/Containers/AirVentFlowRateUnit",
					"ConsolDetail/Containers/BookingReference",

					"ConsolDetail/CoLoadWith",
					"ConsolDetail/SendingAgent",
					"ConsolDetail/ReceivingAgent",
					"ConsolDetail/Carrier",
					"ConsolDetail/Containers/Seal2",
					"ConsolDetail/Containers/Seal3",
					"ConsolDetail/Creditor",
					"Shipments",
					"ConsolDetail/Schedule/LoadPort",
					"ConsolDetail/Schedule/DischargePort",
					"ConsolDetail/Item/DepartureCTO/Organisation",
					"ConsolDetail/Item/ArrivalCTO/Organisation",
					"ConsolDetail/Item/IsTranshipment",
					"ConsolDetail/Item/IsPublished",
					"ConsolDetail/Item/LoadPortETA",
					"ConsolDetail/Item/LoadPortATA",
					"ConsolDetail/PlannedLegs/Item/IsTranshipment",
					"ConsolDetail/PlannedLegs/Item/IsPublished",
					"ConsolDetail/PlannedLegs/Item/DepartureCTO",
					"ConsolDetail/PlannedLegs/Item/ArrivalCTO",
					"ConsolDetail/PlannedLegs/Item/LoadPortETA",
					"ConsolDetail/PlannedLegs/Item/LoadPortATA",

					"Notes",
					"ConsolDetail/DateCreated",

					"ConsolDetail/PlannedLegs/PortOfLoading/ActualDateTime", // this node isn't suppose to be used
					"ConsolDetail/PlannedLegs/PortOfDischarge/ActualDateTime", // this node isn't suppose to be used
					"ConsolDetail/AgentReference", // covered in a separate test
					"ConsolDetail/ExternalAgentReference", // covered in a separate test
					"ConsolDetail/MasterBillIssueDate", //covered in a separate test
					"ConsolDetail/CustomsEntryNumbers/Number",
					"ConsolDetail/CustomsEntryNumbers/Country",
					"ConsolDetail/CustomsEntryNumbers/Type",
					"ConsolDetail/Addresses",

					"Documents",

					"ARInvoices",

					"CustomValues",
					"ConsolDetail/CustomValues",
					"AWBHeaders"
				};
			}
		}

		protected override TBusinessObject NewBusinessObject()
		{
			return Factory.New<TBusinessObject>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
