namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TSCAIndicatorListTest : NUnit.Framework.TestCase
	{
		public void TestGetMessagingCodeFor()
		{
			AssertEquals("TSCA+", TSCAIndicatorList.GetMessagingCodeFor(TSCAIndicatorList.Codes.TSCAPositive));
			AssertEquals("TSCA-", TSCAIndicatorList.GetMessagingCodeFor(TSCAIndicatorList.Codes.TSCANegative));
			AssertEquals("", TSCAIndicatorList.GetMessagingCodeFor("~~~"));
		}
	}
}
