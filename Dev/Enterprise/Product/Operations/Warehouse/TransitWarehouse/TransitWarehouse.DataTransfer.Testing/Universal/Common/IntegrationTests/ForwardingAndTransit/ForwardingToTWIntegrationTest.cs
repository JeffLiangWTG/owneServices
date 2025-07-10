using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;
using Constants = Enterprise.Core.Constants;
using Events = Enterprise.ZArchitecture.Business.Events;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ForwardingToTWIntegrationTest : IntegrationTestCaseWithFactory
	{
		protected override void TearDown()
		{
			base.TearDown();

			Freight.Business.Testing.FreightTestHelper.TryRemovePackLineIdSequence();
		}

		#region TestUpdateExistingDispatchLoadListWithNoContainer

		public void TestUpdateExistingEmptyDispatchLoadList_UnassignedPackages() => TestUpdateExistingEmptyDispatchLoadList(false);

		public void TestUpdateExistingEmptyDispatchLoadList_AssignedPackages() => TestUpdateExistingEmptyDispatchLoadList(true);

		void TestUpdateExistingEmptyDispatchLoadList(bool assignPackage)
		{
			var testData = CreateTestData(true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			var dll = loadLists.Single();

			AssertEquals("Import should create 1 dll.", 1, loadLists.Length);
			AssertEquals("Import should not create any dtu.", 0, dll.DispatchTransportationUnits.Count);

			var container1 = CreateContainer(consol, "CONT1");
			if (assignPackage)
			{
				packline.JL_JC = container1.PK;
			}
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalkerResend = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListsResend = newBizOFactoryAfterRunningLogWalkerResend.Load<WhsItemDispatchLoadList>(new ZQuery());
			var dllAfterResend = loadListsResend.Single();

			AssertEquals("Import should update existing dll.", 1, loadListsResend.Length);
			AssertEquals("Should not create new dll.", dll.PK, dllAfterResend.PK);
			AssertEquals("Import should create 1 dtu.", 1, dllAfterResend.DispatchTransportationUnits.Count);
		}

		#endregion

		#region TestCreateDCN_AllowPartialLoadingBasedOnMixedDLLs

		public void TestCreateDCN_AllowPartialLoadingBasedOnMixedDLLs()
		{
			var testData = CreateTestData(true);
			testData.warehouse.WW_AllowPartialLoadingDefault = true;
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "2", containerCount: 1, containerTypeCode: "40GP");

			var container3 = CreateContainer(consol, containerNum: "3", containerCount: 1, containerTypeCode: "20FR");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");

			var packline1InContainer3 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, container3, "PKG4");
			var packline2InContainer3 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Drum, container3, "PKG5");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipment2.PK);
			AssertConsignmentPackages(rcn1, 3);
			AssertConsignmentPackages(rcn2, 2);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(3, loadLists.Length);

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 2, dispatchConsignments.Length);

			var dcn1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
			var dcn2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB2");

			AssertEquals(true, dcn1.WDC_AllowPartialLoading);
			AssertEquals(true, dcn2.WDC_AllowPartialLoading);
		}

		#endregion

		#region Consols

		#region TestImportConsol_PackagesInDifferentWarehouse_CreatingReceiveConsignment

		public void TestImportConsol_PackagesInDifferentWarehouse_CreatingReceiveConsignment()
		{
			var testData = CreateTestData(true);
			var differentWarehouse = Helper.CreateTRWWarehouse("TW2");
			var stageLocationIndifferentWarehouse = differentWarehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", differentWarehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", differentWarehouse.PK, stageLocationIndifferentWarehouse.PK);

			var packageState1 = Helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateAdditionalReference(packageState1, "S00001000", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, entryNum: "S00001000");
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-2");
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "S00001000", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 2);

			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-1"));
			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-2"));
		}

		public void TestImportConsol_PackagesInDifferentWarehouse_CreatingReceiveConsignment_UsingExtraPortToCreateTransportRouting()
		{
			var testData = CreateTestData(true);
			var differentWarehouse = Helper.CreateTRWWarehouse("TW2");
			var stageLocationIndifferentWarehouse = differentWarehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", differentWarehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", differentWarehouse.PK, stageLocationIndifferentWarehouse.PK);

			var extraPort = Factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = testData.warehouse.WW_GB_RelatedCompanyBranch;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "NLEUG";

			var packageState1 = Helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateAdditionalReference(packageState1, "S00001000", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, entryNum: "S00001000");
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "S00001000", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var outboundLeg = CreateTransport(shipment1, 1, "SEA", "A", "AA", "NLEUG", "AUSYD", testData.today, testData.today.AddDays(2));
			var outboundLegForASN = CreateTransport(consol, 1, "SEA", "A", "AA", "NLEUG", "AUSYD", testData.today, testData.today.AddDays(2));

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-2");
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "S00001000", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 2);

			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-1"));
			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-2"));

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var transportRouting = newBizOFactoryAfterRunningLogWalker.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, rcn1.PK)).Single();
			AssertEquals("Home Port", "NLEUG", transportRouting.JW_RL_NKLoadPort);
			AssertEquals("Disc Port", "AUSYD", transportRouting.JW_RL_NKDiscPort);

			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var transportRoutingForASN = newBizOFactoryAfterRunningLogWalker.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, asn.PK)).Single();
			AssertEquals("Home Port", "NLEUG", transportRouting.JW_RL_NKLoadPort);
			AssertEquals("Disc Port", "AUSYD", transportRouting.JW_RL_NKDiscPort);
		}

		public void TestImportConsol_PackagesInDifferentWarehouse_CreatingReceiveConsignment_UsingExtraPortToCreateTransportRouting_MultipleExtraPortAndOutBoundLeg()
		{
			var testData = CreateTestData(true);
			var differentWarehouse = Helper.CreateTRWWarehouse("TW2");
			var stageLocationIndifferentWarehouse = differentWarehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", differentWarehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", differentWarehouse.PK, stageLocationIndifferentWarehouse.PK);

			var extraPort = Factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = testData.warehouse.WW_GB_RelatedCompanyBranch;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "NLEUG";

			var extraPort2 = Factory.New<GlbBranchExtraPorts>();
			extraPort2.GY_GB = testData.warehouse.WW_GB_RelatedCompanyBranch;
			extraPort2.GY_IsValid = true;
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUS2M";

			var packageState1 = Helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateAdditionalReference(packageState1, "S00001000", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, entryNum: "S00001000");
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "S00001000", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var outboundLeg = CreateTransport(shipment1, 1, "SEA", "A", "AA", "NLEUG", "AUSYD", testData.today, testData.today.AddDays(2));
			var outboundLeg2 = CreateTransport(shipment1, 1, "SEA", "B", "BB", "AUS2M", "AUSYD", testData.today, testData.today.AddDays(2));

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, reference: "PKG-2");
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "S00001000", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 2);

			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-1"));
			AssertEquals(true, rcn1.PackageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-2"));

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var transportRouting = newBizOFactoryAfterRunningLogWalker.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, rcn1.PK)).Single();
			AssertEquals("Home Port", "AUS2M", transportRouting.JW_RL_NKLoadPort);
			AssertEquals("Disc Port", "AUSYD", transportRouting.JW_RL_NKDiscPort);
		}

		#endregion

		#region TestImportConsol_WhenConsolHasDepartureCFSTransportAddress_ASNsHaveTransportCompany

		public void TestImportConsol_ASNsHaveTransportCompany()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   2 shipments
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var departureTransportCompany = TestHelper.CreateOrganisation("DTC");
			var arrivalTransportCompany = TestHelper.CreateOrganisation("ATC");

			consol.JK_OA_DeparturePackCFSTransportAddress = departureTransportCompany.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportCompany.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "S002", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "S001", testData.warehouse, shipment1.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "S002", testData.warehouse, shipment2.PK);

			var asns = rcn1.PackageStates.Concat(rcn2.PackageStates).Select(p => p.ReceiveASN).Distinct();
			AssertEquals("Two receive ASNs must be created.", 2, asns.Count());

			var receiveASNForCont1 = asns.Single(r => r.WRP_VehicleReference == "CONT1");
			var receiveASNForLoose = asns.Single(r => r.WRP_VehicleReference == "MSB1");

			AssertReceiveASNTransportCompany(Factory, receiveASNForCont1, arrivalTransportCompany.MainAddress);
			AssertReceiveASNTransportCompany(Factory, receiveASNForLoose, arrivalTransportCompany.MainAddress);
		}

		#endregion

		#region TestImportConsol_WhenConsolHasNoDepartureCFSTransportAddress_ASNsHaveNoTransportCompany

		public void TestImportConsol_WhenConsolHasNoDepartureCFSTransportAddress_ASNsHaveNoTransportCompany()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   2 shipments
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "S002", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.DocsAndCartage.JP_OA_PickupCartageCoAddr = testData.consignor.MainAddress.PK;

			var container1 = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "S001", testData.warehouse, shipment1.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "S002", testData.warehouse, shipment2.PK);

			var asns = rcn1.PackageStates.Concat(rcn2.PackageStates).Select(p => p.ReceiveASN).Distinct();
			AssertEquals("Two receive ASNs must be created.", 2, asns.Count());

			var receiveASNForCont1 = asns.Single(r => r.WRP_VehicleReference == "CONT1");
			var receiveASNForLoose = asns.Single(r => r.WRP_VehicleReference == "MSB1");

			AssertReceiveASNTransportCompany(Factory, receiveASNForCont1, null);
			AssertReceiveASNTransportCompany(Factory, receiveASNForLoose, null);
		}

		#endregion

		#region TestImportConsol_WhenConsolHasNoShipments

		public void TestImportConsol_WhenConsolHasNoShipments()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   0 shipments
			//   route : NZAKL -> AUSYD
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Should not create RCN", 0, rcns.Length);

			// Dispatch consignments
			TriggerAndFireTransitRequestForRelease(consol);

			newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should not create DCN", 0, dcns.Length);
		}

		#endregion

		#region TestImportConsol_WhenMultipleShipmentsHaveErrors

		public void TestImportConsol_WhenMultipleShipmentsHaveErrors()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   2 shipments
			//   route : NZAKL -> AUSYD
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var validShipment = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var emptyAssemblyMaster = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, Array.Empty<ForwardingShipment>(), shipmentType: Constants.ShipmentTypes.AssemblyMaster);
			var shipmentWithDuplicatePackages = CreateShipment(consol, "S002", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			CreateOuterPackline(validShipment, 1, Constants.PkgUnit.Package);
			CreateOuterPackline(shipmentWithDuplicatePackages, 1, Constants.PkgUnit.Pallet, reference: "DUPLICATE_ID");
			CreateOuterPackline(shipmentWithDuplicatePackages, 1, Constants.PkgUnit.Pallet, reference: "DUPLICATE_ID");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Should not create RCN", 0, rcns.Length);

			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have an unsuccessful export.", EDIMessageStatusList.Codes.Discarded, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have the combined error messages at the expected positions.",
@"Error - Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment 'S00001001'.", importNote?.ST_NoteText);
			AssertContains("Log message must have the combined error messages at the expected positions.",
@"Error importing ForwardingShipment - S00001002:
Reference number DUPLICATE_ID is used for more than one package on a shipment S002. Please provide a unique reference number or leave it empty.", importNote?.ST_NoteText);
			AssertContains("Log message should contain a summary error log.",
@"Error - The following Sub Shipments encountered errors when importing ForwardingConsol - C00001000:
Data Source
ForwardingShipment - S00001001
ForwardingShipment - S00001002
Error importing ForwardingShipment - S00001001:
Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment 'S00001001'.

Error importing ForwardingShipment - S00001002:
Reference number DUPLICATE_ID is used for more than one package on a shipment S002. Please provide a unique reference number or leave it empty.

No changes were made due to the above errors. Please fix the errors and try again.", importNote?.ST_NoteText);
		}

		#endregion

		#region TestImportFowardingConsol_SingleRoute_CreatingReceiveAndDispatchConsignments

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_SingleRoute_CreatingReceiveAndDispatchConsignments()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   2 shipments
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
			var packline12 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box, container2);
			var packline13 = CreateOuterPackline(shipment1, 3, Constants.PkgUnit.Package);
			var packline21 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Case, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "MSB1", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN1 Destination", "AUSYD", rcn1.WRC_RL_NKDestination);

			AssertConsignmentPackages(rcn1, 3);

			var rcn2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn2, "HSB2", "S00001001", "MSB1", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN2 Destination", "AUSYD", rcn2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2, 1);

			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, rcn1.WRC_TransportMode);
			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, rcn2.WRC_TransportMode);

			var asns = rcn1.PackageStates.Concat(rcn2.PackageStates).Select(p => p.ReceiveASN).Distinct();
			AssertEquals("Three receive ASNs must be created.", 3, asns.Count());
			var receiveASNForCont1 = asns.Single(r => r.WRP_VehicleReference == "CONT1");
			var receiveASNForCont2 = asns.Single(r => r.WRP_VehicleReference == "CONT2");
			var receiveASNForLoose = asns.Single(r => r.WRP_VehicleReference == "MSB1");
			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, receiveASNForCont1.WRP_TransportMode);
			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, receiveASNForCont2.WRP_TransportMode);
			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, receiveASNForLoose.WRP_TransportMode);

			AssertReceiveTransportationUnits("An RTU should be created for each container", newFactory, container1, container2);
			var pivots = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("RTUs should be linked to their ASNs", new[] { receiveASNForCont1.PK, receiveASNForCont2.PK }, pivots.Select(p => p.WAR_WRP_TransitReceiveASN));

			var packageStateForCN1 = rcn1.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont1.PK);
			AssertPackage(packageStateForCN1, Constants.PkgUnit.Pallet);

			var packageStateForCN2 = rcn1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont2.PK).ToArray()[0];
			AssertPackage(packageStateForCN2, Constants.PkgUnit.Box);

			var packageStateForEmptyContainer = rcn1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForLoose.PK).ToArray()[0];
			AssertPackage(packageStateForEmptyContainer, Constants.PkgUnit.Package);

			var packageStateForCN1_RCN2 = rcn2.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont1.PK);
			AssertPackage(packageStateForCN1_RCN2, Constants.PkgUnit.Case);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertEquals("Should have populated Transport Mode.", Constants.TransportModes.Sea, loadList.WDL_TransportMode);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

			var dispatchConsignment1 = dispatchConsignments.First(c => c.PackageStates.Count == 3);
			AssertDispatchConsignment(packageStateForCN1, dispatchConsignment1, shipment1.PK);
			AssertDispatchConsignment(packageStateForCN2, dispatchConsignment1, shipment1.PK);
			AssertDispatchConsignment(packageStateForEmptyContainer, dispatchConsignment1, shipment1.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment1, "HSB1", "S00001000", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN1 Destination", "AUSYD", dispatchConsignment1.WDC_RL_NKDestination);

			var dispatchConsignment2 = dispatchConsignments.First(c => c.PackageStates.Count == 1);
			AssertDispatchConsignment(packageStateForCN1_RCN2, dispatchConsignment2, shipment2.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment2, "HSB2", "S00001001", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN2 Destination", "AUSYD", dispatchConsignment2.WDC_RL_NKDestination);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_SingleRoute_CreatingReceiveAndDispatchConsignments_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestImportFowardingConsol_SingleRoute_CreatingReceiveAndDispatchConsignments();
			}
		}

		#endregion

		#region TestImportFowardingConsol_MultiRoute_CreatingReceiveAndDispatchConsignments

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_MultiRoute_CreatingReceiveAndDispatchConsignments()
		{
			var testData = CreateTestData(true, "NZCHC");

			// Consol w/
			//   2 shipments
			//   2 containers
			//   route : NZCHC -> NZAKL-> AUSYD -> AUADL
			//   depot : ^
			//   Note : Phase 1, NZAKL is where the packing occurs, NZAKL & AUSYD are pass though hubs that are controlled by the airline. ie. Forwarding wont be sending UXML to NZAKL/AUSYD Transit Warehouses.
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
			var packline12 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box, container2);
			var packline13 = CreateOuterPackline(shipment1, 3, Constants.PkgUnit.Package);
			var packline21 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Case, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "MSB1", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);

			AssertConsignmentPackages(rcn1, 3);

			var rcn2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn2, "HSB2", "S00001001", "MSB1", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN2 Destination", "AUADL", rcn2.WRC_RL_NKDestination);

			AssertConsignmentPackages(rcn2, 1);

			var asns = rcn1.PackageStates.Concat(rcn2.PackageStates).Select(p => p.ReceiveASN).Distinct();
			AssertEquals("Three receive ASNs must be created.", 3, asns.Count());
			var receiveASNForCont1 = asns.Single(r => r.WRP_VehicleReference == "CONT1");
			var receiveASNForCont2 = asns.Single(r => r.WRP_VehicleReference == "CONT2");
			var receiveASNForLoose = asns.Single(r => r.WRP_VehicleReference == "MSB1");
			var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertReceiveTransportationUnits("An RTU should be created for each container", newFactory, container1, container2);
			var pivots = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("RTUs should be linked to the container ASNs", new[] { receiveASNForCont1.PK, receiveASNForCont2.PK }, pivots.Select(p => p.WAR_WRP_TransitReceiveASN));

			var packageStateForCN1 = rcn1.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont1.PK);
			AssertReceiveASN(receiveASNForCont1, container1.PK, "JC", "CONT1", outboundLeg);
			AssertReceiveASN(receiveASNForCont2, container2.PK, "JC", "CONT2", outboundLeg);
			AssertReceiveASN(receiveASNForLoose, consol.PK, "JK", "MSB1", outboundLeg);
			AssertPackage(packageStateForCN1, Constants.PkgUnit.Pallet);

			var packageStateForCN2 = rcn1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont2.PK).ToArray()[0];
			AssertPackage(packageStateForCN2, Constants.PkgUnit.Box);

			var packageStateForEmptyContainer = rcn1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForLoose.PK).ToArray()[0];
			AssertPackage(packageStateForEmptyContainer, Constants.PkgUnit.Package);

			var packageStateForCN1_RCN2 = rcn2.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForCont1.PK);
			AssertPackage(packageStateForCN1_RCN2, Constants.PkgUnit.Case);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Precondition", "C00001000", consol.JK_UniqueConsignRef);
			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", "C00001000", outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

			var dispatchConsignment1 = dispatchConsignments.First(c => c.PackageStates.Count == 3);
			AssertDispatchConsignment(packageStateForCN1, dispatchConsignment1, shipment1.PK);
			AssertDispatchConsignment(packageStateForCN2, dispatchConsignment1, shipment1.PK);
			AssertDispatchConsignment(packageStateForEmptyContainer, dispatchConsignment1, shipment1.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment1, "HSB1", "S00001000", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN1 Destination", "AUADL", dispatchConsignment1.WDC_RL_NKDestination);

			var dispatchConsignment2 = dispatchConsignments.First(c => c.PackageStates.Count == 1);
			AssertDispatchConsignment(packageStateForCN1_RCN2, dispatchConsignment2, shipment2.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment2, "HSB2", "S00001001", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN2 Destination", "AUADL", dispatchConsignment2.WDC_RL_NKDestination);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_MultiRoute_CreatingReceiveAndDispatchConsignments_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestImportFowardingConsol_MultiRoute_CreatingReceiveAndDispatchConsignments();
			}
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ForwarderAddsContainerAndReimports

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderAddsContainerAndReimports()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   3 shipments
			//   0 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment3 = CreateShipment(consol, "HSB3", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, reference: "P1");
			var packline2 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Box, reference: "P2");
			var packline3 = CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package, reference: "P3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and ASN
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcn2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			var rcn3 = AssertConsignment(newFactory, "HSB3", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);

			var asns = rcn1.PackageStates.Concat(rcn2.PackageStates).Concat(rcn3.PackageStates).Select(p => p.ReceiveASN).Distinct();
			AssertEquals("One receive ASN must be created.", 1, asns.Count());
			var receiveASN = asns.Single();

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, Array.Empty<string>());
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), loadList.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have created 3 Dispatch Consignments", new string[] { "HSB1", "HSB2", "HSB3" }, dispatchConsignments.Select(c => c.WDC_ConsignmentID));

			// Add a container, remove one shipment, and reimport
			var container1 = CreateContainer(consol, "CONT1");
			packline1.JL_JC = container1.PK;
			packline2.JL_JC = container1.PK;

			consol.Shipments.Remove(shipment3);

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The load list should be reused.", 1, loadListsAfterReimport.Length);
			var containerLoadlistAfterReimport = loadListsAfterReimport.Single();
			AssertEquals("The load list should be reused.", loadList.PK, containerLoadlistAfterReimport.PK);
			AssertEquals("The load list should be activated", true, containerLoadlistAfterReimport.WDL_IsActive);
			AssertEquals("The load list should not be completed.", false, containerLoadlistAfterReimport.WDL_CompleteTime.IsValid);
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1" }, containerLoadlistAfterReimport.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker2, new string[] { "CONT1" });
			AssertContainsExactElementsInAnyOrder("Should have attached the included dispatch consignments", new string[] { "HSB1", "HSB2" }, containerLoadlistAfterReimport.PackageStates.Select(p => p.DispatchConsignment.WDC_ConsignmentID));

			var looseLoadlistAfterReimport = loadListsAfterReimport.SingleOrDefault(l => l.PK == loadList.PK);
			AssertNotNull("The load list should not be deleted.", looseLoadlistAfterReimport);
			AssertEquals("The load list should be updated with its packages.", 2, looseLoadlistAfterReimport.PackageStates.Count);
			AssertEquals("The load list should be activate", true, looseLoadlistAfterReimport.WDL_IsActive);
			AssertEquals("The load list should not be completed.", false, looseLoadlistAfterReimport.WDL_CompleteTime.IsValid);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ErrorInShipments_ErrorsFromEveryShipmentShouldBeLogged

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ErrorInShipments_ErrorsFromEveryShipmentShouldBeLogged()
		{
			var testData = CreateTestData(true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment3 = CreateShipment(consol, "HSB3", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, reference: "P1");
			var packline2 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Box, reference: "P2");
			var packline3 = CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package, reference: "P3");

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("No Dispatch Consignments should have been created due to error.", 0, dispatchConsignments.Length);

			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have an unsuccessful export.", EDIMessageStatusList.Codes.Discarded, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("There should be a log message.",
