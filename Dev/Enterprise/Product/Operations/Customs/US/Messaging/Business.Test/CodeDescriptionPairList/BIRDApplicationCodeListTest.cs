namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class BIRDApplicationCodeListTest : NUnit.Framework.TestCase
	{
		public void TestGetABIApplicationCode()
		{
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.EntrySummaryQueryInput));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.CargoRelease));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.EntrySummary));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, BIRDApplicationCodeList.GetABIApplicationCode(BIRDApplicationCodeList.Codes.Status));
		}
	}
}
