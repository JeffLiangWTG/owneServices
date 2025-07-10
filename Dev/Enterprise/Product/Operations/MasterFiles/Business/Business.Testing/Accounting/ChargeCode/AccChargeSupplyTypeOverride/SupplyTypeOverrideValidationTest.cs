namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SupplyTypeOverrideValidationTest : SupplyTypeConfigurationValidationTest
	{
		protected override IJobConfigurationSelector GetNewBizObj => Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();

		protected override IRegistrySettingCollection GetNewBizObjCollection => new AccChargeSupplyTypeOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());
	}
}
