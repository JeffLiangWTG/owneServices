using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryLineNumberComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			CusEntryLine entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;

			CusEntryLine entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 2;

			EntryLineNumberComparer comparer = new EntryLineNumberComparer();
			AssertEquals("Negative", true, comparer.Compare(entryLine1, entryLine2) < 0);

			entryLine2.CL_LineNumber = 1;
			AssertEquals("Zero", true, comparer.Compare(entryLine1, entryLine2) == 0);

			entryLine1.CL_LineNumber = 2;
			AssertEquals("Positive", true, comparer.Compare(entryLine1, entryLine2) > 0);
		}
	}
}
