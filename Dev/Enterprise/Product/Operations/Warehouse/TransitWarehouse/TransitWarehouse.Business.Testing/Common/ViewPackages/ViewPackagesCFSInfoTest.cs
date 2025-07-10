using Enterprise.Freight.Forwarding.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class ViewPackagesCFSInfoTest : WhsTransitTestCaseWithFactory
	{
		#region TestWarehouseName

		public void TestWarehouseName()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var transitWarehouse = Helper.CreateTRWWarehouse();
			var productWarehouse = Helper.CreateWarehouse("PRW");
			productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			productWarehouse.WarehouseAddress.Header.OH_FullName = "Product Warehouse Org";
			var viewCFSForTRW = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			AssertEquals(transitWarehouse.WW_WarehouseName, viewCFSForTRW.WarehouseName);
			AssertEquals(transitWarehouse, viewCFSForTRW.TransitWarehouse);
			AssertEquals(transitWarehouse.WarehouseAddress, viewCFSForTRW.OrgAddress);

			var viewCFSForPRW = new ViewPackagesCFSInfo(Factory, productWarehouse.WarehouseAddress.PK, controller);
			AssertEquals("Product Warehouse Org", viewCFSForPRW.WarehouseName);
			AssertNull(viewCFSForPRW.TransitWarehouse);
			AssertEquals(productWarehouse.WarehouseAddress, viewCFSForPRW.OrgAddress);
		}

		#endregion

		#region TestProperties_RelatedPackagesViaReceiveInstructions

		public void TestProperties_RelatedPackagesViaReceiveInstructions()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var differentTransitWarehouse = Helper.CreateTRWWarehouse("TR2");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var rcnInSameWarehouseButNotAssignedToParent = Helper.CreateReceiveConsignment("RCN2", "STD", transitWarehouse.PK, "EXTREF2");
			var rcnInDifferentWarehouse = Helper.CreateReceiveConsignment("RCN3", "STD", differentTransitWarehouse.PK, "EXTREF3", shipmentParent.PK, shipmentParent.TablePrefix);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, volume: 1.5m, volumeUQ: "M3", weight: 10, weightUQ: "KG");
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, volume: 200.5m, volumeUQ: "CC", weight: 500, weightUQ: "HG");
			var bookedPackageInDifferentWarehouse = Helper.CreatePackageState(rcnInDifferentWarehouse, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var bookedPackageInSameWarehouseNotRelatedToParent = Helper.CreatePackageState(rcnInSameWarehouseButNotAssignedToParent, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Receiving, cfsInfo.WarehouseStatus);
				AssertEquals("1 of 2", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("Total weight should convert weights to KG and sum up.", "50 / 60 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("Total volume should convert volumes to M3 and sum up.", "0 / 1.5 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveInstructions_WarehouseStatus_Booked()
		{
			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", parent.PK, parent.TablePrefix);
			var bookedPackage1 = Helper.CreatePackageState(rcn, 2, "PKG", "", TransitWarehouseStatuses.Codes.Booked, volume: 1, weight: 2);
			var bookedPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, volume: 2, weight: 3);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, parent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Booked, cfsInfo.WarehouseStatus);
				AssertEquals("3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("5 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("3 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveInstructions_WarehouseStatus_Receiving()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 2, "PKG", "", TransitWarehouseStatuses.Codes.Booked, weight: 2, volume: 3);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 3, volume: 4);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Receiving, cfsInfo.WarehouseStatus);
				AssertEquals("1 of 3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("3 / 5 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("4 / 7 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveInstructions_WarehouseStatus_Received()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, volume: 1m, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, rtu, Helper.CreateDispatchConsignment("DSPC1", transitWarehouse.PK));
			putaway.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, volume: 2m, volumeUQ: "M3", weight: 3, weightUQ: "KG");
			committed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			committed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchLoadList: dispatchLoadList, volume: 3m, volumeUQ: "M3", weight: 4, weightUQ: "KG");
			picked.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			picked.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, rtu, dispatchLoadList: dispatchLoadList, volume: 4m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			staged.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			staged.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Received, cfsInfo.WarehouseStatus);
				AssertEquals("5", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("14 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("10 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveInstructions_WarehouseStatus_Dispatching()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var bookedPackage = Helper.CreatePackageState(rcn, 2, "PKG", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2, volume: 3);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1, volume: 2);

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment,
				dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 2m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu,
				dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 1m, volumeUQ: "M3", weight: 1, weightUQ: "KG");
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatching, cfsInfo.WarehouseStatus);
				AssertEquals("2 of 5", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("6 / 9 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("3 / 8 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveInstructions_WarehouseStatus_Departed()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, weight: 1, volume: 2);
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, weight: 2, volume: 3);
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatched, cfsInfo.WarehouseStatus);
				AssertEquals("2", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("3 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("5 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		#endregion

		#region TestBookingConfirmed

		public void TestProperties_Receive_BookingConfirmed()
		{
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			transitWarehouse.WarehouseAddress.OA_City = "MELBOURNE";
			var city = transitWarehouse.WarehouseAddress.OA_City;
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentParent.Logs.AddNew(Events.ServiceRequested, $"|LOC={city}|TYP=Transit Receive|FAC=CFS");
			shipmentParent.Logs.AddNew(Events.BookingConfirmed, $"|LOC={city}|TYP=Transit Receive|FAC=CFS");
			shipmentParent.Logs.AddNew(Events.BookingConfirmed, $"|LOC=SYDNEY|TYP=Transit Receive|FAC=CFS");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1",
				shipmentParent.PK, shipmentParent.TablePrefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(true, cfsInfo.IsRCNBookingConfirmed);
				AssertEquals(false, cfsInfo.IsDCNBookingConfirmed);
			});
		}

		public void TestProperties_Dispatch_BookingConfirmed()
		{
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			transitWarehouse.WarehouseAddress.OA_City = "MELBOURNE";
			var city = transitWarehouse.WarehouseAddress.OA_City;
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentParent.Logs.AddNew(Events.ServiceRequested, $"|LOC={city}|TYP=Transit Dispatch|FAC=CFS");
			shipmentParent.Logs.AddNew(Events.BookingConfirmed, $"|LOC={city}|TYP=Transit Dispatch|FAC=CFS");
			shipmentParent.Logs.AddNew(Events.BookingConfirmed, $"|LOC=SYDNEY|TYP=Transit Dispatch|FAC=CFS");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(false, cfsInfo.IsRCNBookingConfirmed);
				AssertEquals(true, cfsInfo.IsDCNBookingConfirmed);
			});
		}

		#endregion

		#region TestProperties_AttachedBlindPackages

		public void TestProperties_AttachedBlindPackages()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var differentTransitWarehouse = Helper.CreateTRWWarehouse("TR2");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var rcnInSameWarehouseButNotAssignedToParent = Helper.CreateReceiveConsignment("RCN2", "STD", transitWarehouse.PK, "EXTREF2");
			var rcnInDifferentWarehouse = Helper.CreateReceiveConsignment("RCN3", "STD", differentTransitWarehouse.PK, "EXTREF3", shipmentParent.PK, shipmentParent.TablePrefix);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rtuInDifferentWarehouse = Helper.CreateReceiveTransportationUnit("RTU2", differentTransitWarehouse.PK, differentTransitWarehouse.DefaultLocation.PK);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, volume: 1.5m, volumeUQ: "M3", weight: 10, weightUQ: "KG");
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, volume: 200.5m, volumeUQ: "M3", weight: 500, weightUQ: "KG",
				entryNum: shipmentParent.JobNumber);
			var arrivedPackageInDifferentWarehouse = Helper.CreatePackageState(rcnInDifferentWarehouse, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuInDifferentWarehouse, entryNum: shipmentParent.JobNumber);
			var arrivedPackageInSameWarehouseNotRelatedToParent = Helper.CreatePackageState(rcnInSameWarehouseButNotAssignedToParent, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			AssertEquals(TransitWarehouseJobsStatus.Descriptions.Received, cfsInfo.WarehouseStatus);

			AssertEquals("1", cfsInfo.NumberOfPackagesDisplayText);
			AssertEquals("Total weight should convert weights to KG and sum up.", "500 KG", cfsInfo.TotalWeightDisplayText);
			AssertEquals("Total volume should convert volumes to M3 and sum up.", "200.5 M3", cfsInfo.TotalVolumeDisplayText);
		}

		public void TestProperties_AttachedBlindPackages_WarehouseStatus_Received()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, volume: 1m, volumeUQ: "M3", weight: 2, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, rtu, Helper.CreateDispatchConsignment("DSPC1", transitWarehouse.PK), entryNum: shipmentParent.JobNumber);
			putaway.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, volume: 2m, volumeUQ: "M3", weight: 3, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			committed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			committed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchLoadList: dispatchLoadList, volume: 3m, volumeUQ: "M3", weight: 4, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			picked.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			picked.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, rtu, dispatchLoadList: dispatchLoadList, volume: 4m, volumeUQ: "M3", weight: 5, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			staged.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			staged.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Received, cfsInfo.WarehouseStatus);
				AssertEquals("5", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("14 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("10 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_AttachedBlindPackages_WarehouseStatus_Dispatching()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: shipmentParent.JobNumber, volume: 1m, weight: 1m);

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment,
				dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 2m, volumeUQ: "M3", weight: 5, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu,
				dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 1m, volumeUQ: "M3", weight: 1, weightUQ: "KG", entryNum: shipmentParent.JobNumber);
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatching, cfsInfo.WarehouseStatus);
				AssertEquals("2 of 3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("6 / 7 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("3 / 4 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_AttachedBlindPackages_WarehouseStatus_Departed()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, shipmentParent.TablePrefix);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu,
				entryNum: shipmentParent.JobNumber);
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu,
				entryNum: shipmentParent.JobNumber);
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatched, cfsInfo.WarehouseStatus);
			AssertEquals("2", cfsInfo.NumberOfPackagesDisplayText);
		}

		#endregion

		#region TestProperties_RelatedPackagesViaDispatchInstructions

		public void TestProperties_RelatedPackagesViaDispatchInstructions_WarehouseStatus_Booked()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK, parentPK: shipmentParent.PK, parentCode: shipmentParent.TablePrefix);
			var bookedPackage1 = Helper.CreatePackageState(rcn, 2, "PKG", "", TransitWarehouseStatuses.Codes.Booked,
				dispatchConsignment: dispatchConsignment, weight: 1, volume: 2);
			var bookedPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked,
				dispatchConsignment: dispatchConsignment, weight: 2, volume: 3);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Booked, cfsInfo.WarehouseStatus);
				AssertEquals("3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("3 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("5 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaDispatchInstructions_WarehouseStatus_Receiving()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK, parentPK: shipmentParent.PK, parentCode: shipmentParent.TablePrefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 2, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment, volume: 2.1m, weight: 5.2m);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, receiveUnit: rtu, volume: 90000.5m, volumeUQ: "CC", weight: 500.5m, weightUQ: "HG");
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Receiving, cfsInfo.WarehouseStatus);
				AssertEquals("1 of 3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("50.1 / 55.3 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("0.1 / 2.2 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaDispatchInstructions_WarehouseStatus_Received()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK, parentPK: shipmentParent.PK, parentCode: shipmentParent.TablePrefix);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, receiveUnit: rtu, volume: 1m, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, rtu, dispatchConsignment);
			putaway.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, dispatchConsignment: dispatchConsignment, volume: 2m, volumeUQ: "M3", weight: 3, weightUQ: "KG");
			committed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			committed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, volume: 3m, volumeUQ: "M3", weight: 4, weightUQ: "KG");
			picked.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			picked.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, volume: 4m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			staged.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			staged.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Received, cfsInfo.WarehouseStatus);
				AssertEquals("5", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("14 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("10 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaDispatchInstructions_WarehouseStatus_Dispatching()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK, parentPK: shipmentParent.PK, parentCode: shipmentParent.TablePrefix);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dispatchConsignment, volume: 1, weight: 2);

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment,
				dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 2m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu,
				dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 1m, volumeUQ: "M3", weight: 1, weightUQ: "KG");
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatching, cfsInfo.WarehouseStatus);
				AssertEquals("2 of 3", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("6 / 8 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("3 / 4 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		public void TestProperties_RelatedPackagesViaReceiveDispatchInstructions_WarehouseStatus_Departed()
		{
			var shipmentParent = Factory.NewWithValidTestData<ForwardingShipment>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK, parentPK: shipmentParent.PK, parentCode: shipmentParent.TablePrefix);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, weight: 1, volume: 2);
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, weight: 2, volume: 3);
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);

			CombineAssertions(() =>
			{
				AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatched, cfsInfo.WarehouseStatus);
				AssertEquals("2", cfsInfo.NumberOfPackagesDisplayText);
				AssertEquals("3 KG", cfsInfo.TotalWeightDisplayText);
				AssertEquals("5 M3", cfsInfo.TotalVolumeDisplayText);
			});
		}

		#endregion
	}
}
