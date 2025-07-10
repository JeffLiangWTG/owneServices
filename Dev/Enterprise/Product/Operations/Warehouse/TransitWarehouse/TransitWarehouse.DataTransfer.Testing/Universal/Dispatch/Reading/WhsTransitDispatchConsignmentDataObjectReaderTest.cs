using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using StateSchema = Enterprise.ZArchitecture.Schema.WhsItemPackageStateSchema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitDispatchConsignmentDataObjectReaderTest : WhsTransitConsignmentDataObjectReaderTest<WhsItemDispatchConsignment, WhsTransitDispatchConsignmentDataObjectReader>
	{
		#region TestReadIntoBusinessObject_BasicCases

		public void TestReadIntoBusinessObject_BasicCases_ConsignmentID()
		{
			TestReadIntoBusinessObject_BasicCases_Core(includePackageIDs: false, includeConsignmentID: true);
		}

		public void TestReadIntoBusinessObject_BasicCases_PackageIDs()
		{
			TestReadIntoBusinessObject_BasicCases_Core(includePackageIDs: true, includeConsignmentID: false);
		}

		public void TestReadIntoBusinessObject_BasicCases_PackageIDsAndConsignmentID()
		{
			TestReadIntoBusinessObject_BasicCases_Core(includePackageIDs: true, includeConsignmentID: true);
		}

		void TestReadIntoBusinessObject_BasicCases_Core(bool includePackageIDs, bool includeConsignmentID)
		{
			var consignmentID = "CONID123";
			var packageIDs = includePackageIDs ? new[] { "PKG-1", "PKG-2", "PKG-3" } : new string[] { "" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			// Set up Dispatch Consignment Data Object
			var packageIDsToUseInDispatch = packageIDs;
			var dispatchConsignmentID = includeConsignmentID ? consignmentID : "DISPATCH123";
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(dispatchConsignmentID, packageIDsToUseInDispatch);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			// packages should be on both the receive and dispatch consignment
			var packageStateQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			packageStateQuery.AddToFilter(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var linkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, packageStateQuery);
			if (includePackageIDs)
			{
				AssertEquals("Should have linked packages to dispatch consignment.", 3, linkedPackages.Length);
			}
			else
			{
				AssertEquals("Should have linked Packages using consignment id.", 1, linkedPackages.Length);
			}
			AssertNoExceptionThrown(() => Factory.SaveForTesting()); // ensure correct data
		}

		public void TestReadIntoBusinessObject_SetExternalReference_MatchByPackageID_Overpack()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", packingParent: rcn, receiveUnit: rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DIS1", "OVP-1");

			var innerPackingLine1 = Helper.CreatePackingLine("PKG-1", "WTLLWC00000001", Constants.PkgUnit.Package);
			var innerPackingLine2 = Helper.CreatePackingLine("PKG-2", "WTLLWC00000002", Constants.PkgUnit.Package);
			var innerPackingLineCollection = new List<PackingLine> { innerPackingLine1, innerPackingLine2 };

			var outerPackingLine = Helper.CreatePackingLine("OVP-1", "WTLLWC00000003", Constants.PkgUnit.Pallet);
			outerPackingLine.SetPackingLineCollection(() => innerPackingLineCollection);

			dispatchConsignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { outerPackingLine });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertEquals("Should create 3 packages", 3, packageStates.Length);

			var package1 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "PKG-1");
			AssertNotNull(package1);
			AssertEquals("WTLLWC00000001", package1.KP_ExternalReference);

			var package2 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "PKG-2");
			AssertNotNull(package2);
			AssertEquals("WTLLWC00000002", package2.KP_ExternalReference);

			var package3 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "OVP-1");
			AssertNotNull(package3);
			AssertEquals("WTLLWC00000003", package3.KP_ExternalReference);
		}

		public void TestReadIntoBusinessObject_SetExternalReference_MatchByPackageID()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2", "PKG-3" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001", ReferenceNumber = "PKG-1" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002", ReferenceNumber = "PKG-2" };
			var packingLine3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, PackType = new PackageType { Code = "PKG" }, PackingLineID = "WTLLWC00000003", ReferenceNumber = "PKG-3" };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2, packingLine3 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123");
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertEquals("Should create 3 packages", 3, packageStates.Length);

			var package1 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "PKG-1");
			AssertNotNull(package1);
			AssertEquals("WTLLWC00000001", package1.KP_ExternalReference);

			var package2 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "PKG-2");
			AssertNotNull(package2);
			AssertEquals("WTLLWC00000002", package2.KP_ExternalReference);

			var package3 = packageStates.Select(p => p.Package).FirstOrDefault(p => p.KP_PackageID == "PKG-3");
			AssertNotNull(package3);
			AssertEquals("WTLLWC00000003", package3.KP_ExternalReference);
		}

		public void TestReadIntoBusinessObject_SetExternalReference_MatchByRCNQtyAndType()
		{
			var consignmentID = "CONID123";
			var consignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds(consignmentID, new Tuple<string, int>("PLT", 2), new Tuple<string, int>("BOX", 3));
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002" };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(consignmentID);
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertEquals("Should create 2 packages", 2, packageStates.Length);
			AssertEquals(1, packageStates.Select(p => p.Package).Count(p => p.KP_F3_NKPackType == "PLT"));
			AssertEquals(1, packageStates.Select(p => p.Package).Count(p => p.KP_F3_NKPackType == "BOX"));

			var package1 = packageStates.Select(p => p.Package).First(p => p.KP_F3_NKPackType == "PLT");
			AssertEquals(packingLine1.PackQty.Value.ToZInt(), package1.KP_PackageQty);
			AssertEquals("Should set KP_ExternalReference", packingLine1.PackingLineID.Value, package1.KP_ExternalReference);

			var package2 = packageStates.Select(p => p.Package).First(p => p.KP_F3_NKPackType == "BOX");
			AssertEquals(packingLine2.PackQty.Value.ToZInt(), package2.KP_PackageQty);
			AssertEquals("Should set KP_ExternalReference", packingLine2.PackingLineID.Value, package2.KP_ExternalReference);
		}

		public void TestReadIntoBusinessObject_MatchByExternalReference()
		{
			var consignmentID = "CONID123";
			var consignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds(consignmentID, new Tuple<string, int>("PLT", 2), new Tuple<string, int>("BOX", 3));
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var package1 = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, "PLT")).First();
			package1.KP_ExternalReference = "WTLLWC00000001";
			var package2 = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_F3_NKPackType, "BOX")).First();
			package2.KP_ExternalReference = "WTLLWC00000002";
			Factory.SaveForTesting();

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002" };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(consignmentID);
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertEquals("Should create 2 packages", 2, packageStates.Length);
		}

		public void TestReadIntoBusinessObject_UpdatesPackageStateIsHighRisk_MatchByPackageID()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			consignmentDataObject.PackingLineCollection.First().IsHighRisk = true;
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			dispatchConsignmentDataObject.PackingLineCollection.First().IsHighRisk = false;
			dispatchConsignmentDataObject.PackingLineCollection.Last().IsHighRisk = true;

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK));
			AssertEquals("Should create 2 package", 2, packageStates.Length);

			var packageState1 = packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-1");
			packageState1.Reload();
			var packageState2 = packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-2");
			packageState2.Reload();

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
			AssertNotNull(packageState1);
			AssertEquals("Should update WPS_IsHighRisk", false, packageState1.WPS_IsHighRisk);
			AssertNotNull(packageState2);
			AssertEquals("Should update WPS_IsHighRisk", true, packageState2.WPS_IsHighRisk);
		}

		public void TestReadIntoBusinessObject_SetPackageStateIsHighRisk_ForInnerPackages_MatchByPackageID()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("PKG-1", "", Constants.PkgUnit.Package, isHighRisk: true);
			var inner2 = Helper.CreatePackingLine("PKG-2", "", Constants.PkgUnit.Package, isHighRisk: true);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 3 packages attached", 3, packages.Count);
			AssertEquals("OVP-1 should be attached", true, packages.Any(p => p.PK == overpackPackage.PK));
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackageState1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackageState2.PK));
			AssertEquals("Should set WPS_IsHighRisk", true, childPackageState1.WPS_IsHighRisk);
			AssertEquals("Should set WPS_IsHighRisk", true, childPackageState2.WPS_IsHighRisk);
		}

		public void TestReadIntoBusinessObject_UpdatesPackageStateIsHighRisk_MatchByRCNQtyAndType()
		{
			var consignmentID = "CONID123";
			var consignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds(consignmentID, new Tuple<string, int>("PLT", 2), new Tuple<string, int>("BOX", 3));
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PLT" }, PackingLineID = "WTLLWC00000001", IsHighRisk = true };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "BOX" }, PackingLineID = "WTLLWC00000002", IsHighRisk = true };
			var packingLineCollection = new DataObjectList<PackingLine>() { packingLine1, packingLine2 };

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(consignmentID);
			dispatchConsignmentDataObject.SetPackingLineCollection(() => packingLineCollection);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK));
			AssertEquals("Should create 2 packages", 2, packageStates.Length);
			AssertEquals(1, packageStates.Select(p => p.Package).Count(p => p.KP_F3_NKPackType == "PLT"));
			AssertEquals(1, packageStates.Select(p => p.Package).Count(p => p.KP_F3_NKPackType == "BOX"));

			var packageState1 = packageStates.First(p => p.Package.KP_F3_NKPackType == "PLT");
			AssertEquals(packingLine1.PackQty.Value.ToZInt(), packageState1.Package.KP_PackageQty);
			AssertEquals("Should update WPS_IsHighRisk", true, packageState1.WPS_IsHighRisk);

			var packageState2 = packageStates.First(p => p.Package.KP_F3_NKPackType == "BOX");
			AssertEquals(packingLine2.PackQty.Value.ToZInt(), packageState2.Package.KP_PackageQty);
			AssertEquals("Should update WPS_IsHighRisk", true, packageState2.WPS_IsHighRisk);
		}

		#endregion

		#region TestReadIntoBusinessObject_DetachExistingPackages

		public void TestReadIntoBusinessObject_DetachExistingPackages()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2", "PKG-3" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			consignmentDataObject.DataContext.DataSourceCollection.Single(ds => ds.Type.Equals(nameof(DataContextType.ForwardingShipment))).Key = "";

			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			// packages should be on both the receive and dispatch consignment, and also attached to dispatch load list
			var packageStateQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			packageStateQuery.AddToFilter(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var linkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, packageStateQuery);
			AssertEquals("Should have linked packages to dispatch consignment.", 3, linkedPackages.Length);

			var dll1 = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK, Data.Warehouse.DefaultLocation, true);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK, Data.Warehouse.DefaultLocation, true);

			receiveConsignment.PackageStates.First(p => p.Package.KP_PackageID == "PKG-1").WPS_WDL_LoadList = dll1.PK;
			receiveConsignment.PackageStates.First(p => p.Package.KP_PackageID == "PKG-2").WPS_WDL_LoadList = dll1.PK;
			receiveConsignment.PackageStates.First(p => p.Package.KP_PackageID == "PKG-3").WPS_WDL_LoadList = dll2.PK;

			Factory.SaveForTesting();

			var dllRow = Factory.RowFactory.Load(WhsItemDispatchLoadListSchema.Constants.TableName, new ZQuery(WhsItemDispatchLoadListSchema.PK, dll2.PK));
			AssertEquals("Dispatch load list should be started.", true, (bool)dllRow.Select(d => d[WhsItemDispatchLoadListSchema.Constants.WDL_IsReadyToStage]).First());

			var dllLinkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, new ZQuery(StateSchema.WPS_WDL_LoadList, dll2.PK));
			AssertEquals("Dispatch load list should have linked package.", 1, dllLinkedPackages.Length);

			var dispatchConsignmentDataObjectWithLessPackages = Data.CreateShipmentWithPackages("DISPATCH123", new[] { "PKG-1", "PKG-2" });
			var updatedDispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObjectWithLessPackages, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have updated Dispatch Consignment.", dispatchConsignment, updatedDispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var newLinkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, packageStateQuery);
			AssertEquals("Should have removed 1 package from the dispatch consignment.", 2, newLinkedPackages.Length);

			dllRow = Factory.RowFactory.Load(WhsItemDispatchLoadListSchema.Constants.TableName, new ZQuery(WhsItemDispatchLoadListSchema.PK, dll2.PK));
			AssertEquals("Dispatch load list should be stopped.", false, (bool)dllRow.Select(d => d[WhsItemDispatchLoadListSchema.Constants.WDL_IsReadyToStage]).First());

			dllLinkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, new ZQuery(StateSchema.WPS_WDL_LoadList, dll2.PK));
			AssertEquals("Should have removed package from the dispatch load list.", 0, dllLinkedPackages.Length);

			AssertContainsExactElementsInAnyOrder("Should have linked correct packages.", new[] { "PKG-1", "PKG-2" },
				newLinkedPackages.Select(o => Factory.Load<PkgPackage>((Guid)o[StateSchema.Constants.WPS_KP_Package])).Select(p => p.KP_PackageID.ToString()));
			AssertNoExceptionThrown(() => Factory.SaveForTesting()); // ensure correct data
		}

		#endregion

		#region TestReadIntoBusinessObject_PackingLinesSplitOnReceive

		public void TestReadIntoBusinessObject_PackingLinesSplitOnReceive()
		{
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", "");
			consignmentDataObject.PackingLineCollection[0].PackQty = 5;

			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);

			var dispatchConsignmentQuery = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var linkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, dispatchConsignmentQuery);
			var receivePackageStatesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			var receivePackageStates = Factory.RowFactory.Load(StateSchema.Constants.TableName, receivePackageStatesQuery);
			AssertContainsExactElementsInAnyOrder("Should have linked packages to dispatch consignment.",
				receivePackageStates.Select(r => r[StateSchema.Constants.WPS_KP_Package]), linkedPackages.Select(d => d[StateSchema.Constants.WPS_KP_Package]));
		}

		#endregion

		#region TestReadIntoBusinessObject_WithEmptyPackageIDInLines

		public void TestReadIntoBusinessObject_WithEmptyPackageIDInLines()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("CONID123", "123", "456");
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("CONID123", "123", "456");
			dispatchConsignmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "" });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);

			var dispatchPackagesQuery = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var linkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery);
			var receivePackageStatesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			var receivePackageStates = Factory.RowFactory.Load(StateSchema.Constants.TableName, receivePackageStatesQuery);
			AssertContainsExactElementsInAnyOrder("Should have linked packages to dispatch consignment.",
				receivePackageStates.Select(r => r[StateSchema.Constants.WPS_KP_Package]), linkedPackages.Select(d => d[StateSchema.Constants.WPS_KP_Package]));
		}

		#endregion

		#region TestReadIntoBusinessObject_MultipleReceiveConsignments

		public void TestReadIntoBusinessObject_MultipleReceiveConsignments()
		{
			var receiveConsignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3");
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject1);
			var receivePackages1 = Factory.RowFactory.Load(StateSchema.Constants.TableName, new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment1.PK));

			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("CONID456", "PKG-4", "PKG-5", "PKG-6");
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);
			var receivePackages2 = Factory.RowFactory.Load(StateSchema.Constants.TableName, new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment2.PK));

			var dispatchConsignmentID = "DISPATCH123"; // Needs to be different so it does not match receives
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(dispatchConsignmentID, "PKG-1", "PKG-2", "PKG-3", "PKG-4", "PKG-5", "PKG-6");
			// Needs to be blank so it cant match on consignmentID
			dispatchConsignmentDataObject.DataContext.DataSourceCollection.Single(ds => ds.Type.Equals(nameof(DataContextType.ForwardingShipment))).Key = "";
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);

			var dispatchPackagesQuery = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var linkedPackages = Factory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery);
			AssertContainsExactElementsInAnyOrder("Should have Linked Packages from Both Receive Consignments.",
				receivePackages1.Select(r => r[StateSchema.Constants.WPS_KP_Package]).Concat(receivePackages2.Select(r => r[StateSchema.Constants.WPS_KP_Package])),
				linkedPackages.Select(d => d[StateSchema.Constants.WPS_KP_Package]));
		}

		#endregion

		#region TestReadIntoBusinessObject_UpdateExistingDispatch

		public void TestReadIntoBusinessObject_UpdateExistingDispatch()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3");
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			var dispatchPackagesQuery1 = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			AssertEquals("Should be 3 packages.", 3, Factory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery1).Length);

			// Update receive consignment first
			var consignmentDataObject2 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3", "PKG-4");
			AssertEquals("Precondition: Receive Consignment was updated.", receiveConsignment, Data.CreateReceiveConsignmentInDB(consignmentDataObject2));

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			var packageIdSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIdSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, "PKG-4");
			packageQuery.AddSubQuery(packageIdSubQuery, JoinCondition.And);

			var package = Factory.LoadTop1<PkgPackage>(packageQuery);
			var existingPackagesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			existingPackagesQuery.AddToFilter(StateSchema.WPS_KP_Package, SQLComparisonOperator.NotEqual, package.PK);
			var updatedPackagesQuery = new ZQuery(dispatchPackagesQuery1);
			updatedPackagesQuery.AddToFilter(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			AssertContainsExactElementsInAnyOrder("Precondition: Packages Re-pointed to Dispatch Consignment.",
				Factory.RowFactory.Load(StateSchema.Constants.TableName, existingPackagesQuery).Select(d => d[StateSchema.Constants.PK]),
				Factory.RowFactory.Load(StateSchema.Constants.TableName, updatedPackagesQuery).Select(d => d[StateSchema.Constants.PK]));

			var matchingDispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have updated Dispatch Consignment.", dispatchConsignment, matchingDispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
			Factory.SaveForTesting();

			var otherFactory = new UniversalObjectFactory();
			var dispatchPackagesQuery2 = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var receivePackagesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			var receivePackages = otherFactory.RowFactory.Load(StateSchema.Constants.TableName, receivePackagesQuery);
			var dispatchWave = otherFactory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery2);
			AssertContainsExactElementsInAnyOrder("Should dispatch the receive's packages.",
				receivePackages.Select(d => d[StateSchema.Constants.WPS_KP_Package]), dispatchWave.Select(d => d[StateSchema.Constants.WPS_KP_Package]));
		}

		#endregion

		#region TestReadIntoBusinessObject_UpdateExistingDispatch_DuplicatePackageIDsExist

		public void TestReadIntoBusinessObject_UpdateExistingDispatch_DuplicatePackageIDsExist()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3");
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			Data.CreateReceiveConsignmentInDB(Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3", "PKG-4"));
			Data.CreateReceiveConsignmentInDB(Data.CreateShipmentWithPackages("CONID456", "PKG-1", "PKG-2", "PKG-3"));

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			var packageIdSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIdSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, "PKG-4");
			packageQuery.AddSubQuery(packageIdSubQuery, JoinCondition.And);

			var package = Factory.LoadTop1<PkgPackage>(packageQuery);
			var existingPackagesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			existingPackagesQuery.AddToFilter(StateSchema.WPS_KP_Package, SQLComparisonOperator.NotEqual, package.PK);
			var updatedPackagesQuery = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			updatedPackagesQuery.AddToFilter(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			AssertContainsExactElementsInAnyOrder("Precondition: Packages Re-pointed to Dispatch Consignment.",
				Factory.RowFactory.Load(StateSchema.Constants.TableName, existingPackagesQuery).Select(d => d[StateSchema.Constants.PK]),
				Factory.RowFactory.Load(StateSchema.Constants.TableName, updatedPackagesQuery).Select(d => d[StateSchema.Constants.PK]));

			var consignmentDataObject2 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3", "PKG-4");
			AssertExceptionThrown<DataObjectReadFailureException>("Duplicate package ids should throw an error.",
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Package IDs matched multiple Packages with the same ID: PKG-1, PKG-2, PKG-3
These Packages may be relabeled via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_UpdateExistingDispatch_NoPackageIDs

		public void TestReadIntoBusinessObject_UpdateExistingDispatch_NoPackageIDs()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123");
			consignmentDataObject1.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PKG" } });
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			var dispatchPackagesQuery1 = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			AssertEquals("Should be 1 packages.", 1, Factory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery1).Length);

			// Update receive consignment first
			var consignmentDataObject2 = Data.CreateShipmentWithPackages("CONID123");
			consignmentDataObject2.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, PackType = new PackageType { Code = "PKG" } });
			AssertEquals("Precondition: Receive Consignment was updated.", receiveConsignment, Data.CreateReceiveConsignmentInDB(consignmentDataObject2));

			var receivePackagesQuery = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			var updatedPackagesQuery = new ZQuery(dispatchPackagesQuery1);
			updatedPackagesQuery.AddToFilter(receivePackagesQuery);
			AssertContainsExactElementsInAnyOrder("Precondition: Packages Re-pointed to Dispatch Consignment.",
				Factory.RowFactory.Load(StateSchema.Constants.TableName, receivePackagesQuery).Select(d => d[StateSchema.Constants.PK]),
				Factory.RowFactory.Load(StateSchema.Constants.TableName, updatedPackagesQuery).Select(d => d[StateSchema.Constants.PK]));

			var matchingDispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should have updated Dispatch Consignment.", dispatchConsignment, matchingDispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
			Factory.SaveForTesting();

			var otherFactory = new UniversalObjectFactory();
			var dispatchPackagesQuery2 = new ZQuery(StateSchema.WPS_WDC_TransitDispatchConsignment, dispatchConsignment.PK);
			var receivePackages = otherFactory.RowFactory.Load(StateSchema.Constants.TableName, receivePackagesQuery);
			var dispatchWave = otherFactory.RowFactory.Load(StateSchema.Constants.TableName, dispatchPackagesQuery2);
			AssertContainsExactElementsInAnyOrder("Should dispatch the receive's packages.",
				receivePackages.Select(d => d[StateSchema.Constants.WPS_KP_Package]), dispatchWave.Select(d => d[StateSchema.Constants.WPS_KP_Package]));
		}

		#endregion

		#region TestReadIntoBusinessObject_PackagesDuplicated_WithConsignmentID_ShouldReject

		public void TestReadIntoBusinessObject_PackagesDuplicated_WithConsignmentID_ShouldReject()
		{
			var receiveConsignmentDataObject1 = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			var receiveConsignmentToMatch = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject1);

			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("456", "PKG-1", "PKG-2");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);

			// should match the packages from receive consignment '123'
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			AssertExceptionThrown<DataObjectReadFailureException>("Multiple package ids exists in the system. Therefore message is rejected.",
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Package IDs matched multiple Packages with the same ID: PKG-1, PKG-2
These Packages may be relabeled via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_PackageInDifferentWarehouse

		public void TestReadIntoBusinessObject_PackageInDifferentWarehouse_NotInDCN()
		{
			var warehouse = Data.Warehouse;
			var twForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			Factory.SaveForTesting();

			var stageLocationInDifferentWhs = twForDifferentBranch.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", twForDifferentBranch.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", twForDifferentBranch.PK, stageLocationInDifferentWhs.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_PackageInDifferentWarehouse_NotInDCN_HandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var twForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			Factory.SaveForTesting();

			var stageLocationInDifferentWhs = twForDifferentBranch.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", twForDifferentBranch.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", twForDifferentBranch.PK, stageLocationInDifferentWhs.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "HU-1");

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: HU-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_PackageInDifferentWarehouse_NotInDCN_ChildPackage()
		{
			var warehouse = Data.Warehouse;
			var twForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			Factory.SaveForTesting();

			var stageLocationInDifferentWhs = twForDifferentBranch.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", twForDifferentBranch.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", twForDifferentBranch.PK, stageLocationInDifferentWhs.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA");
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA");
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1", "PKG-2");

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1, PKG-2
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch

		#region TestReadIntoBusinessObject_RejectDispatch_ExtraPackagesInPackingLineCollection

		public void TestReadIntoBusinessObject_RejectDispatch_ExtraPackagesInPackingLineCollection()
		{
			TestReadIntoBusinessObject_Dispatch_Core(
				receiveConsignmentID: "CONID123",
				receiveConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3" },
				dispatchConsignmentID: "CONID123",
				dispatchConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3", "PKG-4" },
				expectedErrorMessage: @"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-4
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.");
		}

		#endregion

		#region TestReadIntoBusinessObject_AcceptDispatch_ExtraPackagesOnReceiveConsignment

		public void TestReadIntoBusinessObject_AcceptDispatch_ExtraPackagesOnReceiveConsignment()
		{
			TestReadIntoBusinessObject_Dispatch_Core(
				receiveConsignmentID: "CONID123",
				receiveConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3", "PKG-4" },
				dispatchConsignmentID: "CONID123",
				dispatchConsignmentPackages: new[] { "PKG-1", "PKG-2" },
				expectedErrorMessage: "");
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithConsignmentID

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithConsignmentID()
		{
			TestReadIntoBusinessObject_Dispatch_Core(
				receiveConsignmentID: "CONID123",
				receiveConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3" },
				dispatchConsignmentID: "CONID123",
				dispatchConsignmentPackages: new[] { "PKG-4", "PKG-5", "PKG-6" },
				expectedErrorMessage: @"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-4, PKG-5, PKG-6
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.");
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithConsignmentID_UpdateExistingDispatchConsignmentWithPackagesAlreadyLinked

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithConsignmentID_UpdateExistingDispatchConsignmentWithPackagesAlreadyLinked()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			var consignmentDataObject2 = Data.CreateShipmentWithPackages("CONID123", "PKG-4", "PKG-5", "PKG-6");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-4, PKG-5, PKG-6
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithoutConsignmentID

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch_WithoutConsignmentID()
		{
			TestReadIntoBusinessObject_Dispatch_Core(
				receiveConsignmentID: "",
				receiveConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3" },
				dispatchConsignmentID: "",
				dispatchConsignmentPackages: new[] { "PKG-4", "PKG-5", "PKG-6" },
				expectedErrorMessage: @"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-4, PKG-5, PKG-6
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.");
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesDoNotMatch()
		{
			var warehouse = Data.Warehouse;
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-4");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-4
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_AcceptDispatch_PackagesFromMultipleConsignmentIDs

		public void TestReadIntoBusinessObject_AcceptDispatch_PackagesFromMultipleConsignmentIDs()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("Different ID", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			TestReadIntoBusinessObject_Dispatch_Core(
				receiveConsignmentID: "CONID123",
				receiveConsignmentPackages: new[] { "PKG-4", "PKG-5", "PKG-6" },
				dispatchConsignmentID: "CONID123",
				dispatchConsignmentPackages: new[] { "PKG-1", "PKG-2", "PKG-3", "PKG-4", "PKG-5", "PKG-6" },
				expectedErrorMessage: "");
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesAlreadyLinkedToADifferentDispatch

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesAlreadyLinkedToADifferentDispatch()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch Consigment.", dispatchConsignment);
			Factory.SaveForTesting();

			var consignmentDataObject2 = Data.CreateShipmentWithPackages("CONID456", "PKG-1", "PKG-2", "PKG-3");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Packages are currently assigned to a different Dispatch Consignment:
(Package ID - Dispatch Consignment ID): PKG-1 - CONID123, PKG-2 - CONID123, PKG-3 - CONID123
To dispatch these Packages on this Dispatch Consignment, first remove the Packages from the other Dispatch Consignments.
You can do this via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackagesDuplicated

		public void TestReadIntoBusinessObject_RejectDispatch_PackagesDuplicated()
		{
			var receiveConsignmentDataObject1 = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject1);

			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("456", "PKG-1", "PKG-2");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("789", "PKG-1", "PKG-2", "PKG-3");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Package IDs matched multiple Packages with the same ID: PKG-1, PKG-2
These Packages may be relabeled via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_Mixed

		public void TestReadIntoBusinessObject_RejectDispatch_Mixed()
		{
			var warehouse = Data.Warehouse;

			var receiveConsignmentDataObject1 = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject1);

			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("456", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);

			var receiveConsignmentDataObject3 = Data.CreateShipmentWithPackages("789", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject3);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1", "PKG-2", "PKG-3", "PKG-4");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Package IDs matched multiple Packages with the same ID: PKG-3
These Packages may be relabeled via Transit Warehouse.

The following Packages are currently assigned to a different Dispatch Consignment:
(Package ID - Dispatch Consignment ID): PKG-1 - 123, PKG-2 - 123
To dispatch these Packages on this Dispatch Consignment, first remove the Packages from the other Dispatch Consignments.
You can do this via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse.

Could not find the following Package IDs: PKG-4
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackageIsDeparted

		public void TestReadIntoBusinessObject_RejectDispatch_PackageIsDeparted()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			packageState.WPS_WL_LastLocation = stageLocation.PK;
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackageIsFinalized

		public void TestReadIntoBusinessObject_RejectDispatch_PackageIsFinalized()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Finalized, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			packageState.WPS_WL_LastLocation = stageLocation.PK;
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_PackageIsAdjustedOut

		public void TestReadIntoBusinessObject_RejectDispatch_PackageIsAdjustedOut()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: rtu);
			packageState.WPS_WL_LastLocation = stageLocation.PK;
			packageState.WPS_AdjustedOut = "TST";
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_UnloadInDifferentWarehouse

		public void TestReadIntoBusinessObject_RejectDispatch_UnloadInDifferentWarehouse()
		{
			var warehouse = Data.Warehouse;
			var twForDifferentBranch = Helper.CreateTRWWarehouse("TW1");
			Factory.SaveForTesting();

			var stageLocationInDifferentWhs = twForDifferentBranch.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", twForDifferentBranch.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", twForDifferentBranch.PK, stageLocationInDifferentWhs.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", twForDifferentBranch.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PKG-1
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_UnloadInDifferentWarehouse

		public void TestReadIntoBusinessObject_RejectDispatch_OverFiveMembers()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("456", "PKG-4", "PKG-5", "PKG-6");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);

			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObject2, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1", "PKG-2", "PKG-3", "PKG-4", "PKG-5", "PKG-6");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"Failed to match valid Packages in Transit Warehouse 'TWH'.

The following Packages are currently assigned to a different Dispatch Consignment:
(Package ID - Dispatch Consignment ID): PKG-1 - 123, PKG-2 - 123, PKG-3 - 123, PKG-4 - 456, PKG-5 - 456...
To dispatch these Packages on this Dispatch Consignment, first remove the Packages from the other Dispatch Consignments.
You can do this via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_NoConsignmentID

		public void TestReadIntoBusinessObject_RejectDispatch_NoConsignmentID_OnlyPackageIds()
		{
			var packages = new[] { "PKG-1", "PKG-2", "PKG-3" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("CONID123", packages);
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", packages);
			dispatchConsignmentDataObject.DataContext.DataSourceCollection.Single(ds => ds.Type.Equals(nameof(DataContextType.ForwardingShipment))).Key = "";
			AssertNoExceptionThrown("Should not throw an error since package ids are used to match to existing packages",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_RejectDispatch_NoShipmentNumberOrHouseBill()
			=> TestReadIntoBusinessObject_RejectDispatch_NoConsginmentID(DataContextType.ForwardingShipment, "Cannot Import Dispatch Consignment as no House Bill or Forwarding Shipment Number was provided.");

		public void TestReadIntoBusinessObject_RejectDispatch_NoLandTransportConsignmentNumberOrHouseBill()
			=> TestReadIntoBusinessObject_RejectDispatch_NoConsginmentID(DataContextType.LandTransportConsignment, "Cannot Import Dispatch Consignment as no House Bill or Land Transport Consignment Number was provided.");

		public void TestReadIntoBusinessObject_RejectDispatch_NoHouseBillForSeaCargoOutturn()
			=> TestReadIntoBusinessObject_RejectDispatch_NoConsginmentID(DataContextType.SeaCargoOutturn, "Cannot Import Dispatch Consignment as no House Bill was provided.");

		void TestReadIntoBusinessObject_RejectDispatch_NoConsginmentID(DataContextType dataContextType, string expectedMessage)
		{
			Data.SetupForForwardingImport();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", new[] { "" });
			Data.SetupNewDataContextWithDataSource(dispatchConsignmentDataObject, shipmentNumber: null);
			dispatchConsignmentDataObject.DataContext.AddDataSource(dataContextType, "");

			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedMessage,
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_WithReceiveConsignmentID_ConsignmentHasNoPackages_WayBillNumberIsEmpty

		public void TestReadIntoBusinessObject_RejectDispatch_WithReceiveConsignmentID_ConsignmentHasNoPackages_WayBillNumberIsEmpty()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", new string[] { "PKG1" });
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", new string[] { "PKG1", "" });
			dispatchConsignmentDataObject.WayBillNumber = "";
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Could not import Dispatch Instruction because RCN Packline Quantity and Pack Type discrepancy:
RCN            Dispatch Instruction
0 PKG          1 PKG
Ensure your Dispatch Instruction matches the RCN Quantity and Pack Type, or dispatch packages by package ID.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_StandalonePackageHasNoRCN

		public void TestReadIntoBusinessObject_RejectDispatch_StandalonePackageHasNoRCN()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreatePackageState(rtu, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "PKG-1", "PKG-2");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"The following Packages were matched but they were received blind into the Transit Warehouse and have yet to be attached to a Receive Consignment:
Package        RCN            DCN            Load List      Status
PKG-1          -              -              -              Arrived
PKG-2          -              -              -              Arrived
Packages without a Receive Consignment cannot be dispatched from a Transit Warehouse. The Receive process must be completed via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_ChildPackageHasNoRCN

		public void TestReadIntoBusinessObject_RejectDispatch_ChildPackageHasNoRCN()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var package1 = Helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "PKG-1", "PKG-2");
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"The following Packages were matched but they were received blind into the Transit Warehouse and have yet to be attached to a Receive Consignment:
Package        RCN            DCN            Load List      Status
PKG-1          -              -              -              Arrived
Packages without a Receive Consignment cannot be dispatched from a Transit Warehouse. The Receive process must be completed via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_RejectDispatch_OverpackPackageHasNoRCN

		public void TestReadIntoBusinessObject_RejectDispatch_OverpackPackageHasNoRCN()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DIS1");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", packingParent: dispatchConsignment, receiveUnit: rtu, dcn: dispatchConsignment);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dispatchConsignment);
			var childPackage2 = Helper.CreatePackageState(rtu, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived);
			childPackage2.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.DisableCheckOverpackInnersRCNDCNForTest(TestConnection);
			Factory.SaveForTesting();

			dispatchConsignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "OVP-1", PackType = new PackageType { Code = "PLT" } } });
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"The following Packages were matched but they were received blind into the Transit Warehouse and have yet to be attached to a Receive Consignment:
Package        RCN            DCN                  Load List      Status
PKG-2          -              DIS1 (DC00000001)    -              Arrived
Packages without a Receive Consignment cannot be dispatched from a Transit Warehouse. The Receive process must be completed via Transit Warehouse.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			childPackage2.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			Factory.SaveForTesting();
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_DoesNotRejectDispatch_DCNIsComplete

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_DCNIsComplete_MatchedByJobLink()
		{
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", "PKG-1");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch Consigment.", dispatchConsignment);

			dispatchConsignment.WDC_CompleteTime = ZDateTimeOffset.Now;
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_DCNIsComplete_MatchedByHouseBill()
		{
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", "PKG-1");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch Consigment.", dispatchConsignment);

			dispatchConsignment.WDC_CompleteTime = ZDateTimeOffset.Now;
			Factory.SaveForTesting();

			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_DCNIsComplete_MatchedByConsignmentID()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", "PKG-1");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch Consigment.", dispatchConsignment);
			dispatchConsignment.WDC_CompleteTime = ZDateTimeOffset.Now;

			var consignmentDataObject2 = Data.CreateShipmentWithPackages("S1000000", "PKG-2");
			Data.CreateReceiveConsignmentInDB(consignmentDataObject2);

			var dispatchConsignment2 = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject2, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch Consigment.", dispatchConsignment2);
			dispatchConsignment2.WDC_CompleteTime = ZDateTimeOffset.Now;
			Factory.SaveForTesting();

			consignmentDataObject1.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment).Key = "S1000000";

			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_RelatedRCNIsComplete()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123", packageIDs: new string[] { null });
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			receiveConsignment.WRC_CompleteTime = DateTime.Now;

			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_SubShipmentIsComplete()
		{
			var subShipment = Data.CreateShipmentWithPackages("CONID1", packageIDs: new string[] { "PKG-1" });
			var masterShipment = Data.CreateShipmentWithPackages("CONID2", packageIDs: new string[] { "PKG-2" });
			masterShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });
			masterShipment.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };

			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(subShipment);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(masterShipment);

			new WhsTransitDispatchConsignmentDataObjectReader(masterShipment, Logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var dispatchConsignments = Factory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery());
			var subShipmentDCN = dispatchConsignments.FirstOrDefault(d => d.WDC_ConsignmentID == "CONID1");
			AssertNotNull("Precondition: Subshipment should be imported.", subShipmentDCN);
			var masterShipmentDCN = dispatchConsignments.FirstOrDefault(d => d.WDC_ConsignmentID == "CONID2");
			subShipmentDCN.WDC_CompleteTime = DateTime.Now;

			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(masterShipment, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_Dispatch_Core

		void TestReadIntoBusinessObject_Dispatch_Core(string receiveConsignmentID, string[] receiveConsignmentPackages, string dispatchConsignmentID, string[] dispatchConsignmentPackages, string expectedErrorMessage = "")
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages(receiveConsignmentID, receiveConsignmentPackages);
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(dispatchConsignmentID, dispatchConsignmentPackages);
			if (string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
			}
			else
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorMessage,
					() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
			}
		}

		#endregion

		#endregion

		#region TestPopulateReferences_AssemblyMasterShipment

		public void TestPopulateReferences_AssemblyMasterShipment()
		{
			var subShipment = Data.CreateShipmentWithPackages("CONID1", packageIDs: new string[] { "PKG-1" });
			var masterShipment = Data.CreateShipmentWithPackages("CONID2", packageIDs: new string[] { "PKG-2" });
			masterShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });
			masterShipment.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(subShipment);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(masterShipment);
			var reader = new WhsTransitDispatchConsignmentDataObjectReader(masterShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var asmRefNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, reader.PK));
			var asmRefNumber = asmRefNumbers.FirstOrDefault(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
			Helper.AssertAdditionalReference(asmRefNumber, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "CONID2", countryCode: "", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);
		}

		#endregion

		#region TestReadIntoBusinessObject_HandlingUnit

		public void TestReadIntoBusinessObject_HandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "HU-1");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 2 packages attached", 2, packages.Count);
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackage1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackage2.PK));
		}

		#endregion

		#region TestReadIntoBusinessObject_Overpack

		public void TestReadIntoBusinessObject_OverPack()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("PKG-1", "", Constants.PkgUnit.Package);
			var inner2 = Helper.CreatePackingLine("PKG-2", "", Constants.PkgUnit.Package);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 3 packages attached", 3, packages.Count);
			AssertEquals("OVP-1 should be attached", true, packages.Any(p => p.PK == overpackPackage.PK));
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackage1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackage2.PK));
		}

		public void TestReadIntoBusinessObject_MultiLevelOverPack()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-2", rcn, rtu, rcn: rcn);
			var topOverpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: topOverpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: topOverpackPackage);
			Helper.PackPackageIntoHandlingUnit(topOverpackPackage, overpackPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: topOverpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 4 packages attached", 4, packages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { topOverpackPackage.PK, overpackPackage.PK, childPackage1.PK, childPackage2.PK }, packages.Select(p => p.PK));
		}

		public void TestReadIntoBusinessObject_MultiLevelOverPackWithHandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var topOverpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var overpackPackage = Helper.CreateOverpackPackage("OVP-2", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(topOverpackPackage, overpackPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, topOverpackPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "HU-1");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 4 packages attached", 4, packages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { topOverpackPackage.PK, overpackPackage.PK, childPackage1.PK, childPackage2.PK }, packages.Select(p => p.PK));
		}

		public void TestReadIntoBusinessObject_OverPack_MergedInnerPackline()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("PKG-1", "", Constants.PkgUnit.Package, packQty: 2);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 3 packages attached", 3, packages.Count);
			AssertEquals("OVP-1 should be attached", true, packages.Any(p => p.PK == overpackPackage.PK));
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackage1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackage2.PK));
		}

		public void TestReadIntoBusinessObject_OverPack_MixedWithNonTrackedItems()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreateAndAttachArrivedInners(overpackPackage, 40, "BOX");
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("PKG-1", "", Constants.PkgUnit.Package);
			var inner2 = Helper.CreatePackingLine("PKG-2", "", Constants.PkgUnit.Package);
			var inner3 = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box, packQty: 40);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 3 packages attached", 3, packages.Count);
			AssertEquals("OVP-1 should be attached", true, packages.Any(p => p.PK == overpackPackage.PK));
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackage1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackage2.PK));
		}

		public void TestReadIntoBusinessObject_OverPack_MixedWithPackline()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked);
			var packline = Helper.CreatePackageState(rcn, 18, "PKG", "", TransitWarehouseStatuses.Codes.Booked, unitType: "PKL");
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, packline, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("", "", Constants.PkgUnit.Package, packQty: 20);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Should have 4 packages attached", 4, packages.Count);
			AssertEquals("OVP-1 should be attached", true, packages.Any(p => p.PK == overpackPackage.PK));
			AssertEquals("ChildPackage1 should be attached", true, packages.Any(p => p.PK == childPackage1.PK));
			AssertEquals("ChildPackage2 should be attached", true, packages.Any(p => p.PK == childPackage2.PK));
			AssertEquals("packline should be attached", true, packages.Any(p => p.PK == packline.PK));
		}

		public void TestReadIntoBusinessObject_OverPack_PackageIDNotMatched()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("PKG-1", "", Constants.PkgUnit.Package);
			var inner2 = Helper.CreatePackingLine("PKG-5", "", Constants.PkgUnit.Package);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			AssertExceptionThrown<DataObjectReadFailureException>("Should throw an error when cannot match inner PackageID.",
@"Could not import Dispatch Instruction because cannot find matched inner Package with ID PKG-5.",
	() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_OverPack_MixedWithPacklineNotMatched()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var overpackPackage = Helper.CreateOverpackPackage("OVP-1", rcn, rtu, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked);
			var packline = Helper.CreatePackageState(rcn, 18, "PKG", "", TransitWarehouseStatuses.Codes.Booked, unitType: "PKL");
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, packline, ZDateTimeOffset.Now, "AAA", topHandlingUnit: overpackPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "OVP-1");
			var inner1 = Helper.CreatePackingLine("", "", Constants.PkgUnit.Package, packQty: 30);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			AssertExceptionThrown<DataObjectReadFailureException>("Should throw an error when cannot match inner Packline Qty.",
@"Could not import Dispatch Instruction because inner Packline Qty of Type PKG is 20 which is different from the expected Qty 30.",
	() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulatePackline

		public void TestReadIntoBusinessObject_PopulatePackline_DeleteExistingNonTrackedItem()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreateAndAttachArrivedInners(package, 40, Constants.PkgUnit.Box);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");
			var inner = Helper.CreatePackingLine("", "", Constants.PkgUnit.Pallet, packQty: 100);
			dispatchConsignmentDataObject.PackingLineCollection.Single().SetPackingLineCollection(() => new List<PackingLine> { inner });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var packages = dispatchConsignment.PackageStates;
			AssertEquals("Attached package should not be changed.", package.PK, packages.Single().PK);

			AssertEquals("Old non-tracked item should be deleted.", false, package.Package.Packages.Any(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
			AssertEquals("New non-tracked item should be created.", 100, package.Package.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet).KP_PackageQty);
		}

		#endregion

		#region TestReadIntoBusinessObject_PackageStatusIsLoaded

		public void TestReadIntoBusinessObject_PackageStatusIsLoaded_HasMatchedRCN()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var package1 = packages[0];
			var package2 = packages[1];

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;

			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageStatusIsLoaded_HasMatchedRCN_PackLineWithNoReferenceNumber()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "", "");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var package1 = packages[0];
			var package2 = packages[1];

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);

			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;

			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageFinishedLoading_WithPackageID()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V1";
			dtu2.WDH_VehicleReference = "V2";
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			Helper.FinishLoading(dtu2);
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageStatusIsDeparted_WithPackageID_HandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "HU-1");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			dtu1.WDH_VehicleReference = "V1";
			dtu2.WDH_VehicleReference = "V2";
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			Helper.FinishLoading(dtu1);
			Helper.FinishLoading(dtu2);
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is not stopped.", true, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageStatusIsLoaded_PackLineWithNoReferenceNumber()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("HSB1", "STD", warehouse.PK, "EXTREF1");
			rcn.ConsignorDocAddress.E2_OA_Address = Data.Orgs.CRAHOLSYD.MainAddress.PK;
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "S1000000");
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "S1000000");
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("HSB1", "", "");
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
			AssertEquals("Precondition - DCN is created with two packages linked from RCN.",
				2, dispatchConsignment.PackageStates.Count);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageStatusIsLoaded_ShowVehicleNumber()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackageStatusIsLoaded_DuplicatedVehicleNumber()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			dtu2.WDH_VehicleReference = "V001";
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;

			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Load list is stopped.", false, dll.WDL_IsReadyToStage);
			AssertEquals(2, dll.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.FreightLoaded, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu1.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, dll.PackageStates.Single(l => l.WPS_WDH_TransitDispatchHeader == dtu2.PK && l.WPS_WRH_TransitReceiveHeader == rtu.PK).WPS_Status);
		}

		void SetPackageState(WhsItemPackageState packageState, WhsLocation location, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, string status)
		{
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_Status = status;
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WDH_TransitDispatchHeader = dtu?.PK ?? ZGuid.Empty;
			packageState.WPS_WDL_LoadList = dll.PK;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";
		}

		#endregion

		#region TestReadIntoBusinessObject_PackagesBelong_To_LoadListMarkedAsReadyToStaged

		public void TestReadIntoBusinessObject_PackagesBelong_To_LoadListMarkedAsReadyToStaged()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			var warehouse = Data.Warehouse;
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var package1 = packages[0];
			var package2 = packages[1];

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dllMarkedAsReadyToStage = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK, stageLocation, true);
			var dllMarkedAsNotReadyToStage = Helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK, stageLocation, false);
			SetPackageState(package1, stageLocation, rtu, null, dllMarkedAsReadyToStage, TransitWarehouseStatuses.Codes.Arrived);
			SetPackageState(package2, stageLocation, rtu, null, dllMarkedAsNotReadyToStage, TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("All Load lists are stopped.", false, dllMarkedAsReadyToStage.WDL_IsReadyToStage);
			AssertEquals("All Load lists are stopped.", false, dllMarkedAsNotReadyToStage.WDL_IsReadyToStage);
			AssertEquals(1, dllMarkedAsReadyToStage.PackageStates.Count);
			AssertEquals(1, dllMarkedAsNotReadyToStage.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, dllMarkedAsReadyToStage.PackageStates.Single(l => l.PK == package1.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, dllMarkedAsNotReadyToStage.PackageStates.Single(l => l.PK == package2.PK).WPS_Status);
		}

		public void TestReadIntoBusinessObject_PackagesBelong_To_LoadListMarkedAsReadyToStaged_MultipleDispatchLoadLists()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			var warehouse = Data.Warehouse;
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var package1 = packages[0];
			var package2 = packages[1];

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dllMarkedAsReadyToStage1 = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK, stageLocation, true);
			var dllMarkedAsReadyToStage2 = Helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK, stageLocation, true);
			SetPackageState(package1, stageLocation, rtu, null, dllMarkedAsReadyToStage1, TransitWarehouseStatuses.Codes.Arrived);
			SetPackageState(package2, stageLocation, rtu, null, dllMarkedAsReadyToStage2, TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("All Load lists are stopped.", false, dllMarkedAsReadyToStage1.WDL_IsReadyToStage);
			AssertEquals("All Load lists are stopped.", false, dllMarkedAsReadyToStage2.WDL_IsReadyToStage);
			AssertEquals(1, dllMarkedAsReadyToStage1.PackageStates.Count);
			AssertEquals(1, dllMarkedAsReadyToStage2.PackageStates.Count);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, dllMarkedAsReadyToStage1.PackageStates.Single(l => l.PK == package1.PK).WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Arrived, dllMarkedAsReadyToStage2.PackageStates.Single(l => l.PK == package2.PK).WPS_Status);
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferences_Arrived() =>
			TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferencesCore(TransitWarehouseStatuses.Codes.Arrived, true);

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferences_FreightLoaded() =>
			TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferencesCore(TransitWarehouseStatuses.Codes.FreightLoaded, true);

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferences_Departed() =>
			TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferencesCore(TransitWarehouseStatuses.Codes.Departed, false);

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferences_Finalized() =>
			TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferencesCore(TransitWarehouseStatuses.Codes.Finalized, false);

		void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferencesCore(string blindPackageStatus, bool shouldUpdateCustomsStatus)
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123", "PKG-1", "PKG-2");
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			var warehouse = Data.Warehouse;
			var factory = NewUniversalObjectFactory();

			var receiveConsignmentForShipment = Data.CreateReceiveConsignmentInDB(shipmentDataObject);
			AssertEquals(0, receiveConsignmentForShipment.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForShipment.WRC_CustomsStatus);

			shipmentDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			shipmentDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				CountryOfIssue = new Country
				{
					Code = ""
				},
				Type = new EntryType
				{
					Code = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber
				},
				Number = "CEN123"
			});

			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			factory.SaveForTesting();

			var new_factory = NewUniversalObjectFactory();
			receiveConsignmentForShipment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, new_factory).ReadIntoBusinessObject();
			dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, new_factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(1, receiveConsignmentForShipment.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
				var rcnCustomsReference = receiveConsignmentForShipment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
				AssertNotNull(rcnCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReference.CE_Category);
				AssertEquals("CEN123", rcnCustomsReference.CE_EntryNum);
				AssertEquals("CUS", receiveConsignmentForShipment.WRC_CustomsStatus);
			});

			// Add blind package PKG-3
			var helper = new WhsTransitTestHelper(new_factory.BOFactory);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignmentForBlindPackage = helper.CreateReceiveConsignment("RC123", warehouse.PK);
			var dll = helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var blindPackage = blindPackageStatus == TransitWarehouseStatuses.Codes.Arrived ?
				helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "PKG", "PKG-3", blindPackageStatus, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation, dispatchConsignment: dispatchConsignment) :
				helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "PKG", "PKG-3", blindPackageStatus, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation, dispatchConsignment: dispatchConsignment, dispatchLoadList: dll, dispatchUnit: dtu);
			shipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "PKG-3", PackType = new PackageType { Code = "PKG" } });
			new_factory.SaveForTesting();

			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindPackage.WPS_CustomsStatus);

			shipmentDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				CountryOfIssue = new Country
				{
					Code = ""
				},
				Type = new EntryType
				{
					Code = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber
				},
				Number = "CRN123"
			});

			dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, new_factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			new_factory.SaveForTesting();
			receiveConsignmentForShipment.Reload();
			blindPackage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveConsignmentForShipment.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
				var rcnCustomsReference = receiveConsignmentForShipment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
				AssertNotNull(rcnCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReference.CE_Category);
				AssertEquals("CRN123", rcnCustomsReference.CE_EntryNum);
				AssertEquals("CLR", receiveConsignmentForShipment.WRC_CustomsStatus);

				AssertEquals(2, blindPackage.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
				var packageCustomsReference = blindPackage.Package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
				AssertNotNull(packageCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsReference.CE_Category);
				AssertEquals("CRN123", packageCustomsReference.CE_EntryNum);
				var expectedCustomsStatus = shouldUpdateCustomsStatus ? "CLR" : TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired;
				AssertEquals(expectedCustomsStatus, blindPackage.WPS_CustomsStatus);
			});
		}

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_CustomsReferences_WithReferenceMapping()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			var transitMapping = new TransitReferenceMappingConfiguration();
			var cenReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.T1,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber,
				Direction = ""
			};

			var crnReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.T2,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber
			};

			transitMapping.TransitReferenceMappingCollection.Add(cenReferenceMapping);
			transitMapping.TransitReferenceMappingCollection.Add(crnReferenceMapping);

			using (WarehouseDataRegistry.Instance.TransitReferenceMapping.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, transitMapping))
			{
				var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123", "PKG-1", "PKG-2");
				shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
				var warehouse = Data.Warehouse;

				var receiveConsignmentForShipment = Data.CreateReceiveConsignmentInDB(shipmentDataObject);
				AssertEquals(0, receiveConsignmentForShipment.CustomsReferenceNumbers.Count);
				AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForShipment.WRC_CustomsStatus);
				Factory.SaveForTesting();

				var newFactory1 = NewUniversalObjectFactory();
				receiveConsignmentForShipment = newFactory1.Load<WhsItemReceiveConsignment>(receiveConsignmentForShipment.PK);
				newFactory1.Load<WhsItemPackageState>(new ZQuery(StateSchema.PK, receiveConsignmentForShipment.PackageStates.Select(p => p.PK).ToArray()));

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				shipmentDataObject.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.T1
					},
					ReferenceNumber = "CEN123"
				});

				shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
				receiveConsignmentForShipment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, newFactory1).ReadIntoBusinessObject();
				var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, newFactory1).ReadIntoBusinessObject();
				AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
				newFactory1.SaveForTesting();
				receiveConsignmentForShipment.Reload();

				CombineAssertions(() =>
				{
					AssertEquals(1, receiveConsignmentForShipment.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
					var rcnCustomsReference = receiveConsignmentForShipment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber) as CusEntryNumber;
					AssertNotNull(rcnCustomsReference);
					AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReference.CE_Category);
					AssertEquals("CEN123", rcnCustomsReference.CE_EntryNum);
					AssertEquals("CUS", receiveConsignmentForShipment.WRC_CustomsStatus);
					var sourceTypeAddOnValue = rcnCustomsReference.GetAddOnValues(a => a.XV_Name == "SourceType").SingleOrDefault();
					AssertNotNull(sourceTypeAddOnValue);
					AssertEquals("T1", sourceTypeAddOnValue.XV_Data);
				});

				var newFactory2 = NewUniversalObjectFactory();

				// Add blind package PKG-3
				var helper = new WhsTransitTestHelper(newFactory2.BOFactory);
				var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
				var receiveConsignmentForBlindPackage = helper.CreateReceiveConsignment("RC123", warehouse.PK);
				var blindPackage = helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation, dispatchConsignment: dispatchConsignment);
				shipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "PKG-3", PackType = new PackageType { Code = "PKG" } });
				newFactory2.SaveForTesting();

				AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindPackage.WPS_CustomsStatus);

				shipmentDataObject.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.T2
					},
					ReferenceNumber = "CRN123"
				});

				dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, newFactory2).ReadIntoBusinessObject();
				AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
				newFactory2.SaveForTesting();
				receiveConsignmentForShipment = newFactory2.Load<WhsItemReceiveConsignment>(receiveConsignmentForShipment.PK);
				blindPackage.Reload();

				CombineAssertions(() =>
				{
					AssertEquals(2, receiveConsignmentForShipment.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
					var rcnCustomsReference = receiveConsignmentForShipment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber) as CusEntryNumber;
					AssertNotNull(rcnCustomsReference);
					AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReference.CE_Category);
					AssertEquals("CRN123", rcnCustomsReference.CE_EntryNum);
					var sourceTypeAddOnValue = rcnCustomsReference.GetAddOnValues(a => a.XV_Name == "SourceType").SingleOrDefault();
					AssertNotNull(sourceTypeAddOnValue);
					AssertEquals("T2", sourceTypeAddOnValue.XV_Data);

					AssertEquals(2, blindPackage.Package.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
					var packageCustomsReference = blindPackage.Package.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber) as CusEntryNumber;
					AssertNotNull(packageCustomsReference);
					AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, packageCustomsReference.CE_Category);
					AssertEquals("CRN123", packageCustomsReference.CE_EntryNum);
					AssertEquals("CLR", blindPackage.WPS_CustomsStatus);
					var pkgSourceTypeAddOnValue = packageCustomsReference.GetAddOnValues(a => a.XV_Name == "SourceType").SingleOrDefault();
					AssertNotNull(pkgSourceTypeAddOnValue);
					AssertEquals("T2", pkgSourceTypeAddOnValue.XV_Data);
				});
			}
		}

		public void TestReadIntoBusinessObject_PopulatesGoverningReferencesOnReceiveConsignmentAndPackageStates_PortReferences()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123", "PKG-1", "PKG-2");
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			var warehouse = Data.Warehouse;
			var factory = NewUniversalObjectFactory();

			var receiveConsignmentForShipment = Data.CreateReceiveConsignmentInDB(shipmentDataObject);
			AssertEquals(0, receiveConsignmentForShipment.CustomsReferenceNumbers.Count);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForShipment.WRC_CustomsStatus);

			shipmentDataObject.SetPortReferenceCollection(() => new List<PortReference>());
			shipmentDataObject.PortReferenceCollection.Add(new PortReference()
			{
				Country = new Country
				{
					Code = ""
				},
				Type = new PortReferenceType
				{
					Code = TransitWarehousePortReferenceTypes.Codes.PortAuthority
				},
				Reference = "PAN123",
				Status = new PortReferenceStatus
				{
					Code = ""
				}
			});

			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			factory.SaveForTesting();
			receiveConsignmentForShipment.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(1, receiveConsignmentForShipment.PortReferences.Cast<CusEntryNumber>().Distinct().Count());
				var rcnCustomsReference = receiveConsignmentForShipment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
				AssertNotNull(rcnCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnCustomsReference.CE_Category);
				AssertEquals("PAN123", rcnCustomsReference.CE_EntryNum);
				AssertEquals("PAN", receiveConsignmentForShipment.WRC_CustomsStatus);
			});

			// Add blind package PKG-3
			var new_factory = NewUniversalObjectFactory();
			var helper = new WhsTransitTestHelper(new_factory.BOFactory);
			dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, new_factory).ReadIntoBusinessObject();
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignmentForBlindPackage = helper.CreateReceiveConsignment("RC123", warehouse.PK);
			var blindPackage = helper.CreatePackageState(receiveConsignmentForBlindPackage, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation, dispatchConsignment: dispatchConsignment);
			shipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "PKG-3", PackType = new PackageType { Code = "PKG" } });
			new_factory.SaveForTesting();

			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindPackage.WPS_CustomsStatus);

			shipmentDataObject.PortReferenceCollection.Add(new PortReference()
			{
				Country = new Country
				{
					Code = ""
				},
				Type = new PortReferenceType
				{
					Code = TransitWarehousePortReferenceTypes.Codes.PortAuthority
				},
				Reference = "PAN123",
				Status = new PortReferenceStatus
				{
					Code = "CLR"
				}
			});

			dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, new_factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			new_factory.SaveForTesting();
			receiveConsignmentForShipment.Reload();
			blindPackage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(1, receiveConsignmentForShipment.PortReferences.Cast<CusEntryNumber>().Distinct().Count());
				var rcnCustomsReference = receiveConsignmentForShipment.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
				AssertNotNull(rcnCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, rcnCustomsReference.CE_Category);
				AssertEquals("PAN123", rcnCustomsReference.CE_EntryNum);
				AssertEquals("CLR", receiveConsignmentForShipment.WRC_CustomsStatus);

				AssertEquals(1, blindPackage.Package.PortReferences.Cast<CusEntryNumber>().Distinct().Count());
				var packageCustomsReference = blindPackage.Package.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortAuthority);
				AssertNotNull(packageCustomsReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, packageCustomsReference.CE_Category);
				AssertEquals("PAN123", packageCustomsReference.CE_EntryNum);
				AssertEquals("CLR", blindPackage.WPS_CustomsStatus);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulatesValidAdditionalReferencesOnDispatchConsignment

		public void TestReadIntoBusinessObject_PopulatesValidAdditionalReferencesOnDispatchConsignment()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			var dispatchConsignmentForShipment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Factory.SaveForTesting();
			AssertEquals(0, dispatchConsignmentForShipment.AdditionalReferenceNumbers.Count);

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber
					},
					ReferenceNumber = "Invoice No"
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = "UNK"
					},
					ReferenceNumber = "Unknown Type"
				}
			});

			var dispatchConsignmentAfterReading = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignmentAfterReading);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var dcnAdditionalReferences = dispatchConsignmentAfterReading.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToList();
				var dcnAdditionalReference = dcnAdditionalReferences.Single(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber);
				AssertNotNull(dcnAdditionalReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.AdditionalReference, dcnAdditionalReference.CE_Category);
				AssertEquals("Invoice No", dcnAdditionalReference.CE_EntryNum);
			});

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber
					},
					ReferenceNumber = "Invoice No Updated"
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.OrderNumber
					},
					ReferenceNumber = "Order No"
				}
			});

			var newFactory = new UniversalObjectFactory();
			var dispatchConsignmentAfterReimport = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, newFactory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignmentAfterReimport);
			newFactory.SaveForTesting();

			CombineAssertions(() =>
			{
				var dcnAdditionalReferencesAfterReimport = dispatchConsignmentAfterReimport.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToList();
				var dcnValidAdditionalReference1 = dcnAdditionalReferencesAfterReimport.Single(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber);
				AssertNotNull(dcnValidAdditionalReference1);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.AdditionalReference, dcnValidAdditionalReference1.CE_Category);
				AssertEquals("Invoice No Updated", dcnValidAdditionalReference1.CE_EntryNum);

				var dcnValidAdditionalReference2 = dcnAdditionalReferencesAfterReimport.Single(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.OrderNumber);
				AssertNotNull(dcnValidAdditionalReference2);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.AdditionalReference, dcnValidAdditionalReference2.CE_Category);
				AssertEquals("Order No", dcnValidAdditionalReference2.CE_EntryNum);
			});
		}

		public void TestReadIntoBusinessObject_PopulatesMasterBillNumber_HasDTW()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			var dispatchConsignmentForShipment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Factory.SaveForTesting();
			AssertEquals(0, dispatchConsignmentForShipment.AdditionalReferenceNumbers.Count);

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						 Code = WarehouseAdditionalReferenceTypes.Codes.MasterBill
					},
					ReferenceNumber = "MAB0001"
				}
			});

			var dispatchConsignmentAfterReading = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignmentAfterReading);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var dcnAdditionalReferences = dispatchConsignmentAfterReading.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToList();
				var mabAdditionalReference = dcnAdditionalReferences.FirstOrDefault(r => r.CE_EntryType == TransportAdditionalReferenceTypes.Codes.MasterBill);

				AssertNotNull(mabAdditionalReference);
				AssertEquals(TransitWarehouseReferenceCategories.Codes.AdditionalReference, mabAdditionalReference.CE_Category);
				AssertEquals("MAB0001", dispatchConsignmentAfterReading.MasterBillNumber);
			});
		}

		public void TestReadIntoBusinessObject_PopulatesMasterBillNumber_NoDTW()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			shipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.Warehouse_INTHEMSYD);
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			var warehouse = Data.WarehouseINTHEMSYD;

			var dispatchConsignmentForShipment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Factory.SaveForTesting();
			AssertEquals(0, dispatchConsignmentForShipment.AdditionalReferenceNumbers.Count);

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						 Code = WarehouseAdditionalReferenceTypes.Codes.MasterBill
					},
					ReferenceNumber = "MAB0001"
				}
			});

			var dispatchConsignmentAfterReading = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignmentAfterReading);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var dcnAdditionalReferences = dispatchConsignmentAfterReading.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToList();
				var mabAdditionalReference = dcnAdditionalReferences.FirstOrDefault(r => r.CE_EntryType == TransportAdditionalReferenceTypes.Codes.MasterBill);

				AssertNull(mabAdditionalReference);
			});
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulatesValidAdditionalReferencesOnDispatchConsignment

		public void TestReadIntoBusinessObject_PopulatesTransportMode()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.Air };

			Factory.SaveForTesting();

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
			AssertEquals("Should have populated Transport Mode.", TransportModes.Air, dispatchConsignment.WDC_TransportMode);
		}

		#endregion

		#region TestReadIntoBusinessObject_DelinkLoadedPackagesFromDCN

		public void TestReadIntoBusinessObject_DelinkLoadedPackagesFromDCN_FLOPackages()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2", "PKG-3" };

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package3 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[2], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package3, stageLocation, rtu, null, dll, TransitWarehouseStatuses.Codes.Arrived);
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var delinkedConsignemntDataObject = Data.CreateShipmentWithPackages("DISPATCH123");
			new WhsTransitDispatchConsignmentDataObjectReader(delinkedConsignemntDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should detached from DCN.", ZGuid.Empty, package1.WPS_WDC_TransitDispatchConsignment);
			AssertEquals("Should detached from DLL.", ZGuid.Empty, package1.WPS_WDL_LoadList);
			AssertEquals("Should not detached from DTU.", dtu1.PK, package1.WPS_WDH_TransitDispatchHeader);
			AssertEquals("Flag shall be true.", true, package1.WPS_RemoveFromDTU);

			AssertEquals("Should detached from DCN.", ZGuid.Empty, package2.WPS_WDC_TransitDispatchConsignment);
			AssertEquals("Should detached from DLL.", ZGuid.Empty, package2.WPS_WDL_LoadList);
			AssertEquals("Should not detached from DTU.", dtu1.PK, package2.WPS_WDH_TransitDispatchHeader);
			AssertEquals("Flag shall be true.", true, package2.WPS_RemoveFromDTU);

			AssertEquals("Should detached from DCN.", ZGuid.Empty, package3.WPS_WDC_TransitDispatchConsignment);
		}

		public void TestReadIntoBusinessObject_DelinkLoadedPackagesFromDCN_DEPOrFinPackages()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2", "PKG-3" };

			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package3 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[2], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.Departed);
			SetPackageState(package2, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.Finalized);

			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var delinkedConsignemntDataObject = Data.CreateShipmentWithPackages("DISPATCH123", new string[] { "PKG-1", "PKG-3" });
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"At least one package is departed already. Therefore cannot be removed from dispatch consignment.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(delinkedConsignemntDataObject, Logger, Factory).ReadIntoBusinessObject());

			delinkedConsignemntDataObject = Data.CreateShipmentWithPackages("DISPATCH123", new string[] { "PKG-2", "PKG-3" });
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"At least one package is departed already. Therefore cannot be removed from dispatch consignment.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(delinkedConsignemntDataObject, Logger, Factory).ReadIntoBusinessObject());

			delinkedConsignemntDataObject = Data.CreateShipmentWithPackages("DISPATCH123", new string[] { "PKG-1", "PKG-2" });
			new WhsTransitDispatchConsignmentDataObjectReader(delinkedConsignemntDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Should detached from DCN.", ZGuid.Empty, package3.WPS_WDC_TransitDispatchConsignment);
		}

		#endregion

		#region TestReadIntoBusinessObject_AllowPartialLoading

		public void TestReadIntoBusinessObject_AllowPartialLoading_NewDCN()
		{
			var warehouse = Data.Warehouse;
			warehouse.WW_AllowPartialLoadingDefault = true;

			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should populate AllowPartialLoading for DCN if warehouse enabled AllowPartialLoadingDefault", true, dispatchConsignment.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
		}

		public void TestReadIntoBusinessObject_AllowPartialLoading_ExistingDCN()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			var query = new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			AssertEquals("Precondition:", 2, packages.Length);

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should not populate AllowPartialLoading for DCN if warehouse enabled AllowPartialLoadingDefault", false, dispatchConsignment.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var warehouse = Data.Warehouse;
			warehouse.WW_AllowPartialLoadingDefault = true;
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			var dispatchConsignment2 = reader.ReadIntoBusinessObject();
			AssertNotNull("Should have updated Dispatch Consignment.", dispatchConsignment2);
			AssertEquals("Should populate AllowPartialLoading for DCN if warehouse enabled AllowPartialLoadingDefault", true, dispatchConsignment2.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
		}

		public void TestReadIntoBusinessObject_AllowPartialLoading_PackagesInSameDLL()
		{
			var warehouse = Data.Warehouse;
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should not populate AllowPartialLoading for DCN if warehouse enabled AllowPartialLoadingDefault", false, dispatchConsignment.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			warehouse.WW_AllowPartialLoadingDefault = true;

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll, TransitWarehouseStatuses.Codes.Departed);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			var dispatchConsignment2 = reader.ReadIntoBusinessObject();
			AssertNotNull("Should have updated Dispatch Consignment.", dispatchConsignment2);
			AssertEquals("Should populate AllowPartialLoading for DCN if packages are in same load list but warehouse enabled AllowPartialLoadingDefault",
				true, dispatchConsignment2.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
		}

		public void TestReadIntoBusinessObject_AllowPartialLoading_ExistingDCN_PackagesInMultipleDLLs()
		{
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[0], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", packageIDs[1], TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", packageIDs);
			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Should have created Dispatch Consignment.", dispatchConsignment);
			AssertEquals("Should not populate AllowPartialLoading for DCN if warehouse enabled AllowPartialLoadingDefault", false, dispatchConsignment.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);

			var dll = Helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dtu1.WDH_VehicleReference = "V001";
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK);
			SetPackageState(package1, stageLocation, rtu, dtu1, dll, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetPackageState(package2, stageLocation, rtu, dtu2, dll2, TransitWarehouseStatuses.Codes.Departed);
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var reader = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory);
			var dispatchConsignment2 = reader.ReadIntoBusinessObject();
			AssertNotNull("Should have updated Dispatch Consignment.", dispatchConsignment2);
			AssertEquals("Should populate AllowPartialLoading for DCN if packages are in different load lists", true, dispatchConsignment2.WDC_AllowPartialLoading);
			AssertEquals("Should have no errors.", false, Logger.HasErrors);
		}

		#endregion

		#region TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType

		public void TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType_AddNote()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNA", "Unknown Type A", "AU"));

			var dispatchConsignmentAfterReading = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignmentAfterReading);
			Factory.SaveForTesting();

			var notes = dispatchConsignmentAfterReading.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should only contain unrecognised additional reference type notes.", 1, notes.Count());
			var expectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU";
			Helper.AssertNoteContents(notes.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, expectedMessage, StmNoteDescription.Int, false);
		}

		public void TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType_UpdateNote()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNA", "Unknown Type A", "AU"),
				("UNB", "Unknown Type B", "AU"));

			new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNB", "Unknown Type B", "AU"),
				("UNC", "Unknown Type C", "AU"));

			var updatedDispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var updatedNote = updatedDispatchConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should add new unrecognised additional reference type notes.", 1, updatedNote.Count());
			var updatedExpectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU

