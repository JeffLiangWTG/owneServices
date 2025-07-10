using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveFetchStrategyTest : WhsDocketFetchStrategyTest<WhsReceive>
	{
		#region TestFetchForLoad

		// test in WhsDocketFetchStrategyTest

		#endregion

		#region TestFetchForFactorySave

		public void TestFetchForFactorySave()
		{
			var receiveGUIDs = CreateReceives();

			for (var i = 0; i < receiveGUIDs.Count; i++)
			{
				var receiveGuid = receiveGUIDs[i];
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var receive = newFactory.Load<WhsDocket>(receiveGuid);

				using (RowFactory.SetCachedTables())
				{
					receive.Lines.Single().WE_TransactionQuantity = 2m;

					newFactory.Save();
				}

				var expectedDbHits = new Dictionary<string, int>
				{
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
					{ StmALogSchema.Constants.TableName, 1 },
					{ GlbBranchSchema.Constants.TableName, 1 }
				};

				AssertDbHits(expectedDbHits, newFactory);
			}
		}

		List<ZGuid> CreateReceives()
		{
			var result = new List<ZGuid>();

			var whs1 = Helper.CreateWarehouse("Warehouse");
			var org1 = Helper.CreateClient("1", "Client1");
			var part1 = Helper.CreateProduct(org1, "Product1");
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			for (int i = 0; i < 10; i++)
			{
				var numberString = (i + 1).ToString();

				var receive = Helper.CreateWhsReceive(org1, whs1, "R" + numberString);
				Helper.CreateWhsReceiveInventoryLine(receive, part1, 10m);
				receive.FinaliseDocket();
				receive.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this receive.");

				var container = Factory.NewWithValidTestData<WhsDocketContainer>();
				container.WC_WD = receive.PK;
				container.WC_ContainerNum = "Container" + numberString;
				container.WC_RC = gP20.PK;

				result.Add(receive.PK);
			}

			Factory.Save();

			return result;
		}

		#endregion

		#region Implementation

		protected override WhsDocketFetchStrategy GetNewFetchStrategy()
		{
			return new WhsReceiveFetchStrategy(Factory.New<WhsReceive>());
		}

		protected override WhsReceive CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits)
		{
			return Helper.CreateWhsReceiveWithInventory(org, warehouse, docketId, part, numberOfUnits);
		}

		#endregion
	}
}
