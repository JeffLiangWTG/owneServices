namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackSummaryStatusListTest : NUnit.Framework.TestCase
	{
		public void TestIsACEDrawbackClearedEntryStatus()
		{
			Assert(DrawbackSummaryStatusList.IsACEDrawbackClearedEntryStatus(DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal));
			Assert(DrawbackSummaryStatusList.IsACEDrawbackClearedEntryStatus(DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryReplacement));
			Assert(DrawbackSummaryStatusList.IsACEDrawbackClearedEntryStatus(DrawbackSummaryStatusList.Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings));
			Assert(DrawbackSummaryStatusList.IsACEDrawbackClearedEntryStatus(DrawbackSummaryStatusList.Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings));
		}

		public void TestIsACEDrawbackAwaitingForResponse()
		{
			Assert(DrawbackSummaryStatusList.IsACEDrawbackAwaitingForResponse(DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal));
			Assert(DrawbackSummaryStatusList.IsACEDrawbackAwaitingForResponse(DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryReplacement));
		}
	}
}