Type: UNB
Number: Unknown Type B
Country: AU

Type: UNC
Number: Unknown Type C
Country: AU";
			Helper.AssertNoteContents(updatedNote.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, updatedExpectedMessage, StmNoteDescription.Int, false);

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNB", "Unknown Type B Updated", "AU"),
				("UNC", "Unknown Type C", "AU"),
				(WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HouseBill", "AU"),
				("UND", "Unknown Type D", "AU"));

			var doubleUpdatedDispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var doubleUpdatedNotes = doubleUpdatedDispatchConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should contain updated unrecognised additional reference type.", 1, doubleUpdatedNotes.Count());
			var doubleUpdatedExpectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU

Type: UNB
Number: Unknown Type B
Country: AU

Type: UNC
Number: Unknown Type C
Country: AU

Type: UNB
Number: Unknown Type B Updated
Country: AU

Type: UND
Number: Unknown Type D
Country: AU";
			var doubleUpdatedNote = doubleUpdatedDispatchConsignment.Notes.GetAllNotes().Cast<StmNote>();
			Helper.AssertNoteContents(doubleUpdatedNote.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, doubleUpdatedExpectedMessage, StmNoteDescription.Int, false);
		}

		#endregion

		#region TestSetDCNDirection

		public void TestDCNDirectionRule_OutboundPortOfDischargeIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(outboundPOD: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(outboundPOD: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
		}

		public void TestDCNDirectionRule_OutboundPortOfDischargeIsForeignPortPrecedence()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(ForeignPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(ForeignPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
		}

		public void TestDCNDirectionRule_InboundPortOfLoadingIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(inboundPOL: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(inboundPOL: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
		}

		public void TestDCNDirectionRule_InboundPortOfLoadingIsForeignPortPrecedence()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
		}

		public void TestDCNDirectionRule_InboundPortOfLoadingIsDomesticPortAndDestinationIsDomesticPort()
		{
			var dcnSetDirection = GenerateConsolDCNWithDirection(inboundPOL: DomesticPort, destinationPort: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
			dcnSetDirection = GenerateShipmentDCNWithDirection(inboundPOL: DomesticPort, destinationPort: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
		}

		public void TestDCNDirectionRule_OutboundPortOfDischargeIsDomesticPortAndOriginIsDomesticPort()
		{
			var dcnSetDirection = GenerateConsolDCNWithDirection(outboundPOD: DomesticPort, originPort: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
			dcnSetDirection = GenerateShipmentDCNWithDirection(outboundPOD: DomesticPort, originPort: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
		}

		public void TestDCNDirectionRule_NoOutboundLegAndShipmentPlannedLoadIsDomesticPortAndShipmentPlannedDischargeIsDomesticPort()
		{
			var dcnSetDirection = GenerateConsolDCNWithDirection(shippingPL: DomesticPort, shippingPD: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
			dcnSetDirection = GenerateShipmentDCNWithDirection(shippingPL: DomesticPort, shippingPD: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "DOM");
		}

		public void TestDCNDirectionRule_NoOutboundLegAndShipmentPlannedLoadIsDomesticPortAndShipmentPlannedDischargeIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(shippingPL: DomesticPort, shippingPD: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(shippingPL: DomesticPort, shippingPD: ForeignPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "EXP");
		}

		public void TestDCNDirectionRule_NoInboundLegAndShipmentPlannedLoadIsForeignPortAndShipmentPlannedDischargeIsDomesticPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var dcnSetDirection = GenerateConsolDCNWithDirection(shippingPL: ForeignPort, shippingPD: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
			dcnSetDirection = GenerateShipmentDCNWithDirection(shippingPL: ForeignPort, shippingPD: DomesticPort);
			AssertEquals(dcnSetDirection.WDC_Direction, "IMP");
		}

		public void TestDCNDirectionRule_NoMatchingConditionsReturnsNull()
		{
			var dcnSetDirection = GenerateConsolDCNWithDirection();
			AssertEquals(dcnSetDirection.WDC_Direction, ZString.Empty);
			dcnSetDirection = GenerateShipmentDCNWithDirection();
			AssertEquals(dcnSetDirection.WDC_Direction, ZString.Empty);
		}

		public WhsItemDispatchConsignment GenerateConsolDCNWithDirection(ZString? inboundPOL = null, ZString? outboundPOD = null, ZString? destinationPort = null, ZString? originPort = null, ZString? shippingPL = null, ZString? shippingPD = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var consol = Data.HeaderDataObject;
			TransportLeg inboundRoutingLeg = null;
			TransportLeg outboundRoutingLeg = null;

			if (inboundPOL is not null)
			{
				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: inboundPOL, portOfDischarge: homePort);
			}

			if (outboundPOD is not null)
			{
				outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: outboundPOD);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			Helper.AddTestDataToShipment(consol, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);
			Factory.SaveForTesting();

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();

			return consignment;
		}

		public WhsItemDispatchConsignment GenerateShipmentDCNWithDirection(ZString? inboundPOL = null, ZString? outboundPOD = null, ZString? destinationPort = null, ZString? originPort = null, ZString? shippingPL = null, ZString? shippingPD = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var legs = new DataObjectList<TransportLeg>();
			TransportLeg inboundRoutingLeg = null;
			TransportLeg outboundRoutingLeg = null;

			if (inboundPOL is not null)
			{
				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: inboundPOL, portOfDischarge: homePort);
				legs.Add(inboundRoutingLeg);
			}

			if (outboundPOD is not null)
			{
				outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: outboundPOD);
				legs.Add(outboundRoutingLeg);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			var shipment = Data.CreateShipmentWithPackages("SHIP123");

			Helper.AddTestDataToShipment(shipment, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			Factory.SaveForTesting();

			var consignment = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();

			return consignment;
		}

		public void TestDCNDirectionRule_ConsolIsImport_ShipmentIsImport() => GenerateConsolAndShipmentDCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, "IMP");
		public void TestDCNDirectionRule_ConsolIsImport_ShipmentIsExport() => GenerateConsolAndShipmentDCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, ForeignPort, DomesticPort, ForeignPort, "IMP");
		public void TestDCNDirectionRule_ConsolIsImport_ShipmentIsDomestic() => GenerateConsolAndShipmentDCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, null, DomesticPort, DomesticPort, "IMP");
		public void TestDCNDirectionRule_ConsolIsImport_ShipmentIsBlank() => GenerateConsolAndShipmentDCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, null, null, null, "IMP");

		public void TestDCNDirectionRule_ConsolIsExport_ShipmentIsImport() => GenerateConsolAndShipmentDCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, ForeignPort, null, ForeignPort, DomesticPort, "EXP");
		public void TestDCNDirectionRule_ConsolIsExport_ShipmentIsExport() => GenerateConsolAndShipmentDCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, ForeignPort, "EXP");
		public void TestDCNDirectionRule_ConsolIsExport_ShipmentIsDomestic() => GenerateConsolAndShipmentDCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, null, DomesticPort, DomesticPort, "EXP");
		public void TestDCNDirectionRule_ConsolIsExport_ShipmentIsBlank() => GenerateConsolAndShipmentDCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, null, null, null, "EXP");

		public void TestDCNDirectionRule_ConsolIsDomestic_ShipmentIsImport() => GenerateConsolAndShipmentDCNWithDirection(null, null, DomesticPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, "DOM");
		public void TestDCNDirectionRule_ConsolIsDomestic_ShipmentIsExport() => GenerateConsolAndShipmentDCNWithDirection(null, null, DomesticPort, DomesticPort, null, ForeignPort, DomesticPort, ForeignPort, "DOM");
		public void TestDCNDirectionRule_ConsolIsDomestic_ShipmentIsDomestic() => GenerateConsolAndShipmentDCNWithDirection(null, null, DomesticPort, DomesticPort, null, null, DomesticPort, DomesticPort, "DOM");
		public void TestDCNDirectionRule_ConsolIsDomestic_ShipmentIsBlank() => GenerateConsolAndShipmentDCNWithDirection(null, null, DomesticPort, DomesticPort, null, null, null, null, "DOM");

		public void TestDCNDirectionRule_ConsolIsBlank_ShipmentIsImport() => GenerateConsolAndShipmentDCNWithDirection(null, null, null, null, ForeignPort, null, ForeignPort, DomesticPort, "");
		public void TestDCNDirectionRule_ConsolIsBlank_ShipmentIsExport() => GenerateConsolAndShipmentDCNWithDirection(null, null, null, null, null, ForeignPort, DomesticPort, ForeignPort, "");
		public void TestDCNDirectionRule_ConsolIsBlank_ShipmentIsDomestic() => GenerateConsolAndShipmentDCNWithDirection(null, null, null, null, null, null, DomesticPort, DomesticPort, "");
		public void TestDCNDirectionRule_ConsolIsBlank_ShipmentIsBlank() => GenerateConsolAndShipmentDCNWithDirection(null, null, null, null, null, null, null, null, "");

		public void GenerateConsolAndShipmentDCNWithDirection(ZString? consolInboundLeg = null, ZString? consolOutboundLeg = null, ZString? consolLoadingPort = null, ZString? consolDischargePort = null, ZString? shipmentInboundLeg = null, ZString? shipmentOutboundLeg = null, ZString? shipmentLoadingPort = null, ZString? shipmentDischargePort = null, ZString? dcnDirection = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var consol = Data.HeaderDataObject;
			TransportLeg consolInboundRoutingLeg = null;
			TransportLeg consolOutboundRoutingLeg = null;

			if (consolInboundLeg is not null)
			{
				consolInboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: consolInboundLeg, portOfDischarge: homePort);
			}

			if (consolOutboundLeg is not null)
			{
				consolOutboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: consolOutboundLeg);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			Helper.AddTestDataToShipment(consol, inboundLeg: consolInboundRoutingLeg, outboundLeg: consolOutboundRoutingLeg, portOfLoading: consolLoadingPort, portOfDischarge: consolDischargePort, estimatedPickup: estimatedPickupDate);

			TransportLeg shipmentInboundRoutingLeg = null;
			TransportLeg shipmentOutboundRoutingLeg = null;

			if (shipmentInboundLeg is not null)
			{
				shipmentInboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: shipmentInboundLeg, portOfDischarge: homePort);
			}

			if (shipmentOutboundLeg is not null)
			{
				shipmentOutboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: shipmentOutboundLeg);
			}

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, inboundLeg: shipmentInboundRoutingLeg, outboundLeg: shipmentOutboundRoutingLeg, portOfLoading: shipmentLoadingPort, portOfDischarge: shipmentDischargePort, estimatedPickup: estimatedPickupDate);
			Factory.SaveForTesting();

			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);
			Factory.SaveForTesting();

			var dcn = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(dcnDirection, dcn.WDC_Direction);
		}

		#endregion

		#region TestLogs

		#region TestLogsForFindConsignmentID

		public void TestLogsForFindConsignmentID_GivenForwardingShipment()
		{
			string rcnExpectedCusEntryNumberLogs = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...";
			TestLogsForFindConsignmentID(DataContextType.ForwardingShipment, rcnExpectedCusEntryNumberLogs);
		}

		public void TestLogsForFindConsignmentID_GivenLandTransportConsignment()
		{
			string rcnExpectedCusEntryNumberLogs = @"";
			TestLogsForFindConsignmentID(DataContextType.LandTransportConsignment, rcnExpectedCusEntryNumberLogs);
		}

		public void TestLogsForFindConsignmentID_GivenSeaCargoOutturn()
		{
			string rcnExpectedCusEntryNumberLogs = @"";
			TestLogsForFindConsignmentID(DataContextType.SeaCargoOutturn, rcnExpectedCusEntryNumberLogs);
		}

		public void TestLogsForFindConsignmentID(DataContextType dataContextType, string rcnExpectedCusEntryNumberLogs)
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("SHIP123", "");
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObject, shipmentNumber: null);
			receiveConsignmentDataObject.DataContext.AddDataSource(dataContextType, "S1000000");

			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("SHIP123", "");
			Data.SetupNewDataContextWithDataSource(dispatchConsignmentDataObject, shipmentNumber: null);
			dispatchConsignmentDataObject.DataContext.AddDataSource(dataContextType, "S1000000");

			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(
string.Format(@"Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is '{0} - S1000000'.
Information - Searching for Dispatch Consignment for '{0} - S1000000'.
Information - No matching Dispatch Consignment found, creating new Dispatch Consignment.
Information - Populating Dispatch Consignment...{1}
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment for '{0} - S1000000'.
Information - Found matching Receive Consignment 'RC00000001 - SHIP123'.
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment for '{0} - S1000000'.
Information - Found matching Receive Consignment 'RC00000001 - SHIP123'.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment SHIP123:
UXML Package    Package        RCN
1 PKG           1 PKG          SHIP123 (RC00000001)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneePickupDeliveryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'DeliveryLocalCartage':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Added Dispatch Consignment DC00000001 from UniversalShipment.", dataContextType.ToString(), rcnExpectedCusEntryNumberLogs), Logger.Logs);
		}

		public void TestLogMessage_GivenDataTarget()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("Waybill123", new string[] { "PKG1", "PKG2" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			var dcn = Helper.CreateDispatchConsignment("Waybill123", Data.Warehouse.PK, jobID: "DC0000001");
			rcn.WRC_ConsignmentID = "Waybill123";

			Factory.SaveForTesting();

			// Remove package IDs to fallback to RCN matching
			foreach (var packline in receiveConsignmentDataObject.PackingLineCollection)
			{
				packline.ReferenceNumber = null;
			}

			Logger.TopLevelDataObject = receiveConsignmentDataObject;
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObject, shipmentNumber: null);
			receiveConsignmentDataObject.DataContext.AddDataTarget(DataContextType.TransitDispatch, dcn.WDC_JobID);

			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(@"Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Target is 'TransitDispatch - DC0000001'.
Information - Successfully loaded matching Dispatch Consignment DC0000001.
Information - Populating Dispatch Consignment DC0000001...
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment.
Information - Found matching Receive Consignment 'RC00000001 - Waybill123'.
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment.
Information - Found matching Receive Consignment 'RC00000001 - Waybill123'.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment Waybill123:
UXML Package    Package        RCN
2 PKG           PKG1           Waybill123 (RC00000001)
-               PKG2           Waybill123 (RC00000001)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneePickupDeliveryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'DeliveryLocalCartage':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Updated Dispatch Consignment DC0000001 from UniversalShipment.", Logger.Logs);
		}

		public void TestLogMessage_GivenMultiTypePackagesWithoutID()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithCombinedPackages("Waybill123", ("PKG", 1, "PKG1"), ("PKG", 1, "PKG2"), ("PKG", 3, ""), ("BOX", 1, "BOX1"), ("BOX", 5, ""));
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			var dcn = Helper.CreateDispatchConsignment("Waybill123", Data.Warehouse.PK, jobID: "DC0000001");
			rcn.WRC_ConsignmentID = "Waybill123";

			Factory.SaveForTesting();

			// Remove package IDs to fallback to RCN matching
			var receiveConsignmentDataObjectNew = Data.CreateShipmentWithCombinedPackages("Waybill123", ("PKG", 5, ""), ("BOX", 6, ""));

			Logger.TopLevelDataObject = receiveConsignmentDataObjectNew;
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObjectNew, shipmentNumber: null);
			receiveConsignmentDataObjectNew.DataContext.AddDataTarget(DataContextType.TransitDispatch, dcn.WDC_JobID);

			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObjectNew, Logger, Factory).ReadIntoBusinessObject();

			var expectedMessage =
