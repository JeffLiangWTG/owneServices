using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageRateLineConditionsSupporterTest : TestCaseWithFactory
	{
		public void TestExportBroker()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.ExportBroker);
		}

		public void TestImportBroker()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.ImportBroker);
		}

		public void TestSendingAgent()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.SendingAgent);
		}

		public void TestReceivingAgent()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.ReceivingAgent);
		}

		public void TestDepartureCFS()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.DepartureCFS);
		}

		public void TestArrivalCFS()
		{
			var conditionSupporter = new CartageRateLineConditionsSupporter(Factory.New<CommonCartage>());
			AssertNull(conditionSupporter.ArrivalCFS);
		}

		public void TestHasDangerousGoods()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var supporter = new CartageRateLineConditionsSupporter(cartage);
			Assert(!supporter.HasDangerousGoods);
			move.UNDGs.AddNew();
			Assert(supporter.HasDangerousGoods);
		}
	}
}
