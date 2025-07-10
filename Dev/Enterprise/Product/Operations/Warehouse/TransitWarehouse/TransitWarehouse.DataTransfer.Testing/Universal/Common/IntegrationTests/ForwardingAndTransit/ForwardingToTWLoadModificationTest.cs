using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ForwardingToTWLoadModificationTest : IntegrationTestCaseWithFactory
	{
		#region Matching Dispatch Load Lists Resending same containers

		public void TestConsol_ResendingDispatchInstructions_ContainerNumbersAndNotStrictlyPackedIn()
		{
			/*
				1. Send Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A
					Container B
					Shipment A
						PKG1 PLT 1
				2. Resend same Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A
					Container B
					Shipment A
						PKG1 PLT 1
				3. Existing load list get matched.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container1 = CreateContainer(consol, "CNT1");
			var container2 = CreateContainer(consol, "CNT2");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 2, shipment.OuterPackLines.Count);
			var newPLTPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			newPLTPackLine.SetContainer(consol, container1);
			newDRMPackLine.Containers.RemoveAll();
			Factory.Save();

			// 1.Send Dispatch Instruction
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var drm1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(drm1.PK);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);
			var loadListForContainer1 = loadLists.Single(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1");
			var loadListForContainer2 = loadLists.Single(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2");
			AssertEquals(plt1InNewFactory.WPS_WDL_LoadList, loadListForContainer1.PK);
			AssertEquals(drm1InNewFactory.WPS_WDL_LoadList, loadListForContainer2.PK);

			// 2. Resend same Dispatch Instruction
			TriggerAndFireTransitRequestForRelease(consol);

			// 3. Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(plt1.PK);
			var drm1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(drm1.PK);

			var loadListsAfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadListsAfterResending.Length);
			var loadListForContainer1AfterResending = loadListsAfterResending.Single(l => l.PK == loadListForContainer1.PK && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1");
			var loadListForContainer2AfterResending = loadListsAfterResending.Single(l => l.PK == loadListForContainer2.PK && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2");
			AssertEquals(plt1AfterResending.WPS_WDL_LoadList, loadListForContainer1AfterResending.PK);
			AssertEquals(drm1AfterResending.WPS_WDL_LoadList, loadListForContainer2AfterResending.PK);
			AssertContainsExactElementsInAnyOrder(loadListForContainer1.DispatchTransportationUnits.Select(d => d.PK), loadListForContainer1AfterResending.DispatchTransportationUnits.Select(d => d.PK));
			AssertContainsExactElementsInAnyOrder(loadListForContainer2.DispatchTransportationUnits.Select(d => d.PK), loadListForContainer2AfterResending.DispatchTransportationUnits.Select(d => d.PK));
		}

		public void TestConsol_ResendingDispatchInstructions_ContainerNumbersAreStrictlyPackedIn()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container A
						Shipment A
							PKG1 PLT 1
					Container B
						Shipment A
							DRM1 DRM 1
					
				2. Resend Dispatch Instructions
					Consol A - Master Bill MAB1
					Container A
						Shipment A
							PKG1 PLT 1
					Container B
						Shipment A
							DRM1 DRM 1
				3. Existing load list get matched.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container1 = CreateContainer(consol, "CNT1");
			var container2 = CreateContainer(consol, "CNT2");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 2, shipment.OuterPackLines.Count);
			var newPLTPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			newPLTPackLine.SetContainer(consol, container1);
			newDRMPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var drm1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(drm1.PK);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadLists.Length);
			var loadListForContainer1 = loadLists.Single(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1");
			var loadListForContainer2 = loadLists.Single(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2");
			AssertEquals(plt1InNewFactory.WPS_WDL_LoadList, loadListForContainer1.PK);
			AssertEquals(drm1InNewFactory.WPS_WDL_LoadList, loadListForContainer2.PK);

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(plt1.PK);
			var drm1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(drm1.PK);

			var loadListsAfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, loadListsAfterResending.Length);
			var loadListForContainer1AfterResending = loadListsAfterResending.Single(l => l.PK == loadListForContainer1.PK && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1");
			var loadListForContainer2AfterResending = loadListsAfterResending.Single(l => l.PK == loadListForContainer2.PK && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2");
			AssertEquals(plt1AfterResending.WPS_WDL_LoadList, loadListForContainer1AfterResending.PK);
			AssertEquals(drm1AfterResending.WPS_WDL_LoadList, loadListForContainer2AfterResending.PK);
			AssertContainsExactElementsInAnyOrder(loadListForContainer1.DispatchTransportationUnits.Select(d => d.PK), loadListForContainer1AfterResending.DispatchTransportationUnits.Select(d => d.PK));
			AssertContainsExactElementsInAnyOrder(loadListForContainer2.DispatchTransportationUnits.Select(d => d.PK), loadListForContainer2AfterResending.DispatchTransportationUnits.Select(d => d.PK));
		}

		public void TestConsol_ResendingDispatchInstructions_GroupedContainers()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					2x 20GP Containers
					Shipment A
						PKG1 PLT 1
				2. Add second container and send again
					Consol A - Master Bill MAB1
					2x 20GP Containers
					Shipment A
						PKG1 PLT 1
				3. Existing load list get matched.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var groupedContainers = CreateContainer(consol, "", 2, "20GP");
			var containerWithNumber = CreateContainer(consol, "CNT2");
			var containerWithoutNumber = CreateContainer(consol, "", 1, "40GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var rel = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Reel, "REL1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1, rel });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 3, shipment.OuterPackLines.Count);
			var newPLTPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			var newRELPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Reel);
			newPLTPackLine.SetContainer(consol, groupedContainers);
			newDRMPackLine.SetContainer(consol, containerWithNumber);
			newRELPackLine.SetContainer(consol, containerWithoutNumber);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var drm1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(drm1.PK);
			var relInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(rel.PK);

			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(3, loadLists.Length);
			var loadListForGroupedContainers = loadLists.Where(l => l.DispatchTransportationUnits.Count == 2).Single();
			var loadListForContainerWithNumber = loadLists.Where(l => l.DispatchTransportationUnits.Count == 1 && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			var loadListForContainerWithoutNumber = loadLists.Where(l => l.DispatchTransportationUnits.Count == 1 && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "").Single();
			AssertEquals(plt1InNewFactory.WPS_WDL_LoadList, loadListForGroupedContainers.PK);
			AssertEquals(drm1InNewFactory.WPS_WDL_LoadList, loadListForContainerWithNumber.PK);
			AssertEquals(relInNewFactory.WPS_WDL_LoadList, loadListForContainerWithoutNumber.PK);

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(plt1.PK);
			var drm1AfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(drm1.PK);
			var relAfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemPackageState>(rel.PK);

			var loadListsAfterResending = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(3, loadListsAfterResending.Length);
			var loadListForGroupedContainersAfterResending = loadListsAfterResending.Where(l => l.PK == loadListForGroupedContainers.PK).Single();
			var loadListForContainerWithNumberAfterResending = loadListsAfterResending.Where(l => l.PK == loadListForContainerWithNumber.PK).Single();
			var loadListForContainerWithoutNumberAfterResending = loadListsAfterResending.Where(l => l.PK == loadListForContainerWithoutNumber.PK).Single();
			CombineAssertions(() =>
			{
				AssertEquals("Grouped container count must be 2.", 2, loadListForGroupedContainersAfterResending.DispatchTransportationUnits.Count);
				AssertEquals("Container with number must remain.", "CNT2", loadListForContainerWithNumberAfterResending.DispatchTransportationUnits.Single().WDH_VehicleReference);
				AssertEquals("Container without number must remain.", "", loadListForContainerWithoutNumberAfterResending.DispatchTransportationUnits.Single().WDH_VehicleReference);
				AssertEquals(plt1AfterResending.WPS_WDL_LoadList, loadListForGroupedContainersAfterResending.PK);
				AssertEquals(drm1AfterResending.WPS_WDL_LoadList, loadListForContainerWithNumberAfterResending.PK);
				AssertEquals(relAfterResending.WPS_WDL_LoadList, loadListForContainerWithoutNumberAfterResending.PK);
				AssertContainsExactElementsInAnyOrder(loadListForGroupedContainers.DispatchTransportationUnits.Select(d => d.PK), loadListForGroupedContainersAfterResending.DispatchTransportationUnits.Select(d => d.PK));
				AssertContainsExactElementsInAnyOrder(loadListForContainerWithNumber.DispatchTransportationUnits.Select(d => d.PK), loadListForContainerWithNumberAfterResending.DispatchTransportationUnits.Select(d => d.PK));
				AssertContainsExactElementsInAnyOrder(loadListForContainerWithoutNumber.DispatchTransportationUnits.Select(d => d.PK), loadListForContainerWithoutNumberAfterResending.DispatchTransportationUnits.Select(d => d.PK));
			});
		}

		public void TestConsol_ResendingDispatchInstructions_NoContainerIDAndNoPackageIDs()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "", containerCount: 1, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container, "");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcn1, 1);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListForContainer = loadLists.Single();

			var dtuForLoadList = loadListForContainer.DispatchTransportationUnits.Single();
			AssertEquals("", dtuForLoadList.ContainerNumber);

			var packageStatesForContainer = loadListForContainer.PackageStates;
			AssertEquals(1, packageStatesForContainer.Count);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { drm1 });
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterResending = newBizOFactoryAfterResendingDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForContainer.PK);

			var dtuAfterResending = loadListAfterResending.DispatchTransportationUnits.Single();
			AssertEquals("", dtuAfterResending.ContainerNumber);

			var packageStatesForContainerAfterResending = loadListAfterResending.PackageStates;
			AssertEquals(2, packageStatesForContainerAfterResending.Count);
		}

		public void TestConsol_ResendingDispatchInstructions_ContainerGroupsAndPackageIDs()
		{
			var testData = CreateTestData(true);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var container = CreateContainer(consol, containerNum: "", containerCount: 3, containerTypeCode: "20GP");
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container, "");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcn1 = AssertAndReturnReceiveConsignment(Factory, "HSB1", testData.warehouse, shipment.PK);
			AssertConsignmentPackages(rcn1, 1);

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListForContainerGroup = loadLists.Single();

			var dtusForLoadList = loadListForContainerGroup.DispatchTransportationUnits;
			AssertEquals(3, dtusForLoadList.Count);

			var packageStatesForContainer = loadListForContainerGroup.PackageStates;
			AssertEquals(1, packageStatesForContainer.Count);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { drm1 });
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterResendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterResending = newBizOFactoryAfterResendingDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForContainerGroup.PK);

			var dtusAfterResending = loadListAfterResending.DispatchTransportationUnits;
			AssertEquals(3, dtusAfterResending.Count);

			var packageStatesForContainerAfterResending = loadListAfterResending.PackageStates;
			AssertEquals(2, packageStatesForContainerAfterResending.Count);
		}

		#endregion

		#region Matching Dispatch LoadLists

		public void TestConsol_NoContainers_StartLoading_SendDispatchInstructionsAgain()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Shipment A
						PKG1 PLT 1
						
				2. Resend dispatch instructions
					Consol A - Master Bill MAB1
					Shipment A
						PKG1 PLT 1 <== In TW this loaded.
					
				3. Match to existing load list.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Helper.CreateAdditionalReference(loadList, "C00001000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(0, loadList.DispatchTransportationUnits.Count);

			var dtu = new WhsTransitTestHelper(newBizOFactoryAfterRunningLogWalker).CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var pivot = newBizOFactoryAfterRunningLogWalker.New<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDH_TransitDispatchTransportationUnit = dtu.PK;
			pivot.WLD_WDL_TransitDispatchLoadList = loadList.PK;
			newBizOFactoryAfterRunningLogWalker.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(false, loadListAfterReSendingDispatchInstructions.WDL_IsReadyToStage);
			AssertEquals(plt1InNewFactory.PK, loadListAfterReSendingDispatchInstructions.PackageStates.Single().PK);
			AssertEquals("Load list has been matched.", loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
		}

		public void TestConsol_NoContainers_AddNewShipment()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Shipment A
						PKG1 PLT 1
				2. Add second shipment and send again
					Consol A - Master Bill MAB1
					Shipment A
						PKG1 PLT 1
					Shipment B
						PKG2 PLT 1
				3. New shipment added and send dispatch instructions.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);

			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { plt2 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			newPLTPackLine.SetContainer(consol, container);
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(false, loadListAfterReSendingDispatchInstructions.WDL_IsReadyToStage);
			AssertEquals(2, loadListAfterReSendingDispatchInstructions.PackageStates.Count);
			AssertEquals("Load list has been matched.", loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
		}

		public void TestConsol_AddNewContainerBeforeSendingDispatchInstructions_WithMasterBill()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 1
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Add second container and send again
					Consol A - Master Bill MAB1
					Container 1
					Container 2
					Shipment A
						PKG1 PLT 1 – Container 1
						DRM1 DRM 1 - Unassigned
				3. Existing load list get matched and second container is attached to a new load list.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);

			// Add a new container
			var container2 = CreateContainer(consol, "CNT2");
			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1 });

			Factory.Save();
			AssertEquals("Precondition: There must be two outer packline for the shipment.", 2, shipment.OuterPackLines.Count);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			newDRMPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("There must be two load lists.", 2, loadListAfterReSendingDispatchInstructions.Length);
			var loadListCreatedForContainer1 = loadListAfterReSendingDispatchInstructions.Single(l => l.PK == loadList.PK);
			AssertEquals(false, loadListCreatedForContainer1.WDL_IsReadyToStage);
			AssertEquals(plt1.PK, loadListCreatedForContainer1.PackageStates.Single().PK);

			var newLoadListCreatedForContainer2 = loadListAfterReSendingDispatchInstructions.Single(l => l.PK != loadList.PK);
			AssertEquals(false, newLoadListCreatedForContainer2.WDL_IsReadyToStage);
			AssertEquals(drm1.PK, newLoadListCreatedForContainer2.PackageStates.Single().PK);
		}

		public void TestConsol_SameContainers_MasterBillChanged()
		{
			/*
				1.Send dispatch instructions
					Consol A - MAB1
					Container 1
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Add master bill and send again
					Consol A - MAB2
					Container 1
					Shipment A
						PKG1 PLT 1 – Container 1
				3. Existing load list get matched and masterbill updated.
			*/
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(plt1.PK, loadList.PackageStates.Single().PK);

			// Master bill changes
			consol.JK_MasterBillNum = "MAB2";
			Factory.Save();
			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(plt1.PK, loadListAfterReSendingDispatchInstructions.PackageStates.Single().PK);
		}

		public void TestConsol_SameContainers_MasterBill_AddedLater()
		{
			/*
				1.Send dispatch instructions
					Consol A - No Master Bill
					Container 1
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Add master bill and send again
					Consol A - Master Bill MAB1
					Container 1
					Shipment A
						PKG1 PLT 1 – Container 1
				3. Existing load list get matched and masterbill updated.
			*/
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(plt1.PK, loadList.PackageStates.Single().PK);

			consol.JK_MasterBillNum = "MAB1";
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(plt1.PK, loadListAfterReSendingDispatchInstructions.PackageStates.Single().PK);
		}

		public void TestConsol_SameContainers_ContainerNumberChanged()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 1 - CNT1
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Add second container and send again
					Consol A - Master Bill MAB1
					Container 2 <== Container number changed to ABCD (Container 1 exist in it's DTU)
					Shipment A
						PKG1 PLT 1 – Container 2
				3. Existing load list get matched, second container get attached. we will keep extra container - consider in later work item.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(plt1.PK, loadList.PackageStates.Single().PK);

			// container number changed
			container.JC_ContainerNum = "ABCD";
			Factory.Save();
			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			var oldLoadList = loadListAfterReSendingDispatchInstructions.Single(l => l.PK == loadList.PK);
			AssertEquals(false, oldLoadList.WDL_IsActive);
			AssertEquals(0, oldLoadList.DispatchTransportationUnits.Count);
			AssertEquals("ABCD", loadListAfterReSendingDispatchInstructions.Single(l => l.PK != loadList.PK).DispatchTransportationUnits.Single().WDH_VehicleReference);
		}

		public void TestConsol_NoContainerNumber_ButTransitEntersContainerNumber_MatchesToSameContainerUsingType()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 1 - Empty
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Container arrives at the gate and container number entered in to TWH
					Consol A - Master Bill MAB1
					Container 2 <== Container number changed to ABCD in TW (Container 1 exist in it's DTU)
					Shipment A
						PKG1 PLT 1 – Container 2
				3. Resend DispatchInstructions
				4. Existing load list get matched and blank(unknown) container matches to TWH container with ID.
					Matches to same container using container type.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 1);
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(plt1.PK, loadList.PackageStates.Single().PK);
			AssertEquals("", loadList.DispatchTransportationUnits.Single().WDH_VehicleReference);

			// container number changed
			loadList.DispatchTransportationUnits.Single().WDH_VehicleReference = "ABCD";
			newBizOFactoryAfterRunningLogWalker.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(plt1.PK, loadListAfterReSendingDispatchInstructions.PackageStates.Single().PK);
			AssertEquals("ABCD", loadListAfterReSendingDispatchInstructions.DispatchTransportationUnits.Single().WDH_VehicleReference);
		}

		public void TestConsol_GroupedContainers_AddExtraGroupedContainers()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Shipment A
						PKG1 PLT 1 – Container 2x 20GP
				2. Add second container, attach extra package to second container and resend
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Container 3x 40GP
					Shipment A
						PKG1 PLT 1 – Container 2x 20GP
						PKG1 DRM 1 – Container 3x 40GP
				3. Existing load list, DTUs for 2x20GP get matched and new DTUs created for 3x 40GP.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 2, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);

			// Add new grouped containers
			var container2 = CreateContainer(consol, "", 3, "40GP");
			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1 });

			Factory.Save();
			AssertEquals("Precondition: There must be two outer packline for the shipment.", 2, shipment.OuterPackLines.Count);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			newDRMPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("There must be two load lists.", 2, loadListAfterReSendingDispatchInstructions.Length);
			var loadListCreatedForContainerGroup1 = loadListAfterReSendingDispatchInstructions.Single(l => l.PK == loadList.PK);
			AssertEquals(false, loadListCreatedForContainerGroup1.WDL_IsReadyToStage);
			AssertEquals(plt1.PK, loadListCreatedForContainerGroup1.PackageStates.Single().PK);
			AssertEquals(2, loadListCreatedForContainerGroup1.DispatchTransportationUnits.Count);

			var newLoadListCreatedForContainerGroup2 = loadListAfterReSendingDispatchInstructions.Single(l => l.PK != loadList.PK);
			AssertEquals(false, newLoadListCreatedForContainerGroup2.WDL_IsReadyToStage);
			AssertEquals(drm1.PK, newLoadListCreatedForContainerGroup2.PackageStates.Single().PK);
			AssertEquals(3, newLoadListCreatedForContainerGroup2.DispatchTransportationUnits.Count);
		}

		public void TestConsol_GroupedContainers_ContainerCountIncreased()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Container count increased and resend
					Consol A - Master Bill MAB1
					Container 3x 20GP
					Shipment A
						PKG1 PLT 1 – Container 2
				3. Existing load list get matched and additional DTU get created.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 2, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(2, loadList.DispatchTransportationUnits.Count);

			// Container count increased
			container.JC_ContainerCount = 3;
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(3, loadListAfterReSendingDispatchInstructions.DispatchTransportationUnits.Count);
		}

		public void TestConsol_GroupedContainers_ContainerCountDecreased()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Shipment A
						PKG1 PLT 1 – Container 2x 20GP
				2. Decrease container count and Resend dispatch instructions
					Consol A - Master Bill MAB1
					Container 1x 20GP
					Shipment A
						PKG1 PLT 1 – Container 1x20GP
				3. All DTUs remain as before.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 3, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(3, loadList.DispatchTransportationUnits.Count);

			// Container count decreased
			container.JC_ContainerCount = 2;
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// Existing load list get matched
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(3, loadListAfterReSendingDispatchInstructions.DispatchTransportationUnits.Count);
		}

		public void TestConsol_GroupedContainers_ContainerTypeChanged()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Shipment A
						PKG1 PLT 1 – Container 2x 20GP
				2. Container Type changed and Resend dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 40GP
					Shipment A
						PKG1 PLT 1 – Container 2x 40GP
				3. Existing load list for 2x 20GP remains with 0 DTUs and extra load list added for 2x40GP and 2 DTUs.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 2, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(2, loadList.DispatchTransportationUnits.Count);

			// container type changed
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			loadList = loadListAfterReSendingDispatchInstructions.Single(l => l.PK == loadList.PK);
			AssertEquals(true, loadList.WDL_IsActive);
			AssertEquals(4, loadList.DispatchTransportationUnits.Count);
		}

		public void TestConsol_GroupedContainers_ContainerCountAndTypeChanged()
		{
			/*
				1.Send dispatch instructions
					Consol A - Master Bill MAB1
					Container 2x 20GP
					Shipment A
						PKG1 PLT 1 – Container 1
				2. Add second container and send again
					Consol A - Master Bill MAB1
					Container 3x 40GP
					Shipment A
						PKG1 PLT 1 – Container 2
				3. New load list and 3 DTUs get created, packages are moved to the new load list. Old Load List remains with 0 DTUs.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 2, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(2, loadList.DispatchTransportationUnits.Count);

			// container type and count changed
			container.JC_ContainerCount = 3;
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			// New load list get created
			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			loadList = loadListAfterReSendingDispatchInstructions.Single(l => l.PK == loadList.PK);
			AssertEquals(true, loadList.WDL_IsActive);
			AssertEquals(5, loadList.DispatchTransportationUnits.Count);
		}

		#endregion

		#region Adding New Shipments

		#region TestTWReceivesBlindConsignment

		public void TestTWReceivesBlindConsignment_Forwarder_Add_NewShipmentAfterSendingDispatchInstructions()
		{
			var data = SetupDataForTWReceivesBlindPackages();
			var consol = data.consol;
			var shipment = data.shipment;
			var container = data.container;
			var dtu = data.dtu;
			var loadList = data.dll;

			// loading packages to DTU
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var pkg1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var ctn1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtu, pkg1);
			Helper.LoadPackage(dtu, ctn1);
			loadList.Factory.Save();

			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			Factory.Save();

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container);
			newBAGPackLine.SetContainer(consol, container);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(data.consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtuAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadListAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(false, loadListAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(2, dtuAfterSendingDispatchInstructionsAgain.PackageStates.Count);
			var packageStates = loadListAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(5, packageStates.Count);
			AssertPackage(packageStates, "PKG1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStates, "PKG2", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "PKG3", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "CTN1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStates, "CTN2", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestTWReceivesBlindConsignment_NewShipmentAddedAfterSendingDispatchInstructions_ContainerLoadCompleted()
		{
			var data = SetupDataForTWReceivesBlindPackages();
			var consol = data.consol;
			var shipment = data.shipment;
			var container = data.container;
			var dtu = data.dtu;
			var loadList = data.dll;

			// loading packages to DTU
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var pkg1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var ctn1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtu, pkg1);
			Helper.LoadPackage(dtu, pkg2);
			Helper.LoadPackage(dtu, ctn1);
			Helper.FinishLoading(dtu);
			loadList.Factory.Save();

			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container);
			newBAGPackLine.SetContainer(consol, container);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(data.consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(1, dispatchConsignments.Length);

			var dtuAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadListAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(false, loadListAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(3, dtuAfterSendingDispatchInstructionsAgain.PackageStates.Count);
			var packageStates = loadListAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStates.Count);
			AssertPackage(packageStates, "PKG1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStates, "PKG2", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStates, "CTN1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
		}

		public void TestTWReceivesBlindConsignment_NewShipmentAddedAfterSendingDispatchInstructions_ContainerLoadCompleted_AllNewPackagesForDifferentContainer()
		{
			var data = SetupDataForTWReceivesBlindPackages();
			var consol = data.consol;
			var shipment = data.shipment;
			var container = data.container;
			var dtu = data.dtu;
			var loadList = data.dll;

			// loading packages to DTU
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var pkg1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var ctn1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtu, pkg1);
			Helper.LoadPackage(dtu, pkg2);
			Helper.LoadPackage(dtu, ctn1);
			Helper.FinishLoading(dtu);
			loadList.Factory.Save();

			// Add an extra shipment
			var container2 = CreateContainer(consol, "CNT2");
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container2);
			newBAGPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(data.consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadListsAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, dtusAfterSendingDispatchInstructionsAgain.Length);
			AssertEquals(2, loadListsAfterSendingDispatchInstructionsAgain.Length);

			var dtuForCNT1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "CNT1").Single();
			var dllForCNT1 = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			AssertEquals(3, dtuForCNT1.PackageStates.Count);
			var packagesForLoadListForCNT1 = loadListsAfterSendingDispatchInstructionsAgain.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var packageStatesInCNT1 = packagesForLoadListForCNT1.PackageStates;
			AssertEquals(3, packageStatesInCNT1.Count);
			AssertPackage(packageStatesInCNT1, "PKG1", dtuForCNT1, dllForCNT1, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesInCNT1, "PKG2", dtuForCNT1, dllForCNT1, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesInCNT1, "CTN1", dtuForCNT1, dllForCNT1, TransitWarehouseStatuses.Codes.Departed);

			var dllForCNT2 = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			var packageStatesInCNT2 = dllForCNT2.PackageStates;
			AssertEquals(2, packageStatesInCNT2.Count);
			AssertPackage(packageStatesInCNT2, "PKG3", null, dllForCNT2, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesInCNT2, "CTN2", null, dllForCNT2, TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestTWReceivesBlindConsignment_NewShipmentAddedAfterSendingDispatchInstructions_MultipleContainers()
		{
			var data = SetupDataForTWReceivesBlindPackages_MultipleContainers();
			var consol = data.consol;
			var shipment = data.shipment;
			var dtus = data.dtus;
			var loadLists = data.loadLists;

			// loading packages to DTU
			var loadListForCNT1 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var loadListForCNT2 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			loadListForCNT1.WDL_IsReadyToStage = true;
			loadListForCNT1.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			loadListForCNT2.WDL_IsReadyToStage = true;
			loadListForCNT2.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var dtuForCNT1 = dtus.Where(d => d.WDH_VehicleReference == "CNT1").Single();
			var dtuForCNT2 = dtus.Where(d => d.WDH_VehicleReference == "CNT2").Single();
			var pkg1 = loadListForCNT1.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var ctn1 = loadListForCNT2.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtuForCNT1, pkg1);
			Helper.LoadPackage(dtuForCNT2, ctn1);
			loadListForCNT1.Factory.Save();

			var container1 = consol.Containers.Cast<ForwardingContainer>().Single(c => c.JC_ContainerNum == "CNT1");
			var container2 = consol.Containers.Cast<ForwardingContainer>().Single(c => c.JC_ContainerNum == "CNT2");
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container1);
			newBAGPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "CNT1").Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "CNT2").Single();
			var loadListsAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			var dllForContainer1 = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var dllForContainer2 = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			AssertEquals(false, dllForContainer1.WDL_IsReadyToStage);
			AssertEquals(false, dllForContainer2.WDL_IsReadyToStage);
			AssertEquals(1, dtuForContainer1.PackageStates.Count);
			AssertEquals(1, dtuForContainer2.PackageStates.Count);
			var packageStatesForContainer1 = dllForContainer1.PackageStates;
			var packageStatesForContainer2 = dllForContainer2.PackageStates;
			AssertEquals(3, packageStatesForContainer1.Count);
			AssertEquals(2, packageStatesForContainer2.Count);
			AssertPackage(packageStatesForContainer1, "PKG1", dtuForContainer1, dllForContainer1, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForContainer1, "PKG2", null, dllForContainer1, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForContainer1, "PKG3", null, dllForContainer1, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForContainer2, "CTN1", dtuForContainer2, dllForContainer2, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForContainer2, "CTN2", null, dllForContainer2, TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestTWReceivesBlindConsignment_NewShipmentAddedAfterSendingDispatchInstructions_MultipleContainers_FirstContainerFinishedLoading()
		{
			/*
				Consol A
				Container 1
				Container 2
				Shipment A
					PKG1 PLT 1 – Container 1
					PKG2 PLT 1 – Container 2
					Carton1 CTN 1 – Container 2

			 */
			var data = SetupDataForTWReceivesBlindPackages_MultipleContainers();
			var consol = data.consol;
			var shipment = data.shipment;
			var dtus = data.dtus;
			var loadLists = data.loadLists;

			/* 
				Load List 1 – Consol A and Container 1 - Finished Loading so all packages are departed
				Load List 2 – Consol A and Container 2
						DTU Container 1
				DTU Container 2		
						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded & departed – Container 1
							PKG2 PLT 1 - Loaded & departed - Container 1
							Carton1 CTN 1 – Loaded – Container 2
			 */

			// loading packages to DTU
			var loadListForCNT1 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var loadListForCNT2 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			loadListForCNT1.WDL_IsReadyToStage = true;
			loadListForCNT1.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			loadListForCNT2.WDL_IsReadyToStage = true;
			loadListForCNT2.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var dtuForCNT1 = dtus.Where(d => d.WDH_VehicleReference == "CNT1").Single();
			var dtuForCNT2 = dtus.Where(d => d.WDH_VehicleReference == "CNT2").Single();
			var pkg1 = loadListForCNT1.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListForCNT1.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var ctn1 = loadListForCNT2.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtuForCNT1, pkg1);
			Helper.LoadPackage(dtuForCNT1, pkg2);
			Helper.LoadPackage(dtuForCNT2, ctn1);
			Helper.FinishLoading(dtuForCNT1);
			loadListForCNT1.Factory.Save();

			/*
					Consol A
					Container 1
					Container 2
					Shipment A
						PKG1 PLT 1 – Container 1
						PKG2 PLT 1 – Container 1
						Carton1 CTN 1 – Container 2

					Shipment B
						PKG4 PLT 1 – Container 2
						Carton2 CTN 1 – Container 2
			 */
			var container1 = consol.Containers.Cast<ForwardingContainer>().Single(c => c.JC_ContainerNum == "CNT1");
			var container2 = consol.Containers.Cast<ForwardingContainer>().Single(c => c.JC_ContainerNum == "CNT2");
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container2);
			newBAGPackLine.SetContainer(consol, container2);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "CNT1").Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "CNT2").Single();
			var loadListsAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			var dllForLoadCompleted = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var dllForOpenContainer = loadListsAfterSendingDispatchInstructionsAgain.Where(d => d.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			AssertEquals(false, dllForLoadCompleted.WDL_IsReadyToStage);
			AssertEquals(false, dllForOpenContainer.WDL_IsReadyToStage);
			AssertEquals(2, dtuForContainer1.PackageStates.Count);
			var packageStatesForContainer1 = dllForLoadCompleted.PackageStates;
			var packageStatesForContainer2 = dllForOpenContainer.PackageStates;
			AssertEquals(2, packageStatesForContainer1.Count);
			AssertEquals(3, packageStatesForContainer2.Count);
			AssertPackage(packageStatesForContainer1, "PKG1", dtuForContainer1, dllForLoadCompleted, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesForContainer1, "PKG2", dtuForContainer1, dllForLoadCompleted, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesForContainer2, "PKG3", null, dllForOpenContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForContainer2, "CTN1", dtuForContainer2, dllForOpenContainer, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForContainer2, "CTN2", null, dllForOpenContainer, TransitWarehouseStatuses.Codes.Arrived);
		}

		(WhsWarehouse warehouse, OrgHeader consignee, OrgHeader consignor, ZDateTime today, ForwardingConsol consol, ForwardingContainer container, ForwardingShipment shipment,
			WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll)
			SetupDataForTWReceivesBlindPackages()
		{
			/*
			Consol A
				Container 1
				Shipment A
					PKG1 PLT 1
					PKG2 PLT 1
					Carton1 CTN 1
			 */
			var testData = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, plt2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, container);
			packline2.SetContainer(consol, container);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(dispatchConsignment, shipment.PK);

			// assert load list, DTU and dcn on package states
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var plt2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, plt2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtu = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(3, loadList.PackageStates.Count);
			AssertEquals("CNT1", dtu.WDH_VehicleReference);
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, plt2InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, ctn1InNewFactory.WPS_WDL_LoadList);

			return (testData.warehouse, testData.consignee, testData.consignor, testData.today, consol, container, shipment, dtu, loadList);
		}

		(WhsWarehouse warehouse, OrgHeader consignee, OrgHeader consignor, ZDateTime today, ForwardingConsol consol, ForwardingShipment shipment,
	WhsItemDispatchTransportationUnit[] dtus, WhsItemDispatchLoadList[] loadLists)
			SetupDataForTWReceivesBlindPackages_MultipleContainers()
		{
			/*
				Consol A
						Container 1
						Container 2
						Shipment A
							PKG1 PLT 1 – Container 1
							PKG2 PLT 1 – Container 1
							Carton1 CTN 1 – Container 2

			 */
			var testData = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container1 = CreateContainer(consol, "CNT1");
			var container2 = CreateContainer(consol, "CNT2");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, plt2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, container1);
			packline2.SetContainer(consol, container2);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(dispatchConsignment, shipment.PK);

			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var plt2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, plt2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, dtus.Length);
			AssertEquals(2, loadLists.Length);

			var loadListForContainer1 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var loadListForContainer2 = loadLists.Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			AssertEquals(2, loadListForContainer1.PackageStates.Count);
			AssertEquals(1, loadListForContainer2.PackageStates.Count);
			AssertEquals(loadListForContainer1.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadListForContainer1.PK, plt2InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadListForContainer2.PK, ctn1InNewFactory.WPS_WDL_LoadList);

			return (testData.warehouse, testData.consignee, testData.consignor, testData.today, consol, shipment, dtus, loadLists);
		}

		#endregion

		#region TestGroupOfContainers

		public void TestGroupOfContainers()
		{
			/*
				Consol A
						2x 20GP
						2x 40GP	
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
							Carton1 CTN 1 2x 40GP
			 */
			var data = SetupDataForTWReceivesBlindPackages_GroupOfContainers();
			var consol = data.consol;
			var shipment = data.shipment;
			var dtus20GP = data.DTUs20GP;
			var dtus40GP = data.DTUs40GP;

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

				Load List 2 – Consol A – DTU 3 – 40GP
				Load List 2 – Consol A – DTU 4 – 40GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1 – Loaded – DTU3

			 */

			// loading packages to DTU
			var dtu1 = dtus20GP[0];
			var dtu2 = dtus20GP[1];
			var dtu3 = dtus40GP[0];
			dtu1.WDH_VehicleReference = "dtu1";
			dtu2.WDH_VehicleReference = "dtu2";
			dtu3.WDH_VehicleReference = "dtu3";
			var loadListFor20GP = dtu1.DispatchLoadLists.Single();
			var loadListFor40GP = dtu3.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListFor20GP, data.warehouse.DefaultLocation);
			MarkLoadListReadyToStage(loadListFor40GP, data.warehouse.DefaultLocation);

			var pkg1 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var ctn1 = loadListFor40GP.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.LoadPackage(dtu3, ctn1);
			loadListFor20GP.Factory.Save();

			/*
					Consol A
						2x 20GP
						2x 40GP	
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
							Carton1 CTN 1 2x 40GP

						Shipment B
							PKG4 PLT  2x 20GP
							Carton2 CTN 1 2x 40GP
			 */
			var container20GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "20GP");
			var container40GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "40GP");
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container20GP);
			newBAGPackLine.SetContainer(consol, container40GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals(4, dtusAfterSendingDispatchInstructionsAgain.Length);
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();
			var dtuForContainer3 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu3.WDH_ReferenceNumber).Single();
			var dtuForEmptyContainer = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "").Single();

			AssertEquals(2, newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			var loadListFor20GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor20GP.PK);
			var loadListFor40GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor40GP.PK);
			AssertEquals(false, loadListFor20GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(false, loadListFor40GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			var packageStatesFor20GP = loadListFor20GPAfterSendingDispatchInstructionsAgain.PackageStates;
			var packageStatesFor40GP = loadListFor40GPAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStatesFor20GP.Count);
			AssertEquals(2, packageStatesFor40GP.Count);
			AssertPackage(packageStatesFor20GP, "PKG1", dtuForContainer1, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG2", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG3", null, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor40GP, "CTN1", dtuForContainer3, loadListFor40GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor40GP, "CTN2", null, loadListFor40GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestGroupOfContainers_OneContainerOnLoadListIsLoadCompletedAndOtherOneIsAvailable()
		{
			/*
				Consol A
						2x 20GP
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
			 */
			var data = SetupDataForTWReceivesBlindPackages_GroupOfContainers();
			var consol = data.consol;
			var shipment = data.shipment;
			var dtus20GP = data.DTUs20GP;

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1

			 */

			// loading packages to DTU
			var dtu1 = dtus20GP[0];
			var dtu2 = dtus20GP[1];
			dtu1.WDH_VehicleReference = "DTU1";
			dtu2.WDH_VehicleReference = "DTU2";

			var loadListFor20GP = dtu1.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListFor20GP, data.warehouse.DefaultLocation);

			var pkg1 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.FinishLoading(dtu1); // dtu2 is still available
			loadListFor20GP.Factory.Save();

			/*
					Consol A
						2x 20GP
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP

						Shipment B
							PKG4 PLT  2x 20GP
			 */
			var container20GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "20GP");
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container20GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();

			//var loadListsAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListFor20GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor20GP.PK);
			AssertEquals(false, loadListFor20GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			AssertEquals(1, dtuForContainer1.PackageStates.Count);
			AssertEquals(1, dtuForContainer2.PackageStates.Count);
			var packageStatesFor20GP = loadListFor20GPAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStatesFor20GP.Count);
			AssertPackage(packageStatesFor20GP, "PKG1", dtuForContainer1, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesFor20GP, "PKG2", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG3", null, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestGroupOfContainers_BothContainersAllocatedToLoadListAreCompleted_RejectImport()
		{
			/*
				Consol A
						2x 20GP
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
			 */
			var data = SetupDataForTWReceivesBlindPackages_GroupOfContainers();
			var consol = data.consol;
			var shipment = data.shipment;
			var dtus20GP = data.DTUs20GP;

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1

			 */

			// loading packages to DTU
			var dtu1 = dtus20GP[0];
			var dtu2 = dtus20GP[1];
			dtu1.WDH_VehicleReference = "DTU1";
			dtu2.WDH_VehicleReference = "DTU2";
			var loadListFor20GP = dtu1.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListFor20GP, data.warehouse.DefaultLocation);

			var pkg1 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.FinishLoading(dtu1);
			Helper.FinishLoading(dtu2);
			loadListFor20GP.Factory.Save();

			/*
					Consol A
						2x 20GP
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP

						Shipment B
							PKG4 PLT  2x 20GP
			 */
			var container20GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "20GP");
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", data.today.AddDays(-1), data.today.AddDays(9), data.consignor, data.consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", data.warehouse.PK, data.warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", data.warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, container20GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(1, dispatchConsignments.Length); // second shipment is rejected since both containers are load completed.

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();

			var loadListsAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListFor20GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor20GP.PK);
			AssertEquals(false, loadListFor20GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			// need to assert DTUs
			AssertEquals(1, dtuForContainer1.PackageStates.Count);
			AssertEquals(1, dtuForContainer2.PackageStates.Count);
			var packageStatesFor20GP = loadListFor20GPAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(2, packageStatesFor20GP.Count);
			AssertPackage(packageStatesFor20GP, "PKG1", dtuForContainer1, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStatesFor20GP, "PKG2", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
		}

		public void TestGroupOfContainers_SameTypes_Resending()
		{
			/*
				Consol A
						2x 20GP
						3x 20GP	
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
							Carton1 CTN 1 3x 20GP
			 */
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var two20GP = CreateContainer(consol, "", 2, "20GP");
			var three20GP = CreateContainer(consol, "", 3, "20GP");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { plt1, plt2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var packline1 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, two20GP);
			packline2.SetContainer(consol, three20GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(dispatchConsignment, shipmentA.PK);

			// assert load list, DTU and dcn on package states
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var plt2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, plt2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(5, dtus.Length);
			AssertEquals(2, loadLists.Length);

			var loadListForTwo20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.Count == 2);
			var loadListForThree20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.Count == 3);
			var twoTwentyGPContainers = loadListForTwo20GPContainers.DispatchTransportationUnits;
			var three20GPContainers = loadListForThree20GPContainers.DispatchTransportationUnits;
			AssertEquals(2, twoTwentyGPContainers.Count);
			AssertEquals(3, three20GPContainers.Count);
			Assert(twoTwentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(three20GPContainers.All(c => c.ContainerNumber == ""));

			AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2" }, loadListForTwo20GPContainers.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("CTN1", loadListForThree20GPContainers.PackageStates.Single().Package.KP_PackageID);

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(1, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals(5, dtusAfterSendingDispatchInstructionsAgain.Length);

			AssertEquals(2, newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			var loadListFortwo20GPContainersAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForTwo20GPContainers.PK);
			var loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForThree20GPContainers.PK);
			AssertEquals(false, loadListFortwo20GPContainersAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(false, loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			// need to assert DTUs
			var packageStatesFor20GP = loadListFortwo20GPContainersAfterSendingDispatchInstructionsAgain.PackageStates;
			var packageStatesFor40GP = loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(2, packageStatesFor20GP.Count);
			AssertEquals(1, packageStatesFor40GP.Count);
		}

		public void TestGroupOfContainers_SameTypes_SomePackagesAreLoaded_AdExtraShipment()
		{
			/*
				Consol A
						2x 20GP
						3x 20GP	
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
							Carton1 CTN 1 3x 20GP
			 */
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var two20GP = CreateContainer(consol, "", 2, "20GP");
			var three20GP = CreateContainer(consol, "", 3, "20GP");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { plt1, plt2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var packline1 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, two20GP);
			packline2.SetContainer(consol, three20GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(dispatchConsignment, shipmentA.PK);

			// assert load list, DTU and dcn on package states
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var plt2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, plt2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(5, dtus.Length);
			AssertEquals(2, loadLists.Length);

			var loadListForTwo20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.Count == 2);
			var loadListForThree20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.Count == 3);
			var two20GPContainers = loadListForTwo20GPContainers.DispatchTransportationUnits.ToArray();
			var three20GPContainers = loadListForThree20GPContainers.DispatchTransportationUnits.ToArray();
			AssertEquals(2, two20GPContainers.Length);
			AssertEquals(3, three20GPContainers.Length);
			Assert(two20GPContainers.All(c => c.ContainerNumber == ""));
			Assert(three20GPContainers.All(c => c.ContainerNumber == ""));

			AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2" }, loadListForTwo20GPContainers.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("CTN1", loadListForThree20GPContainers.PackageStates.Single().Package.KP_PackageID);

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

				Load List 2 – Consol A – DTU 3 – 20GP
				Load List 2 – Consol A – DTU 4 – 20GP
				Load List 2 – Consol A – DTU 5 – 20GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1 – Loaded – DTU3

			 */

			// loading packages to DTU
			var dtu1 = two20GPContainers[0];
			var dtu2 = two20GPContainers[1];
			var dtu3 = three20GPContainers[0];
			dtu1.WDH_VehicleReference = "dtu1";
			dtu2.WDH_VehicleReference = "dtu2";
			dtu3.WDH_VehicleReference = "dtu3";
			var loadListForTwo20GP = dtu1.DispatchLoadLists.Single();
			var loadListForThree20GP = dtu3.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListForTwo20GP, warehouse.DefaultLocation);
			MarkLoadListReadyToStage(loadListForThree20GP, warehouse.DefaultLocation);

			var pkg1 = loadListForTwo20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListForTwo20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var ctn1InTW = loadListForThree20GP.PackageStates.Single(p => p.Package.KP_PackageID == "CTN1");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.LoadPackage(dtu3, ctn1InTW);
			loadListForTwo20GP.Factory.Save();

			/*
					Consol A
						2x 20GP
						3x 20GP	
						Shipment A
							PKG1 PLT 1 2x 20GP
							PKG2 PLT 1 2x 20GP
							Carton1 CTN 1 3x 20GP

						Shipment B
							PKG4 PLT  2x 20GP
							Carton2 CTN 1 3x 20GP
			 */
			var container20GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "20GP" && c.JC_ContainerCount == 2);
			var container40GP = consol.Containers.Cast<ForwardingContainer>().Single(c => c.Container.RC_Code == "20GP" && c.JC_ContainerCount == 3);
			// Add an extra shipment
			var newShipment = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK);
			var newPackageToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);
			var newCartonToAssign = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu2, entryNum: newShipment.JobNumber);

			// Attach packages to Shipment
			((ITransitWarehouseParent)newShipment).AttachPackages(new[] { newPackageToAssign, newCartonToAssign });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, newShipment.OuterPackLines.Count);
			var newPLTPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var newBAGPackLine = newShipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			newPLTPackLine.SetContainer(consol, two20GP);
			newBAGPackLine.SetContainer(consol, three20GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals(5, dtusAfterSendingDispatchInstructionsAgain.Length);
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();
			var dtuForContainer3 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu3.WDH_ReferenceNumber).Single();
			var dtuForContainer4 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == three20GPContainers[1].WDH_ReferenceNumber).Single();
			var dtuForContainer5 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == three20GPContainers[2].WDH_ReferenceNumber).Single();

			AssertEquals(2, newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			var loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForTwo20GPContainers.PK);
			var loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForThree20GPContainers.PK);
			AssertEquals(false, loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(false, loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			// need to assert DTUs
			var packageStatesForTwo20GP = loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain.PackageStates;
			var packageStatesForThree20GP = loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStatesForTwo20GP.Count);
			AssertEquals(2, packageStatesForThree20GP.Count);
			AssertPackage(packageStatesForTwo20GP, "PKG1", dtuForContainer1, loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForTwo20GP, "PKG2", dtuForContainer2, loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForTwo20GP, "PKG3", null, loadListForTwo20GPContainersAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForThree20GP, "CTN1", dtuForContainer3, loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesForThree20GP, "CTN2", null, loadListForThree20GPContainersAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
		}

		static void MarkLoadListReadyToStage(WhsItemDispatchLoadList loadList, WhsLocation stagingLocation)
		{
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = stagingLocation.PK;
		}

		(WhsWarehouse warehouse, OrgHeader consignee, OrgHeader consignor, ZDateTime today, ForwardingConsol consol, ForwardingContainer container, ForwardingShipment shipment,
	WhsItemDispatchTransportationUnit[] DTUs20GP, WhsItemDispatchTransportationUnit[] DTUs40GP)
			SetupDataForTWReceivesBlindPackages_GroupOfContainers()
		{
			/*
			Consol A
				2x 20GP
				2x 40GP	
				Shipment A
					PKG1 PLT 1 2x 20GP
					PKG2 PLT 1 2x 20GP
					Carton1 CTN 1 2x 40GP

			 */
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var two20GP = CreateContainer(consol, "", 2, "20GP");
			var two40GP = CreateContainer(consol, "", 2, "40GP");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { plt1, plt2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var packline1 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, two20GP);
			packline2.SetContainer(consol, two40GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertDispatchConsignment(dispatchConsignment, shipmentA.PK);

			// assert load list, DTU and dcn on package states
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var plt2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, plt2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(4, dtus.Length);
			AssertEquals(2, loadLists.Length);

			var loadListFor20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");
			var twentyGPContainers = loadListFor20GPContainers.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainers.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(2, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2" }, loadListFor20GPContainers.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("CTN1", loadListFor40GPContainers.PackageStates.Single().Package.KP_PackageID);

			return (warehouse, consignee, consignor, today, consol, two20GP, shipmentA, twentyGPContainers.ToArray(), fourtyGPContainers.ToArray());
		}

		#endregion

		#endregion

		#region Remove Shipments

		#region TestTWReceivesBlindConsignment

		public void TestTWReceivesBlindConsignment_Forwarder_Remove_ShipmentAfterSendingDispatchInstructions()
		{
			/*
				1.
			 		Consol A
						Container 1
						Shipment A
							PKG1 PLT 1 - Container 1 - Loaded
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 – Container 1
						Shipment B
							PKG3 PLT 1 - Container 1
							Carton2 CTN 1 - Container 1 - Loaded
				2. Remove shipment B and reimport
				3. PKG3 and Carton2 is removed from Load List and Carton2 is marked as IsRemoveFromDTU  is true.
			 */
			var data = SetupDataForTWReceivesBlindPackages_NewShipments();
			var consol = data.consol;
			var shipmentA = data.shipmentA;
			var shipmentB = data.shipmentB;
			var container = data.container;

			var loadList = data.dll;

			// loading packages to DTU
			var dtu = loadList.DispatchTransportationUnits.Single();
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var pkg1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg3 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG3");
			var ctn2 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "CTN2");
			Helper.LoadPackage(dtu, pkg1);
			Helper.LoadPackage(dtu, ctn2);
			loadList.Factory.Save();

			// Remove shipment B from consol.
			consol.Shipments.Remove(shipmentB);
			Factory.Save();

			// Reimport consol
			TriggerAndFireTransitRequestForRelease(data.consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtuAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadListAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(false, loadListAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(2, dtuAfterSendingDispatchInstructionsAgain.PackageStates.Count);
			var packageStatesOnLoadList = loadListAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStatesOnLoadList.Count);
			AssertPackage(packageStatesOnLoadList, "PKG1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: false);
			AssertPackage(packageStatesOnLoadList, "PKG2", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackage(packageStatesOnLoadList, "CTN1", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);

			// PKG3 and Carton2 is removed from Load List and Carton2 is marked as IsRemoveFromDTU is true.
			var pkg3InNewFactory = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(pkg3.PK);
			var ctn2InNewFactory = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(ctn2.PK);
			AssertPackageState(pkg3InNewFactory, null, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackageState(ctn2InNewFactory, dtuAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: true);
		}

		public void TestTWReceivesBlindConsignment_ShipmentRemovedAfterSendingDispatchInstructions_ContainerLoadCompleted()
		{
			/*
				1.
			 		Consol A
						Container 1 - Load completed
						Shipment A
							PKG1 PLT 1 - Container 1 - Loaded
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 – Container 1
						Shipment B
							PKG3 PLT 1 - Container 1
							Carton2 CTN 1 - Container 1 - Loaded
				2. Remove Shipment B and reimport
				3. Import is rejected.
			 */
			var data = SetupDataForTWReceivesBlindPackages_NewShipments();
			var consol = data.consol;
			var shipmentA = data.shipmentA;
			var shipmentB = data.shipmentA;
			var container = data.container;

			var loadList = data.dll;

			// loading packages to DTU
			var dtu = loadList.DispatchTransportationUnits.Single();
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = data.warehouse.DefaultLocation.PK;
			var pkg1 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var ctn2 = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "CTN2");
			Helper.LoadPackage(dtu, pkg1);
			Helper.LoadPackage(dtu, ctn2);
			Helper.FinishLoading(dtu);
			loadList.Factory.Save();

			// Remove shipment B from consol.
			consol.Shipments.Remove(shipmentB);
			Factory.Save();

			// Reimport consol
			TriggerAndFireTransitRequestForRelease(data.consol);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtuAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadListAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(true, loadListAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(2, dtuAfterSendingDispatchInstructionsAgain.PackageStates.Count);
			var packageStates = loadListAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(5, packageStates.Count);
			AssertPackage(packageStates, "PKG1", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed);
			AssertPackage(packageStates, "PKG2", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "PKG3", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "CTN1", null, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "CTN2", dtuAfterSendingDispatchInstructionsAgain, loadListAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed, isRemoveFromDTU: false);
		}

		public void TestTWReceivesBlindConsignment_ShipmentRemovedAfterSendingDispatchInstructions_AllRemovedPackagesForStillLoadingContainer_ExistingPackagesAreForALoadCompletedContainer()
		{
			/*
				a. send dispatch instructions.
					Consol A
						Container 1
						Container 2
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 - Container 1
						Shipment B
							PKG3 PLT 1 - Container 2
							Carton2 CTN 1 - Container 2
				b. Transit finished loading of container 1 and container 2 is still being loaded.
				c. Remove second shipment which has packages for container 2 which is not load completed yet.
					Consol A
						Container 1 - Load Completed <== since it is load completed should we reject the import?
						Container 2 (Still Loading) <== should be removed from the consol otherwise import is rejected since no packages for Container 2.
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 - Container 1
				d. Import is accepted. Load completed Container and packages are untouched. Mark Shipment B's loaded packages are RemoveFromDTU. Those packages are detached from load list.
				Container 2 still remains on load list
				Note: We will remove the load list for container 2 and will be implemented in another work item.
			 */
			var testData = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var loadCompleted = CreateContainer(consol, "CNT1");
			var stillLoading = CreateContainer(consol, "CNT2");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipmentB = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Factory.Save();

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var pkg1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);
			var ctn2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { pkg1, pkg2, ctn1 });
			Factory.Save();
			((ITransitWarehouseParent)shipmentB).AttachPackages(new[] { pkg3, ctn2 });
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var pkg1AndPkg2PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var ctn1PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			pkg1AndPkg2PackLine.SetContainer(consol, loadCompleted);
			ctn1PackLine.SetContainer(consol, loadCompleted);
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentB.OuterPackLines.Count);
			var pkg3PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var ctn2PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			pkg3PackLine.SetContainer(consol, stillLoading);
			ctn2PackLine.SetContainer(consol, stillLoading);
			Factory.Save();

			// Import consol
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);
			var dcnForShipmentA = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB1");
			var dcnForShipmentB = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB2");

			// Assert load list, DTU and dcn on package states
			var pkg1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			var pkg3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg3.PK);
			var ctn2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn2.PK);
			AssertEquals(dcnForShipmentA.PK, pkg1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, pkg2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, pkg3InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, ctn2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadListForLoadCompletedContainer = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var loadListForStillLoadingContainer = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			var packageStatesForLoadCompletedDLL = loadListForLoadCompletedContainer.PackageStates;
			var packageStatesForStillLoadingDLL = loadListForStillLoadingContainer.PackageStates;
			AssertEquals(3, packageStatesForLoadCompletedDLL.Count);
			AssertEquals(2, packageStatesForStillLoadingDLL.Count);
			AssertPackage(packageStatesForLoadCompletedDLL, "PKG1", null, loadListForLoadCompletedContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForLoadCompletedDLL, "PKG2", null, loadListForLoadCompletedContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForLoadCompletedDLL, "CTN1", null, loadListForLoadCompletedContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForStillLoadingDLL, "PKG3", null, loadListForStillLoadingContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForStillLoadingDLL, "CTN2", null, loadListForStillLoadingContainer, TransitWarehouseStatuses.Codes.Arrived);

			var dtuForContainerLoadCompleted = loadListForLoadCompletedContainer.DispatchTransportationUnits.Single(d => d.WDH_VehicleReference == "CNT1");
			loadListForLoadCompletedContainer.WDL_IsReadyToStage = true;
			loadListForLoadCompletedContainer.WDL_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtuForContainerLoadCompleted, pkg1InNewFactory);
			Helper.LoadPackage(dtuForContainerLoadCompleted, ctn1InNewFactory);
			Helper.FinishLoading(dtuForContainerLoadCompleted, ZDateTimeOffset.UtcNow.AddDays(-1), ZDateTimeOffset.UtcNow, ZDateTimeOffset.Empty);

			var dtuForContainerStillLoading = loadListForStillLoadingContainer.DispatchTransportationUnits.Single(d => d.WDH_VehicleReference == "CNT2");
			loadListForStillLoadingContainer.WDL_IsReadyToStage = true;
			loadListForStillLoadingContainer.WDL_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtuForContainerStillLoading, ctn2InNewFactory);

			loadListForLoadCompletedContainer.Factory.Save();

			// Remove shipment B from consol
			// All shipment B packages are still loading to Container 2
			// and none of the packages from Shipment A are assigned to Container 2
			// Container 1 is load completed.
			var consolInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var shipmentBInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingShipment>(shipmentB.PK);
			consolInNewFactory.Shipments.Remove(shipmentBInNewFactory);
			var container2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingContainer>(stillLoading.PK);
			consolInNewFactory.Containers.Remove(container2InNewFactory);
			container2InNewFactory.Delete();
			newBizOFactoryAfterRunningLogWalker.Save();

			// Reimport consol without Shipment B
			TriggerAndFireTransitRequestForRelease(consolInNewFactory);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignmentsAfterSendingNewDispatchInstructions = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Both DCNs still remain in the system.", 2, dispatchConsignmentsAfterSendingNewDispatchInstructions.Length);

			var loadCompletedDTU = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single(d => d.PK == dtuForContainerLoadCompleted.PK);
			var stillLoadingDTU = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single(d => d.PK == dtuForContainerStillLoading.PK);
			var dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForLoadCompletedContainer.PK);
			var dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForStillLoadingContainer.PK);
			AssertEquals(false, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage); // <== should we stop the load list. All load list's containers are load completed. So shall we reject the import?
			AssertEquals(false, dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage); // <== This load list is found through the consol number and its packages are detached.
			AssertEquals(2, loadCompletedDTU.PackageStates.Count);
			AssertEquals(1, stillLoadingDTU.PackageStates.Count);

			var packageStatesAfterSendingNewDispatchInstructions = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(new ZQuery());
			AssertPackage(packageStatesAfterSendingNewDispatchInstructions, "PKG1", loadCompletedDTU, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: false);
			AssertPackage(packageStatesAfterSendingNewDispatchInstructions, "PKG2", null, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackage(packageStatesAfterSendingNewDispatchInstructions, "CTN1", loadCompletedDTU, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: false);
			AssertPackage(packageStatesAfterSendingNewDispatchInstructions, "PKG3", null, null, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackage(packageStatesAfterSendingNewDispatchInstructions, "CTN2", stillLoadingDTU, null, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: true);
		}

		public void TestTWReceivesBlindConsignment_ShipmentRemovedAfterSendingDispatchInstructions_AllRemovedPackagesForLoadCompletedContainer_ExistingPackagesAreForStillLoadingContainer()
		{
			/*
				a. send dispatch instructions.
					Consol A
						Container 1
						Container 2
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 - Container 1
						Shipment B
							PKG3 PLT 1 - Container 2
							Carton2 CTN 1 - Container 2
				b. Transit finished loading of container 2 and container 1 is still being loaded.
				c. Remove second shipment which has packages for container 2 which is load completed.
					Consol A
						Container 1 - still loading
						Container 2 (Load completed) <== should be removed from the consol otherwise import is rejected since no packages for Container 2.
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
							Carton1 CTN 1 - Container 1
				d. Import is accepted. Shipment A and B packages should not be changed?
				Note: Import should be rejected will implement in another work item.
			 */
			var testData = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var stillLoading = CreateContainer(consol, "CNT1");
			var loadCompleted = CreateContainer(consol, "CNT2");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipmentB = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Factory.Save();

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var pkg1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);
			var ctn2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { pkg1, pkg2, ctn1 });
			Factory.Save();
			((ITransitWarehouseParent)shipmentB).AttachPackages(new[] { pkg3, ctn2 });
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var pkg1AndPkg2PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var ctn1PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			pkg1AndPkg2PackLine.SetContainer(consol, stillLoading);
			ctn1PackLine.SetContainer(consol, stillLoading);
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentB.OuterPackLines.Count);
			var pkg3PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var ctn2PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			pkg3PackLine.SetContainer(consol, loadCompleted);
			ctn2PackLine.SetContainer(consol, loadCompleted);
			Factory.Save();

			// Import consol
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);
			var dcnForShipmentA = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB1");
			var dcnForShipmentB = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB2");

			// Assert load list, DTU and dcn on package states
			var pkg1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			var pkg3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg3.PK);
			var ctn2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn2.PK);
			AssertEquals(dcnForShipmentA.PK, pkg1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, pkg2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, pkg3InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, ctn2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadListForStillLoading = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			var loadListForLoadCompletedContainer = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Where(l => l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			var packageStatesForStillLoadingDLL = loadListForStillLoading.PackageStates;
			var packageStatesForLoadCompletedDLL = loadListForLoadCompletedContainer.PackageStates;
			AssertEquals(3, packageStatesForStillLoadingDLL.Count);
			AssertEquals(2, packageStatesForLoadCompletedDLL.Count);
			AssertPackage(packageStatesForStillLoadingDLL, "PKG1", null, loadListForStillLoading, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForStillLoadingDLL, "PKG2", null, loadListForStillLoading, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForStillLoadingDLL, "CTN1", null, loadListForStillLoading, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForLoadCompletedDLL, "PKG3", null, loadListForLoadCompletedContainer, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesForLoadCompletedDLL, "CTN2", null, loadListForLoadCompletedContainer, TransitWarehouseStatuses.Codes.Arrived);

			var dtuForContainerStillLoading = loadListForStillLoading.DispatchTransportationUnits.Single(d => d.WDH_VehicleReference == "CNT1");
			var dtuForContainerLoadCompleted = loadListForLoadCompletedContainer.DispatchTransportationUnits.Single(d => d.WDH_VehicleReference == "CNT2");
			loadListForStillLoading.WDL_IsReadyToStage = true;
			loadListForStillLoading.WDL_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtuForContainerStillLoading, pkg1InNewFactory);
			Helper.LoadPackage(dtuForContainerStillLoading, ctn1InNewFactory);

			loadListForLoadCompletedContainer.WDL_IsReadyToStage = true;
			loadListForLoadCompletedContainer.WDL_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtuForContainerLoadCompleted, ctn2InNewFactory);
			Helper.FinishLoading(dtuForContainerLoadCompleted);

			loadListForStillLoading.Factory.Save();

			// Remove shipment B from consol all shipment B packages are for Cotnainer 2 which is load completed.
			// and none of the packages from Shipment A are assigned to Container 2
			// Container 1 is still loading.
			var consolInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingConsol>(consol.PK);
			var shipmentBInNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingShipment>(shipmentB.PK);
			consolInNewFactory.Shipments.Remove(shipmentBInNewFactory);
			var container2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<ForwardingContainer>(loadCompleted.PK);
			consolInNewFactory.Containers.Remove(container2InNewFactory);
			container2InNewFactory.Delete();
			newBizOFactoryAfterRunningLogWalker.Save();

			// Reimport consol without Shipment B
			TriggerAndFireTransitRequestForRelease(consolInNewFactory);
			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignmentsAfterSendingNewDispatchInstructions = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals("Both DCNs still remain in the system.", 2, dispatchConsignmentsAfterSendingNewDispatchInstructions.Length);

			var stillLoadingDTU = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single(d => d.PK == dtuForContainerStillLoading.PK);
			var loadCompletedDTU = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single(d => d.PK == dtuForContainerLoadCompleted.PK);
			var dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForStillLoading.PK);
			var dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListForLoadCompletedContainer.PK);
			AssertEquals(false, dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage); // <== should this remain as true?
			AssertEquals("Load list for the load complete container should be stopped because the staged package was detached.",
				false, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(2, stillLoadingDTU.PackageStates.Count);
			AssertEquals(1, loadCompletedDTU.PackageStates.Count);
			var packageStates = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(new ZQuery());
			AssertPackage(packageStates, "PKG1", stillLoadingDTU, dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: false);
			AssertPackage(packageStates, "PKG2", null, dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackage(packageStates, "PKG3", null, null, TransitWarehouseStatuses.Codes.Arrived, isRemoveFromDTU: false);
			AssertPackage(packageStates, "CTN1", stillLoadingDTU, dllForStillLoadingContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: false);
			AssertPackage(packageStates, "CTN2", loadCompletedDTU, dllForLoadCompletedContainerAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Departed, isRemoveFromDTU: false);
		}

		(WhsWarehouse warehouse, OrgHeader consignee, OrgHeader consignor, ZDateTime today, ForwardingConsol consol, ForwardingContainer container, ForwardingShipment shipmentA,
			ForwardingShipment shipmentB, WhsItemDispatchLoadList dll)
			SetupDataForTWReceivesBlindPackages_NewShipments()
		{
			/*
			Consol A
				Container 1
				Shipment A
					PKG1 PLT 1
					PKG2 PLT 1
					Carton1 CTN
				Shipment B
					PKG3 PLT 1
					Carton2 CTN 1
			 */
			var testData = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var shipmentB = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			Factory.Save();

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", testData.warehouse.PK);
			var pkg1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);
			var ctn2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Bag, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentB.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { pkg1, pkg2, ctn1 });
			Factory.Save();
			((ITransitWarehouseParent)shipmentB).AttachPackages(new[] { pkg3, ctn2 });
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var pkg1AndPkg2PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var ctn1PackLine = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			pkg1AndPkg2PackLine.SetContainer(consol, container);
			ctn1PackLine.SetContainer(consol, container);
			Factory.Save();

			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentB.OuterPackLines.Count);
			var pkg3PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var ctn2PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Bag && p.JL_PackageCount == 1);
			pkg3PackLine.SetContainer(consol, container);
			ctn2PackLine.SetContainer(consol, container);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);
			var dcnForShipmentA = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB1");
			var dcnForShipmentB = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB2");

			// assert load list, DTU and dcn on package states
			var pkg1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			var pkg3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg3.PK);
			var ctn2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn2.PK);
			AssertEquals(dcnForShipmentA.PK, pkg1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, pkg2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, pkg3InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, ctn2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var packageStates = loadList.PackageStates;
			AssertEquals(5, packageStates.Count);
			AssertPackage(packageStates, "PKG1", null, loadList, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "PKG2", null, loadList, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "PKG3", null, loadList, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "CTN1", null, loadList, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStates, "CTN2", null, loadList, TransitWarehouseStatuses.Codes.Arrived);

			return (testData.warehouse, testData.consignee, testData.consignor, testData.today, consol, container, shipmentA, shipmentB, loadList);
		}

		#endregion

		#region TestGroupOfContainers_RemoveShipments

		public void TestGroupOfContainers_RemoveShipments()
		{
			/*
			Consol A
				2x 20GP
				2x 40GP	
				Shipment A
					PKG1 PLT 1 2x 20GP
					PKG2 PLT 1 2x 20GP
					Carton1 CTN 1 2x 40GP
			
				Shipment B
					PKG4 PLT  2x 20GP
					Carton2 CTN 1 2x 40GP
			 */
			var data = SetupDataForTWReceivesBlindPackages_GroupOfContainers_ToRemoveShipments();
			var consol = data.consol;
			var shipmentB = data.shipmentB;
			var dtus20GP = data.DTUs20GP;
			var dtus40GP = data.DTUs40GP;

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

				Load List 2 – Consol A – DTU 3 – 40GP
				Load List 2 – Consol A – DTU 4 – 40GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1
						Dispatch consignment – Shipment B
							PKG4 PLT  2x 20GP – Loaded – DTU2
							Carton2 CTN 1 2x 40GP – Loaded – DTU3
			 */

			// loading packages to DTU
			var dtu1 = dtus20GP[0];
			var dtu2 = dtus20GP[1];
			var dtu3 = dtus40GP[0];
			dtu1.WDH_VehicleReference = "dtu1";
			dtu2.WDH_VehicleReference = "dtu2";
			dtu3.WDH_VehicleReference = "dtu3";
			var loadListFor20GP = dtu1.DispatchLoadLists.Single();
			var loadListFor40GP = dtu3.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListFor20GP, data.warehouse.DefaultLocation);
			MarkLoadListReadyToStage(loadListFor40GP, data.warehouse.DefaultLocation);

			var pkg1 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var pkg3 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG3");
			var ctn2 = loadListFor40GP.PackageStates.Single(p => p.Package.KP_PackageID == "CTN2");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.LoadPackage(dtu2, pkg3);
			Helper.LoadPackage(dtu3, ctn2);
			loadListFor20GP.Factory.Save();

			// Dispatch consignment
			consol.Shipments.Remove(shipmentB);
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals(4, dtusAfterSendingDispatchInstructionsAgain.Length);
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();
			var dtuForContainer3 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu3.WDH_ReferenceNumber).Single();
			var dtuForEmptyContainer = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "").Single();

			AssertEquals(2, newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			var loadListFor20GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor20GP.PK);
			var loadListFor40GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor40GP.PK);
			AssertEquals(false, loadListFor20GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(false, loadListFor40GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			var packageStatesFor20GP = loadListFor20GPAfterSendingDispatchInstructionsAgain.PackageStates;
			var packageStatesFor40GP = loadListFor40GPAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(2, packageStatesFor20GP.Count);
			AssertEquals(1, packageStatesFor40GP.Count);
			AssertPackage(packageStatesFor20GP, "PKG1", dtuForContainer1, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG2", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			var pkg3InNewFactory = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(pkg3.PK);
			AssertPackageState(pkg3InNewFactory, dtuForContainer2, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: true);
			AssertPackage(packageStatesFor40GP, "CTN1", null, loadListFor40GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			var cnt2InNewFactory = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemPackageState>(ctn2.PK);
			AssertPackageState(cnt2InNewFactory, dtuForContainer3, TransitWarehouseStatuses.Codes.FreightLoaded, isRemoveFromDTU: true);
		}

		static void AssertPackageState(WhsItemPackageState packageState, WhsItemDispatchTransportationUnit dtu, string packageStatus, bool isRemoveFromDTU)
		{
			AssertEquals(ZGuid.Empty, packageState.WPS_WDL_LoadList);
			AssertEquals(packageStatus, packageState.WPS_Status);
			AssertEquals(isRemoveFromDTU, packageState.WPS_RemoveFromDTU);
			AssertEquals(dtu?.PK ?? ZGuid.Empty, packageState.WPS_WDH_TransitDispatchHeader);
		}

		(WhsWarehouse warehouse, OrgHeader consignee, OrgHeader consignor, ZDateTime today, ForwardingConsol consol, ForwardingContainer container, ForwardingShipment shipmentA, ForwardingShipment shipmentB,
WhsItemDispatchTransportationUnit[] DTUs20GP, WhsItemDispatchTransportationUnit[] DTUs40GP)
			SetupDataForTWReceivesBlindPackages_GroupOfContainers_ToRemoveShipments()
		{
			/*
			Consol A
				2x 20GP
				2x 40GP	
				Shipment A
					PKG1 PLT 1 2x 20GP
					PKG2 PLT 1 2x 20GP
					Carton1 CTN 1 2x 40GP
			
				Shipment B
					PKG4 PLT  2x 20GP
					Carton2 CTN 1 2x 40GP
			 */
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var two20GP = CreateContainer(consol, "", 2, "20GP");
			var two40GP = CreateContainer(consol, "", 2, "40GP");
			var shipmentA = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var shipmentB = CreateShipment(consol, "HSB2", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets and 1 CTN blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var pkg1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var pkg3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);
			var ctn2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipmentA.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipmentA).AttachPackages(new[] { pkg1, pkg2, ctn1 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentA.OuterPackLines.Count);
			var packline1 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 2);
			var packline2 = shipmentA.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			packline1.SetContainer(consol, two20GP);
			packline2.SetContainer(consol, two40GP);
			Factory.Save();

			((ITransitWarehouseParent)shipmentB).AttachPackages(new[] { pkg3, ctn2 });
			Factory.Save();
			AssertEquals("Precondition: There must be two outer packlines for the shipment.", 2, shipmentB.OuterPackLines.Count);
			var pkg3PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet && p.JL_PackageCount == 1);
			var ctn2PackLine = shipmentB.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton && p.JL_PackageCount == 1);
			pkg3PackLine.SetContainer(consol, two20GP);
			ctn2PackLine.SetContainer(consol, two40GP);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignments = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);
			var dcnForShipmentA = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB1");
			var dcnForShipmentB = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "HSB2");

			// assert load list, DTU and dcn on package states
			var pkg1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn1.PK);
			var pkg3InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg3.PK);
			var ctn2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(ctn2.PK);
			AssertEquals(dcnForShipmentA.PK, pkg1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, pkg2InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentA.PK, ctn1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, pkg3InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcnForShipmentB.PK, ctn2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtus = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			var loadLists = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(4, dtus.Length);
			AssertEquals(2, loadLists.Length);

			var loadListFor20GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "20GP");
			var loadListFor40GPContainers = loadLists.Single(l => l.DispatchTransportationUnits.First().Container.ContainerType.RC_Code == "40GP");
			var twentyGPContainers = loadListFor20GPContainers.DispatchTransportationUnits;
			var fourtyGPContainers = loadListFor40GPContainers.DispatchTransportationUnits;
			AssertEquals(2, twentyGPContainers.Count);
			AssertEquals(2, fourtyGPContainers.Count);
			Assert(twentyGPContainers.All(c => c.ContainerNumber == ""));
			Assert(fourtyGPContainers.All(c => c.ContainerNumber == ""));

			var packageStatesFor20GP = loadListFor20GPContainers.PackageStates;
			var packageStatesFor40GP = loadListFor40GPContainers.PackageStates;
			AssertEquals(3, packageStatesFor20GP.Count);
			AssertEquals(2, packageStatesFor40GP.Count);

			AssertPackage(packageStatesFor20GP, "PKG1", null, loadListFor20GPContainers, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor20GP, "PKG2", null, loadListFor20GPContainers, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor20GP, "PKG3", null, loadListFor20GPContainers, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor40GP, "CTN1", null, loadListFor40GPContainers, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor40GP, "CTN2", null, loadListFor40GPContainers, TransitWarehouseStatuses.Codes.Arrived);

			return (warehouse, consignee, consignor, today, consol, two20GP, shipmentA, shipmentB, twentyGPContainers.ToArray(), fourtyGPContainers.ToArray());
		}

		public void TestGroupOfContainers_RemoveShipments_LoadCompletedContainerPackages()
		{
			/*
			Consol A
				2x 20GP
				2x 40GP
				Shipment A
					PKG1 PLT 1 2x 20GP
					PKG2 PLT 1 2x 20GP
					Carton1 CTN 1 2x 40GP
			
				Shipment B
					PKG3 PLT  2x 20GP
					Carton2 CTN 1 2x 40GP
			 */
			var data = SetupDataForTWReceivesBlindPackages_GroupOfContainers_ToRemoveShipments();
			var consol = data.consol;
			var shipmentB = data.shipmentB;
			var dtus20GP = data.DTUs20GP;
			var dtus40GP = data.DTUs40GP;

			/* 
				Load List 1 – Consol A – DTU 1 – 20GP 
				Load List 1 – Consol A – DTU 2 – 20GP

				Load List 2 – Consol A – DTU 3 – 40GP
				Load List 2 – Consol A – DTU 4 – 40GP

						Dispatch consignment – Shipment A
							PKG1 PLT 1 – Loaded – DTU1
							PKG2 PLT 1 – Loaded – DTU2
							Carton1 CTN 1
						Dispatch consignment – Shipment B
							PKG4 PLT  2x 20GP – Loaded – DTU2
							Carton2 CTN 1 2x 40GP – Loaded – DTU3 - Load completed
			 */

			// loading packages to DTU
			var dtu1 = dtus20GP[0];
			var dtu2 = dtus20GP[1];
			var dtu3 = dtus40GP[0];
			dtu1.WDH_VehicleReference = "dtu1";
			dtu2.WDH_VehicleReference = "dtu2";
			dtu3.WDH_VehicleReference = "dtu3";
			var loadListFor20GP = dtu1.DispatchLoadLists.Single();
			var loadListFor40GP = dtu3.DispatchLoadLists.Single();
			MarkLoadListReadyToStage(loadListFor20GP, data.warehouse.DefaultLocation);
			MarkLoadListReadyToStage(loadListFor40GP, data.warehouse.DefaultLocation);

			var pkg1 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			var pkg3 = loadListFor20GP.PackageStates.Single(p => p.Package.KP_PackageID == "PKG3");
			var ctn2 = loadListFor40GP.PackageStates.Single(p => p.Package.KP_PackageID == "CTN2");
			Helper.LoadPackage(dtu1, pkg1);
			Helper.LoadPackage(dtu2, pkg2);
			Helper.LoadPackage(dtu2, pkg3);
			Helper.LoadPackage(dtu3, ctn2);
			Helper.FinishLoading(dtu3);
			Helper.FinaliseDTU(dtu3);
			loadListFor20GP.Factory.Save();

			// Dispatch consignment
			consol.Shipments.Remove(shipmentB);
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterSendingNewDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignments = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchConsignment>(new ZQuery());
			AssertEquals(2, dispatchConsignments.Length);

			var dtusAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals(4, dtusAfterSendingDispatchInstructionsAgain.Length);
			var dtuForContainer1 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu1.WDH_ReferenceNumber).Single();
			var dtuForContainer2 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu2.WDH_ReferenceNumber).Single();
			var dtuForContainer3 = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_ReferenceNumber == dtu3.WDH_ReferenceNumber).Single();
			var dtuForEmptyContainer = dtusAfterSendingDispatchInstructionsAgain.Where(d => d.WDH_VehicleReference == "").Single();

			AssertEquals(2, newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			var loadListFor20GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor20GP.PK);
			var loadListFor40GPAfterSendingDispatchInstructionsAgain = newBizOFactoryAfterSendingNewDispatchInstructions.Load<WhsItemDispatchLoadList>(loadListFor40GP.PK);
			AssertEquals(true, loadListFor20GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);
			AssertEquals(true, loadListFor40GPAfterSendingDispatchInstructionsAgain.WDL_IsReadyToStage);

			var packageStatesFor20GP = loadListFor20GPAfterSendingDispatchInstructionsAgain.PackageStates;
			var packageStatesFor40GP = loadListFor40GPAfterSendingDispatchInstructionsAgain.PackageStates;
			AssertEquals(3, packageStatesFor20GP.Count);
			AssertEquals(2, packageStatesFor40GP.Count);
			AssertPackage(packageStatesFor20GP, "PKG1", dtuForContainer1, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG2", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor20GP, "PKG3", dtuForContainer2, loadListFor20GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.FreightLoaded);
			AssertPackage(packageStatesFor40GP, "CTN1", null, loadListFor40GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Arrived);
			AssertPackage(packageStatesFor40GP, "CTN2", dtuForContainer3, loadListFor40GPAfterSendingDispatchInstructionsAgain, TransitWarehouseStatuses.Codes.Finalized, isRemoveFromDTU: false);
		}

		#endregion

		#endregion

		#region RemovingPackages

		#region TestRemovingPackages_PackageRemovedFromDCN_MatchingRCNExists

		public void TestRemovingPackages_PackageRemovedFromDCN_MatchingRCNExists_LoadCompleted()
		{
			/*
				1. Send receive and dispatch instructions for Consol A
					Consol A
						Container 1 - Load Completed
						Shipment A
							PKG1 PLT 1 - Container 1 - Load Completed
							PKG2 PLT 1 - Container 1
				2. Remove PKG1 and reimport
				3. Import is rejected.
			*/
			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_MatchingRCNExists();
			Helper.LoadPackage(dtu, pkg1);
			Helper.FinishLoading(dtu);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(2, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_MatchingRCNExists_LoadFinalised()
		{
			/*x
				1. Send receive and dispatch instructions for Consol A
					Consol A
						Container 1 - Load Finalised
						Shipment A
							PKG1 PLT 1 - Container 1 - Finalised
							PKG2 PLT 1 - Container 1
				2. Remove PKG1 and reimport
				3. Import is rejected.
			*/

			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_MatchingRCNExists();
			Helper.LoadPackage(dtu, pkg1);
			Helper.FinishLoading(dtu);
			Helper.FinaliseDTU(dtu);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(2, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_MatchingRCNExists_Loaded()
		{
			/*
				1. Send receive and dispatch instructions for Consol A
					Consol A
						Container 1 - Not load completed
						Shipment A
							PKG1 PLT 1 - Container 1 - Loaded
							PKG2 PLT 1 - Container 1
				2. Remove PKG1 and reimport
				3. Import is accepted. PKG1 is detached from Container 1 and marked as remove from DTU
			*/

			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_MatchingRCNExists();
			Helper.LoadPackage(dtu, pkg1);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(1, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = factoryAfterDispatch.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDL_LoadList);
			AssertEquals(ZBool.True, pkg1AfterSendingDispatchInstructions.WPS_RemoveFromDTU);
			AssertEquals(dtu.PK, pkg1AfterSendingDispatchInstructions.WPS_WDH_TransitDispatchHeader);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_MatchingRCNExists_NotLoaded()
		{
			/*
				1. Send receive and dispatch instructions for Consol A
					Consol A
						Container 1
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
				2. Remove PKG1 and reimport
				3. Import is accepted. PKG1 is detached from DLL
			*/

			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_MatchingRCNExists();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals("Packages are read from RCN so both packages are kept although it was removed from Shipment",
				1, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = factoryAfterDispatch.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDL_LoadList);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		#endregion

		#region TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages

		#region TestRemovingPackages_PackageRemovedFromDCN_LoadCompleted

		public void TestRemovingPackages_PackageRemovedFromDCN_Departed()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages(groupContainers: false, isDeparted: true);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_GroupContainers_Departed()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages(groupContainers: true, isDeparted: true);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_GroupContainers_LoadCompleted()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages(groupContainers: true, isDeparted: false);
		}

		void AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages(bool groupContainers = false, bool isDeparted = false)
		{
			/*
				1. TW receive PKG1 and PKG2 and then attach it to Shipment A. Assign those to Container with a number.
					Consol A
						Container 1 - Load Completed
						Shipment A (attach below arrived packages)
							PKG1 PLT 1 - Container 1 - Load Completed
							PKG2 PLT 1 - Container 1
				2. Send dispatch instructions
				3. Remove PKG1 and reimport
				4. Import is rejected.
			*/
			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_AttachingPackages(groupContainers);
			dtu.WDH_VehicleReference = "CNT1";
			Helper.LoadPackage(dtu, pkg1);
			var now = ZDateTimeOffset.UtcNow;
			Helper.FinishLoading(dtu, now.AddDays(-1), now, isDeparted ? now : ZDateTimeOffset.Empty);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(2, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(isDeparted ? TransitWarehouseStatuses.Codes.Departed : TransitWarehouseStatuses.Codes.FreightLoaded, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		#endregion

		#region TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_LoadFinalised

		public void TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_LoadFinalised()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_LoadFinalised(groupContainers: false);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_GroupContainers_LoadFinalised()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_LoadFinalised(groupContainers: true);
		}

		void AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_LoadFinalised(bool groupContainers = false)
		{
			/*
				1. TW receive PKG1 and PKG2 and then attach it to Shipment A. Assign those to Container with a number.
					Consol A
						Container 1 - Load Finalised
						Shipment A (attach below arrived packages)
							PKG1 PLT 1 - Container 1 - Finalised
							PKG2 PLT 1 - Container 1
				2. Send dispatch instructions
				3. Remove PKG1 and reimport
				4. Import is rejected.
			*/
			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_AttachingPackages(groupContainers);
			dtu.WDH_VehicleReference = "CNT1";
			Helper.LoadPackage(dtu, pkg1);
			Helper.FinishLoading(dtu);
			Helper.FinaliseDTU(dtu);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(2, dcnPackageStates.Count);
			var pkg1AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg1.PK);
			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		#endregion

		#region TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_Loaded

		public void TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_Loaded()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_Loaded(groupContainers: false);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_GroupOfContainers_AttachingPackages_Loaded()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_Loaded(groupContainers: true);
		}

		void AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_Loaded(bool groupContainers = false)
		{
			/*
				1. TW receive PKG1 and PKG2 and then attach it to Shipment A. Assign those to Container with a number.
					Consol A
						Container 1 - Not load completed
						Shipment A (attach below arrived packages)
							PKG1 PLT 1 - Container 1 - Loaded
							PKG2 PLT 1 - Container 1
				2. Send dispatch instructions
				3. Remove PKG1 and reimport
				4. Import is accepted. PKG1 is detached from Container 1 and marked as remove from DTU
			*/

			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_AttachingPackages(groupContainers);
			Helper.LoadPackage(dtu, pkg1);
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(1, dcnPackageStates.Count);

			var pkg1AfterSendingDispatchInstructions = factoryAfterDispatch.Load<WhsItemPackageState>(pkg1.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDL_LoadList);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZBool.True, pkg1AfterSendingDispatchInstructions.WPS_RemoveFromDTU);
			AssertEquals(dtu.PK, pkg1AfterSendingDispatchInstructions.WPS_WDH_TransitDispatchHeader);

			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		#endregion

		#region TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_NotLoaded

		public void TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_NotLoaded()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_NotLoaded(groupContainers: false);
		}

		public void TestRemovingPackages_PackageRemovedFromDCN_AttachingPackages_GroupOfContainers_NotLoaded()
		{
			AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_NotLoaded(groupContainers: true);
		}

		void AssertRemovingPackages_PackageRemovedFromDCN_AttachingPackages_NotLoaded(bool groupContainers = false)
		{
			/*
				1. TW receive PKG1 and PKG2 and then attach it to Shipment A. Assign those to Container with a number.
					Consol A
						Container 1
						Shipment A
							PKG1 PLT 1 - Container 1
							PKG2 PLT 1 - Container 1
				2. Send dispatch instructions
				3. Remove PKG1 and reimport
				4. Import is accepted. PKG1 is detached from DLL
			*/

			var (consol, pkg1, pkg2, warehouse, dtu, pkg1PackLine) = SetupDataForRemovingPackages_AttachingPackages(groupContainers);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pkg1PackLineNewFactory = newFactory.Load<ForwardingPackLine>(pkg1PackLine.PK);
			pkg1PackLineNewFactory.Delete();
			newFactory.Save();
			TriggerAndFireTransitRequestForRelease(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(1, dcnPackageStates.Count);

			var pkg1AfterSendingDispatchInstructions = factoryAfterDispatch.Load<WhsItemPackageState>(pkg1.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg1AfterSendingDispatchInstructions.WPS_Status);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDL_LoadList);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZBool.False, pkg1AfterSendingDispatchInstructions.WPS_RemoveFromDTU);
			AssertEquals(ZGuid.Empty, pkg1AfterSendingDispatchInstructions.WPS_WDH_TransitDispatchHeader);

			var pkg2AfterSendingDispatchInstructions = dcnPackageStates.Single(p => p.PK == pkg2.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, pkg2AfterSendingDispatchInstructions.WPS_Status);
		}

		#endregion

		#endregion

		#region TestDelinkLoadedPackageFromDCN

		public void TestDelinkLoadedPackageFromDCN_ViaConsol_FLOPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaConsol(false);
		}

		public void TestDelinkLoadedPackageFromDCN_ViaConsol_DEPPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaConsol(true);
		}

		void TestDelinkLoadedPackageFromDCN_ViaConsol(bool packageDeparted)
		{
			var testData = CreateTestData(false);
			var consol = CreateConsol("MSB1", testData.vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG-1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, reference: "PKG-2");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			consol.JK_OA_PackDepotAddress = testData.cfs.MainAddress.PK;
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var warehouse = testData.warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var packageForDelink = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Pallet)).First();
			var packageStateForDelink = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForDelink.PK)).First();
			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).First();
			var dll = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).First();
			dll.WDL_WL_StagingLocation = stageLocation.PK;
			dll.WDL_IsReadyToStage = true;

			LoadPackage(packageStateForDelink, stageLocation, rtu, dtu, dll, dcn);
			if (packageDeparted)
			{
				packageStateForDelink.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}

			packline.Delete();
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			packageStateForDelink = newFactory.Load<WhsItemPackageState>(packageStateForDelink.PK);
			dll = newFactory.Load<WhsItemDispatchLoadList>(dll.PK);
			if (packageDeparted)
			{
				AssertEquals(dcn.PK, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(dll.PK, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);

				AssertEquals(true, dll.WDL_IsReadyToStage);
			}
			else
			{
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);
				AssertEquals(true, packageStateForDelink.WPS_RemoveFromDTU);

				AssertEquals(false, dll.WDL_IsReadyToStage);
			}
		}

		public void TestDelinkLoadedPackageFromDCN_ViaShipment_FLOPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaShipment(false);
		}

		public void TestDelinkLoadedPackageFromDCN_ViaShipment_DEPPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaShipment(true);
		}

		void TestDelinkLoadedPackageFromDCN_ViaShipment(bool packageDeparted)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: false);

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG-2");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var packageForDelink = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Package)).First();
			var packageStateForDelink = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForDelink.PK)).First();
			var packageForChangeDLL = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Pallet)).First();
			var packageStateForChangeDLL = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForChangeDLL.PK)).First();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).First();
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, stageLocation, isReadyToStage: true);
			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).First();

			LoadPackage(packageStateForDelink, stageLocation, rtu, dtu, dll, dcn);
			LoadPackage(packageStateForChangeDLL, stageLocation, rtu, dtu, dll, dcn);
			if (packageDeparted)
			{
				packageStateForDelink.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}
			packline.Delete();
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			packageStateForDelink = newFactory.Load<WhsItemPackageState>(packageStateForDelink.PK);
			dll = newFactory.Load<WhsItemDispatchLoadList>(dll.PK);
			AssertEquals(rcn.PK, packageStateForDelink.WPS_WRC_TransitReceiveConsignment);
			if (packageDeparted)
			{
				AssertEquals(dcn.PK, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(dll.PK, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);

				AssertEquals(true, dll.WDL_IsReadyToStage);
			}
			else
			{
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);
				AssertEquals(true, packageStateForDelink.WPS_RemoveFromDTU);

				AssertEquals(false, dll.WDL_IsReadyToStage);
			}
		}

		public void TestDelinkLoadedPackageFromDCN_ViaShipment_DetachNonFLOPackage()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: false);

			var shipment = CreateShipment("HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG-2");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var floPackage = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Pallet)).First();
			var floPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, floPackage.PK)).First();
			var packageForDelink = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Package)).First();
			var packageStateForDelink = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForDelink.PK)).First();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).First();
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, stageLocation, isReadyToStage: true);
			LoadPackage(floPackageState, stageLocation, rtu, dtu, dll, dcn);
			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).First();

			packline.Delete();
			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			packageStateForDelink = newFactory.Load<WhsItemPackageState>(packageStateForDelink.PK);
			AssertEquals(rcn.PK, packageStateForDelink.WPS_WRC_TransitReceiveConsignment);
			AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDL_LoadList);
			AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDH_TransitDispatchHeader);

			dll = newFactory.Load<WhsItemDispatchLoadList>(dll.PK);
			AssertEquals(false, dll.WDL_IsReadyToStage);
		}

		public void TestDelinkLoadedPackageFromDCN_ViaShipment_ShipmentAttachedToConsol_FLOPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaShipment_ShipmentAttachedToConsol(false);
		}

		public void TestDelinkLoadedPackageFromDCN_ViaShipment_ShipmentAttachedToConsol_DEPPackage()
		{
			TestDelinkLoadedPackageFromDCN_ViaShipment_ShipmentAttachedToConsol(true);
		}
		void TestDelinkLoadedPackageFromDCN_ViaShipment_ShipmentAttachedToConsol(bool packageDeparted)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, parentProcessIsConsol: false);

			var consol = CreateConsol("MSB1", vessel, "NZAKL", "AUSYD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "S00001000", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ImportReleaseDepot = cfs.MainAddress.PK;

			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Package, reference: "PKG-1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, reference: "PKG-2");

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var packageForDelink = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Package)).First();
			var packageStateForDelink = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForDelink.PK)).First();
			var packageForChangeDLL = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Pallet)).First();
			var packageStateForChangeDLL = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packageForChangeDLL.PK)).First();
			var dcn = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).First();
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, stageLocation, isReadyToStage: true);

			LoadPackage(packageStateForDelink, stageLocation, rtu, dtu, dll, dcn);
			LoadPackage(packageStateForChangeDLL, stageLocation, rtu, dtu, dll, dcn);
			if (packageDeparted)
			{
				packageStateForDelink.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}
			packline.Delete();
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(shipment);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			packageStateForDelink = newFactory.Load<WhsItemPackageState>(packageStateForDelink.PK);
			dll = newFactory.Load<WhsItemDispatchLoadList>(dll.PK);
			if (packageDeparted)
			{
				AssertEquals(dcn.PK, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(dll.PK, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);

				AssertEquals(true, dll.WDL_IsReadyToStage);
			}
			else
			{
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDC_TransitDispatchConsignment);
				AssertEquals(ZGuid.Empty, packageStateForDelink.WPS_WDL_LoadList);
				AssertEquals(dtu.PK, packageStateForDelink.WPS_WDH_TransitDispatchHeader);
				AssertEquals(true, packageStateForDelink.WPS_RemoveFromDTU);

				AssertEquals(false, dll.WDL_IsReadyToStage);
			}
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

		(ForwardingConsol consol, WhsItemPackageState pkg1, WhsItemPackageState pkg2, WhsWarehouse warehouse, WhsItemDispatchTransportationUnit dtu, ForwardingPackLine pkg1PackLine) SetupDataForRemovingPackages_MatchingRCNExists()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			var pkg1PackLine = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG2");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var pkg1 = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
			var pkg2 = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
			AssertEquals(1, pkg1.Package.KP_PackageQty);
			AssertEquals(1, pkg2.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(pkg1, rtu1, "PKG1");

			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("HSB1", dispatchConsignment.WDC_ConsignmentID);

			// assert load list, DTU and dcn on package states
			var pkg1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg1.PK);
			var pkg2InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(pkg2.PK);
			AssertEquals(dispatchConsignment.PK, pkg1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, pkg2InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtu = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, pkg1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, pkg2InNewFactory.WPS_WDL_LoadList);

			return (consol, pkg1InNewFactory, pkg2InNewFactory, warehouse, dtu, pkg1PackLine);
		}

		(ForwardingConsol consol, WhsItemPackageState pkg1, WhsItemPackageState pkg2, WhsWarehouse warehouse, WhsItemDispatchTransportationUnit dtu, ForwardingPackLine pkg1PackLine)
			SetupDataForRemovingPackages_AttachingPackages(bool groupContainers = false)
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = groupContainers ? CreateContainer(consol, "", 2) : CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var drm1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Drum, "DRM1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, drm1 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 2, shipment.OuterPackLines.Count);
			var newPLTPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var newDRMPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Drum);
			newPLTPackLine.SetContainer(consol, container);
			newDRMPackLine.SetContainer(consol, container);
			Factory.Save();

			// Dispatch consignment
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("HSB1", dispatchConsignment.WDC_ConsignmentID);

			// assert load list, DTU and dcn on package states
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);
			var drm1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(drm1.PK);
			AssertEquals(dispatchConsignment.PK, plt1InNewFactory.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dispatchConsignment.PK, drm1InNewFactory.WPS_WDC_TransitDispatchConsignment);

			var dtu = groupContainers
						? newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).First()
						: newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals(loadList.PK, drm1InNewFactory.WPS_WDL_LoadList);

			return (consol, plt1InNewFactory, drm1InNewFactory, warehouse, dtu, newPLTPackLine);
		}

		#endregion

		#region TestConsol_ResendingDispatchInstructions_After_AddingContainerNumber

		public void TestConsol_ResendingDispatchInstructions_After_AddingContainerNumber()
		{
			/*
				1. Send Receive and Dispatch Instruction
					Consol A - Master Bill MAB1
					Container No Container Number
					Shipment A
						PLT 2
				2. Resend Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A <= Container Number has been updated.
					Shipment A
						PLT 2
				3. Existing load list and DTU get matched.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestDataForCombined("NZAKL", isArrival: false, parentProcessIsConsol: true);
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);

			// Send Receive and dispatch Instructions to CFS
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterImport = newFactoryAfterImport.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var dcnAfterImport = newFactoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dtuAfterImport = newFactoryAfterImport.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var dllAfterImport = newFactoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Precondition: Successfully imported an RCN.", "HSB1", rcnAfterImport.WRC_ConsignmentID);
			AssertEquals("Precondition: Successfully imported an DCN.", "HSB1", dcnAfterImport.WDC_ConsignmentID);
			AssertEquals("Precondition: Successfully imported an DTU.", "", dtuAfterImport.VehicleNumber);
			AssertEquals("Precondition: Successfully imported an DTU.", dtuAfterImport.PK, dllAfterImport.DispatchTransportationUnits.Single().PK);

			container.JC_ContainerNum = "CNT1";
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterReImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuAfterReImport = newFactoryAfterReImport.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var dllAfterReImport = newFactoryAfterReImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("CNT1", dtuAfterReImport.ContainerNumber);
			AssertEquals("Importers must reuse previous DTU as there is only one DTU on DLL.", dtuAfterImport.PK, dtuAfterReImport.PK);
			AssertEquals(dllAfterImport.PK, dllAfterReImport.PK);
			AssertEquals(dtuAfterReImport.PK, dllAfterReImport.DispatchTransportationUnits.Single().PK);
		}

		#endregion

		#region TestConsol_ResendingDispatchInstructions_TransitWarehouse_UpdateToSameContainerNumberAsForwarding

		public void TestConsol_ResendingDispatchInstructions_TransitWarehouse_UpdateToSameContainerNumberAsForwarding()
		{
			/*
				1. Send Receive and Dispatch Instruction
					Consol A - Master Bill MAB1
					Container No Container Number
					Shipment A
						PLT 2
				2. DLL has been started and DTU is given Container Number A
				3. Resend same Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A <= Container Number has been updated to same as TW.
					DTU.VehicleNumber updated to Container A
					Shipment A
						PLT 2
				4. Existing load list and DTU get matched, DLL is stopped.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestDataForCombined("NZAKL", isArrival: false, parentProcessIsConsol: true);
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, isArrival: false);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			// CFS Receive and dispatch Instructions
			var newFactoryAfterImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterImport = newFactoryAfterImport.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			var dcnAfterImport = newFactoryAfterImport.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dtuAfterImport = newFactoryAfterImport.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var dllAfterImport = newFactoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Precondition: Successfully imported an RCN.", "HSB1", rcnAfterImport.WRC_ConsignmentID);
			AssertEquals("Precondition: Successfully imported an DCN.", "HSB1", dcnAfterImport.WDC_ConsignmentID);
			AssertEquals("Precondition: Successfully imported an DTU.", "", dtuAfterImport.VehicleNumber);
			AssertEquals("Precondition: Successfully imported an DTU.", dtuAfterImport.PK, dllAfterImport.DispatchTransportationUnits.Single().PK);

			container.JC_ContainerNum = "CNT1";
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactoryAfterReImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuAfterReImport = newFactoryAfterReImport.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			var dllAfterReImport = newFactoryAfterReImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("CNT1", dtuAfterReImport.WDH_VehicleReference);
			AssertEquals("Importers must reuse previous DTU as there is only one DTU on DLL.", dtuAfterImport.PK, dtuAfterReImport.PK);
			AssertEquals(dllAfterImport.PK, dllAfterReImport.PK);
			AssertEquals(dtuAfterReImport.PK, dllAfterReImport.DispatchTransportationUnits.Single().PK);
		}

		#endregion

		#region TestConsol_ReSendingDispatchInstructions_After_AddingContainerNumberInForwarding

		public void TestConsol_ReSendingDispatchInstructions_After_AddingContainerNumberInForwarding()
		{
			/*
				1. Send  Dispatch Instruction
					Consol A - Master Bill MAB1
					Container No Container Number
					Shipment A
						PLT 1 PKG1
				2. Load List started and DTU does not have a container number yet.
				2. Resend same Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A <= Container Number has been updated.
					Shipment A
						PLT 1 PKG 2
				3. Existing load list and DTU get matched, DLL is stopped.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 1, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, plt2 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var dtu = loadList.DispatchTransportationUnits.Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals("", dtu.WDH_VehicleReference);
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtu, plt1InNewFactory);
			newBizOFactoryAfterRunningLogWalker.Save();

			// container number changes in forwarding
			container.JC_ContainerNum = "CNT1";
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var dtuAfterReSendingDispatchInstructions = loadListAfterReSendingDispatchInstructions.DispatchTransportationUnits.Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(false, loadListAfterReSendingDispatchInstructions.WDL_IsReadyToStage);
			AssertEquals("CNT1", dtuAfterReSendingDispatchInstructions.WDH_VehicleReference);
			AssertEquals(dtu.PK, dtuAfterReSendingDispatchInstructions.PK);
		}

		public void TestConsol_ReSendingDispatchInstructions_After_AddingContainerNumbersInForwarding()
		{
			/*
				1. Send  Dispatch Instruction
					Consol A - Master Bill MAB1
					2x Containers
					Shipment A
						PLT 1 PKG1
						CTN 1 PKG2
				2. Load List started and DTU does not have container numbers yet.
				2. Resend same Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A <= Container Number has been updated.
					Container B <= Container Number has been updated.
					Shipment A
						PLT 1 PKG 1
						CTN 1 PKG 2
				3. Existing load list and DTU get matched, DLL is stopped.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 2, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, ctn1 });
			Factory.Save();
			var outerPackLines = shipment.OuterPackLines;
			AssertEquals(2, outerPackLines.Count);
			outerPackLines[0].SetContainer(consol, container);
			outerPackLines[1].SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var dtus = loadList.DispatchTransportationUnits.ToArray();
			AssertEquals(2, dtus.Length);
			var dtu1 = dtus[0];
			var dtu2 = dtus[1];
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals("", dtu1.WDH_VehicleReference);
			AssertEquals("", dtu2.WDH_VehicleReference);
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Helper.LoadPackage(dtu1, plt1InNewFactory);
			newBizOFactoryAfterRunningLogWalker.Save();

			// container number changes in forwarding
			container.JC_ContainerNum = "CNT1";
			container.JC_ContainerCount = 1;
			var container2 = CreateContainer(consol, "CNT2", 1, "20GP");
			outerPackLines[1].SetContainer(consol, container2);
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListsAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(3, loadListsAfterReSendingDispatchInstructions.Length);

			var ctn1DTUAfterResending = loadListsAfterReSendingDispatchInstructions.Where(l => l.DispatchTransportationUnits.Any() && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT1").Single();
			AssertNotEquals(dtu1.PK, ctn1DTUAfterResending.DispatchTransportationUnits.Single().PK);
			AssertEquals(false, ctn1DTUAfterResending.WDL_IsReadyToStage);

			var ctn2DTUAfterResending = loadListsAfterReSendingDispatchInstructions.Where(l => l.DispatchTransportationUnits.Any() && l.DispatchTransportationUnits.Single().WDH_VehicleReference == "CNT2").Single();
			AssertNotEquals(dtu2.PK, ctn2DTUAfterResending.DispatchTransportationUnits.Single().PK);
			AssertEquals(false, ctn2DTUAfterResending.WDL_IsReadyToStage);
		}

		#endregion

		#region TestConsol_ReSendingDispatchInstructions_AfterAddingContainerNumberInBothTransitAndForwarding

		public void TestConsol_ReSendingDispatchInstructions_AfterAddingContainerNumberInBothTransitAndForwarding()
		{
			/*
				1. Send  Dispatch Instruction
					Consol A - Master Bill MAB1
					Container No Container Number
					Shipment A
						PLT 1 PKG1
				2. TW started loading DTU and given Container Number A
				2. Resend same Dispatch Instruction
					Consol A - Master Bill MAB1
					Container A <= Container Number has been updated.
					Shipment A
						PLT 1 PKG 2
				3. Existing load list and DTU get matched, DLL is stopped.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "", 1, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, plt2 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.SetContainer(consol, container);
			Factory.Save();

			// Send Dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
			var plt1InNewFactory = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(plt1.PK);

			var loadList = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var dtu = loadList.DispatchTransportationUnits.Single();
			AssertEquals(loadList.PK, plt1InNewFactory.WPS_WDL_LoadList);
			AssertEquals("", dtu.WDH_VehicleReference);
			loadList.WDL_IsReadyToStage = true;
			loadList.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			dtu.WDH_VehicleReference = "CNT1";
			newBizOFactoryAfterRunningLogWalker.Save();

			// container number changes in forwarding
			container.JC_ContainerNum = "CNT1";
			Factory.Save();

			// Resend dispatch instructions
			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterReSendingDispatchInstructions = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListAfterReSendingDispatchInstructions = newBizOFactoryAfterReSendingDispatchInstructions.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var dtuAfterReSendingDispatchInstructions = loadListAfterReSendingDispatchInstructions.DispatchTransportationUnits.Single();
			AssertEquals(loadList.PK, loadListAfterReSendingDispatchInstructions.PK);
			AssertEquals(false, loadListAfterReSendingDispatchInstructions.WDL_IsReadyToStage);
			AssertEquals("CNT1", dtuAfterReSendingDispatchInstructions.WDH_VehicleReference);
			AssertEquals(dtu.PK, dtuAfterReSendingDispatchInstructions.PK);
		}

		#endregion

		#region TestConsol_SendingDispatchInstructions_WhenMultipleDTUsMatchedByContainerNumber

		public void TestConsol_SendingDispatchInstructions_WhenMultipleMatchedByContainerNumber()
		{
			/*
				1. Send  Dispatch Instruction
					Dtu1, Dtu2, Dtu3 get matched and the latest created one dtu2 is updated.
				2. Change Dtu3's Create Time Then Resend Dispatch Instruction
					Dtu3 becomes the latest matched one and is then updated.
				3. Change Container Number And Container Type In Consol Then Resend Dispatch Instruction
					Dtu4, Dtu5, Dtu6 get matched and latest created one Dtu5 is updated.
			*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1", 1, "20GP");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			((ITransitWarehouseParent)shipment).AttachPackages(new[] { plt1, plt2 });
			Factory.Save();
			AssertEquals("Precondition: There must be one outer packline for the shipment.", 1, shipment.OuterPackLines.Count);
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.SetContainer(consol, container);
			Factory.Save();

			//Send Dispatch Instruction and 3 DTUs are matched. The latest created one should be chosen to update.
			var testDate = ZDateTime.Today;
			var dtu1 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU01", warehouse.PK, "CNT1", testDate);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU02", warehouse.PK, "CNT1", testDate.AddDays(2));
			var dtu3 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU03", warehouse.PK, "CNT1", testDate.AddDays(1));
			var dtu4 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU04", warehouse.PK, "CNT2", testDate);
			var dtu5 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU05", warehouse.PK, "CNT2", testDate.AddDays(2));
			var dtu6 = Helper.CreateDispatchTransportationUnitWithCreateTime("DTU06", warehouse.PK, "CNT2", testDate.AddDays(1));

			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			dtu1 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu1.PK);
			dtu2 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu2.PK);
			dtu3 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu3.PK);
			dtu4 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu4.PK);
			dtu5 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu5.PK);
			dtu6 = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu6.PK);

			AssertEquals("", dtu1.ContainerTypeCode);
			AssertEquals("20GP", dtu2.ContainerTypeCode);
			AssertEquals("", dtu3.ContainerTypeCode);
			AssertEquals("", dtu4.ContainerTypeCode);
			AssertEquals("", dtu5.ContainerTypeCode);
			AssertEquals("", dtu6.ContainerTypeCode);

			//Change dtu3's create time and it becomes the latest matching DTU.
			dtu3.WDH_SystemCreateTimeUtc = testDate.AddDays(3);
			newFactory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newFactoryAfterCreateTimeChanged = new BusinessObjectFactory { RefreshEnabled = false };
			dtu1 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu1.PK);
			dtu2 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu2.PK);
			dtu3 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu3.PK);
			dtu4 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu4.PK);
			dtu5 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu5.PK);
			dtu6 = newFactoryAfterCreateTimeChanged.Load<WhsItemDispatchTransportationUnit>(dtu6.PK);

			AssertEquals("", dtu1.ContainerTypeCode);
			AssertEquals("20GP", dtu2.ContainerTypeCode);
			AssertEquals("20GP", dtu3.ContainerTypeCode);
			AssertEquals("", dtu4.ContainerTypeCode);
			AssertEquals("", dtu5.ContainerTypeCode);
			AssertEquals("", dtu6.ContainerTypeCode);

			//Change container number and container type in consol.
			container.JC_ContainerNum = "CNT2";
			container.JC_RC = consol.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			TriggerAndFireTransitRequestForRelease(consol);

			var newFactoryAfterContainerInConsolChanged = new BusinessObjectFactory { RefreshEnabled = false };
			dtu1 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu1.PK);
			dtu2 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu2.PK);
			dtu3 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu3.PK);
			dtu4 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu4.PK);
			dtu5 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu5.PK);
			dtu6 = newFactoryAfterContainerInConsolChanged.Load<WhsItemDispatchTransportationUnit>(dtu6.PK);

			AssertEquals("", dtu1.ContainerTypeCode);
			AssertNull(dtu2);
			AssertNull(dtu3);
			AssertEquals("", dtu4.ContainerTypeCode);
			AssertEquals("40GP", dtu5.ContainerTypeCode);
			AssertEquals("", dtu6.ContainerTypeCode);
		}

		#endregion

		static void AssertPackage(Business.WhsItemPackageStateCollection packageStates, string packageID, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, string status, bool isRemoveFromDTU = false)
		{
			AssertPackage((IEnumerable<WhsItemPackageState>)packageStates, packageID, dtu, dll, status, isRemoveFromDTU);
		}

		static void AssertPackage(IEnumerable<WhsItemPackageState> packageStates, string packageID, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, string status, bool isRemoveFromDTU = false)
		{
			var packageState = packageStates.Single(p => p.Package.KP_PackageID == packageID);
			AssertEquals(status, packageState.WPS_Status);
			if (dtu != null)
			{
				AssertEquals(dtu.PK, packageState.DispatchTransportationUnit.PK);
			}
			else
			{
				AssertNull(packageState.DispatchTransportationUnit);
			}

			if (dll != null)
			{
				AssertEquals(dll.PK, packageState.DispatchLoadList.PK);
			}
			else
			{
				AssertNull(packageState.DispatchLoadList);
			}

			AssertEquals(isRemoveFromDTU, packageState.WPS_RemoveFromDTU);
		}
	}
}