@"Information - The following packages have been attached to Dispatch Consignment Waybill123:
UXML Package    Package        RCN
5 PKG           3 PKG          Waybill123 (RC00000001)
-               PKG1           Waybill123 (RC00000001)
-               PKG2           Waybill123 (RC00000001)
6 BOX           5 BOX          Waybill123 (RC00000001)
-               BOX1           Waybill123 (RC00000001)";
			AssertContains("Log should contain a combined type table", expectedMessage, Logger.Logs);
		}

		public void TestLogsForConsignmentMatching_GivenEmptyDataSource()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("Waybill123", new string[] { "PKG1" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			var dcn = Helper.CreateDispatchConsignment("Waybill123", Data.Warehouse.PK, jobID: "DC0000001");

			Factory.SaveForTesting();

			Logger.TopLevelDataObject = receiveConsignmentDataObject;
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObject, shipmentNumber: null);

			new WhsTransitDispatchConsignmentDataObjectReader(receiveConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Information - Searching for Dispatch Consignment.", Logger.Logs);
		}

		#region TestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages

		public void TestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingCountFailedAndThrowException_Count(BuildTestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_ReceiveConsignmentHasMorePackages()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 2), new Tuple<string, int>("CTN", 3), new Tuple<string, int>("PLT", 5));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 2), new Tuple<string, int>("CTN", 3));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
