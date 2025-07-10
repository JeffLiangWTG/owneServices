using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ForwardingToTWBookedAndBlindMixedIntegrationTests : IntegrationTestCaseWithFactory
	{
		#region TestAttachBlindPackagesAfterReceivingBookedPackages

		public void TestAttachBlindPackagesAfterReceivingBookedPackages()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1 <= Hasn't received into the warehouse yet.

			2. Send receive instructions for above consol
			3. Receive below packages to TW.
				RCN B
					PLT2 
					CTN2
			4. Receive Shipment A's booked packages but do not send back outturn
			5. Assign those blind packages to Shipment A
			6. Send dispatch instructions
			7. DCN created for Shipment A,
					Shipment A
						PLT 2
							(i) PKG1
							(ii) PLT1
						PLT2 PLT 
						PKG2 
						CTN2
			 */

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var container = CreateContainer(consol, "CNT1");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateTwoPallets = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 2);
			var packageStateForSinglePallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 1);
			AssertEquals(2, packageStateTwoPallets.Package.KP_PackageQty);
			AssertEquals(1, packageStateForSinglePallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForSinglePallet, rtu1, "PKG1");
			UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT2");

			// CFS Receive 1 Pallet and carton blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var pkg2InNewFactory = newFactory.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { pkg2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(5, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
		}

		public void TestAttachBlindPackagesAfterReceivingAdditionalPackages()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1 <= Hasn't received into the warehouse yet.

			2. Send receive instructions for above consol
			3. Receive below packages to TW.
				RCN B
					PLT2 
					CTN2
			4. Receive Shipment A's booked packages but do not send back outturn
			5. RCN for Shipment A receive Additional PLT with ID SUR1
			6. Assign RCN B's blind packages to Shipment A
			7. Send dispatch instructions
			8. DCN instructions are accepted and suplus package will be ignored.
