namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeRevRecOverReadOnlyTest : RevenueRecognitionReadOnlyTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			}
		}

		public new void TestOffsetReadOnly()
		{
			Assert("This test in not actual for AccChargeRevRecOverride", true);
		}

		public new void TestOffsetTypeReadOnly()
		{
			Assert("This test in not actual for AccChargeRevRecOverride", true);
		}
	}
}
