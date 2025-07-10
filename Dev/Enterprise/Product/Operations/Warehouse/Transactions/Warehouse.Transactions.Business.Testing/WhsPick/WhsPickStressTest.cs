using System.Collections.Generic;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;
using WhsLocationDO = CargoWise.Database.TestFramework.ObjectModel.WhsLocation;
using WhsPickDO = CargoWise.Database.TestFramework.ObjectModel.WhsPick;
using WhsWarehouseDO = CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickStressTest : WhsTestCaseWithFactory
	{
		#region TestAllInventoriesAndOrderFetchHintsUsesTVP

		[StressTest]
		public void TestAllInventoriesAndOrderFetchHintsUsesTVP()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Factory.Save();
		
			var warehouseDO = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, data.Whs1.PK.ToGuid());
			var pick = new WhsPickDO(warehouseDO, "P1", "PIS")
			{
				WP_WL_DockDoor = warehouseDO.WW_DefaultOutboundDockDoor
			}.InsertAndReturnObject(TestConnection);
			var order =
				new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "ORD", "ORD", "PIC", "R1")
				{
					WD_WP = pick.PK
				}.InsertAndReturnObject(TestConnection);
		
			var sql = new ZStringBuilder();
			var orderLineDOList = Enumerable.Range(1, 30000)
				.Select(i => new WhsDocketLineDO(order, data.Part1.PK.ToGuid(), 1) { WE_PartAttrib1 = $"SN{i}" })
				.ToArray();
		
			TestConnection.ExecuteNonQuery(orderLineDOList.GetBulkInsertStatement());
		
			var pickBizO = Factory.Load<WhsPick>(pick.PK);
			AssertNoExceptionThrown(() => _ = pickBizO.AllInventories);
		
			var orderBizO = Factory.Load<WhsOrder>(order.PK);
			orderBizO.RunPreSaveValidation();
			AssertNoExceptionThrown(() => Factory.ExecuteAllFetchHints());
		}

		#endregion

		#region TestAutoAllocateItems_DbHits_ManyLocations

		[GuiTest]
		[StressTest]
		public void TestAutoAllocateItems_DbHits_ManyLocations()
		{
			var numberOfLocations = 31900;
			var data = new TestDataSimpleEnvironment(Factory);
			var now = ZDateTime.Now.Date;
			Factory.Save();

			var locationType = Factory.LoadTop1<Environment.Business.WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));

			var locations = new List<WhsLocationDO>(numberOfLocations);
			var docketLines = new List<WhsDocketLineDO>(numberOfLocations);

			var receive =
				new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "INW", "REC", "FIN", "R1")
				{
					WD_FinalisedDate = now.ToDateTime(),
				}.InsertAndReturnObject(TestConnection);

			var defaultLocation = data.Whs1.DefaultLocation;
			for (var i = 1; i <= numberOfLocations; i++)
			{
				var location = new WhsLocationDO(
					defaultLocation.WLV_WR.ToGuid(),
					defaultLocation.WLV_WA_PickingArea.ToGuid(),
					defaultLocation.WLV_WA_PutawayArea.ToGuid(),
					ForeignKey.FromGuid(locationType.PK.ToGuid()))
				{
					WL_Column = 2,
					WL_Tray = (short)i,
				};

				locations.Add(location);

				var receiveLine = new WhsDocketLineDO(receive, data.Part1.PK.ToGuid(), 1m)
				{
					WE_F3_NKPackType = "PKG",
					WE_WL = location.PK,
					WE_DocketLineStatus = "FIN",
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_FinalisedDate = now.ToDateTime(),
					WE_AdjustmentArrivalDate = now.ToDateTime(),
					WE_StockOnHand = 1,
				};

				docketLines.Add(receiveLine);
			}

			TestConnection.ExecuteNonQuery(locations.GetBulkInsertStatement());
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER ALL ON WhsDocketLine;"); // Make test go brr, took 30+ mins with triggers
			TestConnection.ExecuteNonQuery(docketLines.GetBulkInsertStatement());

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			using (RowFactory.SetCachedTables())
			{
				pickInNewFactory.AutoAllocateItems();
			}

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 4 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 2 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
			};

			AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory);
		}

		#endregion
	}
}
