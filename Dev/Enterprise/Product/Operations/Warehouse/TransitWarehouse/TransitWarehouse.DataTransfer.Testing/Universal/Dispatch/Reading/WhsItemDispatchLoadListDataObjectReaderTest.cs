using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForDLL;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	class WhsItemDispatchLoadListDataObjectReaderTest : TransitUniversalTestCase
	{
		#region TestPopulateBizO_CreateNewHeader

		public void TestPopulateBusinessObject()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var loadListWithPackageStates = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadListWithPackageStates.WDL_WW_Warehouse = Data.Warehouse.PK;

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader.WRH_ReferenceNumber = "R123";
			receiveHeader.WRH_WW_Warehouse = Data.Warehouse.PK;
			receiveHeader.WRH_WL_StagingLocation = location.PK;
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], loadListWithPackageStates, receiveHeader);
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState1.PK })).ToArray();

			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(Data.HeaderDataObject, newFactory, packageStates, new List<ContainerByDLLDTO>(), null);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment,  Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched, finder);
			var loadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();
			AssertEquals(0, loadListsMatched.Count);
			AssertEquals("DLL00000001", loadList.WDL_JobID);

			var bizOFactory = newFactory.BOFactory;
			AssertNotEquals("The load list should not be matched.", loadListWithPackageStates.PK, loadList.PK);
			AssertEquals("PackageState1 must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(packageState1.PK).WPS_WDL_LoadList, loadList.PK);
			Assert("New Load list Staging Location is Empty as no Staging Rule Setup.", loadList.WDL_WL_StagingLocation.IsEmpty);
			Assert("New Load list must not be ready to stage as Staging Location is Empty.", !loadList.WDL_IsReadyToStage);
		}

		public void TestPopulateBusinessObjectWithLastDischargePort()
		{
			var consignmentID = "CONID123";
			var portOfDischarge = new UNLOCO() { Code = "NZTST" };
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.PortOfDischarge = portOfDischarge;
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.FreightLoaded);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState1.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			AssertNotEquals(dll.WDL_RL_NKLastDischargePort, portOfDischarge.Code);
			var loadList = reader.ReadIntoBusinessObject();
			AssertEquals(loadList.WDL_RL_NKLastDischargePort, portOfDischarge.Code);
			AssertEquals("Should not create a new Load List", dll.PK, loadList.PK);
		}

		public void TestPopulateBusinessObjectWithNullLastDischargePort()
		{
			var consignmentID = "CONID123";
			var portOfDischarge = new UNLOCO() { Code = "NZTST" };
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			dll.WDL_RL_NKLastDischargePort = portOfDischarge.Code.Value;
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.FreightLoaded);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState1.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			AssertNull(consol.PortOfDischarge);
			AssertEquals(packageState1.DispatchLoadList.WDL_RL_NKLastDischargePort, portOfDischarge.Code.Value);
			var resultDll = reader.ReadIntoBusinessObject();
			AssertEquals("Discharge Port Returns Empty String", resultDll.WDL_RL_NKLastDischargePort, ZString.Empty);
		}

		public void TestPopulateBusinessObject_AllPackagesAreDeparted()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			AssertEquals("Precondition", 2, receiveConsignment.PackageStates.Count);

			var leg = new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "ZAJNB" } };
			leg.VesselName = "VesselName";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(leg);
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.Departed);
			var packageState2 = SetupPackageState(receiveConsignment.PackageStates[1], dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.Finalized);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState1.PK, packageState2.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var loadList = reader.ReadIntoBusinessObject();
			AssertEquals("Should not create a new Load List", dll.PK, loadList.PK);

			var additionalReferences = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			AssertEquals("Should not populate additional reference when all packages are departed.", false, additionalReferences.Any(r => r.CE_EntryNum == "VesselName"));
		}

		public void TestPopulateBusinessObject_RemovedPackageIsDeparted()
		{
			TestPopulateBusinessObject_RemovedPackageCore(isFinalized: false);
		}

		public void TestPopulateBusinessObject_RemovedPackageIsFinalised()
		{
			TestPopulateBusinessObject_RemovedPackageCore(isFinalized: true);
		}

		void TestPopulateBusinessObject_RemovedPackageCore(bool isFinalized)
		{
			var reader = GetReaderFor_TestPopulateBusinessObject_RemovedPackage_Core(removeLoadedPackage: false, removeDepartedPackage: true, departedPackageIsFinalized: isFinalized);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), @"At least one package is departed already. Therefore cannot be removed from load list.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBusinessObject_RemovedPackageIsFreightLoaded_LoadNotComplete_DoesNotThrow()
		{
			TestPopulateBusinessObject_RemovedPackageIsFreightLoaded_Core(isLoadComplete: false);
		}

		public void TestPopulateBusinessObject_RemovedPackageIsFreightLoaded_LoadComplete_Throws()
		{
			TestPopulateBusinessObject_RemovedPackageIsFreightLoaded_Core(isLoadComplete: true);
		}

		void TestPopulateBusinessObject_RemovedPackageIsFreightLoaded_Core(bool isLoadComplete)
		{
			var reader = GetReaderFor_TestPopulateBusinessObject_RemovedPackage_Core(isLoadComplete: isLoadComplete, removeLoadedPackage: true);

			if (isLoadComplete)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Attempting to reassign Packages from Load Lists for Dispatch Transportation Units that have already been closed.
These Packages can only be removed from their current Load List via Transit Warehouse Desktop:
Package        Load List                   DTU
PKG-Loaded     WaybillParent (C1000000)    V1 (DTU1)",
					() => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Should not throw if the removed package's DTU is not load complete.", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestPopulateBusinessObject_RemovePackage_LoadListReadyToStage()
		{
			TestPopulateBusinessObject_RemovePackage_LoadListReadyToStage_Core(isReadyToStage: true);
		}

		public void TestPopulateBusinessObject_RemovePackage_LoadListNotReadyToStage()
		{
			TestPopulateBusinessObject_RemovePackage_LoadListReadyToStage_Core(isReadyToStage: false);
		}

		void TestPopulateBusinessObject_RemovePackage_LoadListReadyToStage_Core(bool isReadyToStage)
		{
			var reader = GetReaderFor_TestPopulateBusinessObject_RemovedPackage_Core(isReadyToStage: isReadyToStage, removeLoadedPackage: true);
			AssertNoExceptionThrown("Should not throw if the removed package's DTU is not load complete.", () => reader.ReadIntoBusinessObject());

			const string stoppedLoadingLogMessage = "Stopping Load List C1000000 as some of its packages were removed.";
			if (isReadyToStage)
			{
				AssertContains("Should log that the load list has been stopped", stoppedLoadingLogMessage, Logger.Logs);
			}
			else
			{
				AssertNotContains("Should log that the load list has been stopped if it was already stopped", stoppedLoadingLogMessage, Logger.Logs);
			}
		}

		WhsItemDispatchLoadListDataObjectReader GetReaderFor_TestPopulateBusinessObject_RemovedPackage_Core(bool isReadyToStage = true, bool isLoadComplete = false, bool removeLoadedPackage = false, bool removeDepartedPackage = false, bool departedPackageIsFinalized = false)
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-Received", "PKG-Loaded", "PKG-Departed" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var receivedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Received");
			var loadedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded");
			var departedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Departed");

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var loadCompleteDTU = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK, vehicleRef: "V1");
			var departedDTU = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK, vehicleRef: "V2");
			var dll = CreateLoadList(Data.Warehouse, "C1000000");
			dll.WDL_IsReadyToStage = isReadyToStage;
			dll.WDL_WL_StagingLocation = location.PK;
			SetupPackageState(receivedPackage, dll, receiveHeader, null, null);
			SetupPackageState(loadedPackage, dll, receiveHeader, dcn, loadCompleteDTU, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetupPackageState(departedPackage, dll, receiveHeader, dcn, departedDTU, departedPackageIsFinalized ? TransitWarehouseStatuses.Codes.Finalized : TransitWarehouseStatuses.Codes.Departed);

			var now = DateTime.UtcNow;
			Helper.FinishLoading(loadCompleteDTU, gateIn: now.AddDays(-1), loadComplete: isLoadComplete ? now : ZDateTimeOffset.Empty, gateOut: ZDateTimeOffset.Empty);
			Helper.FinishLoading(departedDTU, gateIn: now.AddDays(-2), loadComplete: now.AddDays(-1), gateOut: now);

			Factory.SaveForTesting();

			var packagesToReimport = new List<ZGuid> { receivedPackage.PK };

			if (!removeLoadedPackage)
			{
				packagesToReimport.Add(loadedPackage.PK);
			}

			if (!removeDepartedPackage)
			{
				packagesToReimport.Add(departedPackage.PK);
			}

			var newFactory = new UniversalObjectFactory();
			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var packageStates = newFactory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packagesToReimport)).ToArray();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(Data.HeaderDataObject, newFactory, packageStates, new List<ContainerByDLLDTO>(), null);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched, finder);

			return reader;
		}

		public void TestPopulateBusinessObject_PackagesOnDCNNotAuthorizedForDispatchNotRemoved()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-Received", "PKG-Loaded", "PKG-Departed" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var receivedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Received");
			var loadedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded");
			var departedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Departed");

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK, isAuthorizedForDispatch: false);
			var loadCompleteDTU = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK, vehicleRef: "V1");
			var departedDTU = Helper.CreateDispatchTransportationUnit("DTU2", Data.Warehouse.PK, vehicleRef: "V2");
			var dll = CreateLoadList(Data.Warehouse, "C1000000");
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = location.PK;
			SetupPackageState(receivedPackage, dll, receiveHeader, null, null);
			SetupPackageState(loadedPackage, dll, receiveHeader, dcn, loadCompleteDTU, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetupPackageState(departedPackage, dll, receiveHeader, dcn, departedDTU, false ? TransitWarehouseStatuses.Codes.Finalized : TransitWarehouseStatuses.Codes.Departed);

			var now = DateTime.UtcNow;
			Helper.FinishLoading(loadCompleteDTU, gateIn: now.AddDays(-1), loadComplete: true ? now : ZDateTimeOffset.Empty, gateOut: ZDateTimeOffset.Empty);
			Helper.FinishLoading(departedDTU, gateIn: now.AddDays(-2), loadComplete: now.AddDays(-1), gateOut: now);

			Factory.SaveForTesting();

			var packagesToReimport = new List<ZGuid> { receivedPackage.PK };

			packagesToReimport.Add(loadedPackage.PK);

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, packagesToReimport)).ToArray();
			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched);
			var dllAfterReimport = reader.ReadIntoBusinessObject();
			AssertEquals("DLL packages qty not changed", 3, dllAfterReimport.PackageStates.Count);
			AssertNotContains("The following packages have been detached from their Load Lists:", Logger.Logs);
		}

		public void TestPopulateBusinessObject_RemovedMultiplePackages()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-Received", "PKG-Loaded1", "PKG-Loaded2", "PKG-Loaded3" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var receivedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Received");
			var loadedPackage1 = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded1");
			var loadedPackage2 = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded2");
			var loadedPackage3 = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded3");

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK, vehicleRef: "V1");
			var dll = CreateLoadList(Data.Warehouse, "C1000000");
			dll.WDL_IsReadyToStage = true;
			dll.WDL_WL_StagingLocation = location.PK;
			SetupPackageState(receivedPackage, dll, receiveHeader, null, null);
			SetupPackageState(loadedPackage1, dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetupPackageState(loadedPackage2, dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.FreightLoaded);
			SetupPackageState(loadedPackage3, dll, receiveHeader, dcn, dtu, TransitWarehouseStatuses.Codes.FreightLoaded);

			var now = DateTime.UtcNow;
			Helper.FinishLoading(dtu, gateIn: now.AddDays(-1), loadComplete: now, gateOut: ZDateTimeOffset.Empty);

			Factory.SaveForTesting();

			var packagesToReimport = new[] { receivedPackage.PK };

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packagesToReimport)).ToArray();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(Data.HeaderDataObject, newFactory, packageStates, new List<ContainerByDLLDTO>(), null);
			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched, finder);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Attempting to reassign Packages from Load Lists for Dispatch Transportation Units that have already been closed.
