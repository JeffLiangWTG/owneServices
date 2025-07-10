using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentPickupCartageTypeTest : TestCaseWithFactory
	{
		public void TestPickupCompleted_Loose()
		{
			CommonShipment shp = GetNewShipment();
			shp.JS_TransportMode = Constants.TransportModes.Air;
			PackLine p1 = shp.OuterPackLines.AddNew();
			PackLine p2 = shp.OuterPackLines.AddNew();
			p1.JL_PackageCount = 10;
			p2.JL_PackageCount = 20;

			ZDateTime now = ZDateTime.Now;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			AssertEquals(0, shp.PickupConfirms.Count);

			ct.PickupCompleted(DocAddressType.LocalCartageExporter, now);
			Factory.Save();
			AssertEquals(now, shp.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(1, shp.PickupConfirms.Count);
			AssertEquals(30, shp.PickupConfirms[0].TotalBookedPackages);
			AssertEquals(30, shp.PickupConfirms[0].TotalDeliveredPackages);
			AssertEquals(now, shp.PickupConfirms[0].EU_PickupDeliveryTime);
		}

		public void TestPickupCompleted_Containerised()
		{
			CommonConsol csl = GetNewConsol();
			csl.JK_TransportMode = Constants.TransportModes.Sea;
			csl.JK_ConsolMode = Constants.ContainerModes.FCL;
			CommonContainer cnt1 = csl.Containers.AddNew();
			CommonContainer cnt2 = csl.Containers.AddNew();
			CommonShipment shp = csl.Shipments.AddNew();
			PackLine p1 = shp.OuterPackLines.AddNew();
			PackLine p2 = shp.OuterPackLines.AddNew();
			p1.JL_PackageCount = 10;
			p2.JL_PackageCount = 20;
			p1.SetContainer(csl, cnt1);
			p2.SetContainer(csl, cnt2);

			ZDateTime now = ZDateTime.Now;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			AssertEquals(0, shp.PickupConfirms.Count);
			AssertEquals(ZDateTime.Empty, cnt1.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, cnt2.OriginConfirm.EU_PickupDeliveryTime);

			ct.PickupCompleted(cnt1, DocAddressType.LocalCartageExporter, now);
			AssertEquals(0, shp.PickupConfirms.Count);
			AssertEquals(now, cnt1.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, cnt2.OriginConfirm.EU_PickupDeliveryTime);

			ct.PickupCompleted(cnt2, DocAddressType.LocalCartageExporter, now.AddDays(1));
			AssertEquals(0, shp.PickupConfirms.Count);
			AssertEquals(now, cnt1.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1), cnt2.OriginConfirm.EU_PickupDeliveryTime);
		}

		/// <summary>
		/// Cartage Jobs are constructed from the subshipment movement details, therefore if the cartage is complete, so are the subs + asm
		/// </summary>
		public void TestPickupCompleted_AssemblyMasterShipment()
		{
			#region setup
			var consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var asmShipment = consol.Shipments.AddNew();
			var subShipment1 = consol.Shipments.AddNew();
			var subShipment2 = consol.Shipments.AddNew();

			asmShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			asmShipment.JS_UniqueConsignRef = "ASMPARENT001";
			asmShipment.JS_OuterPacks = 5;
			asmShipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			asmShipment.JS_TotalPackageCount = 50;
			asmShipment.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;

			subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipmentForBinding = asmShipment.PK;
			subShipment1.JS_UniqueConsignRef = "SUBCHILD001";
			subShipment1.JS_OuterPacks = 3;
			subShipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			subShipment1.JS_TotalPackageCount = 30;
			subShipment1.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;

			subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment2.JS_JS_ColoadMasterShipmentForBinding = asmShipment.PK;
			subShipment2.JS_UniqueConsignRef = "SUBCHILD002";
			subShipment2.JS_OuterPacks = 2;
			subShipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			subShipment2.JS_TotalPackageCount = 20;
			subShipment2.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			#endregion

			AssertEquals("Precondition - No Pickup Date on ASM Master", ZDateTime.Empty, asmShipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("Precondition - No Pickup Date on Sub Shipment 1", ZDateTime.Empty, subShipment1.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("Precondition - No Pickup Date on Sub Shipment 2", ZDateTime.Empty, subShipment2.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("Precondition - No PCF event on ASM Master", false, HasPCFEvent(asmShipment));
			AssertEquals("Precondition - No PCF event on Sub Shipment 1", false, HasPCFEvent(subShipment1));
			AssertEquals("Precondition - No PCF event on Sub Shipment 2", false, HasPCFEvent(subShipment2));

			ZDateTime now = ZDateTime.Now;
			var cartageJob = new ShipmentPickupCartageType(asmShipment);
			cartageJob.PickupCompleted(DocAddressType.LocalCartageExporter, now);
			Factory.Save();

			AssertEquals("Pickup Date on ASM Master", now, asmShipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("PCF event on ASM Master", true, HasPCFEvent(asmShipment));
			AssertEquals("No Pickup Date on Sub Shipment 1", ZDateTime.Empty, subShipment1.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("No Pickup Date on Sub Shipment 2", ZDateTime.Empty, subShipment2.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals("No PCF event on Sub Shipment 1", false, HasPCFEvent(subShipment1));
			AssertEquals("No PCF event on Sub Shipment 2", false, HasPCFEvent(subShipment2));
		}

		bool HasPCFEvent(CommonShipment shipment)
		{
			return shipment.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.PickupCartageCompleteFinalised.Code);
		}

		public void TestSetTotalDemurrage()
		{
			var shp = GetNewShipment();
			var now = ZDateTime.Now;
			var ct = new ShipmentPickupCartageType(shp);
			AssertEquals(true, shp.DocsAndCartage.JP_PickupTruckWaitTime.IsEmpty);

			ct.SetTotalDemurrage(new TimeSpan(2, 3, 4));
			AssertEquals((ZDateTime)new TimeSpan(2, 3, 4), shp.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		public void TestEstimatedCartagePickup()
		{
			CommonShipment shp = GetNewShipment();
			ZDateTime now = ZDateTime.Now;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			shp.DocsAndCartage.JP_EstimatedPickup = now;
			AssertEquals(now, ct.EstimatedCartagePickup);
		}

		public void TestEstimatedCartageDelivery()
		{
			CommonShipment shp = GetNewShipment();
			ZDateTime now = ZDateTime.Now;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			shp.DocsAndCartage.JP_PickupRequiredBy = now;
			AssertEquals(now, ct.EstimatedCartageDelivery);
		}

		public void TestCartageJobType()
		{
			CommonShipment shp = GetNewShipment();
			shp.JS_TransportMode = Constants.TransportModes.Road;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			AssertEquals(Constants.CartageJobType.NEW_LCLExport, ct.CartageJobType);

			shp.JS_RL_NKOrigin = "AUSYD";
			shp.JS_RL_NKDestination = "AUBNE";

			ct = new ShipmentPickupCartageType(shp);
			AssertEquals(Constants.CartageJobType.NEW_DomesticLoosePickup, ct.CartageJobType);

			shp.JS_TransportMode = Constants.TransportModes.Courier;
			ct = new ShipmentPickupCartageType(shp);
			AssertEquals(Constants.CartageJobType.NEW_DomesticLoosePickup, ct.CartageJobType);

			shp.JS_RL_NKDestination = "NZAKL";
			ct = new ShipmentPickupCartageType(shp);
			AssertEquals(Constants.CartageJobType.NEW_LCLExport, ct.CartageJobType);

			shp.JS_TransportMode = Constants.TransportModes.Sea;
			shp.JS_PackingMode = Constants.ContainerModes.FCL;
			CommonConsol consol = shp.Consols.AddNew();
			AssertEquals(Constants.CartageJobType.NEW_FCLSHPtoCTO, ct.CartageJobType);

			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportToSHP, ct.CartageJobType);

			consol.JK_OA_PackDepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportPack, ct.CartageJobType);

			consol.JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			AssertEquals(Constants.CartageJobType.NEW_FCLCFStoCTO, ct.CartageJobType);
		}

		public void TestDropMode()
		{
			CommonShipment shp = GetNewShipment();
			shp.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			ShipmentPickupCartageType ct = new ShipmentPickupCartageType(shp);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, ct.DropMode);
		}

		public void TestOriginScheduleDates()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);

			var shipment = GetNewShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.Transports.AddNew();
			consol.Transports[0].JW_JX = sailingHelper.SydLaxSailing.PK;

			var ct = new ShipmentPickupCartageType(shipment);
			Assert(!ct.FCLReceivalCommences.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOReceivalCommences, ct.FCLReceivalCommences);

			Assert(!ct.FCLCutOff.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOCutOff, ct.FCLCutOff);

			Assert(!ct.LCLReceivalCommences.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotReceivalCommences, ct.LCLReceivalCommences);

			Assert(!ct.LCLCutOff.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotCutOff, ct.LCLCutOff);
		}

		public void TestGetMatchingDirectionCodes()
		{
			CommonShipment shp = GetNewShipment();
			var cartageType = new ShipmentPickupCartageType(shp);
			AssertArrayEqualsByElements("GetMatchingDirectionCodes() returned the wrong values", new ZString[] { "EXP", "ORG" }, cartageType.GetMatchingDirectionCodes().ToArray());
		}

		public void TestShipmentParentWithoutSailingPopulatesScheduleFromMostInterestingTransportSailing()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			var sailing = sailingHelper.SydLaxSailing;
			sailingHelper.SetupFCLLCLDates(sailing);
			sailing.Destination.JB_A_ARV = sailing.JX_JB_E_ARV;
			sailing.Origin.JA_A_DEP = sailing.JX_JA_E_DEP;

			var shipment = GetNewShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var transport = shipment.Transports.AddNew();
			transport.JW_JX = sailing.PK;

			CombineAssertions("Preconditions on parent shipment docs and cartage record - check that availability and storage dates are empty.", () =>
			{
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLAvailable);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLStorageCommences);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLAvailable);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLStorageCommences);
			});

			CombineAssertions("Preconditions on sailing record - check for non-empty values to appear on ShipmentCartageType schedule, and check shipment's MostInterestingTransport and sailing links", () =>
			{
				AssertNotEquals(ZString.Empty, sailing.JX_JA_RL_NKPortOfLoading);
				AssertNotEquals(ZString.Empty, sailing.JX_JB_RL_NKPortOfDischarge);
				AssertNotEquals(ZString.Empty, sailing.JX_JV_NKVessel);
				AssertNotEquals(ZString.Empty, sailing.JX_JV_VoyageFlight);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_E_ARV);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JA_E_DEP);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_A_ARV);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JA_A_DEP);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_CTOAvailabilityDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_CTOStorageDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_DepotAvailabilityDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_DepotStorageDate);
				AssertNotEquals(ZString.Empty, sailing.JX_JA_CTOCutOff);
				AssertNotEquals(ZString.Empty, sailing.JX_JA_CTOReceivalCommences);
				AssertNotEquals(ZString.Empty, sailing.JX_DepotCutOff);
				AssertNotEquals(ZString.Empty, sailing.JX_DepotReceivalCommences);

				AssertEquals("Check that transport is shipment's MostInterestingTransport", transport, shipment.MostInterestingTransport);
				AssertNull("Shipment Sailing should be null", shipment.Sailing);
				AssertEquals("Sailing is on transport", sailing, transport.Sailing);
			});

			var cartageType = new ShipmentPickupCartageType(shipment);

			CombineAssertions("Check that schedule fields have carried on to ShipmentCartageType", () =>
			{
				AssertEquals("Cartage Type PortOfLoading should come from Sailing PortOfLoading", sailing.JX_JA_RL_NKPortOfLoading, cartageType.PortOfLoading);
				AssertEquals("Cartage Type PortOfDischarge should come from Sailing PortOfDischarge", sailing.JX_JB_RL_NKPortOfDischarge, cartageType.PortOfDischarge);
				AssertEquals("Cartage Type Vessel should come from Sailing Vessel", sailing.JX_JV_NKVessel, cartageType.Vessel);
				AssertEquals("Cartage Type VoyageFlight should come from Sailing VoyageFlight", sailing.JX_JV_VoyageFlight, cartageType.VoyageFlight);
				AssertEquals("Cartage Type Estimated Arrival should come from Sailing Estimated Arrival of Destination", sailing.JX_JB_E_ARV, cartageType.E_ARV);
				AssertEquals("Cartage Type Estimated Departure should come from Sailing Estimated Departure of Origin", sailing.JX_JA_E_DEP, cartageType.E_DEP);
				AssertEquals("Cartage Type Actual Arrival should come from Sailing Actual Arrival of Destination", sailing.JX_JB_A_ARV, cartageType.A_ARV);
				AssertEquals("Cartage Type Actual Departure should come from Sailing Actual Departure of Origin", sailing.JX_JA_A_DEP, cartageType.A_DEP);
				AssertEquals("Cartage Type FCL Availability Date should come from Sailing CTO Availability Date of Destination", sailing.JX_JB_CTOAvailabilityDate, cartageType.FCLAvailabilityDate);
				AssertEquals("Cartage Type FCL Storage Date should come from Sailing CTO Storage Date of Destination", sailing.JX_JB_CTOStorageDate, cartageType.FCLStorageDate);
				AssertEquals("Cartage Type LCL Availability Date should come from Sailing Depot Availability Date", sailing.JX_DepotAvailabilityDate, cartageType.LCLAvailabilityDate);
				AssertEquals("Cartage Type LCL Storage Date should come from Sailing Depot Storage Date", sailing.JX_DepotStorageDate, cartageType.LCLStorageDate);
				AssertEquals("Cartage Type FCL Cut Off Date should come from Sailing CTO Cut Off Date of Origin", sailing.JX_JA_CTOCutOff, cartageType.FCLCutOff);
				AssertEquals("Cartage Type FCL Receival Commences Date should come from Sailing CTO Receival Date of Origin", sailing.JX_JA_CTOReceivalCommences, cartageType.FCLReceivalCommences);
				AssertEquals("Cartage Type LCL Cut Off Date should come from Sailing Depot Cut Off Date", sailing.JX_DepotCutOff, cartageType.LCLCutOff);
				AssertEquals("Cartage Type LCL Receival Commences Date should come from Sailing Depot Receival Commences Date", sailing.JX_DepotReceivalCommences, cartageType.LCLReceivalCommences);
			});
		}

		public void TestWhenSailingIsNullForParentAndMostInterestingTransport_PopulatesScheduleFromMostInterestingTransport()
		{
			var shipment = GetNewShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "Load";
			transport.JW_RL_NKDiscPort = "Disch";
			transport.JW_Vessel = "Test Vessel";
			transport.JW_VoyageFlight = "Test VF";
			transport.JW_ETA = ZDateTime.Now;
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ATA = ZDateTime.Now;
			transport.JW_ATD = ZDateTime.Now;
			transport.JW_TerminalAvailabilityDate = ZDateTime.Now;
			transport.JW_TerminalStorageDate = ZDateTime.Now;
			transport.JW_DepotAvailabilityDate = ZDateTime.Now;
			transport.JW_DepotStorageDate = ZDateTime.Now;
			transport.JW_TerminalCutOff = ZDateTime.Now;
			transport.JW_TerminalReceivalCommences = ZDateTime.Now;
			transport.JW_DepotCutOff = ZDateTime.Now;
			transport.JW_DepotReceivalCommences = ZDateTime.Now;

			CombineAssertions("Preconditions on parent shipment docs and cartage record - check that availability and storage dates are empty.", () =>
			{
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLAvailable);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLStorageCommences);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLAvailable);
				AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLStorageCommences);
			});

			CombineAssertions("Preconditions on sailing record - check for non-empty values to appear on transport, and check shipment's sailing and MostInterestingTransport sailing links", () =>
			{
				AssertNotEquals("PortOfLoading", ZString.Empty, transport.JW_RL_NKLoadPort);
				AssertNotEquals("PortOfDischarge", ZString.Empty, transport.JW_RL_NKDiscPort);
				AssertNotEquals("Vessel", ZString.Empty, transport.JW_Vessel);
				AssertNotEquals("VoyageFlight", ZString.Empty, transport.JW_VoyageFlight);
				AssertNotEquals("Estimated Arrival", ZDateTime.Empty, transport.JW_ETA);
				AssertNotEquals("Estimated Departure", ZDateTime.Empty, transport.JW_ETD);
				AssertNotEquals("Actual Arrival", ZDateTime.Empty, transport.JW_ATA);
				AssertNotEquals("Actual Departure", ZDateTime.Empty, transport.JW_ATD);
				AssertNotEquals("CTO Availability", ZDateTime.Empty, transport.JW_TerminalAvailabilityDate);
				AssertNotEquals("CTO Storage", ZDateTime.Empty, transport.JW_TerminalStorageDate);
				AssertNotEquals("Depot Availability", ZDateTime.Empty, transport.JW_DepotAvailabilityDate);
				AssertNotEquals("Depot Storage", ZDateTime.Empty, transport.JW_DepotStorageDate);
				AssertNotEquals("CTO Cut Off", ZDateTime.Empty, transport.JW_TerminalCutOff);
				AssertNotEquals("CTO Receival", ZDateTime.Empty, transport.JW_TerminalReceivalCommences);
				AssertNotEquals("Depot Cut Off", ZDateTime.Empty, transport.JW_DepotCutOff);
				AssertNotEquals("Depot Receival", ZDateTime.Empty, transport.JW_DepotReceivalCommences);

				AssertNull("Check that MostInterestingTransport Sailing is null", shipment.MostInterestingTransport.Sailing);
				AssertNull("Shipment Sailing should be null", shipment.Sailing);
				AssertEquals("Check that transport is MostInterestingTransport on shipment", transport, shipment.MostInterestingTransport);
			});

			var cartageType = new ShipmentPickupCartageType(shipment);

			CombineAssertions("Check that shipment properties have carried on to ShipmentCartageType", () =>
			{
				AssertEquals("Cartage Type PortOfLoading should come from transport PortOfLoading", transport.JW_RL_NKLoadPort, cartageType.PortOfLoading);
				AssertEquals("Cartage Type PortOfDischarge should come from transport PortOfDischarge", transport.JW_RL_NKDiscPort, cartageType.PortOfDischarge);
				AssertEquals("Cartage Type Vessel should come from transport Vessel", transport.JW_Vessel, cartageType.Vessel);
				AssertEquals("Cartage Type VoyageFlight should come from transport VoyageFlight", transport.JW_VoyageFlight, cartageType.VoyageFlight);
				AssertEquals("Cartage Type Estimated Arrival should come from transport Estimated Arrival of Destination", transport.JW_ETA, cartageType.E_ARV);
				AssertEquals("Cartage Type Estimated Departure should come from transport Estimated Departure of Origin", transport.JW_ETD, cartageType.E_DEP);
				AssertEquals("Cartage Type Actual Arrival should come from transport Actual Arrival of Destination", transport.JW_ATA, cartageType.A_ARV);
				AssertEquals("Cartage Type Actual Departure should come from transport Actual Departure of Origin", transport.JW_ATD, cartageType.A_DEP);
				AssertEquals("Cartage Type FCL Availability Date should come from transport CTO Availability Date of Destination", transport.JW_TerminalAvailabilityDate, cartageType.FCLAvailabilityDate);
				AssertEquals("Cartage Type FCL Storage Date should come from transport CTO Storage Date of Destination", transport.JW_TerminalStorageDate, cartageType.FCLStorageDate);
				AssertEquals("Cartage Type LCL Availability Date should come from transport Depot Availability Date", transport.JW_DepotAvailabilityDate, cartageType.LCLAvailabilityDate);
				AssertEquals("Cartage Type LCL Storage Date should come from transport Depot Storage Date", transport.JW_DepotStorageDate, cartageType.LCLStorageDate);
				AssertEquals("Cartage Type FCL Cut Off Date should come from transport CTO Cut Off Date of Origin", transport.JW_TerminalCutOff, cartageType.FCLCutOff);
				AssertEquals("Cartage Type FCL Receival Commences Date should come from transport CTO Receival Date of Origin", transport.JW_TerminalReceivalCommences, cartageType.FCLReceivalCommences);
				AssertEquals("Cartage Type LCL Cut Off Date should come from transport Depot Cut Off Date", transport.JW_DepotCutOff, cartageType.LCLCutOff);
				AssertEquals("Cartage Type LCL Receival Commences Date should come from transport Depot Receival Commences Date", transport.JW_DepotReceivalCommences, cartageType.LCLReceivalCommences);
			});
		}

		public void TestAvailabilityAndStorageScheduleDates_PopulateFromShipmentBeforeSailingOrMostInterestingTransport()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			var sailing = sailingHelper.SydLaxSailing;
			sailingHelper.SetupFCLLCLDates(sailing);
			sailing.Destination.JB_A_ARV = sailing.JX_JB_E_ARV;
			sailing.Origin.JA_A_DEP = sailing.JX_JA_E_DEP;

			var shipment = GetNewShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Now.AddMinutes(-10);
			shipment.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.Now.AddMinutes(-10);
			shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Now.AddMinutes(-10);
			shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Now.AddMinutes(-10);

			var transport = shipment.Transports.AddNew();
			transport.JW_JX = sailing.PK;
			transport.JW_TerminalAvailabilityDate = ZDateTime.Now;
			transport.JW_TerminalStorageDate = ZDateTime.Now;
			transport.JW_DepotAvailabilityDate = ZDateTime.Now;
			transport.JW_DepotStorageDate = ZDateTime.Now;

			CombineAssertions("Preconditions on parent shipment docs and cartage record - check for non-empty values for availability and storage date properties.", () =>
			{
				AssertNotEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLAvailable);
				AssertNotEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLStorageCommences);
				AssertNotEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLAvailable);
				AssertNotEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLStorageCommences);
			});

			CombineAssertions("Preconditions on transport record - check for non-empty values for availability and storage date properties.", () =>
			{
				AssertNotEquals("CTO Availability", ZDateTime.Empty, transport.JW_TerminalAvailabilityDate);
				AssertNotEquals("CTO Storage", ZDateTime.Empty, transport.JW_TerminalStorageDate);
				AssertNotEquals("Depot Availability", ZDateTime.Empty, transport.JW_DepotAvailabilityDate);
				AssertNotEquals("Depot Storage", ZDateTime.Empty, transport.JW_DepotStorageDate);
			});

			CombineAssertions("Preconditions on sailing record - check for non-empty values for availability and storage date properties.", () =>
			{
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_CTOAvailabilityDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_JB_CTOStorageDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_DepotAvailabilityDate);
				AssertNotEquals(ZDateTime.Empty, sailing.JX_DepotStorageDate);
			});

			CombineAssertions("Additional preconditions - check shipment's sailing and MostInterestingTransport sailing links.", () =>
			{
				AssertEquals("Check that transport is shipment's MostInterestingTransport", transport, shipment.MostInterestingTransport);
				AssertNull("Shipment Sailing should be null", shipment.Sailing);
				AssertEquals("Sailing is on transport", sailing, transport.Sailing);
			});

			var cartageType = new ShipmentPickupCartageType(shipment);

			CombineAssertions("Check that shipment properties have carried on to ShipmentCartageType", () =>
			{
				AssertEquals("Cartage Type FCL Availability Date should come from shipment DocsAndCartage JP_FCLAvailable.", shipment.DocsAndCartage.JP_FCLAvailable, cartageType.FCLAvailabilityDate);
				AssertEquals("Cartage Type FCL Storage Date should come from shipment DocsAndCartage JP_FCLStorageCommences.", shipment.DocsAndCartage.JP_FCLStorageCommences, cartageType.FCLStorageDate);
				AssertEquals("Cartage Type LCL Availability Date should come from shipment DocsAndCartage JP_LCLAvailable.", shipment.DocsAndCartage.JP_LCLAvailable, cartageType.LCLAvailabilityDate);
				AssertEquals("Cartage Type LCL Storage Date should come from shipment DocsAndCartage JP_LCLStorageCommences.", shipment.DocsAndCartage.JP_LCLStorageCommences, cartageType.LCLStorageDate);
			});
		}

		CommonShipment GetNewShipment()
		{
			return (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
		}

		CommonConsol GetNewConsol()
		{
			return (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
		}
	}
}
