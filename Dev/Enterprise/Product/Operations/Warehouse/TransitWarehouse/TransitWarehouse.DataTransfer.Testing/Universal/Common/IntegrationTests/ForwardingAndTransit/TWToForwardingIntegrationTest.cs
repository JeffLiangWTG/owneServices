using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class TWToForwardingIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region TestTWToForwarding_Container

		// Containers aren't generally used on unload in a departure transit warehouse, but the case is supported
		public void TestTWToForwarding_Receive_UpdateContainer() => TestTWToForwarding_Receive_ContainerCore(isForwardingContainer: true);

		public void TestTWToForwarding_Receive_CreateContainer() => TestTWToForwarding_Receive_ContainerCore(isForwardingContainer: false);

		void TestTWToForwarding_Receive_ContainerCore(bool isForwardingContainer)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			ForwardingContainer container = null;
			if (isForwardingContainer)
			{
				container = CreateContainer(consol, "CONT1", 1, "20GP");
				container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;
			}

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: A single Package should be created.", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			WhsItemReceiveTransportationUnit rtu = null;

			AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
			rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP");

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");
			rtu.WRH_UnloadCompleteTime = dateTimeOffset;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var containersAfterExport = shipmentInAnotherFactory.Containers;
			AssertEquals("The Consol should have a single container.", 1, containersAfterExport.Count());
			var containerAfterExport = containersAfterExport.Cast<ForwardingContainer>().Single();

			if (isForwardingContainer)
			{
				AssertEquals("The existing Forwarding Container should be updated.", container.PK, containerAfterExport.PK);
			}

			AssertEquals("The Forwarding Container should be updated", "40GP", containerAfterExport.RefContainer.RC_Code);
			AssertEquals("The Forwarding Container should be updated", "FCL", containerAfterExport.JC_ContainerMode);
		}

		#endregion

		#region TestTWToForwarding_ContainerAllocatePacklines

		#region Receive

		public void TestTWToForwarding_Receive_ContainerAllocatePacklinesVEH() => TestTWToForwarding_Receive_ContainerAllocatePacklinesCore("VEH", "DROP");
		public void TestTWToForwarding_Receive_ContainerAllocatePacklinesAIR() => TestTWToForwarding_Receive_ContainerAllocatePacklinesCore("ULD", "AAA");
		public void TestTWToForwarding_Receive_ContainerAllocatePacklinesCNT() => TestTWToForwarding_Receive_ContainerAllocatePacklinesCore("CNT", "20GP");

		void TestTWToForwarding_Receive_ContainerAllocatePacklinesCore(string unitType, string containerType)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, containerType);
			container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: A single Package should be created.", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", containerType, unitType);
			var pltUnloadedPackage = UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var containersAfterExport = shipmentInAnotherFactory.Containers;
			AssertEquals("The Consol should have a single container.", 1, containersAfterExport.Count());
			var containerAfterExport = containersAfterExport.Cast<ForwardingContainer>().Single();

			var packlines = containerAfterExport.PackLines;
			AssertEquals("The container should have a single packline.", 1, packlines.Count);

			var consolInAnotherFactory = factoryToLoadShipment.Load<ForwardingConsol>(consol.PK);
			var unAllocatedPackLines = consolInAnotherFactory.UnAllocatedPackLines;
			AssertEquals("The Consol should have no unallocated packline.", 0, unAllocatedPackLines.Count);
		}

		#endregion

		#region Dispatch

		public void TestTWToForwarding_Dispatch_ContainerAllocatePacklinesVEH() => TestTWToForwarding_Dispatch_ContainerAllocatePacklinesCore("VEH", "DROP");
		public void TestTWToForwarding_Dispatch_ContainerAllocatePacklinesAIR() => TestTWToForwarding_Dispatch_ContainerAllocatePacklinesCore("ULD", "AAA");
		public void TestTWToForwarding_Dispatch_ContainerAllocatePacklinesCNT() => TestTWToForwarding_Dispatch_ContainerAllocatePacklinesCore("CNT", "20GP");

		void TestTWToForwarding_Dispatch_ContainerAllocatePacklinesCore(string unitType, string containerType)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, containerType);
			container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = factoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Length);
			var dispatchConsignment = dispatchConsignments.Single();
			var loadLists = factoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Precondition: A single Load List should be created", 1, loadLists.Length);
			var packageStateForPallet = dispatchConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", containerType, unitType);
			var pltUnloadedPackage = UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");

			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("CONT1", warehouse.PK, "CONT1", containerType);

			pltUnloadedPackage.WPS_WDH_TransitDispatchHeader = dtu.PK;
			pltUnloadedPackage.WPS_IsSecure = true;
			pltUnloadedPackage.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			pltUnloadedPackage.WPS_LoadedTime = DateTimeOffset.Now;
			pltUnloadedPackage.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;

			Factory.Save();

			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var containersAfterExport = shipmentInAnotherFactory.Containers;
			AssertEquals("The Consol should have a single container.", 1, containersAfterExport.Count());
			var containerAfterExport = containersAfterExport.Cast<ForwardingContainer>().Single();

			var packlines = containerAfterExport.PackLines;
			AssertEquals("The container should still have the existing packline.", 1, packlines.Count);

			var consolInAnotherFactory = factoryToLoadShipment.Load<ForwardingConsol>(consol.PK);
			var unAllocatedPackLines = consolInAnotherFactory.UnAllocatedPackLines;
			AssertEquals("The Consol should have no unallocated packline.", 0, unAllocatedPackLines.Count);
		}

		#endregion

		#endregion

		#region TestTWToForwarding_Dispatch_Consol

		[TestDate(2021, 8, 1)]
		public void TestTWToForwarding_Dispatch_WithSubShipments() => TestTWToForwarding_Dispatch();

		[TestDate(2021, 10, 1)]
		public void TestTWToForwarding_Dispatch_WithRelatedShipments() => TestTWToForwarding_Dispatch();

		void TestTWToForwarding_Dispatch()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			// Create a shipment on a consol.
			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");
			container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "P0000001");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = factoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Length);
			var dispatchConsignment = dispatchConsignments.Single();
			var loadLists = factoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Precondition: A single Load List should be created", 1, loadLists.Length);

			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignment);

			var factoryAfterExport = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentAfterExport = factoryAfterExport.Load<ForwardingShipment>(shipment.PK);
			var ediMessage = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
			AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
		}

		#endregion

		#region TestTWToForwarding_Receive_BlockRead

		public void TestTWToForwarding_Receive_BlockRead()
		{
			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false);
			var consol = CreateConsol("MSB1", vessel, "NLEUG", "AUSYD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NLEUG", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateContainer(consol, "CONT2");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("A single RCN should be created.", 1, receiveConsignments.Length);
			var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("A single DCN should be created.", 1, dispatchConsignments.Length);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP");
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("TD001", warehouse.PK, "CONT2", "40GP");
			var dispatchConsignment = dispatchConsignments.Single();
			var packageState1 = dispatchConsignment.PackageStates[0];
			var packageState2 = dispatchConsignment.PackageStates[1];
			var packageState3 = dispatchConsignment.PackageStates[2];
			packageState1.Package.KP_PackageID = "P1";
			packageState2.Package.KP_PackageID = "P2";
			packageState3.Package.KP_PackageID = "P3";
			UnloadAndLabelPackage(packageState2, rtu, "P2", setDetails: true);
			LoadPackage(packageState3, warehouse.DefaultLocation, rtu, dtu, dll);
			packageState3.Package.KP_Weight = 2;

			Factory.Save();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryAfterExport = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentAfterExport = factoryAfterExport.Load<ForwardingShipment>(shipment.PK);
			var ediMessageAfterExport = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
			AssertEquals("Precondition: The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessageAfterExport?.EM_Status);

			var receiveConsignment = receiveConsignments.Single();
			TriggerAndFireOutturn(receiveConsignment);

			factoryAfterExport = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentAfterExport = factoryAfterExport.Load<WhsItemReceiveConsignment>(receiveConsignment.PK);
			ediMessageAfterExport = UniversalHelper.GetEDIMessageFromDB(receiveConsignmentAfterExport, AutoEvents.DataExportCode);
			AssertEquals("The receive consignment should have an unsuccessful export.", EDIMessageStatusList.Codes.Discarded, ediMessageAfterExport?.EM_Status);
			var notesAfterExport = ediMessageAfterExport.GetNotes().GetAllNotes();
			AssertEquals("One note expected to be on the bizo.", 1, notesAfterExport.Count);
			AssertContains("RCN xml has not been processed. This update should be done through the DCN.", notesAfterExport.Cast<StmNote>().First().ST_NoteDataAsText);
		}

		#endregion

		#region TestTWToForwarding_DoNotImportTransportLegsFromTW

		public void TestTWToForwarding_DoNotImportTransportLegsFromTW()
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var inboundLegInConsol = CreateTransport(consol, 1, "SEA", "A", "AA", "USA2N", "NLEUG", testData.today, testData.today.AddDays(2));
			var outboundLegInConsol = CreateTransport(consol, 2, "SEA", "B", "BB", "NLEUG", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLegInShipment = CreateTransport(shipment, 3, "SEA", "C", "CC", "USA2N", "NZAKL", testData.today, testData.today.AddDays(2));
			var outboundLegInShipment = CreateTransport(shipment, 4, "SEA", "D", "DD", "NZAKL", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));

			var container = CreateContainer(consol, "CONT1");
			var packline11 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: A single Package should be created.", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK, "CONT1", "40GP");

			UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");

			inboundLegInConsol.JW_Vessel = "A_Modified";
			outboundLegInConsol.JW_Vessel = "B_Modified";
			inboundLegInShipment.JW_Vessel = "C_Modified";
			outboundLegInShipment.JW_Vessel = "D_Modified";
			Factory.Save();

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);
			AssertTWToForwarding_DoNotImportTransportLegsFromTW(consol.PK, shipment.PK);

			var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Length);
			var dispatchConsignment = dispatchConsignments.Single();

			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignment);
			AssertTWToForwarding_DoNotImportTransportLegsFromTW(consol.PK, shipment.PK);
		}

		void AssertTWToForwarding_DoNotImportTransportLegsFromTW(ZGuid consolPK, ZGuid shipmentPK)
		{
			var newFactoryAfterReceive = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolTransportRoutings = newFactoryAfterReceive.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consolPK));

			AssertEquals(0, consolTransportRoutings.Count(t => t.JW_Vessel == "A"));
			AssertEquals(0, consolTransportRoutings.Count(t => t.JW_Vessel == "B"));
			AssertEquals(1, consolTransportRoutings.Count(t => t.JW_Vessel == "A_Modified"));
			AssertEquals(1, consolTransportRoutings.Count(t => t.JW_Vessel == "B_Modified"));

			var shipmentTranportRoutings = newFactoryAfterReceive.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, shipmentPK));
			AssertEquals(0, shipmentTranportRoutings.Count(t => t.JW_Vessel == "C"));
			AssertEquals(0, shipmentTranportRoutings.Count(t => t.JW_Vessel == "D"));
			AssertEquals(1, shipmentTranportRoutings.Count(t => t.JW_Vessel == "C_Modified"));
			AssertEquals(1, shipmentTranportRoutings.Count(t => t.JW_Vessel == "D_Modified"));
		}

		#endregion

		#region TestTWToForwarding_Dispatch_NoTransportMode

		public void TestTWToForwarding_Dispatch_NoTransportMode()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			// Create a shipment on a consol.
			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1", 1, "20GP");
			container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "P0000001");
			shipment.JS_TransportMode = "SEA";
			AssertEquals("Precondition", TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals("Precondition", TransportModes.Sea, shipment.JS_TransportMode);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var loadList = factoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			dispatchConsignment.WDC_TransportMode = "";
			loadList.WDL_TransportMode = "";

			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignment);

			var factoryAfterExport = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentAfterExport = factoryAfterExport.Load<ForwardingShipment>(shipment.PK);
			var consolAfterExport = factoryAfterExport.Load<ForwardingConsol>(consol.PK);
			var ediMessage = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
			AssertEquals("Precondition: The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
			AssertEquals(TransportModes.Sea, consolAfterExport.JK_TransportMode);
			AssertEquals(TransportModes.Sea, shipmentAfterExport.JS_TransportMode);
		}

		#endregion

		#region TestTWToForwarding_SendingPackages_UpdatePackLines

		[TestDate(2021, 1, 1)]
		public void TestTWToForwarding_SendingPackages_UpdatePackLines()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			var dateTimeNow = ZDateTimeOffset.Now;
			var dateTimeOffset = new ZDateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var pltUnloadedPackage = UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1", setDetails: true, weight: 2, weightUQ: "KG", volume: 3, volumeUQ: "M3");
			rtu1.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu1.WRH_UnloadCompleteTime = dateTimeOffset.AddHours(1);
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			var boxUnloadedPackage = UnloadAndLabelPackage(packageStateForBox, rtu2, "BOX1", setDetails: true, weight: 3, weightUQ: "G", volume: 4, volumeUQ: "D3");
			rtu2.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = dateTimeOffset.AddHours(2);
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;

			//Received Overs
			var cas1UnloadedPackage = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3");
			var cas2UnloadedPackage = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, weight: 6, weightUQ: "KG", volume: 7, volumeUQ: "M3");
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals(3, packLinesInShipment.Count);

			var packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCAS = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);
			AssertEquals("No new packline should be generated.", packLinesForPLT.PK, packline1.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForPLT.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", pltUnloadedPackage.WPS_UnloadedTime.ToZDateTime(), packLinesForPLT.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForPLT.PkgPackageCollection.Single(), weight: 2, weightUQ: "KG", volume: 3, volumeUQ: "M3");

			AssertEquals("No new packline should be generated.", packLinesForBOX.PK, packline2.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForBOX.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", boxUnloadedPackage.WPS_UnloadedTime.ToZDateTime(), packLinesForBOX.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForBOX.PkgPackageCollection.Single(), weight: 3, weightUQ: "G", volume: 4, volumeUQ: "D3");

			AssertEquals("new packline should be created with pkgpackages attached", 2, packLinesForCAS.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", new[] { cas1UnloadedPackage.WPS_UnloadedTime, cas2UnloadedPackage.WPS_UnloadedTime }.Max().ToZDateTime(), packLinesForCAS.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForCAS.PkgPackageCollection.Single(p => p.KP_PackageID == "CAS1"), weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3");
			AssertDimensions(packLinesForCAS.PkgPackageCollection.Single(p => p.KP_PackageID == "CAS2"), weight: 6, weightUQ: "KG", volume: 7, volumeUQ: "M3");
		}

		[TestDate(2021, 1, 1)]
		public void TestTWToForwarding_SendingPackages_ChangeConsol_UpdatePackLines()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("MAB1", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			var dateTimeNow = ZDateTimeOffset.Now;
			var dateTimeOffset = new ZDateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var pltUnloadedPackage = UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			rtu1.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu1.WRH_UnloadCompleteTime = dateTimeOffset.AddHours(1);
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			var boxUnloadedPackage = UnloadAndLabelPackage(packageStateForBox, rtu2, "BOX1");
			rtu2.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = dateTimeOffset.AddHours(2);
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;

			//Received Overs
			var cas1UnloadedPackage = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			var cas2UnloadedPackage = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals(3, packLinesInShipment.Count);

			var newConsolWithDifferentRouting = CreateConsol("MAB2", vessel, "NZCHC", "AUSYD", "ABCD");
			newConsolWithDifferentRouting.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.Shipments.Remove(shipment);
			newConsolWithDifferentRouting.Shipments.Add(shipment);
			Factory.Save();

			var consolReference = receiveConsignment.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			consolReference.CE_EntryNum = newConsolWithDifferentRouting.JK_UniqueConsignRef;
			Helper.CreateAdditionalReference(receiveConsignment, "MAB2", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			SetDimensions(pltUnloadedPackage, weight: 1, weightUQ: "KG", volume: 2, volumeUQ: "M3");
			SetDimensions(boxUnloadedPackage, weight: 2, weightUQ: "G", volume: 3, volumeUQ: "D3");
			SetDimensions(cas1UnloadedPackage, weight: 3, weightUQ: "KG", volume: 4, volumeUQ: "M3");
			SetDimensions(cas2UnloadedPackage, weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3");
			Factory.Save();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryAfterResendingShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentAfterResendingShipment = factoryAfterResendingShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesAfterResending = shipmentAfterResendingShipment.OuterPackLines;
			AssertEquals(3, packLinesAfterResending.Count);

			var packLinesForPLT = packLinesAfterResending.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packLinesForBOX = packLinesAfterResending.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCAS = packLinesAfterResending.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);
			AssertEquals("No new packline should be generated.", packLinesForPLT.PK, packline1.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForPLT.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", pltUnloadedPackage.WPS_UnloadedTime.ToZDateTime(), packLinesForPLT.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForPLT.PkgPackageCollection.Single(), weight: 1, weightUQ: "KG", volume: 2, volumeUQ: "M3");

			AssertEquals("No new packline should be generated.", packLinesForBOX.PK, packline2.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForBOX.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", boxUnloadedPackage.WPS_UnloadedTime.ToZDateTime(), packLinesForBOX.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForBOX.PkgPackageCollection.Single(), weight: 2, weightUQ: "G", volume: 3, volumeUQ: "D3");

			AssertEquals("new packline should be created with pkgpackages attached", 2, packLinesForCAS.PkgPackageCollection.Count);
			AssertEquals("Last Known Status DateTime should be the latest Time", new[] { cas1UnloadedPackage.WPS_UnloadedTime, cas2UnloadedPackage.WPS_UnloadedTime }.Max().ToZDateTime(), packLinesForCAS.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertDimensions(packLinesForCAS.PkgPackageCollection.Single(p => p.KP_PackageID == "CAS1"), weight: 3, weightUQ: "KG", volume: 4, volumeUQ: "M3");
			AssertDimensions(packLinesForCAS.PkgPackageCollection.Single(p => p.KP_PackageID == "CAS2"), weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3");
		}

		void SetDimensions(WhsItemPackageState packageState, decimal volume = 0.0m, string volumeUQ = "M3", decimal weight = 0.0m, string weightUQ = "KG")
		{
			packageState.Package.KP_Weight = weight;
			packageState.Package.KP_WeightUQ = weightUQ;
			packageState.Package.KP_Volume = volume;
			packageState.Package.KP_VolumeUQ = volumeUQ;
		}

		void AssertDimensions(PkgPackage pkgPackage, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			CombineAssertions("Dimensions must be updated", () =>
			{
				AssertEquals(weight, pkgPackage.KP_Weight);
				AssertEquals(weightUQ, pkgPackage.KP_WeightUQ);
				AssertEquals(volume, pkgPackage.KP_Volume);
				AssertEquals(volumeUQ, pkgPackage.KP_VolumeUQ);
			});
		}

		public void TestTWToForwarding_SendingPackages_UpdatePackLines_AIR()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD", Enterprise.Core.Constants.TransportModes.Air);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			UnloadAndLabelPackage(packageStateForBox, rtu1, "BOX1");

			//Received Overs
			Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals(3, packLinesInShipment.Count);

			var packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCAS = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);

			AssertEquals("No new packline should be generated.", packLinesForPLT.PK, packline1.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForPLT.PkgPackageCollection.Count);

			AssertEquals("No new packline should be generated.", packLinesForBOX.PK, packline2.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForBOX.PkgPackageCollection.Count);

			AssertEquals("new packline should be created with pkgpackages attached", 2, packLinesForCAS.PkgPackageCollection.Count);
		}

		//public void TestTWToForwarding_SendingPackages_AfterChangingHSBOnShipment_UpdatePackLines()
		//{
		//	var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

		//	var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
		//	consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

		//	var shipment = CreateShipment(consol, "", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
		//	var packline1 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
		//	var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);
		//	Factory.Save();
		//	shipment.JS_HouseBill = shipment.JS_UniqueConsignRef;
		//	Factory.Save();

		//	TriggerAndFireTransitRequestForReceive(consol);

		//	var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment.JS_UniqueConsignRef)).Single();
		//	AssertEquals(2, receiveConsignment.PackageStates.Count);

		//	var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
		//	var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
		//	AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
		//	AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

		//	var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
		//	BreakPackageAndSetRTU(warehouse, packageStateForPallet, rtu1, "PLT1");

		//	var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
		//	BreakPackageAndSetRTU(warehouse, packageStateForBox, rtu1, "BOX1");

		//	Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
		//	Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
		//	CreateWorkflowTemplateForReceiveConsignmentToShipment();

		//	shipment.JS_HouseBill = "HSB1";
		//	Factory.Save();

		//	TriggerAndFireToShipment(receiveConsignment);

		//	var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
		//	var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
		//	AssertEquals(3, packLinesInShipment.Count);

		//	var packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
		//	var packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
		//	var packLinesForCAS = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);
		//	AssertEquals(1, packLinesForPLT.JL_PackageCount);
		//	AssertEquals(1, packLinesForBOX.JL_PackageCount);
		//	AssertEquals(2, packLinesForCAS.JL_PackageCount);
		//}

		public void TestTWToForwarding_SendingPackages_AdjustedOutPackage()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD", Enterprise.Core.Constants.TransportModes.Air);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);
			var packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Coil);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForCoil = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Coil);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");
			UnloadAndLabelPackage(packageStateForBox, rtu, "BOX1");
			UnloadAndLabelPackage(packageStateForCoil, rtu, "Coil1");

			packageStateForCoil.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageStateForCoil.WPS_AdjustedOut = "ADJ";
			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;

			var packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCoil = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Coil);
			AssertEquals("PkgPackage should created and attached to PLT packline", 1, packLinesForPLT.PkgPackageCollection.Count);
			AssertEquals("PkgPackage should created and attached to BOX packline", 1, packLinesForBOX.PkgPackageCollection.Count);
			AssertEquals("No PkgPackage be created and attached to Coil packline", 0, packLinesForCoil.PkgPackageCollection.Count);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			packageStateForBox = dispatchConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			packageStateForBox.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageStateForBox.WPS_AdjustedOut = "ADJ";
			TriggerAndFireOutturn(dispatchConsignment);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			shipmentInAnotherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;

			packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals("PkgPackage should created and attached to PLT packline", 1, packLinesForPLT.PkgPackageCollection.Count);
			AssertEquals("No PkgPackage be created and attached to BOX packline", 0, packLinesForBOX.PkgPackageCollection.Count);
			AssertEquals("No PkgPackage be created and attached to Coil packline", 0, packLinesForCoil.PkgPackageCollection.Count);
		}

		#endregion

		#region TestTWToForwarding_SendingCRESAMessage

		public void TestTWToForwarding_SendingCRESAMessageFromDCN()
		{
			var unitType = "VEH";
			var containerType = "DROP";
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);
			Helper.AddOrgCode(GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.SOW, "sow");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.CodeTypes.PortSystemNumber, "001\\ZZZ");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.CodeTypes.PortServiceReference, "002");

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, containerType);
			container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			packline.JL_Description = "Goods Description";
			packline.JL_ActualVolume = 10.0;
			packline.JL_ActualWeight = 10.1;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = factoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Length);
			var dispatchConsignment = dispatchConsignments.Single();
			var loadLists = factoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Precondition: A single Load List should be created", 1, loadLists.Length);
			var packageStateForPallet = dispatchConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", containerType, unitType);
			var pltUnloadedPackage = UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");
			rtu.WRH_GateInTime = ZDateTimeOffset.Today;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_GateInTime.AddHours(1);
			rtu.WRH_UnloadCompleteTime = rtu.WRH_GateInTime.AddHours(1);

			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("CONT1", warehouse.PK, "CONT1", containerType);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);

			pltUnloadedPackage.WPS_WDH_TransitDispatchHeader = dtu.PK;
			pltUnloadedPackage.WPS_IsSecure = true;
			pltUnloadedPackage.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			pltUnloadedPackage.WPS_LoadedTime = DateTimeOffset.Now;
			pltUnloadedPackage.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			pltUnloadedPackage.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;
			pltUnloadedPackage.WPS_WDL_LoadList = dll.PK;

			Factory.Save();

			CreateWorkflowTemplateForSendingCRESAMessageFromDCN();
			TriggerAndFireFreightLoadedEventFromDCN(dispatchConsignment);
			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadDispatchConsignment = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentInAnotherFactory = factoryToLoadDispatchConsignment.Load<WhsItemDispatchConsignment>(dispatchConsignment.PK);
			var msnEvent = dispatchConsignmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode && l.SL_Table == WhsItemDispatchConsignmentSchema.Constants.TableName).SingleOrDefault();
			AssertEquals("Sending CRESA Message succeed", "Propagated: All Document Data|DEP=Terminal|MST=Goods Received (CRESA)", msnEvent?.SL_Reference);

			AssertEquals("Sending CRESA Message event is created", 1, dispatchConsignmentInAnotherFactory.BusinessObjectsWithRelatedEvents.Length);
			var cresaMessageLogs = dispatchConsignmentInAnotherFactory.BusinessObjectsWithRelatedEvents[0].GetLogs();
			AssertEquals("DEX event is created", 1, cresaMessageLogs.Find(l => l.SL_SE_NKEvent == Events.DataExportCode && l.SL_Reference == "Purpose: APP - As Per Payload").Count());
			AssertEquals("MSN event is created", 1, cresaMessageLogs.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode && l.SL_Reference == "|DEP=Terminal|MST=Goods Received (CRESA)").Count());
		}

		public void TestTWToForwarding_SendingCRESAMessageFromRCN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			Helper.AddOrgCode(GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.SOW, "sow");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.CodeTypes.PortSystemNumber, "001\\ZZZ");
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.CodeTypes.PortServiceReference, "002");

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			packline.JL_Description = "Goods Description";
			packline.JL_ActualVolume = 10.0;
			packline.JL_ActualWeight = 10.1;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: A single Package should be created.", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			CreateWorkflowTemplateForSendingCRESAMessageFromRCN();

			AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP");

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			UnloadAndLabelPackage(packageStateForPallet, rtu, "PLT1");
			rtu.WRH_UnloadCompleteTime = dateTimeOffset;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireFreightUnloadedEventFromRCN(receiveConsignment);
			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadReceiveConsignment = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentInAnotherFactory = factoryToLoadReceiveConsignment.Load<WhsItemReceiveConsignment>(receiveConsignment.PK);
			var msnEvent = receiveConsignmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode && l.SL_Table == WhsItemReceiveConsignmentSchema.Constants.TableName).SingleOrDefault();
			AssertEquals("Sending CRESA Message succeed", "Propagated: All Document Data|DEP=Terminal|MST=Goods Received (CRESA)", msnEvent?.SL_Reference);

			AssertEquals("Sending CRESA Message event is created", 1, receiveConsignmentInAnotherFactory.BusinessObjectsWithRelatedEvents.Length);
			var cresaMessageLogs = receiveConsignmentInAnotherFactory.BusinessObjectsWithRelatedEvents[0].GetLogs();
			AssertEquals("DEX event is created", 1, cresaMessageLogs.Find(l => l.SL_SE_NKEvent == Events.DataExportCode && l.SL_Reference == "Purpose: APP - As Per Payload").Count());
			AssertEquals("MSN event is created", 1, cresaMessageLogs.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode && l.SL_Reference == "|DEP=Terminal|MST=Goods Received (CRESA)").Count());
		}

		#endregion

		#region TestTWSendingBKCEventToForwardingShipment

		public void TestTWSendingBKCEventToForwardingShipment_SEA()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			CreateWorkflowTemplateForReceiveConsignment_SendBKCEventToForwarder();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("No booking confirmed event from TW yet.", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var bkcEvent = shipmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			Assert("Booking confirmed event is added to shipment.", bkcEvent.CheckReferenceEquals("New|FAC=CFS|LOC=Sydney|TYP=Transit Receive|RFN=RC00000001|WHS=TRW"));
		}

		public void TestTWSendingBKCEventToForwardingShipment_AIR()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL", transportMode: TransportModes.Air);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			AssertEquals("Precondition - Shipment created for AIR consol must be AIR.", TransportModes.Air, shipment.TransportMode);
			CreateWorkflowTemplateForReceiveConsignment_SendBKCEventToForwarder();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("No booking confirmed event from TW yet.", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var bkcEvent = shipmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			Assert("Booking confirmed event is added to shipment.", bkcEvent.CheckReferenceEquals("New|FAC=CFS|LOC=Sydney|TYP=Transit Receive|RFN=RC00000001|WHS=TRW"));
		}

		public void TestTWSendingBKCEventToForwardingShipment_SEA_WithoutHouseBill()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			var shipment = CreateShipment(consol, "", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			CreateWorkflowTemplateForReceiveConsignment_SendBKCEventToForwarder();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("No booking confirmed event from TW yet.", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var bkcEvent = shipmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			Assert("Booking confirmed event is added to shipment.", bkcEvent.CheckReferenceEquals("New|FAC=CFS|LOC=Sydney|TYP=Transit Receive|RFN=RC00000001|WHS=TRW"));
		}

		public void TestTWSendingBKCEventToForwardingShipment_AIR_WithoutHouseAirwayBill()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL", transportMode: TransportModes.Air);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			var shipment = CreateShipment(consol, "", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			AssertEquals("Precondition - Shipment created for AIR consol must be AIR.", TransportModes.Air, shipment.TransportMode);
			CreateWorkflowTemplateForReceiveConsignment_SendBKCEventToForwarder();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);
			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("No booking confirmed event from TW yet.", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			MasterFilesTestHelper.RunLogWalker();

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var bkcEvent = shipmentInAnotherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			Assert("Booking confirmed event is added to shipment.", bkcEvent.CheckReferenceEquals("New|FAC=CFS|LOC=Sydney|TYP=Transit Receive|RFN=RC00000001|WHS=TRW"));
		}

		#endregion

		#region TestTWToForwarding_UpdateContainerNumberInTransitHeader

		public void TestTWToForwarding_UpdateContainerNumberInRTU()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHZ");

			var consolWith20GPContainer = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consolWith20GPContainer.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var containerOf20GP = CreateContainer(consolWith20GPContainer, "CONT1", 1, "20GP");

			var consolWith40GPContainer = CreateConsol("MSB2", vessel, "AUADL", "NLEUG");
			consolWith40GPContainer.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var containerOf40GP = CreateContainer(consolWith40GPContainer, "CONT1", 1, "40GP");

			var shipment = CreateShipment("HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.Consols.Add(consolWith20GPContainer);
			shipment.Consols.Add(consolWith40GPContainer);

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, containerOf20GP);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Case, containerOf40GP);

			TriggerAndFireTransitRequestUsingBookingRequested(consolWith20GPContainer);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: Two Packages should be created.", 2, receiveConsignment.PackageStates.Count);

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: An RTU should be created for the ASN.", rtu);

			CreateWorkflowTemplateForTransitWarehouseToSendCIDEventToForwarder(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code);

			rtu.WRH_VehicleReference = "CONT2";
			TriggerAndFireTransitHeaderCIDEvent(rtu, "|TYP=ContainerID|NEW=CONT2|OLD=CONT1");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_Table, JobContainerSchema.Constants.TableName);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);

			var containerCIDEventLogs = newFactory.Load<StmALog>(filter);
			AssertCollectionContains("|NEW=CONT2|OLD=CONT1|TYP=ContainerID", containerCIDEventLogs.Select(l => l.SL_Reference).ToArray());

			//uncomment these lines after "WI00573600 - CID events targetted to Consol can update Container IDs" check in
			//var containers = newFactory.Load<ForwardingContainer>(new ZQuery());
			//AssertEquals("Precondition: Two containers should be in the database", 2, containers.Count());
			//AssertEquals(1, containers.Count(c => c.JC_ContainerNum == "CONT1" && c.RefContainer.RC_Code == "40GP"));
			//AssertEquals(1, containers.Count(c => c.JC_ContainerNum == "CONT2" && c.RefContainer.RC_Code == "20GP"));
		}

		public void TestTWToForwarding_UpdateContainerNumberInDTU()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHZ", createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: true);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var dtus = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Precondition: A single DTU should be created.", 1, dtus.Length);
			var dtu = dtus.Single();

			CreateWorkflowTemplateForTransitWarehouseToSendCIDEventToForwarder(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code);

			dtu.WDH_VehicleReference = "CONT2";
			TriggerAndFireTransitHeaderCIDEvent(dtu, "|TYP=ContainerID|NEW=CONT2|OLD=CONT1");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_Table, JobContainerSchema.Constants.TableName);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);

			var containerCIDEventLogs = newFactory.Load<StmALog>(filter);
			AssertCollectionContains("|NEW=CONT2|OLD=CONT1|TYP=ContainerID", containerCIDEventLogs.Select(l => l.SL_Reference).ToArray());

			//uncomment these lines after "WI00573600 - CID events targetted to Consol can update Container IDs" check in
			//var containers = Factory.Load<ForwardingContainer>(new ZQuery());
			//AssertEquals("Precondition: A single container should be in the database", 1, containers.Count());
			//AssertEquals(1, containers.Count(c => c.JC_ContainerNum == "CONT2" && c.RefContainer.RC_Code == "20GP"));
		}

		#endregion

		#region TestTWToForwarding_PopulatePAN

		public void TestTWToForwarding_PopulatePAN_OnlyHasPEN() => TestTWToForwarding_PopulatePAN(true, false);

		public void TestTWToForwarding_PopulatePAN_OnlyHasPAN() => TestTWToForwarding_PopulatePAN(true, false);

		public void TestTWToForwarding_PopulatePAN_HasPENAndPAN() => TestTWToForwarding_PopulatePAN(true, false);

		void TestTWToForwarding_PopulatePAN(bool hasPEN, bool hasPAN)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: false, cfsPortCode: "FRBLV");

			var shipment = CreateShipment("HSB1", "FRBLV", "NZCHC", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 3, Constants.PkgUnit.Pallet);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var packageStateToUnload = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			if (hasPEN)
			{
				Helper.CreateCustomsAdditionalReference(receiveConsignment, "PEN", "BBE001", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "FR");
			}
			if (hasPAN)
			{
				Helper.CreateCustomsAdditionalReference(receiveConsignment, "PAN", "BBE002", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "FR");
			}

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP");

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			var packageStateToRelabel = UnloadAndLabelPackage(packageStateToUnload, rtu, "P1");
			UnloadAndLabelPackage(packageStateToUnload, rtu, "P2");
			UnloadAndLabelPackage(packageStateToUnload, rtu, "P3");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);

			var expectedPortReferenceNumber = hasPAN ? "BBE002" : "BBE001";
			AssertPackageID(shipment.PK, "P1", expectedPortReferenceNumber);
		}

		#endregion

		#region TestTWToForwarding_RelabelPackageIDMultipleTimes

		public void TestTWToForwarding_RelabelPackageIDMultipleTimes()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: false);

			var shipment = CreateShipment("HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 3, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Length);
			var receiveConsignment = receiveConsignments.Single();
			AssertEquals("Precondition: A single Package should be created.", 1, receiveConsignment.PackageStates.Count);
			var packageStateToUnload = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			WhsItemReceiveTransportationUnit rtu = null;

			AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
			rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP");

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			var packageStateToRelabel = UnloadAndLabelPackage(packageStateToUnload, rtu, "P1");
			UnloadAndLabelPackage(packageStateToUnload, rtu, "P2");
			UnloadAndLabelPackage(packageStateToUnload, rtu, "P3");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment);
			AssertPackageID(shipment.PK, "P1");

			packageStateToRelabel.Package.KP_PackageID = "A1";
			packageStateToRelabel.Package.KP_PreviousPackageID = "P1";
			TriggerAndFireOutturn(receiveConsignment);
			AssertPackageID(shipment.PK, "A1");

			packageStateToRelabel.Package.KP_PackageID = "B1";
			packageStateToRelabel.Package.KP_PreviousPackageID = "A1";
			TriggerAndFireOutturn(receiveConsignment);
			AssertPackageID(shipment.PK, "B1");
		}

		void AssertPackageID(ZGuid shipmentPK, string newPackageID, string expectedExportRefNumber = "")
		{
			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment = factoryToLoadShipment.Load<ForwardingShipment>(shipmentPK);
			var packlinesInShipment = shipment.OuterPackLines;
			AssertEquals(1, packlinesInShipment.Count);

			var packline = packlinesInShipment.Cast<ForwardingPackLine>().Single();
			AssertEquals(1, packline.PkgPackageCollection.Count(p => p.KP_PackageID == newPackageID));
			AssertEquals(1, packline.PkgPackageCollection.Count(p => p.KP_PackageID == "P2"));
			AssertEquals(1, packline.PkgPackageCollection.Count(p => p.KP_PackageID == "P3"));
			if (!expectedExportRefNumber.IsNullOrEmpty())
			{
				AssertEquals(expectedExportRefNumber, packline.JL_ExportRefNumber);
			}
		}

		#endregion

		#region TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR

		public void TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR_AIR() => TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR("ULD", "AAA");

		public void TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR_CNT() => TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR("CNT", "20GP");

		public void TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR_VEHWithContainer() => TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR("VEH", "DROP");

		public void TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR_VEHWithoutContainer() => TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR("VEH");

		void TestTWToForwarding_CreateASNRTUPivotToSendATCEventToFOR(string unitType, string containerType = "")
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_ConsolMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			AssertEquals("The Consol should don't have a single container.", 0, consol.Containers.Count);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var asn = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			WhsItemReceiveTransportationUnit rtu;
			if (string.IsNullOrEmpty(containerType))
			{
				rtu = Helper.CreateReceiveTransportationUnit("TR001", warehouse.PK, warehouse.DefaultLocation.PK);
			}
			else
			{
				rtu = Helper.CreateReceiveTransportationUnitWithContainerType("TR001", warehouse.PK, warehouse.DefaultLocation.PK, containerID: "CNT1", containerType: containerType, unitType, vehicleRef: unitType + "_RTU");
			}
			Factory.Save();

			var pivot = Factory.NewWithValidTestData<WhsItemReceiveASNRTUPivot>();
			pivot.WAR_WRH_TransitReceiveTransportationUnit = rtu.PK;
			pivot.WAR_WRP_TransitReceiveASN = asn.PK;
			Factory.Save();

			CreateWorkflowTemplateForTransitWarehouseToSendATCEventToForwarder(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code);

			var factoryToLoadRTU = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtuInAnotherFactory = factoryToLoadRTU.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			TriggerAndFireTransitHeaderATCEvent(rtuInAnotherFactory);

			var factoryToLoadConsol = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInAnotherFactory = factoryToLoadConsol.Load<ForwardingConsol>(consol.PK);
			if (string.IsNullOrEmpty(containerType))
			{
				AssertEquals("The Consol should don't have a new container.", 0, consolInAnotherFactory.Containers.Count);
			}
			else
			{
				AssertEquals("The Consol should have a new container.", 1, consolInAnotherFactory.Containers.Count);
				var newContainer = consolInAnotherFactory.Containers.Cast<ForwardingContainer>().Single();
				switch (unitType)
				{
					case "CNT":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "CNT_RTU");
						break;
					case "ULD":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "ULD_RTU");
						break;
					case "VEH":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "VEH_RTU");
						break;
				}
			}
		}

		#endregion

		#region TestTWToForwarding_CreateDLLDTUPivotToSendATCEventToFOR

		public void TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR_AIR() => TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR("ULD", "AAA");

		public void TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR_CNT() => TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR("CNT", "20GP");

		public void TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR_VEHWithContainer() => TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR("VEH", "DROP");

		public void TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR_VEHWithoutContainer() => TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR("VEH");

		void TestTWToForwarding_CreateDLLDTDUPivotToSendATCEventToFOR(string unitType, string containerType = "")
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_ConsolMode = CMRCargoTypes.Codes.FullContainerLoad;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			AssertEquals("The Consol should don't have a single container.", 0, consol.Containers.Count);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = factoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();

			WhsItemDispatchTransportationUnit dtu;
			if (string.IsNullOrEmpty(containerType))
			{
				dtu = Helper.CreateDispatchTransportationUnit("TD001", warehouse.PK);
			}
			else
			{
				dtu = Helper.CreateDispatchTransportationUnitWithContainerType("TD001", warehouse.PK, containerID: "CNT1", containerType: containerType);
				dtu.WDH_VehicleReference = unitType + "_DTU";
			}
			Factory.Save();

			var pivot = Factory.NewWithValidTestData<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDH_TransitDispatchTransportationUnit = dtu.PK;
			pivot.WLD_WDL_TransitDispatchLoadList = loadList.PK;
			Factory.Save();

			CreateWorkflowTemplateForTransitWarehouseToSendATCEventToForwarder(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code);

			var factoryToLoadDTU = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuInAnotherFactory = factoryToLoadDTU.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			TriggerAndFireTransitHeaderATCEvent(dtuInAnotherFactory);

			var factoryToLoadConsol = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInAnotherFactory = factoryToLoadConsol.Load<ForwardingConsol>(consol.PK);
			if (string.IsNullOrEmpty(containerType))
			{
				AssertEquals("The Consol should don't have a new container.", 0, consolInAnotherFactory.Containers.Count);
			}
			else
			{
				AssertEquals("The Consol should have a new container.", 1, consolInAnotherFactory.Containers.Count);
				var newContainer = consolInAnotherFactory.Containers.Cast<ForwardingContainer>().Single();
				switch (unitType)
				{
					case "CNT":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "CNT_DTU");
						break;
					case "ULD":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "ULD_DTU");
						break;
					case "VEH":
						AssertEquals("The container name should be equal.", newContainer.JC_ContainerNum, "VEH_DTU");
						break;
				}
			}
		}

		#endregion

		#region TestTWToShipment_SendDispatchPacksFilterPackageByOutturnAndReferenceNumber

		public void TestTWToShipment_SendDispatchPacksFilterPackageByOutturnAndReferenceNumber()
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container = CreateContainer(consol, "CONT1");
			var containerWithDtu = CreateContainer(consol, "CONT2");
			var dll = Helper.CreateDispatchLoadList("DLL1", testData.warehouse.PK);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			var packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("A single RCN should be created.", 1, receiveConsignments.Length);

			var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("A single DCN should be created.", 1, dispatchConsignments.Length);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK, "CONT1", "40GP");
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("TD001", testData.warehouse.PK, "CONT2", "40GP");

			var dispatchConsignment = dispatchConsignments.Single();
			var packageState1 = dispatchConsignment.PackageStates[0];
			var packageState2 = dispatchConsignment.PackageStates[1];
			var packageState3 = dispatchConsignment.PackageStates[2];
			packageState1.Package.KP_PackageID = "P1";
			packageState2.Package.KP_PackageID = "P2";
			packageState3.Package.KP_PackageID = "P3";

			UnloadAndLabelPackage(packageState2, rtu, "P2", setDetails: true);
			UnloadAndLabelPackage(packageState3, rtu, "P3", setDetails: true);
			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignments.Single());

			Factory.Save();
			LoadPackage(packageState3, testData.warehouse.DefaultLocation, rtu, dtu, dll);
			packageState3.Package.KP_Weight = 2;
			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignments.Single());

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var pkgJobs = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_JobID, shipmentInAnotherFactory.JS_UniqueConsignRef)).Single();

			AssertEquals("Two package in PkgPackageJob.", 2, pkgJobs.Packages.Count);
			AssertContainsExactElementsInAnyOrder("Check PackageId.", new[] { "P2", "P3" }, new[] { pkgJobs.Packages[0].KP_PackageID, pkgJobs.Packages[1].KP_PackageID });

			var packlines = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals("FOR's packline should be update with PackageId P2", 1, packlines.Where(p => p.PkgPackageCollection.Any(pkg => pkg.KP_PackageID == "P2" && pkg.KP_Weight == packageState2.Package.KP_Weight)).Count());
			AssertEquals("FOR's packline should be update with PackageId P3", 1, packlines.Where(p => p.PkgPackageCollection.Any(pkg => pkg.KP_PackageID == "P3" && pkg.KP_Weight == packageState3.Package.KP_Weight)).Count());
		}

		void LoadPackage(WhsItemPackageState packageState, WhsLocation location, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll)
		{
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WDH_TransitDispatchHeader = dtu.PK;
			packageState.WPS_WDL_LoadList = dll.PK;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";
		}

		#endregion

		#region TestTWToForwarding_DispatchIsSplit

		public void TestTWToForwarding_DispatchIsSplit()
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container = CreateContainer(consol, "CONT1");
			var containerWithDtu = CreateContainer(consol, "CONT2");
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "P1");
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("A single DCN should be created.", 1, dispatchConsignments.Length);

			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK, "CONT1", "40GP");

			var dispatchConsignment = dispatchConsignments.Single();
			var packageState1 = dispatchConsignment.PackageStates[0];
			var packageState2 = dispatchConsignment.PackageStates[1];

			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DCN2", dispatchConsignment.WDC_WW_Warehouse, dispatchConsignment.BookingPartyDocAddress.Organisation);
			dispatchConsignment2.WDC_ParentID = shipment.PK;
			dispatchConsignment2.WDC_ParentTableCode = "JS";
			dispatchConsignment2.WDC_JobID = "DC0002";

			packageState1.WPS_WDC_TransitDispatchConsignment = dispatchConsignment2.PK;
			packageState1.Package.KP_F3_NKPackType = Constants.PkgUnit.Case;
			packageState2.Package.KP_F3_NKPackType = Constants.PkgUnit.Case;

			UnloadAndLabelPackage(packageState1, rtu, "P1", setDetails: true);
			UnloadAndLabelPackage(packageState2, rtu, "P2", setDetails: true);

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);

			var packlines = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals("the IsSplit of dispatchConsignment2 is true", true, dispatchConsignment2.IsSplit);
			AssertEquals("FOR's packline should be update", 1, packlines.Where(p => p.PkgPackageCollection.Any(pkg => pkg.KP_F3_NKPackType == Constants.PkgUnit.Case)).Count());
		}

		#endregion

		// Comment out until Overpack is exposed in TWH and FOR makes changes to their readers
		//#region TestTWSendingOutturnedHandlingUnitsAndInnersToForwardingShipment

		//public void TestTWSendingOutturnedHandlingUnitsToForwardingShipment_CreatesNewPacklinesAndInners_TriggerFromRCN()
		//{
		//	var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

		//	var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
		//	consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

		//	ForwardingContainer container = null;
		//	container = CreateContainer(consol, "CONT1", 1, "20GP");
		//	container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

		//	var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);

		//	TriggerAndFireTransitRequestUsingBookingRequested(consol);

		//	var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
		//	AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Count());
		//	var receiveConsignment = receiveConsignments.Single();
		//	AssertEquals("Precondition: No Package should be created.", 0, receiveConsignment.PackageStates.Count);

		//	AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
		//	var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("CONT1", warehouse.PK, warehouse.DefaultLocation.PK, "CONT1", "40GP", "ULD");

		//	var dateTimeNow = DateTimeOffset.Now;
		//	var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
		//	rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
		//	rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
		//	rtu.WRH_UnloadCompleteTime = dateTimeOffset;

		//	var singleLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var multiLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var subHandlingUnit = Helper.CreatePackageHandlingUnit();

		//	var singleLevelHandlingUnitPackageState = CreatePackageStateForTesting("Single", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, singleLevelHandlingUnit, true, "MN_1", weight: 10, length: 1, width: 1, height: 1);
		//	var multiLevelHandlingUnitPackageState = CreatePackageStateForTesting("Multi", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, multiLevelHandlingUnit, true, "MN_2", weight: 20, length: 2, width: 2, height: 2);
		//	var subHandlingUnitPackageState = CreatePackageStateForTesting("Sub", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, subHandlingUnit, true, "MN_3", weight: 30, length: 3, width: 3, height: 3);
		//	singleLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	multiLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	subHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;

		//	var singleLevelHandlingUnitInner1 = CreatePackageStateForTesting("Single_Inner1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, null, weight: 1, length: 1, width: 1, height: 1);
		//	var singleLevelHandlingUnitInner2 = CreatePackageStateForTesting("Single_Inner2", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, null, weight: 2, length: 2, width: 2, height: 2);
		//	var multiLevelHandlingUnitInner = CreatePackageStateForTesting("Multi_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Carton, rtu, receiveConsignment, null, null, weight: 3, length: 3, width: 3, height: 3);
		//	var subHandlingUnitInner = CreatePackageStateForTesting("Sub_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Box, rtu, receiveConsignment, null, null, weight: 4, length: 4, width: 4, height: 4);
		//	var standAlonePackageState = CreatePackageStateForTesting("StandAlone", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Box, rtu, receiveConsignment, null, null, marksAndNumbers: "MN_4", weight: 5, length: 5, width: 5, height: 5);

		//	// Setting up the handling unit trees before exporting to forwarding
		//	//	Single
		//	//		Single_Inner1
		//	//		Single_Inner2
		//	//	Multi
		//	//		Multi_Inner1
		//	//		Sub
		//	//			Sub_Inner
		//	//	StandAlone
		//	Helper.DisableTopLevelHUFKForTest(TestConnection);
		//	Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner1, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner2, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, multiLevelHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackageState, subHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, subHandlingUnitPackageState, ZDateTimeOffset.Now, "BOB");
		//	Factory.Save();

		//	var packages = Factory.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("Precondition - no packages linked to shipment before export.", 0, packages.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());
		//	AssertEquals("Precondition - no packLines before export.", 0, Factory.Load<ForwardingPackLine>(new ZQuery()).Count());

		//	// Export to forwarding
		//	CreateWorkflowTemplateForReceiveConsignmentToShipment();
		//	TriggerAndFireToShipment(receiveConsignment);

		//	var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentAfterExport = newFactory.Load<ForwardingShipment>(shipment.PK);
		//	var ediMessage = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
		//	var packlinesAfterExport = shipmentAfterExport.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	var packagesAfterExport = newFactory.Load<PkgPackage>(new ZQuery());

		//	AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
		//	AssertEquals("3 outer packlines should have been created.", 3, packlinesAfterExport.Count());
		//	AssertEquals("There should be 8 packages linked to shipment after export.", 8, packagesAfterExport.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());

		//	var standAlonePackagePackline = AssertAndReturnForwardingPackLine(packlinesAfterExport, "StandAlone", "MN_4", 5m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
		//	var singleLevelHandlingUnitPackline = AssertAndReturnForwardingPackLine(packlinesAfterExport, "Single", "MN_1", 10m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Pallet);
		//	var multiLevelHandlingUnitPackline = AssertAndReturnForwardingPackLine(packlinesAfterExport, "Multi", "MN_2", 20m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var standAlonePackageAfterExport = AssertAndReturnPackage(standAlonePackagePackline.PkgPackageCollection, shipment.PK, 1, "StandAlone", standAlonePackagePackline.JL_PackLineId, "MN_4", 5m, "KG", 5m, 5m, 5m, "CM", 1, "BOX");
		//	var singleLevelHandlingUnitAfterExport = AssertAndReturnPackage(singleLevelHandlingUnitPackline.PkgPackageCollection, shipment.PK, 1, "Single", singleLevelHandlingUnitPackline.JL_PackLineId, "MN_1", 10m, "KG", 1m, 1m, 1m, "CM", 1, "PLT");
		//	var multiLevelHandlingUnitAfterExport = AssertAndReturnPackage(multiLevelHandlingUnitPackline.PkgPackageCollection, shipment.PK, 1, "Multi", multiLevelHandlingUnitPackline.JL_PackLineId, "MN_2", 20m, "KG", 2m, 2m, 2m, "CM", 1, "PLT");

		//	AssertEquals("StandAlone package should not have any child.", 0, standAlonePackageAfterExport.Packages.Count());

		//	var singleLevelHandlingUnitPackageInners = singleLevelHandlingUnitAfterExport.Packages.ToArray();
		//	AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner1", string.Empty, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Package);
		//	AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner2", string.Empty, string.Empty, 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Package);

		//	var multiLevelHandlingUnitPackageInners = multiLevelHandlingUnitAfterExport.Packages.ToArray();
		//	AssertAndReturnPackage(multiLevelHandlingUnitPackageInners, shipment.PK, 2, "Multi_Inner", string.Empty, string.Empty, 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Carton);
		//	var subHandlingUnitAfterExport = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners.ToArray(), shipment.PK, 2, "Sub", string.Empty, "MN_3", 30m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var subHandlingUnitPackageInners = subHandlingUnitAfterExport.Packages.ToArray();
		//	AssertAndReturnPackage(subHandlingUnitPackageInners, shipment.PK, 1, "Sub_Inner", string.Empty, string.Empty, 4m, "KG", 4m, 4m, 4m, "CM", 1, Constants.PkgUnit.Box);
		//}

		//public void TestTWSendingOutturnedHandlingUnitsToForwardingShipment_UpdatesExistingPacklinesAndPackages_TriggerFromRCN()
		//{
		//	var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

		//	var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
		//	consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

		//	ForwardingContainer container = null;
		//	container = CreateContainer(consol, "CONT1", 1, "20GP");
		//	container.JC_ContainerMode = CMRCargoTypes.Codes.FullContainerLoad;

		//	var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
		//	var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, weight: 1, length: 1, width: 1, height: 1);
		//	var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Carton, weight: 2, length: 2, width: 2, height: 2);
		//	var packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, weight: 3, length: 3, width: 3, height: 3);

		//	TriggerAndFireTransitRequestUsingBookingRequested(consol);

		//	AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
		//	AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);
		//	AssertNotNullOrEmpty("Precondition: Packline id is set", packline3.JL_PackLineId);

		//	var packages = Factory.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("Precondition - no packages linked to shipment before export.", 0, packages.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());

		//	var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
		//	AssertEquals("3 Packages should have been created.", 3, receiveConsignment.PackageStates.Count);

		//	var packageStateForPKG = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package);
		//	var packageStateForCTN = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Carton);
		//	var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
		//	AssertEquals(1, packageStateForPKG.Package.KP_PackageQty);
		//	AssertEquals(1, packageStateForCTN.Package.KP_PackageQty);
		//	AssertEquals(1, packageStateForBox.Package.KP_PackageQty);
		//	AssertEquals(packline1.JL_PackLineId, packageStateForPKG.Package.KP_ExternalReference);
		//	AssertEquals(packline2.JL_PackLineId, packageStateForCTN.Package.KP_ExternalReference);
		//	AssertEquals(packline3.JL_PackLineId, packageStateForBox.Package.KP_ExternalReference);

		//	var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
		//	UnloadAndLabelPackage(packageStateForPKG, rtu1, "PKG1");

		//	var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
		//	UnloadAndLabelPackage(packageStateForCTN, rtu2, "CTN1");
		//	UnloadAndLabelPackage(packageStateForBox, rtu2, "BOX1");

		//	//Received Overs
		//	var packageStateForCAS = CreatePackageStateForTesting("CAS1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Case, rtu1, receiveConsignment, null, null, null, null, true, "MN_1", weight: 1m, length: 1m, width: 1m, height: 1m);

		//	// Build Handling Units inside the warehouse
		//	var singleLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var multiLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var subHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var singleLevelHandlingUnitPackageState = CreatePackageStateForTesting("Single", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, singleLevelHandlingUnit, true, "MN_2", weight: 2m, length: 2m, width: 2m, height: 2m);
		//	var multiLevelHandlingUnitPackageState = CreatePackageStateForTesting("Multi", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, multiLevelHandlingUnit, true, "MN_3", weight: 3m, length: 3m, width: 3m, height: 3m);
		//	var subHandlingUnitPackageState = CreatePackageStateForTesting("Sub", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, null, null, subHandlingUnit, true, "MN_4", weight: 4m, length: 4m, width: 4m, height: 4m);
		//	singleLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu1.WRH_WL_StagingLocation;
		//	multiLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu1.WRH_WL_StagingLocation;
		//	subHandlingUnitPackageState.WPS_WL_LastLocation = rtu1.WRH_WL_StagingLocation;

		//	// Setting up the handling unit tree before exporting to forwarding
		//	//	Single
		//	//		PKG1
		//	//	Multi
		//	//		CTN1
		//	//		Sub
		//	//			BOX1
		//	//	CAS1
		//	Helper.DisableTopLevelHUFKForTest(TestConnection);
		//	Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, packageStateForPKG, ZDateTimeOffset.Now, "BOB");
		//	var packageStateForCTN1Divot = Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, packageStateForCTN, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackageState, packageStateForBox, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, subHandlingUnitPackageState, ZDateTimeOffset.Now, "BOB");
		//	Factory.Save();

		//	// Export to forwarding
		//	CreateWorkflowTemplateForReceiveConsignmentToShipment();
		//	TriggerAndFireToShipment(receiveConsignment);

		//	var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentFromNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
		//	var packLinesAfterExport = shipmentFromNewFactory.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	var packagesAfterExport = newFactory.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("7 packages linked to shipment after export.", 7, packagesAfterExport.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());
		//	AssertEquals("3 new outer packlines created for Single, Multi and CAS1, existing packlines will remain.", 6, packLinesAfterExport.Count());

		//	var packLineForPKG = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Package);
		//	var packLineForCTN = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
		//	var packLineForBOX = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
		//	AssertEquals("Same Packlines as before.", packLineForPKG.PK, packline1.PK);
		//	AssertEquals("Same Packlines as before.", packLineForCTN.PK, packline2.PK);
		//	AssertEquals("Same Packlines as before.", packLineForBOX.PK, packline3.PK);
		//	AssertEquals("No PkgPackage created for the old packline.", 0, packLineForPKG.PkgPackageCollection.Count());
		//	AssertEquals("No PkgPackage created for the old packline.", 0, packLineForCTN.PkgPackageCollection.Count());
		//	AssertEquals("No PkgPackage created for the old packline.", 0, packLineForBOX.PkgPackageCollection.Count());

		//	var packLineForCAS1 = AssertAndReturnForwardingPackLine(packLinesAfterExport, "CAS1", "MN_1", 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Case);
		//	var packLineForSingle = AssertAndReturnForwardingPackLine(packLinesAfterExport, "Single", "MN_2", 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Pallet);
		//	var packLineForMulti = AssertAndReturnForwardingPackLine(packLinesAfterExport, "Multi", "MN_3", 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var packageForCAS1AfterExport = AssertAndReturnPackage(packLineForCAS1.PkgPackageCollection, shipment.PK, 1, "CAS1", packLineForCAS1.JL_PackLineId, "MN_1", 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Case);
		//	AssertEquals("CAS1 should not have any child.", 0, packageForCAS1AfterExport.Packages.Count());

		//	var packageForSingleAfterExport = AssertAndReturnPackage(packLineForSingle.PkgPackageCollection, shipment.PK, 1, "Single", packLineForSingle.JL_PackLineId, "MN_2", 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Pallet);
		//	var packageForMultiAfterExport = AssertAndReturnPackage(packLineForMulti.PkgPackageCollection, shipment.PK, 1, "Multi", packLineForMulti.JL_PackLineId, "MN_3", 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var singleInnerPackages = packageForSingleAfterExport.Packages.ToArray();
		//	AssertAndReturnPackage(singleInnerPackages, shipment.PK, 1, "PKG1", packLineForPKG.JL_PackLineId, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Package);

		//	var multiInnerPackages = packageForMultiAfterExport.Packages.ToArray();
		//	var packageForCTN1 = AssertAndReturnPackage(multiInnerPackages, shipment.PK, 2, "CTN1", packLineForCTN.JL_PackLineId, string.Empty, 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton);
		//	AssertEquals("CTN1 should not have any child.", 0, packageForCTN1.Packages.Count());

		//	var packageForSubAfterExport = AssertAndReturnPackage(multiInnerPackages, shipment.PK, 2, "Sub", string.Empty, "MN_4", 4m, "KG", 4m, 4m, 4m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var subInnerPackages = packageForSubAfterExport.Packages.ToArray();
		//	var packageForBOX1 = AssertAndReturnPackage(subInnerPackages, shipment.PK, 1, "BOX1", packLineForBOX.JL_PackLineId, string.Empty, 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Box);
		//	AssertEquals("BOX1 should not have any child.", 0, packageForBOX1.Packages.Count());

		//	// Modify the packages in TW then resend outturn to FOR
		//	var multiHandlingUnitAfterExport = newFactory.Load<WhsItemPackageState>(multiLevelHandlingUnitPackageState.PK);
		//	multiHandlingUnitAfterExport.Package.KP_Weight = 50m;

		//	var cTNPackageDivotAfterExport = newFactory.Load<PkgPackageHandlingUnitDivot>(packageStateForCTN1Divot.PK);
		//	// Unpack CTN from Multi
		//	Helper.UnpackPackageFromHandlingUnit(cTNPackageDivotAfterExport, DateTime.Now, "TOM");

		//	var singleHandlingUnitAfterExport = newFactory.Load<WhsItemPackageState>(singleLevelHandlingUnitPackageState.PK);
		//	singleHandlingUnitAfterExport.Package.KP_WeightUQ = "MG";

		//	// Pack a BAG into Single
		//	var packageForBAG = CreatePackageStateForTesting("BAG1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Bag, rtu1, receiveConsignment, null, weight: 1, length: 1, width: 1, height: 1);
		//	Helper.PackPackageIntoHandlingUnit(singleHandlingUnitAfterExport, packageForBAG, ZDateTimeOffset.Now, "TIM");
		//	Factory.Save();

		//	var pKGPackageStateAfterExport = newFactory.Load<WhsItemPackageState>(packageStateForPKG.PK);
		//	pKGPackageStateAfterExport.Package.KP_MarksAndNumbers = "MN_Updated";

		//	var subHandlingUnitAfterExport = newFactory.Load<WhsItemPackageState>(subHandlingUnitPackageState.PK);
		//	subHandlingUnitAfterExport.Package.KP_Length = 50m;

		//	var cASPackageStateAfterExport = newFactory.Load<WhsItemPackageState>(packageStateForCAS.PK);
		//	cASPackageStateAfterExport.Package.KP_Height = 50m;

		//	var bOXPackageStateAfterExport = newFactory.Load<WhsItemPackageState>(packageStateForBox.PK);
		//	bOXPackageStateAfterExport.Package.KP_DimensionUQ = "MM";

		//	var receiveConsignmentAfterExport = newFactory.Load<WhsItemReceiveConsignment>(receiveConsignment.PK);

		//	newFactory.Save();

		//	// Export to forwarding again to update package fields
		//	TriggerAndFireToShipment(receiveConsignmentAfterExport);

		//	var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
		//	shipmentFromNewFactory = newFactory2.Load<ForwardingShipment>(shipment.PK);
		//	packLinesAfterExport = shipmentFromNewFactory.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	packagesAfterExport = newFactory2.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("1 new package created for BAG1 and linked to the shipment.", 8, packagesAfterExport.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());
		//	AssertEquals("Unpacked CTN matched to the existing CTN packline.", 6, packLinesAfterExport.Count());

		//	packLineForPKG = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Package);
		//	packLineForCTN = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
		//	packLineForBOX = packLinesAfterExport.Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
		//	AssertEquals("Same Packlines as before.", packLineForPKG.PK, packline1.PK);
		//	AssertEquals("Same Packlines as before.", packLineForCTN.PK, packline2.PK);
		//	AssertEquals("Same Packlines as before.", packLineForBOX.PK, packline3.PK);
		//	AssertEquals("No PkgPackage created for the old packline.", 0, packLineForPKG.PkgPackageCollection.Count());
		//	AssertEquals("PkgPackage created for the matching packline.", 1, packLineForCTN.PkgPackageCollection.Count());
		//	AssertEquals("No PkgPackage created for the old packline.", 0, packLineForBOX.PkgPackageCollection.Count());

		//	packLineForCAS1 = AssertAndReturnForwardingPackLine(packLinesAfterExport, "CAS1", "MN_1", 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Case);
		//	packLineForSingle = AssertAndReturnForwardingPackLine(packLinesAfterExport, "Single", "MN_2", 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Pallet);
		//	packLineForMulti = AssertAndReturnForwardingPackLine(packLinesAfterExport, "Multi", "MN_3", 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	packageForCAS1AfterExport = AssertAndReturnPackage(packLineForCAS1.PkgPackageCollection, shipment.PK, 1, "CAS1", string.Empty, "MN_1", 1m, "KG", 1m, 1m, 50m, "CM", 1, Constants.PkgUnit.Case);
		//	AssertEquals("CAS1 should not have any child.", 0, packageForCAS1AfterExport.Packages.Count());

		//	packageForSingleAfterExport = AssertAndReturnPackage(packLineForSingle.PkgPackageCollection, shipment.PK, 1, "Single", string.Empty, "MN_2", 2m, "MG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Pallet);
		//	packageForMultiAfterExport = AssertAndReturnPackage(packLineForMulti.PkgPackageCollection, shipment.PK, 1, "Multi", string.Empty, "MN_3", 48m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	singleInnerPackages = packageForSingleAfterExport.Packages.ToArray();
		//	var packageForPKG1 = AssertAndReturnPackage(singleInnerPackages, shipment.PK, 2, "PKG1", packLineForPKG.JL_PackLineId, "MN_Updated", 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Package);
		//	var packageForBAG1 = AssertAndReturnPackage(singleInnerPackages, shipment.PK, 2, "BAG1", string.Empty, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Bag);
		//	AssertEquals("PKG1 should not have any child.", 0, packageForPKG1.Packages.Count());
		//	AssertEquals("BAG1 should not have any child.", 0, packageForBAG1.Packages.Count());

		//	multiInnerPackages = packageForMultiAfterExport.Packages.ToArray();
		//	packageForSubAfterExport = AssertAndReturnPackage(multiInnerPackages, shipment.PK, 1, "Sub", string.Empty, "MN_4", 4m, "KG", 50m, 4m, 4m, "CM", 1, Constants.PkgUnit.Pallet);

		//	subInnerPackages = packageForSubAfterExport.Packages.ToArray();
		//	packageForBOX1 = AssertAndReturnPackage(subInnerPackages, shipment.PK, 1, "BOX1", packLineForBOX.JL_PackLineId, string.Empty, 3m, "KG", 3m, 3m, 3m, "MM", 1, Constants.PkgUnit.Box);
		//	AssertEquals("BOX1 should not have any child.", 0, packageForBOX1.Packages.Count());
		//}

		//public void TestTWSendingOutturnedHandlingUnitsToForwardingShipment_CreatesNewPacklinesAndInners_TriggerFromDCN()
		//{
		//	var testData = CreateTestDataForCombined();
		//	var warehouse = testData.warehouse;
		//	// workflow template
		//	//   BookingRequested event send Combined Instructions
		//	//   BookingConfirmed event send Dispatch Instructions
		//	CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
		//	var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
		//	consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

		//	var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

		//	var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");

		//	var packlineInContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1);

		//	TriggerAndFireTransitRequestUsingBookingRequested(consol);

		//	var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
		//	AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Count());
		//	var receiveConsignment = receiveConsignments.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, receiveConsignment.PackageStates.Count);

		//	var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
		//	AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Count());
		//	var dispatchConsignment = dispatchConsignments.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, dispatchConsignment.PackageStates.Count);

		//	var dispatchLoadLists = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
		//	AssertEquals("Precondition: A single DLL should be created.", 1, dispatchLoadLists.Count());
		//	var loadList = dispatchLoadLists.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, loadList.PackageStates.Count);

		//	var dtus = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
		//	AssertEquals("One DTUs are created since there is one container.", 1, dtus.Count());
		//	var dtu = dtus.Single();

		//	AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
		//	var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

		//	var dateTimeNow = DateTimeOffset.Now;
		//	var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
		//	rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
		//	rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
		//	rtu.WRH_UnloadCompleteTime = dateTimeOffset;

		//	var singleLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var multiLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var subHandlingUnit = Helper.CreatePackageHandlingUnit();

		//	var standAlonePackageState = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == "BOX");
		//	UnloadAndLabelPackage(standAlonePackageState, rtu, "StandAlone");

		//	var singleLevelHandlingUnitPackageState = CreatePackageStateForTesting("Single", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, singleLevelHandlingUnit, true, "MN_1", weight: 10, length: 1, width: 1, height: 1);
		//	var multiLevelHandlingUnitPackageState = CreatePackageStateForTesting("Multi", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, multiLevelHandlingUnit, true, "MN_2", weight: 20, length: 2, width: 2, height: 2);
		//	var subHandlingUnitPackageState = CreatePackageStateForTesting("Sub", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, subHandlingUnit, true, "MN_3", weight: 30, length: 3, width: 3, height: 3);
		//	singleLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	multiLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	subHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;

		//	var singleLevelHandlingUnitInner1 = CreatePackageStateForTesting("Single_Inner1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 1, length: 1, width: 1, height: 1);
		//	var singleLevelHandlingUnitInner2 = CreatePackageStateForTesting("Single_Inner2", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 2, length: 2, width: 2, height: 2);
		//	var multiLevelHandlingUnitInner = CreatePackageStateForTesting("Multi_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Carton, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 3, length: 3, width: 3, height: 3);
		//	var subHandlingUnitInner = CreatePackageStateForTesting("Sub_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Box, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 4, length: 4, width: 4, height: 4);

		//	// Setting up the handling unit trees before exporting to forwarding
		//	//	Single
		//	//		Single_Inner1
		//	//		Single_Inner2
		//	//	Multi
		//	//		Multi_Inner1
		//	//		Sub
		//	//			Sub_Inner
		//	//	StandAlone
		//	Helper.DisableTopLevelHUFKForTest(TestConnection);
		//	var singleInnerDivot = Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner1, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner2, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, multiLevelHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackageState, subHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, subHandlingUnitPackageState, ZDateTimeOffset.Now, "BOB");
		//	Factory.Save();

		//	// Load packages onto DTU
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, multiLevelHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, subHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitInner1);
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitInner2);
		//	Helper.LoadPackage(dtu, multiLevelHandlingUnitInner);
		//	Helper.LoadPackage(dtu, subHandlingUnitInner);
		//	Helper.LoadPackage(dtu, standAlonePackageState);
		//	Factory.Save();

		//	var packages = Factory.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("Precondition - no packages linked to shipment before export.", 0, packages.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());
		//	AssertEquals("Precondition - One packLine before export.", 1, Factory.Load<ForwardingPackLine>(new ZQuery()).Count());

		//	// Export to forwarding
		//	CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
		//	TriggerAndFireToShipment(dispatchConsignment);

		//	var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentAfterExport = newFactory.Load<ForwardingShipment>(shipment.PK);
		//	var ediMessage = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
		//	var packlinesAfterExport = shipmentAfterExport.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	var packagesAfterExport = newFactory.Load<PkgPackage>(new ZQuery());

		//	AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
		//	AssertEquals("1 outer packline for the container should have been created for all packages.", 2, packlinesAfterExport.Count());

		//	var packagesLinkedToShipment = packagesAfterExport.Where(p => p.PackageJob.KJ_ParentID == shipment.PK);
		//	AssertEquals("There should be 8 packages linked to shipment after export.", 8, packagesLinkedToShipment.Count());

		//	var singleLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "Single", string.Empty, "MN_1", 10m, "KG", 1m, 1m, 1m, "CM", 1, "PLT");
		//	var multiLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "Multi", string.Empty, "MN_2", 20m, "KG", 2m, 2m, 2m, "CM", 1, "PLT");
		//	var standAlonePackageAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "StandAlone", packlineInContainer1.JL_PackLineId, string.Empty, 0m, "KG", 0m, 0m, 0m, "CM", 1, "BOX");
		//	AssertEquals("StandAlone package should not have any inners.", 0, standAlonePackageAfterExport.Packages.Count());

		//	var singleLevelHandlingUnitPackageInners = singleLevelHandlingUnitAfterExport.Packages.ToArray();
		//	var singleInner1 = AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner1", string.Empty, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Package);
		//	var singleInner2 = AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner2", string.Empty, string.Empty, 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Package);

		//	var multiLevelHandlingUnitPackageInners = multiLevelHandlingUnitAfterExport.Packages.ToArray();
		//	var multiInner = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners, shipment.PK, 2, "Multi_Inner", string.Empty, string.Empty, 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Carton);
		//	var subHandlingUnitAfterExport = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners.ToArray(), shipment.PK, 2, "Sub", string.Empty, "MN_3", 30m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var subHandlingUnitPackageInners = subHandlingUnitAfterExport.Packages.ToArray();
		//	var subInner = AssertAndReturnPackage(subHandlingUnitPackageInners, shipment.PK, 1, "Sub_Inner", string.Empty, string.Empty, 4m, "KG", 4m, 4m, 4m, "CM", 1, Constants.PkgUnit.Box);
		//}

		//public void TestTWSendingOutturnedHandlingUnitsToForwardingShipment_UpdatesExistingPacklinesAndPackages_TriggerFromDCN()
		//{
		//	var testData = CreateTestDataForCombined();
		//	var warehouse = testData.warehouse;
		//	// workflow template
		//	//   BookingRequested event send Combined Instructions
		//	//   BookingConfirmed event send Dispatch Instructions
		//	CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
		//	var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
		//	consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

		//	var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

		//	var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");

		//	var packlineInContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1);

		//	TriggerAndFireTransitRequestUsingBookingRequested(consol);

		//	var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
		//	AssertEquals("Precondition: A single RCN should be created.", 1, receiveConsignments.Count());
		//	var receiveConsignment = receiveConsignments.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, receiveConsignment.PackageStates.Count);

		//	var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
		//	AssertEquals("Precondition: A single DCN should be created.", 1, dispatchConsignments.Count());
		//	var dispatchConsignment = dispatchConsignments.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, dispatchConsignment.PackageStates.Count);

		//	var dispatchLoadLists = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
		//	AssertEquals("Precondition: A single DLL should be created.", 1, dispatchLoadLists.Count());
		//	var loadList = dispatchLoadLists.Single();
		//	AssertEquals("Precondition: One Package should be created.", 1, loadList.PackageStates.Count);

		//	var dtus = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
		//	AssertEquals("One DTUs are created since there is one container.", 1, dtus.Count());
		//	var dtu = dtus.Single();

		//	AssertReceiveTransportationUnits("Precondition: No RTUs should be created", Factory);
		//	var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

		//	var dateTimeNow = DateTimeOffset.Now;
		//	var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
		//	rtu.WRH_GateInTime = dateTimeOffset.AddHours(-1);
		//	rtu.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
		//	rtu.WRH_UnloadCompleteTime = dateTimeOffset;

		//	var singleLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var multiLevelHandlingUnit = Helper.CreatePackageHandlingUnit();
		//	var subHandlingUnit = Helper.CreatePackageHandlingUnit();

		//	var standAlonePackageState = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == "BOX");
		//	UnloadAndLabelPackage(standAlonePackageState, rtu, "StandAlone");

		//	var singleLevelHandlingUnitPackageState = CreatePackageStateForTesting("Single", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, singleLevelHandlingUnit, true, "MN_1", weight: 10, length: 1, width: 1, height: 1);
		//	var multiLevelHandlingUnitPackageState = CreatePackageStateForTesting("Multi", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, multiLevelHandlingUnit, true, "MN_2", weight: 20, length: 2, width: 2, height: 2);
		//	var subHandlingUnitPackageState = CreatePackageStateForTesting("Sub", true, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Pallet, null, receiveConsignment, null, dispatchConsignment, loadList, subHandlingUnit, true, "MN_3", weight: 30, length: 3, width: 3, height: 3);
		//	singleLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	multiLevelHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
		//	subHandlingUnitPackageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;

		//	var singleLevelHandlingUnitInner1 = CreatePackageStateForTesting("Single_Inner1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 1, length: 1, width: 1, height: 1);
		//	var singleLevelHandlingUnitInner2 = CreatePackageStateForTesting("Single_Inner2", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Package, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 2, length: 2, width: 2, height: 2);
		//	var multiLevelHandlingUnitInner = CreatePackageStateForTesting("Multi_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Carton, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 3, length: 3, width: 3, height: 3);
		//	var subHandlingUnitInner = CreatePackageStateForTesting("Sub_Inner", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Box, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 4, length: 4, width: 4, height: 4);

		//	// Setting up the handling unit trees before exporting to forwarding
		//	//	Single
		//	//		Single_Inner1
		//	//		Single_Inner2
		//	//	Multi
		//	//		Multi_Inner1
		//	//		Sub
		//	//			Sub_Inner
		//	//	StandAlone
		//	Helper.DisableTopLevelHUFKForTest(TestConnection);
		//	var singleInnerDivot = Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner1, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(singleLevelHandlingUnitPackageState, singleLevelHandlingUnitInner2, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, multiLevelHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackageState, subHandlingUnitInner, ZDateTimeOffset.Now, "BOB");
		//	Helper.PackPackageIntoHandlingUnit(multiLevelHandlingUnitPackageState, subHandlingUnitPackageState, ZDateTimeOffset.Now, "BOB");
		//	Factory.Save();

		//	// Load packages onto DTU
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, multiLevelHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, subHandlingUnitPackageState);
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitInner1);
		//	Helper.LoadPackage(dtu, singleLevelHandlingUnitInner2);
		//	Helper.LoadPackage(dtu, multiLevelHandlingUnitInner);
		//	Helper.LoadPackage(dtu, subHandlingUnitInner);
		//	Helper.LoadPackage(dtu, standAlonePackageState);
		//	Factory.Save();

		//	var packages = Factory.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("Precondition - no packages linked to shipment before export.", 0, packages.Where(p => p.PackageJob.KJ_ParentID == shipment.PK).Count());
		//	AssertEquals("Precondition - One packLine before export.", 1, Factory.Load<ForwardingPackLine>(new ZQuery()).Count());

		//	// Export to forwarding
		//	CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
		//	TriggerAndFireToShipment(dispatchConsignment);

		//	var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentAfterExport = newFactory.Load<ForwardingShipment>(shipment.PK);
		//	var ediMessage = UniversalHelper.GetEDIMessageFromDB(shipmentAfterExport, AutoEvents.DataImportCode);
		//	var packlinesAfterExport = shipmentAfterExport.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	var packagesAfterExport = newFactory.Load<PkgPackage>(new ZQuery());

		//	AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
		//	AssertEquals("1 outer packline for the container should have been created for all packages.", 2, packlinesAfterExport.Count());

		//	var packagesLinkedToShipment = packagesAfterExport.Where(p => p.PackageJob.KJ_ParentID == shipment.PK);
		//	AssertEquals("There should be 8 packages linked to shipment after export.", 8, packagesLinkedToShipment.Count());

		//	var singleLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "Single", string.Empty, "MN_1", 10m, "KG", 1m, 1m, 1m, "CM", 1, "PLT");
		//	var multiLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "Multi", string.Empty, "MN_2", 20m, "KG", 2m, 2m, 2m, "CM", 1, "PLT");
		//	var standAlonePackageAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 8, "StandAlone", packlineInContainer1.JL_PackLineId, string.Empty, 0m, "KG", 0m, 0m, 0m, "CM", 1, "BOX");
		//	AssertEquals("StandAlone package should not have any inners.", 0, standAlonePackageAfterExport.Packages.Count());

		//	var singleLevelHandlingUnitPackageInners = singleLevelHandlingUnitAfterExport.Packages.ToArray();
		//	var singleInner1 = AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner1", string.Empty, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Package);
		//	var singleInner2 = AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 2, "Single_Inner2", string.Empty, string.Empty, 2m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Package);

		//	var multiLevelHandlingUnitPackageInners = multiLevelHandlingUnitAfterExport.Packages.ToArray();
		//	var multiInner = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners, shipment.PK, 2, "Multi_Inner", string.Empty, string.Empty, 3m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Carton);
		//	var subHandlingUnitAfterExport = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners.ToArray(), shipment.PK, 2, "Sub", string.Empty, "MN_3", 30m, "KG", 3m, 3m, 3m, "CM", 1, Constants.PkgUnit.Pallet);

		//	var subHandlingUnitPackageInners = subHandlingUnitAfterExport.Packages.ToArray();
		//	var subInner = AssertAndReturnPackage(subHandlingUnitPackageInners, shipment.PK, 1, "Sub_Inner", string.Empty, string.Empty, 4m, "KG", 4m, 4m, 4m, "CM", 1, Constants.PkgUnit.Box);

		//	// Modify the packages in TW then resend outturn to FOR
		//	var singleHandlingUnitAfterExport = newFactory.Load<WhsItemPackageState>(singleLevelHandlingUnitPackageState.PK);
		//	singleHandlingUnitAfterExport.Package.KP_Weight = 50m;

		//	var singleInner1Divot = newFactory.Load<PkgPackageHandlingUnitDivot>(singleInnerDivot.PK);
		//	// Unpack Single_Inner1 from Single
		//	Helper.UnpackPackageFromHandlingUnit(singleInner1Divot, DateTime.Now, "TOM");

		//	var singleInner2AfterExport = newFactory.Load<WhsItemPackageState>(singleLevelHandlingUnitInner2.PK);
		//	singleInner2AfterExport.Package.KP_WeightUQ = "LT";

		//	var multiHandlingUnitAfterExport = newFactory.Load<WhsItemPackageState>(multiLevelHandlingUnitPackageState.PK);
		//	multiHandlingUnitAfterExport.Package.KP_Length = 50m;

		//	var multiInnerAfterExport = newFactory.Load<WhsItemPackageState>(multiLevelHandlingUnitInner.PK);
		//	multiInnerAfterExport.Package.KP_Width = 50m;

		//	// Pack a BAG into Single
		//	var packageForBAG = CreatePackageStateForTesting("BAG1", false, TransitWarehouseStatuses.Codes.Arrived, Constants.PkgUnit.Bag, rtu, receiveConsignment, null, dispatchConsignment, loadList, weight: 1, length: 1, width: 1, height: 1);
		//	Helper.PackPackageIntoHandlingUnit(multiHandlingUnitAfterExport, packageForBAG, ZDateTimeOffset.Now, "TIM");
		//	Helper.LoadPackage(dtu, packageForBAG);
		//	Factory.Save();

		//	var subAfterExport = newFactory.Load<WhsItemPackageState>(subHandlingUnitPackageState.PK);
		//	subAfterExport.Package.KP_Height = 50m;

		//	var subInnerExport = newFactory.Load<WhsItemPackageState>(subHandlingUnitInner.PK);
		//	subInnerExport.Package.KP_DimensionUQ = "MM";

		//	var standAloneExport = newFactory.Load<WhsItemPackageState>(standAlonePackageState.PK);
		//	standAloneExport.Package.KP_MarksAndNumbers = "MN_Updated";

		//	var dispatchConsignmentAfterExport = newFactory.Load<WhsItemDispatchConsignment>(dispatchConsignment.PK);

		//	newFactory.Save();

		//	// Export to forwarding again to update package fields
		//	TriggerAndFireToShipment(dispatchConsignmentAfterExport);

		//	var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var shipmentFromNewFactory = newFactory2.Load<ForwardingShipment>(shipment.PK);
		//	var packLinesAfterExport2 = shipmentFromNewFactory.OuterPackLines.Cast<ForwardingPackLine>().ToArray();
		//	var packagesAfterExport2 = newFactory2.Load<PkgPackage>(new ZQuery());
		//	AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessage?.EM_Status);
		//	AssertEquals("1 outer packline created for the unpacked package.", 3, packLinesAfterExport2.Count());

		//	packagesLinkedToShipment = packagesAfterExport2.Where(p => p.PackageJob.KJ_ParentID == shipment.PK);
		//	AssertEquals("There should be 9 packages linked to shipment after export.", 9, packagesLinkedToShipment.Count());

		//	singleLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 9, "Single", string.Empty, "MN_1", 49m, "KG", 1m, 1m, 1m, "CM", 1, "PLT");
		//	multiLevelHandlingUnitAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 9, "Multi", string.Empty, "MN_2", 20m, "KG", 50m, 2m, 2m, "CM", 1, "PLT");
		//	standAlonePackageAfterExport = AssertAndReturnPackage(packagesLinkedToShipment, shipment.PK, 9, "StandAlone", packlineInContainer1.JL_PackLineId, "MN_Updated", 0m, "KG", 0m, 0m, 0m, "CM", 1, "BOX");
		//	AssertEquals("StandAlone package should not have any inners.", 0, standAlonePackageAfterExport.Packages.Count());

		//	singleLevelHandlingUnitPackageInners = singleLevelHandlingUnitAfterExport.Packages.ToArray();
		//	singleInner2 = AssertAndReturnPackage(singleLevelHandlingUnitPackageInners, shipment.PK, 1, "Single_Inner2", string.Empty, string.Empty, 2m, "LT", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Package);

		//	multiLevelHandlingUnitPackageInners = multiLevelHandlingUnitAfterExport.Packages.ToArray();
		//	multiInner = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners, shipment.PK, 3, "Multi_Inner", string.Empty, string.Empty, 3m, "KG", 3m, 50m, 3m, "CM", 1, Constants.PkgUnit.Carton);
		//	var bag = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners, shipment.PK, 3, "BAG1", string.Empty, string.Empty, 1m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Bag);
		//	subHandlingUnitAfterExport = AssertAndReturnPackage(multiLevelHandlingUnitPackageInners.ToArray(), shipment.PK, 3, "Sub", string.Empty, "MN_3", 30m, "KG", 3m, 3m, 50m, "CM", 1, Constants.PkgUnit.Pallet);

		//	subHandlingUnitPackageInners = subHandlingUnitAfterExport.Packages.ToArray();
		//	subInner = AssertAndReturnPackage(subHandlingUnitPackageInners, shipment.PK, 1, "Sub_Inner", string.Empty, string.Empty, 4m, "KG", 4m, 4m, 4m, "MM", 1, Constants.PkgUnit.Box);
		//}

		//#endregion

		#region TestTWToForwarding_SendingPackages_IsHighRiskAndAdditionalScreeningIsPopulated

		public void TestTWToForwarding_SendingPackages_IsHighRiskAndAdditionalScreeningIsPopulated()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			var dateTimeNow = ZDateTimeOffset.Now;
			var dateTimeOffset = new ZDateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			rtu1.WRH_GateInTime = dateTimeOffset.AddHours(-1);
			rtu1.WRH_UnloadCompleteTime = dateTimeOffset.AddHours(1);
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;

			var cas1UnloadedPackage = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3", isHighRisk: true);
			var cas1Screening1 = Helper.CreatePackageScreening(cas1UnloadedPackage.Package, "XRY", true);
			var cas1Screening2 = Helper.CreatePackageScreening(cas1UnloadedPackage.Package, "AOW", true);

			cas1Screening1.KPS_Time = DateTime.Now;
			cas1Screening2.KPS_Time = DateTime.Now.AddMinutes(10);

			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals(1, packLinesInShipment.Count);

			var packLinesForCAS = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);

			AssertEquals("new packline should be created with pkgpackages attached", 1, packLinesForCAS.PkgPackageCollection.Count);
			AssertDimensions(packLinesForCAS.PkgPackageCollection.Single(p => p.KP_PackageID == "CAS1"), weight: 4, weightUQ: "KG", volume: 5, volumeUQ: "M3");

			AssertEquals("IsHighRisk must be set to true.", true, packLinesForCAS.JL_IsHighRisk);

			AssertEquals("InspectionTypeCode must be set to XRY.", "XRY", packLinesForCAS.JL_InspectionTypeCode);
			AssertEquals("InspectionTypeCode must be set to AOW.", "AOW", packLinesForCAS.JL_AdditionalInspectionTypeCode);
		}

		#endregion

		#region TestForwardingConsignmentOrderReferences

		#region DispatchConsignment

		public void TestImportDispatchConsignmentOrderReferences_SendAdditonalOrdeRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, dispatchConsignment.OrderReferences.Count);
			AssertEquals("ORDER1", dispatchConsignment.OrderReferences[0].WOR_OrderReference);

			var consignmentOrderRef2 = Helper.CreateWhsItemConsignmentOrderReference("ORDER2", dispatchConsignment);

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
		}

		public void TestImportDispatchConsignmentOrderReferences_SendEmptyOrderRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, dispatchConsignment.OrderReferences.Count);
			AssertEquals("ORDER1", dispatchConsignment.OrderReferences[0].WOR_OrderReference);

			dispatchConsignment.OrderReferences.DeleteAll();

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(1, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
		}

		public void TestImportDispatchConsignmentOrderReferences_SendDuplicateOrderRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "ORDER2";
			item2.JT_Sequence = 2;
			var item3 = shipment.DocsAndCartage.OrderItems.AddNew();
			item3.JT_OrderReference = "ORDER3";
			item3.JT_Sequence = 3;
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(3, dispatchConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "ORDER1", "ORDER2", "ORDER3" }, dispatchConsignment.OrderReferences.Select(or => or.WOR_OrderReference));

			dispatchConsignment.OrderReferences.Single(or => or.WOR_OrderReference == "ORDER2").Delete();
			dispatchConsignment.OrderReferences.Single(or => or.WOR_OrderReference == "ORDER3").Delete();

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
			AssertEquals("ORDER3", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals((short)3, jobDocsAndCartage.OrderItems[2].JT_Sequence);
		}

		public void TestImportDispatchConsignmentOrderReferences_ResendSameOrderRefs()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "ORDER2";
			item2.JT_Sequence = 2;
			var item3 = shipment.DocsAndCartage.OrderItems.AddNew();
			item3.JT_OrderReference = "ORDER3";
			item3.JT_Sequence = 3;
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(3, dispatchConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "ORDER1", "ORDER2", "ORDER3" }, dispatchConsignment.OrderReferences.Select(or => or.WOR_OrderReference));

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
			AssertEquals("ORDER3", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals((short)3, jobDocsAndCartage.OrderItems[2].JT_Sequence);
		}

		public void TestImportDispatchConsignmentOrderReferences_NonExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(0, dispatchConsignment.OrderReferences.Count);

			var consignmentOrderRef1 = Helper.CreateWhsItemConsignmentOrderReference("ORDER1", dispatchConsignment);
			var consignmentOrderRef2 = Helper.CreateWhsItemConsignmentOrderReference("ORDER2", dispatchConsignment);

			Factory.Save();
			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();

			TriggerAndFireOutturn(dispatchConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
		}

		#endregion

		#region ReceiveConsignment

		public void TestImportReceiveConsignmentOrderReferences_SendAdditonalOrdeRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.OrderReferences.Count);
			AssertEquals("ORDER1", receiveConsignment.OrderReferences[0].WOR_OrderReference);

			var consignmentOrderRef2 = Helper.CreateWhsItemConsignmentOrderReference("ORDER2", receiveConsignment);

			Factory.Save();
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
		}

		public void TestImportReceiveConsignmentOrderReferences_SendEmptyOrderRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.OrderReferences.Count);
			AssertEquals("ORDER1", receiveConsignment.OrderReferences[0].WOR_OrderReference);

			receiveConsignment.OrderReferences.DeleteAll();

			Factory.Save();
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(1, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
		}

		public void TestImportReceiveConsignmentOrderReferences_SendDuplicateOrderRefsExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "ORDER2";
			item2.JT_Sequence = 2;
			var item3 = shipment.DocsAndCartage.OrderItems.AddNew();
			item3.JT_OrderReference = "ORDER3";
			item3.JT_Sequence = 3;
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(3, receiveConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "ORDER1", "ORDER2", "ORDER3" }, receiveConsignment.OrderReferences.Select(or => or.WOR_OrderReference));

			receiveConsignment.OrderReferences.Single(or => or.WOR_OrderReference == "ORDER2").Delete();
			receiveConsignment.OrderReferences.Single(or => or.WOR_OrderReference == "ORDER3").Delete();

			Factory.Save();
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
			AssertEquals("ORDER3", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals((short)3, jobDocsAndCartage.OrderItems[2].JT_Sequence);
		}

		public void TestImportReceiveConsignmentOrderReferences_ResendSameOrderRefs()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var item1 = shipment.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "ORDER1";
			item1.JT_Sequence = 1;
			var item2 = shipment.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "ORDER2";
			item2.JT_Sequence = 2;
			var item3 = shipment.DocsAndCartage.OrderItems.AddNew();
			item3.JT_OrderReference = "ORDER3";
			item3.JT_Sequence = 3;
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(3, receiveConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "ORDER1", "ORDER2", "ORDER3" }, receiveConsignment.OrderReferences.Select(or => or.WOR_OrderReference));

			Factory.Save();
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(3, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
			AssertEquals("ORDER3", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
			AssertEquals((short)3, jobDocsAndCartage.OrderItems[2].JT_Sequence);
		}

		public void TestImportReceiveConsignmentOrderReferences_NonExistingOrderItems()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(0, receiveConsignment.OrderReferences.Count);

			var consignmentOrderRef1 = Helper.CreateWhsItemConsignmentOrderReference("ORDER1", receiveConsignment);
			var consignmentOrderRef2 = Helper.CreateWhsItemConsignmentOrderReference("ORDER2", receiveConsignment);

			Factory.Save();
			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentInAnotherFactory);
			AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
			AssertEquals("ORDER1", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
			AssertEquals((short)1, jobDocsAndCartage.OrderItems[0].JT_Sequence);
			AssertEquals("ORDER2", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
			AssertEquals((short)2, jobDocsAndCartage.OrderItems[1].JT_Sequence);
		}

		#endregion

		#endregion

		#region Implementation

		public ForwardingPackLine AssertAndReturnForwardingPackLine(ForwardingPackLine[] packLines, string refNumber, string marksAndNumbers, decimal weight, string weightUQ, decimal length, decimal width, decimal height, string dimUQ, int packageCount, string packType)
		{
			var packLine = packLines.Single(p => p.JL_RefNumber == refNumber);
			AssertEquals(marksAndNumbers, packLine.JL_MarksAndNumbers);
			AssertEquals(weight, packLine.JL_ActualWeight);
			AssertEquals(weightUQ, packLine.JL_ActualWeightUQ);
			AssertEquals(length, packLine.JL_Length);
			AssertEquals(width, packLine.JL_Width);
			AssertEquals(height, packLine.JL_Height);
			AssertEquals(dimUQ, packLine.JL_UnitOfDimension);
			AssertEquals(packageCount, packLine.JL_PackageCount);
			AssertEquals(packType, packLine.JL_F3_NKPackType);

			return packLine;
		}

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory);

		#endregion

	}
}
