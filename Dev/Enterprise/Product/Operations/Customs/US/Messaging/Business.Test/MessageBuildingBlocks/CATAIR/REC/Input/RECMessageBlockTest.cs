namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECMessageBlockTest : NUnit.Framework.TestCase
	{
		public void TestSerialiseRECR21()
		{
			var block = new RECR21();
			block.FirstFeeClass = "499";
			block.FirstOriginalFee = 0m;
			block.FirstEstimateReconciliationFee = 50.20m;
			block.SecondFeeClass = "501";
			block.SecondOriginalFee = 12.50m;
			block.SecondEstimateReconciliationFee = 0m;
			AssertEquals("R210049900000000000000000050205010000000125000000000000                         ", block.Serialise(false));
		}

		public void TestSerialiseRECR89()
		{
			var block = new RECR89();
			block.FeeClass = "499";
			block.TotalOriginalFee = 0m;
			block.TotalEstimateReconciliationFee = 50.20m;
			block.FeeClass1 = "501";
			block.TotalOriginalFee1 = 12.50m;
			block.TotalEstimateReconciliationFee1 = 0m;
			AssertEquals("R890049900000000000000000050205010000000125000000000000                         ", block.Serialise(false));
		}
	}
}
