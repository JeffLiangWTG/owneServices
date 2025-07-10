using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class DispatchConsignmentCIN750NotificationHistoryManagerTest : TestCaseWithFactory
	{
		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				Out(4),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment are reported Out completely.")
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_Overpack() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				DoOverpack(),
				ErrorOut("Cannot send CIN 750 Out Notification because the reported Quantity of Packages on the Dispatch Consignment does not match the Quantity in the Warehouse. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATDCN1", "EDIDATDCN1", doOverpack: true),
				Out(2),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment are reported Out completely.")
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_DoOverpackThenLoadToULD() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				DoOverpackThenLoadToULD(),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATDCN1", "EDIDATDCN1", doOverpack: true),
				Out(2)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_LoadToULD() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				LoadToULD(),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_LoadToHandlingUnit() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				LoadToHandlingUnit(),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_LoadToHandlingUnitThenULD() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				LoadToHandlingUnitThenULD(),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_DoOverpackThenLoadToHandlingUnitThenULD() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				DoOverpackThenLoadToHandlingUnitThenULD(),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATDCN1", "EDIDATDCN1", doOverpack: true),
				Out(2)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithMBL_DCNWithoutHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveNoHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment does not have a House Bill to do a Consolidation into Master Bill.")
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithoutMBL_DCNWithoutHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment does not have a House Bill to do a Consolidation.")
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's House Bill does not match the reported Packages. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATRCN1", "HSB1"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithMBL_DCNWithHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATRCN1", "HSB1"),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "MAB-1"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithMBL_DCNWithHBL_DoHalf() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATRCN1", "HSB1", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "EDIDATRCN1", "HSB1", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "MAB-1", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "MAB-1", doHalf: true),
				OutHalf(),
				Out(2)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithHBL_DCNWithMBL_DCNWithHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "MAB-1"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithHBL_DCNWithMBL_DCNWithAnotherHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveAnotherHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's House Bill does not match the reported Packages. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "HSB2"),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB2", "MAB-1"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithMBL_DCNWithHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithAnotherMBL_DCNWithHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveAnotherMasterBill, HouseBillConditions.HaveHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's Master Bill does not match the reported Packages. Please send CIN 750 Deconsolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750DeconsNotification, "MAB-1", "HSB1"),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "MAB-2"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithAnotherMBL_DCNWithAnotherHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveAnotherMasterBill, HouseBillConditions.HaveAnotherHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's Master Bill does not match the reported Packages. Please send CIN 750 Deconsolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750DeconsNotification, "MAB-1", "HSB2"),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB2", "MAB-2"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithAnotherMBL_DCNWithAnotherHBL_DoHalf() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveAnotherMasterBill, HouseBillConditions.HaveAnotherHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's Master Bill does not match the reported Packages. Please send CIN 750 Deconsolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750DeconsNotification, "MAB-1", "HSB2", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's Master Bill does not match the reported Packages. Please send CIN 750 Deconsolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750DeconsNotification, "MAB-1", "HSB2", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB2", "MAB-2", doHalf: true),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB2", "MAB-2", doHalf: true),
				OutHalf(),
				Out(2)
			);

		public void TestGetNextMessageType_RCNWithMBL_RCNWithHBL_DCNWithoutMBL_DCNWithAnotherHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveAnotherHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment haven't been reported De-consolidations to House Bill yet. Please send CIN 750 Deconsolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750DeconsNotification, "MAB-1", "HSB2"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithHBL_DCNWithoutMBL_DCNWithAnotherHBL() =>
			TestGetNextMessageTypeCore_FullDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveAnotherHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Dispatch Consignment's House Bill does not match the reported Packages. Please send CIN 750 Consolidation Notification."),
				Step(CIN750NotificationMessageTypes.CIN750ConsNotification, "HSB1", "HSB2"),
				Out(4)
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_HalfDeparted() =>
			TestGetNextMessageTypeCore_HalfDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				Out(2),
				ErrorOut("Cannot send CIN 750 Out Notification because some Packages on the Dispatch Consignment have not been Departed yet.")
			);

		public void TestGetNextMessageType_RCNWithoutMBL_RCNWithoutHBL_DCNWithoutMBL_DCNWithoutHBL_NoDeparted() =>
			TestGetNextMessageTypeCore_NoDeparted(
				rcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				dcnCondition: (MasterBillConditions.HaveNoMasterBill, HouseBillConditions.HaveNoHouseBill),
				ErrorOut("Cannot send CIN 750 Out Notification because the Packages on the Dispatch Consignment have not been Departed yet.")
			);

		void TestGetNextMessageTypeCore_FullDeparted((MasterBillConditions rcnMBL, HouseBillConditions rcnHBL) rcnCondition, (MasterBillConditions dcnMBL, HouseBillConditions dcnHBL) dcnCondition, params CINStep[] steps)
			=> TestGetNextMessageTypeCore(rcnCondition, dcnCondition, DepartedConditions.FullDeparted, steps);

		void TestGetNextMessageTypeCore_HalfDeparted((MasterBillConditions rcnMBL, HouseBillConditions rcnHBL) rcnCondition, (MasterBillConditions dcnMBL, HouseBillConditions dcnHBL) dcnCondition, params CINStep[] steps)
			=> TestGetNextMessageTypeCore(rcnCondition, dcnCondition, DepartedConditions.HalfDeparted, steps);

		void TestGetNextMessageTypeCore_NoDeparted((MasterBillConditions rcnMBL, HouseBillConditions rcnHBL) rcnCondition, (MasterBillConditions dcnMBL, HouseBillConditions dcnHBL) dcnCondition, params CINStep[] steps)
			=> TestGetNextMessageTypeCore(rcnCondition, dcnCondition, DepartedConditions.NotDeparted, steps);

		void TestGetNextMessageTypeCore((MasterBillConditions rcnMBL, HouseBillConditions rcnHBL) rcnCondition, (MasterBillConditions dcnMBL, HouseBillConditions dcnHBL) dcnCondition, DepartedConditions departedCondition, params CINStep[] steps)
		{
			var (rcn, dcn, dtu, dll) = PrepareTestData(rcnCondition.rcnMBL, rcnCondition.rcnHBL, dcnCondition.dcnMBL, dcnCondition.dcnHBL, departedCondition);
			int stepNo = 1;

			foreach (var step in steps)
			{
				if (step.DoOverpack && !step.LoadToULD && !step.LoadToHandlingUnit && !step.MessageType.HasValue)
				{
					var overpack1 = Helper.CreateOverpackPackage("OVP1", dcn, null, dcn: dcn, dtu: dtu, dll: dll);
					var overpack2 = Helper.CreateOverpackPackage("OVP2", dcn, null, rcn: rcn, dcn: dcn, dtu: dtu, dll: dll);
					var innerPackages = dcn.PackageStates.Where(p => p.WPS_UnitType == "PKG").ToList();
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[0], ZDateTimeOffset.Now, "ABC", overpack1);
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[1], ZDateTimeOffset.Now, "ABC", overpack1);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[2], ZDateTimeOffset.Now, "DEF", overpack2);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[3], ZDateTimeOffset.Now, "DEF", overpack2);
					overpack1.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					overpack2.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					Factory.Save();
				}
				else if (step.DoOverpack && step.LoadToULD && !step.LoadToHandlingUnit && !step.MessageType.HasValue)
				{
					var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", dcn.WDC_WW_Warehouse);
					var dll2 = Helper.CreateDispatchLoadList("DLL2", dcn.WDC_WW_Warehouse);

					var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, dispatchUnit: dtu, dispatchLoadList: dll);
					packageStateULD.WPS_UnitType = "ULD";
					packageStateULD.WPS_IsHandlingUnit = true;
					Factory.Save();

					var overpack1 = Helper.CreateOverpackPackage("OVP1", dcn, null, dcn: dcn, dtu: dtu, dll: dll);
					var overpack2 = Helper.CreateOverpackPackage("OVP2", dcn, null, rcn: rcn, dcn: dcn, dtu: dtu, dll: dll);
					var innerPackages = dcn.PackageStates.Where(p => p.WPS_UnitType == "PKG").ToList();
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[0], ZDateTimeOffset.Now, "ABC", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[1], ZDateTimeOffset.Now, "ABC", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[2], ZDateTimeOffset.Now, "DEF", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[3], ZDateTimeOffset.Now, "DEF", packageStateULD);
					overpack1.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					overpack1.WPS_WDL_LoadList = dll2.PK;
					overpack1.WPS_WDH_TransitDispatchHeader = uld.PK;
					overpack2.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					overpack2.WPS_WDL_LoadList = dll2.PK;
					overpack2.WPS_WDH_TransitDispatchHeader = uld.PK;

					foreach (var packageState in dcn.PackageStates)
					{
						packageState.WPS_WDL_LoadList = dll2.PK;
						packageState.WPS_WDH_TransitDispatchHeader = uld.PK;

						Helper.PackPackageIntoHandlingUnit(packageStateULD, packageState, ZDateTimeOffset.Now, "AAA", packageStateULD);
					}

					Factory.Save();
				}
				else if (step.DoOverpack && step.LoadToULD && step.LoadToHandlingUnit && !step.MessageType.HasValue)
				{
					var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", dcn.WDC_WW_Warehouse);
					var dll2 = Helper.CreateDispatchLoadList("DLL2", dcn.WDC_WW_Warehouse, dll.Location);

					var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, dispatchUnit: dtu, dispatchLoadList: dll);
					packageStateULD.WPS_UnitType = "ULD";
					packageStateULD.WPS_IsHandlingUnit = true;

					var rtu = Helper.CreateReceiveTransportationUnit("RTU2", dcn.WDC_WW_Warehouse, dll2.WDL_WL_StagingLocation);
					var handlingUnit = Helper.CreatePackageHandlingUnit();
					var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dll: dll2, dtu: uld);
					handlingUnitPackage.WPS_WL_LastLocation = dll2.WDL_WL_StagingLocation;
					Factory.Save();

					var overpack1 = Helper.CreateOverpackPackage("OVP1", dcn, null, dcn: dcn, dtu: dtu, dll: dll);
					var overpack2 = Helper.CreateOverpackPackage("OVP2", dcn, null, rcn: rcn, dcn: dcn, dtu: dtu, dll: dll);
					var innerPackages = dcn.PackageStates.Where(p => p.WPS_UnitType == "PKG").ToList();
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[0], ZDateTimeOffset.Now, "ABC", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack1, innerPackages[1], ZDateTimeOffset.Now, "ABC", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[2], ZDateTimeOffset.Now, "DEF", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(overpack2, innerPackages[3], ZDateTimeOffset.Now, "DEF", packageStateULD);
					overpack1.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					overpack1.WPS_WDL_LoadList = dll2.PK;
					overpack1.WPS_WDH_TransitDispatchHeader = uld.PK;
					overpack2.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
					overpack2.WPS_WDL_LoadList = dll2.PK;
					overpack2.WPS_WDH_TransitDispatchHeader = uld.PK;

					Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, overpack1, ZDateTimeOffset.Now, "DEF", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, overpack2, ZDateTimeOffset.Now, "DEF", packageStateULD);
					Helper.PackPackageIntoHandlingUnit(packageStateULD, handlingUnitPackage, ZDateTimeOffset.Now, "DEF", packageStateULD);

					foreach (var packageState in dcn.PackageStates)
					{
						packageState.WPS_WDL_LoadList = dll2.PK;
						packageState.WPS_WDH_TransitDispatchHeader = uld.PK;
					}

					Factory.Save();
				}
				else if (step.LoadToULD && !step.LoadToHandlingUnit)
				{
					var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", dcn.WDC_WW_Warehouse);
					var dll2 = Helper.CreateDispatchLoadList("DLL2", dcn.WDC_WW_Warehouse);

					var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, dispatchUnit: dtu, dispatchLoadList: dll);
					packageStateULD.WPS_UnitType = "ULD";
					packageStateULD.WPS_IsHandlingUnit = true;
					Factory.Save();

					foreach (var packageState in dcn.PackageStates)
					{
						packageState.WPS_WDL_LoadList = dll2.PK;
						packageState.WPS_WDH_TransitDispatchHeader = uld.PK;

						Helper.PackPackageIntoHandlingUnit(packageStateULD, packageState, ZDateTimeOffset.Now, "AAA", packageStateULD);
					}

					Factory.Save();
				}
				else if (step.LoadToHandlingUnit && !step.LoadToULD)
				{
					var rtu = Helper.CreateReceiveTransportationUnit("RTU2", dcn.WDC_WW_Warehouse, dll.WDL_WL_StagingLocation);
					var handlingUnit = Helper.CreatePackageHandlingUnit();
					var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dll: dll, dtu: dtu);
					handlingUnitPackage.WPS_WL_LastLocation = dll.WDL_WL_StagingLocation;

					foreach (var packageState in dcn.PackageStates)
					{
						Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
					}

					Factory.Save();
				}
				else if (step.LoadToHandlingUnit && step.LoadToULD)
				{
					var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", dcn.WDC_WW_Warehouse);
					var dll2 = Helper.CreateDispatchLoadList("DLL2", dcn.WDC_WW_Warehouse, dll.Location);

					var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, dispatchUnit: dtu, dispatchLoadList: dll);
					packageStateULD.WPS_UnitType = "ULD";
					packageStateULD.WPS_IsHandlingUnit = true;
					Factory.Save();

					var rtu = Helper.CreateReceiveTransportationUnit("RTU2", dcn.WDC_WW_Warehouse, dll.WDL_WL_StagingLocation);
					var handlingUnit = Helper.CreatePackageHandlingUnit();
					var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dll: dll2, dtu: uld);
					handlingUnitPackage.WPS_WL_LastLocation = dll2.WDL_WL_StagingLocation;

					foreach (var packageState in dcn.PackageStates)
					{
						packageState.WPS_WDL_LoadList = dll2.PK;
						packageState.WPS_WDH_TransitDispatchHeader = uld.PK;

						Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState, ZDateTimeOffset.Now, "AAA", packageStateULD);
					}
					Helper.PackPackageIntoHandlingUnit(packageStateULD, handlingUnitPackage, ZDateTimeOffset.Now, "AAA", packageStateULD);

					Factory.Save();
				}
				else
				{
					var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
					if (step.ErrorType.HasValue && step.ErrorMessage != null)
					{
						var errorMessage = historyManager.GetCheckSendingNotificationMessage(step.ErrorType.Value);
						AssertEquals($"Error Message of Step {stepNo}.", step.ErrorMessage, errorMessage);
					}
					else
					{
						var nextStep = historyManager.GetNextMessageType();
						AssertEquals($"Message Type of Step {stepNo}.", step.MessageType, nextStep);

						if (step.MessageType == CIN750NotificationMessageTypes.CIN750OutNotification && step.ExpectedOutQuantity != 0)
						{
							AssertEquals("Expected Out Quantity", step.ExpectedOutQuantity, historyManager.AdditionalDataForOut.PackageQuantityReadyToOut);
						}
					}
					if (step.MessageType.HasValue)
					{
						CreateHistoryForStep(rcn, dcn, step);
					}
				}
				stepNo++;
			}
			var finalHisitoryManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var finalStep = finalHisitoryManager.GetNextMessageType();
			AssertEquals(null, finalStep);
		}

		void CreateHistoryForStep(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, CINStep step)
		{
			switch (step.MessageType)
			{
				case CIN750NotificationMessageTypes.CIN750DeconsNotification:
					{
						var toRefType = step.ToCode.StartsWith("MAB") ? "AWB" : step.ToCode.StartsWith("HSB") ? "HWB" : "REF";
						var fromRefType = step.FromCode.StartsWith("MAB") ? "AWB" : step.FromCode.StartsWith("HSB") ? "HWB" : "REF";
						var qty = step.DoHalf ? 2 : 4;
						Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(rcn.HouseBillNumber)}|JOB=RC0000001|MBL={FillWithHyphenIfEmpty(rcn.MasterBillNumber)}|MST=CIN750DeconsNotification_From|OTY={qty}|PTP={fromRefType}|RFN=EDIDATRCN1|WGT=40");
						Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(dcn.HouseBillNumber)}|JOB=RC0000001|MBL={FillWithHyphenIfEmpty(dcn.MasterBillNumber)}|MST=CIN750DeconsNotification_To|OTY={qty}|PTP={toRefType}|RFN=EDIDATDCN1|WGT=40");
						break;
					}
				case CIN750NotificationMessageTypes.CIN750ConsNotification:
					{
						var toRefType = step.ToCode.StartsWith("MAB") ? "AWB" : step.ToCode.StartsWith("HSB") ? "HWB" : "REF";
						var fromRefType = step.FromCode.StartsWith("MAB") ? "AWB" : step.FromCode.StartsWith("HSB") ? "HWB" : "REF";
						var fromQty = step.DoHalf ? 2 : 4;
						if (fromRefType == "HWB" && toRefType == "AWB" && (rcn.HouseBillNumber != dcn.HouseBillNumber || !rcn.MasterBillNumber.IsEmpty))
						{
							Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(dcn.HouseBillNumber)}|JOB=DC0000001|MBL={FillWithHyphenIfEmpty(dcn.MasterBillNumber)}|MST=CIN750ConsNotification_From|OTY={fromQty}|PTP={fromRefType}|RFN=EDIDATDCN1|WGT=40");
						}
						else
						{
							Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(rcn.HouseBillNumber)}|JOB=RC0000001|MBL={FillWithHyphenIfEmpty(rcn.MasterBillNumber)}|MST=CIN750ConsNotification_From|OTY={fromQty}|PTP={fromRefType}|RFN=EDIDATRCN1|WGT=40");
						}
						var toQty = step.DoOverpack || step.DoHalf ? 2 : 4;
						Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(dcn.HouseBillNumber)}|JOB=DC0000001|MBL={FillWithHyphenIfEmpty(dcn.MasterBillNumber)}|MST=CIN750ConsNotification_To|OTY={toQty}|PTP={toRefType}|RFN=EDIDATDCN1|WGT=40");
						break;
					}
				case CIN750NotificationMessageTypes.CIN750OutNotification:
					{
						var outQty = step.DoHalf ? step.ExpectedOutQuantity / 2 : step.ExpectedOutQuantity;
						var outRefType = !dcn.MasterBillNumber.IsEmpty ? "AWB" : !dcn.HouseBillNumber.IsEmpty ? "HWB" : "REF";
						Helper.CreateStmALog(dcn, EventCodes.MessageSent, $"|HBL={FillWithHyphenIfEmpty(dcn.HouseBillNumber)}|JOB=RC0000001|MBL={FillWithHyphenIfEmpty(dcn.MasterBillNumber)}|MST=CIN750OutNotification|OTY={outQty}|PTP={outRefType}|RFN=EDIDATDCN1|WGT=40");
						break;
					}
				default:
					break;
			}
		}

		(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll) PrepareTestData(MasterBillConditions rcnMBL, HouseBillConditions rcnHBL, MasterBillConditions dcnMBL, HouseBillConditions dcnHBL, DepartedConditions departedConsition)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_JobID = "DC0000001";
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_JobID = "RC0000001";

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var rcnAWB = "-";
			var rcnHWB = "-";
			var inRefType = "REF";
			if (rcnHBL == HouseBillConditions.HaveHouseBill)
			{
				rcn.WRC_HouseBillNumber = "HSB1";
				rcnHWB = "HSB1";
				inRefType = "HWB";
			}
			if (rcnMBL == MasterBillConditions.HaveMasterBill)
			{
				Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
				rcnAWB = "MAB-1";
				inRefType = "AWB";
			}
			if (dcnHBL == HouseBillConditions.HaveHouseBill)
			{
				dcn.WDC_HouseBillNumber = "HSB1";
			}
			else if (dcnHBL == HouseBillConditions.HaveAnotherHouseBill)
			{
				dcn.WDC_HouseBillNumber = "HSB2";
			}

			if (dcnMBL == MasterBillConditions.HaveMasterBill)
			{
				Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			}
			else if (dcnMBL == MasterBillConditions.HaveAnotherMasterBill)
			{
				Helper.CreateAdditionalReference(dcn, "MAB-2", AdditionalReferenceTypes.Codes.MasterBill);
			}

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			if (departedConsition == DepartedConditions.NotDeparted)
			{
				var outerPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn);
				SetPackageStateDetails(outerPackageState, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
				var outerPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn);
				SetPackageStateDetails(outerPackageState2, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			}
			else
			{
				var outerPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
				SetPackageStateDetails(outerPackageState, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
				var outerPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
				SetPackageStateDetails(outerPackageState2, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			}

			if (departedConsition == DepartedConditions.FullDeparted)
			{
				var outerPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
				SetPackageStateDetails(outerPackageState3, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
				var outerPackageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
				SetPackageStateDetails(outerPackageState4, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			}
			else
			{
				var outerPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn);
				SetPackageStateDetails(outerPackageState3, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
				var outerPackageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn);
				SetPackageStateDetails(outerPackageState4, 10, "BOOKS", TransitWarehouseReceiveAs.Codes.ScannedIn);
			}

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, $"|HBL={rcnHWB}|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750InNotification|OTY=4|PTP={inRefType}|RFN=EDIDATRCN1|WGT=40");

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			var ctoAddress = Factory.NewWithValidTestData<OrgAddress>();
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			var cinCodeForCTO = ctoJobDocAddress.Address.Header.CustomsCodes.AddNew();
			cinCodeForCTO.OK_CodeType = OrgCusCode.FranceCodeTypes.CIN;
			cinCodeForCTO.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			cinCodeForCTO.OK_CustomsRegNo = "C002";

			Factory.Save();

			return (rcn, dcn, dtu, dll);
		}

		protected void SetPackageStateDetails(WhsItemPackageState packageState, ZDecimal weight, ZString description, string receiveAs = "")
		{
			packageState.WPS_ReceivedAs = string.IsNullOrEmpty(receiveAs) ? TransitWarehouseReceiveAs.Codes.ScannedIn : receiveAs;
			packageState.Package.KP_Weight = weight;
			packageState.Package.KP_GoodsDescription = description;
		}

		class CINStep
		{
			public CIN750NotificationMessageTypes? MessageType { get; set; }
			public string FromCode { get; set; }
			public string ToCode { get; set; }
			public string ErrorMessage { get; set; }
			public int ExpectedOutQuantity { get; set; }
			public bool DoHalf { get; set; }
			public bool DoOverpack { get; set; }
			public bool LoadToULD { get; set; }
			public bool LoadToHandlingUnit { get; set; }

			public CIN750NotificationMessageTypes? ErrorType { get; set; }
		}

		CINStep Step(CIN750NotificationMessageTypes type, string fromCode, string toCode, bool doHalf = false, bool doOverpack = false)
		{
			return new CINStep()
			{
				MessageType = type,
				FromCode = fromCode,
				ToCode = toCode,
				DoHalf = doHalf,
				DoOverpack = doOverpack
			};
		}

		CINStep Out(int expectedOutQuantity) => new CINStep() { MessageType = CIN750NotificationMessageTypes.CIN750OutNotification, ExpectedOutQuantity = expectedOutQuantity };
		CINStep OutHalf() => new CINStep() { MessageType = CIN750NotificationMessageTypes.CIN750OutNotification, DoHalf = true, ExpectedOutQuantity = 4 };
		CINStep ErrorOut(string errorMessage) => new CINStep() { ErrorType = CIN750NotificationMessageTypes.CIN750OutNotification, ErrorMessage = errorMessage };
		CINStep DoOverpack() => new CINStep() { DoOverpack = true };
		CINStep LoadToULD() => new CINStep() { LoadToULD = true };
		CINStep LoadToHandlingUnit() => new CINStep() { LoadToHandlingUnit = true };
		CINStep LoadToHandlingUnitThenULD() => new CINStep { LoadToHandlingUnit = true, LoadToULD = true };
		CINStep DoOverpackThenLoadToULD() => new CINStep { DoOverpack = true, LoadToULD = true };
		CINStep DoOverpackThenLoadToHandlingUnitThenULD() => new CINStep { DoOverpack = true, LoadToHandlingUnit = true, LoadToULD = true };

		ZString FillWithHyphenIfEmpty(ZString source) => source.IsEmpty ? "-" : source;

		enum HouseBillConditions
		{
			HaveNoHouseBill,
			HaveHouseBill,
			HaveAnotherHouseBill
		}

		enum MasterBillConditions
		{
			HaveNoMasterBill,
			HaveMasterBill,
			HaveAnotherMasterBill
		}

		enum DepartedConditions
		{
			NotDeparted,
			HalfDeparted,
			FullDeparted
		}

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
