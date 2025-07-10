using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOriginalEntryHeaderDocumentComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry1 = reconDec.OriginalEntries.AddNew();
			ReconOriginalEntryHeader entry2 = reconDec.OriginalEntries.AddNew();
			AssertEquals("Compare", 0, new ReconOriginalEntryHeaderDocumentComparer().Compare(entry1, entry2));
			entry1.CH_OrigEntryReference = "XJ51";
			entry2.CH_OrigEntryReference = "XJ52";
			AssertEquals("Compare", -1, new ReconOriginalEntryHeaderDocumentComparer().Compare(entry1, entry2));
			entry1.CH_OrigEntryReference = "XJ51";
			entry2.CH_OrigEntryReference = "XJ50";
			AssertEquals("Compare", 1, new ReconOriginalEntryHeaderDocumentComparer().Compare(entry1, entry2));
		}
	}
}
