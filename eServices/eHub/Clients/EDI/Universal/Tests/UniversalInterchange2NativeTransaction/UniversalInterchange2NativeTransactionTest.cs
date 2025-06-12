using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.UniversalInterchange2NativeTransaction;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests.UniversalInterchange2NativeTransactionTest
{
	[TestClass]
	public class UniversalInterchange2NativeTransactionTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2NativeTransaction()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalInterchange2NativeTransaction.TestFiles.UniversalInterchange2NativeTransaction_input.xml";
			string expectedFile = "UniversalInterchange2NativeTransaction.TestFiles.UniversalInterchange2NativeTransaction_output.xml";
			mapTester.Execute<UniversalInterchange2NativeTransaction>(sourceFile, expectedFile);
		}
	}
}