*/

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CNT1");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateTwoPallets = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 2);
			var packageStateForSinglePallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 1);
			AssertEquals(2, packageStateTwoPallets.Package.KP_PackageQty);
			AssertEquals(1, packageStateForSinglePallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var plt2 = UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT2");
			UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateForSinglePallet, rtu1, "PKG1");
			var sur1 = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Pallet, "SUR1", AttachablePackageStateStatuses.Codes.ARV, rtu1);

			// CFS Receive 1 Pallet and carton blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var pkg2InNewFactory = newFactory.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { pkg2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Will create new DCN because all packlines have external reference and can be matched.",
				1, factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Length);

			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(5, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
		}

		public void TestAttachBlindPackagesAfterReceivingBookedPackages_TransitSendBackOutturn()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1 <= Hasn't received into the warehouse yet.

			2. Send receive instructions for above consol
			3. Receive below packages to TW.
				RCN B
					PLT2 
					CTN2
			4. Send Outturn to Forwarding after receiving Shipment A's booked packages
			5. Assign those blind packages to Shipment A
			6. Send dispatch instructions
			7. DCN created for Shipment A,
					Shipment A
						PLT 2
						PKG1 PLT 1 
						PLT2 
						CTN2
			 */

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CNT1");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateTwoPallets = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 2);
			var packageStateForSinglePallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 1);
			AssertEquals(2, packageStateTwoPallets.Package.KP_PackageQty);
			AssertEquals(1, packageStateForSinglePallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var plt2 = UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT2");
			UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateForSinglePallet, rtu1, "PLT3");

			// CFS Receive 1 Pallet and carton blindly
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment); // send back outturn

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var pkg2InNewFactory = newFactory.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { pkg2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(5, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT3").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
		}

		public void TestAttachBlindPackagesAfterReceivingBookedPackages_TransitSendBackOutturn_Surplus()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1 <= Hasn't received into the warehouse yet.

			2. Send receive instructions for above consol
			3. Receive below packages to TW.
				RCN B
					PLT2 
					CTN2
			4. Send Outturn to Forwarding after receiving Shipment A's booked packages and one additional SUR1 Package
			5. Assign those blind packages to Shipment A
			6. Send dispatch instructions
			7. DCN created for Shipment A,
					Shipment A
						PLT 2
						PKG1 PLT 1 
						PLT2 
						CTN2
			 */

			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CNT1");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateTwoPallets = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 2);
			var packageStateForSinglePallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.Package.KP_PackageQty == 1);
			AssertEquals(2, packageStateTwoPallets.Package.KP_PackageQty);
			AssertEquals(1, packageStateForSinglePallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var plt2 = UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT2");
			UnloadAndLabelPackage(packageStateTwoPallets, rtu1, "PLT1");
			UnloadAndLabelPackage(packageStateForSinglePallet, rtu1, "PLT3");
			var sur1 = Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Pallet, "SUR1", AttachablePackageStateStatuses.Codes.ARV, rtu1);
			sur1.Package.KP_ExternalReference = packageStateTwoPallets.Package.KP_ExternalReference;

			// CFS Receive 1 Pallet and carton blindly
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var pkg2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			CreateWorkflowTemplateForReceiveConsignmentToShipment();
			TriggerAndFireOutturn(receiveConsignment); // send back outturn

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var pkg2InNewFactory = newFactory.Load<WhsItemPackageState>(pkg2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { pkg2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(6, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PLT3").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "SUR1").Package.KP_F3_NKPackType);
		}

		#endregion

		#region TestAttachBlindPackagesAfterSendingReceiveInstructionsToBookPackages

		public void TestAttachBlindPackagesAfterSendingReceiveInstructionsToBookPackages()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1
			2. Send receive instruction and do not receive packages to warehouse yet. All Packages remain booked.
			3. Receive below packages to TW.
				RCN B
					PKG2 
					CTN2
			4. Assign those blind packages to Shipment A
			5. Send dispatch instructions
			6. DCN created for Shipment A,
					Shipment A
						PLT 2
						PKG1 PLT 1
						PKG2
						CTN2
			 */
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CNT1");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var plt2InNewFactory = newFactory.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { plt2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dispatchConsignment = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dispatchConsignment.PackageStates;
			AssertEquals(4, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageQty == 2).Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
		}

		public void TestAttachBlindAndBookedPackagesAfterSendingReceiveInstructionsToBookPackages()
		{
			/*
			 1. Create consol and shipment with following packlines.
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1
			2. Send receive instruction and do not receive packages to warehouse yet. All Packages remain booked.
			3. Receive below packages to TW.
				RCN B
					PKG2 
					CTN2
			4. Assign those blind packages to Shipment A
			5. Modify Shipment A so that extra package is booked
				Consol A
					Container 1
					Shipment A
						PLT 2
						PKG1 PLT 1
			5. Send dispatch instructions
			6. Dispatch instructions are accepted only packages in RCN are assigned to DCN.
			*/
			var (cfs, today, warehouse, consignor, consignee, _) = CreateTestData(false, parentProcessIsConsol: true);

			// Shipment
			//   route : NZAKL -> AUSYD
			//   depot : ^
			var consol = CreateConsol("MAB1", null, "NZAKL", "AUSYD");
			var shipment = CreateShipment(consol, "HSB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var container = CreateContainer(consol, "CNT1");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet, container);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container, "PKG1");
			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol); // send receive instructions

			// CFS Receive 2 pallets blindly
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var plt2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);
			var ctn1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "CTN1", AttachablePackageStateStatuses.Codes.ARV, receiveUnit: rtu, entryNum: shipment.JobNumber);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var plt2InNewFactory = newFactory.Load<WhsItemPackageState>(plt2.PK);
			var ctn1InNewFactory = newFactory.Load<WhsItemPackageState>(ctn1.PK);
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { plt2InNewFactory });
			((ITransitWarehouseParent)shipmentInNewFactory).AttachPackages(new[] { ctn1InNewFactory });
			AssertEquals("Precondition", 4, shipmentInNewFactory.OuterPackLines.Count);
			newFactory.Save();

			TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(newFactory.Load<ForwardingConsol>(consol.PK)); // send dispatch instructions

			var factoryAfterDispatch = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcn = factoryAfterDispatch.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			var dcnPackageStates = dcn.PackageStates;
			AssertEquals("Packages without ids are retrieved from RCN.", 4, dcnPackageStates.Count);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageQty == 2).Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG1").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Pallet, dcnPackageStates.Single(p => p.Package.KP_PackageID == "PKG2").Package.KP_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Carton, dcnPackageStates.Single(p => p.Package.KP_PackageID == "CTN1").Package.KP_F3_NKPackType);
		}

		#endregion
	}
}
