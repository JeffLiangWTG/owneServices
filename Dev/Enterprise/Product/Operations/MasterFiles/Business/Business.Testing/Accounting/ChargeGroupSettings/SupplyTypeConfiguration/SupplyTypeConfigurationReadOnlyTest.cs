namespace Enterprise.MasterFiles.Business.Testing
{
	public class SupplyTypeConfigurationReadOnlyTest : JobConfigurationSelectorReadOnlyTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new SupplyTypeConfiguration();
			}
		}
	}
}