3 CTN          3 CTN
2 PKG          2 PKG
5 PLT          0 PLT";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region TestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages

		public void TestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages);
			}
		}

		public void TestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingCountFailedAndThrowException_Count(BuildTestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages);
			}
		}

		public void TestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_DispatchInstructionsHaveMorePackages()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 250), new Tuple<string, int>("CTN", 3));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 200), new Tuple<string, int>("CTN", 3), new Tuple<string, int>("PLT", 5));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
3 CTN          3 CTN
250 PKG        200 PKG
0 PLT          5 PLT";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesButDifferentQtys

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingCountFailedAndThrowException_Count(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackageTypesButDifferentQtys()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 2), new Tuple<string, int>("PLT", 5));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 4), new Tuple<string, int>("PLT", 6));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
2 PKG          4 PKG
5 PLT          6 PLT";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingFailedButTotalCountMatchedAndContinueToImport_Count(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveSameNumberOfPackagesTotalCountButDifferentInTypes()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 2), new Tuple<string, int>("PLT", 5));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 4), new Tuple<string, int>("BOX", 3));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
2 PKG          4 PKG
5 PLT          0 PLT
0 BOX          3 BOX";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingFailedButTotalCountMatchedAndContinueToImport_Count(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes);
			}
		}

		public void TestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_ReceiveAndDispatchInstructionsHaveDifferentTypes()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PLT", 2), new Tuple<string, int>("CTN", 3));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PKG", 2), new Tuple<string, int>("DRM", 3));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