@"Error - The following Sub Shipments encountered errors when importing ForwardingConsol - C00001000:
Data Source
ForwardingShipment - S00001000
ForwardingShipment - S00001001
ForwardingShipment - S00001002
", importNote?.ST_NoteText);
			AssertContains("There should be a log message.",
@"Error importing ForwardingShipment - S00001000:
Failed to match valid Packages in Transit Warehouse 'TRW'.

Could not find the following Package IDs: P1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.
", importNote?.ST_NoteText);

			AssertContains("There should be a log message.",
@"Error importing ForwardingShipment - S00001002:
Failed to match valid Packages in Transit Warehouse 'TRW'.

Could not find the following Package IDs: P3
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.
", importNote?.ST_NoteText);

			AssertContains("There should be a log message.",
@"Error importing ForwardingShipment - S00001001:
Failed to match valid Packages in Transit Warehouse 'TRW'.

Could not find the following Package IDs: P2
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.
", importNote?.ST_NoteText);

			AssertContains("There should be a log message.",
"No changes were made due to the above errors. Please fix the errors and try again.", importNote?.ST_NoteText);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UpdateAwaitingForwardingChanges

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_UpdateAwaitingForwardingChanges()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "P1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());

			loadList.WDL_IsAwaitingForwardingChanges = true;

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouse

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouse_MasterBillNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouseCore("MSB1");
		}

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouse_ConsolNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouseCore("");
		}

		[TestDate(2018, 1, 1)]
		void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouseCore(string masterBillNumber)
		{
			var testData = CreateTestData(false);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol(masterBillNumber, testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = testData.warehouse.DefaultOutboundDockDoorLocation.PK;
			loadList.WDL_CompleteTime = new DateTime(2018, 1, 2);

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceSuspended, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be true after import", true, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage should be false after import", false, loadlisAfterReimport.WDL_IsReadyToStage);
			AssertEquals("Complete Time shall be empty", ZDateTimeOffset.Empty, loadlisAfterReimport.WDL_CompleteTime);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.Single(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNotNull("Should have Status Updated log", log);
			AssertEquals("Log should be same as expected", "|TYP=STOP LOAD|WHS=TRW|JOB=DLL00000001|RFN=C00001000", log.SL_Reference);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouse

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouse_MasterBillNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouseCore("MSB1");
		}

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouse_ConsolNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouseCore("");
		}

		[TestDate(2018, 1, 1)]
		void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouseCore(string masterBillNumber)
		{
			var testData = CreateTestData(false);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol(masterBillNumber, testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = true;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceRequested, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.Single(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNotNull("Should have Status Updated log", log);
			AssertEquals("Log should be same as expected", "|TYP=CANCEL STOP LOAD|WHS=TRW|JOB=DLL00000001|RFN=C00001000", log.SL_Reference);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouse_FlagNotChange

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_DepartureWarehouse_FlagNotChange()
		{
			var testData = CreateTestData(false);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = true;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceSuspended, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be true", true, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.SingleOrDefault(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNull("Should not have Status Updated log", log);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouse_FlagNotChange

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_DepartureWarehouse_FlagNotChange()
		{
			var testData = CreateTestData(false);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = false;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceRequested, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.SingleOrDefault(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNull("Should not have Status Updated log", log);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArriveWarehouse

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArrivalWarehouse_MasterBillNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArrivalWarehouseCore("MSB1");
		}

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArrivalWarehouse_ConsolNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArrivalWarehouseCore("");
		}

		[TestDate(2018, 1, 1)]
		void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArrivalWarehouseCore(string masterBillNumber)
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol(masterBillNumber, testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = testData.warehouse.DefaultOutboundDockDoorLocation.PK;
			loadList.WDL_CompleteTime = new DateTime(2018, 1, 2);

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceSuspended, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be true after import", true, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage should be false after import", false, loadlisAfterReimport.WDL_IsReadyToStage);
			AssertEquals("Complete Time shall be empty", ZDateTimeOffset.Empty, loadlisAfterReimport.WDL_CompleteTime);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.Single(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNotNull("Should have Status Updated log", log);
			AssertEquals("Log should be same as expected", "|TYP=STOP LOAD|WHS=TRW|JOB=DLL00000001|RFN=C00001000", log.SL_Reference);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouse

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouse_MasterBillNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouseCore("MSB1");
		}

		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouse_ConsolNumber()
		{
			TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouseCore("");
		}

		[TestDate(2018, 1, 1)]
		void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArrivalWarehouseCore(string masterBillNumber)
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol(masterBillNumber, testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = true;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceRequested, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.Single(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNotNull("Should have Status Updated log", log);
			AssertEquals("Log should be same as expected", "|TYP=CANCEL STOP LOAD|WHS=TRW|JOB=DLL00000001|RFN=C00001000", log.SL_Reference);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArriveWarehouse_FlagNotChange

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_StopLoad_ArriveWarehouse_FlagNotChange()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = true;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceSuspended, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be true", true, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.SingleOrDefault(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNull("Should not have Status Updated log", log);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArriveWarehouse_FlagNotChange

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_UseEventUpdateAwaitingForwardingChanges_CancelStopLoad_ArriveWarehouse_FlagNotChange()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   0 containers
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactoryAfterRunningLogWalker.LoadTop1<WhsItemDispatchLoadList>(new ZQuery());
			loadList.WDL_IsAwaitingForwardingChanges = false;

			newBizOFactoryAfterRunningLogWalker.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireUniversalEventFromConsol(consol, Events.ServiceRequested, $"|LOC=NZAKL|TYP=LOAD");

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterReimport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("The replaced load list should not be deleted.", 1, loadListsAfterReimport.Length);

			var loadlisAfterReimport = loadListsAfterReimport.Single(l => l.PK == loadList.PK);
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, loadlisAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", loadList.PK, loadlisAfterReimport.PK);

			var logs = newBizOFactoryAfterRunningLogWalker2.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, loadlisAfterReimport.PK));
			var log = logs.SingleOrDefault(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);
			AssertNull("Should not have Status Updated log", log);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ForwarderRenamesContainersAndReimports

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderRenamesContainersAndReimports()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipments
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var container1 = CreateContainer(consol, "CONT1");
			var unallocatedContainer = CreateContainer(consol, "CONT2");

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container: null, reference: "P1");
			var packline2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container: container1, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list for the container and another for loose packages", 2, loadLists.Length);
			var loadListForLoosePackages = loadLists.SingleOrDefault(l => "P1" == l.PackageStates.Single().Package.KP_PackageID);
			var containerLoadList = loadLists.SingleOrDefault(l => "P2" == l.PackageStates.Single().Package.KP_PackageID);
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT2" },
				loadListForLoosePackages.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1" },
				containerLoadList.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have created 1 Dispatch Consignment", new string[] { "HSB1" }, dispatchConsignments.Select(c => c.WDC_ConsignmentID));

			// Rename the packed container, then reimport
			container1.JC_ContainerNum = "CONT3";

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterImport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created for the renamed container", 3, loadListsAfterImport.Length);
			var loadListForLoosePackagesAfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK == loadListForLoosePackages.PK);
			var oldContainerLoadListAfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK == containerLoadList.PK);
			var newContainerLoadListAfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK != containerLoadList.PK && l.PK != loadListForLoosePackages.PK);
			AssertNotNull("Should have reused the load list for the Loose Packages", loadListForLoosePackagesAfterImport);
			AssertNotNull("Should not remove the load list for the replaced Container", oldContainerLoadListAfterImport);
			AssertNotNull("Should have created a new load list for the renamed container", newContainerLoadListAfterImport);
			AssertEquals(0, oldContainerLoadListAfterImport.DispatchTransportationUnits.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT2" },
				loadListForLoosePackagesAfterImport.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT3" },
				newContainerLoadListAfterImport.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));

			AssertEquals("The new load list should be activated", true, newContainerLoadListAfterImport.WDL_IsActive);
			AssertEquals("The new load list should not be completed.", false, newContainerLoadListAfterImport.WDL_CompleteTime.IsValid);
			AssertEquals("The replaced load list should have its packages detached", 0, oldContainerLoadListAfterImport.PackageStates.Count);
			AssertEquals("The replaced load lists should be deactivated", false, oldContainerLoadListAfterImport.WDL_IsActive);
			AssertEquals(1, oldContainerLoadListAfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertEquals("The replaced load list should not be completed.", false, oldContainerLoadListAfterImport.WDL_CompleteTime.IsValid);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ForwarderRemovesContainerAndReimports

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderRemovesContainerAndReimports()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");
			var unallocatedContainer = CreateContainer(consol, "CONT3");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container: null, reference: "P1");
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container: container1, reference: "P2");
			var packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container: container2, reference: "P3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment.PK, outboundLeg);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list per container and the consol", 3, loadLists.Length);
			var loadListForLoosePackages = loadLists.SingleOrDefault(l => "P1" == l.PackageStates.Single().Package.KP_PackageID);
			var containerLoadList1 = loadLists.SingleOrDefault(l => "P2" == l.PackageStates.Single().Package.KP_PackageID);
			var containerLoadList2 = loadLists.SingleOrDefault(l => "P3" == l.PackageStates.Single().Package.KP_PackageID);
			AssertNotNull("Should have created a load list for the Loose Packages", loadListForLoosePackages);
			AssertNotNull("Should have created a load list for each container", containerLoadList1);
			AssertNotNull("Should have created a load list for each container", containerLoadList2);

			AssertContainsExactElementsInAnyOrder(new string[] { "CONT3" },
				loadListForLoosePackages.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1" },
				containerLoadList1.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT2" },
				containerLoadList2.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have created 1 Dispatch Consignment", new string[] { "HSB1" }, dispatchConsignments.Select(c => c.WDC_ConsignmentID));

			// Rename a container and detach its package, then reimport
			packline3.JL_JC = ZGuid.Empty;
			consol.Containers.Remove(container2);

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterImport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should not have created or removed load lists", 3, loadListsAfterImport.Length);
			var loadListForLoosePackagesAfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK == loadListForLoosePackages.PK);
			var containerLoadList1AfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK == containerLoadList1.PK);
			var containerLoadList2AfterImport = loadListsAfterImport.SingleOrDefault(l => l.PK == containerLoadList2.PK);
			AssertNotNull("Should have reused the load list for the Loose Packages", loadListForLoosePackagesAfterImport);
			AssertNotNull("Should have reused the load list for the unchanged Container", containerLoadList1AfterImport);
			AssertNotNull("Should not remove unused load lists", containerLoadList2AfterImport);

			AssertContainsExactElementsInAnyOrder(new string[] { "CONT3" },
				loadListForLoosePackagesAfterImport.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1" },
				containerLoadList1AfterImport.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertEquals(0, containerLoadList2AfterImport.DispatchTransportationUnits.Count);

			AssertEquals("The removed container's load list should have no packages", 0, containerLoadList2AfterImport.PackageStates.Count);
			AssertEquals("The removed container's load list should be deactivated", false, containerLoadList2AfterImport.WDL_IsActive);
			AssertEquals(1, containerLoadList2AfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertEquals("The removed container's load list should not be completed.", false, containerLoadList2AfterImport.WDL_CompleteTime.IsValid);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ForwarderRemovesAllContainersAndReimports

		// These are rare business cases.
		// For now if a Consol is imported with no containers we first try to match the most recent load list with no containers (for the consol)
		// Then we fallback to using the most recent load list with containers
		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderRemovesAllContainersAndReimports()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   3 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");
			var unallocatedContainer = CreateContainer(consol, "CONT3");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container: null, reference: "P1");
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container: container1, reference: "P2");
			var packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container: container2, reference: "P3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment.PK, outboundLeg);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Precondition: Should have created a load list per container and the consol", 3, loadLists.Length);
			var loadListForLoosePackages = loadLists.SingleOrDefault(l => l.PackageStates.Single().Package.KP_PackageID == "P1");
			var containerLoadList1 = loadLists.SingleOrDefault(l => l.PackageStates.Single().Package.KP_PackageID == "P2");
			var containerLoadList2 = loadLists.SingleOrDefault(l => l.PackageStates.Single().Package.KP_PackageID == "P3");
			AssertNotNull("Precondition: Should have created a load list for the Loose Packages", loadListForLoosePackages);
			AssertNotNull("Precondition: Should have created a load list for each container", containerLoadList1);
			AssertNotNull("Precondition: Should have created a load list for each container", containerLoadList2);
			AssertEquals("Precondition: The load list should be activated", true, loadListForLoosePackages.WDL_IsActive);
			AssertEquals("Precondition: The load list should be activated", true, containerLoadList1.WDL_IsActive);
			AssertEquals("Precondition: The load list should be activated", true, containerLoadList2.WDL_IsActive);
			AssertEquals("Precondition: The load list should not be completed.", false, containerLoadList2.WDL_CompleteTime.IsValid);

			AssertContainsExactElementsInAnyOrder(new string[] { "CONT3" },
				loadListForLoosePackages.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1" },
				containerLoadList1.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT2" },
				containerLoadList2.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have created 1 Dispatch Consignment", new string[] { "HSB1" }, dispatchConsignments.Select(c => c.WDC_ConsignmentID));

			// Remove all containers and make container1 the most recent
			consol.Containers.Remove(container1);
			consol.Containers.Remove(container2);
			consol.Containers.Remove(unallocatedContainer);
			packline2.JL_JC = ZGuid.Empty;
			packline3.JL_JC = ZGuid.Empty;
			Factory.Save();

			var newUniversalFactory = new UniversalObjectFactory(); // we have to use Unviersal factory so that WDL_SystemCreateTimeUtc isn't reset after factory save.
			var loadListForLoosePackagesInNewFactory = newUniversalFactory.Load<WhsItemDispatchLoadList>(loadListForLoosePackages.PK);
			var containerLoadList1InNewFactory = newUniversalFactory.Load<WhsItemDispatchLoadList>(containerLoadList1.PK);
			var containerLoadList2InNewFactory = newUniversalFactory.Load<WhsItemDispatchLoadList>(containerLoadList2.PK);
			containerLoadList1InNewFactory.WDL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			containerLoadList2InNewFactory.WDL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			loadListForLoosePackagesInNewFactory.WDL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			newUniversalFactory.SaveForTesting();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterImport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should not have created or removed load lists", 3, loadListsAfterImport.Length);
			var loadListForLoosePackagesAfterImport = loadListsAfterImport.Single(l => l.PK == loadListForLoosePackages.PK);
			var containerLoadList1AfterImport = loadListsAfterImport.Single(l => l.PK == containerLoadList1.PK);
			var containerLoadList2AfterImport = loadListsAfterImport.Single(l => l.PK == containerLoadList2.PK);

			// The most recently created load list should be reused as all have containers.
			AssertEquals("The reused load list should be activated", true, containerLoadList1AfterImport.WDL_IsActive);
			AssertEquals("The reused load list should not be completed.", false, containerLoadList1AfterImport.WDL_CompleteTime.IsValid);
			AssertEquals("The unused load list should be deactivated", false, loadListForLoosePackagesAfterImport.WDL_IsActive);
			AssertEquals(1, loadListForLoosePackagesAfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertEquals("The unused load list should not be completed.", false, loadListForLoosePackagesAfterImport.WDL_CompleteTime.IsValid);
			AssertEquals("The unused load list should be deactivated", false, containerLoadList2AfterImport.WDL_IsActive);
			AssertEquals(1, containerLoadList2AfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertEquals("The unused load list should not be completed.", false, containerLoadList2AfterImport.WDL_CompleteTime.IsValid);

			AssertEquals("The reused load list should have all packages", 3, containerLoadList1AfterImport.PackageStates.Count);
			AssertEquals("The unused load list should have packages detached", 0, loadListForLoosePackagesAfterImport.PackageStates.Count);
			AssertEquals("The unused load list should have packages detached", 0, containerLoadList2AfterImport.PackageStates.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderAddsButThenRemovesAllContainers()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   0 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "P1");
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment.PK, outboundLeg);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a single load list", 1, loadLists.Length);
			var loadListForLoosePackages = loadLists.Single();
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(),
				loadListForLoosePackages.DispatchTransportationUnits.Select(d => d.WDH_VehicleReference));
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have created 1 Dispatch Consignment", new string[] { "HSB1" }, dispatchConsignments.Select(c => c.WDC_ConsignmentID));

			// Add containers and reimport
			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");
			packline1.JL_JC = container1.PK;
			packline2.JL_JC = container2.PK;

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterImport = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Precondition: Should have created new load list for the containers", 2, loadListsAfterImport.Length);
			var loadListForLoosePackagesAfterImport = loadListsAfterImport.SingleOrDefault(l => loadListForLoosePackages.PK == l.PK);
			var containerLoadList1AfterImport = loadListsAfterImport.SingleOrDefault(l => l.PackageStates.Any() && l.PackageStates.Single().Package.KP_PackageID == "P1");
			var containerLoadList2AfterImport = loadListsAfterImport.SingleOrDefault(l => l.PackageStates.Any() && l.PackageStates.Single().Package.KP_PackageID == "P2");
			AssertNotNull("Precondition: Should not have removed the loose load list", loadListForLoosePackagesAfterImport);
			AssertNotNull("Precondition: Should have created a load list for each container", containerLoadList1AfterImport);
			AssertNotNull("Precondition: Should have created a load list for each container", containerLoadList2AfterImport);
			Assert("Precondition: The load list should be active", loadListForLoosePackagesAfterImport.WDL_IsActive);
			Assert("Precondition: The load list should not be completed.", !loadListForLoosePackagesAfterImport.WDL_CompleteTime.IsValid);
			Assert("Precondition: The load list should be activated", containerLoadList1AfterImport.WDL_IsActive);
			Assert("Precondition: The load list should not be completed.", !containerLoadList1AfterImport.WDL_CompleteTime.IsValid);
			Assert("Precondition: The load list should be activated", containerLoadList2AfterImport.WDL_IsActive);
			Assert("Precondition: The load list should not be completed.", !containerLoadList2AfterImport.WDL_CompleteTime.IsValid);

			// Remove all containers
			consol.Containers.Remove(container1);
			consol.Containers.Remove(container2);
			packline1.JL_JC = ZGuid.Empty;
			packline2.JL_JC = ZGuid.Empty;

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterImport2 = newBizOFactoryAfterRunningLogWalker3.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should not have created or removed load lists", 2, loadListsAfterImport2.Length);
			var loadListForLoosePackagesAfterImport2 = loadListsAfterImport2.SingleOrDefault(l => l.PK == loadListForLoosePackages.PK);
			var containerLoadList1AfterImport2 = loadListsAfterImport2.SingleOrDefault(l => l.PK == containerLoadList1AfterImport.PK);
			var containerLoadList2AfterImport2 = loadListsAfterImport2.SingleOrDefault(l => l.PK == containerLoadList2AfterImport.PK);
			AssertNotNull("Should not have removed a load list", loadListForLoosePackagesAfterImport2);
			AssertNotNull("Should not have removed a load list", containerLoadList1AfterImport2);
			AssertNotNull("Should not have removed a load list", containerLoadList2AfterImport2);

			AssertEquals("The load list with no containers should be updated.", 2, loadListForLoosePackagesAfterImport2.PackageStates.Count);

			Assert("The load list with no containers should be active.", loadListForLoosePackagesAfterImport2.WDL_IsActive);
			Assert("The reused load list should not be completed.", !loadListForLoosePackagesAfterImport2.WDL_CompleteTime.IsValid);
			Assert("The unused load list should be not changed", containerLoadList1AfterImport2.WDL_IsActive);
			Assert("The unused load list should not be completed.", !containerLoadList1AfterImport2.WDL_CompleteTime.IsValid);
			Assert("The unused load list should be deactivated", !containerLoadList2AfterImport2.WDL_IsActive);
			AssertEquals(1, containerLoadList2AfterImport2.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			Assert("The unused load list should not be completed.", !containerLoadList2AfterImport2.WDL_CompleteTime.IsValid);
		}

		#endregion

		#region TestImportFowardingConsol_ForDispatch_ForwarderRemovesContainerAndShipmentAndReimports

		[TestDate(2018, 1, 1)]
		public void TestImportFowardingConsol_ForDispatch_ForwarderRemovesContainerAndShipmentAndReimports()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot :          ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container: container1, reference: "P1");
			var packline2 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, container: container2, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcn2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);

			// Dispatch consignments and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dtusAfterImport1 = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new string[] { "CONT1", "CONT2" }, dtusAfterImport1.Select(d => d.WDH_VehicleReference));
			AssertEquals(2, newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.WDL_IsActive, true)).Length);
			AssertEquals(2, newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery()).Length);
			var pkgPackageExtensions = newBizOFactoryAfterRunningLogWalker.Load<PkgPackageExtension>(new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, dtusAfterImport1.Select(d => d.PK)));
			AssertEquals(2, pkgPackageExtensions.Length);
			var pkgPackages = newBizOFactoryAfterRunningLogWalker.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, pkgPackageExtensions.Select(t => t.KPN_KP_Package)));
			AssertEquals(2, pkgPackages.Length);
			AssertEquals(2, newBizOFactoryAfterRunningLogWalker.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.PK, pkgPackages.Select(t => t.KP_KPH_PackageHeader))).Length);
			AssertEquals(2, newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgPackages.Select(t => t.PK))).Length);
			AssertEquals(2, newBizOFactoryAfterRunningLogWalker.Load<PkgPackageContainer>(new ZQuery(PkgPackageContainerSchema.K0_KP_Package, pkgPackages.Select(t => t.PK))).Length);

			// Rename a container and shipment, then reimport
			consol.Containers.Remove(container2);
			consol.Shipments.Remove(shipment2);

			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var dtusAfterImport2 = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("CONT1", dtusAfterImport2.Single().WDH_VehicleReference);
			AssertEquals(1, newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.WDL_IsActive, true)).Length);
			AssertEquals(1, newBizOFactoryAfterRunningLogWalker2.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery()).Length);
			pkgPackageExtensions = newBizOFactoryAfterRunningLogWalker2.Load<PkgPackageExtension>(new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, dtusAfterImport1.Select(d => d.PK)));
			AssertEquals(1, pkgPackageExtensions.Length);
			pkgPackages = newBizOFactoryAfterRunningLogWalker2.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, pkgPackageExtensions.Select(t => t.KPN_KP_Package)));
			AssertEquals(1, pkgPackages.Length);
			AssertEquals(1, newBizOFactoryAfterRunningLogWalker2.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.PK, pkgPackages.Select(t => t.KP_KPH_PackageHeader))).Length);
			AssertEquals(1, newBizOFactoryAfterRunningLogWalker2.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgPackages.Select(t => t.PK))).Length);
			AssertEquals(1, newBizOFactoryAfterRunningLogWalker2.Load<PkgPackageContainer>(new ZQuery(PkgPackageContainerSchema.K0_KP_Package, pkgPackages.Select(t => t.PK))).Length);
		}

		#endregion

		#region TestTWReceivesBlindConsignment_CreateHandlingUnits_ForwarderSendsDispatchInstructions_ViaConsol_AfterAttachingHandlingUnit

		[TestDate(2020, 04, 20)]
		public void TestTWReceivesBlindConsignment_CreateHandlingUnits_ForwarderSendsDispatchInstructions_ViaConsol_AfterAttachingHandlingUnit()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var divotToChild1 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			var divotToChild2 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "HU1");
			var now = ZDateTimeOffset.Now;
			Helper.UnpackPackageFromHandlingUnit(divotToChild1, now, "AAA");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var handlingUnitPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(handlingUnitPackage.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage2.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", now, divotToChild1.KPD_UnpackedTime);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, divotToChild2.KPD_UnpackedTime);
			AssertEquals(ZGuid.Empty, handlingUnitPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZGuid.Empty, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertDispatchConsignment(childPackage2InNewFactory, dispatchConsignment, shipment.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(ZGuid.Empty, handlingUnitPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(ZGuid.Empty, childPackage1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage2InNewFactory.WPS_WDL_LoadList);
		}

		public void TestTWReceivesBlindConsignment_CreateHandlingUnits_ForwarderSendsDispatchInstructions_ViaConsol_AfterAttachingHandlingUnit_HasContainer()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "HU1");

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, entryNum: shipment.JobNumber);

			var packedChildPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var unpackedChildPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var attachedChildPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipment.JobNumber);

			var divotToChild1 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packedChildPackage, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			var divotToChild2 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, unpackedChildPackage, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			var divotToChild3 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, attachedChildPackage, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			Helper.UnpackPackageFromHandlingUnit(divotToChild1, now, "AAA");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var handlingUnitPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(handlingUnitPackage.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(packedChildPackage.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(unpackedChildPackage.PK);
			var childPackage3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(attachedChildPackage.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", now, divotToChild1.KPD_UnpackedTime);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, divotToChild2.KPD_UnpackedTime);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, divotToChild3.KPD_UnpackedTime);
			AssertEquals(ZGuid.Empty, handlingUnitPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZGuid.Empty, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertDispatchConsignment(childPackage2InNewFactory, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(childPackage3InNewFactory, dispatchConsignment, shipment.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(ZGuid.Empty, handlingUnitPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(ZGuid.Empty, childPackage1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage2InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage3InNewFactory.WPS_WDL_LoadList);
		}

		#endregion

		#region TestCreateLoadPlan

		#region TestCreateLoadPlan_PacklinesAllocatedToContainersWithQuantityOne_NoContainerNumber

		public void TestCreateLoadPlan_PacklinesAllocatedToContainersWithQuantityOne_NoContainerNumber()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 3);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);
			var loadListForContainer1 = loadLists.Single(l => l.DispatchTransportationUnits.Single().Container.ContainerType.RC_Code == "20GP");
			var loadListForContainer2 = loadLists.Single(l => l.DispatchTransportationUnits.Single().Container.ContainerType.RC_Code == "40GP");

			var dtuForLoadList1 = loadListForContainer1.DispatchTransportationUnits.Single();
			var dtuForLoadList2 = loadListForContainer2.DispatchTransportationUnits.Single();
			AssertEquals("", dtuForLoadList1.ContainerNumber);
			AssertEquals("", dtuForLoadList2.ContainerNumber);

			var packageStatesForContainer1 = loadListForContainer1.PackageStates;
			var packageStatesForContainer2 = loadListForContainer2.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(1, packageStatesForContainer2.Count);
			var packageState1InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);
			var packageState1InContainer2 = packageStatesForContainer2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
			AssertEquals("PKG3", packageState1InContainer2.Package.KP_PackageID);
		}

		#endregion

		#region TestCreateLoadPlan_PacklinesAllocatedToContainersWithQuantityMoreThan1

		public void TestCreateLoadPlan_PacklinesAllocatedToContainersWithQuantityMoreThan1()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "", containerCount: 3, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 3);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);

			var loadListFor20GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");
			var twentyGPContainers = loadListFor20GPContainer.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainer.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(3, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			var packageStatesForContainer1 = loadListFor20GPContainer.PackageStates;
			var packageStatesForContainer2 = loadListFor40GPContainer.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(1, packageStatesForContainer2.Count);

			var packageState1InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);
			var packageState1InContainer2 = packageStatesForContainer2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
			AssertEquals("PKG3", packageState1InContainer2.Package.KP_PackageID);
		}

		#endregion

		#region TestCreateLoadPlan_UnAssignedPackLines_SingleUnAllocatedContainer

		public void TestCreateLoadPlan_UnAssignedPackLines_SingleUnAllocatedContainer()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "", containerCount: 3, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var unAllocatedPackLine = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, null, "PKG3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 3);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);
			var loadListFor20GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");

			var twentyGPContainers = loadListFor20GPContainer.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainer.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(3, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			var packageStatesForContainer1 = loadListFor20GPContainer.PackageStates;
			var packageStatesForContainer2 = loadListFor40GPContainer.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(1, packageStatesForContainer2.Count);
			var packageState1InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);
			var packageState1InContainer2 = packageStatesForContainer2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
			AssertEquals("PKG3", packageState1InContainer2.Package.KP_PackageID);
		}

		#endregion

		#region TestCreateLoadPlan_UnAssignedPackLines_MultipleUnAllocatedContainers

		public void TestCreateLoadPlan_UnAssignedPackLines_MultipleUnAllocatedContainers()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var emptyContainer1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "40GP");
			var emtpyContainer2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "20GP");
			var emtpyContainer3 = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "40GP");

			var unAllocatedPackLine1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, null, "PKG1");
			var unAllocatedPackLine2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, null, "PKG2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 2);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertUnAssignedPackLines_MultipleUnAllocatedContainers(newBizOFactoryAfterRunningLogWalker);

			TriggerAndFireTransitRequestForRelease(consol); // resending

			var newBizOFactoryAfterResending = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertUnAssignedPackLines_MultipleUnAllocatedContainers(newBizOFactoryAfterResending);
		}

		static void AssertUnAssignedPackLines_MultipleUnAllocatedContainers(BusinessObjectFactory factory)
		{
			var loadLists = factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(1, loadLists.Count(l => l.PackageStates.Any()));

			var dtusForLoadList = loadLists.Single(l => l.PackageStates.Any()).DispatchTransportationUnits;
			AssertEquals(4, dtusForLoadList.Count);

			var containersForDTUs = dtusForLoadList.Select(l => l.Container);
			AssertEquals(4, containersForDTUs.Count());
			var dtuForA = dtusForLoadList.Single(c => c.ContainerNumber == "A");
			var dtuForB = dtusForLoadList.Single(c => c.ContainerNumber == "B");
			var fourtyGPEmptyContainerNumberDTUs = dtusForLoadList.Where(c => c.ContainerNumber == "");
			AssertEquals("A", dtuForA.WDH_VehicleReference);
			AssertEquals("B", dtuForB.WDH_VehicleReference);
			AssertEquals(2, fourtyGPEmptyContainerNumberDTUs.Count());
			Assert(fourtyGPEmptyContainerNumberDTUs.All(d => d.Container.ContainerType.RC_Code == "40GP"));
			Assert(fourtyGPEmptyContainerNumberDTUs.All(d => d.WDH_VehicleReference == ""));

			var packageStatesForLoadList = loadLists.Single(l => l.PackageStates.Any()).PackageStates;
			AssertEquals(2, packageStatesForLoadList.Count);
			var packageState1InContainer1 = packageStatesForLoadList.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForLoadList.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
		}

		#endregion

		#region TestCreateLoadPlan_UnAssignedPackLines_AllContainersAllocated

		public void TestCreateLoadPlan_UnAssignedPackLines_AllContainersAllocated()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");
			var unAllocatedPackLine = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, null, "PKG4");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 4);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("No load lists must be created since there is an unallocated packline.", 0, loadLists.Length);
		}

		#endregion

		#endregion

		#region TestImportETA_ShipmentsWithConsol_RCN

		public void TestImportETA_ShipmentsWithConsol_RCN_ShouldTakeShipmentDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var inboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "AUSYD", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);

			var estimatedPickup = new ZDateTime(2005, 5, 5);
			var pickupRequiredFrom = new ZDateTime(2006, 6, 6);
			var shipmentWithEstimatedPickup = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor,
				testData.consignee, estimatedPickup: estimatedPickup);
			var shipmentWithPickupRequiredFrom = CreateShipment(consol, "HSB2", "NZCHC", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor,
				testData.consignee, estimatedPickup: ZDateTime.Empty, pickupRequiredFrom: pickupRequiredFrom);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipmentWithEstimatedPickup.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipmentWithPickupRequiredFrom.PK);
			AssertEquals("RCN 1 will fallback to its Shipment's Estimated Fallback date as there is no Inbound Leg ETA", estimatedPickup, rcn1.WRC_ExpectedArrivalTime);
			AssertEquals(
				"RCN 2 will fallback to its Shipment's Pickup Required From date as there is no Inbound Leg ETA and Estimated Pickup",
				pickupRequiredFrom,
				rcn2.WRC_ExpectedArrivalTime
			);
		}

		public void TestImportETA_ShipmentsWithConsol_RCN_ShouldTakeCFSReceival() =>
			TestImportETA_ShipmentWithConsol_RCN_ShouldTakeCFSReceival(extraPortCode: null);

		public void TestImportETA_ShipmentsWithConsol_ExtraPorts_RCN_ShouldTakeCFSReceival() =>
			TestImportETA_ShipmentWithConsol_RCN_ShouldTakeCFSReceival(extraPortCode: "NZCHC", outboundLoadPortCode: "NZCHC");

		void TestImportETA_ShipmentWithConsol_RCN_ShouldTakeCFSReceival(string extraPortCode, string outboundLoadPortCode = "NZAKL")
		{
			var testData = CreateTestData(false);
			if (extraPortCode != null)
			{
				UniversalHelper.CreateExtraPort(testData.warehouse, extraPortCode);
			}

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var inboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "AUSYD", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);
			var outboundLegWithCfsReceival = CreateTransport(consol, 2, "SEA", "A", "AA", outboundLoadPortCode, "AUSYD", ZDateTime.Empty, ZDateTime.Empty);
			var cfsReceival = new ZDateTime(2005, 5, 5);
			outboundLegWithCfsReceival.JW_DepotReceivalCommencesForBinding = cfsReceival;

			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(
				"RCN ETA will use Outbound CFS Receival when there is no Inbound ETA and no Origin Warehouse pickup details",
				cfsReceival,
				rcn.WRC_ExpectedArrivalTime
			);
		}

		#endregion

		#region TestImportETD_ShipmentsWithConsol_RCN

		public void TestImportETD_ShipmentsWithConsol_RCN_ShouldTakeShipmentDeliveryDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipmentWithEstimatedDelivery = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var estimatedDelivery = new ZDateTime(2005, 5, 5);
			shipmentWithEstimatedDelivery.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipmentWithEstimatedDelivery.DocsAndCartage.JP_EstimatedDelivery = estimatedDelivery;

			var shipmentWithDeliveryRequiredBy = CreateShipment(consol, "HSB2", "AUSYD", "NZCHC", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var deliveryRequiredBy = new ZDateTime(2006, 6, 6);
			shipmentWithDeliveryRequiredBy.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipmentWithDeliveryRequiredBy.DocsAndCartage.JP_DeliveryRequiredBy = deliveryRequiredBy;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipmentWithEstimatedDelivery.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipmentWithDeliveryRequiredBy.PK);

			AssertEquals(estimatedDelivery, rcn1.WRC_ExpectedDispatchTime);
			AssertEquals(deliveryRequiredBy, rcn2.WRC_ExpectedDispatchTime);
		}

		public void TestImportETD_ShipmentsWithConsol_RCN_ShouldTakeOutboundLegDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "NZAKL", "AUSYD", ZDateTime.Empty, ZDateTime.Empty);

			var shipment1 = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var ctoCutOff = new ZDateTime(2005, 5, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOff;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);

			AssertEquals(ctoCutOff, rcn1.WRC_ExpectedDispatchTime);

			var shipment2 = CreateShipment(consol, "HSB2", "AUSYD", "NZCHC", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment2.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;
			var etd = new ZDateTime(2006, 6, 6);
			outboundLeg.JW_ETDForBinding = etd;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipment2.PK);

			AssertEquals(etd, rcn2.WRC_ExpectedDispatchTime);
		}

		#endregion

		#region TestImportETA_ShipmentsWithConsol_ASN

		public void TestImportETA_ShipmentWithConsol_ASN_ShouldTakeInboundEta() =>
			TestImportETA_ShipmentWithConsol_ASN_ShouldTakeInboundEta(extraPortCode: null);

		public void TestImportETA_ShipmentWithConsol_ExtraPort_ASN_ShouldTakeInboundEta() =>
			TestImportETA_ShipmentWithConsol_ASN_ShouldTakeInboundEta(extraPortCode: "NZCHC", inboundDischargePortCode: "NZCHC");

		void TestImportETA_ShipmentWithConsol_ASN_ShouldTakeInboundEta(string extraPortCode, string inboundDischargePortCode = "NZAKL")
		{
			var testData = CreateTestData(false);
			if (extraPortCode != null)
			{
				UniversalHelper.CreateExtraPort(testData.warehouse, extraPortCode);
			}

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var inboundLegEta = new ZDateTime(2005, 5, 5);
			CreateTransport(consol, 2, "SEA", "A", "AA", "AUSYD", inboundDischargePortCode, ZDateTime.Empty, inboundLegEta);

			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			var asn = rcn.PackageStates.SingleOrDefault().ReceiveASN;
			AssertEquals(inboundLegEta, asn.WRP_ETA);
		}

		public void TestImportETA_ShipmentsWithConsol_ASN_ShouldTakeEarliestEstimatedPickupAsFallback()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var inboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "AUSYD", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);

			var earliestEstimatedPickup = new ZDateTime(2005, 5, 5);
			var earliestPickupRequiredFrom = new ZDateTime(2006, 6, 6);

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee,
				estimatedPickup: earliestEstimatedPickup, pickupRequiredFrom: earliestPickupRequiredFrom.AddHours(3));
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);

			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee,
				estimatedPickup: earliestEstimatedPickup.AddHours(3), pickupRequiredFrom: earliestPickupRequiredFrom.AddHours(3));
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipment2.PK);
			var rcn1Asn = rcn1.PackageStates.SingleOrDefault().ReceiveASN;
			var rcn2Asn = rcn2.PackageStates.SingleOrDefault().ReceiveASN;
			AssertEquals("Only one ASN is generated for consol", rcn1Asn, rcn2Asn);
			AssertEquals("The single ASN's ETA will use the earliest Estimated Pickup from all possible shipments", earliestEstimatedPickup, rcn1Asn.WRP_ETA);
		}

		public void TestImportETA_ShipmentsWithConsol_ASN_ShouldTakeEarliestPickupRequiredFromAsFallback()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var inboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "AUSYD", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);

			var earliestPickupRequiredFrom = new ZDateTime(2006, 6, 6);
			var shipmentWithIgnoredDates = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee, estimatedPickup: ZDateTime.Empty, pickupRequiredFrom: earliestPickupRequiredFrom.AddHours(4));
			CreateOuterPackline(shipmentWithIgnoredDates, 1, Constants.PkgUnit.Package);

			var shipmentWithEarliestPickupRequiredFrom = CreateShipment(consol, "HSB2", "NZCHC", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee, estimatedPickup: ZDateTime.Empty, pickupRequiredFrom: earliestPickupRequiredFrom);
			CreateOuterPackline(shipmentWithEarliestPickupRequiredFrom, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipmentWithIgnoredDates.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipmentWithEarliestPickupRequiredFrom.PK);
			var rcn1Asn = rcn1.PackageStates.SingleOrDefault().ReceiveASN;
			var rcn2Asn = rcn2.PackageStates.SingleOrDefault().ReceiveASN;
			AssertEquals("Only one ASN is generated for consol", rcn1Asn, rcn2Asn);
			AssertEquals("The single ASN's ETA will use the earliest Pickup Required From from all possible shipments", earliestPickupRequiredFrom, rcn1Asn.WRP_ETA);
		}

		public void TestImportETA_ShipmentWithConsol_ASN_ShouldTakeCFSReceivalAsFallback() =>
			TestImportETA_ShipmentWithConsol_ASN_ShouldTakeCFSReceivalAsFallback(extraPortCode: null);

		public void TestImportETA_ShipmentWithConsol_ExtraPort_ASN_ShouldTakeCFSReceivalAsFallback() =>
			TestImportETA_ShipmentWithConsol_ASN_ShouldTakeCFSReceivalAsFallback(extraPortCode: "NZCHC", outboundLoadPortCode: "NZCHC");

		void TestImportETA_ShipmentWithConsol_ASN_ShouldTakeCFSReceivalAsFallback(string extraPortCode, string outboundLoadPortCode = "NZAKL")
		{
			var testData = CreateTestData(false);
			if (extraPortCode != null)
			{
				UniversalHelper.CreateExtraPort(testData.warehouse, extraPortCode);
			}

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			CreateTransport(consol, 1, "SEA", "A", "AA", "AUSYD", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);
			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", outboundLoadPortCode, "AUSYD", ZDateTime.Empty, ZDateTime.Empty);
			var cfsReceival = new ZDateTime(2005, 5, 5);
			outboundLeg.JW_DepotReceivalCommencesForBinding = cfsReceival;

			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			var asn = rcn.PackageStates.SingleOrDefault().ReceiveASN;

			AssertEquals("The ASN ETA will only use the CFS Receival if there's no Inbound ETA or any Shipment Estimated Pickup or Pickup Required From", cfsReceival, asn.WRP_ETA);
		}

		#endregion

		#region TestDeparturePortTransportOrg

		public void TestDeparturePortTransportOrg_WithConsol_SendInstructionsFromConsolLevel()
		{
			var testData = CreateTestData(false, "NZCHC");
			var departureTransportOrg = TestHelper.CreateOrganisation("DEP");
			var arrivalTransportOrg = TestHelper.CreateOrganisation("ARV");

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = departureTransportOrg.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportOrg.MainAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var asn = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals(departureTransportOrg.PK, asn.TransportCompany.OrganisationPK);

			TriggerAndFireTransitRequestForRelease(consol);
			var dtu = newFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals(departureTransportOrg.PK, dtu.TransportCompany.OrganisationPK);
		}

		public void TestArrivalPortTransportOrg_WithConsol_SendInstructionsFromConsolLevel()
		{
			var testData = CreateTestData(true, "NZCHC");
			var departureTransportOrg = TestHelper.CreateOrganisation("DEP");
			var arrivalTransportOrg = TestHelper.CreateOrganisation("ARV");

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = departureTransportOrg.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportOrg.MainAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var asn = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals(arrivalTransportOrg.PK, asn.TransportCompany.OrganisationPK);

			var rtu = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals(arrivalTransportOrg.PK, rtu.TransportCompany.OrganisationPK);

			TriggerAndFireTransitRequestForRelease(consol);
			var dtu = newFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals(arrivalTransportOrg.PK, dtu.TransportCompany.OrganisationPK);
		}

		#endregion

		#region TestImportETD_ShipmentsWithConsol_DCN

		readonly TimeSpan aucklandUtcOffset = new TimeSpan(12, 0, 0);
		readonly TimeSpan aucklandDstUtcOffset = new TimeSpan(13, 0, 0);

		public void TestImportETD_ShipmentsWithConsol_DCN_ShouldTakeDeliveryDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipmentWithEstimatedDelivery = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var estimatedDelivery = new ZDateTime(2005, 5, 5);
			shipmentWithEstimatedDelivery.DocsAndCartage.JP_EstimatedDelivery = estimatedDelivery;

			var shipmentWithDeliveryRequiredBy = CreateShipment(consol, "HSB2", "AUSYD", "NZCHC", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var deliveryRequiredByDst = new ZDateTime(2007, 10, 1);
			shipmentWithDeliveryRequiredBy.DocsAndCartage.JP_DeliveryRequiredBy = deliveryRequiredByDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var dcnWithEstimatedDeliveryShipment = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipmentWithEstimatedDelivery.PK);
			var dcnWithDeliveryRequiredByShipment = AssertAndReturnDispatchConsignment(Factory, "HSB2", testData.warehouse, shipmentWithDeliveryRequiredBy.PK);

			CombineAssertions(() =>
			{
				AssertEquals(
					"Estimated Delivery should be used first AND its UTC Offset is that of the Warehouse location (NZAKL)",
					new ZDateTimeOffset(estimatedDelivery, aucklandUtcOffset),
					dcnWithEstimatedDeliveryShipment.WDC_ExpectedDispatchTime
				);
				AssertEquals(
					"For shipments without Estimated Delivery, fallback to Delivery Required By AND its UTC Offset is that of the Warehouse location (NZAKL)",
					new ZDateTimeOffset(deliveryRequiredByDst, aucklandDstUtcOffset),
					dcnWithDeliveryRequiredByShipment.WDC_ExpectedDispatchTime
				);
			});
		}

		public void TestImportETD_ShipmentsWithConsol_DCN_FallsbackToOutboundRoutingDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUPER");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZCHC");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "NZAKL", "NZCHC", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOff = new ZDateTime(2005, 5, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOff;

			TriggerAndFireTransitRequestForRelease(consol);

			var dcn = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(
				"Without necessary shipment details, fallback to CTO Cut Off AND its UTC Offset is from Warehouse which is in Auckland.",
				new ZDateTimeOffset(ctoCutOff, aucklandUtcOffset),
				dcn.WDC_ExpectedDispatchTime
			);

			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;
			outboundLeg.JW_RL_NKLoadPort = "AUPER"; // Testing that the leg is still outbound for the additional port
			var etd = new ZDateTime(2006, 6, 6);
			outboundLeg.JW_ETDForBinding = etd;

			TriggerAndFireTransitRequestForRelease(consol);

			Factory.ReloadAll<WhsItemDispatchConsignment>(); // Fixes issue where DCN isn't retrieved from the updated DB values
			dcn = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(
				"Without CTO Cut Off, fallback to ETD AND its UTC Offset is from Warehouse which is in Auckland.",
				new ZDateTimeOffset(etd, aucklandUtcOffset),
				dcn.WDC_ExpectedDispatchTime
			);
		}

		public void TestImportETD_ShipmentsWithConsol_DCN_ShouldNotOverwriteIfEmpty()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUPER");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZCHC");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "NZAKL", "NZCHC", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOff = new ZDateTime(2005, 5, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOff;

			TriggerAndFireTransitRequestForRelease(consol);

			var expectedAucklandDate = new ZDateTimeOffset(ctoCutOff, aucklandUtcOffset);
			var dcn = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(
				"Without necessary shipment details, fallback to CTO Cut Off AND its UTC Offset is from Warehouse which is in Auckland.",
				expectedAucklandDate,
				dcn.WDC_ExpectedDispatchTime
			);

			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;
			outboundLeg.JW_RL_NKLoadPort = "AUPER"; // Testing that the leg is still outbound for the additional port

			TriggerAndFireTransitRequestForRelease(consol);

			Factory.ReloadAll<WhsItemDispatchConsignment>(); // Fixes issue where DCN isn't retrieved from the updated DB values
			dcn = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(
				"We should still use the previous Auckland date as our new value is Empty",
				expectedAucklandDate,
				dcn.WDC_ExpectedDispatchTime
			);
		}

		#endregion

		#region TestImportETD_ShipmentsWithConsol_DLL

		public void TestImportETD_ShipmentsWithConsol_DLL_ShouldTakeEarliestEstimatedDelivery()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var earliestEstimatedDelivery = new ZDateTime(2005, 5, 5);

			var shipment1 = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.DocsAndCartage.JP_EstimatedDelivery = earliestEstimatedDelivery.AddHours(2); // Tests that the first shipment isn't picked up automatically

			var shipment2 = CreateShipment(consol, "HSB2", "AUSYD", "NZCHC", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment2.DocsAndCartage.JP_EstimatedDelivery = earliestEstimatedDelivery;

			TriggerAndFireTransitRequestForRelease(consol);

			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"The DLL's Expected Dispatch will use the earliest Estimated Delivery from all possible shipments AND its UTC Offset is that of the Warehouse location (NZAKL)",
				new ZDateTimeOffset(earliestEstimatedDelivery, aucklandUtcOffset),
				dll.WDL_ExpectedDispatchTime
			);
		}

		public void TestImportETD_ShipmentsWithConsol_DLL_FallsbackToEarliestDeliveryRequiredBy()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var earliestDeliveryRequiredByDst = new ZDateTime(2008, 2, 5);

			var shipment1 = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.DocsAndCartage.JP_DeliveryRequiredBy = earliestDeliveryRequiredByDst.AddHours(2); // Tests that the first shipment isn't picked up automatically

			var shipment2 = CreateShipment(consol, "HSB2", "AUSYD", "NZCHC", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment2.DocsAndCartage.JP_DeliveryRequiredBy = earliestDeliveryRequiredByDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"The DLL's Expected Dispatch will use the earliest Delivery Required By from all possible shipments if there was no Estimated Delivery AND its UTC Offset is that of the Warehouse location (NZAKL)",
				new ZDateTimeOffset(earliestDeliveryRequiredByDst, aucklandDstUtcOffset),
				dll.WDL_ExpectedDispatchTime
			);
		}

		public void TestImportETD_ShipmentsWithConsol_DLL_FallsbackToOutboundRoutingDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUPER");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZCHC");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "NZAKL", "NZCHC", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOffDst = new ZDateTime(2005, 11, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOffDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"Without necessary shipment details, fallback to CTO Cut Off AND its UTC Offset is from Warehouse which is in Auckland.",
				new ZDateTimeOffset(ctoCutOffDst, aucklandDstUtcOffset),
				dll.WDL_ExpectedDispatchTime
			);

			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;
			outboundLeg.JW_RL_NKLoadPort = "AUPER"; // Testing that the leg is still outbound for the additional port
			var etd = new ZDateTime(2006, 6, 6);
			outboundLeg.JW_ETDForBinding = etd;

			TriggerAndFireTransitRequestForRelease(consol);

			Factory.ReloadAll<WhsItemDispatchLoadList>(); // Fixes issue where DLL isn't updated from the DB
			dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"Without CTO Cut Off, fallback to ETD AND its UTC Offset is from Warehouse which is in Auckland.",
				new ZDateTimeOffset(etd, aucklandUtcOffset),
				dll.WDL_ExpectedDispatchTime
			);
		}

		public void TestImportETD_ShipmentsWithConsol_DLL_ShouldNotOverwriteIfEmpty()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUPER");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZCHC");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "NZAKL", "NZCHC", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOffDst = new ZDateTime(2005, 11, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOffDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var expectedAucklandDstDate = new ZDateTimeOffset(ctoCutOffDst, aucklandDstUtcOffset);
			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"Without necessary shipment details, fallback to CTO Cut Off AND its UTC Offset is that of the Leg Origin Port (NZAKL)",
				expectedAucklandDstDate,
				dll.WDL_ExpectedDispatchTime
			);

			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;
			outboundLeg.JW_RL_NKLoadPort = "AUPER"; // Testing that the leg is still outbound for the additional port

			TriggerAndFireTransitRequestForRelease(consol);

			Factory.ReloadAll<WhsItemDispatchLoadList>(); // Fixes issue where DLL isn't updated from the DB
			dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"We should still use the previous Auckland date as our new value is Empty",
				expectedAucklandDstDate,
				dll.WDL_ExpectedDispatchTime
			);
		}

		#endregion

		#region TestImportETD_ShipmentsWithConsol_DCN_DLL

		public void TestImportETD_ShipmentsWithConsol_DCN_DLL_ShouldBeEmptyIfNoValidDetails()
		{
			var testData = CreateTestData(false);

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "NZCHC");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", null, null, ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOffDst = new ZDateTime(2005, 11, 5);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOffDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var dcn = AssertAndReturnDispatchConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();

			CombineAssertions(() =>
			{
				AssertEquals(
					"Without necessary details, DCN ETD should be empty",
					ZDateTimeOffset.Empty,
					dll.WDL_ExpectedDispatchTime
				);
				AssertEquals(
					"Without necessary details, DLL ETD should be empty",
					ZDateTimeOffset.Empty,
					dll.WDL_ExpectedDispatchTime
				);
			});
		}

		#endregion

		#region TestImportCTOCutOff_ShipmentsWithConsol_DLL

		public void TestImportCTOCutOff_ShipmentsWithConsol_DLL_ShouldTakeOutboundRoutingDetails()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUSYD");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "AUMEL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "AUSYD", "AUMEL", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "AUMEL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOffDst = new ZDateTime(2005, 11, 15);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOffDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"CTO Cut Off is retrieved from Outbound Leg CTO Cut Off AND its UTC Offset is from the warehouse which is in Auckland",
				new ZDateTimeOffset(ctoCutOffDst, aucklandDstUtcOffset),
				dll.WDL_CTOCutOffTime
			);
		}

		public void TestImportCTOCutOff_ShipmentsWithConsol_DLL_ShouldNotOverwriteIfEmpty()
		{
			var testData = CreateTestData(false);
			UniversalHelper.CreateExtraPort(testData.warehouse, "AUSYD");

			var consol = CreateConsol("MSB1", testData.vessel, "AUSYD", "AUMEL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", "AUSYD", "AUMEL", ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "AUSYD", "AUMEL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var ctoCutOffDst = new ZDateTime(2005, 11, 15);
			outboundLeg.JW_TerminalCutOffForBinding = ctoCutOffDst;

			TriggerAndFireTransitRequestForRelease(consol);

			var expectedSydneyDstDate = new ZDateTimeOffset(ctoCutOffDst, aucklandDstUtcOffset);
			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"CTO Cut Off is retrieved from Outbound Leg CTO Cut Off AND its UTC Offset is from the Warehouse which is in Auckland",
				expectedSydneyDstDate,
				dll.WDL_CTOCutOffTime
			);

			outboundLeg.JW_TerminalCutOffForBinding = ZDateTime.Empty;

			TriggerAndFireTransitRequestForRelease(consol);

			Factory.ReloadAll<WhsItemDispatchLoadList>(); // Fixes issue where DLL isn't updated from the DB
			dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(
				"We should still use the previous Sydney date as our new value is Empty",
				expectedSydneyDstDate,
				dll.WDL_CTOCutOffTime
			);
		}

		public void TestImportCTOCutOff_ShipmentsWithConsol_DLL_ShouldBeEmptyIfNoValidValues()
		{
			var testData = CreateTestData(false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 2, "SEA", "A", "AA", null, null, ZDateTime.Empty, ZDateTime.Empty);
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);

			TriggerAndFireTransitRequestForRelease(consol);

			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("CTO Cut Off is an Empty date as no valid values exist", ZDateTimeOffset.Empty, dll.WDL_CTOCutOffTime);
		}

		#endregion

		#region Test Create DLL By Multiple Container From Consol

		public void TestCreateDLLByMultipleContainerFromConsol_RegTrue_PKGInOneCNT() => TestCreateDLLByMultipleContainerFromConsolCore(true, false);

		public void TestCreateDLLByMultipleContainerFromConsol_RegTrue_PKGInEachCNT() => TestCreateDLLByMultipleContainerFromConsolCore(true, true);

		public void TestCreateDLLByMultipleContainerFromConsol_RegTrue_HasPKG_CNTAllEmpty() => TestCreateDLLByMultipleContainerFromConsolCore(true, false, true);

		public void TestCreateDLLByMultipleContainerFromConsol_RegTrue_NoPKG_CNTAllEmpty_ShouldReject() => TestCreateDLLByMultipleContainerFromConsolCore(true, false, true);

		public void TestCreateDLLByMultipleContainerFromConsol_RegFalse_PKGInOneCNT() => TestCreateDLLByMultipleContainerFromConsolCore(false, false);

		public void TestCreateDLLByMultipleContainerFromConsol_RegFalse_PKGInEachCNT() => TestCreateDLLByMultipleContainerFromConsolCore(false, true);

		public void TestCreateDLLByMultipleContainerFromConsol_RegFalse_HasPKG_CNTAllEmpty() => TestCreateDLLByMultipleContainerFromConsolCore(false, false, true);

		public void TestCreateDLLByMultipleContainerFromConsol_RegFalse_NoPKG_CNTAllEmpty_ShouldReject() => TestCreateDLLByMultipleContainerFromConsolCore(false, false, true);

		void TestCreateDLLByMultipleContainerFromConsolCore(bool registryValue, bool pkgAssignedToEachCNT, bool allCNTEmpty = false)
		{
			var testData = CreateTestData(true);
			if (registryValue)
			{
				WarehouseDataRegistry.Instance.CreateSingleDLLForAllContainers.SetValue(Guid.Empty, testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true);
			}
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "20GP");
			var container3 = CreateContainer(consol, containerNum: "C", containerCount: 1, containerTypeCode: "20GP");

			ForwardingPackLine packline1 = null;
			ForwardingPackLine packline2 = null;
			ForwardingPackLine packline3 = null;

			packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG1");
			packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG2");
			packline3 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG3");

			if (!allCNTEmpty)
			{
				if (pkgAssignedToEachCNT)
				{
					packline1.JL_JC = container1.PK;
					packline2.JL_JC = container2.PK;
					packline3.JL_JC = container3.PK;
				}
				else
				{
					packline1.JL_JC = container1.PK;
					packline2.JL_JC = container1.PK;
					packline3.JL_JC = container1.PK;
				}
			}
			shipment.UpdateShipmentFromOuterPackLines();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			if (registryValue)
			{
				if (allCNTEmpty)
				{
					AssertEquals("Should create one DLL for all containers.", 1, loadLists.Length);
					AssertEquals("Should create three dtu", 3, dtus.Length);
				}
				else
				{
					if (pkgAssignedToEachCNT)
					{
						AssertEquals("Should create one DLL for all containers.", 1, loadLists.Length);
						AssertEquals("Should create three dtu", 3, dtus.Length);
					}
					else
					{
						AssertEquals("Should create one DLL for all containers.", 1, loadLists.Length);
						AssertEquals("Should create three dtu", 3, dtus.Length);
					}
				}
			}
			else
			{
				if (allCNTEmpty)
				{
					AssertEquals("Should create one DLL for all containers.", 1, loadLists.Length);
					AssertEquals("Should create 3 dtus", 3, dtus.Length);
				}
				else
				{
					if (pkgAssignedToEachCNT)
					{
						AssertEquals("Should create DLL for each container.", 3, loadLists.Length);
						AssertEquals("Should create 3 dtus for each container", 3, dtus.Length);
					}
					else
					{
						AssertEquals("Should create one DLL for container with pkg.", 1, loadLists.Length);
						AssertEquals("Should create 1 dtu for container with pkg", 1, dtus.Length);
					}
				}
			}
		}

		#endregion

		#endregion

		#region TestExportFowardingConsol_MultiRoute_ShouldCreateSingleASNForConsol

		[TestDate(2018, 1, 1)]
		public void TestExportFowardingConsol_MultiRoute_ShouldCreateSingleASNForConsol()
		{
			var testData = CreateTestData(false, "NZCHC");
			// Consol w/
			//   2 shipments
			//   2 containers
			//   route : NZCHC -> NZAKL-> AUSYD -> AUADL
			//   depot : ^
			//   Note : Phase 1, NZAKL is where the packing occurs, NZAKL & AUSYD are pass though hubs that are controlled by the airline. ie. Forwarding wont be sending UXML to NZAKL/AUSYD Transit Warehouses.
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLeg = CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container1);
			var packline12 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box, container2);
			var packline13 = CreateOuterPackline(shipment1, 3, Constants.PkgUnit.Box);
			var packline21 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Case, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn1, 3);

			var rcn2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn2, "HSB2", "S00001001", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN2 Destination", "AUADL", rcn2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2, 1);

			AssertEquals("One receive ASN must be created for the entire consol.", 1, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			var asnPK = rcn1.PackageStates.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct().Single();
			var receiveASNForShipment1PK = rcn1.PackageStates.First().WPS_WRP_ReceiveExpectedPacking;
			var receiveASNForShipment2PK = rcn2.PackageStates.Single().WPS_WRP_ReceiveExpectedPacking;
			AssertReceiveASN(newFactory.Load<WhsItemReceiveASN>(asnPK), consol.PK, "JK", "MSB1", outboundLeg);
			AssertEquals("No RTUs should be created for export", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);

			var packageStatesForShipment1 = rcn1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipment1PK).ToArray();
			var packageState1ForShipment1 = packageStatesForShipment1[0];
			var packageState2ForShipment1 = packageStatesForShipment1[1];
			var packageState3ForShipment1 = packageStatesForShipment1[2];
			AssertPackage(packageState1ForShipment1, Constants.PkgUnit.Box);
			AssertPackage(packageState2ForShipment1, Constants.PkgUnit.Box);
			AssertPackage(packageState3ForShipment1, Constants.PkgUnit.Box);

			var packageStateForShipment2 = rcn2.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipment2PK);
			AssertPackage(packageStateForShipment2, Constants.PkgUnit.Case);

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

			var dispatchConsignment1 = dispatchConsignments.First(c => c.PackageStates.Count == 3);
			AssertDispatchConsignment(packageState1ForShipment1, dispatchConsignment1, shipment1.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment1, "HSB1", "S00001000", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN1 Destination", "AUADL", dispatchConsignment1.WDC_RL_NKDestination);
			var dispatchConsignment2 = dispatchConsignments.First(c => c.PackageStates.Count == 1);
			AssertDispatchConsignment(packageStateForShipment2, dispatchConsignment2, shipment2.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment2, "HSB2", "S00001001", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN2 Destination", "AUADL", dispatchConsignment2.WDC_RL_NKDestination);
		}

		[TestDate(2018, 1, 1)]
		public void TestExportFowardingConsol_MultiRoute_ShouldCreateSingleASNForConsol_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestExportFowardingConsol_MultiRoute_ShouldCreateSingleASNForConsol();
			}
		}

		#endregion

		#region TestExportConsol_MultiRoute_WithTransportLegs

		[TestDate(2018, 1, 1)]
		public void TestExportConsol_MultiRoute_WithTransportLegs()
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var inboundLegInConsol = CreateTransport(consol, 1, "SEA", "A", "AA", "USA2N", "NZAKL", testData.today, testData.today.AddDays(2));
			var outboundLegInConsol = CreateTransport(consol, 2, "SEA", "B", "BB", "NZAKL", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLegInShipment = CreateTransport(shipment, 3, "SEA", "C", "CC", "USA2N", "NZAKL", testData.today, testData.today.AddDays(2));
			var outboundLegInShipment = CreateTransport(shipment, 4, "SEA", "D", "DD", "NZAKL", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));

			var container = CreateContainer(consol, "CONT1");

			var packline11 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			var packline13 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignments = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1"));
			var receiveConsignment = receiveConsignments.Single();
			var transportsForConsignment = new TransportCollection(receiveConsignment);
			transportsForConsignment.Load();
			AssertEquals("Precondition", 2, transportsForConsignment.Count);
			AssertTransport(inboundLegInShipment, transportsForConsignment.Cast<Transport>().Single(t => t.JW_Vessel == "C"));
			AssertTransport(outboundLegInShipment, transportsForConsignment.Cast<Transport>().Single(t => t.JW_Vessel == "D"));

			var asn = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var transportsForASN = new TransportCollection(asn);
			transportsForASN.Load();
			AssertEquals("Precondition", 2, transportsForASN.Count);
			AssertTransport(inboundLegInConsol, transportsForASN.Cast<Transport>().Single(t => t.JW_Vessel == "A"));
			AssertTransport(outboundLegInConsol, transportsForASN.Cast<Transport>().Single(t => t.JW_Vessel == "B"));

			// Dispatch consignments and loadlist
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var transportsForLoadList = new TransportCollection(loadList);
			transportsForLoadList.Load();
			AssertEquals("Precondition", 1, transportsForLoadList.Count);
			AssertTransport(outboundLegInConsol, transportsForLoadList.Cast<Transport>().Single(t => t.JW_Vessel == "B"));
		}

		[TestDate(2018, 1, 1)]
		public void TestExportConsol_MultiRoute_WithTransportLegs_UsingExtraPortToCreateTransportRouting()
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NLEUG", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var extraPort = Factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = testData.warehouse.WW_GB_RelatedCompanyBranch;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "NLEUG";

			var shipment = CreateShipment(consol, "HSB1", "NLEUG", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var inboundLegInConsol = CreateTransport(consol, 1, "SEA", "A", "AA", "USA2N", "NLEUG", testData.today, testData.today.AddDays(2));
			var outboundLegInConsol = CreateTransport(consol, 2, "SEA", "B", "BB", "NLEUG", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLegInShipment = CreateTransport(shipment, 3, "SEA", "C", "CC", "USA2N", "NZAKL", testData.today, testData.today.AddDays(2));
			var outboundLegInShipment = CreateTransport(shipment, 4, "SEA", "D", "DD", "NZAKL", "LKCMB", testData.today.AddDays(2), testData.today.AddDays(4));

			var container = CreateContainer(consol, "CONT1");

			var packline11 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			var packline13 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignments = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1"));
			var receiveConsignment = receiveConsignments.Single();
			var transportsForConsignment = new TransportCollection(receiveConsignment);
			transportsForConsignment.Load();
			AssertEquals("Precondition", 2, transportsForConsignment.Count);
			AssertTransport(inboundLegInShipment, transportsForConsignment.Cast<Transport>().Single(t => t.JW_Vessel == "C"));
			AssertTransport(outboundLegInShipment, transportsForConsignment.Cast<Transport>().Single(t => t.JW_Vessel == "D"));

			var asn = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var transportsForASN = new TransportCollection(asn);
			transportsForASN.Load();
			AssertEquals("Precondition", 2, transportsForASN.Count);
			AssertTransport(inboundLegInConsol, transportsForASN.Cast<Transport>().Single(t => t.JW_Vessel == "A"));
			AssertTransport(outboundLegInConsol, transportsForASN.Cast<Transport>().Single(t => t.JW_Vessel == "B"));

			// Dispatch consignments and loadlist
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var transportsForLoadList = new TransportCollection(loadList);
			transportsForLoadList.Load();
			AssertEquals("Precondition", 1, transportsForLoadList.Count);
			AssertTransport(outboundLegInConsol, transportsForLoadList.Cast<Transport>().Single(t => t.JW_Vessel == "B"));

			var transportRouting = newBizOFactoryAfterRunningLogWalker.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, loadList.PK)).Single();
			AssertEquals("Home Port", "NLEUG", transportRouting.JW_RL_NKLoadPort);
			AssertEquals("Disc Port", "LKCMB", transportRouting.JW_RL_NKDiscPort);
		}

		#endregion

		#region Shipments

		#region TestExportForwardingShipment_ShouldNotCreateASN

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestExportForwardingShipment_WhenImport_ShouldNotCreateASN()
			=> TestExportForwardingShipment_ShouldNotCreateASN(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestExportForwardingShipment_WhenExport_ShouldNotCreateASN()
			=> TestExportForwardingShipment_ShouldNotCreateASN(isArrivalTransitWarehouse: false);

		void TestExportForwardingShipment_ShouldNotCreateASN(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee, totalCountPackType: "BOX", totalPackageCount: 10);
			shipment.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			var outboundLegInShipment = CreateTransport(shipment, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", today, today.AddDays(2));

			var packline11 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			var packline12 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			// Receive consignments and wave
			var rcn = AssertConsignment(Factory, "HSB1", today.AddDays(-1), today, "AUSYD", "STD", warehouse, shipment.PK, outboundLegInShipment);
			AssertConsignmentAdditionalRefs(rcn, "HSB1", "S00001000", ZString.Empty, ZString.Empty, "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUSYD", rcn.WRC_RL_NKDestination);

			AssertConsignmentPackages(rcn, 2);

			AssertEquals("No ASNs should be created for shipments", 0, Factory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals("No RTUs should be created for shipments", 0, Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
			AssertEquals("No pivots should be created for shipments", 0, Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
		}

		#endregion

		#region TestExportFowardingShipment_WhenExportShipmentInConsol_ShouldNotCreateASN

		[TestDate(2018, 1, 1)]
		public void TestExportFowardingShipment_WhenExportShipmentInConsol_ShouldNotCreateASN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false, parentProcessIsConsol: false);

			// Consol w/
			//   2 shipments
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment1.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			shipment2.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var outboundLegInConsol = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", today, today.AddDays(2));

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2);
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Case, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn = AssertConsignment(newFactory, "HSB1", today.AddDays(-1), today, "AUSYD", "STD", warehouse, shipment1.PK, outboundLegInConsol);
			AssertConsignmentAdditionalRefs(rcn, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUSYD", rcn.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn, 3);

			AssertEquals("No ASNs should be created for shipments", 0, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals("No RTUs should be created for shipments", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
			AssertEquals("No pivots should be created for shipments", 0, newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
		}

		[TestDate(2018, 1, 1)]
		public void TestExportFowardingShipment_WhenExportShipmentInConsol_ShouldNotCreateASN_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestExportFowardingShipment_WhenExportShipmentInConsol_ShouldNotCreateASN();
			}
		}

		#endregion

		#region TestExportConsol_WithContainer_HasArrivalTransport

		public void TestExportConsol_WithContainer_HasArrivalTransport()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var arrivalTransportCompany = TestHelper.CreateOrganisation("ATC");
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportCompany.MainAddress.PK;

			var shipment = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1" });

			var containerDTU = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();

			AssertEquals(containerDTU.ContainerNumber, "CONT1");
			AssertDispatchDTUTransportCompany(newBizOFactoryAfterRunningLogWalker, containerDTU, arrivalTransportCompany.MainAddress);
		}

		#endregion

		#region TestExportConsol_WithContainer_HasNoArrivalTransport

		public void TestExportConsol_WithContainer_HasNoArrivalTransport()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			Factory.Save();

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1" });

			var containerDTU = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();

			AssertEquals(containerDTU.ContainerNumber, "CONT1");
			AssertDispatchDTUTransportCompany(newBizOFactoryAfterRunningLogWalker, containerDTU, null);
		}

		#endregion

		#region TestForwarder_Sends_ReceiveInstructions

		[TestDate(2018, 1, 1)]
		public void TestForwarder_Sends_ReceiveInstructionsWithout_Housebill_And_Later_Send_DispatchInstructionsWithHouseBill()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipmentWithNoHSB = CreateShipment("", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipmentWithNoHSB.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var outboundLegInShipment = CreateTransport(shipmentWithNoHSB, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			CreateOuterPackline(shipmentWithNoHSB, 3, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipmentWithNoHSB);

			// Receive consignment
			var rcn = AssertConsignment(Factory, shipmentWithNoHSB.JobNumber, testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipmentWithNoHSB.PK, outboundLegInShipment);
			AssertConsignmentAdditionalRefs(rcn, "", "S00001000", ZString.Empty, ZString.Empty, "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUSYD", rcn.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn, 1);

			var packageStateForPKG3 = rcn.PackageStates.Single();
			AssertEquals(Constants.PkgUnit.Pallet, packageStateForPKG3.Package.KP_F3_NKPackType);
			AssertEquals(3, packageStateForPKG3.Package.KP_PackageQty);

			// Receive 3 pallets to CFS and assign package ids
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var packageStateForPKG1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu);
			var packageStateForPKG2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu);
			packageStateForPKG3.WPS_WRH_TransitReceiveHeader = rtu.PK;
			var pkg3 = PackingHelper.CreatePackage(rcn.PackageJob, 1, Constants.PkgUnit.Pallet, "PKG3");
			packageStateForPKG3.WPS_KP_Package = pkg3.PK;
			packageStateForPKG3.WPS_Status = AttachablePackageStateStatuses.Codes.ARV;
			packageStateForPKG3.WPS_WL_LastLocation = testData.warehouse.DefaultLocation.PK;

			shipmentWithNoHSB.JS_HouseBill = "HSB1"; // Shipment now has housebill
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipmentWithNoHSB);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(packageStateForPKG1, dispatchConsignment, shipmentWithNoHSB.PK);
			AssertDispatchConsignment(packageStateForPKG2, dispatchConsignment, shipmentWithNoHSB.PK);
			AssertDispatchConsignment(packageStateForPKG3, dispatchConsignment, shipmentWithNoHSB.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment, "HSB1", "S00001000", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN Destination", "AUSYD", dispatchConsignment.WDC_RL_NKDestination);
		}

		public void TestTWReceivesConsignment_ForwarderSendsUXMLMultipleTimes_UNDGsPopulated()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG1");
			var undgDataItem1 = Helper.CreateUNDGDataItem(packline.PK, packline.TablePrefix, undgSubstance, 2, 3);
			packline.UNDGs.Add(undgDataItem1);
			var undgDataItem2 = Helper.CreateUNDGDataItem(packline.PK, packline.TablePrefix, undgSubstance, 2, 3);
			packline.UNDGs.Add(undgDataItem2);
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcns = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1"));
			AssertEquals(1, rcns.Length);

			var receiveConsignment = rcns.Single();
			var packageStates = receiveConsignment.PackageStates.ToArray();
			AssertEquals(1, packageStates.Length);
			AssertEquals(2, packageStates[0].Package.UNDGs.Count);
		}

		public void TestForwarder_Sends_ReceiveInstructionsContainRadioactivePackages()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance1.DG_ExceptedQuantityCode = "E1";

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DG = undgSubstance1.PK;
			undgDataItem1.Substance.DG_Class = "1";

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG1");

			packline.UNDGs.Add(undgDataItem1);

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "", "1", totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var rcns = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(0, rcns.Length);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have an unsuccessful export.", EDIMessageStatusList.Codes.Discarded, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have the combined error messages at the expected positions.",
				@"Error - The packages on this Receive/Dispatch Instruction could not be created as the UNDG Class threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Class 1 goods.", importNote?.ST_NoteText);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_OuterPackCountMatches()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn.PK;

			var rcnWithMatchingHouseBillNumber = Helper.CreateReceiveConsignment("HSB1", testData.warehouse.PK, consignor: testData.consignor, consignee: testData.consignee);
			var bookedCartons = Helper.CreatePackageState(rcnWithMatchingHouseBillNumber, 2, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked);

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3 });
			// Attach packages to Shipment
			AssertEquals("Precondition", shipment.JobNumber, pkg1.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg2.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg3.ParentJobNumber);

			Factory.Save();
			var outterPackLineOnShipment = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			AssertEquals(Constants.PkgUnit.Pallet, outterPackLineOnShipment.JL_F3_NKPackType);
			AssertEquals(3, outterPackLineOnShipment.JL_PackageCount);
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Carton);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			pkg1.Reload();
			pkg2.Reload();
			pkg3.Reload();
			AssertDispatchConsignment(pkg1, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg2, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg3, dispatchConsignment, shipment.PK);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_OuterPackCountMatches_SingleReceiveConsignment()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Carton, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var rcn1 = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn1.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3 });
			// Attach packages to Shipment
			AssertEquals("Precondition", shipment.JobNumber, pkg1.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg2.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg3.ParentJobNumber);
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var outterPackLineOnShipmentForCTN = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(2, outterPackLineOnShipmentForPLT.JL_PackageCount);
			AssertEquals(1, outterPackLineOnShipmentForCTN.JL_PackageCount);

			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(pkg1, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg2, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg3, dispatchConsignment, shipment.PK);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_OuterPackCountMatches_MultipleReceiveConsignments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var pkg4 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Carton, "PKG4", AttachablePackageStateStatuses.Codes.ARV);
			var pkg5 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG5", AttachablePackageStateStatuses.Codes.ARV);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", "STD", testData.warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg4.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			pkg5.WPS_WRC_TransitReceiveConsignment = rcn2.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Attach packages to Shipment
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			pkg4.ParentJobNumber = shipment.JobNumber;
			pkg5.ParentJobNumber = shipment.JobNumber;

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3, pkg4, pkg5 });
			// Attach packages to Shipment
			AssertEquals("Precondition", shipment.JobNumber, pkg1.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg2.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg3.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg4.ParentJobNumber);
			AssertEquals("Precondition", shipment.JobNumber, pkg5.ParentJobNumber);
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var outterPackLineOnShipmentForCTN = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(4, outterPackLineOnShipmentForPLT.JL_PackageCount);
			AssertEquals(1, outterPackLineOnShipmentForCTN.JL_PackageCount);

			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(pkg1, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg2, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg3, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg4, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg5, dispatchConsignment, shipment.PK);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_MatchingPackageIds_MultipleReceiveConsignments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var pkg4 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Carton, "PKG4", AttachablePackageStateStatuses.Codes.ARV);
			var pkg5 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG5", AttachablePackageStateStatuses.Codes.ARV);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", "STD", testData.warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg4.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			pkg5.WPS_WRC_TransitReceiveConsignment = rcn2.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Attach packages to Shipment
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			pkg4.ParentJobNumber = shipment.JobNumber;
			pkg5.ParentJobNumber = shipment.JobNumber;

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3, pkg4, pkg5 });

			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var outterPackLineOnShipmentForCTN = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(4, outterPackLineOnShipmentForPLT.JL_PackageCount);
			AssertEquals(1, outterPackLineOnShipmentForCTN.JL_PackageCount);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(pkg1, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg2, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg3, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg4, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(pkg5, dispatchConsignment, shipment.PK);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_MatchingPackageIds_MultipleReceiveConsignments_WithUnMathingPackageIDs()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var pkg4 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Carton, "PKG4", AttachablePackageStateStatuses.Codes.ARV);
			var pkg5 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG5", AttachablePackageStateStatuses.Codes.ARV);
			var unMatchingPackage = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG6", AttachablePackageStateStatuses.Codes.ARV);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", "STD", testData.warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg4.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			pkg5.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			unMatchingPackage.WPS_WRC_TransitReceiveConsignment = rcn2.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Attach packages to Shipment
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			pkg4.ParentJobNumber = shipment.JobNumber;
			pkg5.ParentJobNumber = shipment.JobNumber;
			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3, pkg4, pkg5 });
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var outterPackLineOnShipmentForCTN = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(4, outterPackLineOnShipmentForPLT.JL_PackageCount);
			AssertEquals(1, outterPackLineOnShipmentForCTN.JL_PackageCount);
			Factory.Save();
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Carton, reference: "UnMatchingPackageID");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignmentCount = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Length;
			AssertEquals("Since there are unmatched package ids import must be rejected and no DCNs are created.", 0, dispatchConsignmentCount);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_OuterPackCountDiffer_SingleRCN()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Attach packages to Shipment
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3 });
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(3, outterPackLineOnShipmentForPLT.JL_PackageCount);

			Factory.Save();
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet); // Add extra outer pack so pack counts does not match.

			// Send dispatch instructions
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var countOfDcnsCreated = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Length;
			AssertEquals("No DCNS must be created since outer packline count does not match attached packages.", 0, countOfDcnsCreated);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_After_AttachingArrivedPackages_OuterPackCountDiffer_MultipleRCN()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var pkg1 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV);
			var pkg2 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV);
			var pkg3 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV);
			var pkg4 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Carton, "PKG4", AttachablePackageStateStatuses.Codes.ARV);
			var pkg5 = Helper.CreatePackageState(rtu, 1, Constants.PkgUnit.Pallet, "PKG5", AttachablePackageStateStatuses.Codes.ARV);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", "STD", testData.warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", "STD", testData.warehouse.PK);
			pkg1.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg2.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg3.WPS_WRC_TransitReceiveConsignment = rcn1.PK;
			pkg4.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			pkg5.WPS_WRC_TransitReceiveConsignment = rcn2.PK;

			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			// Attach packages to Shipment
			pkg1.ParentJobNumber = shipment.JobNumber;
			pkg2.ParentJobNumber = shipment.JobNumber;
			pkg3.ParentJobNumber = shipment.JobNumber;
			pkg4.ParentJobNumber = shipment.JobNumber;

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { pkg1, pkg2, pkg3, pkg4 });
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var outterPackLineOnShipmentForCTN = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(3, outterPackLineOnShipmentForPLT.JL_PackageCount);
			AssertEquals(1, outterPackLineOnShipmentForCTN.JL_PackageCount);
			Factory.Save();
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Carton);

			// Send dispatch instructions
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var countOfDcnsCreated = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Length;
			AssertEquals("No DCNS must be created since outer packline count does not match attached packages.", 0, countOfDcnsCreated);
		}

		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_PackageInDifferentWarehouse()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var shipment = CreateShipment("", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG-1");
			Factory.Save();

			var twForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			Factory.Save();

			var stageLocation = twForDifferentBranch.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", twForDifferentBranch.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", twForDifferentBranch.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			// Send dispatch instructions
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var countOfDcnsCreated = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Length;
			AssertEquals("No DCNS must be created since the package is in different warehouse.", 0, countOfDcnsCreated);
		}

		[TestDate(2020, 04, 20)]
		public void TestTWReceivesBlindConsignment_CreateHandlingUnits_ForwarderSendsDispatchInstructions_AfterAttachingHandlingUnit()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var divotToChild1 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			var divotToChild2 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.Save();

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "HU1");

			var now = ZDateTimeOffset.Now;
			Helper.UnpackPackageFromHandlingUnit(divotToChild1, now, Env.CurrentUser.Initials);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var handlingUnitPackageInNewFactory = Factory.Load<WhsItemPackageState>(handlingUnitPackage.PK);
			var childPackage1InNewFactory = Factory.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = Factory.Load<WhsItemPackageState>(childPackage2.PK);
			AssertEquals("Precondition", now, divotToChild1.KPD_UnpackedTime);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, divotToChild2.KPD_UnpackedTime);
			AssertEquals(ZGuid.Empty, handlingUnitPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZGuid.Empty, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertDispatchConsignment(childPackage2InNewFactory, dispatchConsignment, shipment.PK);
		}

		[TestDate(2020, 06, 24)]
		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_AttachedPackagesAreLoaded()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			var warehouse = testData.warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var package2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			var dcn = Helper.CreateDispatchConsignment(shipment.JobNumber, warehouse.PK);
			dcn.ConsigneeDocAddress.E2_OA_Address = testData.consignee.MainAddress.PK;
			dcn.ConsignorDocAddress.E2_OA_Address = testData.consignor.MainAddress.PK;
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			LoadPackage(package1, stageLocation, rtu, dtu, dll, dcn);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(WhsItemDispatchConsignmentSchema.PK, SQLComparisonOperator.NotEqual, dcn.PK);
			AssertEquals("Should not create a new dispatch consignment", false, newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(query).Any());

			package1.Reload();
			AssertEquals("Receive Transport Unit should not be changed", rtu.PK, package1.WPS_WRH_TransitReceiveHeader);
			AssertEquals("Dispatch Load List should not be changed", dll.PK, package1.WPS_WDL_LoadList);
			AssertEquals("Dispatch Transport Unit should not be changed", dtu.PK, package1.WPS_WDH_TransitDispatchHeader);
		}

		[TestDate(2020, 06, 24)]
		public void TestTWReceivesBlindConsignment_ForwarderSendsDispatchInstructions_AttachedPackagesAreLoaded_ThenSendDispatchInstructionAgain()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			Factory.Save();

			var warehouse = testData.warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var package2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { package1, package2 });
			var outterPackLineOnShipmentForPLT = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(2, outterPackLineOnShipmentForPLT.JL_PackageCount);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Should create new dispatch consignment", dcn);

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			package1.Reload();
			LoadPackage(package1, stageLocation, rtu, dtu, dll);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(WhsItemDispatchConsignmentSchema.PK, SQLComparisonOperator.NotEqual, dcn.PK);
			AssertEquals("Should not create a new dispatch consignment", false, newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(query).Any());

			package1.Reload();
			AssertEquals("Receive Transport Unit should not be changed", rtu.PK, package1.WPS_WRH_TransitReceiveHeader);
			AssertEquals("Dispatch Load List should not be changed", dll.PK, package1.WPS_WDL_LoadList);
			AssertEquals("Dispatch Transport Unit should not be changed", dtu.PK, package1.WPS_WDH_TransitDispatchHeader);
		}

		[TestDate(2020, 07, 14)]
		public void TestTWReceivesConsignment_ForwarderSendsDispatchInstructions_WithReceiveConsignmentID_ConsignmentHasNoPackages()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			Factory.Save();

			var rcns = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1"));
			AssertEquals(1, rcns.Length);

			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should not create a new dispatch consignment", 0, dcn.Length);

			var rcn = rcns.Single();
			rcn.WRC_ConsignmentID = "S00001000";
			Factory.Save();

			shipment.JS_HouseBill = "";

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should not create a new dispatch consignment", 0, dcn.Length);
		}

		void LoadPackage(WhsItemPackageState packageState, WhsLocation location, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, WhsItemDispatchConsignment dcn = null)
		{
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WDH_TransitDispatchHeader = dtu.PK;
			packageState.WPS_WDL_LoadList = dll.PK;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";

			if (dcn != null)
			{
				packageState.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			}
		}

		#endregion

		#region TestForwardingShipmentBilling_OnJobCreating_LinkTransitWarehouseConsignmentJobsToShipmentJob

		public void TestForwardingShipmentBilling_OnJobCreating_LinkTransitWarehouseConsignmentJobsToShipmentJob()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var transitWarehouse = Helper.CreateTRWWarehouse();

			var receiveConsignment = Helper.CreateReceiveConsignment("RC00000001", transitWarehouse.PK);
			receiveConsignment.WRC_ParentID = shipment.PK;
			receiveConsignment.WRC_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var dispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", transitWarehouse.PK);
			dispatchConsignment.WDC_ParentID = shipment.PK;
			dispatchConsignment.WDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			var jobOnReceiveConsignment = new JobHeader.Loader(receiveConsignment).TryLoadOrCreate();
			var jobOnDispatchConsignment = new JobHeader.Loader(dispatchConsignment).TryLoadOrCreate();
			AssertEquals(true, jobOnReceiveConsignment.JH_JH_ParentJob.IsEmpty);
			AssertEquals(true, jobOnDispatchConsignment.JH_JH_ParentJob.IsEmpty);

			new JobHeader.Loader(shipment).TryLoadOrCreate();
			AssertEquals(shipment.Job.PK, jobOnReceiveConsignment.JH_JH_ParentJob);
			AssertEquals(shipment.Job.PK, jobOnDispatchConsignment.JH_JH_ParentJob);
		}

		#endregion

		#region TestASNCreation_VehicleReference_FallbackRules

		[TestDate(2018, 1, 1)]
		public void TestASNCreation_VehicleReferenceWithConsol_FallbackRules()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var asn = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault();
			AssertNotNull("One receive ASN must be created for the consol", asn);
			AssertReceiveASN(asn, consol.PK, "JK", consol.JK_UniqueConsignRef);

			consol.JK_MasterBillNum = "MSB1";
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault();
			AssertNotNull("One receive ASN must be created for the consol.", asn);
			AssertReceiveASN(asn, consol.PK, "JK", "MSB1");
		}

		[TestDate(2018, 1, 1)]
		public void TestASNCreation_VehicleReferenceWithConsol_FallbackRules_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestASNCreation_VehicleReferenceWithConsol_FallbackRules();
			}
		}

		#endregion

		#region TestImportETA_ShipmentWithoutConsol_ShouldTakeInboundETA

		public void TestImportETA_ShipmentWithoutConsol_ShouldTakeInboundETA() => TestImportETA_ShipmentWithoutConsol_ShouldTakeInboundETA(null);
		public void TestImportETA_ShipmentWithoutConsol_ExtraPorts_ShouldTakeInboundETA() => TestImportETA_ShipmentWithoutConsol_ShouldTakeInboundETA("NZCHC", "NZCHC");
		void TestImportETA_ShipmentWithoutConsol_ShouldTakeInboundETA(string extraPortCode, string inboundDischargePortCode = "NZAKL")
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			if (extraPortCode != null)
			{
				UniversalHelper.CreateExtraPort(testData.warehouse, extraPortCode);
			}

			var shipment = CreateShipment("HSB1", "AUSYD", "NZAKL", testData.today.AddDays(-3), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var shipmentInboundEta = new ZDateTime(2005, 5, 5);
			var shipmentInboundLeg = CreateTransport(shipment, 1, "SEA", "A", "AA", "AUSYD", inboundDischargePortCode, testData.today, shipmentInboundEta);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(shipmentInboundEta, rcn.WRC_ExpectedArrivalTime);
		}

		#endregion

		#region TestImportShipment_ReceiveAndDispatch

		[TestDate(2020, 04, 30)]
		public void TestImportShipment_ReceiveAndDispatch()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container1 = CreateContainer(consol, "CONT1");

			var packline11 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1);
			var packline23 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "", "C00001000", "AA", "30-Apr-20 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn1, 2);

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "30-Apr-20 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);
			var newBizOFactoryAfterResending = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertLoadList(newBizOFactoryAfterResending, "NZAKL", "A", "AA", "30-Apr-20 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
		}

		[TestDate(2020, 04, 30)]
		public void TestImportShipment_ReceiveAndDispatch_CompletedRCNAndNotCompletedDCN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false, parentProcessIsConsol: false, createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, isArrival: false);

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newFactoryAfterImport1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterImport1 = newFactoryAfterImport1.Load<WhsItemReceiveConsignment>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Precondition: Successfully imported an RCN.", rcnAfterImport1);
			rcnAfterImport1.WRC_CompleteTime = DateTime.Now;
			var dcnAfterImport1 = newFactoryAfterImport1.Load<WhsItemDispatchConsignment>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Successfully imported a DCN.", dcnAfterImport1);
			var exportLogAfterImport1 = UniversalHelper.GetMostRecentExportLog(dcnAfterImport1, AutoEvents.DataImportCode);
			var ediMessageAfterImport1 = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport1);
			AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport1?.EM_Status);
			newFactoryAfterImport1.Save();

			TriggerAndFireTransitRequestForRelease(shipment);

			var newFactoryAfterImport2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcnAfterImport2 = newFactoryAfterImport2.Load<WhsItemDispatchConsignment>(dcnAfterImport1.PK);
			var exportLogAfterImport2 = UniversalHelper.GetMostRecentExportLog(dcnAfterImport2, AutoEvents.DataImportCode, exportLogAfterImport1.PK);
			var ediMessageAfterImport2 = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport2);
			AssertNotEquals("The shipment should have a second successful import", ediMessageAfterImport1.PK, ediMessageAfterImport2?.PK);
			AssertEquals("The shipment should have a second successful import.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport2.EM_Status);
		}

		#endregion

		#endregion

		#region TestImportingFowardingConsol_ExternalReference

		[TestDate(2020, 1, 1)]
		public void TestImportingFowardingConsol_ExternalReference()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignment and ASN
			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			var asn = receiveConsignment.PackageStates.Select(p => p.ReceiveASN).Distinct().Single();
			AssertReceiveASN(asn, consol.PK, "JK", consol.JK_UniqueConsignRef);
			AssertEquals("TRT00000001", asn.WRP_ReferenceNumber);
		}

		#endregion

		#region TestImportingFowardingConsol_ReceiveAndDispatch

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_ReceiveAndDispatch_CompletedJobs()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: true, parentProcessIsConsol: true, createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: true);

			var consol = CreateConsol("MB1", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterImport1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterImport1 = newFactoryAfterImport1.Load<WhsItemReceiveConsignment>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Precondition: Successfully imported an RCN.", rcnAfterImport1);
			rcnAfterImport1.WRC_CompleteTime = DateTime.Now;
			var asnAfterImport1 = newFactoryAfterImport1.Load<WhsItemReceiveASN>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Precondition: Successfully imported an ASN.", asnAfterImport1);
			asnAfterImport1.WRP_CompleteTime = DateTime.Now;

			var dcnAfterImport1 = newFactoryAfterImport1.Load<WhsItemDispatchConsignment>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Successfully imported a DCN.", dcnAfterImport1);
			var exportLogAfterImport1 = UniversalHelper.GetMostRecentExportLog(dcnAfterImport1, AutoEvents.DataImportCode);
			var ediMessageAfterImport1 = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport1);
			AssertEquals("The shipment should have a successful import.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport1?.EM_Status);
			dcnAfterImport1.WDC_CompleteTime = DateTime.Now;
			var loadlistAfterImport1 = newFactoryAfterImport1.Load<WhsItemDispatchLoadList>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Precondition: Successfully imported a Load List.", loadlistAfterImport1);
			loadlistAfterImport1.WDL_CompleteTime = DateTime.Now;
			newFactoryAfterImport1.Save();
			TriggerAndFireTransitRequestForRelease(consol);

			var newFactoryAfterImport2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcnAfterImport2 = newFactoryAfterImport2.Load<WhsItemDispatchConsignment>(new ZQuery()).FirstOrDefault();
			var exportLogAfterImport2 = UniversalHelper.GetMostRecentExportLog(dcnAfterImport2, AutoEvents.DataImportCode, exportLogAfterImport1.PK);
			var ediMessageAfterImport2 = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport2);
			AssertNotEquals("The shipment should have a second successful import", ediMessageAfterImport1.PK, ediMessageAfterImport2?.PK);
			AssertEquals("The shipment should a second successful import.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport2.EM_Status);
		}

		[TestDate(2022, 01, 30)]
		public void TestImportConsol_ReceiveAndDispatch_BillToParty_ArrivalWarehouse()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: true, parentProcessIsConsol: true, createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: true);

			var consol = CreateConsol("MB1", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			consol.ReceivingForwarderWithContact.AddressFK = consignor.MainAddress.PK;

			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterImport1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtu = newFactoryAfterImport1.LoadTop1<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertNotNull("RTU should be created", rtu);
			AssertEquals("BillToParty of RTU should be same as consol's Receiving Agent", consignor.MainAddress.PK, rtu.ClientRequestedBillToPartyDocAddress.E2_OA_Address);

			var dtu = newFactoryAfterImport1.LoadTop1<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertNotNull("DTU should be created", dtu);
			AssertEquals("BillToParty of DTU should be same as consol's Sending Agent", consignor.MainAddress.PK, dtu.ClientRequestedBillToPartyDocAddress.E2_OA_Address);
		}

		[TestDate(2022, 01, 30)]
		public void TestImportConsol_ReceiveAndDispatch_BillToParty_DepartureWarehouse()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false, parentProcessIsConsol: true, createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: false);

			var consol = CreateConsol("MB1", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.SendingForwarderWithContact.AddressFK = consignor.MainAddress.PK;

			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterImport1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtu = newFactoryAfterImport1.LoadTop1<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertNull("RTU should not be created in departure warehouse", rtu);

			var dtu = newFactoryAfterImport1.LoadTop1<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertNotNull("DTU should be created", dtu);
			AssertEquals("BillToParty of DTU should be same as consol's Sending Agent", consignor.MainAddress.PK, dtu.ClientRequestedBillToPartyDocAddress.E2_OA_Address);
		}

		#endregion

		#region TestSendingUXMLFromConsol_DoesNotTryToSendUXML_To_Shipments

		[TestDate(2020, 1, 1)]
		public void TestSendingUXMLFromConsol_DoesNotTryToSendUXML_From_Shipment()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment1.JS_OA_ImportReleaseDepot = Helper.CreateClient("DIF").MainAddress.PK;
			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box);
			Factory.Save();

			var notifications = new NotificationsForTest();
			var dataExport = new ManualDataExport(Factory, consol, UniversalDataType.UniversalShipment);
			dataExport.RecipientType = "ATW";
			dataExport.RecipientService = "TWR";
			using (Factory.AddDisposableService())
			{
				dataExport.SendData(notifications);

				AssertMultilineASCIIEquals("dataExport.SendData() notifications", @"
Processing Consol C00001000
Universal Shipment sent internally for Organization [EDICUSCHC].".Trim(), notifications.Notifications);

				// Receive consignment and ASN
				var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
				var asn = receiveConsignment.PackageStates.Select(p => p.ReceiveASN).Distinct().Single();
				AssertReceiveASN(asn, consol.PK, "JK", consol.JK_UniqueConsignRef);
			}
		}

		#endregion

		#region TestReSendingUXMLFromConsolAndShipment_InitHandlerManager

		public void TestReSendingUXMLFromConsolAndShipment_InitHandlerManager()
		{
			var testData = CreateTestData(true);

			var handlerManager = ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();

			var consol = CreateConsol("MSB1", null, "NZAKL", "AUBNE");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUBNE", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.JS_UniqueConsignRef = "HSB1";

			AssertNull(handlerManager.GetAffectedASNPKs());

			consol.Shipments.Add(shipment1);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNull(handlerManager.GetAffectedASNPKs());
			handlerManager.AddAffectedASNPK(ZGuid.NewZGuid());

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			AssertEquals("AffectedASNPK shoule be a new collection", 0, handlerManager.GetAffectedASNPKs().Count);
		}

		#endregion

		#region TestSendingUXMLFromConsol_CreateSingleASN

		public void TestSendingUXMLFromConsol_CreateSingleASN_Enabled() => TestSendingUXMLFromConsol_CreateSingleASN(true);

		public void TestSendingUXMLFromConsol_CreateSingleASN_Disabled() => TestSendingUXMLFromConsol_CreateSingleASN(false);

		void TestSendingUXMLFromConsol_CreateSingleASN(bool isRegistryEnabled)
		{
			var testData = CreateTestData(true);

			var consol = CreateConsol("MSB1", null, "NZAKL", "AUBNE");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUBNE", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_UniqueConsignRef = "HSB1";
			var container1 = CreateContainer(consol, "CNT1");
			var container2 = CreateContainer(consol, "CNT2");
			CreateContainer(consol, "CNT3");

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1, reference: "P1", packLineId: "EDIDAT00000001");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container2, reference: "P2", packLineId: "EDIDAT00000002");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container: null, reference: "P3", packLineId: "EDIDAT00000003");

			using (WarehouseDataRegistry.Instance.CreateSingleASNForAllContainers.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, isRegistryEnabled))
			{
				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var newFactory = new BusinessObjectFactory();
				var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
				if (isRegistryEnabled)
				{
					AssertEquals("Should create a single ASN", 1, asns.Length);
					var asn = asns.Single();
					AssertEquals("ASN should be created from consol", "MSB1", asn.WRP_VehicleReference);
				}
				else
				{
					AssertEquals("Should create 4 ASNs", 4, asns.Length);
					var asn1 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT1");
					AssertNotNull("Should create ASN for container 1", asn1);

					var asn2 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT2");
					AssertNotNull("Should create ASN for container 2", asn2);

					var asn3 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT3");
					AssertNotNull("Should create ASN for container 3", asn3);

					var asn4 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "MSB1");
					AssertNotNull("Should create ASN for consol", asn4);
				}
			}
		}

		#endregion

		#region TestShipmentWithConsol_HasDCNInPickupCFS_SendReceiveInstructions_ShouldCreateRTUs

		// Integration test for WI00414081 - TW0 - Fix UXML when importing consols for Arrival TW after Departure TW
		public void TestShipmentWithConsol_HasDCNInPickupCFS_SendReceiveInstructions_ShouldCreateRTUs()
		{
			var testData = CreateTestDataToMatchRCN(CFSType.ArrivalCFSOnConsol, departureCFSPortCode: "NZAKL", arrivalCFSPortCode: "AUBNE");

			var now = ZDateTime.Today;
			var consol = CreateConsol("MSB1", null, "NZAKL", "AUBNE");
			consol.JK_OA_PackDepotAddress = testData.departureCFS.WarehouseAddress.PK;
			consol.JK_OA_UnpackDepotAddress = testData.arrivalCFS.WarehouseAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUBNE", now.AddDays(-1), now.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_UniqueConsignRef = "HSB1";

			var container = CreateContainer(consol, "CONT1");

			var packingLine1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			var packingLine2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			CreateRCNAndDCNInCFS(testData.departureCFS, shipment);

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			// Receive consignments
			var rcnInArrivalCFS = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.arrivalCFS, shipment.PK);
			AssertConsignmentPackages(rcnInArrivalCFS, 2);

			var packageStates = rcnInArrivalCFS.PackageStates.ToArray();

			AssertEquals("Two package states should have been created.", 2, packageStates.Length);

			// ASNs and containers
			var receiveASNForContainer = rcnInArrivalCFS.PackageStates.Select(p => p.ReceiveASN).Distinct().SingleOrDefault();
			AssertEquals("One ASN must be created for the container.", "CONT1", receiveASNForContainer?.WRP_VehicleReference);

			AssertReceiveTransportationUnits("An RTU should be created for the container", newFactory, container);
			var pivots = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("RTU should be linked to its ASN", new[] { receiveASNForContainer.PK }, pivots.Select(p => p.WAR_WRP_TransitReceiveASN));
		}

		void CreateRCNAndDCNInCFS(WhsWarehouse departureCFS, ForwardingShipment shipment)
		{
			var rcnInDepartureCFS = Helper.CreateReceiveConsignment(shipment.JS_UniqueConsignRef, "STD", departureCFS.PK, "RCN1");
			rcnInDepartureCFS.WRC_ParentID = shipment.PK;
			rcnInDepartureCFS.WRC_ParentTableCode = shipment.TablePrefix;

			var dcnInDepartureCFS = Helper.CreateDispatchConsignment("HSB22332", departureCFS.PK, "STD", "DCN1");
			dcnInDepartureCFS.WDC_ParentID = shipment.PK;
			dcnInDepartureCFS.WDC_ParentTableCode = shipment.TablePrefix;

			Helper.CreatePackageState(rcnInDepartureCFS, 1, Constants.PkgUnit.Pallet, $"DEP{departureCFS.WW_WarehouseCode}PLT1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnInDepartureCFS);
			Helper.CreatePackageState(rcnInDepartureCFS, 1, Constants.PkgUnit.Pallet, $"DEP{departureCFS.WW_WarehouseCode}PLT2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnInDepartureCFS);

			Factory.Save();
		}

		#endregion

		#region TestImportingShipmentWithPackageIDs_ShouldReplaceExistingBookedPacklinesOnMatchingRCN

		public void TestImportingShipmentWithPackageIDs_ShouldReplaceExistingBookedPacklinesOnMatchingRCN()
		{
			var testData = CreateTestDataToMatchRCN(CFSType.ArrivalCFSOnConsol, departureCFSPortCode: "NZAKL", arrivalCFSPortCode: "AUBNE");

			var now = ZDateTime.Today;
			var consol = CreateConsol("MSB1", null, "NZAKL", "AUBNE");
			consol.JK_OA_PackDepotAddress = testData.departureCFS.WarehouseAddress.PK;
			consol.JK_OA_UnpackDepotAddress = testData.arrivalCFS.WarehouseAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUBNE", now.AddDays(-1), now.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_UniqueConsignRef = "HSB1";

			var container = CreateContainer(consol, "CONT1");

			var packingLine1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container, reference: "P1", packLineId: "EDIDAT00000001");
			var packingLine2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container, reference: "P2", packLineId: "EDIDAT00000002");

			var rcnInArrivalCFS = Helper.CreateReceiveConsignment("HSB1", "STD", testData.arrivalCFS.PK, "ArrivalRCN", shipment.PK, shipment.TablePrefix);
			Helper.CreateJobDocAddressFromAddress(rcnInArrivalCFS, "LCE", testData.consignor.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rcnInArrivalCFS, "LCI", testData.consignee.MainAddress);

			var originalPackline = Helper.CreatePackageState(rcnInArrivalCFS, 2, Constants.PkgUnit.Box, null, TransitWarehouseStatuses.Codes.Booked, externalReference: "EDIDAT00000001");

			Factory.Save();

			var originalPacklinePackageStatePK = originalPackline.PK;
			var originalPacklinePackagePK = originalPackline.WPS_KP_Package;

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFacotory = new BusinessObjectFactory();
			var rcnForArrivalWarehouse = AssertAndReturnReceiveConsignment(newFacotory, "HSB1", testData.arrivalCFS, shipment.PK);
			AssertConsignmentPackages(rcnForArrivalWarehouse, 2);
			AssertRCNPackageStates("Packages with IDs have been imported.", rcnForArrivalWarehouse, 1, TransitWarehouseStatuses.Codes.Booked, Constants.PkgUnit.Box, 1, "EDIDAT00000001", "P1");
			AssertRCNPackageStates("Packages with IDs have been imported.", rcnForArrivalWarehouse, 1, TransitWarehouseStatuses.Codes.Booked, Constants.PkgUnit.Box, 1, "EDIDAT00000002", "P2");
			AssertEquals("Original Packline PkgPackage has been deleted.", 0, newFacotory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, originalPacklinePackagePK)).Length);
			AssertEquals("Original Packline PkgPackageBookedDetail has been deleted.", 0, newFacotory.Load<PkgPackageBookedDetail>(new ZQuery(PkgPackageBookedDetailSchema.KPB_KP_Package, originalPacklinePackagePK)).Length);
			AssertEquals("Original Packline WhsItemPackageState has been deleted.", 0, newFacotory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, originalPacklinePackageStatePK)).Length);
		}

		void AssertRCNPackageStates(string message, WhsItemReceiveConsignment consignment, int expected, string status, string packType, int packageQty, string externalReference, string packageID = null)
		{
			AssertEquals(message, expected,
				consignment.PackageStates.Count(p =>
					p.WPS_Status == status &&
					p.Package.KP_F3_NKPackType == packType &&
					p.Package.KP_PackageQty == packageQty &&
					(packageID != null ? p.Package.GetPackageHeader().KPH_PackageID == packageID : String.IsNullOrEmpty(packageID)) &&
					p.Package.KP_ExternalReference == externalReference));
		}

		#endregion

		#region TestForwarderSends_ReceiveInstructions_RTUAttachedToASN_ForwarderResendsReceiveInstructions

		[ExpectNoExceptions]
		public void TestForwarderSends_ReceiveInstructions_RTUAttachedToASN_ForwarderResendsConsol()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1");
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "Pack1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", warehouse, shipment.PK);
			var asn = rcn.PackageStates.SingleOrDefault()?.ReceiveASN;
			AssertNotNull("Precondition: An ASN should be created for the container.", asn);
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: An RTU should be created for the ASN.", rtu);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Precondition: There should be a single Pivot.", 1, pivots.Length);
			AssertNotNull("Precondition: The RTU should be linked to the ASN.", pivots.SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK && p.WAR_WRP_TransitReceiveASN == asn.PK));

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn2 = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			AssertEquals("The consignment should be matched.", rcn.PK, rcn2.PK);
			var asnsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("There should be a single ASN.", 1, asnsReimport.Length);
			AssertEquals("The ASN should be matched.", asn.PK, asnsReimport[0].PK);
			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("There should be a single RTU.", 1, rtusReimport.Length);
			AssertEquals("The RTU should be matched.", rtu.PK, rtusReimport[0].PK);
			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("There should be a single Pivot.", 1, pivotsReimport.Length);
			AssertNotNull("The RTU and ASN should be linked.", pivotsReimport.SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtusReimport[0].PK && p.WAR_WRP_TransitReceiveASN == asnsReimport[0].PK));
		}

		public void TestForwarderSends_ReceiveInstructions_RTUAttachedToASN_ForwarderChangesConsolNumber() => AssertMatchFailed(changeConsolNumber: true);

		public void TestForwarderSends_ReceiveInstructions_RTUAttachedToASN_ForwarderChangesMasterBill() => AssertMatchFailed(changeMasterBill: true);

		public void TestForwarderSends_ReceiveInstructions_RTUAttachedToASN_ForwarderChangesContainerNumber() => AssertMatchFailed(changeContainerNumber: true);

		void AssertMatchFailed(bool changeConsolNumber = false, bool changeMasterBill = false, bool changeContainerNumber = false)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1");
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "Pack1");

			// Send first import
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", warehouse, shipment.PK);
			var asn = rcn.PackageStates.SingleOrDefault()?.ReceiveASN;
			AssertNotNull("Precondition: An ASN should be created for the container.", asn);
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: An RTU should be created for the ASN.", rtu);
			var pivot = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK && p.WAR_WRP_TransitReceiveASN == asn.PK);
			AssertNotNull("Precondition: The RTU should be linked to the ASN.", pivot);

			// Alter the consol
			if (changeConsolNumber)
			{
				consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
				consol.JK_MasterBillNum = "001";
				consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
				container.JC_JK = consol.PK;
				consol.Shipments.Add(shipment);
			}
			else if (changeMasterBill)
			{
				consol.JK_MasterBillNum = "CHANGED";
			}
			else if (changeContainerNumber)
			{
				container.JC_ContainerNum = "CHANGED";
			}

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
		}

		#endregion

		#region TestForwarderSends_ReceiveInstructions_TWUnloadsSomePackages_ForwarderResends_ReceiveInstructions_WithoutError

		[ExpectNoExceptions]
		public void TestForwarderSends_ReceiveInstructions_TWUnloadsSomePackages_ForwarderResends_ReceiveInstructions_WithoutError()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1");
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 3, Constants.PkgUnit.Pallet, container, reference: "Pack1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = AssertAndReturnReceiveConsignment(Factory, "HSB1", warehouse, shipment.PK);
			var packageState1 = rcn.PackageStates.FirstOrDefault();
			var asn = packageState1.ReceiveASN;
			AssertNotNull("Precondition: An ASN should be created for the container.", asn);
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: An RTU should be created for the ASN.", rtu);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Precondition: There should be a single Pivot.", 1, pivots.Length);
			AssertNotNull("Precondition: The RTU should be linked to the ASN.", pivots.SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK && p.WAR_WRP_TransitReceiveASN == asn.PK));
			AssertEquals("Precondition: Shipment Consignor should match with RCN Consignor.", consignor.PK, rcn.ConsignorDocAddress.OrganisationPK);
			AssertEquals("Precondition: Shipment Consignee should match with RCN Consignee.", consignee.PK, rcn.ConsigneeDocAddress.OrganisationPK);

			UnloadAndLabelPackage(packageState1, rtu, "BOX1", setDetails: true);

			var consignor2 = TestHelper.CreateOrganisation("CR2");
			var consignee2 = TestHelper.CreateOrganisation("CE2");
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee2.MainAddress.PK;

			rcn.Reload();
			AssertEquals("Precondition: There should be 2 package states after Unloading a package.", 2, rcn.PackageStates.Count);

			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Bag, container, reference: "Pack2");
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn2 = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			AssertEquals("The consignment should be matched.", rcn.PK, rcn2.PK);
			var asnsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("There should be a single ASN.", 1, asnsReimport.Length);
			AssertEquals("The ASN should be matched.", asn.PK, asnsReimport[0].PK);
			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("There should be a single RTU.", 1, rtusReimport.Length);
			AssertEquals("The RTU should be matched.", rtu.PK, rtusReimport[0].PK);
			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("There should be a single Pivot.", 1, pivotsReimport.Length);
			AssertNotNull("The RTU and ASN should be linked.", pivotsReimport.SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtusReimport[0].PK && p.WAR_WRP_TransitReceiveASN == asnsReimport[0].PK));
			AssertEquals("There should still be 2 package states, package data should not be updated.", 2, rcn2.PackageStates.Count);
			AssertEquals("RCN Consignor should be updated with Shipment Consignor.", consignor2.PK, rcn2.ConsignorDocAddress.OrganisationPK);
			AssertEquals("RCN Consignee should be updated with Shipment Consignee.", consignee2.PK, rcn2.ConsigneeDocAddress.OrganisationPK);

			var consolAfterReImport = newFactory.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterReImport = UniversalHelper.GetMostRecentExportLog(consolAfterReImport, AutoEvents.DataExportCode);
			var ediMessageAfterReImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterReImport);
			AssertEquals("The shipment should have a warning while export.", EDIMessageStatusList.Codes.Warning, ediMessageAfterReImport?.EM_Status);
			var importNote = ediMessageAfterReImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have a warning.",
