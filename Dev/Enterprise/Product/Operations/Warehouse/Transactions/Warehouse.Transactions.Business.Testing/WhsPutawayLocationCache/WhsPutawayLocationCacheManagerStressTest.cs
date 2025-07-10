using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPutawayLocationCacheManagerStressTest : WhsTestCaseWithFactory
	{
		[StressTest]
		public void TestCreateCache_PalletQuantityExceedSmallInt()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var now = ZDateTime.Now.Date;
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_PalletFloorSpaces = 5;
			location.WLV_PalletStackHeight = 6;
			Factory.Save();

			var receive =
				new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "INW", "REC", "FIN", "W0001")
				{
					WD_FinalisedDate = now.ToDateTime()
				}.InsertAndReturnObject(TestConnection);

			var sql = new ZStringBuilder();
			var receiveLineDOList = Enumerable.Range(1, 33000)
				.Select(i => new WhsDocketLineDO(receive, data.Part1.PK.ToGuid(), 1)
				{
					WE_WL = location.PK.ToGuid(),
					WE_PalletID = $"PLT{i}",
					WE_F3_NKPackType = "PKG",
					WE_DocketLineStatus = "FIN",
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_FinalisedDate = now.ToDateTime(),
					WE_AdjustmentArrivalDate = now.ToDateTime(),
					WE_StockOnHand = 1,
				}).ToArray();

			TestConnection.ExecuteNonQuery(receiveLineDOList.GetBulkInsertStatement());

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			var cache = WhsPutawayLocationCacheTestHelper.GetAllCacheRecords(Factory, data.Whs1.PK).Where(c =>
				location.PK.Equals(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]))).ToArray();

			AssertEquals("Should return one WhsPutawayLocationCache with the pallet count equal to 32767.", (short)32767,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
		}
	}
}
