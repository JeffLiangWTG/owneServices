using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveValidationStrategyStressTest : WhsTestCaseWithFactory
	{
		[StressTest]
		public void TestSerialNumberIsUnique_NoExceptionWithLargeNumberOfSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var sql = new ZStringBuilder();
			var receive =
				new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "INW", "REC", "ENT", "R1")
					.InsertAndReturnObject(TestConnection);
			var receiveLineDOList = Enumerable.Range(1, 14001)
				.Select(i => new WhsDocketLineDO(receive, data.Part1.PK.ToGuid(), 1)
				{
					WE_OriginalInventoryStatus = "PND",
					WE_CurrentInventoryStatus = "PND",
					WE_StockOnHand = 1m,
					WE_SerialNumber = $"SN{i}"
				})
				.ToArray();
			sql.Append(WhsDocketLineDO.GetBulkInsertStatement(receiveLineDOList));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var receiveBizo = Factory.Load<WhsReceive>(receive.PK);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receiveBizo, data.Part1, 1m);

			var deviceStrategy = new WhsReceiveValidationStrategy(receiveBizo);
			inventory2.WI_SerialNumber = "Test";

			var isSerialUniqueForTest = false;
			AssertNoExceptionThrown(() =>
				isSerialUniqueForTest = deviceStrategy.CheckSerialNumberIsUnique(receiveBizo.Client, inventory2, true));
			AssertEquals("This SN should be unique.", true, isSerialUniqueForTest);

			inventory2.WI_SerialNumber = "SN2";
			AssertNoExceptionThrown(() =>
				isSerialUniqueForTest = deviceStrategy.CheckSerialNumberIsUnique(receiveBizo.Client, inventory2, true));
			AssertEquals("This SN should NOT be unique.", false, isSerialUniqueForTest);
		}
	}
}
