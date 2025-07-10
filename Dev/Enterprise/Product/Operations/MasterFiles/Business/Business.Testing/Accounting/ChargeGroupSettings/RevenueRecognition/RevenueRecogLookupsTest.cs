namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RevenueRecogLookupsTest : RevenueRecognitionLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new RevenueRecognition();
			}
		}
	}
}
