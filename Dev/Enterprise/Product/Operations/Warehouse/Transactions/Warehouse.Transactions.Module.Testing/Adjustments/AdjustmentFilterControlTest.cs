using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class AdjustmentFilterControlTest : WhsFilterControlDBHitsTestCase<WhsAdjustmentCollection, AdjustmentFilterBusinessObject>
	{
		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var adjustments = new WhsAdjustmentCollection(Factory);
			var filterBO = new AdjustmentFilterBusinessObject();
			return new AdjustmentFilterControl(adjustments, filterBO);
		}

		WhsDocket SetupAdjustmentWithLine(ZGuid whsPK, ZGuid clientPK, OrgSupplierPart part, string postfix)
		{
			var adjustment = Factory.New<WhsAdjustment>();
			adjustment.WD_OH_Client = clientPK;
			adjustment.WD_WW_Whs = whsPK;
			adjustment.WD_RequiredDate = ZDateTimeOffset.Now;
			adjustment.NotificationManager.Push(Notify);

			var line = Helper.CreateWhsAdjustmentLine(adjustment, part, 2m, "A" + postfix + "-1");
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);

			return adjustment;
		}

		protected override void SetupData()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

			for (int index = 0; index < 10; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix, "O" + postfix);
				var part = Helper.CreateProduct(client, "p" + postfix);
				var warehouse = Helper.CreateWarehouse("W" + postfix, "A" + postfix, 2, 1);
				Factory.Save();
				SetupAdjustmentWithLine(warehouse.PK, client.PK, part, postfix);
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

			var clientHits = new Dictionary<string, int>();
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			clientHits.Add(WhsDocketSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsAdjustment.WD_OH_Client), clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsAdjustment.WD_WW_Whs), warehouseHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsAdjustment.ClientName), clientNameHits);

			return hitsDictionary;
		}

		protected override WhsAdjustmentCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsAdjustmentCollection(factory);
		}

		protected override AdjustmentFilterBusinessObject GetNewFilterBusinessObject() => new AdjustmentFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsAdjustmentCollection collection, AdjustmentFilterBusinessObject filterBizO)
		{
			return new AdjustmentFilterControl(collection, filterBizO);
		}

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode => WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;

		#endregion
	}
}
