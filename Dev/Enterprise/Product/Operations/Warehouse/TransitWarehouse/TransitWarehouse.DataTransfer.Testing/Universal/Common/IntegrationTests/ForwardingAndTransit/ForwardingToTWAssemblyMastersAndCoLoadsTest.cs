using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class ForwardingToTWAssemblyMastersAndCoLoadsTest : IntegrationTestCaseWithFactory
	{
		#region TestImportForwardingConsol_AssemblyMaster_CreatingDispatchConsignments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingDispatchConsignments()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   3 shipments
			//   2 containers
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment1 = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment2 = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipment3 = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var container1 = CreateContainer(consol, "CONT1");
			var container2 = CreateContainer(consol, "CONT2");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
			CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
			CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
			CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var rcn1 = AssertConsignment(Factory, "S00001000", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);
			var rcn2 = AssertConsignment(Factory, "S00001001", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
			var rcn3 = AssertConsignment(Factory, "S00001002", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
			var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments);
			AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

			var dcn1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "S00001000");
			var dcn2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "S00001001");
			var dcn3 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "S00001002");

			AssertDispatchConsignmentPackageStates(rcn1.PackageStates, dcn1, shipment1.PK);
			AssertDispatchConsignmentPackageStates(rcn2.PackageStates, dcn2, shipment2.PK);
			AssertDispatchConsignmentPackageStates(rcn3.PackageStates, dcn3, shipment3.PK);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_DispatchLoadList_ReadyToStage_SendMultipleTimes()
		{
			var testData = CreateTestData(true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment1 = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container1 = CreateContainer(consol, "CONT1");

			CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "S00001000", testData.warehouse, shipment1.PK);

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			loadList.WDL_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			loadList.WDL_IsReadyToStage = true;
			AssertEquals(1, loadList.PackageStates.Count);
			newBizOFactoryAfterRunningLogWalker.Save();

			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterRunningLogWalkerForSecondTime = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterRunningImportForSecondTime = newBizOFactoryAfterRunningLogWalkerForSecondTime.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(1, loadListAfterRunningImportForSecondTime.PackageStates.Count);
			AssertEquals(testData.warehouse.DefaultLocation.PK, loadListAfterRunningImportForSecondTime.WDL_WL_StagingLocation);
			AssertEquals(false, loadListAfterRunningImportForSecondTime.WDL_IsReadyToStage);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingDispatchConsignments_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestImportForwardingConsol_AssemblyMaster_CreatingDispatchConsignments();
			}
		}

		#endregion

		#region TestImportForwardingConsol_EmptyAssemblyMaster_CreatingDispatchConsignments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_EmptyAssemblyMaster_CreatingDispatchConsignments()
		{
			var testData = CreateTestData(true);

			// Consol w/
			//   1 shipment
			//   1 container
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

			var shipment = CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, "CONT1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// Receive consignments
			var rcn1 = AssertConsignment(Factory, "S00001000", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment.PK, outboundLeg);

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, Array.Empty<ForwardingShipment>());

			// Dispatch consignments, headers and wave
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
			AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1" });

			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

			var dcn1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "S00001000");

			AssertDispatchConsignmentPackageStates(rcn1.PackageStates, dcn1, shipment.PK);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_EmptyAssemblyMaster_CreatingDispatchConsignments_2012XMLSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestImportForwardingConsol_EmptyAssemblyMaster_CreatingDispatchConsignments();
			}
		}

		#endregion

		#region Creating Receive Consginments Only

		#region TestImportForwardingConsol_CreatingReceiveConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BuyersConsol_CreatingReceiveConsignments_ForGroupedShipments()
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "Sub1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "Sub2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton, container2);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForBuyersConsolLead = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
				AssertEquals(2, asns.Length);
				var asnForContainer1 = asns.Single(a => a.WRP_VehicleReference == "CONT1");
				var asnForContainer2 = asns.Single(a => a.WRP_VehicleReference == "CONT2");

				var packageStateForStandard = rcnForStandardShipment.PackageStates.Single();
				AssertEquals(asnForContainer1, packageStateForStandard.ReceiveASN);

				var packageStatesInBuyersConsolLead = rcnForBuyersConsolLead.PackageStates.Single();
				AssertEquals(asnForContainer2, packageStatesInBuyersConsolLead.ReceiveASN);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForBuyersConsolLead.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		void AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");

				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "Sub1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "Sub2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;
				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForMaster = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
				AssertEquals(3, asns.Length);
				var asnForContainer1 = asns.Single(a => a.WRP_VehicleReference == "CONT1");
				var asnForContainer2 = asns.Single(a => a.WRP_VehicleReference == "CONT2");
				var asnForNoContainer = asns.Single(a => a.WRP_VehicleReference == "MSB1");

				var packageStateForStandard = rcnForStandardShipment.PackageStates.Single();
				AssertEquals(asnForContainer1, packageStateForStandard.ReceiveASN);

				var packageStatesInMaster = rcnForMaster.PackageStates;
				AssertEquals(3, packageStatesInMaster.Count);
				AssertEquals(asnForContainer1, packageStatesInMaster.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Case).ReceiveASN);
				AssertEquals(asnForContainer2, packageStatesInMaster.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum).ReceiveASN);
				AssertEquals(asnForNoContainer, packageStatesInMaster.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package).ReceiveASN);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		#endregion

		#region TestImportForwardingConsol_CreatingReceiveConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BuyersConsolLead_CreatingReceiveConsignments_ForSubShipments()
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton, container1);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(4, rcnsInTheSystem.Length);
				var rcnForMaster = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForSub1 = AssertConsignment(Factory, "SUB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
				var rcnForSub2 = AssertConsignment(Factory, "SUB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
				AssertEquals(3, asns.Length);
				var asnForContainer1 = asns.Single(a => a.WRP_VehicleReference == "CONT1");
				var asnForContainer2 = asns.Single(a => a.WRP_VehicleReference == "CONT2");
				var asnForNoContainer = asns.Single(a => a.WRP_VehicleReference == "MSB1");

				var packageStateForStandard = rcnForStandardShipment.PackageStates.Single();
				AssertEquals(asnForContainer1, packageStateForStandard.ReceiveASN);

				var packageStatesInSub1 = rcnForSub1.PackageStates;
				AssertEquals(1, packageStatesInSub1.Count);

				var packageStatesInSub2 = rcnForSub2.PackageStates;
				AssertEquals(2, packageStatesInSub2.Count);

				var packageStatesForBuyerConsolLeadMaster = rcnForMaster.PackageStates;
				AssertEquals(1, packageStatesForBuyerConsolLeadMaster.Count);

				AssertEquals(asnForContainer1, packageStatesInSub1.Single().ReceiveASN);
				AssertEquals(asnForContainer2, packageStatesInSub2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum).ReceiveASN);
				AssertEquals(asnForNoContainer, packageStatesInSub2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package).ReceiveASN);
				AssertEquals(asnForContainer1, packageStatesForBuyerConsolLeadMaster.Single().ReceiveASN);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 4 Dispatch Consignments", 4, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(dcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, shipment2.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, shipment3.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		void AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertConsignment(Factory, "SUB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
				var rcnForSub2 = AssertConsignment(Factory, "SUB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
				AssertEquals(3, asns.Length);
				var asnForContainer1 = asns.Single(a => a.WRP_VehicleReference == "CONT1");
				var asnForContainer2 = asns.Single(a => a.WRP_VehicleReference == "CONT2");
				var asnForNoContainer = asns.Single(a => a.WRP_VehicleReference == "MSB1");

				var packageStateForStandard = rcnForStandardShipment.PackageStates.Single();
				AssertEquals(asnForContainer1, packageStateForStandard.ReceiveASN);

				var packageStatesInSub1 = rcnForSub1.PackageStates;
				AssertEquals(1, packageStatesInSub1.Count);

				var packageStatesInSub2 = rcnForSub2.PackageStates;
				AssertEquals(2, packageStatesInSub2.Count);
				AssertEquals(asnForContainer1, packageStatesInSub1.Single().ReceiveASN);
				AssertEquals(asnForContainer2, packageStatesInSub2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Drum).ReceiveASN);
				AssertEquals(asnForNoContainer, packageStatesInSub2.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package).ReceiveASN);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				AssertLoadList(newBizOFactoryAfterRunningLogWalker, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(newBizOFactoryAfterRunningLogWalker, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, shipment2.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, shipment3.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveConsignments_ForGroupedShipments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		void AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveConsignments_ForSubShipments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(m => (ForwardingShipment)m));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		void AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				if (shipmentType == Constants.ShipmentTypes.AssemblyMaster)
				{
					var asmReferenceRcn1 = rcnForSub1.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					Helper.AssertAdditionalReference(asmReferenceRcn1, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "HSB1", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
					var asmReferenceRcn2 = rcnForSub2.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					Helper.AssertAdditionalReference(asmReferenceRcn2, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "HSB1", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				}

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				if (shipmentType == Constants.ShipmentTypes.AssemblyMaster)
				{
					var asmReferenceDcn1 = dcnForSub1.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					Helper.AssertAdditionalReference(asmReferenceDcn1, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "HSB1", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
					var asmReferenceDcn2 = dcnForSub2.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					Helper.AssertAdditionalReference(asmReferenceDcn1, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "HSB1", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				}

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipmentWithConsol_CreatingReceiveConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_CoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BlindCoLoadMaster_CreatingReceiveConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BuyersConsolLead_CreatingReceiveConsignments_ForGroupedShipments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton, container);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		void AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipmentWithConsol_CreatingReceiveConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_CoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BlindCoLoadMaster_CreatingReceiveConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BuyersConsolLead_CreatingReceiveConsignments_ForSubShipments()
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton, container);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(m => (ForwardingShipment)m));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		void AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				Factory.Save();

				// Dispatch consignments, headers and pick
				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		#endregion

		#endregion

		#region Creating Receive Consginments & Dispatch consignments

		#region TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BuyersConsol_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "Sub1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "Sub2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton, container2);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForMaster = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				AssertLoadList(Factory, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(Factory, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		void AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");

				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "Sub1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "Sub2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;
				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForMaster = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				AssertLoadList(Factory, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(Factory, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		#endregion

		#region TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton, container1);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(4, rcnsInTheSystem.Length);
				var rcnForMaster = AssertConsignment(Factory, "HSB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, masterShipment.PK, outboundLeg);
				var rcnForSub1 = AssertConsignment(Factory, "SUB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
				var rcnForSub2 = AssertConsignment(Factory, "SUB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				AssertLoadList(Factory, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(Factory, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 4 Dispatch Consignments", 4, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(dcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, shipment2.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, shipment3.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		void AssertImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined();
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var imp = Helper.CreateTRWWarehouse("IMP");
				var exp = Helper.CreateTRWWarehouse("EXP");
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));

				var shipment1 = CreateShipment(consol, "MAIN1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment2 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var shipment3 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				shipment1.JS_OA_ImportReleaseDepot = imp.WarehouseAddress.PK;
				shipment1.JS_OA_ExportReceivingDepot = exp.WarehouseAddress.PK;

				var container1 = CreateContainer(consol, "CONT1");
				var container2 = CreateContainer(consol, "CONT2");

				CreateOuterPackline(shipment1, 1, Constants.PkgUnit.Pallet, container1);
				CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Case, container1);
				CreateOuterPackline(shipment3, 2, Constants.PkgUnit.Drum, container2);
				CreateOuterPackline(shipment3, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { shipment2, shipment3 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				AssertEquals("Precondition: Should have 2 top level shipments", 2, consol.TopLevelShipments.Count);
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertConsignment(Factory, "SUB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment2.PK, outboundLeg);
				var rcnForSub2 = AssertConsignment(Factory, "SUB2", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment3.PK, outboundLeg);
				var rcnForStandardShipment = AssertConsignment(Factory, "MAIN1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment1.PK, outboundLeg);

				AssertLoadList(Factory, "AUSYD", "A", "AA", "01-Jan-18 00:00", consol.PK, "JK", "MSB1", consol.JK_UniqueConsignRef, outboundLeg);
				AssertDispatchTransportationUnits(Factory, new[] { "CONT1", "CONT2" });

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");
				var dcnForStandardShipment = dispatchConsignments.First(c => c.WDC_ConsignmentID == "MAIN1");

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, shipment2.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, shipment3.PK);
				AssertDispatchConsignmentPackageStates(rcnForStandardShipment.PackageStates, dcnForStandardShipment, shipment1.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		void AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(m => (ForwardingShipment)m));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		void AssertImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment("HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipmentWithConsol_CreatingReceiveAndDispatchConsignments_ForGroupedShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ForGroupedShipments()
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 2, Constants.PkgUnit.Carton, container);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		void AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForGroupedShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, false))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment("SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment("SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(1, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 1 Dispatch Consignments", 1, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
			}
		}

		#endregion

		#region TestImportForwardingShipmentWithConsol_CreatingReceiveAndDispatchConsignments_ForSubShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipmentWithConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ForSubShipments()
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);
				CreateOuterPackline(sub2, 1, Constants.PkgUnit.Package, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, Constants.ShipmentTypes.BuyersConsolLead);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Carton, container);
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(m => (ForwardingShipment)m));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(3, rcnsInTheSystem.Length);
				var rcnForMaster = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, masterShipment.PK);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 3 Dispatch Consignments", 3, dispatchConsignments.Length);

				var dcnForMaster = dispatchConsignments.First(c => c.WDC_ConsignmentID == "HSB1");
				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForMaster.PackageStates, dcnForMaster, masterShipment.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}

		void AssertImportForwardingShipmentWithConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ForSubShipments(string shipmentType)
		{
			var testData = CreateTestDataForCombined(isArrival: false, parentProcessIsConsol: false);
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Shipment.Code, false);

			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				var container = CreateContainer(consol, "CONT1");

				var sub1 = CreateShipment(consol, "SUB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				var sub2 = CreateShipment(consol, "SUB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				CreateOuterPackline(sub1, 1, Constants.PkgUnit.Pallet, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Case, container);
				CreateOuterPackline(sub2, 2, Constants.PkgUnit.Drum, container);

				var childShipments = new ForwardingShipment[] { sub1, sub2 };
				var masterShipment = CreateConsolidatedShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, childShipments, shipmentType: shipmentType);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
				AssertContainsExactElementsInAnyOrder(childShipments, masterShipment.CoLoadShipments.Select(s => (ForwardingShipment)s));
				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				// Receive consignments
				var rcnsInTheSystem = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, rcnsInTheSystem.Length);
				var rcnForSub1 = AssertAndReturnReceiveConsignment(Factory, "SUB1", testData.warehouse, sub1.PK);
				var rcnForSub2 = AssertAndReturnReceiveConsignment(Factory, "SUB2", testData.warehouse, sub2.PK);

				var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals("Should have created 2 Dispatch Consignments", 2, dispatchConsignments.Length);

				var dcnForSub1 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB1");
				var dcnForSub2 = dispatchConsignments.First(c => c.WDC_ConsignmentID == "SUB2");

				AssertDispatchConsignmentPackageStates(rcnForSub1.PackageStates, dcnForSub1, sub1.PK);
				AssertDispatchConsignmentPackageStates(rcnForSub2.PackageStates, dcnForSub2, sub2.PK);
			}
		}
		#endregion

		#region TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLeveledShipments

		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 2);
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 3);
		public void TestImportForwardingConsol_AssemblyMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 5);

		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 2);
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 3);
		public void TestImportForwardingConsol_CoLoadMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 5);

		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 2);
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 3);
		public void TestImportForwardingConsol_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 5);

		public void TestImportForwardingConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 2);
		public void TestImportForwardingConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 3);
		public void TestImportForwardingConsol_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 5);

		void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(
			string shipmentType, int level)
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));
				var container = CreateContainer(consol, "CONT1");

				var houseBillAndShipments = CreateMultiLevelShipmentsInConsol(shipmentType, level, consol, container, "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				Factory.Save();

				TriggerAndFireTransitRequestForRelease(consol);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var expectedConsignmentCount = shipmentType == Constants.ShipmentTypes.BuyersConsolLead ? level * 3 - 1 : level * 2;

				var receiveConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals($"Should have created {expectedConsignmentCount} Dispatch Consignments", expectedConsignmentCount, receiveConsignments.Length);

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals($"Should have created {expectedConsignmentCount} Dispatch Consignments", expectedConsignmentCount, dispatchConsignments.Length);

				var asns = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveASN>(new ZQuery());
				AssertEquals("Should have created 1 ASN", 1, asns.Length);
				var asn = asns.Single();

				var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
				AssertEquals("Should have created 1 LoadList", 1, loadLists.Length);
				var loadList = loadLists.Single();

				foreach (var houseBillAndShipment in houseBillAndShipments)
				{
					var houseBill = houseBillAndShipment.HouseBill;
					var shipment = houseBillAndShipment.Shipment;
					var rcn = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, houseBill, testData.warehouse, shipment.PK);
					var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == houseBill);
					AssertDispatchConsignmentPackageStates(rcn.PackageStates, dcn, shipment.PK);
					AssertEquals("All packages should be attached to single ASN", true, rcn.PackageStates.All(c => c.WPS_WRP_ReceiveExpectedPacking == asn.PK));
					AssertEquals("All packages should be attached to single LoadList", true, rcn.PackageStates.All(c => c.WPS_WDL_LoadList == loadList.PK));
				}
			}
		}

		List<(string HouseBill, ForwardingShipment Shipment)> CreateMultiLevelShipmentsInConsol(string shipmentType, int maxLevel, ForwardingConsol consol, ForwardingContainer container, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee)
		{
			var houseBillAndShipments = new List<(string HouseBill, ForwardingShipment Shipment)>();
			var currentSubshipments = Array.Empty<ForwardingShipment>();
			for (int level = maxLevel; level > 0; level--)
			{
				currentSubshipments = CreateSingleLevelShipmentsInConsol(level, currentSubshipments, shipmentType, consol, container, origin, destination, etd, eta, consignor, consignee, houseBillAndShipments);
			}
			return houseBillAndShipments;
		}

		ForwardingShipment[] CreateSingleLevelShipmentsInConsol(int level, ForwardingShipment[] subshipments, string shipmentType, ForwardingConsol consol, ForwardingContainer container, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee, List<(string HouseBill, ForwardingShipment Shipment)> houseBillAndShipments)
		{
			var std1 = CreateShipment(consol, $"STD1L{level}", origin, destination, etd, eta, consignor, consignee);
			var std2 = CreateShipment(consol, $"STD2L{level}", origin, destination, etd, eta, consignor, consignee);
			houseBillAndShipments.Add(($"STD1L{level}", std1));
			houseBillAndShipments.Add(($"STD2L{level}", std2));
			CreateOuterPackline(std1, 1, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(std2, 2, Constants.PkgUnit.Pallet, container);
			if (subshipments.Any())
			{
				var master = CreateConsolidatedShipment(consol, $"{shipmentType}L{level}", origin, destination, etd, eta, consignor, consignee, subshipments, shipmentType);
				if (shipmentType == Constants.ShipmentTypes.BuyersConsolLead)
				{
					CreateOuterPackline(master, 1, Constants.PkgUnit.Pallet, container);
					houseBillAndShipments.Add(($"{shipmentType}L{level}", master));
				}

				return new[] { std1, std2, master };
			}
			else
			{
				return new[] { std1, std2 };
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLeveledShipments

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
	TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 2);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 3);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_AssemblyMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.AssemblyMaster, 5);

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 2);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 3);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_CoLoadMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.CoLoadMaster, 5);

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 2);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 3);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BlindCoLoadMaster_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BlindCoLoadMaster, 5);

		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_TwoLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 2);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_ThreeLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 3);
		[TestDate(2018, 1, 1)]
		public void TestImportForwardingShipment_BuyersConsolLead_CreatingReceiveAndDispatchConsignments_FiveLeveledShipments() =>
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(Constants.ShipmentTypes.BuyersConsolLead, 5);

		void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_MultiLevelSubShipments(string shipmentType, int level)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBillAndShipments = CreateMultiLevelShipmentsInShipment(shipmentType, level, "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee, out var masterShipment);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				Factory.Save();

				TriggerAndFireTransitRequestForRelease(masterShipment);

				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

				var expectedConsignmentCount = shipmentType == Constants.ShipmentTypes.BuyersConsolLead ? level * 3 : level * 2;

				var receiveConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals($"Should have created {expectedConsignmentCount} Receive Consignments", expectedConsignmentCount, receiveConsignments.Length);

				var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
				AssertEquals($"Should have created {expectedConsignmentCount} Dispatch Consignments", expectedConsignmentCount, dispatchConsignments.Length);

				foreach (var houseBillAndShipment in houseBillAndShipments)
				{
					var houseBill = houseBillAndShipment.HouseBill;
					var shipment = houseBillAndShipment.Shipment;
					var rcn = AssertAndReturnReceiveConsignment(newBizOFactoryAfterRunningLogWalker, houseBill, testData.warehouse, shipment.PK);
					var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == houseBill);
					AssertDispatchConsignmentPackageStates(rcn.PackageStates, dcn, shipment.PK);
				}
			}
		}

		List<(string HouseBill, ForwardingShipment Shipment)> CreateMultiLevelShipmentsInShipment(string shipmentType, int maxLevel, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee, out ForwardingShipment masterShipment)
		{
			var houseBillAndShipments = new List<(string HouseBill, ForwardingShipment Shipment)>();
			var currentSubshipments = Array.Empty<ForwardingShipment>();
			for (int level = maxLevel; level > 0; level--)
			{
				currentSubshipments = CreateSingleLevelShipmentsInConsol(level, currentSubshipments, shipmentType, origin, destination, etd, eta, consignor, consignee, houseBillAndShipments);
			}
			masterShipment = CreateConsolidatedShipment($"HSB1", origin, destination, etd, eta, consignor, consignee, currentSubshipments, shipmentType);
			if (shipmentType == Constants.ShipmentTypes.BuyersConsolLead)
			{
				CreateOuterPackline(masterShipment, 1, Constants.PkgUnit.Pallet);
			}
			return houseBillAndShipments;
		}

		ForwardingShipment[] CreateSingleLevelShipmentsInConsol(int level, ForwardingShipment[] subshipments, string shipmentType, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee, List<(string HouseBill, ForwardingShipment Shipment)> houseBillAndShipments)
		{
			var std1 = CreateShipment($"STD1L{level}", origin, destination, etd, eta, consignor, consignee);
			var std2 = CreateShipment($"STD2L{level}", origin, destination, etd, eta, consignor, consignee);
			houseBillAndShipments.Add(($"STD1L{level}", std1));
			houseBillAndShipments.Add(($"STD2L{level}", std2));
			CreateOuterPackline(std1, 1, Constants.PkgUnit.Pallet);
			CreateOuterPackline(std2, 2, Constants.PkgUnit.Pallet);
			if (subshipments.Any())
			{
				var master = CreateConsolidatedShipment($"{shipmentType}L{level}", origin, destination, etd, eta, consignor, consignee, subshipments, shipmentType);
				if (shipmentType == Constants.ShipmentTypes.BuyersConsolLead)
				{
					CreateOuterPackline(master, 1, Constants.PkgUnit.Pallet);
					houseBillAndShipments.Add(($"{shipmentType}L{level}", master));
				}

				return new[] { std1, std2, master };
			}
			else
			{
				return new[] { std1, std2 };
			}
		}

		#endregion

		#region TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternA()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true)
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternB()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true),
					CLD("S00004", true)
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00004", "S00004" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00004" } };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternC()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true),
					CLD("S00004", false,
						STD("S00005", true),
						STD("S00006", true))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00005", "S00001" }, { "S00006", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternD()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						STD("S00003", true),
						STD("S00004", true)),
					CLD("S00005", false,
						STD("S00006", true),
						STD("S00007", true))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00003", "S00001" }, { "S00004", "S00001" }, { "S00006", "S00001" }, { "S00007", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternE()
		{
			var pattern =
				CLD("S00001", false,
					BCN("S00002", true,
						STD("S00003", true),
						STD("S00004", true)),
					BCN("S00005", true,
						STD("S00006", true),
						STD("S00007", true)),
					CLD("S00008", true)
					);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00004", "S00001" }, { "S00005", "S00005" }, { "S00006", "S00001" }, { "S00007", "S00001" }, { "S00008", "S00008" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00002" }, { "S00005", "S00005" }, { "S00008", "S00008" } };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternF()
		{
			var pattern =
				CLD("S00001", true);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00001", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00001", "S00001" } };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternG()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", true)))
					);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00004" } };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternH()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", false,
								STD("S00005", true),
								STD("S00006", true))))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00005", "S00001" }, { "S00006", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternI()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", true))),
					CLD("S00005", false,
						CLD("S00006", true)),
					CLD("S00007", false,
						CLD("S00008", false,
							STD("S00009", true),
							STD("S00010", true)))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" }, { "S00006", "S00006" }, { "S00009", "S00001" }, { "S00010", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00004" }, { "S00006", "S00006" } };
			TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		void TestImportForwardingConsol_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(ShipmentTreeNode pattern, Dictionary<string, string> rcnAsmCusEntryMapping, Dictionary<string, string> dcnAsmCusEntryMapping)
		{
			var testData = CreateTestData(true);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableImportingCoLoadMastersWithoutSubs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
				consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
				var outboundLeg = CreateTransport(consol, 1, "SEA", "A", "AA", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));
				var container = CreateContainer(consol, "CONT1");

				var expectedShipments = new List<(string shipmentId, ForwardingShipment shipment)>();
				var parentShipment = CreateShipmentsByShipmentPatternInConsol(consol, container, pattern, expectedShipments, "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				Factory.Save();

				TriggerAndFireTransitRequestForRelease(consol);

				AssertShipmentsWithPattern(testData.warehouse, expectedShipments);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var asmRefNumbers = Factory.Load<CusEntryNumber>(new ZQuery()).Where(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);

				foreach (var shipmentIdAndShipment in expectedShipments)
				{
					var shipmentId = shipmentIdAndShipment.shipmentId;
					var shipment = shipmentIdAndShipment.shipment;
					AssertAndReturnReceiveConsignment(newFactory, shipmentId, testData.warehouse, shipment.PK, asmRefNumbers, rcnAsmCusEntryMapping, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);

					string dcnAsmRefNumber;
					if (dcnAsmCusEntryMapping.TryGetValue(shipmentId, out dcnAsmRefNumber))
					{
						AssertAndReturnDispatchConsignment(newFactory, shipmentId, testData.warehouse, shipment.PK, asmRefNumbers, dcnAsmCusEntryMapping, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					}
				}
			}
		}

		ForwardingShipment CreateShipmentsByShipmentPatternInConsol(ForwardingConsol consol, ForwardingContainer container, ShipmentTreeNode pattern, List<(string shipmentId, ForwardingShipment shipment)> expectedShipments, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee)
		{
			if (pattern.Type == Constants.ShipmentTypes.CoLoadMaster || pattern.Type == Constants.ShipmentTypes.BlindCoLoadMaster || pattern.Type == Constants.ShipmentTypes.BuyersConsolLead)
			{
				var childShipments = pattern.Children.Select(s => CreateShipmentsByShipmentPatternInConsol(consol, container, s, expectedShipments, origin, destination, etd, eta, consignor, consignee)).ToArray();
				var shipment = CreateConsolidatedShipment(consol, pattern.ShipmentID, origin, destination, etd, eta, consignor, consignee, childShipments, pattern.Type);
				if (pattern.ShouldImport)
				{
					CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
					expectedShipments.Add((pattern.ShipmentID, shipment));
				}
				return shipment;
			}
			else
			{
				var shipment = CreateShipment(consol, pattern.ShipmentID, origin, destination, etd, eta, consignor, consignee);
				CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
				if (pattern.ShouldImport)
				{
					expectedShipments.Add((pattern.ShipmentID, shipment));
				}
				return shipment;
			}
		}

		#endregion

		#region TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternA()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true)
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternB()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true),
					CLD("S00004", true)
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00004", "S00004" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00004" } };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternC()
		{
			var pattern =
				CLD("S00001", false,
					STD("S00002", true),
					STD("S00003", true),
					CLD("S00004", false,
						STD("S00005", true),
						STD("S00006", true))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00005", "S00001" }, { "S00006", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternD()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						STD("S00003", true),
						STD("S00004", true)),
					CLD("S00005", false,
						STD("S00006", true),
						STD("S00007", true))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00003", "S00001" }, { "S00004", "S00001" }, { "S00006", "S00001" }, { "S00007", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternE()
		{
			var pattern =
				CLD("S00001", false,
					BCN("S00002", true,
						STD("S00003", true),
						STD("S00004", true)),
					BCN("S00005", true,
						STD("S00006", true),
						STD("S00007", true)),
					CLD("S00008", true)
					);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00004", "S00001" }, { "S00005", "S00005" }, { "S00006", "S00001" }, { "S00007", "S00001" }, { "S00008", "S00008" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00002", "S00001" }, { "S00003", "S00001" }, { "S00004", "S00001" }, { "S00005", "S00005" }, { "S00006", "S00001" }, { "S00007", "S00001" }, { "S00008", "S00008" } };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternF()
		{
			var pattern =
				CLD("S00001", true);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00001", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00001", "S00001" } };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternG()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", true)))
					);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" } };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternH()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", false,
								STD("S00005", true),
								STD("S00006", true))))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00005", "S00001" }, { "S00006", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		public void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_PatternI()
		{
			var pattern =
				CLD("S00001", false,
					CLD("S00002", false,
						CLD("S00003", false,
							CLD("S00004", true))),
					CLD("S00005", false,
						CLD("S00006", true)),
					CLD("S00007", false,
						CLD("S00008", false,
							STD("S00009", true),
							STD("S00010", true)))
				);
			var rcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" }, { "S00006", "S00006" }, { "S00009", "S00001" }, { "S00010", "S00001" } };
			var dcnAsmCusEntryMapping = new Dictionary<string, string>() { { "S00004", "S00001" }, { "S00006", "S00006" }, { "S00009", "S00001" }, { "S00010", "S00001" } };
			TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(pattern, rcnAsmCusEntryMapping, dcnAsmCusEntryMapping);
		}

		void TestImportForwardingShipment_CreatingReceiveAndDispatchConsignments_EnableImportingEmptyColoads_WithPattern(ShipmentTreeNode pattern, Dictionary<string, string> rcnAsmCusEntryMapping, Dictionary<string, string> dcnAsmCusEntryMapping)
		{
			var testData = CreateTestData(false, parentProcessIsConsol: false);
			var branchPK = testData.warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.EnableImportingCoLoadMastersWithoutSubs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedShipments = new List<(string shipmentId, ForwardingShipment shipment)>();
				var masterShipment = CreateShipmentsByShipmentPattern(pattern, expectedShipments, "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
				masterShipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;

				Factory.Save();

				TriggerAndFireTransitRequestUsingBookingRequested(masterShipment);

				Factory.Save();

				TriggerAndFireTransitRequestForRelease(masterShipment);

				AssertShipmentsWithPattern(testData.warehouse, expectedShipments);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var asmRefNumbers = Factory.Load<CusEntryNumber>(new ZQuery()).Where(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);

				foreach (var shipmentIdAndShipment in expectedShipments)
				{
					var shipmentId = shipmentIdAndShipment.shipmentId;
					var shipment = shipmentIdAndShipment.shipment;
					AssertAndReturnReceiveConsignment(newFactory, shipmentId, testData.warehouse, shipment.PK, asmRefNumbers, rcnAsmCusEntryMapping, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);

					string dcnAsmRefNumber;
					if (dcnAsmCusEntryMapping.TryGetValue(shipmentId, out dcnAsmRefNumber))
					{
						AssertAndReturnDispatchConsignment(newFactory, shipmentId, testData.warehouse, shipment.PK, asmRefNumbers, dcnAsmCusEntryMapping, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
					}
				}
			}
		}

		void AssertShipmentsWithPattern(WhsWarehouse warehouse, List<(string shipmentId, ForwardingShipment shipment)> expectedShipments)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var expectedConsignmentCount = expectedShipments.Count;

			var receiveConsignments = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals($"Should have created {expectedConsignmentCount} Receive Consignments", expectedConsignmentCount, receiveConsignments.Length);

			var dispatchConsignments = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals($"Should have created {expectedConsignmentCount} Dispatch Consignments", expectedConsignmentCount, dispatchConsignments.Length);

			foreach (var shipmentIdAndShipment in expectedShipments)
			{
				var shipmentId = shipmentIdAndShipment.shipmentId;
				var shipment = shipmentIdAndShipment.shipment;
				var rcn = AssertAndReturnReceiveConsignment(newFactory, shipmentId, warehouse, shipment.PK);
				var dcn = dispatchConsignments.Single(c => c.WDC_ConsignmentID == shipmentId);
				AssertDispatchConsignmentPackageStates(rcn.PackageStates, dcn, shipment.PK);
			}
		}

		ForwardingShipment CreateShipmentsByShipmentPattern(ShipmentTreeNode pattern, List<(string shipmentId, ForwardingShipment shipment)> expectedShipments, string origin, string destination, ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee)
		{
			if (pattern.Type == Constants.ShipmentTypes.CoLoadMaster || pattern.Type == Constants.ShipmentTypes.BlindCoLoadMaster || pattern.Type == Constants.ShipmentTypes.BuyersConsolLead)
			{
				var childShipments = pattern.Children.Select(s => CreateShipmentsByShipmentPattern(s, expectedShipments, origin, destination, etd, eta, consignor, consignee)).ToArray();
				var shipment = CreateConsolidatedShipment(pattern.ShipmentID, origin, destination, etd, eta, consignor, consignee, childShipments, pattern.Type);
				if (pattern.ShouldImport)
				{
					CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
					expectedShipments.Add((pattern.ShipmentID, shipment));
				}
				return shipment;
			}
			else
			{
				var shipment = CreateShipment(pattern.ShipmentID, origin, destination, etd, eta, consignor, consignee);
				CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
				if (pattern.ShouldImport)
				{
					expectedShipments.Add((pattern.ShipmentID, shipment));
				}
				return shipment;
			}
		}

		ShipmentTreeNode BCN(string shipmentId, bool shouldImport, params ShipmentTreeNode[] children)
		{
			return new ShipmentTreeNode(Constants.ShipmentTypes.BuyersConsolLead, shipmentId, shouldImport, children);
		}

		ShipmentTreeNode CLD(string shipmentId, bool shouldImport, params ShipmentTreeNode[] children)
		{
			return new ShipmentTreeNode(Constants.ShipmentTypes.CoLoadMaster, shipmentId, shouldImport, children);
		}

		ShipmentTreeNode STD(string shipmentId, bool shouldImport, params ShipmentTreeNode[] children)
		{
			return new ShipmentTreeNode(Constants.ShipmentTypes.StandardHouse, shipmentId, shouldImport, children);
		}

		class ShipmentTreeNode
		{
			public ShipmentTreeNode(string type, string shipmentId, bool shouldImport, ShipmentTreeNode[] children)
			{
				Type = type;
				ShipmentID = shipmentId;
				Children = children;
				ShouldImport = shouldImport;
			}
			public string Type { get; set; }
			public string ShipmentID { get; set; }
			public ShipmentTreeNode[] Children { get; set; }
			public bool ShouldImport { get; set; }
		}

		#endregion

		#endregion
	}
}
