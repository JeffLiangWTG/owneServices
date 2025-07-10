using System.Threading;
using NUnit.Framework;

namespace WinzorTestAdapter.TestClasses
{
	class TestClass : TestCase
	{
		public void TestPass()
		{
			Assert(true);
		}

		public void TestFail()
		{
			Assert(false);
		}

		public void TestExplicit()
		{
			Assert(false);
		}

		public void TestExplicitWithWhiteSpace()
		{
			Assert(false);
		}

		public void TestSlow()
		{
			Thread.Sleep(10);
			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRequiresSourceCode()
		{
			Assert(true);
		}

		[GuiTest]
		public void TestGui()
		{
			Assert(true);
		}
	}
}