@"Receive Consignment has been partially received. Package level data will be ignored and not read in.", importNote?.ST_NoteText);
		}

		#endregion

		#region TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreatesASN

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenImport_AttachesAndSendsNewShipmentFromConsol_CreatesASN()
			=> TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreatesASN(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenExport_AttachesAndSendsNewShipmentFromConsol_CreatesASN()
			=> TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreatesASN(isArrivalTransitWarehouse: false);

		void TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreatesASN(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: false);

			// 2 Shipments
			//   route : NZAKL -> AUSYD
			//   depot : ^

			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment1.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment1.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;
			var shipment2 = CreateShipment("HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment2.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment2);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment1.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory, "HSB1", "S00001000", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN1 Destination", "AUSYD", rcnInNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory, 1);

			var rcn2InNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB2", warehouse, shipment2.PK);
			AssertConsignmentAdditionalRefs(rcn2InNewFactory, "HSB2", "S00001001", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN2 Destination", "AUSYD", rcn2InNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2InNewFactory, 2);

			AssertEquals("No ASNs should be created for shipments", 0, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals("No RTUs should be created for shipments", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
			AssertEquals("No pivots should be created for shipments", 0, newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			// The packlines are added to the consol's first container
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			var shipment3 = CreateShipment(consol, "HSB3", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);

			CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package, container);
			CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package, container);
			CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package, container2);
			CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

			if (isArrivalTransitWarehouse)
			{
				CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			}
			else
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			}

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcnInNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB1", warehouse, shipment1.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory2, "HSB1", "S00001000", isArrivalTransitWarehouse ? "MSB1" : "", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN1 Destination", "AUSYD", rcnInNewFactory2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory2, 1);

			var rcn2InNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB2", warehouse, shipment2.PK);
			AssertConsignmentAdditionalRefs(rcn2InNewFactory2, "HSB2", "S00001001", isArrivalTransitWarehouse ? "MSB1" : "", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN2 Destination", "AUSYD", rcn2InNewFactory2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2InNewFactory2, 2);

			var rcn3InNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB3", warehouse, shipment3.PK);
			AssertConsignmentAdditionalRefs(rcn3InNewFactory2, "HSB3", "S00001002", isArrivalTransitWarehouse ? "MSB1" : "", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN3 Destination", "AUSYD", rcn3InNewFactory2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn3InNewFactory2, 4);

			var asns = newFactory2.Load<WhsItemReceiveASN>(new ZQuery());
			if (isArrivalTransitWarehouse)
			{
				AssertEquals("An ASN should be created for the consol and each container", 3, asns.Length);
				AssertEquals("An RTU should be created for each container", 2, newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("A pivot should be created for each container", 2, newFactory2.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);

				AssertEquals(5, asns.Single(a => a.WRP_VehicleReference == "CONT1").PackageStates.Count);
				AssertEquals(1, asns.Single(a => a.WRP_VehicleReference == "CONT2").PackageStates.Count);
				AssertEquals(1, asns.Single(a => a.WRP_VehicleReference == "MSB1").PackageStates.Count);
			}
			else
			{
				AssertEquals("An ASN should be created for the consol", 1, asns.Length);
				AssertEquals("No RTU should be created for export", 0, newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("No pivot should be created for export", 0, newFactory2.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);

				AssertEquals(7, asns.Single().PackageStates.Count);
			}
		}

		#endregion

		#region TestForwardingSendsConsol_AttachesAndSendsFromNewShipment_DoesNotCreateASN

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_WhenImport_AttachesAndSendsFromNewShipment_DoesNotCreateASN()
			=> TestForwardingSendsConsol_AttachesAndSendsFromNewShipment_DoesNotCreateASN(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_WhenExport_AttachesAndSendsFromNewShipment_DoesNotCreateASN()
			=> TestForwardingSendsConsol_AttachesAndSendsFromNewShipment_DoesNotCreateASN(isArrivalTransitWarehouse: false);

		void TestForwardingSendsConsol_AttachesAndSendsFromNewShipment_DoesNotCreateASN(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse);

			// Consol w/
			//   1 shipments
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory, "HSB1", "S00001000", isArrivalTransitWarehouse ? "MSB1" : "", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN Destination", "AUSYD", rcnInNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory, 2);

			var asnsInNewFactory = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			var packageStatesOnASNs = asnsInNewFactory.SelectMany(asn => asn.PackageStates).ToArray();
			if (isArrivalTransitWarehouse)
			{
				AssertEquals("An ASN should be created for the Consol and Container", 2, asnsInNewFactory.Length);
				AssertEquals("An RTUs should be created for the Container", 1, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("The ASN and RTU should be linked by a pivot", 1, newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
			}
			else
			{
				AssertEquals("A single ASN should be created for consol", 1, asnsInNewFactory.Length);
				AssertEquals("No RTUs should be created for export", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("No pivots should be created for export", 0, newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
			}

			if (isArrivalTransitWarehouse)
			{
				CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);
			}
			else
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);
			}

			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment2.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			var container2 = CreateContainer(consol, "CONT2");

			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package, container);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package, container2);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment2);

			// Receive consignments and wave
			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn2InNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB2", warehouse, shipment2.PK);
			AssertConsignmentAdditionalRefs(rcn2InNewFactory2, "HSB2", "S00001001", isArrivalTransitWarehouse ? "MSB1" : "", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN2 Destination", "AUSYD", rcn2InNewFactory2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2InNewFactory2, 3);

			var asnsInNewFactory2 = newFactory2.Load<WhsItemReceiveASN>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(asnsInNewFactory.Select(asn => asn.PK), asnsInNewFactory2.Select(asn => asn.PK));
			AssertContainsExactElementsInAnyOrder(packageStatesOnASNs.Select(p => p.PK), asnsInNewFactory2.SelectMany(asn => asn.PackageStates.Select(p => p.PK)));

			if (isArrivalTransitWarehouse)
			{
				AssertEquals("No new RTU should be created", 1, newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("No new pivot should be created", 1, newFactory2.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
			}
			else
			{
				AssertEquals("No new RTU should be created", 0, newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals("No new pivot should be created", 0, newFactory2.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);
			}
		}

		#endregion

		#region TestForwardingSendsConsol_UnloadsPackages_AddsPackagesAndSendsFromAnotherShipment_CreatesNewConsignment

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_UnloadsPackages_AddsPackagesAndSendsFromAnotherShipment_CreatesNewConsignment()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: true);

			// Consol w/
			//   2 shipments
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;
			var container1 = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment1.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory, "HSB1", "S00001000", "MSB1", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN1 Destination", "AUSYD", rcnInNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory, 2);
			var rcn2InNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB2", warehouse, shipment2.PK);
			AssertConsignmentAdditionalRefs(rcn2InNewFactory, "HSB2", "S00001001", "MSB1", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN2 Destination", "AUSYD", rcn2InNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2InNewFactory, 2);

			var asnsInNewFactory = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			var pivotsInNewFactory = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());

			AssertEquals("An ASN should be created for the Consol and Container", 2, asnsInNewFactory.Length);
			AssertEquals("An RTUs should be created for the Container", 1, rtuInNewFactory.Length);
			AssertEquals("The ASN and RTU should be linked by a pivot", 1, pivotsInNewFactory.Length);
			var rtu = rtuInNewFactory.Single();
			var stagingLocation = warehouse.DefaultInboundDockDoorLocation;
			rtu.WRH_WL_StagingLocation = stagingLocation.PK;

			var packagesForShipment1 = asnsInNewFactory.SelectMany(asn => asn.PackageStates).Where(p => p.WPS_WRC_TransitReceiveConsignment == rcnInNewFactory.PK).ToArray();
			foreach (var package in packagesForShipment1)
			{
				package.WPS_WRH_TransitReceiveHeader = rtu.PK;
				package.WPS_WL_LastLocation = stagingLocation.PK;
				package.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			}

			newFactory.Save();

			var container2 = CreateContainer(consol, "CONT2");

			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);
			// Although we have an ASN for this container, we do not update it, and so it does not matter that it has already begun unloading
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package, container2);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment2);

			// Receive consignments and wave
			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn2InNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB2", warehouse, shipment2.PK);
			// This error will be fixed in another WI. RCNs should not duplicate additional references.
			//AssertConsignmentAdditionalRefs(rcn2InNewFactory, "HSB2", "S00001001", "MSB1", ZString.Empty, "AUSYD", ZString.Empty, ZString.Empty);
			AssertConsignmentPackages(rcn2InNewFactory2, 5);
		}

		#endregion

		#region TestForwardingSendsConsol_AddsPackagesAndResendsFromShipment_DeletesEmptyASN

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_AddsPackagesAndResendsFromShipment_DeletesEmptyASN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: true);

			// Consol w/
			//   1 shipments
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container1 = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and wave
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory, "HSB1", "S00001000", "MSB1", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN Destination", "AUSYD", rcnInNewFactory.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory, 2);

			var asnsInNewFactory = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			var pivotsInNewFactory = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());

			AssertEquals("An ASN should be created for the Consol and Container", 2, asnsInNewFactory.Length);
			AssertEquals("An RTUs should be created for the Container", 1, rtuInNewFactory.Length);
			AssertEquals("The ASN and RTU should be linked by a pivot", 1, pivotsInNewFactory.Length);

			newFactory.Save();

			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container1);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			// Receive consignments and wave
			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnInNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB1", warehouse, shipment.PK);
			AssertConsignmentAdditionalRefs(rcnInNewFactory2, "HSB1", "S00001000", "MSB1", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("RCN Destination", "AUSYD", rcnInNewFactory2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcnInNewFactory2, 4);

			var asnsInNewFactory2 = newFactory2.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("The empty ASNs should be deleted.", 0, asnsInNewFactory2.Length);
		}

		#endregion

		#region TestAttachNonBlindPackageStates

		public void TestAttachNonBlindPackages()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: false);
			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment1.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packLine1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Carton);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			Factory.Save();

			var rcns = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment1.JS_HouseBill));
			AssertEquals(1, rcns.Length);
			var rcn = rcns.Single();
			var packageState = rcn.PackageStates.Single();

			var packageState1 = UnloadAndLabelPackage(packageState, rtu, "P1");
			var packageState2 = UnloadAndLabelPackage(packageState, rtu, "P2");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(rcn);
			Factory.Save();

			packLine1.Reload();
			AssertNotNull(packLine1);
			AssertEquals("RCV", packLine1.JL_LastKnownTransitWarehouseStatus);

			var shipment2 = CreateShipment("HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			((ITransitWarehouseParent)shipment2).AttachPackages(new[] { packageState1, packageState2 });
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var packLines = newFactory.Load<ForwardingPackLine>(new ZQuery());
			var packLine2 = packLines.FirstOrDefault(p => p.JL_JS == shipment2.PK);
			AssertNotNull(packLine2);
			AssertNotEquals(packLine1.PK, packLine2.PK);
			AssertEquals("RCV", packLine2.JL_LastKnownTransitWarehouseStatus);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment2);
			Factory.Save();

			rcns = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment2.JS_HouseBill));
			AssertEquals(0, rcns.Length);

			var dcns = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, shipment2.JS_HouseBill));
			AssertNotNull(dcns);
			AssertEquals(1, dcns.Length);

			var dcn = dcns.Single();
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			packageState1.Reload();
			packageState2.Reload();
			LoadPackage(packageState1, warehouse.DefaultLocation, rtu, dtu, dll, dcn);
			LoadPackage(packageState2, warehouse.DefaultLocation, rtu, dtu, dll, dcn);
			Factory.Save();

			CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder();
			TriggerAndFireOutturn(dcn);
			packLine2.Reload();
			AssertEquals("DSP", packLine2.JL_LastKnownTransitWarehouseStatus);
		}

		#endregion

		#region TestSendingBothReceiveAndDispatchInstructions

		public void TestSendingBothReceiveAndDispatchInstructions_CreateDCN_AllowPartialLoadingBasedOnMixedDLLs()
		{
			var testData = CreateTestDataForCombined(isArrival: true);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);
			testData.warehouse.WW_AllowPartialLoadingDefault = true;
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "2", containerCount: 1, containerTypeCode: "40GP");

			var container3 = CreateContainer(consol, containerNum: "3", containerCount: 1, containerTypeCode: "20FR");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");

			var packline1InContainer3 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, container3, "PKG4");
			var packline2InContainer3 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Drum, container3, "PKG5");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment1.PK);
			var rcn2 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB2", testData.warehouse, shipment2.PK);
			AssertConsignmentPackages(rcn1, 3);
			AssertConsignmentPackages(rcn2, 2);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(3, loadLists.Length);

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 2, dispatchConsignments.Length);

			var dcn1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
			var dcn2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB2");

			AssertEquals(true, dcn1.WDC_AllowPartialLoading);
			AssertEquals(true, dcn2.WDC_AllowPartialLoading);
		}

		public void TestSendingBothReceiveAndDispatchInstructions_CreateLoadPlan_PacklinesAllocatedToContainersWithQuantityMoreThan1()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "", containerCount: 3, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container2, "PKG3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 3);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);

			var loadListFor20GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");
			var twentyGPContainers = loadListFor20GPContainer.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainer.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(3, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			var packageStatesForContainer1 = loadListFor20GPContainer.PackageStates;
			var packageStatesForContainer2 = loadListFor40GPContainer.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(1, packageStatesForContainer2.Count);

			var packageState1InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);
			var packageState1InContainer2 = packageStatesForContainer2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
			AssertEquals("PKG3", packageState1InContainer2.Package.KP_PackageID);
		}

		public void TestSendingBothReceiveAndDispatchInstructions_CreateLoadPlan_UnAssignedPackLines_SingleUnAllocatedContainer()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "", containerCount: 3, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var unAllocatedPackLine = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, null, "PKG3");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1 = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment1.PK);
			AssertConsignmentPackages(rcn1, 3);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);
			var loadListFor20GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainer = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");

			var twentyGPContainers = loadListFor20GPContainer.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainer.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(3, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			var packageStatesForContainer1 = loadListFor20GPContainer.PackageStates;
			var packageStatesForContainer2 = loadListFor40GPContainer.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(1, packageStatesForContainer2.Count);
			var packageState1InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageState2InContainer1 = packageStatesForContainer1.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum);
			var packageState1InContainer2 = packageStatesForContainer2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			AssertEquals("PKG1", packageState1InContainer1.Package.KP_PackageID);
			AssertEquals("PKG2", packageState2InContainer1.Package.KP_PackageID);
			AssertEquals("PKG3", packageState1InContainer2.Package.KP_PackageID);
		}

		public void TestSendingBothReceiveAndDispatchInstructions_ReceiveInstructionsSendingFail_FixShipment_ResendDispatchInstructions()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container2, "PKG3");
			var unAllocatedPackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG4");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnCreated = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnCreated, 4);
			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must be created for the shipment.", asn);
			AssertReceiveASN(asn, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStates = rcnCreated.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asn.PK).ToArray();
			AssertEquals("Four package states should have been created.", 4, packageStates.Length);

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("No DCNs are created since there is an unallocated packline.", 0, dcns.Length);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there is an unallocated packline.", 0, dtus.Length);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("No load lists must be created since there is an unallocated packline.", 0, loadLists.Length);

			// Change shipment so that we can resend dispatch consignment instructions.
			var factoryAfterSendingReceiveConsignment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAfterSendingReceiveConsignment = factoryAfterSendingReceiveConsignment.Load<ForwardingShipment>(shipment.PK);
			var unAllocatedPacklineInAfterSendingReceiveConsignment = factoryAfterSendingReceiveConsignment.Load<ForwardingPackLine>(unAllocatedPackLine.PK);
			unAllocatedPacklineInAfterSendingReceiveConsignment.JL_JC = container2.PK;
			factoryAfterSendingReceiveConsignment.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var factoryAfterFixingShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterFixingShipment = AssertAndReturnReceiveConsignment(factoryAfterFixingShipment, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnAfterFixingShipment, 4);
			var asnAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must still exists.", asnAfterFixingShipment);
			AssertReceiveASN(asnAfterFixingShipment, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStatesAfterFixingShipment = rcnAfterFixingShipment.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asnAfterFixingShipment.PK).ToArray();
			AssertEquals("Four package states should still remain in ASN.", 4, packageStatesAfterFixingShipment.Length);

			var dcnsAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Four Packagestates assigned to DCN.", 4, dcnsAfterFixingShipment.PackageStates.Count);

			var dtusAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Two DTUs are created.", 2, dtusAfterFixingShipment.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "A", "B" }, dtusAfterFixingShipment.Select(d => d.ContainerNumber));

			var loadListAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Two load lists must be created.", 2, loadListAfterFixingShipment.Length);
			var packageIds = loadListAfterFixingShipment.SelectMany(l => l.PackageStates).Select(p => p.Package.KP_PackageID);
			AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2", "PKG3", "PKG4" }, packageIds);
		}

		public void TestSendingBothReceiveAndDispatchInstructions_ReceiveInstructionsSendingFail_FixShipment_ResendCombinedInstructions()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "40GP");

			var packline1InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packline2InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container1, "PKG2");
			var packline1InContainer2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container2, "PKG3");
			var unAllocatedPackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG4");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnCreated = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnCreated, 4);
			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must be created for the shipment.", asn);
			AssertReceiveASN(asn, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStates = rcnCreated.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asn.PK).ToArray();
			AssertEquals("Four package states should have been created.", 4, packageStates.Length);

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("No DCNs are created since there is an unallocated packline.", 0, dcns.Length);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there is an unallocated packline.", 0, dtus.Length);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("No load lists must be created since there is an unallocated packline.", 0, loadLists.Length);

			// Change shipment so that we can resend dispatch consignment instructions.
			var factoryAfterSendingReceiveConsignment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAfterSendingReceiveConsignment = factoryAfterSendingReceiveConsignment.Load<ForwardingShipment>(shipment.PK);
			var unAllocatedPacklineInAfterSendingReceiveConsignment = factoryAfterSendingReceiveConsignment.Load<ForwardingPackLine>(unAllocatedPackLine.PK);
			unAllocatedPacklineInAfterSendingReceiveConsignment.JL_JC = container2.PK;
			factoryAfterSendingReceiveConsignment.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var factoryAfterFixingShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterFixingShipment = AssertAndReturnReceiveConsignment(factoryAfterFixingShipment, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnAfterFixingShipment, 4);
			var asnAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must still exists.", asnAfterFixingShipment);
			AssertReceiveASN(asnAfterFixingShipment, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStatesAfterFixingShipment = rcnAfterFixingShipment.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asnAfterFixingShipment.PK).ToArray();
			AssertEquals("Four package states should still remain in ASN.", 4, packageStatesAfterFixingShipment.Length);

			var dcnsAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Four Packagestates assigned to DCN.", 4, dcnsAfterFixingShipment.PackageStates.Count);

			var dtusAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Two DTUs are created.", 2, dtusAfterFixingShipment.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "A", "B" }, dtusAfterFixingShipment.Select(d => d.ContainerNumber));

			var loadListAfterFixingShipment = factoryAfterFixingShipment.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Two load lists must be created.", 2, loadListAfterFixingShipment.Length);
			var packageIds = loadListAfterFixingShipment.SelectMany(l => l.PackageStates).Select(p => p.Package.KP_PackageID);
			AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2", "PKG3", "PKG4" }, packageIds);
		}

		public void TestReceiveAndDispatchInstructionsSentSuccessfully_ResendingSameDispatchInstruction_DoesNot_DelinkOrDeleteTheMatchingLoadList()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var unAllocatedPackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG1");
			var unAllocatedPackLine2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnCreated = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnCreated, 2);
			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must be created for the shipment.", asn);
			AssertReceiveASN(asn, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStates = rcnCreated.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asn.PK).ToArray();
			AssertEquals("Two package states should have been created.", 2, packageStates.Length);

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("DCN are created since there are unAllocatedPackLines but no containers.", 1, dcns.Length);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there are no containers.", 0, dtus.Length);

			var loadListBeforeResend = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var loadlistPackageStatePKsBeforeResend = loadListBeforeResend.PackageStates.Select(p => p.PK);

			// Resend dispatch instruction from the same consol
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var newBizOFactoryAfterResendingDispatchInstruction = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterResendingDispatchInstruction = AssertAndReturnReceiveConsignment(newBizOFactoryAfterResendingDispatchInstruction, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnAfterResendingDispatchInstruction, 2);
			var asnAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN still exists for the shipment.", asnAfterResendingDispatchInstruction);
			AssertReceiveASN(asnAfterResendingDispatchInstruction, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStatesAfterResendingDispatchInstruction = rcnAfterResendingDispatchInstruction.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asnAfterResendingDispatchInstruction.PK).ToArray();
			AssertEquals("Two package states still exist.", 2, packageStatesAfterResendingDispatchInstruction.Length);

			var dcnsAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("One DCN still exists.", 1, dcnsAfterResendingDispatchInstruction.Length);

			var dtusAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there are no containers.", 0, dtusAfterResendingDispatchInstruction.Length);

			var loadListAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Matched to the same Load List.", loadListBeforeResend.PK, loadListAfterResendingDispatchInstruction.PK);

			var loadListPackageStatePKsAfterResendingDispatchInstruction = loadListAfterResendingDispatchInstruction.PackageStates.Select(p => p.PK);
			AssertContainsExactElementsInAnyOrder(loadlistPackageStatePKsBeforeResend, loadListPackageStatePKsAfterResendingDispatchInstruction);
		}

		public void TestReceiveAndDispatchInstructionsSentSuccessfully_ResendingSameDispatchInstruction_DoesNot_DelinkOrDeleteTheMatchingLoadList_AirTransportMode()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD", transportMode: Constants.TransportModes.Air);
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var unAllocatedPackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG1");
			var unAllocatedPackLine2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnCreated = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnCreated, 2);
			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must be created for the shipment.", asn);
			AssertReceiveASN(asn, consol.PK, JobConsolSchema.Constants.Prefix, "MSB-1");

			var packageStates = rcnCreated.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asn.PK).ToArray();
			AssertEquals("Two package states should have been created.", 2, packageStates.Length);

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("DCN are created since there are unAllocatedPackLines but no containers.", 1, dcns.Length);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there are no containers.", 0, dtus.Length);

			var loadListBeforeResend = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var loadlistPackageStatePKsBeforeResend = loadListBeforeResend.PackageStates.Select(p => p.PK);

			// Resend dispatch instruction from the same consol
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var newBizOFactoryAfterResendingDispatchInstruction = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterResendingDispatchInstruction = AssertAndReturnReceiveConsignment(newBizOFactoryAfterResendingDispatchInstruction, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnAfterResendingDispatchInstruction, 2);
			var asnAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN still exists for the shipment.", asnAfterResendingDispatchInstruction);
			AssertReceiveASN(asnAfterResendingDispatchInstruction, consol.PK, JobConsolSchema.Constants.Prefix, "MSB-1");

			var packageStatesAfterResendingDispatchInstruction = rcnAfterResendingDispatchInstruction.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asnAfterResendingDispatchInstruction.PK).ToArray();
			AssertEquals("Two package states still exist.", 2, packageStatesAfterResendingDispatchInstruction.Length);

			var dcnsAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("One DCN still exists.", 1, dcnsAfterResendingDispatchInstruction.Length);

			var dtusAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there are no containers.", 0, dtusAfterResendingDispatchInstruction.Length);

			var loadListAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Matched to the same Load List.", loadListBeforeResend.PK, loadListAfterResendingDispatchInstruction.PK);

			var loadListPackageStatePKsAfterResendingDispatchInstruction = loadListAfterResendingDispatchInstruction.PackageStates.Select(p => p.PK);
			AssertContainsExactElementsInAnyOrder(loadlistPackageStatePKsBeforeResend, loadListPackageStatePKsAfterResendingDispatchInstruction);
		}

		public void TestReceiveAndDispatchInstructionsSentSuccessfully_ResendingSameDispatchInstruction_DoesNot_DelinkOrDeleteTheMatchingLoadList_WithContainers()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");
			var container2 = CreateContainer(consol, containerNum: "B", containerCount: 1, containerTypeCode: "40GP");

			var packlineInContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container1, "PKG1");
			var packlineInContainer2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container2, "PKG2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnCreated = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnCreated, 2);
			var asn = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN must be created for the shipment.", asn);
			AssertReceiveASN(asn, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStates = rcnCreated.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asn.PK).ToArray();
			AssertEquals("Two package states should have been created.", 2, packageStates.Length);

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("One DCN is created since there are created.", 1, dcns.Length);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Two DTUs are created since there are two containers.", 2, dtus.Length);

			var loadListsBeforeResend = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Two Load Lists are created since tehre are two containers.", 2, loadListsBeforeResend.Length);
			var loadlistPackageStatePKsBeforeResend = loadListsBeforeResend.SelectMany(p => p.PackageStates).Select(p => p.PK);

			// Resend dispatch instruction from the same consol
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var newBizOFactoryAfterResendingDispatchInstruction = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterResendingDispatchInstruction = AssertAndReturnReceiveConsignment(newBizOFactoryAfterResendingDispatchInstruction, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcnAfterResendingDispatchInstruction, 2);
			var asnAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertNotNull("One receive ASN still exists for the shipment.", asnAfterResendingDispatchInstruction);
			AssertReceiveASN(asnAfterResendingDispatchInstruction, consol.PK, JobConsolSchema.Constants.Prefix, "MSB1");

			var packageStatesAfterResendingDispatchInstruction = rcnAfterResendingDispatchInstruction.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == asnAfterResendingDispatchInstruction.PK).ToArray();
			AssertEquals("Two package states still exist.", 2, packageStatesAfterResendingDispatchInstruction.Length);

			var dcnsAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("One DCN still exists.", 1, dcnsAfterResendingDispatchInstruction.Length);

			var dtusAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Two DTUs still exist.", 2, dtusAfterResendingDispatchInstruction.Length);

			var loadListsAfterResendingDispatchInstruction = newBizOFactoryAfterResendingDispatchInstruction.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Matched to the same Load Lists.", loadListsBeforeResend.Select(l => l.PK), loadListsAfterResendingDispatchInstruction.Select(l => l.PK));

			var loadListPackageStatePKsAfterResendingDispatchInstruction = loadListsAfterResendingDispatchInstruction.SelectMany(l => l.PackageStates).Select(p => p.PK);
			AssertContainsExactElementsInAnyOrder(loadlistPackageStatePKsBeforeResend, loadListPackageStatePKsAfterResendingDispatchInstruction);
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_CreateReceiveAndDispatch()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container1);
			var packline12 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Box, container2);
			var packline23 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, container2);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "", "C00001000", "AA", "30-Apr-20 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn1, 2);

			var rcn2 = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn2, "HSB2", "S00001001", "", "C00001000", "AA", "30-Apr-20 00:00");
			AssertEquals("RCN2 Destination", "AUADL", rcn2.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn2, 1);

			AssertEquals("One receive ASN must be created for the consol.", 1, newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery()).Length);

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "30-Apr-20 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 2 Dispatch Consignments", 2, dispatchConsignments.Length);
		}

		#endregion

		#region TestForwardingSendingShipmentsWithGoverningReferences_PopulatesMatchingReceiveConsignmentsAndBlindPackages

		[TestDate(2018, 1, 1)]
		public void TestForwardingSendingShipmentsWithCustomsReferences_PopulatesMatchingReceiveConsignments_WithReferenceMapper()
		{
			SetupForwardingToTWMappingForGoverningReferences();

			var testData = CreateTestData(false, "NZCHC");
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLeg = CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container1, packLineId: "P1");
			var packline12 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container2, packLineId: "P2");
			var packline13 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, packLineId: "P3");
			var packline21 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Case, container1, packLineId: "P4");

			// Send receive instruction
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and packages created
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcnForShipment1, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcnForShipment1.WRC_RL_NKDestination);
			AssertEquals(0, rcnForShipment1.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnForShipment1.WRC_CustomsStatus);
			AssertConsignmentPackages(rcnForShipment1, 3);

			var rcnForShipment2 = AssertConsignment(newFactory, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcnForShipment2, "HSB2", "S00001001", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN2 Destination", "AUADL", rcnForShipment2.WRC_RL_NKDestination);
			AssertEquals(0, rcnForShipment2.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnForShipment2.WRC_CustomsStatus);
			AssertConsignmentPackages(rcnForShipment2, 1);

			AssertEquals("One receive ASN must be created for the entire consol.", 1, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			var asnPK = rcnForShipment1.PackageStates.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct().Single();
			var receiveASNForShipment1PK = rcnForShipment1.PackageStates.First().WPS_WRP_ReceiveExpectedPacking;
			var receiveASNForShipment2PK = rcnForShipment2.PackageStates.Single().WPS_WRP_ReceiveExpectedPacking;
			AssertReceiveASN(newFactory.Load<WhsItemReceiveASN>(asnPK), consol.PK, "JK", "MSB1", outboundLeg);
			AssertEquals("No RTUs should be created for export", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);

			var packageStatesForShipment1 = rcnForShipment1.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipment1PK).ToArray();
			var packageState1ForShipment1 = packageStatesForShipment1[0];
			var packageState2ForShipment1 = packageStatesForShipment1[1];
			var packageState3ForShipment1 = packageStatesForShipment1[2];
			AssertPackage(packageState1ForShipment1, Constants.PkgUnit.Box);
			AssertPackage(packageState2ForShipment1, Constants.PkgUnit.Box);
			AssertPackage(packageState3ForShipment1, Constants.PkgUnit.Box);

			var packageStateForShipment2 = rcnForShipment2.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipment2PK);
			AssertPackage(packageStateForShipment2, Constants.PkgUnit.Case);

			Helper.CreateAdditionalReference(shipment1, "CEN123", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateAdditionalReference(shipment2, "CEN123", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Send dispatch instruction for the first time
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1AfterRunningLogWalker = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcnForShipment2AfterRunningLogWalker = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);

			// CEN references added to consignments
			AssertEquals(1, rcnForShipment1AfterRunningLogWalker.CustomsReferenceNumbers.Count);
			AssertEquals(1, rcnForShipment2AfterRunningLogWalker.CustomsReferenceNumbers.Count);
			AssertGoverningReference(rcnForShipment1AfterRunningLogWalker, rcnForShipment1AfterRunningLogWalker.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(rcnForShipment2AfterRunningLogWalker, rcnForShipment2AfterRunningLogWalker.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, rcnForShipment1AfterRunningLogWalker.WRC_CustomsStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, rcnForShipment2AfterRunningLogWalker.WRC_CustomsStatus);

			// Load list, dispatch transportation units and dispatch consignment created
			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

			var dispatchConsignment1 = dispatchConsignments.First(c => c.PackageStates.Count == 3);
			AssertDispatchConsignment(packageState1ForShipment1, dispatchConsignment1, shipment1.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment1, "HSB1", "S00001000", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN1 Destination", "AUADL", dispatchConsignment1.WDC_RL_NKDestination);
			var dispatchConsignment2 = dispatchConsignments.First(c => c.PackageStates.Count == 1);
			AssertDispatchConsignment(packageStateForShipment2, dispatchConsignment2, shipment2.PK);
			AssertConsignmentAdditionalRefs(dispatchConsignment2, "HSB2", "S00001001", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("DCN2 Destination", "AUADL", dispatchConsignment2.WDC_RL_NKDestination);

			// Create CRN reference on shipments
			Helper.CreateAdditionalReference(shipment1, "CRN123", WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateAdditionalReference(shipment2, "CRN123", WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Resend dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResending = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1AfterResending = AssertConsignment(newBizOFactoryAfterResending, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcnForShipment2AfterResending = AssertConsignment(newBizOFactoryAfterResending, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);

			AssertEquals(2, rcnForShipment1AfterResending.CustomsReferenceNumbers.Count);
			AssertEquals(2, rcnForShipment2AfterResending.CustomsReferenceNumbers.Count);
			AssertEquals(0, rcnForShipment1AfterResending.PortReferences.Count);
			AssertEquals(0, rcnForShipment2AfterResending.PortReferences.Count);

			// CEN references remain
			AssertGoverningReference(rcnForShipment1AfterResending, rcnForShipment1AfterResending.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(rcnForShipment2AfterResending, rcnForShipment2AfterResending.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// CRN references added to matching consignments
			AssertGoverningReference(rcnForShipment1AfterResending, rcnForShipment1AfterResending.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(rcnForShipment2AfterResending, rcnForShipment2AfterResending.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			AssertEquals(TransitWarehouseCustomsStatuses.Codes.Cleared, rcnForShipment1AfterResending.WRC_CustomsStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.Cleared, rcnForShipment2AfterResending.WRC_CustomsStatus);

			// Create PAN reference on shipments
			Helper.CreateAdditionalReference(shipment1, "PAN123", WarehouseAdditionalReferenceTypes.Codes.DestinationPort, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateAdditionalReference(shipment2, "PAN123", WarehouseAdditionalReferenceTypes.Codes.DestinationPort, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Resend dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResending2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1AfterResending2 = AssertConsignment(newBizOFactoryAfterResending2, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcnForShipment2AfterResending2 = AssertConsignment(newBizOFactoryAfterResending2, "HSB2", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment2.PK, outboundLeg);

			AssertEquals(2, rcnForShipment1AfterResending2.CustomsReferenceNumbers.Count);
			AssertEquals(2, rcnForShipment2AfterResending2.CustomsReferenceNumbers.Count);
			AssertEquals(1, rcnForShipment1AfterResending2.PortReferences.Count);
			AssertEquals(1, rcnForShipment2AfterResending2.PortReferences.Count);

			// CEN references remain
			AssertGoverningReference(rcnForShipment1AfterResending2, rcnForShipment1AfterResending2.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(rcnForShipment2AfterResending2, rcnForShipment2AfterResending2.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// CRN references added to matching consignments
			AssertGoverningReference(rcnForShipment1AfterResending2, rcnForShipment1AfterResending2.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(rcnForShipment2AfterResending2, rcnForShipment2AfterResending2.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// PAN reference added to matching consignments
			AssertGoverningReference(rcnForShipment1AfterResending2, rcnForShipment1AfterResending2.PortReferences.Cast<CusEntryNumber>().ToArray(), TransitWarehousePortReferenceTypes.Codes.PortAuthority, "PAN123", TransitWarehouseReferenceCategories.Codes.PortReference);
			AssertGoverningReference(rcnForShipment2AfterResending2, rcnForShipment2AfterResending2.PortReferences.Cast<CusEntryNumber>().ToArray(), TransitWarehousePortReferenceTypes.Codes.PortAuthority, "PAN123", TransitWarehouseReferenceCategories.Codes.PortReference);

			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, rcnForShipment1AfterResending2.WRC_CustomsStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, rcnForShipment2AfterResending2.WRC_CustomsStatus);
		}

		[TestDate(2018, 1, 1)]
		public void TestForwardingSendingShipmentsWithCustomsReferences_PopulatesMatchingReceiveConsignments_WithoutReferenceMapper()
		{
			var testData = CreateTestData(false, "NZCHC");
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLeg = CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");

			var packline11 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container1, packLineId: "P1");

			// Send receive instruction
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and packages created
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1 = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcnForShipment1, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUADL", rcnForShipment1.WRC_RL_NKDestination);
			AssertEquals(0, rcnForShipment1.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnForShipment1.WRC_CustomsStatus);
			AssertConsignmentPackages(rcnForShipment1, 1);

			AssertEquals("One receive ASN must be created for the entire consol.", 1, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			var asnPK = rcnForShipment1.PackageStates.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct().Single();
			var receiveASNForShipment1PK = rcnForShipment1.PackageStates.First().WPS_WRP_ReceiveExpectedPacking;
			AssertReceiveASN(newFactory.Load<WhsItemReceiveASN>(asnPK), consol.PK, "JK", "MSB1", outboundLeg);
			AssertEquals("No RTUs should be created for export", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);

			var packageState1ForShipment1 = rcnForShipment1.PackageStates.Single(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipment1PK);
			AssertPackage(packageState1ForShipment1, Constants.PkgUnit.Box);

			Helper.CreateAdditionalReference(shipment1, "CEN123", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Send dispatch instruction for the first time
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnForShipment1AfterRunningLogWalker = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment1.PK, outboundLeg);

			AssertNull("Additional reference not added to consignment", rcnForShipment1AfterRunningLogWalker.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.AssignedPicker));
			AssertEquals("Customs reference not added because no mapping exists", 0, rcnForShipment1AfterRunningLogWalker.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status not updated", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnForShipment1AfterRunningLogWalker.WRC_CustomsStatus);
		}

		[TestDate(2018, 1, 1)]
		public void TestForwardingSendingShipmentsWithCustomsReferences_PopulatesBlindPackages_WithReferenceMapper()
		{
			SetupForwardingToTWMappingForGoverningReferences();

			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, reference: "HU1");

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, entryNum: shipment.JobNumber);

			var blindChildPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var blindChildPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var divotToChild1 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, blindChildPackage1, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			var divotToChild2 = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, blindChildPackage2, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.Save();

			AssertEquals(0, blindChildPackage1.Package.CustomsReferenceNumbers.Count);
			AssertEquals(0, blindChildPackage2.Package.CustomsReferenceNumbers.Count);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindChildPackage1.WPS_CustomsStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindChildPackage2.WPS_CustomsStatus);

			Helper.CreateAdditionalReference(shipment, "CEN123", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Send dispatch instruction for the first time
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			// Dispatch consignment created
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Dispatch Consignment should have 2 blind packages.", 2, dispatchConsignment.PackageStates.Count);

			// Blind packages linked to dispatch consignment
			var blindPackageLinkedToDCN1 = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(blindChildPackage1.PK);
			var blindPackageLinkedToDCN2 = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(blindChildPackage2.PK);
			AssertDispatchConsignment(blindPackageLinkedToDCN1, dispatchConsignment, shipment.PK);
			AssertDispatchConsignment(blindPackageLinkedToDCN2, dispatchConsignment, shipment.PK);
			AssertEquals("CEN added to blind package.", 1, blindPackageLinkedToDCN1.Package.CustomsReferenceNumbers.Count);
			AssertEquals("CEN added to blind package.", 1, blindPackageLinkedToDCN2.Package.CustomsReferenceNumbers.Count);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, blindPackageLinkedToDCN1.WPS_CustomsStatus);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, blindPackageLinkedToDCN2.WPS_CustomsStatus);

			// Load list created
			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, blindPackageLinkedToDCN1.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, blindPackageLinkedToDCN2.WPS_WDL_LoadList);

			// Create CRN reference on shipments
			Helper.CreateAdditionalReference(shipment, "CRN123", WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateAdditionalReference(shipment, "CRN123", WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Resend dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResending = new BusinessObjectFactory() { RefreshEnabled = false };
			var blindPackageLinkedToDCN1AfterResending = newBizOFactoryAfterResending.Load<WhsItemPackageState>(blindChildPackage1.PK);
			var blindPackageLinkedToDCN2AfterResending = newBizOFactoryAfterResending.Load<WhsItemPackageState>(blindChildPackage2.PK);
			AssertEquals("2 customs references on blind package.", 2, blindPackageLinkedToDCN1AfterResending.Package.CustomsReferenceNumbers.Count);
			AssertEquals("2 customs reference on blind package.", 2, blindPackageLinkedToDCN2AfterResending.Package.CustomsReferenceNumbers.Count);
			AssertEquals("0 port references on blind package.", 0, blindPackageLinkedToDCN1AfterResending.Package.PortReferences.Count);
			AssertEquals("0 port references on blind package.", 0, blindPackageLinkedToDCN2AfterResending.Package.PortReferences.Count);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.Cleared, blindPackageLinkedToDCN1AfterResending.WPS_CustomsStatus);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.Cleared, blindPackageLinkedToDCN2AfterResending.WPS_CustomsStatus);

			// CEN references remain
			AssertGoverningReference(blindPackageLinkedToDCN1AfterResending.Package, blindPackageLinkedToDCN1AfterResending.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(blindPackageLinkedToDCN2AfterResending.Package, blindPackageLinkedToDCN2AfterResending.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// CRN references added to blind package
			AssertGoverningReference(blindPackageLinkedToDCN1AfterResending.Package, blindPackageLinkedToDCN1AfterResending.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(blindPackageLinkedToDCN2AfterResending.Package, blindPackageLinkedToDCN2AfterResending.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// Create PAN reference on shipments
			Helper.CreateAdditionalReference(shipment, "PAN123", WarehouseAdditionalReferenceTypes.Codes.DestinationPort, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateAdditionalReference(shipment, "PAN123", WarehouseAdditionalReferenceTypes.Codes.DestinationPort, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			// Resend dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResending2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var blindPackageLinkedToDCN1AfterResending2 = newBizOFactoryAfterResending2.Load<WhsItemPackageState>(blindChildPackage1.PK);
			var blindPackageLinkedToDCN2AfterResending2 = newBizOFactoryAfterResending2.Load<WhsItemPackageState>(blindChildPackage2.PK);
			AssertEquals("2 customs references on blind package.", 2, blindPackageLinkedToDCN1AfterResending2.Package.CustomsReferenceNumbers.Count);
			AssertEquals("2 customs references on blind package.", 2, blindPackageLinkedToDCN2AfterResending2.Package.CustomsReferenceNumbers.Count);
			AssertEquals("1 port references on blind package.", 1, blindPackageLinkedToDCN1AfterResending2.Package.PortReferences.Count);
			AssertEquals("1 port references on blind package.", 1, blindPackageLinkedToDCN2AfterResending2.Package.PortReferences.Count);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, blindPackageLinkedToDCN1AfterResending2.WPS_CustomsStatus);
			AssertEquals("Customs status recalculated for blind package.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, blindPackageLinkedToDCN2AfterResending2.WPS_CustomsStatus);

			// CEN references remain
			AssertGoverningReference(blindPackageLinkedToDCN1AfterResending2.Package, blindPackageLinkedToDCN1AfterResending2.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(blindPackageLinkedToDCN2AfterResending2.Package, blindPackageLinkedToDCN2AfterResending2.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// CRN references added to blind package
			AssertGoverningReference(blindPackageLinkedToDCN1AfterResending2.Package, blindPackageLinkedToDCN1AfterResending2.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			AssertGoverningReference(blindPackageLinkedToDCN2AfterResending2.Package, blindPackageLinkedToDCN2AfterResending2.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().ToArray(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN123", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			// PAN reference added to blind package
			AssertGoverningReference(blindPackageLinkedToDCN1AfterResending2.Package, blindPackageLinkedToDCN1AfterResending2.Package.PortReferences.Cast<CusEntryNumber>().ToArray(), TransitWarehousePortReferenceTypes.Codes.PortAuthority, "PAN123", TransitWarehouseReferenceCategories.Codes.PortReference);
			AssertGoverningReference(blindPackageLinkedToDCN2AfterResending2.Package, blindPackageLinkedToDCN2AfterResending2.Package.PortReferences.Cast<CusEntryNumber>().ToArray(), TransitWarehousePortReferenceTypes.Codes.PortAuthority, "PAN123", TransitWarehouseReferenceCategories.Codes.PortReference);
		}

		void SetupForwardingToTWMappingForGoverningReferences()
		{
			// Map Forwarding additional references to governing references in Transit
			var transitMapping = new TransitReferenceMappingConfiguration();
			var cenReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber
			};

			var crnReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber
			};

			var panReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.DestinationPort,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.PortReference,
				TargetType = TransitWarehousePortReferenceTypes.Codes.PortAuthority
			};

			transitMapping.TransitReferenceMappingCollection.Add(cenReferenceMapping);
			transitMapping.TransitReferenceMappingCollection.Add(crnReferenceMapping);
			transitMapping.TransitReferenceMappingCollection.Add(panReferenceMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);
		}

		#endregion

		#region TestBookingConfirmedEvent

		[TestDate(2021, 02, 22, 05, 30, 20)]
		public void TestBookingConfirmedEvent_HasConsol_SendFromConsol_SEA()
		{
			var testData = CreateTestData(true);
			testData.warehouse.WW_AllowPartialLoadingDefault = true;
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			var packline1InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcn, rcn.WRC_JobID, "Transit Receive");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactoryAfterSendingReceiveInstructions = new BusinessObjectFactory();
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactoryAfterSendingReceiveInstructions, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcnInNewFactory, rcnInNewFactory.WRC_JobID, "Transit Receive", isNew: false);

			// send dispatch instructions
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			var newFactoryAfterSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newFactoryAfterSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcn, dcn.WDC_JobID, "Transit Dispatch");

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			var newFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentsAfterResending = newFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignmentsAfterResending.Length);
			var dcnAfterResending = dispatchConsignmentsAfterResending.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcnAfterResending, dcnAfterResending.WDC_JobID, "Transit Dispatch", isNew: false);
		}

		[TestDate(2021, 02, 04)]
		public void TestBookingConfirmedEvent_HasConsol_SendFromConsol_AIR()
		{
			var testData = CreateTestData(true);
			testData.warehouse.WW_AllowPartialLoadingDefault = true;
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD", transportMode: Constants.TransportModes.Air);
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			var packline1InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcn, rcn.WRC_JobID, "Transit Receive");
			AssertEquals(Constants.TransportModes.Air, rcn.WRC_TransportMode);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactoryAfterSendingReceiveInstructions = new BusinessObjectFactory();
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactoryAfterSendingReceiveInstructions, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcnInNewFactory, rcnInNewFactory.WRC_JobID, "Transit Receive", isNew: false);

			// send dispatch instructions
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			var newFactoryAfterSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newFactoryAfterSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcn, dcn.WDC_JobID, "Transit Dispatch");
			AssertEquals(Constants.TransportModes.Air, dcn.WDC_TransportMode);

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			var newFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentsAfterResending = newFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignmentsAfterResending.Length);
			var dcnAfterResending = dispatchConsignmentsAfterResending.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcnAfterResending, dcnAfterResending.WDC_JobID, "Transit Dispatch", isNew: false);
			AssertEquals(Constants.TransportModes.Air, dcnAfterResending.WDC_TransportMode);
		}

		[TestDate(2021, 02, 04)]
		public void TestBookingConfirmedEvent_HasConsol_SendFromShipment()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			var packline1InContainer1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcn, rcn.WRC_JobID, "Transit Receive");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			var newFactoryAfterResendingReceiveInstructions = new BusinessObjectFactory();
			var rcnInNewFactory = AssertAndReturnReceiveConsignment(newFactoryAfterResendingReceiveInstructions, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcnInNewFactory, rcnInNewFactory.WRC_JobID, "Transit Receive", isNew: false);

			// send dispatch instructions
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);
			var newFactoryAfterSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newFactoryAfterSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcn, dcn.WDC_JobID, "Transit Dispatch");

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);
			var newFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentsAfterResending = newFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignmentsAfterResending.Length);
			var dcnAfterResending = dispatchConsignmentsAfterResending.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcnAfterResending, dcnAfterResending.WDC_JobID, "Transit Dispatch", isNew: false);
		}

		[TestDate(2021, 02, 04)]
		public void TestBookingConfirmedEvent_HasShipmentOnly()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			testData.warehouse.WW_AllowPartialLoadingDefault = true;

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var packLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, null, "PKG1");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcn, rcn.WRC_JobID, "Transit Receive");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			var newFactoryAfterResendingReceiveInstructions = new BusinessObjectFactory();
			var rcnAFterResendingInstructions = AssertAndReturnReceiveConsignment(newFactoryAfterResendingReceiveInstructions, "HSB1", testData.warehouse, shipment.PK);
			AssertBookingConfirmedEvent(rcnAFterResendingInstructions, rcnAFterResendingInstructions.WRC_JobID, "Transit Receive", isNew: false);

			// send dispatch instructions
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);
			var newFactoryAfterSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newFactoryAfterSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcn, dcn.WDC_JobID, "Transit Dispatch");

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);
			var newFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentsAfterResending = newFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created  Dispatch Consignments", 1, dispatchConsignmentsAfterResending.Length);
			var dcnAfterResending = dispatchConsignmentsAfterResending.Single(c => c.WDC_ConsignmentID == "HSB1");
			AssertBookingConfirmedEvent(dcnAfterResending, dcnAfterResending.WDC_JobID, "Transit Dispatch", isNew: false);
		}

		static void AssertBookingConfirmedEvent<T>(T consignment, string jobID, string type, bool isNew = true) where T : BusinessObject, IStmALogParent
		{
			var bkcEvent = consignment.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingConfirmedCode && (l.SL_Reference.Contains(isNew ? "New" : "Updated"))).Single();
			AssertEquals(ZBool.True, bkcEvent.SL_FireWorkflow);
			AssertEquals(ZDateTime.Now, bkcEvent.SL_EventTime);
			AssertEquals(Env.Instance.CurrentBranch.Code, bkcEvent.SL_GB_NKBranch);
			AssertEquals(Env.Instance.CurrentDepartment.Code, bkcEvent.SL_GE_NKDepartment);
			AssertEquals("~AD", bkcEvent.SL_GS_NKUser);
			AssertEquals(consignment.PK, bkcEvent.SL_Parent);
			AssertEquals(ZDateTime.UtcNow, bkcEvent.SL_PostedTimeUtc);
			AssertEquals(consignment.TableName, bkcEvent.SL_Table);
			AssertEquals($"{(isNew ? "New" : "Updated")}|FAC=CFS|LOC=Sydney|TYP={type}|RFN={jobID}|WHS=TRW", bkcEvent.SL_Reference);
		}

		#endregion

		#region TestForwarder_SendsInstructionsWithRefNumberAndQtyMoreThan1

		public void TestForwarder_SendFromShipment_SendsInstructionsWithRefNumberAndQtyMoreThan1()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var shipment = CreateShipment("", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "2PLTs");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			// Receive consignment
			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var packageStateInRCN = rcn.PackageStates.Single();
			AssertEquals(2, packageStateInRCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInRCN.Package.KP_PackageID);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var packageStateInDCN = dispatchConsignment.PackageStates.Single();
			AssertEquals(2, packageStateInDCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInDCN.Package.KP_PackageID);
			AssertEquals(packageStateInRCN.PK, packageStateInDCN.PK);
		}

		public void TestForwarder_SendFromConsol_NoContainer_SendsInstructionsWithRefNumberAndQtyMoreThan1()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "2PLTs");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignment
			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var asn = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var packageStateInRCN = rcn.PackageStates.Single();
			AssertEquals(2, packageStateInRCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInRCN.Package.KP_PackageID);
			AssertEquals(asn.PK, packageStateInRCN.WPS_WRP_ReceiveExpectedPacking);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var packageStateInDCN = dispatchConsignment.PackageStates.Single();
			AssertEquals(2, packageStateInDCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInDCN.Package.KP_PackageID);
			AssertEquals(packageStateInRCN.PK, packageStateInDCN.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, packageStateInDCN.WPS_WDL_LoadList);
		}

		public void TestForwarder_SendFromConsol_WithContainer_SendsInstructionsWithRefNumberAndQtyMoreThan1()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "2PLTs", container: container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignment
			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var asn = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var packageStateInRCN = rcn.PackageStates.Single();
			AssertEquals(2, packageStateInRCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInRCN.Package.KP_PackageID);
			AssertEquals(asn.PK, packageStateInRCN.WPS_WRP_ReceiveExpectedPacking);

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var packageStateInDCN = dispatchConsignment.PackageStates.Single();
			AssertEquals(2, packageStateInDCN.Package.KP_PackageQty);
			AssertEquals("", packageStateInDCN.Package.KP_PackageID);
			AssertEquals(packageStateInRCN.PK, packageStateInDCN.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, packageStateInDCN.WPS_WDL_LoadList);
		}

		#endregion

		#region TestTWToForwarding_OutturnReceived_ChangeShipmentPackLines

		public void TestTWToForwarding_OutturnReceived_ChangeShipmentPackLines_SendDispatchInstructions()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT2");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLineInShipment = (ForwardingPackLine)shipmentInAnotherFactory.OuterPackLines.Single();
			AssertEquals("Pack type after sending outturn must be PLT", Constants.PkgUnit.Pallet, packLineInShipment.JL_F3_NKPackType);
			AssertEquals("PkgPackages should created and attached to packline", 2, packLineInShipment.PkgPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT1", "PLT2" }, packLineInShipment.PkgPackageCollection.Select(p => p.KP_PackageID));

			var packageToBeDeleted = packLineInShipment.PkgPackageCollection.Single(p => p.KP_PackageID == "PLT1");
			var jobPackLinePackage = factoryToLoadShipment.Load<JobPackLinePackage>(new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, packageToBeDeleted.PK));
			jobPackLinePackage.DeleteAll();
			packageToBeDeleted.Delete();
			factoryToLoadShipment.Save();
			AssertEquals("Shipment package count must be 1.", 1, packLineInShipment.PkgPackageCollection.Count);

			TriggerAndFireTransitRequestForRelease(consol);

			var dispatchConsignment = factoryToLoadShipment.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, dispatchConsignment.PackageStates.Count);
			AssertEquals(packageStateForPallet.PK, dispatchConsignment.PackageStates.Select(p => p.PK).Single());
		}

		public void TestTWToForwarding_OutturnReceived_ChangeShipmentPackLines_ReSendDispatchInstructions()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageStateForPLT1 = UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT2");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();

			TriggerAndFireOutturn(receiveConsignment);

			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLineInShipment = (ForwardingPackLine)shipmentInAnotherFactory.OuterPackLines.Single();
			AssertEquals("Pack type after sending outturn must be PLT", Constants.PkgUnit.Pallet, packLineInShipment.JL_F3_NKPackType);
			AssertEquals("PkgPackages should created and attached to packline", 2, packLineInShipment.PkgPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT1", "PLT2" }, packLineInShipment.PkgPackageCollection.Select(p => p.KP_PackageID));

			TriggerAndFireTransitRequestForRelease(consol);
			var dispatchConsignment = factoryToLoadShipment.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, dispatchConsignment.PackageStates.Count);
			AssertContainsExactElementsInAnyOrder(new[] { packageStateForPallet.PK, packageStateForPLT1.PK }, dispatchConsignment.PackageStates.Select(p => p.PK));

			var packageToBeDeleted = packLineInShipment.PkgPackageCollection.Single(p => p.KP_PackageID == "PLT1");
			var jobPackLinePackage = factoryToLoadShipment.Load<JobPackLinePackage>(new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, packageToBeDeleted.PK));
			jobPackLinePackage.DeleteAll();
			packageToBeDeleted.Delete();
			factoryToLoadShipment.Save();
			AssertEquals("Shipment package count must be 1.", 1, packLineInShipment.PkgPackageCollection.Count);

			TriggerAndFireTransitRequestForRelease(consol);

			factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignmentAfterResend = factoryToLoadShipment.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, dispatchConsignmentAfterResend.PackageStates.Count);
			AssertEquals(packageStateForPallet.PK, dispatchConsignmentAfterResend.PackageStates.Select(p => p.PK).Single());
		}

		#endregion

		#region TestForwardingToTWPortReferences

		public void TestForwardingToTW_WithoutReferenceMapper_PopulatesReceiveConsignment()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(shipment, "Invalid Ref", "UNK");
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			var othReference = rcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.AssertAdditionalReference(othReference, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			AssertNull("Invalid Warehouse Ref should not have been imported.", rcn.AdditionalReferenceNumbers.GetFirstReferenceNumberByType("UNK"));

			var consolAfterImport = newFactory.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have a successful export.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message should show invalid warehouse reference codes not imported.",
@"Warning - The following Additional References are invalid for this Transit Warehouse and could not be imported:
CODE           REFERENCE      COUNTRY
UNK            Invalid Ref", importNote?.ST_NoteText);
		}

		public void TestForwardingToTW_WithoutReferenceMapper_PopulatesDispatchConsignment()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(shipment, "Invalid Ref", "UNK");
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instruction
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);

			// send dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);
			var newFactory2 = new BusinessObjectFactory();
			var dcn = AssertAndReturnDispatchConsignment(newFactory2, "HSB1", testData.warehouse, shipment.PK);
			var othReference = dcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.AssertAdditionalReference(othReference, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			AssertNull("Invalid Warehouse Ref should not have been imported.", dcn.AdditionalReferenceNumbers.GetFirstReferenceNumberByType("UNK"));

			var consolAfterImport = newFactory2.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have a successful export.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message should show invalid warehouse reference codes not imported.",
@"Warning - The following Additional References are invalid for this Transit Warehouse and could not be imported:
CODE           REFERENCE      COUNTRY
UNK            Invalid Ref", importNote?.ST_NoteText);
		}

		public void TestForwardingToTW_ReferenceMapper_WithReferenceMapper_PopulatesDispatchConsignment()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var transitPortReferenceMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
				TransitWarehouseReferenceCategories.Codes.PortReference,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority);

			var transitCustomsReferenceMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference,
				TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);

			transitMapping.TransitReferenceMappingCollection.Add(transitPortReferenceMapping);
			transitMapping.TransitReferenceMappingCollection.Add(transitCustomsReferenceMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "PKR Reference Mapped", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
			Helper.CreateAdditionalReference(shipment, "BPR Reference Mapped", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			Helper.CreateAdditionalReference(shipment, "INV Reference Not Mapped", WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);

			// send dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);
			var newFactory2 = new BusinessObjectFactory();
			var dcn = AssertAndReturnDispatchConsignment(newFactory2, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(2, dcn.AdditionalReferenceNumbers.Count);
			Helper.AssertAdditionalReference(dcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber), WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "INV Reference Not Mapped", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			AssertCollectionNotContains(dcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToArray().Select(c => c.CE_EntryType), new String[] { TransitWarehousePortReferenceTypes.Codes.PortAuthority, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber });
		}

		public void TestForwardingToTW_ReferenceMapper_AdditionalReferenceToPortReferences_PopulatesReceiveConsignment()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var transitReferenceMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
				TransitWarehouseReferenceCategories.Codes.PortReference,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority);

			transitMapping.TransitReferenceMappingCollection.Add(transitReferenceMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(1, rcn.PortReferences.Count);
			Helper.AssertAdditionalReference(rcn.PortReferences.Cast<CusEntryNumber>().Single(), TransitWarehousePortReferenceTypes.Codes.PortAuthority, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.PortReference);
		}

		public void TestForwardingToTW_ReferenceMapper_AdditionalReferenceToCustomsReferences_PopulatesReceiveConsignment()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var transitReferenceMapping = Helper.CreateTransitReferenceMapping(
					TransitWarehouseReferenceCategories.Codes.AdditionalReference,
					WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			transitMapping.TransitReferenceMappingCollection.Add(transitReferenceMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, country: "AU");
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			AssertEquals(1, rcn.CustomsReferenceNumbers.Count);
			Helper.AssertAdditionalReference(rcn.CustomsReferenceNumbers.Cast<CusEntryNumber>().Single(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
		}

		public void TestForwardingToTransitReceive_ReferenceMapper_AdditionalReferenceToDifferentAdditionalReference_PopulatesReceiveConsignment()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var transitReferenceMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);

			transitMapping.TransitReferenceMappingCollection.Add(transitReferenceMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			var bookingPartyReferences = rcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			AssertEquals(1, bookingPartyReferences.Count());
			Helper.AssertAdditionalReference(bookingPartyReferences.Single(), WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
		}

		public void TestForwardingToTransitReceive_ReferenceMapper_MultipleAdditionalReference_PopulatesReceiveConsignment()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var transitReferenceMapping1 = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.AssignedPicker,
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.Other);
			var transitReferenceMapping2 = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.OrderNumber,
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				WarehouseAdditionalReferenceTypes.Codes.Other);

			transitMapping.TransitReferenceMappingCollection.Add(transitReferenceMapping1);
			transitMapping.TransitReferenceMappingCollection.Add(transitReferenceMapping2);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Helper.CreateAdditionalReference(shipment, "ABC Reference", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
			Helper.CreateAdditionalReference(shipment, "DEF Reference", WarehouseAdditionalReferenceTypes.Codes.OrderNumber);
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			var others1 = rcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.Other && a.CE_EntryNum == "ABC Reference");
			AssertEquals(1, others1.Count());
			Helper.AssertAdditionalReference(others1.Single(), WarehouseAdditionalReferenceTypes.Codes.Other, "ABC Reference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var others2 = rcn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.Other && a.CE_EntryNum == "DEF Reference");
			AssertEquals(1, others2.Count());
			Helper.AssertAdditionalReference(others2.Single(), WarehouseAdditionalReferenceTypes.Codes.Other, "DEF Reference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
		}

		public void TestForwardingToTW_ReferenceMapper_RCNCustomsStatus()
		{
			var transitMapping = new TransitReferenceMappingConfiguration();
			var cocToCENMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				"COC",
				TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var amsToCUSMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				"AMS",
				TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var ubrToPRTMapping = Helper.CreateTransitReferenceMapping(
				TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				"UBR",
				TransitWarehouseReferenceCategories.Codes.PortReference,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority);
			transitMapping.TransitReferenceMappingCollection.Add(cocToCENMapping);
			transitMapping.TransitReferenceMappingCollection.Add(amsToCUSMapping);
			transitMapping.TransitReferenceMappingCollection.Add(ubrToPRTMapping);

			WarehouseDataRegistry.Instance.TransitReferenceMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transitMapping);

			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var cocReference = Helper.CreateAdditionalReference(shipment, "COC Reference", "COC");
			var amsReference = Helper.CreateAdditionalReference(shipment, "AMS Reference", "AMS");
			var ubrReference = Helper.CreateAdditionalReference(shipment, "UBR Reference", "UBR");
			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Drum, container, "");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			var newFactory = new BusinessObjectFactory();
			var rcn = AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipment.PK);
			var customsReferences = rcn.CustomsReferenceNumbers.Cast<CusEntryNumber>();
			var portReferences = rcn.PortReferences.Cast<CusEntryNumber>();
			AssertEquals(2, customsReferences.Count());
			AssertEquals(1, portReferences.Count());
			var customsNumber = customsReferences.Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var customsReleaseNumber = customsReferences.Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var portReference = portReferences.Single();
			Helper.AssertAdditionalReference(customsNumber, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "COC Reference", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Helper.AssertAdditionalReference(customsReleaseNumber, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "AMS Reference", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Helper.AssertAdditionalReference(portReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "UBR Reference", "", "", TransitWarehouseReferenceCategories.Codes.PortReference);
			AssertEquals("Customs Status is PAN", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, rcn.WRC_CustomsStatus);
		}

		#endregion

		#region TestSendingUXMLToTW_Stops_DispatchLoadList

		[TestDate(2018, 1, 1)]
		public void TestSendingUXMLToTWFromConsol_StopsLoadList_And_AddsSuspendEvent()
		{
			var testData = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLeg = CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Bag, container1);
			var packline2 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Case, container2);
			var packline3 = CreateOuterPackline(shipment, 3, Constants.PkgUnit.Drum);

			var differentRcn = Helper.CreateReceiveConsignment("DifferentRCN", "STD", testData.warehouse.PK);
			var differentDcn = Helper.CreateDispatchConsignment("DifferentDCN", testData.warehouse.PK);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments and packages
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUADL", rcn.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn, 3);

			AssertEquals("One receive ASN must be created for the entire consol.", 1, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			var asnPK = rcn.PackageStates.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct().Single();
			var receiveASNForShipmentPK = rcn.PackageStates.First().WPS_WRP_ReceiveExpectedPacking;
			AssertReceiveASN(newFactory.Load<WhsItemReceiveASN>(asnPK), consol.PK, "JK", "MSB1", outboundLeg);
			AssertEquals("No RTUs should be created for export", 0, newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);

			var packageStatesForShipment = rcn.PackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking == receiveASNForShipmentPK).OrderBy(ps => ps.Package.KP_F3_NKPackType).ToArray();
			var packageState1ForShipment = packageStatesForShipment[0];
			var packageState2ForShipment = packageStatesForShipment[1];
			var packageState3ForShipment = packageStatesForShipment[2];
			AssertPackage(packageState1ForShipment, Constants.PkgUnit.Bag);
			AssertPackage(packageState2ForShipment, Constants.PkgUnit.Case);
			AssertPackage(packageState3ForShipment, Constants.PkgUnit.Drum);

			// Send dispatch instruction to create Dispatch Consignment and Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListCreatedViaImport = AssertLoadList(newBizOFactoryAfterRunningLogWalker, "NZAKL", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, loadListCreatedViaImport.PK);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newBizOFactoryAfterRunningLogWalker.Load<StmALog>(suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist on Load List.", 0, suspendEvents.Length);

			// Start job on Load List
			loadListCreatedViaImport.WDL_IsReadyToStage = true;
			loadListCreatedViaImport.WDL_WL_StagingLocation = testData.warehouse.DefaultOutboundDockDoorLocation.PK;

			var packageState1ForShipmentAfterRunningLogWalker = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(packageState1ForShipment.PK);
			AssertEquals("Package is currently attached to Load List.", loadListCreatedViaImport.PK, packageState1ForShipmentAfterRunningLogWalker.WPS_WDL_LoadList);

			// detach package(BAG) from current rcn and dcn
			packageState1ForShipmentAfterRunningLogWalker.WPS_WRC_TransitReceiveConsignment = differentRcn.PK;
			packageState1ForShipmentAfterRunningLogWalker.WPS_WDC_TransitDispatchConsignment = differentDcn.PK;

			// delete packline(BAG) on shipment
			packline1.Delete();

			newBizOFactoryAfterRunningLogWalker.Save();

			// Resend dispatch instruction to stop Load List
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterReimportingDispatchInstruction = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReimportingDispatchInstruction = AssertLoadList(newBizOFactoryAfterReimportingDispatchInstruction, "NZAKL", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			var suspendEventAfterStoppingLoadList = loadListAfterReimportingDispatchInstruction.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			AssertEquals("Load List job has been stopped due to package being removed.", false, loadListAfterReimportingDispatchInstruction.WDL_IsReadyToStage);
			AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListAfterReimportingDispatchInstruction, false, Events.ServiceSuspendedCode, "DLL00000001|TYP=Load List|LOC=NZCHC|FAC=CFS|RES=Modifying via UXML|DEP=Forwarder|WHS=TRW");
		}

		[TestDate(2018, 1, 1)]
		public void TestSendingUXMLToTWFromConsol_UnloadHU_ResendDispatch()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: true, parentProcessIsConsol: true, createTemplate: false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: true);

			var consol = CreateConsol("MB1", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			consol.ReceivingForwarderWithContact.AddressFK = consignor.MainAddress.PK;

			var container = CreateContainer(consol, containerNum: "1", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.WDL_JobID, "DLL00000001")).Single();
			var dtu = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).SingleOrDefault();

			var packageStatesForShipment = rcn.PackageStates.OrderBy(ps => ps.Package.KP_F3_NKPackType).ToArray();
			var packageState = packageStatesForShipment[0];
			packageState.WPS_WDL_LoadList = dll.PK;
			Factory.Save();

			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Putaway, dtu: dtu, entryNum: shipment.JobNumber, dll: dll);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);
			handlingUnitPackage.WPS_WDL_LoadList = dll.PK;

			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultOutboundDockDoorLocation.PK;

			LoadPackage(handlingUnitPackage, warehouse.DefaultInboundDockDoorLocation, rtu, dtu, dll);
			Factory.Save();

			AssertEquals("default HU WPS_RemoveFromDTU should not remove ", false, handlingUnitPackage.WPS_RemoveFromDTU);

			// resend dispatch instruction
			TriggerAndFireTransitRequestForRelease(consol);

			handlingUnitPackage.Reload();
			AssertEquals("FreightLoaded HU should not remove from DTU by resend dispatch instruction", false, handlingUnitPackage.WPS_RemoveFromDTU);
		}

		[TestDate(2018, 1, 1)]
		public void TestSendingUXMLToTWFromShipment_StopsLoadList_And_AddsSuspendEvent()
		{
			var testData = CreateTestData(false, "NZCHC", false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));
			CreateTransport(consol, 2, "AIR", "B", "BB", "NZAKL", "AUSYD", testData.today.AddDays(2), testData.today.AddDays(4));
			var inboundLeg = CreateTransport(consol, 3, "AIR", "C", "CC", "AUSYD", "AUADL", testData.today.AddDays(6), testData.today.AddDays(8));

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Bag, container1);
			var packline2 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Case, container2);
			var packline3 = CreateOuterPackline(shipment, 3, Constants.PkgUnit.Drum);

			var differentRcn = Helper.CreateReceiveConsignment("DifferentRCN", "STD", testData.warehouse.PK);
			var differentDcn = Helper.CreateDispatchConsignment("DifferentDCN", testData.warehouse.PK);

			// Manually create Load List and to attach shipment packages
			var dispatchLoadListForPackages = Helper.CreateDispatchLoadList("DLL1", testData.warehouse.PK);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			// Receive consignments and packages
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn = AssertConsignment(newFactory, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn, "HSB1", "S00001000", "", "C00001000", "AA", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUADL", rcn.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn, 3);

			var packageStatesForShipment = rcn.PackageStates.OrderBy(ps => ps.Package.KP_F3_NKPackType).ToArray();
			var packageState1ForShipment = packageStatesForShipment[0];
			var packageState2ForShipment = packageStatesForShipment[1];
			var packageState3ForShipment = packageStatesForShipment[2];
			AssertPackage(packageState1ForShipment, Constants.PkgUnit.Bag);
			AssertPackage(packageState2ForShipment, Constants.PkgUnit.Case);
			AssertPackage(packageState3ForShipment, Constants.PkgUnit.Drum);

			// Attach packages to Load List
			packageState1ForShipment.WPS_WDL_LoadList = dispatchLoadListForPackages.PK;
			packageState2ForShipment.WPS_WDL_LoadList = dispatchLoadListForPackages.PK;
			packageState3ForShipment.WPS_WDL_LoadList = dispatchLoadListForPackages.PK;

			var loadListFromNewFactory = newFactory.Load<WhsItemDispatchLoadList>(dispatchLoadListForPackages.PK);
			AssertEquals("Suspend event has not been added yet.", 0, loadListFromNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Count());

			// Start job on Load List
			loadListFromNewFactory.WDL_IsReadyToStage = true;
			loadListFromNewFactory.WDL_WL_StagingLocation = testData.warehouse.DefaultOutboundDockDoorLocation.PK;

			newFactory.Save();

			// Send dispatch instruction to stop Load List
			TriggerAndFireTransitRequestForRelease(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterRunningLogWalker = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(dispatchLoadListForPackages.PK);
			var suspendEventAfterStoppingLoadList = loadListAfterRunningLogWalker.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			AssertEquals("Load List job has been stopped due to package being removed.", false, loadListAfterRunningLogWalker.WDL_IsReadyToStage);
			AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListAfterRunningLogWalker, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=NZCHC|FAC=CFS|RES=Modifying via UXML|DEP=Forwarder|WHS=TRW");
		}

		void AssertLoadListEvent(StmALog eventLog, WhsItemDispatchLoadList loadList, bool isCancelled, string eventCode, string reference)
		{
			AssertEquals(loadList.TableName, eventLog.SL_Table);
			AssertEquals(loadList.PK, eventLog.SL_Parent);
			AssertEquals(isCancelled, eventLog.IsCancelled);
			AssertEquals(eventCode, eventLog.SL_SE_NKEvent);
			AssertEquals(reference, eventLog.SL_Reference);
		}

		#endregion

		#region TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenImport_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices()
			=> TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenExport_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices()
			=> TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices(isArrivalTransitWarehouse: false);

		void TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_CreateAdditionalServices(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: false);

			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment1.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment1.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;
			var shipment2 = CreateShipment("HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment2.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);

			var service1 = CreateAdditionalService(shipment1, "CLN", warehouse.WW_OA_WarehouseAddress);
			var service2 = CreateAdditionalService(shipment2, "CLN", warehouse.WW_OA_WarehouseAddress);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment2);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1InNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment1.PK);
			AssertAdditionalService(rcn1InNewFactory, service1);
			var rcn2InNewFactory = AssertAndReturnReceiveConsignment(newFactory, "HSB2", warehouse, shipment2.PK);
			AssertAdditionalService(rcn2InNewFactory, service2);

			var service3 = CreateAdditionalService(shipment1, "WSH", warehouse.WW_OA_WarehouseAddress);
			var service4 = CreateAdditionalService(shipment2, "WSH", warehouse.WW_OA_WarehouseAddress);

			TriggerAndFireTransitRequestForRelease(shipment1);
			TriggerAndFireTransitRequestForRelease(shipment2);

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1InNewFactory2 = AssertAndReturnReceiveConsignment(newFactory2, "HSB1", warehouse, shipment1.PK);
			var dcn1InNewFactory2 = newFactory2.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, shipment1.PK));
			AssertAdditionalService(dcn1InNewFactory2, service3);
			var dcn2InNewFactory2 = newFactory2.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, shipment2.PK));
			AssertAdditionalService(dcn2InNewFactory2, service4);

			// Set one shipment to completed (which should be ignored when updating services)
			rcn1InNewFactory2.WRC_CompleteTime = DateTime.Now;
			dcn1InNewFactory2.WDC_CompleteTime = DateTime.Now;
			newFactory2.Save();

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			if (isArrivalTransitWarehouse)
			{
				CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			}
			else
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			}

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory3 = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn1InNewFactory3 = AssertAndReturnReceiveConsignment(newFactory3, "HSB1", warehouse, shipment1.PK);
			AssertAdditionalService(rcn1InNewFactory3, service1);
			AssertAdditionalService(rcn1InNewFactory3, service3);
			var rcn2InNewFactory3 = AssertAndReturnReceiveConsignment(newFactory3, "HSB2", warehouse, shipment2.PK);
			AssertAdditionalService(rcn2InNewFactory3, service2);
			AssertAdditionalService(rcn2InNewFactory3, service4);

			var service5 = CreateAdditionalService(shipment1, "CLN", warehouse.WW_OA_WarehouseAddress);
			service5.ES_ServiceId = "CLN001";
			var service6 = CreateAdditionalService(shipment2, "CLN", warehouse.WW_OA_WarehouseAddress);
			service6.ES_ServiceId = "CLN002";
			newFactory3.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment2);

			var newFactory4 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1InNewFactory4 = AssertAndReturnReceiveConsignment(newFactory4, "HSB1", warehouse, shipment1.PK);
			AssertAdditionalService(rcn1InNewFactory4, service5);
			var rcn2InNewFactory4 = AssertAndReturnReceiveConsignment(newFactory4, "HSB2", warehouse, shipment2.PK);
			AssertAdditionalService(rcn2InNewFactory4, service6);

			TriggerAndFireTransitRequestForRelease(consol);

			var newFactory5 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcn1InNewFactory5 = newFactory5.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, shipment1.PK));
			AssertEquals("The service count is 0", 0, dcn1InNewFactory5.Services.Count);
			var dcn2InNewFactory5 = newFactory5.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, shipment2.PK));
			AssertEquals("The service count is 0", 0, dcn2InNewFactory5.Services.Count);
		}

		#endregion

		#region TestForwardingSendsShipments_TWP

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenImport_SendTWP_ThenSendTWD()
			=> TestForwardingSendsShipments_SendTWP_ThenSendTWD(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenExport_SendTWP_ThenSendTWD()
			=> TestForwardingSendsShipments_SendTWP_ThenSendTWD(isArrivalTransitWarehouse: false);

		void TestForwardingSendsShipments_SendTWP_ThenSendTWD(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: false, createTemplate: false);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingPendingCode, "TWR");
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingRequestedCode, "TWP");
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingConfirmedCode, "TWD");
			newFactory.Save();

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingPending(shipment);
			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			var dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is False", !dcnInNewFactory.WDC_IsAuthorizedForDispatch);

			TriggerAndFireTransitRequestForRelease(shipment);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is True", dcnInNewFactory.WDC_IsAuthorizedForDispatch);
		}

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenImport_SendTWD_ThenSendTWP()
			=> TestForwardingSendsShipments_SendTWD_ThenSendTWP(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsShipments_WhenExport_SendTWD_ThenSendTWP()
			=> TestForwardingSendsShipments_SendTWD_ThenSendTWP(isArrivalTransitWarehouse: false);

		void TestForwardingSendsShipments_SendTWD_ThenSendTWP(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: false, createTemplate: false);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingRequestedCode, "TWR");
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingRequestedCode, "TWD");
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingConfirmedCode, "TWP");
			newFactory.Save();

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = isArrivalTransitWarehouse ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = isArrivalTransitWarehouse ? cfs.MainAddress.PK : ZGuid.Empty;

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			var dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is True", dcnInNewFactory.WDC_IsAuthorizedForDispatch);

			TriggerAndFireTransitRequestForRelease(shipment);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is True", dcnInNewFactory.WDC_IsAuthorizedForDispatch);
		}

		#endregion

		#region TestForwardingSendsConsol_TWP

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_WhenImport_SendTWR_ThenSendTWP()
			=> TestForwardingSendsConsol_SendTWR_ThenSendTWP(isArrivalTransitWarehouse: true);

		[ExpectNoExceptions]
		[TestDate(2018, 1, 1)]
		public void TestForwardingSendsConsol_WhenExport_SendTWR_ThenSendTWP()
			=> TestForwardingSendsConsol_SendTWR_ThenSendTWP(isArrivalTransitWarehouse: false);

		void TestForwardingSendsConsol_SendTWR_ThenSendTWP(bool isArrivalTransitWarehouse)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalTransitWarehouse, parentProcessIsConsol: true, createTemplate: false);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingRequestedCode, "TWR");
			CreateWorkflow(template, isArrivalTransitWarehouse ? "ATW" : "DTW", Events.BookingConfirmedCode, "TWP");
			newFactory.Save();

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			if (isArrivalTransitWarehouse)
			{
				consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			}

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestForRelease(consol);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			var dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is False", !dcnInNewFactory.WDC_IsAuthorizedForDispatch);
		}

		public void TestForwardingSendsConsol_SendTWR_ThenSendTWPOnTwoShipments_OneDLLCreated()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: true, createTemplate: false);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			CreateWorkflow(template, "ATW", Events.BookingRequestedCode, "TWR");
			CreateWorkflow(template, "ATW", Events.BookingConfirmedCode, "TWP");

			newFactory.Save();

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);

			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package);

			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertAndReturnReceiveConsignment(newFactory, "HSB1", warehouse, shipment.PK);
			var dcnInNewFactory = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());
			Assert("Authorized For Dispatch is False", !dcnInNewFactory.WDC_IsAuthorizedForDispatch);

			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment2.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Package);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			AssertAndReturnReceiveConsignment(newFactory, "HSB2", warehouse, shipment2.PK);
			var dcnsInNewFactory = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			foreach (var dcn in dcnsInNewFactory)
			{
				Assert("Authorized For Dispatch is False", !dcn.WDC_IsAuthorizedForDispatch);
			}
			var dllInNewFactory = newFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Only one DLL created", 1, dllInNewFactory.Length);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_LoadButNotComplete_NoDuplicateDTU()
		{
			TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU(false);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_LoadAndComplete_NoDuplicateDTU()
		{
			TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU(true);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU(bool loadComplete)
		{
			var testData = CreateTestData(true);

			var destinationDepot = Factory.LoadTop1<OrgAddress>(new ZQuery());

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.JS_OA_ImportReleaseDepot = destinationDepot.PK;
			var packLine1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Carton);

			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var packLine2 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Carton);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			var packageStateForRCN1 = rcn1.PackageStates.Single();
			var packageStateP1 = UnloadAndLabelPackage(packageStateForRCN1, rtu, "P1");
			var packageStateP2 = UnloadAndLabelPackage(packageStateForRCN1, rtu, "P2");

			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipment2.PK);
			var packageStateForRCN2 = rcn2.PackageStates.Single();
			var packageStateP3 = UnloadAndLabelPackage(packageStateForRCN2, rtu, "P3");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(rcn1);
			TriggerAndFireOutturn(rcn2);
			Factory.Save();

			packLine1.Reload();
			AssertNotNull(packLine1);
			AssertEquals("RCV", packLine1.JL_LastKnownTransitWarehouseStatus);

			packLine2.Reload();
			AssertNotNull(packLine2);
			AssertEquals("RCV", packLine2.JL_LastKnownTransitWarehouseStatus);

			var notifier = new NotificationsForTest();
			var helper = new TransitWarehouseInstructionHelper(consol, notifier,
				factory =>
				{
					var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Both);
					var shipmentsForSelection = consolForPrepareDispatch.ShipmentsForSelection;
					shipmentsForSelection[0].SelectedForDelivery = true;
					shipmentsForSelection[1].SelectedForDelivery = false;
					return new ManualDataExport(factory, new[] { consol }, UniversalDataType.UniversalShipment, null, null, null,
						manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatch));
				},
				new ITransitWarehouseInstructionSupporter[] { shipment1, shipment2 });
			AssertNoExceptionThrown(() => helper.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch));
			Factory.Save();

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", testData.warehouse.PK);

			var dcnsInNewFactory = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Only one DCN created", 1, dcnsInNewFactory.Length);
			foreach (var dcn in dcnsInNewFactory)
			{
				Assert("Authorized For Dispatch is False", !dcn.WDC_IsAuthorizedForDispatch);
			}

			var dllInNewFactory = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Only one DLL created", 1, dllInNewFactory.Length);

			var dtu1 = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Only one DTU present", 1, dtu1.Length);

			LoadPackage(packageStateP1, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu, dllInNewFactory[0], dcnsInNewFactory[0]);
			LoadPackage(packageStateP2, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu, dllInNewFactory[0], dcnsInNewFactory[0]);

			if (loadComplete)
			{
				dllInNewFactory[0].WDL_CompleteTime = DateTime.Now;
			}

			TriggerAndFireOutturn(dcnsInNewFactory[0]);

			var helper1 = new TransitWarehouseInstructionHelper(consol, notifier,
				factory =>
				{
					var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Both);
					var shipmentsForSelection = consolForPrepareDispatch.ShipmentsForSelection;
					shipmentsForSelection[0].SelectedForDelivery = false;
					shipmentsForSelection[1].SelectedForDelivery = true;
					return new ManualDataExport(factory, new[] { consol }, UniversalDataType.UniversalShipment, null, null, null,
						manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatch));
				},
				new ITransitWarehouseInstructionSupporter[] { shipment1, shipment2 });
			AssertNoExceptionThrown(() => helper1.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch));
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			dcnsInNewFactory = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			LoadPackage(packageStateP3, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu, dllInNewFactory[0], dcnsInNewFactory[0]);
			TriggerAndFireOutturn(dcnsInNewFactory[0]);

			var finalDTU = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Still Only one DTU present", 1, finalDTU.Length);
			AssertEquals("finalDTU matches the DTU already present", finalDTU[0].PK, dtu1[0].PK);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_ContaineroOrULD_LoadButNotComplete_NoDuplicateDTU()
		{
			TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU_ContainerOrULD(false);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_ContainerOrULD_LoadAndComplete_NoDuplicateDTU()
		{
			TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU_ContainerOrULD(true);
		}

		public void TestForwardingSendsConsolWithTwoShipments_SendNeededInstructions_NoDuplicateDTU_ContainerOrULD(bool loadComplete)
		{
			var testData = CreateTestDataForCombined();

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);

			var destinationDepot = Factory.LoadTop1<OrgAddress>(new ZQuery());

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment1.JS_OA_ImportReleaseDepot = destinationDepot.PK;

			var shipment2 = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);

			var container = CreateContainer(consol, containerNum: "A", containerCount: 1, containerTypeCode: "20GP");

			CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet, reference: "2PLTs", container: container);
			CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, reference: "1PLTs", container: container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment1.PK);
			var packageStateInRCN1 = rcn1.PackageStates.Single();
			var packageStateP1 = UnloadAndLabelPackage(packageStateInRCN1, rtu, "P1");
			var packageStateP2 = UnloadAndLabelPackage(packageStateInRCN1, rtu, "P2");

			var rcn2 = AssertAndReturnReceiveConsignment(Factory, "HSB2", testData.warehouse, shipment2.PK);
			var packageStateInRCN2 = rcn2.PackageStates.Single();
			var packageStateP3 = UnloadAndLabelPackage(packageStateInRCN2, rtu, "P3");

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(rcn1);
			TriggerAndFireOutturn(rcn2);
			Factory.Save();

			var notifier = new NotificationsForTest();
			var helper = new TransitWarehouseInstructionHelper(consol, notifier,
				factory =>
				{
					var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Both);
					var shipmentsForSelection = consolForPrepareDispatch.ShipmentsForSelection;
					shipmentsForSelection[0].SelectedForDelivery = true;
					shipmentsForSelection[1].SelectedForDelivery = false;
					return new ManualDataExport(factory, new[] { consol }, UniversalDataType.UniversalShipment, null, null, null,
						manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatch));
				},
				new ITransitWarehouseInstructionSupporter[] { shipment1, shipment2 });
			AssertNoExceptionThrown(() => helper.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch));
			Factory.Save();

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dcns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("No DCNs are created since there is an unallocated packline.", 2, dcns.Length);

			var dtu1 = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("No DTUs are created since there is an unallocated packline.", 1, dtu1.Length);

			var dll = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("No load lists must be created since there is an unallocated packline.", 1, dll.Length);

			LoadPackage(packageStateP1, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu1[0], dll[0], dcns[0]);
			LoadPackage(packageStateP2, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu1[0], dll[0], dcns[0]);

			if (loadComplete)
			{
				dll[0].WDL_CompleteTime = DateTime.Now;
			}

			TriggerAndFireOutturn(dcns[0]);

			var helper1 = new TransitWarehouseInstructionHelper(consol, notifier,
				factory =>
				{
					var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Both);
					var shipmentsForSelection = consolForPrepareDispatch.ShipmentsForSelection;
					shipmentsForSelection[0].SelectedForDelivery = false;
					shipmentsForSelection[1].SelectedForDelivery = true;
					return new ManualDataExport(factory, new[] { consol }, UniversalDataType.UniversalShipment, null, null, null,
						manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatch));
				},
				new ITransitWarehouseInstructionSupporter[] { shipment1, shipment2 });
			AssertNoExceptionThrown(() => helper1.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch));
			Factory.Save();

			LoadPackage(packageStateP3, testData.warehouse.DefaultInboundDockDoorLocation, rtu, dtu1[0], dll[0], dcns[1]);
			TriggerAndFireOutturn(dcns[1]);

			var finalDTU = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Still Only one DTU present", 1, finalDTU.Length);
			AssertEquals("finalDTU matches the DTU already present", finalDTU[0].PK, dtu1[0].PK);
		}

		#endregion

		#region TestResendDispatchInstructionsWithAndWithoutIDs

		public void TestResendDispatchInstructionsWithAndWithoutIDs()
		{
			var testData = CreateTestData(true);
			// Consol w/
			//   1 shipment
			//   1 Container Without ID
			//   1 Container With ID and Resend
			//   3 Unallocated Pack Lines
			//   route : NZAKL -> AUSYD
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var containerWithoutID = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT2");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT3");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadList.WDL_ParentID);
			AssertEquals("", loadList.DispatchTransportationUnits.Single().ContainerNumber);

			var containerWithID = CreateContainer(consol, containerNum: "CNT1", containerCount: 1, containerTypeCode: "20GP");

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterAddingContainerWithID = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListAfterAddingContainerWithID = newBizOFactoryAfterAddingContainerWithID.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadListAfterAddingContainerWithID.WDL_ParentID);
			var dtus = loadListAfterAddingContainerWithID.DispatchTransportationUnits;
			AssertEquals(2, dtus.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "", "CNT1" }, dtus.Select(d => d.ContainerNumber));
		}

		public void TestResendDispatchInstructionsWithAndWithoutIDs_AllocatedPackLinesWithIDs()
		{
			var testData = CreateTestData(true);
			// Consol w/
			//   1 shipment
			//   1 Container Without ID
			//     3 Allocated Pack Lines
			//   Add 1 Container With ID and resend
			//     1 Allocated Pack Line
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var containerWithoutID = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT1", container: containerWithoutID);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT2", container: containerWithoutID);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT3", container: containerWithoutID);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState4 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PLT4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadList.WDL_ParentID);
			AssertEquals("", loadList.DispatchTransportationUnits.Single().ContainerNumber);

			var containerWithID = CreateContainer(consol, containerNum: "CNT1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PLT4", container: containerWithID);

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterAddingContainerWithID = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListsAfterAddingContainerWithID = newBizOFactoryAfterAddingContainerWithID.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadListsAfterAddingContainerWithID.Length);
			var loadListWithoutContainerID = loadListsAfterAddingContainerWithID.Single(l => l.DispatchTransportationUnits.Single().ContainerNumber == "");
			var loadListWithContainerID = loadListsAfterAddingContainerWithID.Single(l => l.DispatchTransportationUnits.Single().ContainerNumber == "CNT1");
			AssertEquals("Correct Package is assigned to Load list for Container without ID", "PLT4", loadListWithContainerID.PackageStates.Single().Package.KP_PackageID);
			AssertContainsExactElementsInAnyOrder("Correct Packages are assigned to Load list for Container with ID",
				new[] { "PLT1", "PLT2", "PLT3" },
				loadListWithoutContainerID.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertContainsExactElementsInAnyOrder(new[] { "", "CNT1" }, loadListsAfterAddingContainerWithID.Select(l => l.DispatchTransportationUnits.Single().ContainerNumber));

			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadListWithoutContainerID.WDL_ParentID);
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadListWithContainerID.WDL_ParentID);
		}

		public void TestSendDispatchInstructionsWithAndWithoutIDs_AddContainerWithIDAndResend()
		{
			var testData = CreateTestData(true);
			// Consol w/
			//   1 shipment
			//   New Container has an ID but existing container does not.
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var containerWithoutID = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "", container: containerWithoutID);
			var unAllocatedPackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadList.WDL_ParentID);
			AssertEquals("", loadList.DispatchTransportationUnits.Single().ContainerNumber);

			// Split container group to add a container with ID and add a new container without ID
			var containerWithID = CreateContainer(consol, containerNum: "CNT1", containerCount: 1, containerTypeCode: "20GP");
			unAllocatedPackLine.JL_JC = containerWithID.PK;

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterAddingContainerWithID = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListAfterAddingContainerWithID = newBizOFactoryAfterAddingContainerWithID.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadListAfterAddingContainerWithID.WDL_ParentID);
			var dtus = loadListAfterAddingContainerWithID.DispatchTransportationUnits;
			AssertEquals(2, dtus.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "", "CNT1" }, dtus.Select(d => d.ContainerNumber));
		}

		public void TestSendDispatchInstructionsWithAndWithoutIDs_UpdateContainerIDAndResend()
		{
			var testData = CreateTestData(true);
			// Consol w/
			//   1 shipment
			//   Split Container Group and Resend
			//   New Container does not have an ID but splitted one does.
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var containerGroup = CreateContainer(consol, containerNum: "", containerCount: 2, containerTypeCode: "20GP");

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, reference: "");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadList.WDL_ParentID);
			AssertEquals(2, loadList.DispatchTransportationUnits.Count);
			AssertEquals("", loadList.DispatchTransportationUnits.Select(d => d.ContainerNumber).Distinct().Single());

			// Split container group to add a container with ID and add a new container without ID
			containerGroup.JC_ContainerNum = "CNT1";
			containerGroup.JC_ContainerCount = 1;
			var containerWithoutID = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterAddingContainerWithID = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadListAfterAddingContainerWithID = newBizOFactoryAfterAddingContainerWithID.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK must be Consol.", consol.PK, loadListAfterAddingContainerWithID.WDL_ParentID);
			var dtus = loadListAfterAddingContainerWithID.DispatchTransportationUnits;
			AssertEquals(2, dtus.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "", "CNT1" }, dtus.Select(d => d.ContainerNumber));
		}

		#endregion

		#region TestTwoConsolsSharingSingleContainer

		public void TestTwoConsolsSharingSingleContainer()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1 - SAMECONTAINERNUMBER
					Shipment A HSB1
						PKG1 PLT 1

				Consol B
					Container 2 - SAMECONTAINERNUMBER
					Shipment B HSB2
						PKG2 PLT 1

			2. Send receive instructions and dispatch instructions for both consols
			3. Two Load Lists created and single DTU is created. Both Load Lists point to the same DTU.
			 */
			var testData = CreateTestData(true);

			var consolA = CreateConsol(masterBill: "A", testData.vessel, "NZAKL", "AUSYD");
			consolA.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var shipmentA = CreateShipment(consolA, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container1WithSameContainerNumber = CreateContainer(consolA, containerNum: "SameContainerNumber", containerCount: 1, containerTypeCode: "20GP");
			var packlineInContainer1 = CreateOuterPackline(shipmentA, 1, Constants.PkgUnit.Pallet, container1WithSameContainerNumber, "PKG1");

			var consolB = CreateConsol(masterBill: "B", testData.vessel, "NZAKL", "AUSYD");
			consolB.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var shipmentB = CreateShipment(consolB, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container2WithSameContainerNumber = CreateContainer(consolB, containerNum: "SameContainerNumber", containerCount: 1, containerTypeCode: "20GP");
			var packlineInContainer2 = CreateOuterPackline(shipmentB, 1, Constants.PkgUnit.Pallet, container1WithSameContainerNumber, "PKG2");

			// send receive instructions
			TriggerAndFireTransitRequestUsingBookingRequested(consolA);
			TriggerAndFireTransitRequestUsingBookingRequested(consolB);
			var newFactory = new BusinessObjectFactory();
			AssertAndReturnReceiveConsignment(newFactory, "HSB1", testData.warehouse, shipmentA.PK);
			AssertAndReturnReceiveConsignment(newFactory, "HSB2", testData.warehouse, shipmentB.PK);

			// send dispatch instructions
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consolA);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consolB);

			var newFactoryAfterSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuForContainer = newFactoryAfterSendingDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Single DTU with correct container number must be created.", "SAMECONTAINERNUMBER", dtuForContainer.ContainerNumber.ToUpper());

			var loadListForConsolA = dtuForContainer.DispatchLoadLists.Single(l => l.MasterBillNumber == "A");
			var loadListForConsolB = dtuForContainer.DispatchLoadLists.Single(l => l.MasterBillNumber == "B");
			AssertEquals("Load List for Consol A has the correct Package.", "PKG1", loadListForConsolA.PackageStates.Single().Package.KP_PackageID);
			AssertEquals("Load List for Consol B has the correct Package.", "PKG2", loadListForConsolB.PackageStates.Single().Package.KP_PackageID);
			AssertEquals("Load List for Consol A's package belong to the correct DCN.", "HSB1", loadListForConsolA.PackageStates.Single().DispatchConsignment.WDC_ConsignmentID);
			AssertEquals("Load List for Consol B's package belong to the correct DCN.", "HSB2", loadListForConsolB.PackageStates.Single().DispatchConsignment.WDC_ConsignmentID);
		}

		#endregion

		#region TestForwardingToTW_BookingParty

		public void TestForwardingToTW_BookingParty_DepartureCFS()
		{
			var testData = CreateTestData(false);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: false);

			var sendingAgent = TestHelper.CreateOrganisation("SENDER");
			var consol = CreateConsol(masterBill: "A", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "CONT1", containerCount: 1, containerTypeCode: "20GP");
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			var newFactory = new BusinessObjectFactory();
			var receiveConsignments = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: One receive consignment should be created.", 1, receiveConsignments.Length);

			var rcn = receiveConsignments.Single();
			AssertEquals(sendingAgent.OH_Code, rcn.BookingPartyDocAddress.Organisation.OH_Code);

			var receiveASNs = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Precondition: One receive ASN should be created.", 1, receiveASNs.Length);
			var receiveASN = receiveASNs.Single();
			AssertEquals(sendingAgent.OH_Code, receiveASN.BookingPartyDocAddress.Organisation.OH_Code);

			var dispatchConsignments = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: One dispatch consignment should be created.", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single();
			AssertEquals(sendingAgent.OH_Code, dcn.BookingPartyDocAddress.Organisation.OH_Code);
		}

		public void TestForwardingToTW_BookingParty_ArrivalCFS()
		{
			var testData = CreateTestData(true);

			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: true);

			var receivingAgent = TestHelper.CreateOrganisation("RECEIVER");
			var consol = CreateConsol(masterBill: "A", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "CONT1", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			var newFactory = new BusinessObjectFactory();

			var receiveConsignments = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition: One receive consignment should be created.", 1, receiveConsignments.Length);
			var rcn = receiveConsignments.Single();
			AssertEquals(receivingAgent.OH_Code, rcn.BookingPartyDocAddress.Organisation.OH_Code);

			var receiveASNs = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Precondition: One receive ASN should be created.", 1, receiveASNs.Length);
			var receiveASN = receiveASNs.Single();
			AssertEquals(receivingAgent.OH_Code, receiveASN.BookingPartyDocAddress.Organisation.OH_Code);

			var dispatchConsignments = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Precondition: One dispatch consignment should be created.", 1, dispatchConsignments.Length);
			var dcn = dispatchConsignments.Single();
			AssertEquals(receivingAgent.OH_Code, dcn.BookingPartyDocAddress.Organisation.OH_Code);
		}

		#endregion

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_ImportFailedAndLogMessage_Typical_ThrowErrorUsingRegistry()
		{
			TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_ImportFailedAndLogMessage_TypicalCore(true);
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_ImportFailedAndLogMessage_Typical_NoErrorUsingExternalReferenceToMatch()
		{
			TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_ImportFailedAndLogMessage_TypicalCore(false);
		}

		void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_ImportFailedAndLogMessage_TypicalCore(bool clearExternalReference)
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				var consol = SendDispatchInstructionWithPacklineDiscrepancy(testData, clearExternalReference);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadlists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
				if (clearExternalReference)
				{
					AssertEquals("Should not create load list.", 0, loadlists.Length);

					var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
					var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
					var importNote = declinedMessage.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
					AssertContains("Note must contain warning note of packline discrepancy.",
		@"Could not import Dispatch Instruction because RCN Packline Quantity and Pack Type discrepancy", importNote?.ST_NoteText);
				}
				else
				{
					AssertEquals("Should create load list.", 1, loadlists.Length);
					var note = loadlists.Single().Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
					AssertNull(note);
				}
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_Count_ShowWarningUsingRegistry()
		{
			TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_CountCore(true);
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_Count_NoWarningUsingExternalReferenceToMatch()
		{
			TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_CountCore(false);
		}

		void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_CountCore(bool clearExternalReference)
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				var consol = SendDispatchInstructionWithPacklineDiscrepancy(testData, clearExternalReference);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
				var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
				AssertNull("Should not decline the import.", declinedMessage);

				var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
				var note = loadList.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
				if (clearExternalReference)
				{
					AssertContains("Note must contain warning note of packline discrepancy.",
@"Warning :

The following DCNs were created but the Package Type and Qty is not fully matched with the RCN's packline when imported.
HSB1", note.ST_NoteText);
				}
				else
				{
					AssertNull(note);
				}
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_DifferentTypeCountButSameTotalQty_LogMessageAndAddNote_Count()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
				var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
				consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
				CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				var container = CreateContainer(consol, "CONT1");

				var packline1 = CreateOuterPackline(shipment1, 5, Constants.PkgUnit.Box, container);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				ClearExternalReference();

				CreateOuterPackline(shipment1, 3, Constants.PkgUnit.Crate, container);
				packline1.JL_PackageCount = 2;
				packline1.JL_F3_NKPackType = Constants.PkgUnit.Package;

				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
				var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
				AssertNull("Should not decline the import.", declinedMessage);

				var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
				var note = loadList.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
				AssertContains("Note must contain warning note of packline discrepancy.",
@"Warning :

The following DCNs were created but the Package Type and Qty is not fully matched with the RCN's packline when imported.
HSB1", note.ST_NoteText);
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_LogMessageAndAddNote_HBL()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				var consol = SendDispatchInstructionWithPacklineDiscrepancy(testData, true);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
				var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
				AssertNull("Should not decline the import.", declinedMessage);

				var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
				var note = loadList.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
				AssertContains("Note must contain warning note of packline discrepancy.",
@"Warning :

The following DCNs were created but the Package Type and Qty is not fully matched with the RCN's packline when imported.
HSB1", note.ST_NoteText);
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithPacklineDiscrepancy_PackTypeAndCountDifferent_LogMessageAndAddNote_HBL()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
				var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
				consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
				CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				var container = CreateContainer(consol, "CONT1");

				var packline1 = CreateOuterPackline(shipment1, 5, Constants.PkgUnit.Box, container);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				ClearExternalReference();

				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Constants.PkgUnit.Coil;

				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
				var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
				AssertNull("Should not decline the import.", declinedMessage);

				var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
				var note = loadList.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
				AssertContains("Note must contain warning note of packline discrepancy.",
@"Warning :

The following DCNs were created but the Package Type and Qty is not fully matched with the RCN's packline when imported.
HSB1", note.ST_NoteText);
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithNoRCNMatched_LogMessageAndAddNoteIfEnableRegistry()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
				var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
				consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
				CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

				var shipmentWithMatchedRCN = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var container = CreateContainer(consol, "CONT1");
				CreateOuterPackline(shipmentWithMatchedRCN, 5, Constants.PkgUnit.Box, container);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var shipmentWithoutMatchedRCN = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				CreateOuterPackline(shipmentWithoutMatchedRCN, 3, Constants.PkgUnit.Box, container);
				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
				var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
				AssertNull("Should not decline the import.", declinedMessage);

				var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
				var note = loadList.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
				AssertContains("Note must contain warning note of no RCN matched.",
@"Warning :

The following DCNs were not created because packages couldn't be found to attach.
HSB2", note.ST_NoteText);
			}
		}

		[TestDate(2020, 04, 30)]
		public void TestImportConsol_SendDispatchInstructionWithNoRCNMatched_Reject()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

			var shipmentWithMatchedRCN = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipmentWithMatchedRCN, 5, Constants.PkgUnit.Box, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var shipmentWithoutMatchedRCN = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			CreateOuterPackline(shipmentWithoutMatchedRCN, 3, Constants.PkgUnit.Box, container);
			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
			AssertNotNull("Should decline the import.", declinedMessage);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should not create loadlist", 0, loadLists.Length);
		}

		ForwardingConsol SendDispatchInstructionWithPacklineDiscrepancy((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, bool clearExternalReference = false)
		{
			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container = CreateContainer(consol, "CONT1");

			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container);
			var packline2 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Package, container);
			var packline3 = CreateOuterPackline(shipment1, 3, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			if (clearExternalReference)
			{
				ClearExternalReference();
			}

			packline1.JL_PackageCount = 3;
			packline3.JL_PackageCount = 1;

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);
			return consol;
		}

		void ClearExternalReference()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var packages = newFactory.Load<PkgPackage>(new ZQuery());
			foreach (var package in packages)
			{
				package.KP_ExternalReference = string.Empty;
			}
			newFactory.Save();
		}

		#region TestPopulatePackageStateSecurityStatus

		//[TestDate(2024, 02, 24)]
		//public void TestPopulatePackageStateSecurityStatus_WhenSendReceiveInstruction()
		//{
		//	var testData = CreateTestDataForCombined("NZCHC");
		//	var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
		//	using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
		//	{
		//		CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
		//		var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL", transportMode: "AIR");
		//		consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
		//		CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

		//		var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

		//		var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, reference: "P1", isHighRisk: true);

		//		TriggerAndFireTransitRequestUsingBookingRequested(consol);

		//		var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

		//		var packageState = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(new ZQuery()).FirstOrDefault();
		//		AssertEquals("Security Status is HRS", TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening, packageState.WPS_SecurityStatus);
		//		AssertEquals("HighRisk is true", true, packageState.WPS_IsHighRisk);
		//	}
		//}

		[TestDate(2024, 02, 24)]
		public void TestPopulatePackageStateSecurityStatus_WhenSendDispatchInstruction()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			testData.warehouse.WW_TransitSecurityProcessingRequired = true;
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
				var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL", transportMode: "AIR");
				consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
				CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, reference: "P1");

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var newBizOFactoryAfterRunningLogWalker1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var packageState1 = newBizOFactoryAfterRunningLogWalker1.Load<WhsItemPackageState>(new ZQuery()).FirstOrDefault();
				AssertEquals("Security Status is REQ", TransitWarehouseSecurityStatuses.Codes.Required, packageState1.WPS_SecurityStatus);

				Helper.CreatePackageScreening(packageState1.Package, "XRY", true);
				var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);

				UnloadAndLabelPackage(packageState1, rtu, "P1");
				Factory.Save();
				newBizOFactoryAfterRunningLogWalker1.Save();

				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryAfterRunningLogWalker2 = new BusinessObjectFactory() { RefreshEnabled = false };
				packageState1 = newBizOFactoryAfterRunningLogWalker2.Load<WhsItemPackageState>(new ZQuery()).FirstOrDefault();
				AssertEquals("Security Status is Screened", TransitWarehouseSecurityStatuses.Codes.Screened, packageState1.WPS_SecurityStatus);
			}
		}

		#endregion

		#region Populate CTO Address

		public void TestPopulateCTOAddress()
		{
			var testData = CreateTestData(true);
			var arrivalCTO = TestHelper.CreateOrganisation("ORG1", address1: "Arrival CTO Address");
			var departureCTO = TestHelper.CreateOrganisation("ORG2", address1: "Departure CTO Address");

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var shipment = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var jobDocAddresses = newFactory.Load<JobDocAddress>(new ZQuery());
			AssertNotNull(jobDocAddresses);

			var arrivalCTOJobDocAddresses = jobDocAddresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.ArrivalCTOAddress).ToList();
			AssertEquals(1, arrivalCTOJobDocAddresses.Count);

			var arrivalCTOJobDocAddress = arrivalCTOJobDocAddresses.Single();
			AssertEquals("RCN CTO Address should not be null.", WhsItemReceiveConsignmentSchema.Constants.Prefix, arrivalCTOJobDocAddress.E2_ParentTableCode);
			AssertEquals("RCN CTO Address should be consol Arrival CTO Address.", arrivalCTO.MainAddress.PK, arrivalCTOJobDocAddress.E2_OA_Address);

			var departureCTOJobDocAddresses = jobDocAddresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.DepartureCTOAddress).ToList();
			AssertEquals(1, departureCTOJobDocAddresses.Count);

			var departureCTOJobDocAddress = departureCTOJobDocAddresses.Single();
			AssertEquals("DCN CTO Address should not be null.", WhsItemDispatchConsignmentSchema.Constants.Prefix, departureCTOJobDocAddress.E2_ParentTableCode);
			AssertEquals("DCN CTO Address should be consol Departure CTO Address.", departureCTO.MainAddress.PK, departureCTOJobDocAddress.E2_OA_Address);
		}

		#endregion

		#region Populate CRB Address

		public void TestPopulateCRBAddressWhenRegistryValueIsLCN() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.LCN);

		public void TestPopulateCRBAddressWhenRegistryValueIsCED() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED);

		public void TestPopulateCRBAddressWhenRegistryValueIsSFA() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.SFA);

		public void TestPopulateCRBAddressWhenRegistryValueIsRFA() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.RFA);

		public void TestPopulateCRBAddressWhenRegistryValueIsBKD() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD);

		public void TestPopulateCRBAddressWhenRegistryValueIsNON() => TestPopulateCRBAddressCore(DefaultBilltoPartyForTWConsignmentCodeList.Codes.Non);

		void TestPopulateCRBAddressCore(string addressType)
		{
			var testData = CreateTestData(true);
			var sendingOrg = TestHelper.CreateOrganisation("ORGS", address1: "Sending Organisation Address");
			var receivingOrg = TestHelper.CreateOrganisation("ORGR", address1: "Receiving Organisation Address");
			var localClientOrg = TestHelper.CreateOrganisation("ORGL", address1: "Local Client Organisation Address");

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingOrg.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingOrg.MainAddress.PK;

			var shipment = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = localClientOrg.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = testData.consignee.MainAddress.PK;

			WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, addressType);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn = newFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery());
			var dcn = newFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery());

			ZGuid expectedAddress = addressType switch
			{
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.LCN => localClientOrg.MainAddress.PK,
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.SFA => sendingOrg.MainAddress.PK,
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.RFA => receivingOrg.MainAddress.PK,
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED => testData.consignee.MainAddress.PK,
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD => rcn.BookingPartyDocAddress.E2_OA_Address,
				_ => ZGuid.Empty
			};

			AssertEquals($"Address mismatch for {addressType} in RCN.", rcn.ClientRequestedBillToPartyDocAddress.E2_OA_Address, expectedAddress);
			AssertEquals($"Address mismatch for {addressType} in DCN.", dcn.ClientRequestedBillToPartyDocAddress.E2_OA_Address, expectedAddress);
		}

		#endregion

		#region Set Default Transport Company

		public void TestSetDefaultTransportCompany_ByBookingPartyRelatedParties_ArrivalWarehouse() => TestSetDefaultTransportCompany_ByRelatedParties(isArrivalTransitWarehouse: true, setByWarehouseRelatedParties: false);

		public void TestSetDefaultTransportCompany_ByBookingPartyRelatedParties_DepartureWarehouse() => TestSetDefaultTransportCompany_ByRelatedParties(isArrivalTransitWarehouse: false, setByWarehouseRelatedParties: false);

		public void TestSetDefaultTransportCompany_ByWarehouseRelatedParties_ArrivalWarehouse() => TestSetDefaultTransportCompany_ByRelatedParties(isArrivalTransitWarehouse: true, setByWarehouseRelatedParties: true);

		public void TestSetDefaultTransportCompany_ByWarehouseRelatedParties_DepartureWarehouse() => TestSetDefaultTransportCompany_ByRelatedParties(isArrivalTransitWarehouse: false, setByWarehouseRelatedParties: true);

		void TestSetDefaultTransportCompany_ByRelatedParties(bool isArrivalTransitWarehouse, bool setByWarehouseRelatedParties)
		{
			var testData = CreateTestData(isArrivalTransitWarehouse);

			var sendingOrg = TestHelper.CreateOrganisation("ORGS", address1: "Sending Organisation Address");
			var receivingOrg = TestHelper.CreateOrganisation("ORGR", address1: "Receiving Organisation Address");
			var relatedOrg1 = TestHelper.CreateOrganisation("ORG1", address1: "Related Party Orgnisation 1");
			var relatedOrg2 = TestHelper.CreateOrganisation("ORG2", address1: "Related Party Orgnisation 2");
			var relatedOrg3 = TestHelper.CreateOrganisation("ORG3", address1: "Related Party Orgnisation 3");
			var relatedOrg4 = TestHelper.CreateOrganisation("ORG4", address1: "Related Party Orgnisation 4");
			var relatedOrg5 = TestHelper.CreateOrganisation("ORG5", address1: "Related Party Orgnisation 5");
			var relatedOrg6 = TestHelper.CreateOrganisation("ORG6", address1: "Related Party Orgnisation 6");

			Factory.Save();

			sendingOrg.SetRelatedParty(relatedOrg3, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "ROA", "", testData.cfs.UNLOCO.Code);
			sendingOrg.SetRelatedParty(relatedOrg4, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "ROA", "", testData.cfs.UNLOCO.Code);

			receivingOrg.SetRelatedParty(relatedOrg3, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "ROA", "", testData.cfs.UNLOCO.Code);
			receivingOrg.SetRelatedParty(relatedOrg4, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "ROA", "", testData.cfs.UNLOCO.Code);

			if (setByWarehouseRelatedParties)
			{
				var warehouseOrg = Factory.Load<OrgHeader>(testData.cfs.PK);
				warehouseOrg.SetRelatedParty(relatedOrg5, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "SEA", "FCL", testData.cfs.UNLOCO.Code);
				warehouseOrg.SetRelatedParty(relatedOrg6, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", testData.cfs.UNLOCO.Code);
			}
			else
			{
				sendingOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "SEA", "FCL", testData.cfs.UNLOCO.Code);
				sendingOrg.SetRelatedParty(relatedOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", testData.cfs.UNLOCO.Code);
				receivingOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "SEA", "FCL", testData.cfs.UNLOCO.Code);
				receivingOrg.SetRelatedParty(relatedOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", testData.cfs.UNLOCO.Code);
			}

			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingOrg.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingOrg.MainAddress.PK;

			var shipment = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transportCompany = setByWarehouseRelatedParties ? relatedOrg5 : relatedOrg1;

			if (isArrivalTransitWarehouse)
			{
				transportCompany = setByWarehouseRelatedParties ? relatedOrg6 : relatedOrg2;
				var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
				AssertEquals("Only one RTU is created.", 1, rtus.Length);
				AssertReceiveTransportationUnitTransportCompany(newFactory, rtus[0], transportCompany.MainAddress);
			}

			var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("One ASN is created.", 1, asns.Length);
			AssertReceiveASNTransportCompany(newFactory, asns[0], transportCompany.MainAddress);

			var dtus = newFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Only one DTU is created.", 1, dtus.Length);
			AssertDispatchDTUTransportCompany(newFactory, dtus[0], transportCompany.MainAddress);
		}

		public void TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation_UNLOCO() => TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation("UNLOCO");

		public void TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation_Country() => TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation("Country");

		public void TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation_Empty() => TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation("Empty");

		public void TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation_NoMatched() => TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation("NoMatched");

		void TestSetDefaultTransportCompany_ByRelatedParties_MatchedByLocation(string matchCase)
		{
			var testData = CreateTestData(true);

			var relatedOrg1 = TestHelper.CreateOrganisation("ORG1", address1: "Related Party Orgnisation 1");
			var relatedOrg2 = TestHelper.CreateOrganisation("ORG2", address1: "Related Party Orgnisation 2");
			var relatedOrg3 = TestHelper.CreateOrganisation("ORG3", address1: "Related Party Orgnisation 3");

			var anotherUNLOCO = Factory.New<RefUNLOCO>();
			anotherUNLOCO.RL_Code = "XX111";
			anotherUNLOCO.RL_RN_NKCountryCode = "XX";

			Factory.Save();

			var warehouseOrg = Factory.Load<OrgHeader>(testData.cfs.PK);
			var transportCompanyAddress = relatedOrg1.MainAddress;

			if (matchCase == "UNLOCO")
			{
				warehouseOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", warehouseOrg.UNLOCO.Code);
				warehouseOrg.SetRelatedParty(relatedOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", warehouseOrg.UNLOCO.RL_RN_NKCountryCode);
				warehouseOrg.SetRelatedParty(relatedOrg3, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", ZString.Empty);
			}
			else if (matchCase == "Country")
			{
				warehouseOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", warehouseOrg.UNLOCO.RL_RN_NKCountryCode);
				warehouseOrg.SetRelatedParty(relatedOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", anotherUNLOCO.Code);
				warehouseOrg.SetRelatedParty(relatedOrg3, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", ZString.Empty);
			}
			else if (matchCase == "Empty")
			{
				warehouseOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", string.Empty);
				warehouseOrg.SetRelatedParty(relatedOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", anotherUNLOCO.Code);
			}
			else
			{
				warehouseOrg.SetRelatedParty(relatedOrg1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "SEA", "FCL", anotherUNLOCO.Code);
				transportCompanyAddress = null;
			}

			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "S001", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("Only one RTU is created.", 1, rtus.Length);
			AssertReceiveTransportationUnitTransportCompany(newFactory, rtus[0], transportCompanyAddress);

			var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("One ASN is created.", 1, asns.Length);
			AssertReceiveASNTransportCompany(newFactory, asns[0], transportCompanyAddress);

			var dtus = newFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Only one DTU is created.", 1, dtus.Length);
			AssertDispatchDTUTransportCompany(newFactory, dtus[0], transportCompanyAddress);
		}

		#endregion

		#region TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_RTUHasStagingLocationIfWarehouseHasDefaultInboundDockDoor

		[TestDate(2023, 8, 28)]
		public void TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_RTUHasStagingLocationIfWarehouseHasDefaultInboundDockDoor()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: false);
			AssertNotEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);

			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			CreateContainer(consol, "CONT1");
			consol.Shipments.Add(shipment1);

			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtus = newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery());

			AssertEquals("An RTU should be created for each container", 1, rtus.Length);
			AssertEquals("RTU's stagingLocation should be WW_DefaultInboundDockDoor", warehouse.WW_DefaultInboundDockDoor, rtus[0].WRH_WL_StagingLocation);
		}

		#endregion

		#region TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_RTUNotUpdateStagingLocationIfRTUStagingLocationIsExist

		[TestDate(2023, 8, 28)]
		public void TestForwardingSendsShipments_AttachesAndSendsNewShipmentFromConsol_RTUNotUpdateStagingLocationIfRTUStagingLocationIsExist()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: false);
			AssertNotEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);

			var shipment1 = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Package);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			CreateContainer(consol, "CONT1");
			consol.Shipments.Add(shipment1);

			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtus = newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery());

			AssertEquals("An RTU should be created for each container", 1, rtus.Length);
			AssertEquals("RTU has stagingLocation", warehouse.WW_DefaultInboundDockDoor, rtus[0].WRH_WL_StagingLocation);

			var ddlLocationType = Helper.CreateLocationType("DOD", LocationClasses.Codes.DDL);
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			rtus[0].WRH_WL_StagingLocation = locationA1.PK;
			newFactory2.Save();

			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rtusUpdate = newFactory2.Load<WhsItemReceiveTransportationUnit>(new ZQuery());

			AssertEquals("RTU's StagingLocation is already exist", rtus[0].WRH_WL_StagingLocation, rtusUpdate[0].WRH_WL_StagingLocation);
			AssertNotEquals("RTU's StagingLocation dose not update", rtusUpdate[0].WRH_WL_StagingLocation, warehouse.WW_DefaultInboundDockDoor);
		}

		#endregion

		#region Test Send Overpack

		[TestDate(2020, 04, 30)]
		public void TestSendReceiveAndDispatchInstruction_WithOverpack()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container1 = CreateContainer(consol, "CONT1");

			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1, "OVP1");

			var inner1 = CreateInnerPackline(shipment, overpack1, 1, Constants.PkgUnit.Box, "Inner1", "1");
			var inner2 = CreateInnerPackline(shipment, overpack1, 1, Constants.PkgUnit.Box, "Inner2", "2");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "", "C00001000", "AA", "30-Apr-20 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn1, 3);

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 1 Dispatch Consignment", 1, dispatchConsignments.Length);

			AssertEquals("3 packages should be attached to DCN", 3, dispatchConsignments.Single().PackageStates.Count);

			var overpackState = rcn1.PackageStates.Single(p => p.Package.KP_PackageID == "OVP1");
			var innerState1 = rcn1.PackageStates.Single(p => p.Package.KP_PackageID == "Inner1");
			var innerState2 = rcn1.PackageStates.Single(p => p.Package.KP_PackageID == "Inner2");

			AssertPackageState(overpackState, "OVP", true, "BOX", 1);
			AssertPackageState(innerState1, "PKG", false, "BOX", 1, overpackState.Package.PK);
			AssertPackageState(innerState2, "PKG", false, "BOX", 1, overpackState.Package.PK);
		}

		[TestDate(2023, 12, 05)]
		public void TestSendDispatchInstruction_OverpackHasNoRCN()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(10), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

			TriggerAndFireTransitRequestForRelease(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should create one DCN", 1, dispatchConsignments.Length);

			var loadLists = newFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should create one Load List", 1, loadLists.Length);
			var loadList = loadLists.Single();

			var dcn = Factory.Load<WhsItemDispatchConsignment>(dispatchConsignments.Single().PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", dcn, rtu, dcn: dcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Factory.Save();

			var overpackPackline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");
			var innerPackline = CreateInnerPackline(shipment, overpackPackline, 2, Constants.PkgUnit.Package);

			AssertNoExceptionThrown(() => TriggerAndFireTransitRequestForRelease(consol));
			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var overpackPackageInNewFactory = newFactory.Load<WhsItemPackageState>(overpackPackage.PK);
			AssertEquals(overpackPackageInNewFactory.WPS_WDL_LoadList, loadList.PK);
			AssertEquals(overpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment, dcn.PK);

			var childPackage1InNewFactory = newFactory.Load<WhsItemPackageState>(childPackage1.PK);
			AssertEquals(childPackage1InNewFactory.WPS_WDL_LoadList, loadList.PK);
			AssertEquals(childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment, dcn.PK);

			var childPackage2InNewFactory = newFactory.Load<WhsItemPackageState>(childPackage2.PK);
			AssertEquals(childPackage2InNewFactory.WPS_WDL_LoadList, loadList.PK);
			AssertEquals(childPackage2InNewFactory.WPS_WDC_TransitDispatchConsignment, dcn.PK);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendReceiveAndDispatchInstruction_WithNontrackedInner()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container1 = CreateContainer(consol, "CONT1");

			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container1, "");

			var inner1 = CreateInnerPackline(shipment, overpack1, 5, Constants.PkgUnit.Box, "", "");
			var inner2 = CreateInnerPackline(shipment, overpack1, 1, Constants.PkgUnit.Carton, "INNER2", "");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1 = AssertConsignment(newBizOFactoryAfterRunningLogWalker, "HSB1", testData.today.AddDays(-1), testData.today, "NZAKL", "STD", testData.warehouse, shipment.PK, outboundLeg);
			AssertConsignmentAdditionalRefs(rcn1, "HSB1", "S00001000", "", "C00001000", "AA", "30-Apr-20 00:00");
			AssertEquals("RCN1 Destination", "AUADL", rcn1.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn1, 1);

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 1 Dispatch Consignment", 1, dispatchConsignments.Length);

			AssertEquals("1 package should be attached to DCN", 1, dispatchConsignments.Single().PackageStates.Count);

			var overpackState = rcn1.PackageStates.Single();

			AssertPackageState(overpackState, "PKG", false, "PLT", 1);

			var nontrackedInners = overpackState.Package.Packages;

			AssertEquals("2 inner nontracked item should be attached to parent", 2, nontrackedInners.Count);

			var boxNontrackedInner = nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals("inner nontracked item should have correct qty.", 5, boxNontrackedInner.KP_PackageQty);
			AssertEquals("inner nontracked item should have correct type.", Constants.PkgUnit.Box, boxNontrackedInner.KP_F3_NKPackType);

			var ctnNontrackedInner = nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals("inner nontracked item should have correct qty.", 1, ctnNontrackedInner.KP_PackageQty);
			AssertEquals("inner nontracked item should have correct type.", Constants.PkgUnit.Carton, ctnNontrackedInner.KP_F3_NKPackType);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_MatchMergedOverpackInner()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");
			var inner1 = CreateInnerPackline(shipment, overpack1, 2, Constants.PkgUnit.Package, "");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var overpackPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpackPackage.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage2.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals(dispatchConsignment.PK, overpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, overpackPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage2InNewFactory.WPS_WDL_LoadList);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_MatchMergedOverpackInner_MultiLevelOverPack()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var topOverpackPackage = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);
			var overpackPackage = Helper.CreateOverpackPackage("OVP2", rcn, rtu, rcn: rcn);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topOverpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topOverpackPackage);
			Helper.PackPackageIntoHandlingUnit(topOverpackPackage, overpackPackage, ZDateTimeOffset.Now, "AAA", topOverpackPackage);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var topOverpackPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(topOverpackPackage.PK);
			var overpackPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpackPackage.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage2.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals(dispatchConsignment.PK, topOverpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, overpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, topOverpackPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, overpackPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage2InNewFactory.WPS_WDL_LoadList);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_MatchMergedOverpackInner_MultiLevelOverPackWithHandlingUnit()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var topOverpackPackage = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);
			var overpackPackage = Helper.CreateOverpackPackage("OVP2", rcn, rtu, rcn: rcn);
			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(topOverpackPackage, overpackPackage, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, topOverpackPackage, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "HU1");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var topOverpackPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(topOverpackPackage.PK);
			var overpackPackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpackPackage.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage2.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals(dispatchConsignment.PK, topOverpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, overpackPackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, topOverpackPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, overpackPackageInNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, childPackage2InNewFactory.WPS_WDL_LoadList);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_MatchMergedOverpack()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("HSB1", "STD", testData.warehouse.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", overpackPackage);

			var overpack2Package = Helper.CreateOverpackPackage("OVP2", rcn, rtu, rcn: rcn);
			overpack2Package.WPS_UnitType = "OVP";
			overpack2Package.Package.KP_F3_NKPackType = "BOX";

			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage4 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(overpack2Package, childPackage3, ZDateTimeOffset.Now, "AAA", overpack2Package);
			Helper.PackPackageIntoHandlingUnit(overpack2Package, childPackage4, ZDateTimeOffset.Now, "AAA", overpack2Package);

			var overpack3Package = Helper.CreateOverpackPackage("OVP3", rcn, rtu, rcn: rcn);
			overpack3Package.WPS_UnitType = "OVP";
			overpack3Package.Package.KP_F3_NKPackType = "BOX";

			var childPackage5 = Helper.CreatePackageState(rcn, 1, "BOX", "PKG5", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage6 = Helper.CreatePackageState(rcn, 1, "BOX", "PKG6", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(overpack3Package, childPackage5, ZDateTimeOffset.Now, "AAA", overpack3Package);
			Helper.PackPackageIntoHandlingUnit(overpack3Package, childPackage6, ZDateTimeOffset.Now, "AAA", overpack3Package);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var ovp1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");
			var inner1 = CreateInnerPackline(shipment, ovp1, 2, Constants.PkgUnit.Package, "");

			var ovp2 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Box, reference: "");
			var inner3 = CreateInnerPackline(shipment, ovp2, 1, Constants.PkgUnit.Pallet, "PKG3");
			var inner4 = CreateInnerPackline(shipment, ovp2, 1, Constants.PkgUnit.Pallet, "PKG4");
			var inner5 = CreateInnerPackline(shipment, ovp2, 1, Constants.PkgUnit.Box, "PKG5");
			var inner6 = CreateInnerPackline(shipment, ovp2, 1, Constants.PkgUnit.Box, "PKG6");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var overpack1PackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpackPackage.PK);
			var overpack2PackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpack2Package.PK);
			var overpack3PackageInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(overpack3Package.PK);
			var childPackage1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage1.PK);
			var childPackage2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage2.PK);
			var childPackage3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage3.PK);
			var childPackage4InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage4.PK);
			var childPackage5InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage5.PK);
			var childPackage6InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(childPackage6.PK);

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals(dispatchConsignment.PK, overpack1PackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, overpack2PackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, overpack3PackageInNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage3InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage4InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage5InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, childPackage6InNewFactory.WPS_WDC_TransitDispatchConsignment);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_InnerQtyDoesNotMatch()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", overpackPackage);
			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");
			var inner1 = CreateInnerPackline(shipment, overpack1, 5, Constants.PkgUnit.Package, "");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have an unsuccessful export.", EDIMessageStatusList.Codes.Discarded, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have the combined error messages at the expected positions.",
