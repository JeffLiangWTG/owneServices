namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FunctionalitySuspenderWrapperTest : FunctionalitySuspenderTest
	{
		protected override IFunctionalitySuspender GetWrappedSuspender(FunctionalitySuspender suspenderToWrap)
		{
			return new FunctionalitySuspenderWrapper(() => suspenderToWrap.GetSuspender());
		}
	}
}
