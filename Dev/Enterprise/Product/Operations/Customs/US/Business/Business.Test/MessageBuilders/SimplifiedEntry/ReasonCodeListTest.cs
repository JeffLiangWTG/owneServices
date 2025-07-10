namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReasonCodeListTest : NUnit.Framework.TestCase
	{
		public void TestIsBulkQuery()
		{
			AssertEquals(false, ReasonCodeList.IsReferenceNoRequired(ReasonCodeList.Codes.MerchandiseSeized));
			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(ReasonCodeList.Codes.EntryReplacedBy7512));
			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(ReasonCodeList.Codes.EntryReplacedByFTZ));
			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(ReasonCodeList.Codes.MerchandiseClearedByAnother));
		}
	}
}
