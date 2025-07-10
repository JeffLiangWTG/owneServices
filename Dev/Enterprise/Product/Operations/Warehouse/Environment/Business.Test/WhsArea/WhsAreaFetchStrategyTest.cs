using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsAreaFetchStrategyTest : TestCaseWithFactory
	{
		#region TestAreaFetchForView

		public void TestAreaFetchForView()
		{
			for (int index = 0; index < 5; index++)
			{
				var postfix = index.ToString();
				var warehouse = Helper.CreateWarehouse("W" + postfix, "R", 2, 1);
				warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
				var org = Helper.CreateClient("o" + postfix, "Org Name" + postfix);

				var area = warehouse.Areas[0];
				area.WA_WW_Whs = warehouse.PK;
				area.WA_OH_TransitClient = org.PK;
				area.WA_Name = "Area" + postfix;
				area.WA_AreaType = "FRE";
			}

			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var areas = viewFactory.Load<WhsArea>(new ZQuery());
			int beforeFetchForView = viewFactory.DatabaseLoadCount;

			foreach (var area in areas)
			{
				area.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(WhsAreaSchema.Constants.TableName, WhsAreaSchema.Constants.WA_OH_TransitClient),
					new TableColumn(WhsAreaSchema.Constants.TableName, WhsAreaSchema.Constants.WA_WW_Whs),
				});
			}

			foreach (var area in areas)
			{
				var pokeClient = area.TransitClient;
				var pokeWarehouse = area.Warehouse;

				var pokes = new object[]
				{
					area.WA_OH_TransitClient,
					area.WA_WW_Whs
				};
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			AssertDbHits(expectedDbHits, viewFactory);
		}
		#endregion

		#region Implementation
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
