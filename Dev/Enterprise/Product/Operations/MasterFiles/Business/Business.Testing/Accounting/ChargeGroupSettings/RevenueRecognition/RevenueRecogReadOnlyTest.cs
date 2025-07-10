namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RevenueRecogReadOnlyTest : RevenueRecognitionReadOnlyTest
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
