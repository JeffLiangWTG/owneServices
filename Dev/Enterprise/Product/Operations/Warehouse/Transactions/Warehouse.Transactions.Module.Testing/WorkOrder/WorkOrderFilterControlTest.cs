using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class WorkOrderFilterControlTest : WhsFilterControlDBHitsTestCase<WhsWorkOrderCollection, WorkOrderFilterBusinessObject>
	{
		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var workOrders = new WhsWorkOrderCollection(Factory);
			var filterBO = new WorkOrderFilterBusinessObject();
			return new WorkOrderFilterControl(workOrders, filterBO);
		}

		protected override void SetupData()
		{
			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix);
				var bomProduct1 = Helper.CreateProduct(client, "P1" + postfix);
				var componentProduct1 = Helper.CreateProduct(client, "C" + postfix);
				Helper.CreateProductBOM(bomProduct1, componentProduct1, 1m, Constants.PkgUnit.Unit);
				var bomProduct2 = Helper.CreateProduct(client, "P2" + postfix);
				var componentProduct2 = Helper.CreateProduct(client, "D" + postfix);
				Helper.CreateProductBOM(bomProduct2, componentProduct2, 1m, Constants.PkgUnit.Unit);
				var warehouse = Helper.CreateWarehouse("WH" + postfix, "A" + postfix);
				Factory.Save();

				var receive1 = Helper.CreateWhsReceive(client, warehouse, "R1" + postfix);
				Helper.CreateWhsReceiveInventoryLine(receive1, componentProduct1, 5m, warehouse.DefaultLocation);
				receive1.FinaliseDocket();
				Factory.Save();
				Assert("Precondition", receive1.IsFinalised);

				var receive2 = Helper.CreateWhsReceive(client, warehouse, "R2" + postfix);
				Helper.CreateWhsReceiveInventoryLine(receive2, bomProduct2, 5m, warehouse.DefaultLocation);
				receive2.FinaliseDocket();
				Factory.Save();
				Assert("Precondition", receive2.IsFinalised);

				var workOrder1 = Helper.CreateWhsWorkOrder(client, warehouse, "W1" + (index + 1), WorkOrderType.Codes.Assemble);
				Helper.CreateWhsWorkOrderLine(workOrder1, bomProduct1, 1m);
				workOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

				var pick1 = Helper.CreatePickNew(workOrder1);
				workOrder1.WD_WP = pick1.PK;
				workOrder1.FinaliseDocket();
				Assert("Precondition", workOrder1.IsFinalised);
				pick1.FinalisePick();
				Assert("Precondition", workOrder1.Pick.IsFinalised);

				var workOrder2 = Helper.CreateWhsWorkOrder(client, warehouse, "W2" + (index + 1), WorkOrderType.Codes.Disassemble);
				Helper.CreateWhsWorkOrderLine(workOrder2, bomProduct2, 1m);
				workOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

				var pick2 = Helper.CreatePickNew(workOrder2);
				workOrder2.WD_WP = pick2.PK;
				workOrder2.FinaliseDocket();
				Assert("Precondition", workOrder2.IsFinalised);
				pick2.FinalisePick();
				Assert("Precondition", workOrder2.Pick.IsFinalised);
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
			hitsDictionary.Add(nameof(WhsWorkOrder.WD_OH_Client), clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsWorkOrder.WD_WW_Whs), warehouseHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsWorkOrder.ClientName), clientNameHits);

			var pickNoHits = new Dictionary<string, int>(baseHits);
			pickNoHits.Add(WhsPickSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsWorkOrder.Pick) + "+" + nameof(WhsPick.WP_PickNo), pickNoHits);

			return hitsDictionary;
		}

		protected override WhsWorkOrderCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsWorkOrderCollection(factory);
		}

		protected override WorkOrderFilterBusinessObject GetNewFilterBusinessObject() => new WorkOrderFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsWorkOrderCollection collection, WorkOrderFilterBusinessObject filterBizO)
		{
			return new WorkOrderFilterControl(collection, filterBizO);
		}

		#endregion
	}
}
