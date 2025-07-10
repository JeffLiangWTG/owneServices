namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeRevRecOverLookupsTest : RevenueRecognitionLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			}
		}
	}
}
