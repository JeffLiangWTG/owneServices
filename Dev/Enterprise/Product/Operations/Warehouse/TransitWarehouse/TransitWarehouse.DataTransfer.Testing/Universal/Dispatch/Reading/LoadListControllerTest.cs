using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Dispatch.Reading
{
	class LoadListControllerTest : TransitUniversalTestCase
	{
		#region TestLoadList_ReadyToStage_DoNotUpdatePackageStateStatue

		public void TestLoadList_ReadyToStage_DoNotUpdatePackageStateStatus()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var booked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGBKD", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var arrived_NotInDLLStagingLocation = Helper.CreatePackageState(rcn, 1, "PKG", "PKGARV", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var putaway_NotInDLLStagingLocation = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPPIC", TransitWarehouseStatuses.Codes.Picked, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var loaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPFLO", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPDEP", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			var finalized = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPFIN", TransitWarehouseStatuses.Codes.Finalized, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			arrived_NotInDLLStagingLocation.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
			putaway_NotInDLLStagingLocation.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			picked.WPS_WL_LastLocation = loadList.WDL_WL_StagingLocation;
			loaded.WPS_WL_LastLocation = loadList.WDL_WL_StagingLocation;
			departed.WPS_WL_LastLocation = loadList.WDL_WL_StagingLocation;
			finalized.WPS_WL_LastLocation = loadList.WDL_WL_StagingLocation;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.TRF, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, picked, outBoundLocation, warehouse.DefaultLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals($@"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");

				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Booked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(booked.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(arrived_NotInDLLStagingLocation.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(putaway_NotInDLLStagingLocation.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Picked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(picked.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.FreightLoaded, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(loaded.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Departed, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(departed.PK).WPS_Status);
				AssertEquals("Package Status should not change", TransitWarehouseStatuses.Codes.Finalized, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(finalized.PK).WPS_Status);
			});
		}

		#endregion

		#region TestLoadList_ReadyToStage_UpdatePackageStatus

		public void TestLoadList_ReadyToStage_UpdatePackageStatus()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			cttPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			cttPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage2.WPS_WL_LastLocation = outBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage1, inBoundLocation, outBoundLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage2, warehouse.DefaultLocation, outBoundLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals(
					$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");

				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage2.PK).WPS_Status);
				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage2.PK).WPS_Status);
			});
		}

		#endregion

		#region TestLoadList_ReadyToStage_UpdateHandlingUnitStatus

		public void TestLoadList_ReadyToStage_UpdateHandlingUnitStatus()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop1 = Helper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu);

			var huJob1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage1 = Helper.CreateHandlingUnitPackage("HU1", huJob1, rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, handlingUnitPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, cttPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, cttPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);

			var huJobTop2 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop2 = Helper.CreateHandlingUnitPackage("HUT2", huJobTop2, rtu);

			var huJob2 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", huJob2, rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop2, handlingUnitPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop2);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop2, staPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop2);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, staPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop2);

			handlingUnitPackageTop1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			handlingUnitPackage1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			cttPackage1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			cttPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			handlingUnitPackageTop2.WPS_WL_LastLocation = inBoundLocation.PK;
			handlingUnitPackage2.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage2.WPS_WL_LastLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage1, warehouse.DefaultLocation, outBoundLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage2, warehouse.DefaultLocation, outBoundLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals(
					$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");

				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackageTop1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage2.PK).WPS_Status);

				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackageTop2.PK).WPS_Status);
				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackage2.PK).WPS_Status);
				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Arrived.", TransitWarehouseStatuses.Codes.Arrived, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage2.PK).WPS_Status);
			});
		}

		#endregion

		#region TestLoadList_ReadyToStage_DoNotUpdatePackageInAnotherDLL

		public void TestLoadList_ReadyToStage_DoNotUpdatePackageInAnotherDLL()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var loadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList2);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop1 = Helper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu, TransitWarehouseStatuses.Codes.Staged);

			var huJob1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage1 = Helper.CreateHandlingUnitPackage("HU1", huJob1, rtu, TransitWarehouseStatuses.Codes.Staged);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, handlingUnitPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, staPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage1, staPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);

			handlingUnitPackageTop1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			handlingUnitPackage1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals(
					$@"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");

				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackageTop1.PK).WPS_Status);
				AssertEquals("Package Status is not updated.", TransitWarehouseStatuses.Codes.Staged, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Putaway.", TransitWarehouseStatuses.Codes.Putaway, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage1.PK).WPS_Status);
				AssertEquals("Package Status is not updated.", TransitWarehouseStatuses.Codes.Staged, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(staPackage2.PK).WPS_Status);
			});
		}

		#endregion

		#region TestLoadList_ReadyToStage_HandlingUnitWithTransferLine

		public void TestLoadList_ReadyToStage_HandlingUnitWithTransferLine()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var picPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC1", TransitWarehouseStatuses.Codes.Picked, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var picPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC2", TransitWarehouseStatuses.Codes.Picked, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = Helper.CreatePackageHandlingUnit();
			var handlingUnit_ManualTransferTop1 = Helper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu, status: TransitWarehouseStatuses.Codes.Committed);

			var huJob1 = Helper.CreatePackageHandlingUnit();
			var handlingUnit_ManualTransfer = Helper.CreateHandlingUnitPackage("HU1", huJob1, rtu, status: TransitWarehouseStatuses.Codes.Committed);
			Helper.PackPackageIntoHandlingUnit(handlingUnit_ManualTransferTop1, handlingUnit_ManualTransfer, ZDateTimeOffset.Now, "ABC", handlingUnit_ManualTransferTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnit_ManualTransferTop1, cttPackage1, ZDateTimeOffset.Now, "ABC", handlingUnit_ManualTransferTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnit_ManualTransfer, cttPackage2, ZDateTimeOffset.Now, "ABC", handlingUnit_ManualTransferTop1);

			var huJobTop2 = Helper.CreatePackageHandlingUnit();
			var handlingUnitTop_Picked = Helper.CreateHandlingUnitPackage("HUT2", huJobTop2, rtu, status: TransitWarehouseStatuses.Codes.Picked);

			var huJob2 = Helper.CreatePackageHandlingUnit();
			var handlingUnit_Picked = Helper.CreateHandlingUnitPackage("HU2", huJob2, rtu, status: TransitWarehouseStatuses.Codes.Picked);
			Helper.PackPackageIntoHandlingUnit(handlingUnitTop_Picked, handlingUnit_Picked, ZDateTimeOffset.Now, "ABC", handlingUnitTop_Picked);
			Helper.PackPackageIntoHandlingUnit(handlingUnitTop_Picked, picPackage1, ZDateTimeOffset.Now, "ABC", handlingUnitTop_Picked);
			Helper.PackPackageIntoHandlingUnit(handlingUnit_Picked, picPackage2, ZDateTimeOffset.Now, "ABC", handlingUnitTop_Picked);

			handlingUnit_ManualTransferTop1.WPS_WL_LastLocation = outBoundLocation.PK;
			handlingUnit_ManualTransfer.WPS_WL_LastLocation = outBoundLocation.PK;
			cttPackage1.WPS_WL_LastLocation = outBoundLocation.PK;
			cttPackage2.WPS_WL_LastLocation = outBoundLocation.PK;

			handlingUnitTop_Picked.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			handlingUnit_Picked.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			picPackage1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			picPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var unFinalisedTransfer1 = Helper.CreateTransitTransfer(TransferTypes.Codes.TRF, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer1, cttPackage1, outBoundLocation, warehouse.DefaultLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer1, cttPackage2, outBoundLocation, warehouse.DefaultLocation);

			var unFinalisedTransfer2 = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH2", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer2, picPackage1, warehouse.DefaultLocation, outBoundLocation, pickTime: DateTime.Now, pickUser: "ABC");
			Helper.CreateTransitTransferLine(unFinalisedTransfer2, picPackage2, warehouse.DefaultLocation, outBoundLocation, pickTime: DateTime.Now, pickUser: "ABC");

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals(
					$@"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");

				AssertEquals("Package Status is updated to Committed To Transfer.", TransitWarehouseStatuses.Codes.Committed, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnit_ManualTransferTop1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Committed To Transfer.", TransitWarehouseStatuses.Codes.Committed, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnit_ManualTransfer.PK).WPS_Status);
				AssertEquals("Package Status is updated to Committed To Transfer.", TransitWarehouseStatuses.Codes.Committed, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Committed To Transfer.", TransitWarehouseStatuses.Codes.Committed, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(cttPackage2.PK).WPS_Status);

				AssertEquals("Package Status is updated to Picked.", TransitWarehouseStatuses.Codes.Picked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnitTop_Picked.PK).WPS_Status);
				AssertEquals("Package Status is updated to Picked.", TransitWarehouseStatuses.Codes.Picked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(handlingUnit_Picked.PK).WPS_Status);
				AssertEquals("Package Status is updated to Picked.", TransitWarehouseStatuses.Codes.Picked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(picPackage1.PK).WPS_Status);
				AssertEquals("Package Status is updated to Picked.", TransitWarehouseStatuses.Codes.Picked, newFactoryAfterStoppingLoadList.Load<WhsItemPackageState>(picPackage2.PK).WPS_Status);
			});
		}

		#endregion

		#region TestLoadList_NotReadyToStage

		public void TestLoadList_NotReadyToStage()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: false);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrived = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			putaway.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);
			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
			AssertEquals("", Logger.Logs);
		}

		#endregion

		#region TestLoadList_PickTransfers_PickedAndCommittedTransfers

		public void TestLoadList_PickTransfers_PickedAndCommittedTransfers()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			committed.WPS_WL_LastLocation = inWarehouseLocation.PK;
			committed.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			picked.WPS_WL_LastLocation = inWarehouseLocation.PK;
			picked.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			var transferLineForCommittedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, committed, inWarehouseLocation, outBoundLocation);
			var transferLineForPickedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, picked, inWarehouseLocation, outBoundLocation);
			transferLineForPickedPackage.WTF_PickTime = ZDateTimeOffset.Now;
			transferLineForPickedPackage.WTF_GS_NKPickUser = "DGF";
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertNull("Unpicked Transfer line should be deleted.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForCommittedPackage.PK));
				AssertNotNull("Picked transfer line should remain.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForPickedPackage.PK));
				AssertEquals("All unpicked transfer lines are deleted. Therefore transfer is finalised.",
					false, newFactoryAfterStoppingLoadList.Load<WhsItemTransferHeader>(unFinalisedTransfer.PK).WTH_IsFinalised);
				AssertEquals($"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		#endregion

		#region TestLoadList_PickTransfers_CommittedTransfersOnly

		public void TestLoadList_PickTransfers_CommittedTransfersOnly()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			committed.WPS_WL_LastLocation = inWarehouseLocation.PK;
			committed.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			var transferLineForCommittedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, committed, inWarehouseLocation, outBoundLocation);
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertNull("Unpicked Transfer line should be deleted.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForCommittedPackage.PK));
				AssertEquals("All unpicked transfer lines are deleted. Therefore transfer is finalized.",
					true, newFactoryAfterStoppingLoadList.Load<WhsItemTransferHeader>(unFinalisedTransfer.PK).WTH_IsFinalised);
				AssertEquals(
	$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		#endregion

		public void TestLoadList_PickTransfers_DepartedAndLoadedPackages()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			departed.WPS_WL_LastLocation = outBoundLocation.PK;
			departed.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var loaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			loaded.WPS_WL_LastLocation = outBoundLocation.PK;
			loaded.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			var transferLineForDepartedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, departed, inWarehouseLocation, outBoundLocation, DateTime.Now, "DGF", DateTime.Now, "DGF");
			var transferLineForLoadedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, loaded, inWarehouseLocation, outBoundLocation, DateTime.Now, "DGF", DateTime.Now, "DGF");
			Factory.SaveForTesting();
			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertNotNull("Departed transfer line must stay.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForDepartedPackage.PK));
				AssertNotNull("Freight loaded transfer line must stay.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForLoadedPackage.PK));
				AssertEquals("Transfer is finalised since all transfer lines completed.",
					true, newFactoryAfterStoppingLoadList.Load<WhsItemTransferHeader>(unFinalisedTransfer.PK).WTH_IsFinalised);
				AssertEquals(
	$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		public void TestLoadList_CrossDockTransfers_CommittedAndPickedTransferLines()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			committed.WPS_WL_LastLocation = inWarehouseLocation.PK;
			committed.WPS_WL_ReceiveLocation = inBoundLocation.PK;
			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			picked.WPS_WL_LastLocation = inBoundLocation.PK;
			picked.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.XDK, "TH1", warehouse.PK, false);
			var crossDockCommittedTransferLine = Helper.CreateTransitTransferLine(unFinalisedTransfer, committed, inBoundLocation, outBoundLocation);
			var crossDockPickedTransferLine = Helper.CreateTransitTransferLine(unFinalisedTransfer, picked, inBoundLocation, outBoundLocation, DateTime.Now, "DGF");
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();
			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertNull("Committed Transfer line must be deleted.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(crossDockCommittedTransferLine.PK));
				AssertNotNull("Picked Transfer line must remain.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(crossDockPickedTransferLine.PK));
				AssertEquals("Transfer is not finalised since there are some transfer lines in progress.",
					false, newFactoryAfterStoppingLoadList.Load<WhsItemTransferHeader>(unFinalisedTransfer.PK).WTH_IsFinalised);

				AssertEquals($"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		public void TestLoadList_CrossDockTransfers_CommittedTransferLinesOnly()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			committed.WPS_WL_LastLocation = inWarehouseLocation.PK;
			committed.WPS_WL_ReceiveLocation = inBoundLocation.PK;
			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.XDK, "TH1", warehouse.PK, false);
			var crossDockCommittedTransferLine = Helper.CreateTransitTransferLine(unFinalisedTransfer, committed, inBoundLocation, outBoundLocation);

			Factory.SaveForTesting();
			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertNull("Committed Transfer line must be deleted.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(crossDockCommittedTransferLine.PK));
				AssertEquals("Transfer is finalised since all incompleted transfers are deleted.",
					true, newFactoryAfterStoppingLoadList.Load<WhsItemTransferHeader>(unFinalisedTransfer.PK).WTH_IsFinalised);

				AssertEquals(
	$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		public void TestLoadListWithoutTransfers()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var arrived = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			putaway.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			committed.WPS_WL_LastLocation = inWarehouseLocation.PK;
			committed.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			picked.WPS_WL_LastLocation = inWarehouseLocation.PK;
			picked.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			Factory.SaveForTesting();
			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
				AssertEquals($"Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
				AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
			});
		}

		public void TestLoadList_PickTransfers_DepartedPackages()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");
			var inWarehouseLocation = warehouse.DefaultLocation;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var departed1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			departed1.WPS_WL_LastLocation = outBoundLocation.PK;
			departed1.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var departed2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			departed2.WPS_WL_LastLocation = outBoundLocation.PK;
			departed2.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var finalized = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.Finalized, rtu, dispatchConsignment: dcn, dispatchLoadList: loadList, dispatchUnit: dtu);
			finalized.WPS_WL_LastLocation = outBoundLocation.PK;
			finalized.WPS_WL_ReceiveLocation = inBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			var transferLineForDeparted1Package = Helper.CreateTransitTransferLine(unFinalisedTransfer, departed1, inWarehouseLocation, outBoundLocation, DateTime.Now, "DGF", DateTime.Now, "DGF");
			var transferLineForDeparted2Package = Helper.CreateTransitTransferLine(unFinalisedTransfer, departed2, inWarehouseLocation, outBoundLocation, DateTime.Now, "DGF", DateTime.Now, "DGF");
			var transferLineForFinalizedPackage = Helper.CreateTransitTransferLine(unFinalisedTransfer, finalized, inWarehouseLocation, outBoundLocation, DateTime.Now, "DGF", DateTime.Now, "DGF");

			Factory.SaveForTesting();
			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);
			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertEquals(true, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
			AssertNotNull("Departed transfer lines must stay.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForDeparted1Package.PK));
			AssertNotNull("Departed transfer lines must stay.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForDeparted2Package.PK));
			AssertNotNull("Finalized Package transfer lines must stay.", newFactoryAfterStoppingLoadList.Load<WhsItemTransferLine>(transferLineForFinalizedPackage.PK));

			AssertEquals($"Warning - Load List {loadList.WDL_JobID} cannot be stopped as it has already departed.", Logger.Logs);
		}

		#region TestLoadList_StopJob_AddsSuspendEvent

		public void TestLoadList_StopJob_AddsSuspendEvent_WhenImportingShipmentFromForwarding()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			cttPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			cttPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage2.WPS_WL_LastLocation = outBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage1, inBoundLocation, outBoundLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage2, warehouse.DefaultLocation, outBoundLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: true));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();
			AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
			AssertEquals(
				$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
			AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|DEP=Forwarder|WHS=TWH");
		}

		public void TestLoadList_StopJob_AddsSuspendEvent_WhenImportingShipmentFromLocalTransportRunSheet()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			cttPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			cttPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage2.WPS_WL_LastLocation = outBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage1, inBoundLocation, outBoundLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage2, warehouse.DefaultLocation, outBoundLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: true, isFromForwarding: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();
			AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
			AssertEquals(
				$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
			AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|DEP=Transport Company|WHS=TWH");
		}

		public void TestLoadList_StopJob_AddsSuspendEvent_WhenDataSourceIsMissing()
		{
			var warehouse = Data.Warehouse;
			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var cttPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var cttPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT2", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA1", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var staPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA2", TransitWarehouseStatuses.Codes.Staged, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			cttPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			cttPackage2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			staPackage1.WPS_WL_LastLocation = inBoundLocation.PK;
			staPackage2.WPS_WL_LastLocation = outBoundLocation.PK;

			var unFinalisedTransfer = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TH1", warehouse.PK, false);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage1, inBoundLocation, outBoundLocation);
			Helper.CreateTransitTransferLine(unFinalisedTransfer, cttPackage2, warehouse.DefaultLocation, outBoundLocation);

			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newUniversalFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, loadList.PK)).Single();
			var dllColumnIndexer = DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory);

			var suspendEventQuery = new ZQuery(StmALogSchema.SL_Parent, dllColumnIndexer[WhsItemDispatchLoadListSchema.Constants.PK]);
			suspendEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceSuspendedCode);
			var suspendEvents = newUniversalFactory.RowFactory.Load(StmALogSchema.Constants.TableName, suspendEventQuery);
			AssertEquals("SVS - Service Suspended event does not exist.", 0, suspendEvents.Length);

			LoadListController.StopLoadList(newUniversalFactory, Logger, loadListInNewFactory, CreateShipmentWithDataSource(isRunSheet: false, isFromForwarding: true, shouldIncludeDataSource: false));
			newUniversalFactory.SaveForTesting();

			var newFactoryAfterStoppingLoadList = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadListInNewFactoryAfterStoppingLoadList = newFactoryAfterStoppingLoadList.Load<WhsItemDispatchLoadList>(loadList.PK);
			var suspendEventAfterStoppingLoadList = loadListInNewFactoryAfterStoppingLoadList.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceSuspendedCode).Single();
			AssertEquals(false, loadListInNewFactoryAfterStoppingLoadList.WDL_IsReadyToStage);
			AssertEquals(
				$@"Information - Finalized transfer {unFinalisedTransfer.WTH_ReferenceNumber} when stopping Load List {loadListInNewFactoryAfterStoppingLoadList.WDL_JobID}.
Information - Load List {loadList.WDL_JobID} has been stopped.", Logger.Logs);
			AssertLoadListEvent(suspendEventAfterStoppingLoadList, loadListInNewFactoryAfterStoppingLoadList, false, Events.ServiceSuspendedCode, "DLL1|TYP=Load List|LOC=ZAJNB|FAC=CFS|RES=Modifying via UXML|WHS=TWH");
		}

		#endregion

		void AssertLoadListEvent(StmALog eventLog, WhsItemDispatchLoadList loadList, bool isCancelled, string eventCode, string reference)
		{
			AssertEquals(loadList.TableName, eventLog.SL_Table);
			AssertEquals(loadList.PK, eventLog.SL_Parent);
			AssertEquals(isCancelled, eventLog.IsCancelled);
			AssertEquals(eventCode, eventLog.SL_SE_NKEvent);
			AssertEquals(reference, eventLog.SL_Reference);
		}

		UniversalShipment CreateShipmentWithDataSource(bool isRunSheet, bool isFromForwarding, bool shouldIncludeDataSource = true)
		{
			var universalShipment = new UniversalShipment();
			string shipmentNumber = isFromForwarding ? "S1000000" : null;
			string landTransportRunSheet = !isRunSheet && !isFromForwarding ? "R1000000" : null;

			if (shouldIncludeDataSource)
			{
				Data.SetupNewDataContextWithDataSource(universalShipment, runSheet: landTransportRunSheet, shipmentNumber: shipmentNumber);
			}
			if (isRunSheet)
			{
				universalShipment.DataContext.AddDataSource(DataContextType.LocalTransportRunSheet, "R1000000");
			}

			return universalShipment;
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);
	}
}
