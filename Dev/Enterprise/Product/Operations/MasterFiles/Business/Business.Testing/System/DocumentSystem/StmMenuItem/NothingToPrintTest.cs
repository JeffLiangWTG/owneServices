using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	sealed class NothingToPrintTest : TransactionedTestCase
	{
		public void TestEmptyConstructor()
		{
			TitleCopyCountPair testPair = new NothingToPrint();
			AssertEquals("Title", null, testPair.Title);
			AssertEquals("CopyCount", (short)0, testPair.CopyCount);
		}

		public void TestConstructorWithReason()
		{
			NothingToPrint testPair = new NothingToPrint("because");
			AssertEquals("Title", null, testPair.Title);
			AssertEquals("CopyCount", (short)0, testPair.CopyCount);
			AssertEquals("Reason", "because", testPair.Reason);
		}
	}
}
