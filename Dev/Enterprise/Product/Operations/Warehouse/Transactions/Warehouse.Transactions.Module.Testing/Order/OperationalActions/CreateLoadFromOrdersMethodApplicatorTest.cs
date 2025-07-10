using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CreateLoadsFromOrdersMethodApplicator))]
	class CreateLoadFromOrdersMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestCreateLoadsFromOrdersMethodApplicator_Validation

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_Valid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var transportCo = Helper.CreateClient("TransportCo");
			order.TransportCoPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("Precondition.", true, order.TransportCoPK.IsValid);
			AssertEquals("Precondition.", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals("Precondition.", false, order.WD_PL_NKCarrierServiceLevel.IsEmpty);
			AssertNotEquals("Precondition.", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);
			AssertNotEquals("Precondition.", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertNotEquals("Precondition.", DocketStatus.Codes.Finalised, order.WD_DocketStatus);

			var expectedLogText = "INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).";
			ApplyApplicatorAssertSummary(new[] { order }, "", expectedLogText);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_TransportCo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("Precondition.", false, order.TransportCoPK.IsValid);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - does not have a Transport Company.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_CarrierServiceLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var transportCo = Helper.CreateClient("TransportCo");
			order.TransportCoPK = transportCo.PK;
			AssertEquals("Precondition.", true, order.WD_PL_NKCarrierServiceLevel.IsEmpty);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - does not have a Carrier Service Level.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_DefaultDockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order.Warehouse.WW_DefaultOutboundDockDoor = ZGuid.Empty;
			AssertEquals("Precondition.", true, order.Warehouse.WW_DefaultOutboundDockDoor.IsEmpty);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - does not have a Default Outbound Dock Door on the Warehouse.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_AlreadyAssignedToLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var transportCo = Helper.CreateClient("TransportCo");
			order.TransportCoPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation);
			load.WLO_JobID = "WL00000001";
			order.WD_WLO_PlannedLoad = load.PK;
			AssertEquals("Precondition.", false, order.WD_WLO_PlannedLoad.IsEmpty);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - is already assigned to a load. If you are trying to assign a load to a partially loaded order, please do it on the Load desktop module.";
			ApplyApplicatorAssertSummary(new[] { orderInNewFactory }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_AlreadyAssignedToLoad_AttachedToLoadThroughPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var transportCo = Helper.CreateClient("TransportCo");
			order.TransportCoPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			load.WLO_JobID = "WL00000001";
			Factory.Save();

			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Precondition.", true, orderInNewFactory.WD_WLO_PlannedLoad.IsEmpty);
			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - is already assigned to a load. If you are trying to assign a load to a partially loaded order, please do it on the Load desktop module.";
			ApplyApplicatorAssertSummary(new[] { orderInNewFactory }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.WD_DocketStatus = DocketStatus.Codes.Held;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("Precondition.", DocketStatus.Codes.Held, order.WD_DocketStatus);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - has been held and cannot be loaded.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_Cancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.CancelReactivateDocket();
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("Precondition.", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - is canceled and cannot be loaded.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_CancelledEvent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.Warehouse.WW_IsVirtualWarehouse = true;
			order.Logs.AddNew(Events.Cancelled);
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - is canceled and cannot be loaded.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_Validation_Finalized()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			Factory.Save();

			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("Precondition.", false, order.WD_FinalisedDate.IsEmpty);

			var expectedLogText = "WARNING: Warehouse Order W00000002 [HL W00000002] - is finalized and cannot be loaded.";
			ApplyApplicatorAssertSummary(new[] { order }, expectedLogText, "", false);
		}

		#endregion

		#region TestCreateLoadsFromOrdersMethodApplicator_GroupBy

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_AllSame()
		{
			var orders = CreateOrders(5);
			AssertEquals("Precondition.", 5, orders.Length);

			var expectedLogText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000004 [HL W00000004] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000005 [HL W00000005] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.";
			ApplyApplicatorAssertSummary(orders, "", expectedLogText);

			AssertEquals(true, orders.All(o => o.WD_WLO_PlannedLoad.IsValid));
			AssertEquals(1, Factory.Load<WhsLoad>(new ZQuery()).Length);
			var defaultOutboundDockDoor = orders.Select(o => o.Warehouse.WW_DefaultOutboundDockDoor).Distinct().Single();
			var transportCoPK = orders.Select(o => o.TransportCoPK).Distinct().Single();
			var carrierServiceLevel = orders.Select(o => o.WD_PL_NKCarrierServiceLevel).Distinct().Single();
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, transportCoPK);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_AllSame_RunAgainWithoutClosingOperationalAction()
		{
			var orders = CreateOrders(2).OrderBy(o => o.WD_DocketID).ToArray();
			AssertEquals("Precondition.", 2, orders.Length);

			var actionSupporter = new OrderOperationalActionSupporter();
			var context = new OperationalActionContext(actionSupporter, "Whatever");
			var action = Factory.New<OperationalAction>();
			action.Context = context;
			Factory.Save();

			RunOperationalAction(new[] { orders[0].PK }, "INFO: Starting Section: Create Loads from Orders ...");
			RunOperationalAction(new[] { orders[1].PK }, "INFO: Starting Section: Create Loads from Orders ...");

			AssertEquals(true, orders.All(o => o.WD_WLO_PlannedLoad.IsValid));
			var loads = Factory.Load<WhsLoad>(new ZQuery());
			AssertEquals(2, loads.Length);
			var defaultOutboundDockDoor = orders.Select(o => o.Warehouse.WW_DefaultOutboundDockDoor).Distinct().Single();
			var transportCoPK = orders.Select(o => o.TransportCoPK).Distinct().Single();
			var carrierServiceLevel = orders.Select(o => o.WD_PL_NKCarrierServiceLevel).Distinct().Single();
			AssertEquals(2, loads.Count(l => l.WLO_PL_NKCarrierServiceLevel == carrierServiceLevel && l.WLO_WL_PlannedDockDoor == defaultOutboundDockDoor && l.WLO_OH_TransportCompany == transportCoPK));

			void RunOperationalAction(ZGuid[] orderPKs, string expectedLog)
			{
				var log = new DummyOperationalActionLog();
				var selectedOrders = new MockTargetRecordSelectionForTest(new SelectedRecords(orderPKs));
				var runner = new OperationalActionRunner(action, context.Supporter.RootType, selectedOrders) { RunOnAllMatchingRecords = true };
				runner.MethodApplicators.Add(new CreateLoadsFromOrdersMethodApplicator(Factory));
				runner.Run(log, Factory);
				AssertMultilineASCIIEquals(expectedLog, log.MessagesString());
			}
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_TransportCompany()
		{
			var orders = CreateOrders(5);
			AssertEquals("Precondition.", 5, orders.Length);
			var otherTransportCo = Helper.CreateClient("otherTranCo");
			orders[3].TransportCoPK = otherTransportCo.PK;
			Factory.Save();

			var expectedLogText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000004 [HL W00000004] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000005 [HL W00000005] - Attached to load [HL WL00000002] for the Transport Company 'otherTranCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.";
			ApplyApplicatorAssertSummary(orders, "", expectedLogText);

			AssertEquals(true, orders.All(o => o.WD_WLO_PlannedLoad.IsValid));
			AssertEquals(2, Factory.Load<WhsLoad>(new ZQuery()).Length);
			var defaultOutboundDockDoor = orders.Select(o => o.Warehouse.WW_DefaultOutboundDockDoor).Distinct().Single();
			var transportCoPK = orders[0].TransportCoPK;
			var carrierServiceLevel = orders.Select(o => o.WD_PL_NKCarrierServiceLevel).Distinct().Single();
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, transportCoPK);
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, otherTransportCo.PK);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_ServiceLevel()
		{
			var orders = CreateOrders(5);
			AssertEquals("Precondition.", 5, orders.Length);
			orders[3].WD_PL_NKCarrierServiceLevel = "SV1";

			var expectedLogText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000004 [HL W00000004] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000005 [HL W00000005] - Attached to load [HL WL00000002] for the Transport Company 'TransportCo' Carrier Service Level 'SV1' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
";

			ApplyApplicatorAssertSummary(orders, "", expectedLogText);

			AssertEquals(true, orders.All(o => o.WD_WLO_PlannedLoad.IsValid));
			AssertEquals(2, Factory.Load<WhsLoad>(new ZQuery()).Length);
			var defaultOutboundDockDoor = orders.Select(o => o.Warehouse.WW_DefaultOutboundDockDoor).Distinct().Single();
			var transportCoPK = orders.Select(o => o.TransportCoPK).Distinct().Single();
			AssertOnlyOneWhsLoadExists("STD", defaultOutboundDockDoor, orders[0].TransportCoPK);
			AssertOnlyOneWhsLoadExists("SV1", defaultOutboundDockDoor, transportCoPK);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_Mix()
		{
			var orders = CreateOrders(10);
			var whs1 = orders[0].Warehouse;
			var whs2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			Factory.Save();

			AssertEquals("Precondition.", 10, orders.Length);
			orders[3].WD_PL_NKCarrierServiceLevel = "SV1";
			var otherTransportCo = Helper.CreateClient("otherTranCo");
			orders[5].TransportCoPK = otherTransportCo.PK;
			orders[9].WD_WW_Whs = whs2.PK;

			var expectedLogText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000004 [HL W00000004] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000005 [HL W00000005] - Attached to load [HL WL00000002] for the Transport Company 'TransportCo' Carrier Service Level 'SV1' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000007 [HL W00000007] - Attached to load [HL WL00000003] for the Transport Company 'otherTranCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000008 [HL W00000008] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000009 [HL W00000009] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000010 [HL W00000010] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000011 [HL W00000011] - Attached to load [HL WL00000004] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse W2' (load created).";

			ApplyApplicatorAssertSummary(orders, "", expectedLogText);

			AssertEquals(true, orders.All(o => o.WD_WLO_PlannedLoad.IsValid));
			var whsLoads = Factory.Load<WhsLoad>(new ZQuery());
			AssertEquals(4, whsLoads.Length);
			var defaultOutboundDockDoor = orders[0].Warehouse.WW_DefaultOutboundDockDoor;
			var transportCoPK = orders[0].TransportCoPK;
			AssertOnlyOneWhsLoadExists("STD", defaultOutboundDockDoor, transportCoPK);
			AssertOnlyOneWhsLoadExists("SV1", defaultOutboundDockDoor, transportCoPK);
			AssertOnlyOneWhsLoadExists("STD", defaultOutboundDockDoor, otherTransportCo.PK);
			AssertOnlyOneWhsLoadExists("STD", whs2.WW_DefaultOutboundDockDoor, transportCoPK);
		}

		#region TestCreateLoadsFromOrdersMethodApplicator_DBHits

		public void TestCreateLoadsFromOrdersMethodApplicator_DBHits()
		{
			CreateOrders(20);

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
			};

			var orderInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(orderInNewFactory, true);
			}
			AssertEquals(1, Factory.Load<WhsLoad>(new ZQuery()).Length);
		}

		#endregion

		public void TestCreateLoadsFromOrdersMethodApplicator_GroupBy_OverBatchSize()
		{
			var orders = CreateOrders(10).OrderBy(o => o.PK).OrderBy(o => o.WD_DocketID).ToArray();
			var otherTransportCo1 = Helper.CreateClient("Company 1");
			orders[5].TransportCoPK = otherTransportCo1.PK;
			var otherTransportCo2 = Helper.CreateClient("Company 2");
			orders[9].TransportCoPK = otherTransportCo2.PK;
			Factory.Save();
			AssertEquals("Precondition.", 10, orders.Length);
			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var actionSupporter = new OrderOperationalActionSupporter();
				var context = new OperationalActionContext(actionSupporter, "Whatever");
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				Factory.Save();

				var selectedOrders = new MockTargetRecordSelectionForTest(new SelectedRecords(orders.Select(o => o.PK).ToArray()));
				var runner = new OperationalActionRunner(action, context.Supporter.RootType, selectedOrders) { RunOnAllMatchingRecords = true };
				var applicator = new CreateLoadsFromOrdersMethodApplicator(Factory);
				runner.MethodApplicators.Add(applicator);
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
					factory.Save();
					applicator.SummaryLog(dummyLog);
				});

				AssertMultilineASCIIEquals(@"
INFO: Starting Batch 1 of 5: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Starting Batch 2 of 5: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000004 [HL W00000004] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000005 [HL W00000005] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Starting Batch 3 of 5: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000007 [HL W00000007] - Attached to load [HL WL00000002] for the Transport Company 'Company 1' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Starting Batch 4 of 5: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000008 [HL W00000008] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000009 [HL W00000009] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Starting Batch 5 of 5: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000010 [HL W00000010] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Warehouse Order W00000011 [HL W00000011] - Attached to load [HL WL00000003] for the Transport Company 'Company 2' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).", dummyLog.MessagesString());
			}

			var whsLoads = Factory.Load<WhsLoad>(new ZQuery());
			AssertEquals(3, whsLoads.Length);
			var defaultOutboundDockDoor = orders[0].Warehouse.WW_DefaultOutboundDockDoor;
			var transportCoPK = orders[0].TransportCoPK;
			var carrierServiceLevel = orders.Select(o => o.WD_PL_NKCarrierServiceLevel).Distinct().Single();
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, transportCoPK);
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, otherTransportCo1.PK);
			AssertOnlyOneWhsLoadExists(carrierServiceLevel, defaultOutboundDockDoor, otherTransportCo2.PK);
		}

		#endregion

		#region TestCreateLoadsFromOrdersMethodApplicator_ConsolidatedOrders

		public void TestCreateLoadsFromOrdersMethodApplicator_ConsolidatedOrders_NotAllConsolidatedOrdersAreSelected()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var (order1, order2, handlingUnitPackage) = PrepareConsolidatedOrders(data, packingHelper);

			var transportCo = Helper.CreateClient("TransportCo");
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			order1.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order2.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var expectedLogText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
WARNING: Warehouse Order W00000003 [HL W00000003] - was automatically assigned to Load as it was consolidated with Order [HL W00000002].";

			ApplyApplicatorAssertSummary(new[] { order1 }, "", expectedLogText, saveFactoryOnSuccess: true);
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsLoads = newFactory.Load<WhsLoad>(new ZQuery());
			AssertEquals(1, whsLoads.Length);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order1.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order2.PK).WD_WLO_PlannedLoad);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_ConsolidatedOrders_AllConsolidatedOrdersAreSelected()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var (order1, order2, handlingUnitPackage) = PrepareConsolidatedOrders(data, packingHelper);

			var transportCo = Helper.CreateClient("TransportCo");
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			order1.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order2.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var expectedSummaryText = @"
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'."
			;

			ApplyApplicatorAssertSummary(new[] { order1, order2 }, "", expectedSummaryText, saveFactoryOnSuccess: true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsLoads = newFactory.Load<WhsLoad>(new ZQuery());
			AssertEquals(1, whsLoads.Length);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order1.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order2.PK).WD_WLO_PlannedLoad);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_ConsolidatedOrders_InDifferentBatches()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var (order1, order2, handlingUnitPackage) = PrepareConsolidatedOrders(data, packingHelper);
			var normalOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O0", data.Part1, 1m);

			var transportCo = Helper.CreateClient("TransportCo");
			normalOrder.TransportCoPK = transportCo.PK;
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			normalOrder.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order1.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order2.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var actionSupporter = new OrderOperationalActionSupporter();
				var context = new OperationalActionContext(actionSupporter, "Whatever");
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				Factory.Save();

				var selectedOrders = new MockTargetRecordSelectionForTest(new SelectedRecords(new ZGuid[] { normalOrder.PK, order1.PK, order2.PK }));
				var runner = new OperationalActionRunner(action, context.Supporter.RootType, selectedOrders) { RunOnAllMatchingRecords = true };
				var applicator = new CreateLoadsFromOrdersMethodApplicator(Factory);
				runner.MethodApplicators.Add(applicator);
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
					factory.Save();
					applicator.SummaryLog(dummyLog);
				});

				AssertMultilineASCIIEquals(@"
INFO: Starting Batch 1 of 2: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000006 [HL W00000006] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
WARNING: Warehouse Order W00000003 [HL W00000003] - was automatically assigned to Load as it was consolidated with Order [HL W00000002].
INFO: Starting Batch 2 of 2: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000003 [HL W00000003] - was already processed.", dummyLog.MessagesString());
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsLoads = newFactory.Load<WhsLoad>(new ZQuery());
			AssertEquals(1, whsLoads.Length);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(normalOrder.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order1.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order2.PK).WD_WLO_PlannedLoad);
		}

		public void TestCreateLoadsFromOrdersMethodApplicator_ConsolidatedOrders_InDifferentBatches_DoesnotShowRedundantLogs()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var (order1, order2, handlingUnitPackage) = PrepareConsolidatedOrders(data, packingHelper);

			var conLocation = data.Whs1.FindLocation("A-3");
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			order3.WD_UseDirectedPackingConsolidation = true;
			var pick3 = Helper.CreatePickNew(order3);
			var pickLine3 = pick3.GetAllPickLines().Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine3.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer3 = pick3.Transfers[0];
			var transferLine3 = pick3.Transfers[0].Lines[0];
			var transferPickLine3 = pick3.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine3.WE_WL);

			transferLine3.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine3.WE_WL);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg3, pickLine3);
			packingHelper.PackHandlingUnit(handlingUnitPackage, pkg3, handlingUnitPackage);

			Factory.Save();

			var transportCo = Helper.CreateClient("TransportCo");
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			order3.TransportCoPK = transportCo.PK;
			order1.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order2.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			order3.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var actionSupporter = new OrderOperationalActionSupporter();
				var context = new OperationalActionContext(actionSupporter, "Whatever");
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				Factory.Save();

				var selectedOrders = new MockTargetRecordSelectionForTest(new SelectedRecords(new ZGuid[] { order1.PK, order2.PK, order3.PK }));
				var runner = new OperationalActionRunner(action, context.Supporter.RootType, selectedOrders) { RunOnAllMatchingRecords = true };
				var applicator = new CreateLoadsFromOrdersMethodApplicator(Factory);
				runner.MethodApplicators.Add(applicator);
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
					factory.Save();
					applicator.SummaryLog(dummyLog);
				});

				AssertMultilineASCIIEquals(@"
INFO: Starting Batch 1 of 2: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000002 [HL W00000002] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1' (load created).
WARNING: Warehouse Order W00000006 [HL W00000006] - was automatically assigned to Load as it was consolidated with Order [HL W00000002].
INFO: Warehouse Order W00000003 [HL W00000003] - Attached to load [HL WL00000001] for the Transport Company 'TransportCo' Carrier Service Level 'STD' for the Dock Door Location 'DOCKDOOR' Warehouse 'Warehouse 1'.
INFO: Starting Batch 2 of 2: ...
INFO: Starting Section: Create Loads from Orders ...
INFO: Warehouse Order W00000006 [HL W00000006] - was already processed.", dummyLog.MessagesString());
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsLoads = newFactory.Load<WhsLoad>(new ZQuery());
			AssertEquals(1, whsLoads.Length);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order1.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order2.PK).WD_WLO_PlannedLoad);
			AssertEquals(whsLoads[0].PK, newFactory.Load<WhsDocket>(order3.PK).WD_WLO_PlannedLoad);
		}

		(WhsOrder order1, WhsOrder order2, PkgPackage handlingUnitPackage) PrepareConsolidatedOrders(TestDataSimpleEnvironment data, PackingTestHelper packingHelper)
		{
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer1 = pick1.Transfers[0];
			var transfer2 = pick2.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer1);
			AssertNotNull("Precondition: Transfer Created.", transfer2);
			var transferLine1 = pick1.Transfers[0].Lines[0];
			var transferPickLine1 = pick1.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine1.WE_WL);
			var transferLine2 = pick2.Transfers[0].Lines[0];
			var transferPickLine2 = pick2.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine2.WE_WL);

			transferLine1.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine1.WE_WL);
			transferLine2.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine2.WE_WL);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg2, handlingUnitPackage1);
			Factory.Save();

			return (order1, order2, handlingUnitPackage1);
		}

		#endregion

		#region Helper Methods/class

		WhsOrder[] CreateOrders(int numberOfOrders)
		{
			var orders = new List<WhsOrder>();
			var data = new TestDataSimpleEnvironment(Factory);
			var transportCo = Helper.CreateClient("TransportCo");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, numberOfOrders);
			Factory.Save();

			for (int i = 0; i < numberOfOrders; i++)
			{
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", data.Part1, 1m);
				order.TransportCoPK = transportCo.PK;
				order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
				orders.Add(order);
			}

			Factory.Save();
			return orders.ToArray();
		}

		void AssertOnlyOneWhsLoadExists(ZString carrierServiceLevel, ZGuid defaultOutboundDockDoor, ZGuid transportPK)
		{
			var loads = Factory.Load<WhsLoad>(new ZQuery());
			AssertEquals(1, loads.Count(l => l.WLO_PL_NKCarrierServiceLevel == carrierServiceLevel && l.WLO_WL_PlannedDockDoor == defaultOutboundDockDoor && l.WLO_OH_TransportCompany == transportPK));
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		void ApplyApplicatorAssertSummary(WhsOrder[] targets, string expectedText, string expectedSummaryText, bool saveFactoryOnSuccess = true)
		{
			ApplyApplicator(targets, expectedText + "\r\n<-- Summary -->\r\n" + expectedSummaryText, saveFactoryOnSuccess);
		}

		#endregion
	}
}