3 CTN          0 CTN
2 PLT          0 PLT
0 DRM          3 DRM
0 PKG          2 PKG";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region TestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys

		public void TestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys_WithMatchTypeTypical()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "TYP"))
			{
				AssertMatchingFailedAndThrowExpection_Typical(BuildTestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys);
			}
		}

		public void TestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys_WithMatchTypeCount()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "COU"))
			{
				AssertMatchingCountFailedAndThrowException_Count(BuildTestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys);
			}
		}

		public void TestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys_WithMatchTypeHBL()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				AssertMatchingFailedButContinueToImport_HBL(BuildTestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys);
			}
		}

		(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage) BuildTestLogsForFindConsignmentID_DuplicatePackTypes_UnMatchingQtys()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PLT", 2), new Tuple<string, int>("PLT", 3));
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackagesWithoutIds("SHIP123", new Tuple<string, int>("PLT", 3), new Tuple<string, int>("PLT", 4));

			var expectedMatchingTableMessage =
@"RCN            Dispatch Instruction
5 PLT          7 PLT";
			return (dispatchConsignmentDataObject, expectedMatchingTableMessage);
		}

		#endregion

		#region DCNLogAssertions

		void AssertMatchingFailedAndThrowExpection_Typical(Func<(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage)> createTestCase)
		{
			var (dispatchConsignmentDataObject, expectedMatchingTableMessage) = createTestCase();

			AssertExceptionThrown<DataObjectReadFailureException>("Expect an error message since receive consignment instructions have more packages to match",
$@"Could not import Dispatch Instruction because RCN Packline Quantity and Pack Type discrepancy:
{expectedMatchingTableMessage}
Ensure your Dispatch Instruction matches the RCN Quantity and Pack Type, or dispatch packages by package ID.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		void AssertMatchingFailedButTotalCountMatchedAndContinueToImport_Count(Func<(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage)> createTestCase)
		{
			var (dispatchConsignmentDataObject, expectedMatchingTableMessage) = createTestCase();
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			var log = Logger.Logs;
			AssertContains(
$@"Warning - RCN Packline Quantity and Pack Type discrepancy found:
{expectedMatchingTableMessage}
Current Package Matching Type is Count and the Packline's total count matches, continue Dispatch Instruction import.", log);
		}

		void AssertMatchingCountFailedAndThrowException_Count(Func<(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage)> createTestCase)
		{
			var (dispatchConsignmentDataObject, expectedMatchingTableMessage) = createTestCase();

			AssertExceptionThrown<DataObjectReadFailureException>("Expect an error message since receive consignment instructions have more packages to match",
$@"RCN Packline Quantity and Pack Type discrepancy found:
{expectedMatchingTableMessage}
Current Package Matching Type is Count but the Packline's total count does not match, could not import Dispatch Instruction.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		void AssertMatchingFailedButContinueToImport_HBL(Func<(UniversalShipment DispatchConsignmentDataObject, string ExpectedMatchingTableMessage)> createTestCase)
		{
			var (dispatchConsignmentDataObject, expectedMatchingTableMessage) = createTestCase();
			var dcn = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			var log = Logger.Logs;
			AssertContains(
$@"Warning - RCN Packline Quantity and Pack Type discrepancy found:
{expectedMatchingTableMessage}
Current Package Matching Type is HBL, continue Dispatch Instruction import.", log);
		}

		#endregion

		public void TestLogForFindConsignmentID_CannotFindRCNID()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("SHIP123", "");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DCN123", "");
			AssertExceptionThrown<DataObjectReadFailureException>("PackLines found without Ids however matching receive consignment could not be found.", () => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestLogsForMatchOnPackages_OnPackLines

		public void TestLogsForMatchOnPackages_OnPackLines()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1", "PKG-2", "PKG-3");
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Some Imported Packlines have Package IDs. Attempting to match by Package IDs.", Logger.Logs);
		}

		public void TestLogsForMatchOnPackages_OnPackLines_WithoutIDs()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S10000000", "");
			dispatchConsignmentDataObject.WayBillNumber = "";

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"No Receive Consignment could be found matching Shipment S10000000 or House Bill. Send the Receive Instructions then send the Dispatch Instructions again.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			AssertContains("Populating Dispatch Consignment...", Logger.Logs);
		}

		public void TestLogsForMatchOnPackages_OnPackLines_WithoutIDs_HasHouseBillNum()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "");
			dispatchConsignmentDataObject.WayBillNumber = "456";

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"No Receive Consignment could be found matching Shipment S1000000 or House Bill 456. Send the Receive Instructions then send the Dispatch Instructions again.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());

			AssertContains("Populating Dispatch Consignment...", Logger.Logs);
		}

		public void TestLogsForMatchOnPackages_OnPackLines_WithoutIDs_HasHouseBillNum_ContinueImportIfRegistryIsOn()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			using (WarehouseDataRegistry.Instance.IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, "HBL"))
			{
				var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
				Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

				var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "");
				dispatchConsignmentDataObject.WayBillNumber = "456";
				AssertNoExceptionThrown("Should not throw error if no RCN matched.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject());
				var log = Logger.Logs;
				AssertContains(@"Warning - No Receive Consignment could be found matching Shipment S1000000 or House Bill 456.", Logger.Logs);
			}
		}

		#endregion

		#region TestLogsForMatchOnPackages_OnConsignmentIDOnly

		public void TestLogsForMatchOnPackages_OnConsignmentIDOnly()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("123", "PKG-1", "PKG-2", "PKG-3");
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Some Imported Packlines have Package IDs. Attempting to match by Package IDs.", Logger.Logs);
		}

		#endregion

		#region TestLogForGetPackageStatesFromPackingLineCollection_LinkedPackage

		public void TestLogForGetPackageStatesFromPackingLineCollection_LinkedPackage()
		{
			var warehouse = Data.Warehouse;

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			dispatchConsignmentDataObject.WayBillNumber = "";
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			var expectedMessage =
@"Information - The following packages have been attached to Dispatch Consignment S1000000:
Package        RCN
PKG-1          RCN1 (EXTREF1)";
			AssertContains(expectedMessage, Logger.Logs);
		}

		public void TestLogForGetPackageStatesFromPackingLineCollection_LinkedPackage_HandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", "PKG-1", "PKG-2");
			dispatchConsignmentDataObject.WayBillNumber = "";
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			var expectedMessage =
@"Information - The following packages have been attached to Dispatch Consignment S1000000:
Package        RCN
PKG-1          RCN1 (EXTREF1)
PKG-2          RCN1 (EXTREF1)";
			AssertContains(expectedMessage, Logger.Logs);
		}

		#endregion

		#region TestLogForGetPackageStatesFromPackingLineCollection_DetachedPackage

		public void TestLogForGetPackageStatesFromPackingLineCollection_DetachedPackage()
		{
			var warehouse = Data.Warehouse;

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageStateToBeDelinked = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1", "PKG-2");
			var dcn = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(2, dcn.PackageStates.Count);
			var expectedMessage =
@"Information - The following packages have been attached to Dispatch Consignment DISPATCH123:
Package        RCN
PKG-1          RCN1 (EXTREF1)
PKG-2          RCN1 (EXTREF1)";
			AssertContains(expectedMessage, Logger.Logs);
			Factory.SaveForTesting();
			dcn.Reload();
			packageState.Reload();
			packageStateToBeDelinked.Reload();

			Logger.ClearLogs();
			var dispatchConsignmentDataObject2 = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject2, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("PKG-2 should be delinked", 1, dcn.PackageStates.Count);

			var expectedMessage2 =
@"Information - The following packages have been attached to Dispatch Consignment DISPATCH123:
Package        RCN
PKG-1          RCN1 (EXTREF1)";

			AssertContains(expectedMessage2, Logger.Logs);

			var expectedMessage3 =
@"Information - The following packages have been detached from their Dispatch Consignments and their Load Lists:
Package        RCN               DCN            Load List      Status
PKG-2          RCN1 (EXTREF1)    -              -              Arrived";
			AssertContains(expectedMessage3, Logger.Logs);

			Factory.SaveForTesting();

			var eventLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, packageStateToBeDelinked.WPS_KP_Package).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DetachedCode));
			AssertNotNull("Detached package should have a detach event.", eventLog);
			AssertContains("Package PKG-2 has been detached from dispatch consignment DISPATCH123.", eventLog.SL_Reference);
		}

		#endregion

		#region TestLogForDCNLinkedToShipment

		public void TestLogForDCNLinkedToShipment()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject = Data.CreateShipmentWithPackages("SHIP123", "PKG-1", "PKG-2", "PKG-3");
			consignmentDataObject.WayBillNumber = "HSB123";
			Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIP123";
			Factory.SaveForTesting();

			var outboundSessionTracker = new Mock<IDataWritingManager>();
			Logger.OutboundSessionTracker = outboundSessionTracker.Object;

			new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Dispatch Consignment HSB123 linked to Shipment SHIP123.", Logger.Logs);
		}

		#endregion

		#endregion

		#region TestImportNotes

		public void TestPopulateBizO_ImportNoteInXML()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			// Set up Dispatch Consignment Data Object
			var shipmentDO = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");
			shipmentDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			Logger.TopLevelDataObject = shipmentDO;

			var createdConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var notes = createdConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Should import all note visibility types.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note", StmNoteDescription.Int, true);

			shipmentDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note UPDATED", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			createdConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			notes = createdConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Notes should have been updated.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note UPDATED", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, true);
		}

		#endregion

		#region TestReadIntoBusinessObject_AdditionalServices

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_OneService_ImportTwice()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);

			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoDifferentServices_ImportTwice()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");

			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoSameServices_ImportTwice_WithServiceID()
		{
			TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoSameServices_ImportTwiceCore(true);
		}
		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoSameServices_ImportTwice_WithoutServiceID()
		{
			TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoSameServices_ImportTwiceCore(false);
		}

		void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_TwoSameServices_ImportTwiceCore(bool hasServiceID)
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, serviceID: hasServiceID ? "SRV001" : ""));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, serviceID: hasServiceID ? "SRV002" : ""));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			if (hasServiceID)
			{
				var result = SendUxmlThenGetJobServices(shipmentDO, 2);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN", "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[0].ES_ServiceId);
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[1].ES_ServiceId);
				AssertContainsExactElementsInAnyOrder(new[] { "SRV001", "SRV002" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

				result = SendUxmlThenGetJobServices(shipmentDO, 2);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN", "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[0].ES_ServiceId);
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[1].ES_ServiceId);
				AssertContainsExactElementsInAnyOrder(new[] { "SRV001", "SRV002" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			}
			else
			{
				var result = SendUxmlThenGetJobServices(shipmentDO, 1);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ServiceId));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

				result = SendUxmlThenGetJobServices(shipmentDO, 1);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ServiceId));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			}
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_OneService_ImportTwice_AddDifferentServiceBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_OneService_ImportTwice_AddSameServiceBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_OneService_ImportTwice_ClearServiceCollectionBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_ImportTwice_RemovePreviousServiceAndAddNewOnesBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("STE", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "WSH", "STE" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=WSH", "|FAC=CFS|LOC=Johannesburg|TYP=STE");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_ServiceIsCompleted_ImportTwice()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var completeDate = bookedDate;
			var newCompleteDate = new ZDateTime(2021, 12, 17, 0, 0, 0);

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completeDate));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completeDate);
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completeDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: newCompleteDate));

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertContainsExactElementsInAnyOrder(new[] { completeDate }, result.jobServices.Select(t => t.ES_Completed));
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completeDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_ServiceIsCompleted_ImportTwice_ClearServiceCollectionBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var completeDate = new DateTime(2021, 12, 16, 0, 0, 0);

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completeDate));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completeDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completeDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_ImportTwice_ChangeIntendedWarehouseLocationToAnotherBeforeReimport()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Consignor_CRAHOLSYD));

			SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoExistingRCNImportedFromShipment_ServiceIsMissingWarehouseLocation()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_RCNImportedViaShipment_NotMatchRCN_ImportTwice()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S1000001");

			shipmentDO.DataContext = dataContext;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_DeleteExistingBookedServices()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 2);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "CLN"), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "FUM"), "FUM", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=FUM");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();

			result = SendUxmlThenGetJobServices(shipmentDO, 0);
			var oldSrvLogs = result.parent.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode);
			AssertEquals("Should still have 2 Service Requested events after re-importing.", 2, oldSrvLogs.Count());
			Assert("Old Service Requested Events should have been cancelled.", oldSrvLogs.All(l => l.SL_IsCancelled));
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ServiceEventsAddedToParentDCN()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var newBookedDate = new ZDateTime(2021, 12, 16, 0, 0, 0);
			var completeDate = new ZDateTime(2021, 12, 16, 0, 0, 0);

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completeDate));

			// Events not raised for services with location not matching to the warehouse
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("QIN"));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 3);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "CLN"), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "FUM"), "FUM", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "TAI"), "TAI", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completeDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=FUM", "|FAC=CFS|LOC=Johannesburg|TYP=TAI");
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completeDate, "|FAC=CFS|LOC=Johannesburg|TYP=TAI");

			// Update services
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB, bookedTime: newBookedDate));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB, bookedTime: newBookedDate, completedTime: completeDate));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", location: Data.Orgs.Warehouse_WUFSHIJNB, bookedTime: newBookedDate, completedTime: completeDate));

			result = SendUxmlThenGetJobServices(shipmentDO, 3);
			var svrLogs = result.parent.Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == Events.ServiceRequestedCode);
			AssertEquals("Parent should have 3 Service Requested Events.", 3, svrLogs.Count());
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=CLN")), Events.ServiceRequestedCode, newBookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=FUM")), Events.ServiceRequestedCode, newBookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=FUM");
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=TAI")), Events.ServiceRequestedCode, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=TAI");

			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completeDate, "|FAC=CFS|LOC=Johannesburg|TYP=FUM", "|FAC=CFS|LOC=Johannesburg|TYP=TAI");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoBookedTimeAndCompletedTime_NoEventsAdded()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 3);
			AssertEquals("No service events added to parent.", 0, result.parent.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode || l.SL_SE_NKEvent == Events.ServiceCompletedCode).Count());
		}

		public void TestReadIntoBusinessObject_AdditionalServices_PopulateAddOnValueWithJobNumber()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			var clnService = result.jobServices.Single();
			Helper.AssertAdditionalService(clnService, "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			var addOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnService.PK)).Single();
			Helper.AssertAddOnValue(addOnValue, clnService.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("OVP", location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: DateTime.Now));

			result = SendUxmlThenGetJobServices(shipmentDO, 3);
			var clnServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "CLN");
			var fumServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "FUM");
			var ovpServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "OVP");
			var clnAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnServiceAfterReading.PK)).Single();
			var fumAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, fumServiceAfterReading.PK)).Single();
			var ovpAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, ovpServiceAfterReading.PK)).Single();
			Helper.AssertAddOnValue(clnAddOnValue, clnServiceAfterReading.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");
			Helper.AssertAddOnValue(fumAddOnValue, fumServiceAfterReading.PK, fumServiceAfterReading.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");
			Helper.AssertAddOnValue(ovpAddOnValue, ovpServiceAfterReading.PK, ovpServiceAfterReading.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_PopulateAddOnValueWithShipmentNumber_DifferentSenderIDWillNotUpdateExistingService()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipmentDO = Data.CreateShipmentWithPackages("S1000000", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.DataContext.DataSourceCollection.First().Key = "JobNumber123";
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			var clnService = result.jobServices.Single();
			Helper.AssertAdditionalService(clnService, "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			var addOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnService.PK)).Single();
			Helper.AssertAddOnValue(addOnValue, clnService.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			// Change the sender key so it doesn't modify the existing CLN service
			shipmentDO.DataContext.DataSourceCollection.First().Key = "New_JobNumber";
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			var clnServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "CLN");
			var fumServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "FUM");
			var clnAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnServiceAfterReading.PK)).Single();
			var fumAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, fumServiceAfterReading.PK)).Single();
			Helper.AssertAddOnValue(clnAddOnValue, clnServiceAfterReading.PK, clnService.TablePrefix, "STR", "JobNumber123", "JobNumber");
			Helper.AssertAddOnValue(fumAddOnValue, fumServiceAfterReading.PK, clnService.TablePrefix, "STR", "New_JobNumber", "JobNumber");
		}

		(IEnumerable<WhsJobService> jobServices, IStmALogParent parent) SendUxmlThenGetJobServices(UniversalShipment shipmentDO, int expectedCount)
		{
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var dispatchConsignment = newFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			var jobServices = dispatchConsignment.Services.Cast<WhsJobService>();
			AssertEquals($"Service Collection Count should equal {expectedCount}.", expectedCount, jobServices.Count());

			return (jobServices, dispatchConsignment);
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulateIsAuthorizedForDispatch

		public void TestReadIntoBusinessObject_PopulateIsAuthorizedForDispatch_SendTWDThenTWP()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var shipmentDO = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			Assert("WDC_IsAuthorizedForDispatch defaults to True", dispatchConsignment.WDC_IsAuthorizedForDispatch);

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWP } } });
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Prepare 'DC00000001 - DISPATCH123' for Dispatch because Dispatch has already been authorized.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_PopulateIsAuthorizedForDispatch_SendTWPThenTWD()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var shipmentDO = Data.CreateShipmentWithPackages("DISPATCH123", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWP } } });

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			Assert("WDC_IsAuthorizedForDispatch is False", !dispatchConsignment.WDC_IsAuthorizedForDispatch);

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			Assert("WDC_IsAuthorizedForDispatch is False", !dispatchConsignment.WDC_IsAuthorizedForDispatch);

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			Assert("WDC_IsAuthorizedForDispatch is True", dispatchConsignment.WDC_IsAuthorizedForDispatch);

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWP } } });
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Prepare 'DC00000001 - DISPATCH123' for Dispatch because Dispatch has already been authorized.",
				() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			Assert("WDC_IsAuthorizedForDispatch is True", dispatchConsignment.WDC_IsAuthorizedForDispatch);
		}

		#endregion

		#region TestReadIntoBusinessObject_ContainsRadioactivePackages

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGSubstanceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgCode1 = hasLimitedDG ? "0004a" : "1009b";
			var dgCode2 = hasLimitedDG ? "0014a" : "1010b";

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, dgCode1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, dgCode2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "0014a";
				packLineUndg2.Standard = "IMO";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Substance threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Substance 0004a, 0014a goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGClassDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgClass1 = hasLimitedDG ? "1" : "2";
			var dgClass2 = hasLimitedDG ? "Comb" : "3";

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "", dgClass1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "", dgClass2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				packLineUndg1.IMOClass = "1";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "1993d";
				packLineUndg2.Standard = "CFR";
				packLineUndg2.IMOClass = "Comb";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Class threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Class 1, Comb goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGCountryReferenceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgCode1 = hasLimitedDG ? "0004a" : "1009b";
			var dgCode2 = hasLimitedDG ? "0014a" : "1010b";

			var dcr1 = Helper.CreateCountryReference("1234");
			var dcr2 = Helper.CreateCountryReference("5678");

			var dcp1 = Helper.CreateCountryReferencePivot(dcr1, dgCode1);
			var dcp2 = Helper.CreateCountryReferencePivot(dcr2, dgCode2);

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "", "", dcr1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "", "", dcr2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "0014a";
				packLineUndg2.Standard = "IMO";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Country Reference threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Country Reference 1234, 5678 goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestConsolImportFailedWithDuplicatedHouseBillNumber

		public void TestConsolImportFailedWithDuplicatedHouseBillNumber()
		{
			Data.SetupForForwardingImport();

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			var shipment2 = Data.CreateShipmentWithPackages("A123", "Pack3", "Pack4");
			var shipment3 = Data.CreateShipmentWithPackages("B456", "Pack5", "Pack6");
			var shipment4 = Data.CreateShipmentWithPackages("B456", "Pack7", "Pack8");
			var shipment5 = Data.CreateShipmentWithPackages("ABCD", "Pack9", "Pack10");

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment1, shipment2, shipment3, shipment4, shipment5 });

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"Duplicate Waybill Numbers found in the Dispatch Instruction: A123, B456.",
				() => new WhsTransitDispatchConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestConsolImportFailedWithEmptyHouseBillNumber()
		{
			Data.SetupForForwardingImport();

			var warehouse = Data.WarehouseINTHEMSYD;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			Helper.CreatePackageState(rcn, 1, "PKG", "Pack1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "Pack2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "Pack3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, "PKG", "Pack4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			var shipment1 = Data.CreateShipmentWithWayBillAndPackages("S00001", "A123", "Pack1");
			var shipment2 = Data.CreateShipmentWithWayBillAndPackages("S00002", "B456", "Pack2");
			var shipment3 = Data.CreateShipmentWithWayBillAndPackages("S00003", "", "Pack3");
			var shipment4 = Data.CreateShipmentWithWayBillAndPackages("S00004", "", "Pack4");

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment1, shipment2, shipment3, shipment4 });

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestReadIntoBusinessObject_PopulateAllJobDocAddresses

		public void TestReadIntoBusinessObject_PopulateAllJobDocAddresses()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("", "PKG-1");
			Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			var shipmentDO = Data.CreateShipmentWithPackages("DISPATCH1", "PKG-1");
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			AssertNoExceptionThrown(() => new WhsTransitDispatchConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			var dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();

			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, dispatchConsignment.PK));
			AssertEquals("Should have Booking party, Consignor, Consignee, Delivery & Transport Company Doc Addresses attached.", 5, addresses.Length);
			var cneAddress = addresses.Single(address => address.DocAddressType == DocAddressType.ConsigneeDocumentaryAddress);
			var cnrAddress = addresses.Single(address => address.DocAddressType == DocAddressType.LocalCartageExporter);
			var cegAddress = addresses.Single(address => address.DocAddressType == DocAddressType.ConsigneePickupDeliveryAddress);
			var traAddress = addresses.Single(address => address.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress);
			AssertJobDocAddressContentMatches_CRAHOLSYD(cnrAddress);
			AssertJobDocAddressContentMatches_INTHEMSYD(cneAddress);
			AssertJobDocAddressContentMatches_CRAHOLSYD(cegAddress);
			AssertJobDocAddressContentMatches_INTHEMSYD(traAddress);
		}

		#endregion

		#region TestPopulatePackageStateSecurityStatus

		public void TestPopulatePackageStateSecurityStatus()
		{
			Data.Warehouse.WW_TransitSecurityProcessingRequired = true;
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");

			var location = Data.Warehouse.DefaultLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var newFactory = new UniversalObjectFactory();
			var packageState = newFactory.LoadTop1<WhsItemPackageState>(new ZQuery(StateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK));
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_Status = "ARV";
			packageState.WPS_ReceivedAs = "PKL";
			packageState.WPS_UnloadedTime = ZDateTimeOffset.Now;
			packageState.WPS_UnloadedNotYetProcessedTime = ZDateTimeOffset.Now;

			Helper.CreatePackageScreening(packageState.Package, "XRY", passed: true);
			Factory.SaveForTesting();
			newFactory.SaveForTesting();
			// Set up Dispatch Consignment Data Object
			var packageIDsToUseInDispatch = packageIDs;
			var dispatchConsignmentID = consignmentID;
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(dispatchConsignmentID, packageIDsToUseInDispatch);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;
			dispatchConsignmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			Logger.TopLevelDataObject = dispatchConsignmentDataObject;
			new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, newFactory).ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			newFactory = new UniversalObjectFactory();
			packageState = newFactory.Load<WhsItemPackageState>(packageState.PK);
			AssertEquals("Security Status is SCR", TransitWarehouseSecurityStatuses.Codes.Screened, packageState.WPS_SecurityStatus);
		}

		#endregion

		#region TestReadIntoBusinessObject_PackingLineCollectionIsNull

		public void TestReadIntoBusinessObject_PackingLineCollectionIsNull()
		{
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("CONID123");
			consignmentDataObject1.SetPackingLineCollection(() => null);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(consignmentDataObject1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			var dispatchQuery = new ZQuery(WhsItemDispatchConsignmentSchema.PK, dispatchConsignment.PK);
			AssertEquals("Should be 1 packages.", 1, Factory.RowFactory.Load(WhsItemDispatchConsignmentSchema.Constants.TableName, dispatchQuery).Length);
		}

		#endregion

		#region TestOrderReferences

		public void TestOrderReferencesDispatchConsignment()
		{
			var shipment = Data.CreateShipmentWithPackages("CONID123");
			shipment.SetPackingLineCollection(() => null);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(shipment);
			var orderNumberCollection = new DataObjectList<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 }, new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.SetOrderNumberCollection(() => orderNumberCollection);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			Factory.SaveForTesting();

			dispatchConsignment = Factory.Load<WhsItemDispatchConsignment>(new ZQuery()).SingleOrDefault();
			AssertEquals("Dispatch Consignment should have 2 Order References", 2, dispatchConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder("Order References Should Be Applied To Dispatch Consignment",
				dispatchConsignment.OrderReferences.Select(ord => ord.WOR_OrderReference), orderNumberCollection.Select(ord => ord.OrderReference));
		}

		#endregion

		#region TestShipperReferences

		public void TestShippersReferenceDispatchConsignment()
		{
			var shipment = Data.CreateShipmentWithPackages("CONID123");
			shipment.BookingConfirmationReference = "ShippersReference";
			shipment.SetPackingLineCollection(() => null);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(shipment);

			var dispatchConsignment = new WhsTransitDispatchConsignmentDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created dispatch consignment.", dispatchConsignment);
			AssertEquals("Shippers Reference Should Be Created", dispatchConsignment.WDC_ShippersReference, shipment.BookingConfirmationReference);
			Factory.SaveForTesting();
		}

		#endregion

		#region Implementation

		protected override int AdditionalReferencesRows { get => 3; }

		protected override int InitialReferencesRowCount { get => 2; }

		protected override void AdditionalSetupForImport(UniversalShipment consignmentDataObject)
		{
			var rcn = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var count = rcn.PackageStates.Count;
			Helper.CreatePackageState(rcn, 1, "PKG", "ID" + count, TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			if (consignmentDataObject.PackingLineCollection.Count == 0)
			{
				consignmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "ID" + count, PackType = new PackageType { Code = "PKG" } });
			}
			else if (consignmentDataObject.PackingLineCollection.Count != rcn.PackageStates.Count)
			{
				consignmentDataObject.PackingLineCollection.Clear();
				foreach (var package in rcn.PackageStates)
				{
					consignmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = ZLong.Parse(package.Package.KP_PackageQty.ToString()), ReferenceNumber = package.Package.KP_PackageID, PackType = new PackageType { Code = package.Package.KP_F3_NKPackType } });
				}
			}
			consignmentDataObject.GoodsDescription = "shipment desc";
		}

		protected override WhsTransitDispatchConsignmentDataObjectReader GetDataObjectReader(UniversalObjectFactory factory, UniversalShipment shipmentDataObject)
		{
			return new WhsTransitDispatchConsignmentDataObjectReader(shipmentDataObject, Logger, factory);
		}

		protected override SchemaStringColumn ConsignmentIDColumn => WhsItemDispatchConsignmentSchema.WDC_ConsignmentID;
		protected override SchemaStringColumn HouseBillNumberColumn => WhsItemDispatchConsignmentSchema.WDC_HouseBillNumber;
		protected override SchemaDateTimeColumn CreateTimeColumn => WhsItemDispatchConsignmentSchema.WDC_SystemCreateTimeUtc;
		protected override SchemaStringColumn ServiceLevelColumn => WhsItemDispatchConsignmentSchema.WDC_RS_NKServiceLevel;
		protected override SchemaGuidColumn WarehouseColumn => WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse;
		protected override SchemaStringColumn DestinationColumn => WhsItemDispatchConsignmentSchema.WDC_RL_NKDestination;
		protected override SchemaGuidColumn PKColumn => WhsItemDispatchConsignmentSchema.PK;
		protected override SchemaStringColumn JobIDColumn => WhsItemDispatchConsignmentSchema.WDC_JobID;
		protected override SchemaDateTimeOffsetColumn CompleteTimeColumn => WhsItemDispatchConsignmentSchema.WDC_CompleteTime;
		protected override ZString JobIDPrefix => "DC";

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);
		readonly ZDateTime bookedDate = new ZDateTime(2021, 12, 15, 0, 0, 0);

		string HomePort => Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
		string ForeignPort => "USLAX";
		string DomesticPort
		{
			get
			{
				return $"{HomePort.Substring(0, 2)}TST";
			}
		}

		#endregion
	}
}
