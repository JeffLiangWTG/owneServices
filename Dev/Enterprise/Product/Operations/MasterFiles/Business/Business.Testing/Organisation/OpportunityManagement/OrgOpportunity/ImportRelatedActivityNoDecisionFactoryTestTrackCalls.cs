namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ImportRelatedActivityNoDecisionFactoryTestTrackCalls : ImportRelatedActivityNoDecisionFactory
	{
		public bool GetIfAvailableCoreWasCalled;

		protected override T GetIfAvailableCore<T>()
		{
			GetIfAvailableCoreWasCalled = true;
			return base.GetIfAvailableCore<T>();
		}
	}
}
