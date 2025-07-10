using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class TransferFilterControlTest : WhsFilterControlDBHitsTestCase<WhsTransferCollection, TransferFilterBusinessObject>
	{
		public void TestTaskPlanningStatusColumn_RegistryEnabled()
		{
			var taskPlanningStatus = nameof(WhsTransfer.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should not be unavailable", !column.IsUnavailable);
			}
		}

		public void TestTaskPlanningStatusColumn_RegistryDisabled()
		{
			var taskPlanningStatus = nameof(WhsTransfer.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should be unavailable", column.IsUnavailable);
			}
		}

		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var transfers = new WhsTransferCollection(Factory);
			var filterBO = new TransferFilterBusinessObject();
			return new TransferFilterControl(transfers, filterBO);
		}

		protected override void SetupData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();
			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("C" + postfix);
				var product = Helper.CreateProduct(client, "P1" + postfix);
				var receive = Helper.CreateWhsReceive(client, data.Whs1, "R" + postfix, Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 50m, data.Whs1.FindLocation("A-1"), "", "");
				receive.FinaliseDocket();

				var transferInner = Helper.CreateWhsTransfer(client, data.Whs1, "TR1" + postfix, Notify, TransferType.Codes.Internal);
				Helper.CreateWhsTransferLine(transferInner, product, 10m, "A-1", "A-2");
				transferInner.FinaliseDocket();

				var transferInterWhsSource = Helper.CreateWhsTransfer(client, data.Whs1, "TR2" + postfix, Notify, TransferType.Codes.InterWhsSource);
				Helper.CreateWhsTransferLine(transferInterWhsSource, product, 15m, "A-1", whs2.PK, "B");
				transferInterWhsSource.FinaliseDocket();

				var transferInterWhsDest = Helper.CreateWhsTransfer(client, whs2, "TR3" + postfix, Notify, TransferType.Codes.InterWhsDest);
				Helper.CreateWhsTransferLine(transferInterWhsDest, product, 25m, "A-1", data.Whs1.PK, "B");
				transferInterWhsDest.FinaliseDocket();
			}
			Factory.Save();
		}

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsDocketSchema.Constants.TableName, 1);
			return baseHits;
		}

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var clientHits = new Dictionary<string, int>(baseHits);
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsTransfer.WD_OH_Client), clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsTransfer.WD_WW_Whs), warehouseHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsTransfer.ClientName), clientNameHits);

			return hitsDictionary;
		}

		protected override WhsTransferCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsTransferCollection(factory);
		}

		protected override TransferFilterBusinessObject GetNewFilterBusinessObject() => new TransferFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsTransferCollection collection, TransferFilterBusinessObject filterBizO)
		{
			return new TransferFilterControl(collection, filterBizO);
		}

		#endregion

		protected override IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName)
		{
			return base.GetTableNamesToIgnoreForUnusedFetchHints(columnName).Append(GenAddOnColumnSchema.Constants.TableName);
		}
	}
}
