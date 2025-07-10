using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UpdateDynamicPickFaceAreaLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestDynamicPickAreas

		public void TestDynamicPickAreas()
		{
			Assert(typeof(IWhsDynamicAreaCollection).IsAssignableFrom(GetNewLookups().DynamicPickAreas.GetType()));
		}

		public void TestDynamicPickAreas_Filter()
		{
			var warehouse1 = (IWhsWarehouse)Helper.CreateWarehouse("WHS1", "A", 3, 1);
			var dynamicArea1PK = ((IWhsArea)Helper.CreateWhsArea(warehouse1.PK, "Dynamic1", "DPF")).PK;

			var warehouse2 = (IWhsWarehouse)Helper.CreateWarehouse("WHS2", "B", 3, 1);
			var dynamicArea2PK = ((IWhsArea)Helper.CreateWhsArea(warehouse2.PK, "Dynamic1", "DPF")).PK;

			AssertMatchWithArea(warehouse1, dynamicArea1PK);

			AssertMatchWithArea(warehouse2, dynamicArea2PK);

			void AssertMatchWithArea(IWhsWarehouse warehouse, ZGuid dynamicAreaPK)
			{
				Applicator.WarehousePK = warehouse.PK;
				var whsArea = GetNewLookups().DynamicPickAreas.ToArray().Cast<IWhsArea>().Single();
				AssertEquals(warehouse.PK, whsArea.WA_WW_Whs);
				AssertEquals(dynamicAreaPK, whsArea.PK);
			}
		}

		public void TestDynamicPickAreas_Cache()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS1", "A", 3, 1);
			var dynamicAreaPK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic", "DPF")).PK;

			Applicator.WarehousePK = warehouse.PK;
			var lookup = GetNewLookups();

			// First time will call DB.
			AssetDbCountOnLoadArea(expectedDBHits: new Dictionary<string, int> { { WhsAreaSchema.Constants.TableName, 1 } });

			// Second time should use cache
			AssetDbCountOnLoadArea(expectedDBHits: new Dictionary<string, int>());

			void AssetDbCountOnLoadArea(Dictionary<string, int> expectedDBHits)
			{
				Factory.ResetDatabaseLoadCount();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, Factory))
				{
					var whsArea = lookup.DynamicPickAreas.ToArray().Cast<IWhsArea>().Single();
					AssertEquals(warehouse.PK, whsArea.WA_WW_Whs);
					AssertEquals(dynamicAreaPK, whsArea.PK);
				}
			}
		}

		#endregion

		#region Implementation

		UpdateDynamicPickFaceAreaLookups GetNewLookups() => new UpdateDynamicPickFaceAreaLookups(Applicator);

		UpdateDynamicPickFaceAreaMethodApplicator Applicator => applicator ?? (applicator = new UpdateDynamicPickFaceAreaMethodApplicator("test", Factory));
		UpdateDynamicPickFaceAreaMethodApplicator applicator;

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
