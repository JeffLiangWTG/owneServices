using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdPlannedLegObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var consol = Factory.New<CommonConsol>();

			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "BLAH";
			sailing1.LloydsNo = vessel.RV_LloydsNumber;
			sailing1.VoyageNo = "12345";

			var sailing2 = new Xsd.SailingForPlannedLegs();
			sailing2.VesselName = vessel.RV_Name;
			sailing2.LloydsNo = "99999";
			sailing2.VoyageNo = "54321";

			var sailing3 = new Xsd.SailingForPlannedLegs();
			sailing3.VesselName = "BLAH";
			sailing3.LloydsNo = "99999";
			sailing3.VoyageNo = "22222";

			var sailing4 = new Xsd.SailingForPlannedLegs();
			sailing4.LloydsNo = "99999";
			sailing4.VoyageNo = "44444";

			var plannedLegs = new Xsd.PlannedLegCollection();

			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;

			var leg2 = plannedLegs.AddNew();
			leg2.Item = sailing2;
			leg2.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;

			var leg3 = plannedLegs.AddNew();
			leg3.Item = sailing3;
			leg3.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;

			var leg4 = plannedLegs.AddNew();
			leg4.Item = sailing4;
			leg4.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals(4, consol.Transports.Count);
			AssertEquals(vessel.RV_FK, consol.Transports[0].JW_Vessel);
			AssertEquals(vessel.RV_FK, consol.Transports[1].JW_Vessel);
			AssertEquals("BLAH", consol.Transports[2].JW_Vessel);
			AssertEquals("99999", consol.Transports[3].JW_Vessel);
		}

		public void TestImportVesselWithLegOrderNumber()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var consol = Factory.New<CommonConsol>();

			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "BLAH";
			sailing1.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing1.VoyageNo = "12345";

			var sailing2 = new Xsd.SailingForPlannedLegs();
			sailing2.VesselName = existingVessel.RV_Name;
			sailing2.LloydsNo = "99999";
			sailing2.VoyageNo = "54321";

			var sailing3 = new Xsd.SailingForPlannedLegs();
			sailing3.VesselName = "BLAH";
			sailing3.LloydsNo = "99999";
			sailing3.VoyageNo = "22222";

			var sailing4 = new Xsd.SailingForPlannedLegs();
			sailing4.LloydsNo = "99999";
			sailing4.VoyageNo = "44444";

			var plannedLegs = new Xsd.PlannedLegCollection();

			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg1.LegOrderNumber = 2;

			var leg2 = plannedLegs.AddNew();
			leg2.Item = sailing2;
			leg2.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg2.LegOrderNumber = 3;

			var leg3 = plannedLegs.AddNew();
			leg3.Item = sailing3;
			leg3.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg3.LegOrderNumber = 4;

			var leg4 = plannedLegs.AddNew();
			leg4.Item = sailing4;
			leg4.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg4.LegOrderNumber = 1;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals(4, consol.Transports.Count);
			AssertEquals((byte)2, (byte)consol.Transports[0].JW_LegOrder);
			AssertEquals((byte)3, (byte)consol.Transports[1].JW_LegOrder);
			AssertEquals((byte)4, (byte)consol.Transports[2].JW_LegOrder);
			AssertEquals((byte)1, (byte)consol.Transports[3].JW_LegOrder);
		}

		public void TestImportVesselWithLegOrderNumber_Duplicate()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var consol = Factory.New<CommonConsol>();

			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "BLAH";
			sailing1.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing1.VoyageNo = "12345";

			var sailing2 = new Xsd.SailingForPlannedLegs();
			sailing2.VesselName = existingVessel.RV_Name;
			sailing2.LloydsNo = "99999";
			sailing2.VoyageNo = "54321";

			var sailing3 = new Xsd.SailingForPlannedLegs();
			sailing3.VesselName = "BLAH";
			sailing3.LloydsNo = "99999";
			sailing3.VoyageNo = "22222";

			var plannedLegs = new Xsd.PlannedLegCollection();

			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg1.LegOrderNumber = 1;

			var leg2 = plannedLegs.AddNew();
			leg2.Item = sailing2;
			leg2.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg2.LegOrderNumber = 1;

			var leg3 = plannedLegs.AddNew();
			leg3.Item = sailing3;
			leg3.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg3.LegOrderNumber = 2;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals(3, consol.Transports.Count);
			AssertEquals((byte)1, (byte)consol.Transports[0].JW_LegOrder);
			AssertEquals((byte)2, (byte)consol.Transports[1].JW_LegOrder);
			AssertEquals((byte)3, (byte)consol.Transports[2].JW_LegOrder);
		}

		public void TestImportVesselWithLegOrderNumber_GapWithManyLegs()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var consol = Factory.New<CommonConsol>();

			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "BLAH";
			sailing1.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing1.VoyageNo = "12345";

			var sailing2 = new Xsd.SailingForPlannedLegs();
			sailing2.VesselName = existingVessel.RV_Name;
			sailing2.LloydsNo = "99999";
			sailing2.VoyageNo = "54321";

			var sailing3 = new Xsd.SailingForPlannedLegs();
			sailing3.VesselName = "BLAH";
			sailing3.LloydsNo = "99999";
			sailing3.VoyageNo = "22222";

			var plannedLegs = new Xsd.PlannedLegCollection();

			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg1.LegOrderNumber = 1;

			var leg2 = plannedLegs.AddNew();
			leg2.Item = sailing2;
			leg2.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg2.LegOrderNumber = 3;

			var leg3 = plannedLegs.AddNew();
			leg3.Item = sailing3;
			leg3.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg3.LegOrderNumber = 4;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals(3, consol.Transports.Count);
			AssertEquals((byte)1, (byte)consol.Transports[0].JW_LegOrder);
			AssertEquals((byte)2, (byte)consol.Transports[1].JW_LegOrder);
			AssertEquals((byte)3, (byte)consol.Transports[2].JW_LegOrder);
		}

		public void TestImportVesselWithLegOrderNumber_GapWithOneLeg()
		{
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var consol = Factory.New<CommonConsol>();

			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "BLAH";
			sailing1.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing1.VoyageNo = "12345";

			var plannedLegs = new Xsd.PlannedLegCollection();

			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg1.LegOrderNumber = 2;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals(1, consol.Transports.Count);
			AssertEquals((byte)1, (byte)consol.Transports[0].JW_LegOrder);
		}

		public void TestImportLinkedPlannedLeg()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg plannedLegValue = plannedLegs.AddNew();
			Xsd.SailingForPlannedLegs sailing = new Xsd.SailingForPlannedLegs();
			sailing.VesselName = "nonexist" + Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Name;
			AssertNull(RefVessel.LookupVesselByName(sailing.VesselName, Factory).FirstOrDefault());
			sailing.VoyageNo = "12345";
			plannedLegValue.Item = sailing;
			plannedLegValue.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			plannedLegValue.TransportType = Enterprise.DataTransfer.Xml.XsdVersion1.PlannedLegTransportType.MainVessel;
			var portOfLoading = new Xsd.Movement();
			portOfLoading.Port.Value = "AUSUD";
			portOfLoading.EstimatedDateTime = ZDateTime.Now.AddDays(-1);
			var portOfDischarge = new Xsd.Movement();
			portOfDischarge.Port.Value = "USORD";
			portOfDischarge.EstimatedDateTime = ZDateTime.Now.AddDays(1);
			plannedLegValue.PortOfLoading = portOfLoading;
			plannedLegValue.PortOfDischarge = portOfDischarge;

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			Assert(!consol.Transports[0].JW_IsLinked);

			consol = Factory.New<CommonConsol>();

			sailing.VesselName = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Name;
			AssertNotNull(RefVessel.LookupVesselByName(sailing.VesselName, Factory).FirstOrDefault());
			plannedLegValue.Item = sailing;
			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			Assert("Should not link SEA transport when there is no carrier specified", !consol.Transports[0].JW_IsLinked);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			sailing.Carrier = new Xsd.Organisation() { EDICode = "CARRIER" };
			plannedLegValue.Item = sailing;

			consol = Factory.New<CommonConsol>();

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			Assert(consol.Transports[0].JW_IsLinked);
		}

		public void TestImportPlannedLegs_Sea_CarrierNotSpecified()
		{
			var plannedLegs = new Xsd.PlannedLegCollection();
			var plannedLegValue = plannedLegs.AddNew();
			plannedLegValue.TransportMode = Xsd.TransportMode.SEA;

			plannedLegValue.PortOfLoading = new Xsd.Movement()
			{
				Port = new Xsd.UNLOCO() { Value = "AUSYD" }
			};
			plannedLegValue.PortOfDischarge = new Xsd.Movement()
			{
				Port = new Xsd.UNLOCO() { Value = "NZAKL" }
			};

			var sailing = new Xsd.SailingForPlannedLegs();
			sailing.VesselName = "Visund";
			sailing.VoyageNo = "123";
			plannedLegValue.Item = sailing;

			var consol = Factory.New<CommonConsol>();

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals("No carrier specified and no existing sailing could be matched - not creating new sailing", false, consol.Transports[0].JW_IsLinked);

			var voyageQuery = new ZQuery(JobVoyageSchema.JV_AirSeaRoad, Constants.TransportModes.Sea);
			voyageQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, "Visund");
			voyageQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, "123");

			AssertNull("No new voyages should be created", Factory.LoadTop1<JobVoyage>(voyageQuery));

			var helper = new VoyageTestHelper(Factory);
			var voyage = helper.CreateSeaVoyage("Visund", "123", ZGuid.Empty, "AUSYD", "NZAKL");
			Factory.Save();

			consol = Factory.New<CommonConsol>();

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);
			AssertEquals("No carrier specified but existing sailing is matched", true, consol.Transports[0].JW_IsLinked);
			AssertEquals("No carrier specified but existing sailing is matched", voyage.Sailings[0].PK, consol.Transports[0].JW_JX);
		}

		public void TestImportPlannedLegs_Sea_Linked_CarrierIsUsedForMatching()
		{
			var plannedLegs = new Xsd.PlannedLegCollection();
			var plannedLegValue = plannedLegs.AddNew();
			plannedLegValue.TransportMode = Xsd.TransportMode.SEA;

			plannedLegValue.PortOfLoading = new Xsd.Movement()
			{
				Port = new Xsd.UNLOCO() { Value = "AUSYD" }
			};
			plannedLegValue.PortOfDischarge = new Xsd.Movement()
			{
				Port = new Xsd.UNLOCO() { Value = "NZAKL" }
			};

			var sailing = new Xsd.SailingForPlannedLegs();
			sailing.VesselName = "Visund";
			sailing.VoyageNo = "123";
			sailing.Carrier = new Xsd.Organisation() { EDICode = "CARRIER2" };

			plannedLegValue.Item = sailing;

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

			var consol = Factory.New<CommonConsol>();

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, new ValueObjectImportContext(Factory, new NotificationBuffer()), string.Empty);

			var expectedSailingToBeLinkedTo = voyage2.Sailings[0];

			AssertEquals("Carrier used in matching, transport linked to correct sailing", expectedSailingToBeLinkedTo.PK, consol.Transports[0].JW_JX);
			AssertEquals(true, consol.Transports[0].JW_IsLinked);
		}

		public void TestImportPlannedLegsIsNotProcessedForQuotedBookings()
		{
			var sailing1 = new Xsd.SailingForPlannedLegs();
			sailing1.VesselName = "HMS Winnebago";
			sailing1.LloydsNo = "99999";
			sailing1.VoyageNo = "12345";

			var plannedLegs = new Xsd.PlannedLegCollection();
			var leg1 = plannedLegs.AddNew();
			leg1.Item = sailing1;
			leg1.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			leg1.LegOrderNumber = 1;

			var consol = Factory.New<CommonConsol>();
			IValueObjectImportContext consolContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			consolContext.ImportingJob = consol;
			AssertEquals("Context should be importing for consol", consolContext.ImportingJob.TableName, JobConsolSchema.Constants.TableName);

			XsdPlannedLegObjectHelper.ImportPlannedLegs(consol.Transports, plannedLegs, consolContext, string.Empty);
			AssertEquals("Consol should have a planned leg imported", 1, consol.Transports.Count);

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			IValueObjectImportContext quotedBookingContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			quotedBookingContext.ImportingJob = (BusinessObject)quotedBooking;
			AssertEquals("Context should be importing for booking", quotedBookingContext.ImportingJob.TableName, ViewQuotedBookingSchema.Constants.TableName);

			var quotedBookingsShipment = (CommonShipment)quotedBooking.ForwardingShipment;
			XsdPlannedLegObjectHelper.ImportPlannedLegs(quotedBookingsShipment.Transports, plannedLegs, quotedBookingContext, string.Empty);
			AssertEquals("QuotedBooking should NOT have a planned leg imported", 0, quotedBookingsShipment.Transports.Count);
		}

		public void TestExportCargoCarrierCode()
		{
			var sailingsHelper = new SailingsForTestClasses(Factory);

			var transportLeg = Factory.New<Transport>();
			transportLeg.ParentType = typeof(CommonConsol);
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
			var plannedLegXsd = new Xsd.PlannedLeg();
			XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLegXsd, transportLeg, new ValueObjectExportContext(notify), string.Empty);
			var sailingXSD = plannedLegXsd.Item as Xsd.SailingForPlannedLegs;
			AssertNotNull("sailing XSD", sailingXSD);
			AssertEquals("Vessel Name", vessel.RV_Name, sailingXSD.VesselName);
			AssertEquals("Cargo Carrier Code", vessel.RV_CarrierCode, sailingXSD.CargoCarrierCode);
		}

		public void TestExportLegOrderNumber()
		{
			var sailingsHelper = new SailingsForTestClasses(Factory);

			var transportLeg = Factory.New<Transport>();
			transportLeg.ParentType = typeof(CommonConsol);
			transportLeg.JW_RL_NKLoadPort = "USLAX";
			transportLeg.JW_RL_NKDiscPort = "AUMEL";
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg.JW_IsLinked = true;
			transportLeg.JW_JX = sailingsHelper.LaxMelSailing.PK;
			transportLeg.JW_LegOrder = 1;

			var vessel = sailingsHelper.LaxMelSailing.Vessel;
			if (vessel.RV_CarrierCode.IsEmpty)
			{
				vessel.RV_CarrierCode = "1234";
			}

			var notify = new NotificationBuffer();
			var plannedLegXsd = new Xsd.PlannedLeg();
			XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLegXsd, transportLeg, new ValueObjectExportContext(notify), string.Empty);
			AssertEquals("Leg Order Number", (ZByte)1, plannedLegXsd.LegOrderNumber);
		}

		public void TestExport_Sea_LinkedTransport_CarrierIsExported()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "MAERSK";

			var voyage = new VoyageTestHelper(Factory).CreateSeaVoyage("OLIVIA", "123", carrier.PK);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;

			var plannedLegXsd = new Xsd.PlannedLeg();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLegXsd, transport, context, string.Empty);

			var sailingXSD = plannedLegXsd.Item as Xsd.SailingForPlannedLegs;
			AssertEquals("MAERSK", sailingXSD.Carrier.EDICode);
		}
	}
}
