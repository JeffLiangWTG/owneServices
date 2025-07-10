using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsRowFetchStrategyTest : TestCaseWithFactory
	{
		#region TestFetchForLoad_Rows

		public void TestFetchForLoad_Rows()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs2 = Helper.CreateWarehouse("WH2");
			var whs3 = Helper.CreateWarehouse("WH3");
			var rowInWh1 = Helper.CreateRowAndGenerateLocations(whs1, "R1", 1, 1);
			var rowInWh2 = Helper.CreateRowAndGenerateLocations(whs2, "R2", 1, 1);
			var rowInWh3 = Helper.CreateRowAndGenerateLocations(whs3, "R3", 1, 1);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rowInWhs1InNewFactory = newFactory.Load<WhsRow>(rowInWh1.PK);
			rowInWhs1InNewFactory.FetchStrategy.FetchForView(Array.Empty<TableColumn>()); 

			var rowInWhs2InNewFactory = newFactory.Load<WhsRow>(rowInWh2.PK);
			rowInWhs2InNewFactory.FetchStrategy.FetchForView(Array.Empty<TableColumn>());

			var rowInWhs3InNewFactory = newFactory.Load<WhsRow>(rowInWh3.PK);
			rowInWhs3InNewFactory.FetchStrategy.FetchForView(Array.Empty<TableColumn>());

			var poke1 = rowInWhs1InNewFactory.Lookups;
			var poke2 = rowInWhs2InNewFactory.Lookups;
			var poke3 = rowInWhs3InNewFactory.Lookups;

			var expetedDbHits = new Dictionary<string, int>();
			expetedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expetedDbHits.Add(OrgAddressSchema.Constants.TableName, 1);
			expetedDbHits.Add(WhsRowSchema.Constants.TableName, 3); // Load three WhsRow objects
			AssertDbHits(expetedDbHits, newFactory);
		}

		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctionsEnv(Factory);
				}
				return helper;
			}
		}
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
