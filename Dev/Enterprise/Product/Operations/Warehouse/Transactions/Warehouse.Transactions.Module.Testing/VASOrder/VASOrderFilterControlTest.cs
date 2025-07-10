using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class VASOrderFilterControlTest : WhsEnvFilterControlDBHitsTestCase<WhsVASOrderCollection, VASOrderFilterBusinessObject>
	{
		#region TestNewZFilterStrip

		public void TestNewZFilterStrip()
		{
			var collection = new WhsVASOrderCollection(Factory);
			var filterBizO = new VASOrderFilterBusinessObject();
			using (var filterControl = GetNewFilterControl(collection, filterBizO))
			{
				var strip = filterBizO.FilterStrips.AddNew();
				using (var filterStrip = filterControl.AddFilterStrip(strip))
				{
					AssertType<WhsWorkflowFilterStrip>(filterStrip);
				}
			}
		}

		#endregion

		#region Column Initialisation

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetNewFilterControl(new WhsVASOrderCollection(Factory), new VASOrderFilterBusinessObject()))
			{
				var profitLossReason = nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_ProfitLossReasonCode);
				var column = filterControl.FilteredGrid.GetColumnStyle(profitLossReason);
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = GetNewFilterControl(new WhsVASOrderCollection(Factory), new VASOrderFilterBusinessObject()))
			{
				var margin = nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_TotalProfitRevenueMargin);
				var column = filterControl.FilteredGrid.GetColumnStyle(margin);
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		#endregion

		#region SetupData

		protected override void SetupData()
		{
			var today = ZDate.Today;
			var expiryDate = today.AddDays(30);
			var packingDate = today;
			const string attr1 = "PA-1";
			const string attr2 = "PA-2";
			const string attr3 = "PA-3";
			const string serialNo = "123";

			for (int i = 0; i < 10; i++)
			{
				var client = Helper.CreateClient("C" + i);
				var warehouse = Helper.CreateWarehouse("W" + i);
				var serviceArea = Helper.CreateServiceAreaForVASOrder(warehouse, areaName: "A" + i, rowCode: "R" + i);
				var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);

				Helper.SetClientAllAttributeType(client, true);

				for (int j = 0; j < 10; j++)
				{
					var part = Helper.CreateProduct("P" + i + j, client);
					Helper.SetProductAllAttributeUse(client, part, true);

					if (i % 2 == 0)
					{
						var receive = Helper.CreateWhsReceive(client, warehouse, $"R{i}{j}");
						var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, warehouse.DefaultLocation, expiryDate, packingDate, attr1, attr2, attr3, "");
						receiveLine.WI_SerialNumber = serialNo;
						receive.FinaliseDocketWithoutUserConfirmation();
						WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
					}

					var line = Helper.CreateWhsVASOrderLine(vasOrder, part, 1m);
					line.WVL_PackingDate = packingDate;
					line.WVL_ExpiryDate = expiryDate;
					line.WVL_PartAttrib1 = attr1;
					line.WVL_PartAttrib2 = attr2;
					line.WVL_PartAttrib3 = attr3;
					line.WVL_SerialNumber = serialNo;
				}

				if (i % 2 == 0)
				{
					Factory.Save();
					var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
					AssertNotNull("Precondition - Created Transfer.", initialTransfer);
				}
			}
		}

		#endregion

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var clientHits = new Dictionary<string, int>(baseHits);
			clientHits.Add(OrgAddressSchema.Constants.TableName, 0);
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add("Client+OH_Code", clientHits);
			hitsDictionary.Add("Client+OH_FullName", clientHits);

			var areaHits = new Dictionary<string, int>(baseHits);
			areaHits.Add(WhsAreaSchema.Constants.TableName, 1);
			hitsDictionary.Add("ServiceArea+WA_NameMultilingual", areaHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsAreaSchema.Constants.TableName, 1);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsVASOrder.WarehousePK), warehouseHits);

			var transferIntoServiceAreaHits = new Dictionary<string, int>(baseHits);
			transferIntoServiceAreaHits.Add(WhsDocketSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsVASOrder.Status), transferIntoServiceAreaHits);

			var productCodeHits = new Dictionary<string, int>(baseHits);
			productCodeHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			productCodeHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			hitsDictionary.Add("ProductCode", productCodeHits);

			var productDescriptionHits = new Dictionary<string, int>(baseHits);
			productDescriptionHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			productDescriptionHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			hitsDictionary.Add("ProductDescription", productDescriptionHits);

			var productQtyHits = new Dictionary<string, int>(baseHits);
			productQtyHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			hitsDictionary.Add("ProductQty", productQtyHits);

			var jhHits = new Dictionary<string, int>(baseHits);
			jhHits.Add(JobHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_ProfitLossReasonCode), jhHits);
			hitsDictionary.Add(nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_TotalProfitRevenueMargin), jhHits);

			return hitsDictionary;
		}

		#endregion

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsVASOrderSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetNewCollection

		protected override WhsVASOrderCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsVASOrderCollection(factory);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override VASOrderFilterBusinessObject GetNewFilterBusinessObject() => new VASOrderFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsVASOrderCollection collection, VASOrderFilterBusinessObject filterBizO)
		{
			return new VASOrderFilterControl(collection, filterBizO);
		}

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctions(Factory);
		}

		new WhsTestHelperFunctions Helper => (WhsTestHelperFunctions)base.Helper;

		#endregion
	}
}
