using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC513ExportOperationTest : ExportOperation_CC515_CC513Test<CC513ExportOperationProvider, ICC513CExportOperation>
{
	public void TestLRN()
	{
		const string testLRN = "LRN2343234242";
		EntryHeader.CH_BGMReference = testLRN;
		AssertEquals(testLRN, GetProvider().LRN);
	}

	public void TestMRN()
	{
		const string testMRN = "22IEDU4EU157452570";
		EntryHeader.EntryNumber = testMRN;
		AssertEquals(testMRN, GetProvider().MRN);
	}

	protected override CC513ExportOperationProvider GetProvider() => new CC513ExportOperationProvider(SendingObject);
}
