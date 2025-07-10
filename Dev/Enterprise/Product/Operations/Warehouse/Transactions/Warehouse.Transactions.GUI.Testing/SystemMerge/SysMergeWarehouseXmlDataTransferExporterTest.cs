using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using OrgHeaderDO = CargoWise.Database.TestFramework.ObjectModel.OrgHeader;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class SysMergeWarehouseXmlDataTransferExporterTest : WhsTestCaseWithFactory
	{
		#region TestAdapter

		public void TestAdapter()
		{
			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			AssertEquals("Adapter", typeof(SysMergeWarehouseInventoryValueObjectDataAdapter), exporter.Adapter_Exposed.GetType());
		}

		#endregion

		#region TestExportToFile

		public void TestExportToFile()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			exporter.DefaultFileName = "WhsXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830";
			var expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, exporter.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				exporter.PromptUserAndExport(new BusinessObject[] { data.Org1 });
				Assert("Xml file was exported", File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		#endregion

		#region TestPromptEserAndExport

		#region TestPromptUserAndExport_Receives

		public void TestPromptUserAndExport_Receives()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");

			var whs = Helper.CreateWarehouse("WHS", "A");

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client2, "P3");
			var part4 = Helper.CreateProduct(client2, "P4");

			var receive_ForAnotherClient = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part1, 10m);

			var receive_FullyPicked = Helper.CreateWhsReceive(client2, whs, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_FullyPicked, part2, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive_FullyPicked, part2, 5m);
			receive_FullyPicked.AllocateLocationsWithMock();
			receive_FullyPicked.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_FullyPicked);

			var receive_PartialyPicked = Helper.CreateWhsReceive(client2, whs, "R3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_PartialyPicked, part2, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive_PartialyPicked, part3, 10m);
			receive_PartialyPicked.AllocateLocationsWithMock();
			receive_PartialyPicked.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_PartialyPicked);

			var receive_FullyInStock = Helper.CreateWhsReceive(client2, whs, "R4", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_FullyInStock, part4, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive_FullyInStock, part4, 10m);
			receive_FullyInStock.AllocateLocationsWithMock();
			receive_FullyInStock.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_FullyInStock);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client2, whs);
			Helper.CreateWhsOrderLine(order, part2, 15m);
			Helper.CreateWhsOrderLine(order, part3, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { client2 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertEquals("Only 2 receives should be selected for export for client2.", 2, exporter.ElementsToExport_Exposed.Count);
			exporter.ElementsToExport_Exposed.Cast<WhsReceive>().Single(r => r.PK == receive_PartialyPicked.PK);
			exporter.ElementsToExport_Exposed.Cast<WhsReceive>().Single(r => r.PK == receive_FullyInStock.PK);
		}

		#endregion

		#region TestPromptUserAndExport_Receives_ManyClients

		[StressTest]
		public void TestPromptUserAndExport_Receives_ManyClients()
		{
			const int clientsToExportWithReceives = 10;
			const int extraClientsToMake = 25000;

			var whs = Helper.CreateWarehouse("WHS", "A");
			var clients = new List<OrgHeader>();

			for (var i = 0; i < clientsToExportWithReceives; i++)
			{
				var client = Helper.CreateClient($"CLIENT{i}");
				var part = Helper.CreateProduct(client, $"P{i}");

				var receive_FullyInStock = Helper.CreateWhsReceive(client, whs, $"R{i}", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive_FullyInStock, part, 10m, whs.DefaultLocation);
				receive_FullyInStock.FinaliseDocket();

				AssertIsFinalisedPrecondition(receive_FullyInStock);

				clients.Add(client);
			}
			Factory.Save();

			var orgHeaderDOs = Enumerable.Range(1, extraClientsToMake)
				.Select(i => new OrgHeaderDO($"OTHER-{i}"))
				.ToArray();

			TestConnection.ExecuteNonQuery(OrgHeaderDO.GetBulkInsertStatement(orgHeaderDOs));

			clients.AddRange(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgHeaderDOs.Select(o => o.PK))));
			AssertEquals("Precondition.", clientsToExportWithReceives + extraClientsToMake, clients.Count);

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(clients);
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}

			AssertEquals($"{clientsToExportWithReceives} receives should be selected for export.", clientsToExportWithReceives, exporter.ElementsToExport_Exposed.Count);
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedReceive

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(data, () => Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1"));
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedAdjustment

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(data, () => Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1"));
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedTransfer

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(data, () => Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1"));
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedOrder

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(data, () => Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1"));
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedWorkOrder

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinalisedWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(data, () => Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1"));
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs

		void TestPromptUserAndExport_CheckDataIsReadyToBeExported_UnfinaliseJobs(TestDataSimpleEnvironment data, Action setupData)
		{
			setupData();

			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { data.Org1 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertNull("Should detect that some data is not ready to be exported, so nothing should be exported.", exporter.ElementsToExport_Exposed);
			AssertEquals("Should show error.", "Please finalize all Warehouse Jobs before running System Merge.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_CanceledPick

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_CanceledPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.CancelPick();
			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { data.Org1 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertEquals("Canceled Pick should not stop export.", 1, exporter.ElementsToExport_Exposed.Count);
			AssertEquals(receive.PK, ((BusinessObject)exporter.ElementsToExport_Exposed[0]).PK);
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_FreeStore

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_FreeStore

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_FreeStore()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var client3 = Helper.CreateClient("CLIENT3");

			var whs = Helper.CreateWarehouse("WHS", "A");

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client3, "P3");

			Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs, "R2", part2, 10m);
			Helper.CreateWhsReceiveWithInventory(client3, whs, "R3", part3, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client1, whs, "OR1", part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, whs, "OR2", part2, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client3, whs, "OR3", part3, 10m);

			var pickUnFinalised = Helper.CreatePickNew(order1, order2);
			pickUnFinalised.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(order2);
			AssertEquals("Precondition - ensure that pick is NOT finalised.", false, pickUnFinalised.IsFinalised);

			Factory.Save();

			var pickFinalised = Helper.CreatePickNew(order3);
			pickFinalised.FinaliseAllOrders();
			pickFinalised.FinalisePick();
			AssertIsFinalisedPrecondition(order3);
			AssertIsFinalisedPrecondition(pickFinalised);

			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { client2, client3 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertNull("Should detect that some data is not ready to be exported, so nothing should be exported.", exporter.ElementsToExport_Exposed);
			AssertEquals("Should show error.", "Please finalize all Warehouse Jobs before running System Merge.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_FreeStore_UnfinalisedPick

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_FreeStore_UnfinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure pick is not finalised", false, pick.IsFinalised);
			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { data.Org1 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertNull("Should detect that some data is not ready to be exported, so nothing should be exported.", exporter.ElementsToExport_Exposed);
			AssertEquals("Should show error.", "Please finalize all Warehouse Jobs before running System Merge.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_Bonded

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_Bonded

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_Bonded()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var client3 = Helper.CreateClient("CLIENT3");

			var whs = Helper.CreateWarehouse("WHS", "A");
			whs.WW_IsVirtualWarehouse = true;

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client3, "P3");

			Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs, "R2", part2, 10m);
			Helper.CreateWhsReceiveWithInventory(client3, whs, "R3", part3, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client1, whs, "OR1", part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, whs, "OR2", part2, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client3, whs, "OR3", part3, 10m);

			var pickWithPickedOrders = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - ensure that order is NOT finalised.", false, order1.IsFinalised);
			AssertEquals("Precondition - ensure that order is NOT finalised.", false, order2.IsFinalised);
			AssertEquals("Precondition - ensure that pick is NOT finalised.", false, pickWithPickedOrders.IsFinalised);

			Factory.Save();

			var pickUnFinalised = Helper.CreatePickNew(order3);
			pickUnFinalised.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order3);
			AssertEquals("Precondition - ensure that pick is NOT finalised.", false, pickUnFinalised.IsFinalised);

			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { client2, client3 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertNull("Should detect that some data is not ready to be exported, so nothing should be exported.", exporter.ElementsToExport_Exposed);
			AssertEquals("Should show error.", "Please finalize all Warehouse Jobs before running System Merge.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestPromptUserAndExport_CheckDataIsReadyToBeExported_Bonded_UnfinalisePick

		public void TestPromptUserAndExport_CheckDataIsReadyToBeExported_Bonded_UnfinalisePick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure pick is not finalised", false, pick.IsFinalised);
			Factory.Save();

			var exporter = new SysMergeWarehouseInventoryXmlDataTransferExporterForTest();
			try
			{
				exporter.PromptUserAndExport(new OrgHeader[] { data.Org1 });
			}
			finally
			{
				DeleteIfExists(exporter.LastFileExported_Exposed);
			}
			AssertEquals("Pick are not required to be finalised for bonded warehouse, so should not stop export.", 1, exporter.ElementsToExport_Exposed.Count);
			AssertEquals(receive.PK, ((BusinessObject)exporter.ElementsToExport_Exposed[0]).PK);
		}

		#endregion

		#endregion

		#endregion

		#region SysMergeWarehouseXmlDataTransferExporterForTest class

		class SysMergeWarehouseInventoryXmlDataTransferExporterForTest : SysMergeWarehouseInventoryXmlDataTransferExporter
		{
			#region Adapter_Exposed

			public IValueObjectDataAdapter Adapter_Exposed => Adapter;

			#endregion

			#region AreSelectedElementsOkToExport

			protected override bool AreSelectedElementsOkToExport(IList selectedElements)
			{
				elementsToExport_Exposed = selectedElements;
				return base.AreSelectedElementsOkToExport(selectedElements);
			}

			public IList ElementsToExport_Exposed => elementsToExport_Exposed;
			IList elementsToExport_Exposed;

			#endregion

			#region DoExportToFile

			protected override bool DoExportToFile(IList selectedElements, IXmlDataTransferExporterGUI gui)
			{
				LastFileExported_Exposed = gui.UnmappedFile;
				return base.DoExportToFile(selectedElements, gui);
			}

			public ZString LastFileExported_Exposed { get; set; }

			#endregion
		}

		#endregion
	}
}
