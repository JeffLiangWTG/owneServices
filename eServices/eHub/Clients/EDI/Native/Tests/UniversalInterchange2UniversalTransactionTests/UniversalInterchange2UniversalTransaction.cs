using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.Native.UniversalInterchange2UniversalTransaction;

namespace CargoWise.eHub.Clients.EDI.Schemas.Native.Tests.UniversalInterchange2UniversalTransactionTests
{
	[TestClass]
	public class UniversalInterchange2UniversalTransactionTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2UniversalTransaction()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

            string sourceFile = "UniversalInterchange2UniversalTransactionTests.TestFiles.UniversalInterchange2UniversalTransaction_input.xml";
            string expectedFile = "UniversalInterchange2UniversalTransactionTests.TestFiles.UniversalInterchange2UniversalTransaction_output.xml";
            mapTester.Execute<UniversalInterchange2UniversalTransaction>(sourceFile, expectedFile);
		}
	}
}
