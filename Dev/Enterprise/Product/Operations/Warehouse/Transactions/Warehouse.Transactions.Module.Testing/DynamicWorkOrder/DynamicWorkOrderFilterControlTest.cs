using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class DynamicWorkOrderFilterControlTest : WhsFilterControlDBHitsTestCase<WhsDynamicWorkOrderCollection, DynamicWorkOrderFilterBusinessObject>
	{
		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var workOrders = new WhsDynamicWorkOrderCollection(Factory);
			var filterBO = new DynamicWorkOrderFilterBusinessObject();
			return new DynamicWorkOrderFilterControl(workOrders, filterBO);
		}

		protected override void SetupData()
		{
			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix);
				var product1 = Helper.CreateProduct(client, "P1" + postfix);
				var componentProduct1 = Helper.CreateProduct(client, "C" + postfix);
				var product2 = Helper.CreateProduct(client, "P2" + postfix);
				var componentProduct2 = Helper.CreateProduct(client, "D" + postfix);
				var warehouse = Helper.CreateWarehouse("WH" + postfix, "A" + postfix);
				warehouse.WW_IsVirtualWarehouse = true;
				var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
				var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(warehouse, "IPR", 1, 1).Locations[0];
				inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
				inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
				Factory.Save();

				var receive1 = Helper.CreateWhsReceive(client, warehouse, "R1" + postfix);
				receive1.WD_IsInwardsProcessingJob = true;
				receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
				var recLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, componentProduct1, 5m, inwardProcessingLocation);
				recLine1.CustomsData.WB_EntryKey = "ENT" + postfix;
				receive1.FinaliseDocket();
				Factory.Save();
				Assert("Precondition", receive1.IsFinalised);

				var receive2 = Helper.CreateWhsReceive(client, warehouse, "R2" + postfix);
				receive2.WD_IsInwardsProcessingJob = true;
				receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
				var recLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, componentProduct2, 5m, inwardProcessingLocation);
				recLine2.CustomsData.WB_EntryKey = "RENT" + postfix;
				receive2.FinaliseDocket();
				Factory.Save();
				Assert("Precondition", receive2.IsFinalised);

				var workOrder1 = Helper.CreateWhsDynamicWorkOrderWithLine(client, warehouse, "W1" + (index + 1), product1, 1m);
				workOrder1.WD_RequiredDate = ZDateTimeOffset.Now;
				var workOrderLine1 = workOrder1.Lines[0];
				workOrderLine1.IsMainInwardProcessedItem = true;

				var compLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder1, componentProduct1, 1m);
				compLine1.WE_WE_ParentDocketLine = workOrderLine1.PK;

				var pick1 = Helper.CreatePickNew(workOrder1);
				workOrder1.WD_WP = pick1.PK;
				workOrder1.FinaliseDocket();
				Assert("Precondition", workOrder1.IsFinalised);
				pick1.FinalisePick();
				Assert("Precondition", workOrder1.Pick.IsFinalised);

				var workOrder2 = Helper.CreateWhsDynamicWorkOrderWithLine(client, warehouse, "W2" + (index + 1), product2, 1m);
				workOrder2.WD_RequiredDate = ZDateTimeOffset.Now;
				var workOrderLine2 = workOrder2.Lines[0];
				workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItem = true;

				var compLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder2, componentProduct2, 1m);
				compLine2.WE_WE_ParentDocketLine = workOrderLine2.PK;

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
			return new Dictionary<string, int>
			{
				{ WhsDocketSchema.Constants.TableName, 1 }
			};
		}

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var clientHits = new Dictionary<string, int>(baseHits);
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsDynamicWorkOrder.WD_OH_Client), clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsDynamicWorkOrder.WD_WW_Whs), warehouseHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsDynamicWorkOrder.ClientName), clientNameHits);

			var pickNoHits = new Dictionary<string, int>(baseHits);
			pickNoHits.Add(WhsPickSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsDynamicWorkOrder.Pick) + "+" + nameof(WhsPick.WP_PickNo), pickNoHits);

			return hitsDictionary;
		}

		protected override WhsDynamicWorkOrderCollection GetNewCollection(BusinessObjectFactory factory)
			=> new WhsDynamicWorkOrderCollection(factory);

		protected override DynamicWorkOrderFilterBusinessObject GetNewFilterBusinessObject()
			=> new DynamicWorkOrderFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(
			WhsDynamicWorkOrderCollection collection,
			DynamicWorkOrderFilterBusinessObject filterBizO)
				=> new DynamicWorkOrderFilterControl(collection, filterBizO);

		#endregion
	}
}
