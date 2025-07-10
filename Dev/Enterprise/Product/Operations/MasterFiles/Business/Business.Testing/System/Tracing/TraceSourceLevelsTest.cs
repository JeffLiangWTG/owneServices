using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Tracing
{
	sealed class TraceSourceLevelsTest : TestCase
	{
		public void TestCodeList()
		{
			var list = new TraceSourceLevels();
			AssertEquals(8, list.Count);
			Assert(list.ContainsCode("ACT"));
			Assert(list.ContainsCode("ALL"));
			Assert(list.ContainsCode("CRT"));
			Assert(list.ContainsCode("ERR"));
			Assert(list.ContainsCode("INF"));
			Assert(list.ContainsCode("OFF"));
			Assert(list.ContainsCode("VRB"));
			Assert(list.ContainsCode("WRN"));
		}

		public void TestDescriptionList()
		{
			var list = new TraceSourceLevels();
			AssertEquals(8, list.Count);

			AssertEquals("Allows the Stop, Start, Suspend, Transfer, Resume events through.", list["ACT"].Description);
			AssertEquals("Allows all events through.", list["ALL"].Description);
			AssertEquals("Allows only Critical events through.", list["CRT"].Description);
			AssertEquals("Allows Critical, Error, Warning, and Information events through.", list["INF"].Description);
			AssertEquals("Does not allow any events through.", list["OFF"].Description);
			AssertEquals("Allows Critical, Error, Warning, Information and Verbose events through.", list["VRB"].Description);
			AssertEquals("Allows Critical, Error and Warning events through.", list["WRN"].Description);
		}
	}
}