@"Could not import Dispatch Instruction because inner Packline Qty of Type PKG is 2 which is different from the expected Qty 5.", importNote?.ST_NoteText);
		}

		[TestDate(2020, 04, 30)]
		public void TestSendDispatchInstruction_WithOverpack_PopulatePackingLine()
		{
			var testData = CreateTestData(true, parentProcessIsConsol: true);

			// CFS Receive 3 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);

			var existingPackage = Helper.CreatePackageState(rcn, 1, "BOX", "P1", "PUT", receiveUnit: rtu);

			Factory.Save();

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			var package = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "P1");
			var inner1 = CreateInnerPackline(shipment, package, 5, Constants.PkgUnit.Package, "");
			var inner2 = CreateInnerPackline(shipment, package, 3, Constants.PkgUnit.Box, "");
			var inner3 = CreateInnerPackline(shipment, package, 1, Constants.PkgUnit.Pallet, "Inner1");

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have only created 1 Dispatch Consignment", 1, dispatchConsignments.Length);

			AssertEquals("1 package should be attached to DCN", 1, dispatchConsignments.Single().PackageStates.Count);

			var overpackState = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(new ZQuery()).Single();

			AssertPackageState(overpackState, "PKG", false, "BOX", 1);

			var nontrackedInners = overpackState.Package.Packages;

			AssertEquals("3 inner nontracked items should be attached to parent", 3, nontrackedInners.Count);

			AssertEquals("Inner nontracked item should have correct qty and type.", 5, nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package).KP_PackageQty);
			AssertEquals("Inner nontracked item should have correct qty and type.", 3, nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box).KP_PackageQty);
			AssertEquals("Inner nontracked item should have correct qty and type.", 1, nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet).KP_PackageQty);
			AssertEquals("Inner nontracked item should not have Package ID.", ZGuid.Empty, nontrackedInners.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet).KP_KPH_PackageHeader);
		}

		#endregion

		#region TestForwardingSendsConsol_ExcludedPackline_ShouldNotCreateTransitPackage

		public void TestForwardingSendsConsol_ExcludedPackline_ShouldNotCreateTransitPackage()
		{
			var testData = CreateTestData(false);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			packline.JL_DepartureTransitWarehouseExcluded = true;
			AssertEquals("Shipment.OuterPack is 1", 1, shipment.JS_OuterPacks);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnList = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			var rcn = rcnList.Single();

			AssertEquals("Should have created 1 RCN.", 1, rcnList.Length);
			AssertEquals("Should not have any transit packages", 0, rcn.PackageStates.Count);
		}

		#endregion

		#region Set Delivery Address and Transport Company

		public void TestSetDeliveryAddressAndTransportCompany_ShipmentWithConsol_DCN()
		{
			var testData = CreateTestData(false);
			var deliveryOrg = TestHelper.CreateOrganisation("DLV");
			var transportOrg = TestHelper.CreateOrganisation("TRA");
			Factory.Save();

			var consol = CreateConsol("MTA2", testData.vessel, "AUSYD", "NZAKL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSO1", "AUSYD", "NZAKL", testData.today.AddDays(-1), testData.today.AddDays(2), testData.consignor, testData.consignee);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryOrg.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = transportOrg.MainAddress.PK;

			TriggerAndFireTransitRequestForRelease(consol);

			var dcn = AssertAndReturnDispatchConsignment(Factory, "HSO1", testData.warehouse, shipment.PK);

			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, dcn.PK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, dcn.TablePrefix);
			var jobDocAddresses = Factory.Load<JobDocAddress>(query);

			CombineAssertions(() =>
			{
				var deliveryAddress = jobDocAddresses.Single(address => address.E2_AddressType == "CEG");
				AssertEquals("delivery address is created for DCN", dcn.PK, deliveryAddress.E2_ParentID);
				AssertEquals("delivery address should be same as shipment forwarding", shipment.ConsigneeDeliveryAddress.E2_OA_Address, deliveryAddress.E2_OA_Address);
				var transportCompany = jobDocAddresses.Single(address => address.E2_AddressType == "TRA");
				AssertEquals("transport company is created for DCN", dcn.PK, transportCompany.E2_ParentID);
				AssertEquals("transport company should be same as shipment forwarding", shipment.DocsAndCartage.DeliveryCartageCoAddr.PK, transportCompany.E2_OA_Address);
			});
		}

		#endregion

		// Comment out until "WI00653920 - DG - Process inbound TWH outturn containing High Risk and Additional Inspection" is completed
		//#region Populate Package IsHighRisk

		//public void TestPopulatePackageIsHighRisk()
		//{
		//	var testData = CreateTestDataForCombined("NZCHC");
		//	CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);

		//	var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
		//	consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
		//	var outboundLeg = CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "NZAKL", testData.today, testData.today.AddDays(2));

		//	var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
		//	var container1 = CreateContainer(consol, "CONT1");

		//	//var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1, "OVP1");

		//	var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1, "P1", isHighRisk: true);
		//	var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, container1, "P2", isHighRisk: true);

		//	TriggerAndFireTransitRequestUsingBookingRequested(shipment);

		//	var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

		//	var receiveConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveConsignment>(new ZQuery());
		//	AssertEquals("Should have only created 1 Receive Consignment", 1, receiveConsignments.Length);

		//	var rcn1 = receiveConsignments.Single();
		//	AssertConsignmentPackages(rcn1, 2);

		//	var packageState1 = rcn1.PackageStates.Single(p => p.Package.KP_PackageID == "P1");
		//	var packageState2 = rcn1.PackageStates.Single(p => p.Package.KP_PackageID == "P2");

		//	AssertPackageState(packageState1, "PKG", false, "BOX", 1, isHighRisk: true);
		//	AssertPackageState(packageState2, "PKG", false, "BOX", 1, isHighRisk: true);

		//	var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
		//	AssertEquals("Should have only created 1 Dispatch Consignment", 1, dispatchConsignments.Length);

		//	var dcn1 = dispatchConsignments.Single();
		//	AssertEquals("2 packages should be attached to DCN", 2, dcn1.PackageStates.Count);

		//	packageState1 = dcn1.PackageStates.Single(p => p.Package.KP_PackageID == "P1");
		//	packageState2 = dcn1.PackageStates.Single(p => p.Package.KP_PackageID == "P2");

		//	AssertPackageState(packageState1, "PKG", false, "BOX", 1, isHighRisk: true);
		//	AssertPackageState(packageState2, "PKG", false, "BOX", 1, isHighRisk: true);
		//}

		//#endregion

		[TestDate(2024, 02, 24)]
		public void TestPopulatePackageStateSecurityStatusAndCustomsStatus_WhenSendTWXFromConsol()
		{
			var testData = CreateTestDataForCombined("NZCHC");
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			testData.warehouse.WW_TransitSecurityProcessingRequired = true;
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
				var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL", transportMode: "AIR");
				consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
				CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

				var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "P1");
				var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var packageState = newBizOFactory.LoadTop1<WhsItemPackageState>(new ZQuery());
				var rcn = newBizOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery());
				consol = newBizOFactory.LoadTop1<ForwardingConsol>(new ZQuery());
				packline = newBizOFactory.LoadTop1<ForwardingPackLine>(new ZQuery());
				var warehouse = newBizOFactory.LoadTop1<WhsWarehouse>(new ZQuery());
				AssertEquals("Security Status is REQ", TransitWarehouseSecurityStatuses.Codes.Required, packageState.WPS_SecurityStatus);
				AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState.WPS_CustomsStatus);
				AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcn.WRC_CustomsStatus);

				warehouse.WW_IsCustomsControlled = ZBool.True;
				newBizOFactory.Save();

				Helper.CreatePackageScreening(packageState.Package, "XRY", true);
				packageState.Reload();
				UnloadAndLabelPackage(packageState, rtu, "P1");

				newBizOFactory.Save();

				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				packageState = newBizOFactory.LoadTop1<WhsItemPackageState>(new ZQuery());
				rcn = newBizOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals("Security Status is Screened", TransitWarehouseSecurityStatuses.Codes.Screened, packageState.WPS_SecurityStatus);
				AssertEquals("Customs Status is CUS", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, packageState.WPS_CustomsStatus);
				AssertEquals("Customs Status is CUS", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, rcn.WRC_CustomsStatus);
			}
		}

		[TestDate(2024, 02, 24)]
		public void TestPopulatePackageStateSecurityStatusAndCustomsStatus_WhenSendTWXFromShipment()
		{
			var testData = CreateTestDataForCombined("NZCHC", parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			testData.warehouse.WW_TransitSecurityProcessingRequired = true;
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);
				var shipment = CreateShipment("HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment.JS_TransportMode = "AIR";
				shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
				var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "P1");
				var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);

				TriggerAndFireTransitRequestUsingBookingRequested(shipment);

				var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var packageState = newBizOFactory.LoadTop1<WhsItemPackageState>(new ZQuery());
				var rcn = newBizOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery());
				shipment = newBizOFactory.LoadTop1<ForwardingShipment>(new ZQuery());
				packline = newBizOFactory.LoadTop1<ForwardingPackLine>(new ZQuery());
				var warehouse = newBizOFactory.LoadTop1<WhsWarehouse>(new ZQuery());
				AssertEquals("Security Status is REQ", TransitWarehouseSecurityStatuses.Codes.Required, packageState.WPS_SecurityStatus);
				AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState.WPS_CustomsStatus);
				AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcn.WRC_CustomsStatus);

				warehouse.WW_IsCustomsControlled = ZBool.True;
				newBizOFactory.Save();

				Helper.CreatePackageScreening(packageState.Package, "XRY", true);
				packageState.Reload();
				UnloadAndLabelPackage(packageState, rtu, "P1");

				newBizOFactory.Save();

				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);

				newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				packageState = newBizOFactory.LoadTop1<WhsItemPackageState>(new ZQuery());
				rcn = newBizOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals("Security Status is Screened", TransitWarehouseSecurityStatuses.Codes.Screened, packageState.WPS_SecurityStatus);
				AssertEquals("Customs Status is CUS", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, packageState.WPS_CustomsStatus);
				AssertEquals("Customs Status is CUS", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, rcn.WRC_CustomsStatus);
			}
		}

		public void TestSendReceiveInstruction_WithBlindPackage()
		{
			SendReceiveInstruction_WithBlindPackage(true);
		}

		public void TestSendReceiveInstruction_WithPackage()
		{
			SendReceiveInstruction_WithBlindPackage(false);
		}

		public void SendReceiveInstruction_WithBlindPackage(bool isBlind)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			var shipment = CreateShipment("HSB1", "NZAKL", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultInboundDockDoorLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);

			if (!isBlind)
			{
				rcn.WRC_ParentID = shipment.PK;
				rcn.WRC_ParentTableCode = "JS";
			}

			var blindPacakgeState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var blindPacakgeState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "PKG-2");

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingShipment>(shipment.PK);
			var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
			var importNote = declinedMessage.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("shoulde throw exception",
@"Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.", importNote.ST_NoteText);
		}

		public void TestSendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage()
		{
			SendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage(true);
		}

		public void TestSendReceiveInstruction_WithOVPAndOVPHasInnerPackage()
		{
			SendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage(false);
		}

		public void SendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage(bool isBlind)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);
			var shipment = CreateShipment("HSB1", "NZAKL", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);

			if (!isBlind)
			{
				rcn.WRC_ParentID = shipment.PK;
				rcn.WRC_ParentTableCode = "JS";
			}

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultInboundDockDoorLocation.PK);
			var ovp = Helper.CreateOverpackPackage("OVP1", rcn, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: rcn);
			var innerPackage = Helper.CreatePackageState(rcn, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);

			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");

			var inner1 = CreateInnerPackline(shipment, overpack1, 1, Constants.PkgUnit.Box, "Inner1", "1");

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingShipment>(shipment.PK);
			var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
			var importNote = declinedMessage.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("shoulde throw exception",
@"Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.", importNote.ST_NoteText);
		}

		public void TestSendReceiveInstruction_WithBlindOVPAndOVPHasPackline()
		{
			SendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage(true);
		}

		public void TestSendReceiveInstruction_WithOVPAndOVPHasPackline()
		{
			SendReceiveInstruction_WithBlindOVPAndOVPHasInnerPackage(false);
		}

		public void SendReceiveInstruction_WithBlindOVPAndOVPHasPackline(bool isBlind)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			var shipment = CreateShipment("HSB1", "NZAKL", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);

			if (!isBlind)
			{
				rcn.WRC_ParentID = shipment.PK;
				rcn.WRC_ParentTableCode = "JS";
			}

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultInboundDockDoorLocation.PK);
			var ovp = Helper.CreateOverpackPackage("OVP1", rcn, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: rcn);
			var innerPackline = Helper.CreatePackageState(rcn, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			var overpack1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "OVP1");

			var inner1 = CreateInnerPackline(shipment, overpack1, 1, Constants.PkgUnit.Box, "", "1");

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolAfterImport = newBizOFactoryAfterRunningLogWalker.Load<ForwardingShipment>(shipment.PK);
			var declinedMessage = UniversalHelper.GetEDIMessagesFromDB(consolAfterImport, "DEX").FirstOrDefault(m => m.EM_Status == EDIMessageStatusList.Codes.Discarded);
			var importNote = declinedMessage.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("shoulde throw exception",
@"Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.", importNote.ST_NoteText);
		}

		#region Test for duplicate DTU creation

		public void TestFowardingSendsConsol_DtuExistsAndNotLoadCompleted_ShouldNotCreateNewDtu()
		{
			TestFowardingSendsSameConsolTwice_DtuExistsAndUpdateLoadCompleted(false);
		}

		public void TestFowardingSendsConsol_DtuExistsAndLoadCompleted_ShouldCreateNewDtu()
		{
			TestFowardingSendsSameConsolTwice_DtuExistsAndUpdateLoadCompleted(true);
		}

		public void TestFowardingSendsSameConsolTwice_DtuExistsAndUpdateLoadCompleted(bool loadComplete)
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var container1 = CreateContainer(consol, "CONT1");
			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container: container1, reference: "P1");
			var packline2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container: container1, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list for the container", 1, loadList.Length);

			var dtu = newBizOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Should have created a DTU present", 1, dtu.Length);

			if (loadComplete)
			{
				dtu[0].WDH_GateInTime = DateTime.Today;
				dtu[0].WDH_LoadCompleteTime = DateTime.Today;
				newBizOFactory.Save();
			}

			TriggerAndFireTransitRequestForRelease(consol);

			newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var newloadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("There should be only one load list", 1, newloadList.Length);
			AssertEquals("The new load list is the same as the old one", loadList[0].PK, newloadList[0].PK);

			var newDtu = newBizOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			if (loadComplete)
			{
				AssertEquals("There should be a new DTU created", 2, newDtu.Length);
			}
			else
			{
				AssertEquals("There should be only one DTU", 1, newDtu.Length);
			}
		}

		public void TestFowardingSendsTwoConsol_DtuExistsAndNotLoadCompleted_ShouldNotCreateNewDtu()
		{
			TestFowardingSendsTwoConsolWithSameContainer_DtuExistsAndUpdateLoadCompleted(false);
		}

		public void TestFowardingSendsTwoConsol_DtuExistsAndLoadCompleted_ShouldCreateNewDtu()
		{
			TestFowardingSendsTwoConsolWithSameContainer_DtuExistsAndUpdateLoadCompleted(true);
		}

		public void TestFowardingSendsTwoConsolWithSameContainer_DtuExistsAndUpdateLoadCompleted(bool loadComplete)
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var container1 = CreateContainer(consol, "CONT1");
			var shipment1 = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container: container1, reference: "P1");
			var packline2 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, container: container1, reference: "P2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list for the container", 1, loadList.Length);

			var dtu = newBizOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Should have created a DTU present", 1, dtu.Length);

			if (loadComplete)
			{
				dtu[0].WDH_GateInTime = DateTime.Today;
				dtu[0].WDH_LoadCompleteTime = DateTime.Today;
				newBizOFactory.Save();
			}

			var consol2 = CreateConsol("MSB2", testData.vessel, "NZAKL", "AUSYD");
			consol2.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var container2 = CreateContainer(consol2, "CONT1");
			var shipment2 = CreateShipment(consol2, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var packline3 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Pallet, container: container2, reference: "P3");
			var packline4 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Box, container: container2, reference: "P4");

			TriggerAndFireTransitRequestUsingBookingRequested(consol2);
			TriggerAndFireTransitRequestForRelease(consol2);

			newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var newloadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("There should be two load lists", 2, newloadList.Length);

			var newDtu = newBizOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			if (loadComplete)
			{
				AssertEquals("There should be a new DTU created", 2, newDtu.Length);
			}
			else
			{
				AssertEquals("There should be only one DTU", 1, newDtu.Length);
			}
		}

		#endregion

		#region Test for DCN and RCN Direction

		[TestDate(2020, 04, 30)]
		public void TestRcnDirection_Arrival_Domestic_WhenShipmentIsLinkedToMultipleConsol()
		{
			var (_, marseilleWH, parisWH, _, consignor, consignee) =
				CreateTestDataToMatchRCN(CFSType.ArrivalCFSOnConsol, departureCFSPortCode: "FRMRS",
					arrivalCFSPortCode: "FRPAR");

			var consol1_fr_to_fr = CreateConsol("MSB1", null, "FRMRS", "FRPAR", "ABCD", Constants.TransportModes.Road);
			consol1_fr_to_fr.JK_OA_UnpackDepotAddress = parisWH.WarehouseAddress.PK;
			consol1_fr_to_fr.JK_OA_PackDepotAddress = marseilleWH.WarehouseAddress.PK;
			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet);
			consol1_fr_to_fr.Shipments.Add(shipment1);
			CreateTransport(consol1_fr_to_fr, 1, Constants.TransportModes.Road, "A", "AA", "FRMRS", "FRPAR",
				ZDateTime.Today, ZDateTime.Today.AddDays(2));

			TriggerAndFireTransitRequestUsingBookingRequested(consol1_fr_to_fr);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignment = AssertAndReturnReceiveConsignment(newBizOFactory, "HSB1", parisWH, shipment1.PK);
			var packageState1 = receiveConsignment.PackageStates;
			AssertEquals("RCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic,
				receiveConsignment.WRC_Direction);
			AssertEquals(1, packageState1.Count);
			AssertEquals(2, packageState1[0].Package.KP_PackageQty);
		}

		[TestDate(2020, 04, 30)]
		public void TestDcnDirection_Departure_Export_WhenShipmentIsLinkedToMultipleConsol()
		{
			var (_, parisWH, uslaxWH, _, consignor, consignee) =
				CreateTestDataToMatchRCN(CFSType.DepartureCFSOnConsol, departureCFSPortCode: "FRPAR",
					arrivalCFSPortCode: "USLAX");

			var consol_fr_to_us = CreateConsol("MSB2", null, "FRPAR", "USLAX", "ABC2", Constants.TransportModes.Air);
			consol_fr_to_us.JK_OA_PackDepotAddress = parisWH.WarehouseAddress.PK;
			consol_fr_to_us.JK_OA_UnpackDepotAddress = uslaxWH.WarehouseAddress.PK;
			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet);
			consol_fr_to_us.Shipments.Add(shipment1);
			CreateTransport(consol_fr_to_us, 1, Constants.TransportModes.Air, "A", "AA", "FRPAR", "USLAX",
				ZDateTime.Today, ZDateTime.Today.AddDays(2));
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol_fr_to_us);
			TriggerAndFireTransitRequestForRelease(consol_fr_to_us);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list", 1, loadList.Length);
			AssertEquals("Should have created a load list in FR warehouse", parisWH.PK, loadList[0].WDL_WW_Warehouse);
			var dcn = AssertAndReturnDispatchConsignment(newBizOFactory, "HSB1", parisWH, shipment1.PK);
			AssertEquals("DCN Direction is Export", TransitWarehouseConsignmentDirections.Codes.Export,
				dcn.WDC_Direction);
		}

		[TestDate(2020, 04, 30)]
		public void TestRcnDirection_Arrival_Import_WhenShipmentIsLinkedToMultipleConsol()
		{
			var (_, parisWH, uslaxWH, _, consignor, consignee) =
				CreateTestDataToMatchRCN(CFSType.ArrivalCFSOnConsol, departureCFSPortCode: "FRPAR",
					arrivalCFSPortCode: "USLAX");

			var consol_fr_to_us = CreateConsol("MSB2", null, "FRPAR", "USLAX", "ABC2", Constants.TransportModes.Air);
			consol_fr_to_us.JK_OA_PackDepotAddress = parisWH.WarehouseAddress.PK;
			consol_fr_to_us.JK_OA_UnpackDepotAddress = uslaxWH.WarehouseAddress.PK;
			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet);
			consol_fr_to_us.Shipments.Add(shipment1);
			CreateTransport(consol_fr_to_us, 1, Constants.TransportModes.Air, "A", "AA", "FRPAR", "USLAX",
				ZDateTime.Today, ZDateTime.Today.AddDays(2));
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol_fr_to_us);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newReceiveConsignment =
				AssertAndReturnReceiveConsignment(newBizOFactory, "HSB1", uslaxWH, shipment1.PK);
			AssertEquals("RCN Direction is Import", TransitWarehouseConsignmentDirections.Codes.Import,
				newReceiveConsignment.WRC_Direction);
			var newPackageState1 = newReceiveConsignment.PackageStates;
			AssertEquals(1, newPackageState1.Count);
		}

		[TestDate(2020, 04, 30)]
		public void TestDcnDirection_Departure_Domestic_WhenShipmentIsLinkedToMultipleConsol()
		{
			var (_, uslaxWH, usAnyWH, _, consignor, consignee) =
				CreateTestDataToMatchRCN(CFSType.DepartureCFSOnConsol, departureCFSPortCode: "USLAX",
					arrivalCFSPortCode: "USANY");

			var consol_us_to_us = CreateConsol("MSB3", null, "USLAX", "USANY", "ABC3", Constants.TransportModes.Road);
			consol_us_to_us.JK_OA_UnpackDepotAddress = usAnyWH.WarehouseAddress.PK;
			consol_us_to_us.JK_OA_PackDepotAddress = uslaxWH.WarehouseAddress.PK;
			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet);
			consol_us_to_us.Shipments.Add(shipment1);
			CreateTransport(consol_us_to_us, 1, Constants.TransportModes.Road, "A", "AA", "USLAX", "USANY", ZDateTime.Today, ZDateTime.Today.AddDays(2));
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol_us_to_us);
			TriggerAndFireTransitRequestForRelease(consol_us_to_us);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list", 1, loadList.Length);
			AssertEquals("Should have created a load list in US warehouse", uslaxWH.PK, loadList[0].WDL_WW_Warehouse);
			var dcn = AssertAndReturnDispatchConsignment(newBizOFactory, "HSB1", uslaxWH, shipment1.PK);
			AssertEquals("DCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic, dcn.WDC_Direction);
		}

		public void TestConsignmentDirection_OriginFallbackToConsolLoading() => TestConsignmentDirection_FallbackToConsolPorts(CFSType.DepartureCFSOnConsol);
		public void TestConsignmentDirection_DestinationFallbackToConsolDischarge() => TestConsignmentDirection_FallbackToConsolPorts(CFSType.ArrivalCFSOnConsol);

		public void TestConsignmentDirection_FallbackToConsolPorts(CFSType type)
		{
			var (_, uslaxWH, usAnyWH, _, consignor, consignee) =
				CreateTestDataToMatchRCN(type, departureCFSPortCode: "USLAX",
					arrivalCFSPortCode: "USANY");

			var cfsWH = type == CFSType.DepartureCFSOnConsol ? uslaxWH : usAnyWH;

			var consol_us_to_us = CreateConsol("MSB3", null, "USLAX", "USANY", "ABC3", Constants.TransportModes.Road);
			consol_us_to_us.JK_OA_UnpackDepotAddress = usAnyWH.WarehouseAddress.PK;
			consol_us_to_us.JK_OA_PackDepotAddress = uslaxWH.WarehouseAddress.PK;
			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment1, 2, Constants.PkgUnit.Pallet);
			consol_us_to_us.Shipments.Add(shipment1);
			CreateTransport(consol_us_to_us, 1, Constants.TransportModes.Road, "A", "AA", "USLAX", "USANY", ZDateTime.Today, ZDateTime.Today.AddDays(2));

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol_us_to_us);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newReceiveConsignment = AssertAndReturnReceiveConsignment(newBizOFactory, "HSB1", cfsWH, shipment1.PK);
			AssertEquals("RCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic, newReceiveConsignment.WRC_Direction);
			var newPackageState1 = newReceiveConsignment.PackageStates;
			AssertEquals(1, newPackageState1.Count);

			TriggerAndFireTransitRequestForRelease(consol_us_to_us);

			var newBizOFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadList = newBizOFactory2.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should have created a load list", 1, loadList.Length);
			AssertEquals("Should have created a load list in US warehouse", cfsWH.PK, loadList[0].WDL_WW_Warehouse);
			var dcn = AssertAndReturnDispatchConsignment(newBizOFactory, "HSB1", cfsWH, shipment1.PK);
			AssertEquals("DCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic, dcn.WDC_Direction);
		}

		public void TestConsignmentDirection_OriginFallbackToShipmentLoading() => TestConsignmentDirection_FallbackToShipmentPorts(false, "USLAX", null, "USANY", "USANY");
		public void TestConsignmentDirection_OriginFallbackToShipmentOrigin() => TestConsignmentDirection_FallbackToShipmentPorts(false, null, "USLAX", "USANY", "USANY");
		public void TestConsignmentDirection_DestinationFallbackToShipmentDestination() => TestConsignmentDirection_FallbackToShipmentPorts(true, "USLAX", "USLAX", null, "USANY");
		public void TestConsignmentDirection_DestinationFallbackToShipmentDischarge() => TestConsignmentDirection_FallbackToShipmentPorts(true, "USLAX", "USLAX", "USANY", null);
		public void TestConsignmentDirection_FallbackToShipmentPorts(bool isArrivalCFS, string shipmentPortofLoading, string shipmentPortOfOrigin, string shipmentPortOfDischarge, string shipmentPortOfDestination)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(isArrivalCFS, parentProcessIsConsol: false, cfsPortCode: isArrivalCFS ? "USANY" : "USLAX");

			var shipment1 = CreateShipment("HSB1", shipmentPortOfOrigin, shipmentPortOfDestination, today.AddDays(-1), today.AddDays(9), consignor, consignee);

			shipment1.JS_RL_NKDischargePort = shipmentPortOfDischarge;
			shipment1.JS_RL_NKLoadPort = shipmentPortofLoading;
			shipment1.JS_OA_ExportReceivingDepot = isArrivalCFS ? ZGuid.Empty : cfs.MainAddress.PK;
			shipment1.JS_OA_ImportReleaseDepot = isArrivalCFS ? cfs.MainAddress.PK : ZGuid.Empty;
			if (shipmentPortofLoading == null || shipmentPortOfDischarge == null)
			{
				CreateTransport(shipment1, 1, "VEH", "A", "AA", "USLAX", "USANY", today, today.AddDays(2));
			}

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			Factory.Save();
			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn = AssertAndReturnReceiveConsignment(newBizOFactory, "HSB1", warehouse, shipment1.PK);
			AssertEquals("RCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic, rcn.WRC_Direction);

			TriggerAndFireTransitRequestForRelease(shipment1);
			Factory.Save();

			var newBizOFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var dcn = AssertAndReturnDispatchConsignment(newBizOFactory2, "HSB1", warehouse, shipment1.PK);
			AssertEquals("DCN Direction is Domestic", TransitWarehouseConsignmentDirections.Codes.Domestic, dcn.WDC_Direction);
		}

		[TestDate(2020, 04, 30)]
		public void TestConsignmentDirection_NoInboundLegAndShipmentPlannedLoadIsForeignPortAndShipmentPlannedDischargeIsDomesticPort()
		{
			var (cfs, today, warehouse, consignor, consignee, _) = CreateTestData(arrivalWarehouse: true, parentProcessIsConsol: false, cfsPortCode: "USANY");

			var shipment1 = CreateShipment("HSB1", "FRPAR", "USANY", today.AddDays(-1), today.AddDays(9), consignor, consignee);

			shipment1.JS_RL_NKDischargePort = "USANY";
			shipment1.JS_RL_NKLoadPort = "FRPAR";
			shipment1.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment1.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			TriggerAndFireTransitRequestUsingBookingRequested(shipment1);
			Factory.Save();
			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rcn = AssertAndReturnReceiveConsignment(newBizOFactory, "HSB1", warehouse, shipment1.PK);
			AssertEquals("RCN Direction is Import", TransitWarehouseConsignmentDirections.Codes.Import, rcn.WRC_Direction);

			TriggerAndFireTransitRequestForRelease(shipment1);
			Factory.Save();
			var newBizOFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var dcn = AssertAndReturnDispatchConsignment(newBizOFactory2, "HSB1", warehouse, shipment1.PK);
			AssertEquals("DCN Direction is Import", TransitWarehouseConsignmentDirections.Codes.Import, dcn.WDC_Direction);
		}

		#endregion

		#region TestAddPostSaveActionForReader

		public void TestAddPostSaveActionFromConsol_TWR() => AddPostSaveActionFromConsolCore(ServiceCodeType.TWR);
		public void TestAddPostSaveActionFromConsol_TWX() => AddPostSaveActionFromConsolCore(ServiceCodeType.TWX);
		public void TestAddPostSaveActionFromConsol_TWD() => AddPostSaveActionFromConsolCore(ServiceCodeType.TWD);
		public void TestAddPostSaveActionFromConsol_TWP() => AddPostSaveActionFromConsolCore(ServiceCodeType.TWP);

		void AddPostSaveActionFromConsolCore(ServiceCodeType code)
		{
			var mockedSecurityStatusProcedure = "\r\nALTER PROC " + "UpdatePackageStateSecurityStatus" + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @RegistryValue BIT, @WarehouseConfigurationValue BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @PKs NVARCHAR(MAX);SELECT @PKs = STRING_AGG(CAST(Value AS NVARCHAR(MAX)), ',')\r\n    FROM @PackageStatePKs;update DummyBizo set Z0_Number = Z0_Number + 1,Z0_Description = @PKs\r\n\tRETURN 1\r\nEND";

			var mockedCustomStatusProcedure = "\r\nALTER PROC " + "UpdatePackageStateAndRCNCustomStatus" + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @CurrentUTC DATETIME, @WarehouseConfigCustomControlled BIT, @WarehouseConfigPortControlled BIT, @IsApplyPackageQuantityCountingAlgorithm BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @PKs NVARCHAR(MAX);SELECT @PKs = STRING_AGG(CAST(Value AS NVARCHAR(MAX)), ',')\r\n    FROM @PackageStatePKs;update DummyBizo set Z0_AnotherNumber = Z0_AnotherNumber + 1,Z0_NVarCharMax = @PKs\r\n\tRETURN 1\r\nEND";

			Db.Connection.ExecuteNonQuery(mockedSecurityStatusProcedure);
			Db.Connection.ExecuteNonQuery(mockedCustomStatusProcedure);

			var testData = CreateTestDataForCombined();

			if (code == ServiceCodeType.TWR || code == ServiceCodeType.TWD)
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);
			}
			else if (code == ServiceCodeType.TWP)
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code, false);
			}
			else
			{
				CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, false);
			}

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			CreateTransport(consol, 1, "AIR", "A", "AA", "NZCHC", "AUADL", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "HSB2", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);
			// Setting IsHighRisk does not take effect, so only CustomStatus can be tested here
			var packline1 = CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Box, reference: "PKG-1");
			var packline2 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Box, reference: "PKG-2");

			var dummyBizo = Factory.New<DummyBusinessObject>();

			AssertEquals("UpdatePackageStateSecurityStatus execution times", 0, dummyBizo.Z0_Number);
			AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 0, dummyBizo.Z0_AnotherNumber);

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dummyBizoReload = newBizOFactory.LoadTop1<DummyBusinessObject>(new ZQuery());
			var packageStatePKs = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

			AssertEquals("packageStatePKs's count", 2, packageStatePKs.Count);
			AssertEquals("UpdatePackageStateSecurityStatus execution times", 0, dummyBizoReload.Z0_Number);
			// UpdatePackageStateSecurityStatus's packagePKs cannot assert because it is not called in this test
			AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 1, dummyBizoReload.Z0_AnotherNumber);
			AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKs.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReload.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));

			if (code == ServiceCodeType.TWD)
			{
				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryForTWD = new BusinessObjectFactory() { RefreshEnabled = false };
				var dummyBizoReloadForTWD = newBizOFactoryForTWD.LoadTop1<DummyBusinessObject>(new ZQuery());
				var packageStatePKsForTWD = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

				AssertEquals("packageStatePKsForTWD's count", 2, packageStatePKsForTWD.Count);
				AssertEquals("UpdatePackageStateSecurityStatus execution times", 1, dummyBizoReloadForTWD.Z0_Number);
				AssertEquals("UpdatePackageStateSecurityStatus's packageStatePKs:", string.Join(",", packageStatePKs.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWD.Z0_Description.Split(",").OrderByDescending(pk => pk)));
				AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 2, dummyBizoReloadForTWD.Z0_AnotherNumber);
				AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKs.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWD.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));
			}

			if (code == ServiceCodeType.TWP)
			{
				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(consol);

				var newBizOFactoryForTWP = new BusinessObjectFactory() { RefreshEnabled = false };
				var dummyBizoReloadForTWP = newBizOFactoryForTWP.LoadTop1<DummyBusinessObject>(new ZQuery());
				var packageStatePKsForTWP = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

				AssertEquals("packageStatePKsForTWP's count", 2, packageStatePKsForTWP.Count);
				AssertEquals("UpdatePackageStateSecurityStatus execution times", 1, dummyBizoReloadForTWP.Z0_Number);
				AssertEquals("UpdatePackageStateSecurityStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWP.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWP.Z0_Description.Split(",").OrderByDescending(pk => pk)));
				AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 2, dummyBizoReloadForTWP.Z0_AnotherNumber);
				AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWP.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWP.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));
			}
		}

		public void TestAddPostSaveActionFromShipment_TWR() => AddPostSaveActionFromShipmentCore(ServiceCodeType.TWR);
		public void TestAddPostSaveActionFromShipment_TWX() => AddPostSaveActionFromShipmentCore(ServiceCodeType.TWX);
		public void TestAddPostSaveActionFromShipment_TWD() => AddPostSaveActionFromShipmentCore(ServiceCodeType.TWD);
		public void TestAddPostSaveActionFromShipment_TWP() => AddPostSaveActionFromShipmentCore(ServiceCodeType.TWP);

		void AddPostSaveActionFromShipmentCore(ServiceCodeType code)
		{
			var mockedSecurityStatusProcedure = "\r\nALTER PROC " + "UpdatePackageStateSecurityStatus" + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @RegistryValue BIT, @WarehouseConfigurationValue BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @PKs NVARCHAR(MAX);SELECT @PKs = STRING_AGG(CAST(Value AS NVARCHAR(MAX)), ',')\r\n    FROM @PackageStatePKs;update DummyBizo set Z0_Number = Z0_Number + 1,Z0_Description = @PKs\r\n\tRETURN 1\r\nEND";

			var mockedCustomStatusProcedure = "\r\nALTER PROC " + "UpdatePackageStateAndRCNCustomStatus" + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @CurrentUTC DATETIME, @WarehouseConfigCustomControlled BIT, @WarehouseConfigPortControlled BIT, @IsApplyPackageQuantityCountingAlgorithm BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @PKs NVARCHAR(MAX);SELECT @PKs = STRING_AGG(CAST(Value AS NVARCHAR(MAX)), ',')\r\n    FROM @PackageStatePKs;update DummyBizo set Z0_AnotherNumber = Z0_AnotherNumber + 1,Z0_NVarCharMax = @PKs\r\n\tRETURN 1\r\nEND";

			Db.Connection.ExecuteNonQuery(mockedSecurityStatusProcedure);
			Db.Connection.ExecuteNonQuery(mockedCustomStatusProcedure);

			var testData = CreateTestDataForCombined();

			if (code == ServiceCodeType.TWR || code == ServiceCodeType.TWD)
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);
			}
			else if (code == ServiceCodeType.TWP)
			{
				CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code, false);
			}
			else
			{
				CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);
			}

			var shipment = CreateShipment("HSB1", "NZAKL", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			var rcn = Helper.CreateReceiveConsignment("RCN1", testData.warehouse.PK);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, reference: "PKG-2", isHighRisk: true);

			var dummyBizo = Factory.New<DummyBusinessObject>();

			AssertEquals("UpdatePackageStateSecurityStatus execution times", 0, dummyBizo.Z0_Number);
			AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 0, dummyBizo.Z0_AnotherNumber);

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var newBizOFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dummyBizoReload = newBizOFactory.LoadTop1<DummyBusinessObject>(new ZQuery());
			var packageStatePKs = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

			AssertEquals("packageStatePKs's count", 1, packageStatePKs.Count);
			AssertEquals("UpdatePackageStateSecurityStatus execution times", 1, dummyBizoReload.Z0_Number);
			AssertEquals("UpdatePackageStateSecurityStatus's packageStatePKs:", string.Join(",", packageStatePKs.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReload.Z0_Description.Split(",").OrderByDescending(pk => pk)));
			AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 1, dummyBizoReload.Z0_AnotherNumber);
			AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKs.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReload.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));

			if (code == ServiceCodeType.TWD)
			{
				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);

				var newBizOFactoryForTWD = new BusinessObjectFactory() { RefreshEnabled = false };
				var dummyBizoReloadForTWD = newBizOFactoryForTWD.LoadTop1<DummyBusinessObject>(new ZQuery());
				var packageStatePKsForTWD = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

				AssertEquals("packageStatePKsForTWP's count", 1, packageStatePKsForTWD.Count);
				AssertEquals("UpdatePackageStateSecurityStatus execution times", 2, dummyBizoReloadForTWD.Z0_Number);
				AssertEquals("UpdatePackageStateSecurityStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWD.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWD.Z0_Description.Split(",").OrderByDescending(pk => pk)));
				AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 2, dummyBizoReloadForTWD.Z0_AnotherNumber);
				AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWD.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReload.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));
			}

			if (code == ServiceCodeType.TWP)
			{
				TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(shipment);

				var newBizOFactoryForTWP = new BusinessObjectFactory() { RefreshEnabled = false };
				var dummyBizoReloadForTWP = newBizOFactoryForTWP.LoadTop1<DummyBusinessObject>(new ZQuery());
				var packageStatePKsForTWP = newBizOFactory.Load<WhsItemPackageState>(new ZQuery()).Select(p => p.PK).ToList();

				AssertEquals("packageStatePKsForTWP's count", 1, packageStatePKsForTWP.Count);
				AssertEquals("UpdatePackageStateSecurityStatus execution times", 2, dummyBizoReloadForTWP.Z0_Number);
				AssertEquals("UpdatePackageStateSecurityStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWP.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWP.Z0_Description.Split(",").OrderByDescending(pk => pk)));
				AssertEquals("UpdatePackageStateAndRCNCustomStatus execution times", 2, dummyBizoReloadForTWP.Z0_AnotherNumber);
				AssertEquals("UpdatePackageStateAndRCNCustomStatus's packageStatePKs:", string.Join(",", packageStatePKsForTWP.OrderByDescending(pk => pk)).ToUpper(), string.Join(",", dummyBizoReloadForTWP.Z0_NVarCharMax.Split(",").OrderByDescending(pk => pk)));
			}
		}

		#endregion

		#region Implementation

		public PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		class NotificationsForTest : INotifications
		{
			public string Notifications
			{
				get { return string.Join(System.Environment.NewLine, notifications); }
			}

			void INotifications.Add(INotification notification)
			{
				if (notification != null)
				{
					LastNotification = notification;
					notifications.Add(notification.Message);
				}
			}

			readonly List<string> notifications = new List<string>();

			public INotification LastNotification { get; private set; }
		}

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory);

		#endregion

	}
}