These Packages can only be removed from their current Load List via Transit Warehouse Desktop:
Package        Load List                   DTU
PKG-Loaded1    WaybillParent (C1000000)    V1 (DTU1)
PKG-Loaded2    WaybillParent (C1000000)    V1 (DTU1)
PKG-Loaded3    WaybillParent (C1000000)    V1 (DTU1)",
				() => reader.ReadIntoBusinessObject());
		}

		WhsItemPackageState SetupPackageState(WhsItemPackageState packageState, WhsItemDispatchLoadList loadlist, WhsItemReceiveTransportationUnit receiveUnit, WhsItemDispatchConsignment dcn = null, WhsItemDispatchTransportationUnit dtu = null, string status = TransitWarehouseStatuses.Codes.Putaway)
		{
			packageState.WPS_Status = status;
			packageState.WPS_WRH_TransitReceiveHeader = receiveUnit.PK;
			packageState.WPS_WDL_LoadList = loadlist != null ? loadlist.PK : ZGuid.Empty;
			packageState.WPS_WDC_TransitDispatchConsignment = dcn != null ? dcn.PK : ZGuid.Empty;
			packageState.WPS_WDH_TransitDispatchHeader = dtu != null ? dtu.PK : ZGuid.Empty;
			packageState.WPS_WL_LastLocation = receiveUnit.WRH_WL_StagingLocation;
			packageState.WPS_WL_ReceiveLocation = receiveUnit.WRH_WL_StagingLocation;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";

			return packageState;
		}

		WhsItemDispatchLoadList CreateLoadList(IWhsWarehouse warehouse, string referenceNumber)
		{
			var dll = Helper.CreateDispatchLoadList(referenceNumber, warehouse.PK);
			var ent = Factory.New<CusEntryNumber>();
			ent.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber;
			ent.CE_EntryNum = referenceNumber;
			ent.CE_ParentID = dll.PK;
			ent.CE_ParentTable = WhsItemDispatchLoadListSchema.Constants.TableName;
			ent.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			return dll;
		}

		IWhsLocation GetLocation(IWhsWarehouse warehouse)
		{
			var query = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK);
			return Factory.BOFactory.LoadTop1<IWhsLocation>(query);
		}

		#endregion

		#region Matching

		#region TestPopulateBizo_ReimportShipment_DoesNot_DelinkOrDeleteMatchingLoadList

		public void TestPopulateBusinessObject_ReimportShipment_DoesNot_DelinkOrDeleteMatchingLoadList()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);

			AssertEquals("Precondition", 2, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], loadList, receiveHeader);
			var packageState2 = SetupPackageState(receiveConsignment.PackageStates[1], loadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState1.PK, packageState2.PK })).ToArray();

			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched);
			var matchingLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList.WDL_JobID);

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("Matched to the existing Load List.", loadList.PK, matchingLoadList.PK);
			AssertEquals("PackageState1 should still point to the existing Load List.", bizOFactory.Load<WhsItemPackageState>(packageState1.PK).WPS_WDL_LoadList, matchingLoadList.PK);
			AssertEquals("PackageState2 should still point to the existing Load List.", bizOFactory.Load<WhsItemPackageState>(packageState2.PK).WPS_WDL_LoadList, matchingLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_RemoveContainersAndReimportShipment_ReusesLoadList

		public void TestPopulateBizo_RemoveContainersAndReimportShipment_ReusesLoadList()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var matchingLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(matchingLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("Cont1", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(matchingLoadList.PK, dtu.PK);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), matchingLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The loose load list reuses any of the Consol's load lists", matchingLoadList.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_AddContainersAndReimportShipment_ReusesLoadListWithNoContainer

		public void TestPopulateBizo_AddContainersAndReimportShipment_ReusesLoadListWithNoContainer()
		{
			var matchingLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(matchingLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var container = Data.CreateContainer("Cont1", 1);

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The loose load list reuses any of the Consol's load lists", matchingLoadList.PK, selectedLoadList.PK);
		}

		public void TestPopulateBizo_AddContainersAndReimportShipment_MultipleLoadLists_NotReuseLoadListWithNoContainer()
		{
			var emptyLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(emptyLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);

			var loadListWithContainer = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadListWithContainer, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(loadListWithContainer.PK, dtu.PK);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var container = Data.CreateContainer("Cont1", 1);

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list with cointainer should be picked.", loadListWithContainer.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_NoContainers_MultipleLoadLists_MatchesMostRecentWithNoContainers

		public void TestPopulateBizo_NoContainers_MultipleLoadLists_MatchesMostRecentWithNoContainers()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var utcNow = ZDateTime.UtcNow;
			var oldLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			oldLoadList.WDL_SystemCreateTimeUtc = utcNow.AddDays(-2);

			var mostRecentLoadListWithoutContainer = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadListWithoutContainer, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			mostRecentLoadListWithoutContainer.WDL_SystemCreateTimeUtc = utcNow.AddDays(-1);

			var mostRecentLoadListWithContainer = Helper.CreateDispatchLoadList("DLL00000030", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadListWithContainer, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			mostRecentLoadListWithContainer.WDL_SystemCreateTimeUtc = utcNow;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(mostRecentLoadListWithContainer.PK, dtu.PK);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The most recent load list without a container is matched", mostRecentLoadListWithoutContainer.PK, selectedLoadList.PK);
		}

		public void TestPopulateBizo_NoContainers_MultipleLoadLists_MatchesMostRecentWithNoContainers_DataTarget()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateTransitDataTargetWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var utcNow = ZDateTime.UtcNow;
			var oldLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			oldLoadList.WDL_SystemCreateTimeUtc = utcNow.AddDays(-2);

			var mostRecentLoadListWithoutContainer = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadListWithoutContainer, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			mostRecentLoadListWithoutContainer.WDL_SystemCreateTimeUtc = utcNow.AddDays(-1);

			var mostRecentLoadListWithContainer = Helper.CreateDispatchLoadList("DLL00000030", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadListWithContainer, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			mostRecentLoadListWithContainer.WDL_SystemCreateTimeUtc = utcNow;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(mostRecentLoadListWithContainer.PK, dtu.PK);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consolLevelDataTarget = Data.HeaderDataObjectWithDataTarget;
			Logger.TopLevelDataObject = consolLevelDataTarget;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consolLevelDataTarget, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The most recent load list without a container is matched", mostRecentLoadListWithoutContainer.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_NoContainers_MultipleLoadLists_FallsBackToMostRecentWithContainers

		public void TestPopulateBizo_NoContainers_MultipleLoadLists_FallsBackToMostRecentWithContainers()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var utcNow = ZDateTime.UtcNow;
			var oldLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			oldLoadList.WDL_SystemCreateTimeUtc = utcNow.AddDays(-1);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(oldLoadList.PK, dtu.PK);

			var mostRecentLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			mostRecentLoadList.WDL_SystemCreateTimeUtc = utcNow;
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK, containerID: "Cont2");
			Helper.CreateDispatchDLLDTUPivot(mostRecentLoadList.PK, dtu2.PK);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The most recent load list is matched", mostRecentLoadList.PK, selectedLoadList.PK);
		}

		public void TestPopulateBizo_NoContainers_MultipleLoadLists_FallsBackToMostRecentWithContainers_DataTarget()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateTransitDataTargetWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var utcNow = ZDateTime.UtcNow;
			var oldLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			oldLoadList.WDL_SystemCreateTimeUtc = utcNow.AddDays(-1);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(oldLoadList.PK, dtu.PK);

			var mostRecentLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			mostRecentLoadList.WDL_SystemCreateTimeUtc = utcNow;
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK, containerID: "Cont2");
			Helper.CreateDispatchDLLDTUPivot(mostRecentLoadList.PK, dtu2.PK);

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consolWithDataTarget = Data.HeaderDataObjectWithDataTarget;
			Logger.TopLevelDataObject = consolWithDataTarget;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consolWithDataTarget, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The most recent load list is matched", mostRecentLoadList.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_ContainersWithIDs_MultipleMatchingLoadLists_MatchesMostRecent

		public void TestPopulateBizo_ContainersWithIDs_MultipleMatchingLoadLists_MatchesMostRecent()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var oldContainerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldContainerLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(oldContainerLoadList.PK, dtu.PK);
			oldContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var mostRecentContainerLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentContainerLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(mostRecentContainerLoadList.PK, dtu2.PK);
			mostRecentContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldContainerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("Cont1", 1);
			consol.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", mostRecentContainerLoadList.PK, selectedLoadList.PK);
		}

		public void TestPopulateBizo_ContainersWithIDs_MultipleMatchingLoadLists_MatchesMostRecent_WithDataTarget()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateTransitDataTargetWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var oldContainerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldContainerLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(oldContainerLoadList.PK, dtu.PK);
			oldContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var mostRecentContainerLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentContainerLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(mostRecentContainerLoadList.PK, dtu2.PK);
			mostRecentContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldContainerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consolWithDataTarget = Data.HeaderDataObjectWithDataTarget;
			Logger.TopLevelDataObject = consolWithDataTarget;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consolWithDataTarget.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("Cont1", 1);
			consolWithDataTarget.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consolWithDataTarget, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", mostRecentContainerLoadList.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizo_ContainersWithIDs_ForwarderChangesContainerType

		public void TestPopulateBizo_ContainersWithIDs_ForwarderChangesContainerType()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var containerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(containerLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(containerLoadList.PK, dtu.PK);
			AssertEquals("Precondition", "20GP", dtu.Container.ContainerType.RC_Code);
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), containerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("Cont1", 1);
			container.ContainerType = new ContainerType { Code = "40GP" };
			consol.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", containerLoadList.PK, selectedLoadList.PK);
			AssertEquals("The load list is matched", dtu.PK, selectedLoadList.DispatchTransportationUnits.Single().PK);
		}

		public void TestPopulateBizo_ContainersWithIDs_SenderChangesContainerType_DataTarget()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateTransitDataTargetWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var containerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(containerLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(containerLoadList.PK, dtu.PK);
			AssertEquals("Precondition", "20GP", dtu.Container.ContainerType.RC_Code);
			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), containerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consolWithTarget = Data.HeaderDataObjectWithDataTarget;
			Logger.TopLevelDataObject = consolWithTarget;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consolWithTarget.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("Cont1", 1);
			container.ContainerType = new ContainerType { Code = "40GP" };
			consolWithTarget.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consolWithTarget, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", containerLoadList.PK, selectedLoadList.PK);
			AssertEquals("The load list is matched", dtu.PK, selectedLoadList.DispatchTransportationUnits.Single().PK);
		}

		#endregion

		#region TestPopulateBizo_ContainersWithoutIDs_MultipleMatchingLoadLists_MatchesMostRecent

		public void TestPopulateBizo_ContainersWithoutIDs_MultipleMatchingLoadLists_MatchesMostRecent()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var oldContainerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldContainerLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(oldContainerLoadList.PK, dtu2.PK);
			oldContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var mostRecentContainerLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentContainerLoadList, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(mostRecentContainerLoadList.PK, dtu.PK);
			mostRecentContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldContainerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer(string.Empty, 1);
			container.ContainerType = new ContainerType { Code = "20GP" };
			consol.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", mostRecentContainerLoadList.PK, selectedLoadList.PK);
		}

		public void TestPopulateBizo_ContainersWithoutIDs_MultipleMatchingLoadLists_MatchesMostRecent_DataTarget()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateTransitDataTargetWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var oldContainerLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(oldContainerLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000002", Data.Warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(oldContainerLoadList.PK, dtu2.PK);
			oldContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var mostRecentContainerLoadList = Helper.CreateDispatchLoadList("DLL00000020", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(mostRecentContainerLoadList, "WaybillParent", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("RS0000001", Data.Warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(mostRecentContainerLoadList.PK, dtu.PK);
			mostRecentContainerLoadList.WDL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			AssertEquals("Precondition", 1, receiveConsignment.PackageStates.Count);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState = SetupPackageState(receiveConsignment.PackageStates.Single(), oldContainerLoadList, receiveHeader);
			Factory.SaveForTesting();

			var consolWithDataTarget = Data.HeaderDataObjectWithDataTarget;
			Logger.TopLevelDataObject = consolWithDataTarget;

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();

			consolWithDataTarget.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer(string.Empty, 1);
			container.ContainerType = new ContainerType { Code = "20GP" };
			consolWithDataTarget.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consolWithDataTarget, new Container[] { container }, Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var selectedLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("The load list is matched", mostRecentContainerLoadList.PK, selectedLoadList.PK);
		}

		#endregion

		#region TestPopulateBizO_MatchesByMBL

		public void TestPopulateBizO_MatchesByMBL_WhenIgnoreContainerPacking_HasContainer()
		{
			WarehouseDataRegistry.Instance.CreateSingleDLLForAllContainers.SetTemporaryValue(Guid.Empty, Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true);

			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "MAB001", WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObjectWithDataTarget;
			consol.WayBillNumber = "MAB001";
			Logger.TopLevelDataObject = consol;

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer(string.Empty, 1);
			container.ContainerType = new ContainerType { Code = "20GP" };
			consol.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var newFactory = new UniversalObjectFactory();

			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched);
			var matchingLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList.WDL_JobID);

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("Matched to the existing Load List.", loadList.PK, matchingLoadList.PK);
		}

		public void TestPopulateBizO_MatchesByMBL_WhenNotIgnoreContainerPacking_NoContainers()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "MAB001", WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObjectWithDataTarget;
			consol.WayBillNumber = "MAB001";
			Logger.TopLevelDataObject = consol;

			var newFactory = new UniversalObjectFactory();

			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched);
			var matchingLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList.WDL_JobID);

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("Matched to the existing Load List.", loadList.PK, matchingLoadList.PK);
		}

		public void TestPopulateBizO_MatchesByMBL_WhenNotIgnoreContainerPacking_HasContainers()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "MAB001", WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			Factory.SaveForTesting();

			var consol = Data.HeaderDataObjectWithDataTarget;
			consol.WayBillNumber = "MAB001";
			Logger.TopLevelDataObject = consol;

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer(string.Empty, 1);
			container.ContainerType = new ContainerType { Code = "20GP" };
			consol.ContainerCollection.Add(container);
			consignmentDataObject.PackingLineCollection.Single().ContainerLink = 1;

			var newFactory = new UniversalObjectFactory();

			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched);
			var matchingLoadList = reader.ReadIntoBusinessObject();

			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList.WDL_JobID);

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("Matched to the existing Load List.", loadList.PK, matchingLoadList.PK);
		}

		public void TestPopulateBizO_MatchesByMBL_AndPacklingLines_NoContainers()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "MAB001", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			receiveConsignment.PackageStates.ForEach(ps => ps.WPS_WDL_LoadList = loadList.PK);
			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			Factory.SaveForTesting();

			var gateBookingShipment = Data.CreateHeaderObjectForGateBooking("ABC-123", "RTRK", "NewDriver");
			Data.SetupNewDataContextWithDataSource(gateBookingShipment, gateBookingNumber: "GTB001", isArrival: true);
			gateBookingShipment.WayBillNumber = "MAB001";
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("BRK001", "MAB001", isPickup: false);

			var gateBookingPackageIDs = new string[] { "PKG-1" };
			var gateBookingPackingLines = new DataObjectList<PackingLine>(packageIDs.Select(packageID => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = packageID, PackType = new PackageType { Code = "PKG" } }));
			gateSubShipment.SetPackingLineCollection(() => gateBookingPackingLines);
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });

			Logger.TopLevelDataObject = gateBookingShipment;

			var newFactory = new UniversalObjectFactory();
			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader1 = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, gateBookingShipment, Array.Empty<Container>(), gateBookingPackingLines, null, Logger, newFactory, loadListsMatched, masterBillNumber: gateBookingShipment.WayBillNumber);
			var matchingLoadList1 = reader1.ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList1.WDL_JobID);
			AssertEquals("Matched to the existing Load List by using Master Bill and PackingLines.", loadList.PK, matchingLoadList1.PK);
		}

		public void TestPopulateBizO_MatchesByMBL_AndVehicleReference_NoContainers()
		{
			var vehicleReference = "ABC-123";
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var loadList = Helper.CreateDispatchLoadList("DLL00000001", Data.Warehouse.PK);
			Helper.CreateAdditionalReference(loadList, "MAB001", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			receiveConsignment.PackageStates.ForEach(ps => ps.WPS_WDL_LoadList = loadList.PK);
			var dtu = Helper.CreateDispatchTransportationUnit(vehicleReference, Data.Warehouse.PK, vehicleReference);
			var dllDtuPivot = Helper.CreateDispatchDLLDTUPivot(loadList.PK, dtu.PK);
			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			Factory.SaveForTesting();

			var gateBookingShipment = Data.CreateHeaderObjectForGateBooking(vehicleReference, "RTRK", "NewDriver");
			Data.SetupNewDataContextWithDataSource(gateBookingShipment, gateBookingNumber: "GTB001", isArrival: true);
			gateBookingShipment.WayBillNumber = "MAB001";
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("BRK001", "MAB001", isPickup: false);
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });

			Logger.TopLevelDataObject = gateBookingShipment;

			var newFactory = new UniversalObjectFactory();
			var loadListsMatched = new List<WhsItemDispatchLoadList>();
			var reader1 = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, gateBookingShipment, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, loadListsMatched, masterBillNumber: gateBookingShipment.WayBillNumber);
			var matchingLoadList1 = reader1.ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			AssertEquals("DLL00000001", matchingLoadList1.WDL_JobID);
			AssertEquals("Matched to the existing Load List by using Master Bill and vehicle reference.", loadList.PK, matchingLoadList1.PK);
		}

		#endregion

		#endregion

		#region TestPopulateStagingLocation

		public void TestPopulateStagingLocation_OVPPackageHasNoRCN()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var location = GetLocation(Data.Warehouse);
			location[WhsLocationViewSchema.WLV_TransitDischargeLRC.Name] = "AUBNE";
			location[WhsLocationViewSchema.WLV_RS_NKTransitServiceLevel.Name] = "STD";
			receiveConsignment.WRC_RL_NKNextDischargePort = "AUBNE";
			receiveConsignment.WRC_RS_NKServiceLevel = "STD";

			var loadListToBeDeleted = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadListToBeDeleted.WDL_WW_Warehouse = Data.Warehouse.PK;

			AssertEquals("Precondition", 2, receiveConsignment.PackageStates.Count);

			var dcn = Helper.CreateDispatchConsignment("D123", Data.Warehouse.PK);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], loadListToBeDeleted, receiveHeader);
			var packageState2 = SetupPackageState(receiveConsignment.PackageStates[1], loadListToBeDeleted, receiveHeader);
			packageState1.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageState2.WPS_WDC_TransitDispatchConsignment = dcn.PK;

			var ovpPackageState = Helper.CreateOverpackPackage("OVP-1", dcn, receiveHeader, dcn: dcn);
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, packageState1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: ovpPackageState);
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, packageState2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: ovpPackageState);
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var newFactory = new UniversalObjectFactory();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(newFactory, [dcn], FindOption.ByDCNs);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>(), finder);
			var loadList = reader.ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("OverPack PackageState must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(ovpPackageState.PK).WPS_WDL_LoadList, loadList.PK);
			AssertEquals("PackageState1 must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(packageState1.PK).WPS_WDL_LoadList, loadList.PK);
			AssertEquals("PackageState2 must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(packageState2.PK).WPS_WDL_LoadList, loadList.PK);
			AssertEquals("New LoadList is populated with Staging Location.", location.PK, loadList.WDL_WL_StagingLocation);
			AssertEquals("New LoadList Is Ready to Stage is set to False.", false, loadList.WDL_IsReadyToStage);
		}

		public void TestPopulateStagingLocation()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var dcn = Helper.CreateDispatchConsignment("D123", Data.Warehouse.PK);

			var location = GetLocation(Data.Warehouse);
			location[WhsLocationViewSchema.WLV_TransitDischargeLRC.Name] = "AUBNE";
			location[WhsLocationViewSchema.WLV_RS_NKTransitServiceLevel.Name] = "STD";
			receiveConsignment.WRC_RL_NKNextDischargePort = "AUBNE";
			receiveConsignment.WRC_RS_NKServiceLevel = "STD";

			var loadListToBeDeleted = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadListToBeDeleted.WDL_WW_Warehouse = Data.Warehouse.PK;

			AssertEquals("Precondition", 2, receiveConsignment.PackageStates.Count);

			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var packageState1 = SetupPackageState(receiveConsignment.PackageStates[0], loadListToBeDeleted, receiveHeader);
			var packageState2 = SetupPackageState(receiveConsignment.PackageStates[1], loadListToBeDeleted, receiveHeader);
			packageState1.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageState2.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var newFactory = new UniversalObjectFactory();

			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(newFactory, [dcn], FindOption.ByDCNs);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>(), finder);
			var loadList = reader.ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			var bizOFactory = newFactory.BOFactory;
			AssertEquals("PackageState1 must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(packageState1.PK).WPS_WDL_LoadList, loadList.PK);
			AssertEquals("PackageState2 must point to the new load list.", bizOFactory.Load<WhsItemPackageState>(packageState2.PK).WPS_WDL_LoadList, loadList.PK);
			AssertEquals("New LoadList is populated with Staging Location.", location.PK, loadList.WDL_WL_StagingLocation);
			AssertEquals("New LoadList Is Ready to Stage is set to False.", false, loadList.WDL_IsReadyToStage);
		}

		#endregion

		#region TestPopulateTransportMode

		public void TestPopulateTransportMode()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var leg = new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "ZAJNB" } };
			leg.VesselName = "VesselName";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(leg);
			consol.TransportMode = new CodeDescriptionPair { Code = TransportModes.Sea };
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			AssertEquals("Populated Transport Mode.", TransportModes.Sea, loadList.WDL_TransportMode);
		}

		#endregion

		#region TestPopulateBusinessObject_AdditionalReference

		public void TestPopulateBusinessObject_AdditionalReference()
		{
			AssertPopulateAdditionalReference(l => l.VesselName = "VesselName", WarehouseAdditionalReferenceTypes.Codes.Vessel, "VesselName");
			AssertPopulateAdditionalReference(l => l.VoyageFlightNo = "VoyageFlightNumber", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VoyageFlightNumber");
			AssertPopulateAdditionalReference(l => l.PortOfDischarge = new UNLOCO { Code = "SLCMB" }, WarehouseAdditionalReferenceTypes.Codes.DestinationPort, "SLCMB");
			var estimatedDeparture = ZDateTime.Now.AddDays(1);
			AssertPopulateAdditionalReference(l => l.EstimatedDeparture = estimatedDeparture, WarehouseAdditionalReferenceTypes.Codes.ETDDate, estimatedDeparture.FormatDateTime());
		}

		void AssertPopulateAdditionalReference(Action<TransportLeg> setProperty, string referenceType, string expectedReferenceValue)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var leg = new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "ZAJNB" } };

			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";
			shipment.TransportLegCollection.Add(leg);
			setProperty(leg);

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, Array.Empty<Container>(), Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList = reader.ReadIntoBusinessObject();
			var additionalReferences = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			var additionalReference = additionalReferences.Cast<ICusEntryNumber>().Single(c => c.CE_EntryType == referenceType);
			AssertEquals("Additional Reference value should be populated.", expectedReferenceValue, additionalReference.CE_EntryNum);
		}

		#endregion

		#region TestAdditionReference_CarrierBookingReference

		public void TestAdditionReference_CarrierBookingReference()
		{
			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000", runSheet: "R1");
			consolDO.BookingConfirmationReference = "Booking1";

			Logger.TopLevelDataObject = consolDO;
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipmentDO, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList = reader.ReadIntoBusinessObject();

			var additionalReferences = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			var additionalReference = additionalReferences.Cast<ICusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);
			AssertEquals("Additional Reference value should be populated.", "Booking1", additionalReference.CE_EntryNum);
		}

		#endregion

		#region TestAdditionalReferences_RunSheetDataContext

		public void TestAdditionalReferences_RunSheetDataContext()
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObjectWithoutChildShipment;
			var headerDataContext = DataContextFactory.New();
			headerDataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "R1");
			headerDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			header.DataContext = headerDataContext;

			var shipment = Data.CreateShipmentWithPackages("1", "Pack1");
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			var childDataContext = DataContextFactory.New();
			childDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			shipment.DataContext = childDataContext;
			header.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			header.SubShipmentCollection.Add(shipment);

			Logger.TopLevelDataObject = header;

			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList = reader.ReadIntoBusinessObject();

			var additionalReferences = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			var additionalReference = additionalReferences.Cast<ICusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);
			AssertEquals("Additional Reference value should be populated.", "R1", additionalReference.CE_EntryNum);
		}

		#endregion

		#region TestDispatchLoadListJobID

		public void TestDispatchLoadListJobID()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList = reader.ReadIntoBusinessObject();
			AssertEquals("Dispatch Load List WDL_JobID should be DLL00000001", "DLL00000001", loadList.WDL_JobID);
		}

		#endregion

		public void TestPopulateReferenceNumber()
		{
			Data.SetupForForwardingImport();
			var shipment1 = Data.HeaderDataObjectWithoutChildShipment;
			var headerDataContext = DataContextFactory.New();
			headerDataContext.AddDataSource(DataContextType.ForwardingConsol, "C0001");
			shipment1.DataContext = headerDataContext;
			Logger.TopLevelDataObject = shipment1;

			var reader1 = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment1, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList1 = reader1.ReadIntoBusinessObject();

			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipment2.DataContext.AddDataSource(DataContextType.LandTransportConsignment, "C123");
			shipment2.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment2.TransportMode = new CodeDescriptionPair { Code = TransportModes.Sea };
			Logger.TopLevelDataObject = shipment2;

			var reader2 = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment2, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>());
			var loadList2 = reader2.ReadIntoBusinessObject();

			AssertEquals("Reference value should be consolKey.", "C0001", loadList1.WDL_ReferenceNumber);
			AssertEquals("Reference value should be JobID", "DLL00000002", loadList2.WDL_ReferenceNumber);
		}

		public void TestPolulateCreditor()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var leg = new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "ZAJNB" } };
			leg.VesselName = "VesselName";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(leg);
			consol.TransportMode = new CodeDescriptionPair { Code = TransportModes.Sea };

			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.Creditor));
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			AssertEquals("Populated Creditor.", "WUFU SHIPPING LINE", loadList.Creditor.CompanyName);
		}

		#region TestLogForDLLLinkedToConsol

		public void TestLogForDLLLinkedToConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1000000";
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C1000000");
			shipment.DataContext = dataContext;

			var outboundSessionTracker = new Mock<IDataWritingManager>();
			Logger.OutboundSessionTracker = outboundSessionTracker.Object;

			new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, shipment, null, null, Data.Orgs.INTHEMSYD, Logger, Factory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			AssertContains("Dispatch Load list DLL00000001 linked to Consol C1000000.", Logger.Logs);
		}

		#endregion

		#region TestImportNotes

		public void TestPopulateBizO_ImportNoteInXML()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var leg = new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "ZAJNB" } };
			leg.VesselName = "VesselName";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			consol.TransportLegCollection.Add(leg);
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, receiveHeader);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			var notes = loadList.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Should import all note visibility types.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note", StmNoteDescription.Int, true);

			consol.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note UPDATED", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, newFactory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			notes = loadList.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Notes should have been updated.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note UPDATED", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, true);
		}

		#endregion

		#region TestPopulateUniversalLinks

		public void TestUniversalLinks()
		{
			var bookingParty = Data.Orgs.INTHEMSYD;
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", new[] { "PKG-1", "PKG-2" });
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var plannedPackageStates = receiveConsignment.PackageStates.Select(p => DataObjectReader.GetColumnIndexerFromRow(p)).ToArray();
			var consol = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consol, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consol;

			var loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty, Logger, Factory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this load list.", 1, links.Count(l => l.UCL_ParentID == loadList.PK));
			AssertUniversalJobLink(links, bookingParty, loadList.PK, loadList.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");

			var factoryForResending = new UniversalObjectFactory();
			var loadListAfterResending = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty, Logger, factoryForResending, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			factoryForResending.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same LoadList.", loadList.PK, loadListAfterResending.PK);

			var linksAfterResending = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this load list.", 1, linksAfterResending.Count(l => l.UCL_ParentID == loadListAfterResending.PK));
			AssertUniversalJobLink(linksAfterResending, bookingParty, loadListAfterResending.PK, loadListAfterResending.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_LinkWithDifferentDataSource()
		{
			var bookingParty = Data.Orgs.INTHEMSYD;
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", new[] { "PKG-1", "PKG-2" });
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var plannedPackageStates = receiveConsignment.PackageStates.Select(p => DataObjectReader.GetColumnIndexerFromRow(p)).ToArray();
			var consol = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consol, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consol;

			var loadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty, Logger, Factory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal links being created for this load list.", 1, links.Count(l => l.UCL_ParentID == loadList.PK));
			AssertUniversalJobLink(links, bookingParty, loadList.PK, loadList.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");

			links.Single(l => l.UCL_ParentID == loadList.PK).UCL_SourceType = nameof(DataContextType.LandTransportConsignmentConsol);
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var newLoadList = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty, Logger, newUniversalFactory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			newUniversalFactory.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same LoadList.", loadList.PK, newLoadList.PK);

			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only two universal links being created for this load list.", 2, linksInNewFactory.Count(l => l.UCL_ParentID == newLoadList.PK));
			AssertUniversalJobLink(linksInNewFactory, bookingParty, loadList.PK, loadList.TablePrefix, DataContextType.LandTransportConsignmentConsol, "C1000000", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(linksInNewFactory, bookingParty, newLoadList.PK, newLoadList.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_LinkWithDifferentBookingParty()
		{
			var bookingParty1 = Data.Orgs.INTHEMSYD;
			var bookingParty2 = Data.Orgs.CRAHOLSYD;
			var consignmentDataObject = Data.CreateShipmentWithPackages("CONID123", new[] { "PKG-1", "PKG-2" });
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var plannedPackageStates = receiveConsignment.PackageStates.Select(p => DataObjectReader.GetColumnIndexerFromRow(p)).ToArray();
			var consol = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consol, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consol;

			var loadListWithoutBookingParty = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), null, Logger, Factory, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var linksWithoutBookingParty = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("There must not be any universal links being created for this load list.", 0, linksWithoutBookingParty.Count(l => l.UCL_ParentID == loadListWithoutBookingParty.PK));

			var universalFactoryForBookingParty = new UniversalObjectFactory();
			var loadListWithBookingParty = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty1, Logger, universalFactoryForBookingParty, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			universalFactoryForBookingParty.SaveForTesting();

			var linksWithBookingParty = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this load list.", 1, linksWithBookingParty.Count(l => l.UCL_ParentID == loadListWithBookingParty.PK));
			AssertUniversalJobLink(linksWithBookingParty, bookingParty1, loadListWithBookingParty.PK, loadListWithBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");

			var universalFactoryForAnotherBookingParty = new UniversalObjectFactory();
			var tempCompany = universalFactoryForAnotherBookingParty.New<GlbCompany>();
			tempCompany.GC_Code = "FWD";
			consol.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var loadListWithAnotherBookingParty = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), bookingParty2, Logger, universalFactoryForAnotherBookingParty, new List<WhsItemDispatchLoadList>()).ReadIntoBusinessObject();
			universalFactoryForAnotherBookingParty.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same LoadList.", loadListWithBookingParty.PK, loadListWithAnotherBookingParty.PK);

			var linksWithAnotherBookingParty = universalFactoryForAnotherBookingParty.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only two universal links being created for this load list.", 2, linksWithAnotherBookingParty.Count(l => l.UCL_ParentID == loadListWithAnotherBookingParty.PK));
			AssertUniversalJobLink(linksWithAnotherBookingParty.Where(l => l.UCL_CompanyCode == "EDI").ToArray(), bookingParty1, loadListWithBookingParty.PK, loadListWithBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(linksWithAnotherBookingParty.Where(l => l.UCL_CompanyCode == "FWD").ToArray(), bookingParty2, loadListWithAnotherBookingParty.PK, loadListWithAnotherBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "FWD");
		}

		#endregion

		public void TestPopulateBizO_MatchingLoadListIsAwaitingForwardingChanges()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			dll.WDL_IsAwaitingForwardingChanges = true;
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			var dllAfterReimport = reader.ReadIntoBusinessObject();
			AssertEquals("Awaiting Forwarding Changes should be false after import", false, dllAfterReimport.WDL_IsAwaitingForwardingChanges);
			AssertEquals("DLL should be same", dll.PK, dllAfterReimport.PK);

			var logs = newFactory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dllAfterReimport.PK));
			var log = logs.Where(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).Single();
			AssertNotNull("Should have Status Updated log", log);
			AssertEquals("Log should be same as expected", "|TYP=CANCEL STOP LOAD|WHS=TWH|JOB=C123|RFN=C123", log.SL_Reference);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsDeactivated_NoChangesToPackages()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			dll.WDL_IsActive = false;
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			WhsItemDispatchLoadList dllAfterReimport = null;
			AssertNoExceptionThrown("Matching DLL is deactivated but should not throw.", () => dllAfterReimport = reader.ReadIntoBusinessObject());
			AssertNotEquals("The load list should not be changed if Packages were not modified. ", dll.PK, dllAfterReimport.PK);
			AssertEquals("The load list should not be closed if Packages were not modified.", false, dllAfterReimport.WDL_CompleteTime.IsValid);
			AssertEquals("The load list should not be changed if Packages were not modified.", true, dllAfterReimport.WDL_IsActive);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsDeactivated_ShouldCreateNewLoadListIfAddingPackages()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			dll.WDL_IsActive = false;
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			// Add a package that is not on the load list
			var packageStates = newFactory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK, receiveConsignment.PackageStates[1].PK })).ToArray();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(Data.HeaderDataObject, newFactory, packageStates, new List<ContainerByDLLDTO>(), null);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>(), finder);
			WhsItemDispatchLoadList dllAfterReimport = null;
			AssertNoExceptionThrown("Matching DLL is deactivated but should not throw.", () => dllAfterReimport = reader.ReadIntoBusinessObject());
			AssertNotEquals("Don't found deactivated Load list, create a new load list.", dll.PK, dllAfterReimport.PK);
			AssertEquals("The new load list should not be closed", false, dllAfterReimport.WDL_CompleteTime.IsValid);
			AssertEquals("The new load list should not be changed", true, dllAfterReimport.WDL_IsActive);
			AssertContains(@"The following packages have been attached to Load List DLL00000001:
Package        RCN                      DCN
PKG-1          CONID123 (RC00000001)    -
PKG-2          CONID123 (RC00000001)    -", Logger.Logs);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsDeactivated_ShouldCreateNewLoadListIfRemovingPackages()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			var dll = CreateLoadList(Data.Warehouse, "C123");
			dll.WDL_IsActive = false;
			var packageState = SetupPackageState(receiveConsignment.PackageStates.First(p => p.Package.KP_PackageID == "PKG-1"), dll, rtu);
			// Add a package that is excluded from the import.
			var packageStateToRemove = SetupPackageState(receiveConsignment.PackageStates.First(p => p.Package.KP_PackageID == "PKG-2"), dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var finder = new WhsTransitPackageStateBusinessObjectFinderForDLL(Data.HeaderDataObject, newFactory, packageStates, new List<ContainerByDLLDTO>(), null);
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, Array.Empty<Container>(), Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>(), finder);
			WhsItemDispatchLoadList dllAfterReimport = null;
			AssertNoExceptionThrown("Matching DLL is deactivated but should not throw.", () => dllAfterReimport = reader.ReadIntoBusinessObject());
			AssertNotEquals("Don't find deactivated Load list, create a new load list.", dll.PK, dllAfterReimport.PK);
			AssertEquals("The new load list should not be closed", false, dllAfterReimport.WDL_CompleteTime.IsValid);
			AssertEquals("The new load list should active", true, dllAfterReimport.WDL_IsActive);
			AssertContains(@"The following packages have been detached from their Load Lists:
Package        Load List      RCN                      DCN
PKG-1          (C123)         CONID123 (RC00000001)    -", Logger.Logs);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsNotCompleted_ShouldNotRejectIfLoadCompleteAndPackagesModified()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			var container = new Container { Link = 1, ContainerNumber = "CNT-1" };
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			foreach (var packline in consignmentDataObject.PackingLineCollection)
			{
				packline.ContainerLink = 1;
			}
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, containerID: "CNT-1");
			var dll = CreateLoadList(Data.Warehouse, "C123");
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.FinishLoading(dtu);
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			// Add a package that is not on the load list
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK, receiveConsignment.PackageStates[1].PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			AssertNoExceptionThrown("Should not reject a load list if its DTUs are load complete and its packages are modified but DLL not complete.", () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsCompleted_ShouldNotRejectIfLoadCompleteAndPackagesModified()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			var container = new Container { Link = 1, ContainerNumber = "CNT-1" };
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			foreach (var packline in consignmentDataObject.PackingLineCollection)
			{
				packline.ContainerLink = 1;
			}
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, containerID: "CNT-1");
			var dll = CreateLoadList(Data.Warehouse, "C123");
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.FinishLoading(dtu);
			Helper.CompleteLoadList(dll, dtu.WDH_LoadCompleteTime);
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			// Add a package that is not on the load list
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK, receiveConsignment.PackageStates[1].PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			AssertExceptionThrown<DataObjectReadFailureException>("Should reject a load list if its DTUs are load complete and its packages are modified.",
				$"Matching Load List {dll.WDL_JobID} is already completed and its packages cannot be modified.",
				() => reader.ReadIntoBusinessObject());
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingLoadListIsCompleted_ShouldNotRejectIfLoadCompleteButPackagesUnchanged()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			var container = new Container { Link = 1, ContainerNumber = "CNT-1" };
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			foreach (var packline in consignmentDataObject.PackingLineCollection)
			{
				packline.ContainerLink = 1;
			}
			Logger.TopLevelDataObject = consol;

			var location = GetLocation(Data.Warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("R123", Data.Warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, containerID: "CNT-1");
			var dll = CreateLoadList(Data.Warehouse, "C123");
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.FinishLoading(dtu);
			var packageState = SetupPackageState(receiveConsignment.PackageStates[0], dll, rtu);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.PK, new[] { packageState.PK })).ToArray();
			var reader = new WhsItemDispatchLoadListDataObjectReader(Data.Warehouse, consol, new Container[] { container }, Array.Empty<PackingLine>(), Data.Orgs.INTHEMSYD, Logger, newFactory, new List<WhsItemDispatchLoadList>());
			AssertNoExceptionThrown("Should not reject a completed LoadList/RTU if packages are unchanged.", () => reader.ReadIntoBusinessObject());
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
