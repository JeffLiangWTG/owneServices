using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsDocketLineFetchStrategyTest<Docket, Line> : WhsTestCaseWithFactory
			where Docket : WhsDocket
			where Line : WhsDocketLine
	{
		#region TestFetchForLoad

		public void TestFetchForLoad()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			var client = Helper.CreateClient();
			var parts = new OrgSupplierPart[5];
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			for (int index = 0; index < 5; index++)
			{
				parts[index] = Helper.CreateProduct(client, "p" + index);
				Helper.CreateWhsReceiveInventoryLine(receive, parts[index], 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var docket = CreateDocket(client, whs, "D");
			for (int index = 0; index < 5; index++)
			{
				CreateDocketLine(docket, parts[index], whs.DefaultLocation, 1m);
			}
			docket.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var lines = newFactory.Load<WhsDocketLine>(new ZQuery());

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsDocketLineSchema.Constants.TableName, 1);

			AdditionalExpectedDbHitsForFetchForLoad(expectedDbHits);
			AssertDbHits(expectedDbHits, newFactory);
		}

		protected virtual void AdditionalExpectedDbHitsForFetchForLoad(Dictionary<string, int> expectedDbHits)
		{
		}

		#endregion

		#region TestFetchForView

		public void TestFetchForView()
		{
			var whs = Helper.CreateWarehouse("WH1", "A");
			Factory.Save();

			var orgs = new OrgHeader[5];
			var parts = new OrgSupplierPart[5];
			for (int index = 0; index < 5; index++)
			{
				orgs[index] = Helper.CreateClient("o" + index);
				parts[index] = Helper.CreateProduct(orgs[index], "p" + index);
				Helper.CreateWhsReceiveWithInventory(orgs[index], whs, "R" + index, parts[index], 1m);
			}
			Factory.Save();

			for (int index = 0; index < 5; index++)
			{
				var docket = CreateDocket(orgs[index], whs, "D" + index);
				CreateDocketLine(docket, parts[index], whs.DefaultLocation, 1m);
				docket.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			}
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var docketLines = viewFactory.Load<WhsDocketLine>(new ZQuery());
			int beforeFetchForView = viewFactory.DatabaseLoadCount;
			foreach (var line in docketLines)
			{
				line.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_DeclarationReference),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsDeadline),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_InwardStyle),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_InwardProcedure),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_EntryLineNo),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_EntryDate),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsQty),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsUnitOfQty),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_BondedWhsQty),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_BondedWhsUnitOfQty),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_TILV),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_ValueForDuty),
					new TableColumn(WhsBondedWarehouseAttributeSchema.Constants.TableName, WhsBondedWarehouseAttributeSchema.Constants.WB_AddInfo)
				});
			}

			foreach (var line in docketLines)
			{
				var pokeDeclarationReference = line.CustomsData.WB_DeclarationReference;
				var pokeCustomsDeadline = line.CustomsData.WB_CustomsDeadline;
				var pokeInwardStyle = line.CustomsData.WB_InwardStyle;
				var pokeInwardProcedure = line.CustomsData.WB_InwardProcedure;
				var pokeEntryKey = line.CustomsData.WB_EntryKey;
				var pokeEntryLineNo = line.CustomsData.WB_EntryLineNo;
				var pokeEntryDate = line.CustomsData.WB_EntryDate;
				var pokeCustomsQty = line.CustomsData.WB_CustomsQty;
				var pokeCustomsUnitOfQty = line.CustomsData.WB_CustomsUnitOfQty;
				var pokeBondedWhsQty = line.CustomsData.WB_BondedWhsQty;
				var pokeBondedWhsUnitOfQty = line.CustomsData.WB_BondedWhsUnitOfQty;
				var pokeTILV = line.CustomsData.WB_TILV;
				var pokeCountryOfOrigin = line.CustomsData.WB_RN_NKCountryOfOrigin;
				var pokeValueForDuty = line.CustomsData.WB_ValueForDuty;
				var pokeAddInfo = line.CustomsData.WB_AddInfo;
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsBondedWarehouseAttributeSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsDocketSchema.Constants.TableName, 10);
			expectedDbHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			AdditionalExpectedDbHitsForFetchForView(expectedDbHits);

			AssertDbHits(expectedDbHits, viewFactory);
		}

		protected virtual void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
		}

		#endregion

		#region TestFetchForView_Areas_CustomsTariff

		public void TestFetchForView_Areas_CustomsTariffDesc() => TestFetchForView_Areas_CustomsTariff(WhsDocketLine.Schema.CustomsTariffDesc);
		public void TestFetchForView_Areas_CustomsTariffItem() => TestFetchForView_Areas_CustomsTariff(WhsDocketLine.Schema.CustomsTariffItem);
		public void TestFetchForView_Areas_CustomsTariffLookup() => TestFetchForView_Areas_CustomsTariff(WhsDocketLine.Schema.CustomsTariffLookup);

		void TestFetchForView_Areas_CustomsTariff(string columnName)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var areaB = Helper.CreateArea(data.Whs1, "B");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var docket = CreateDocket(data.Org1, data.Whs1, "1");
			CreateDocketLine(docket, data.Part1, data.Whs1.DefaultLocation, 1m);
			docket.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var docketLineInvViewFactory = viewFactory.LoadTop1<WhsDocketLine>(new ZQuery());

			docketLineInvViewFactory.FetchStrategy.FetchForView(new[] { new TableColumn(WhsDocketLineSchema.Constants.TableName, columnName) });

			var poke1 = docketLineInvViewFactory.LocationAreaName;
			var poke2 = docketLineInvViewFactory[columnName];

			AssertEquals(1, viewFactory.GetTableHitCount(WhsAreaSchema.Constants.TableName));
		}

		#endregion

		protected abstract Docket CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference);
		protected abstract void CreateDocketLine(Docket docket, OrgSupplierPart part, WhsLocation location, decimal quantity);
	}
}
