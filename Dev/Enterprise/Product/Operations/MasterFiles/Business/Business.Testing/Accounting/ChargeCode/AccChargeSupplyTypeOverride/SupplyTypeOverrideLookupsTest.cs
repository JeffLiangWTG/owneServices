namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SupplyTypeOverrideLookupsTest : SupplyTypeConfigurationLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			}
		}
	}
}
