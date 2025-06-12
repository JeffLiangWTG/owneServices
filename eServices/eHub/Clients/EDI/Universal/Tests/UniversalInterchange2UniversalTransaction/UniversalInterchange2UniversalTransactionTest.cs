using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.UniversalInterchange2UniversalTransaction;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests.UniversalInterchange2UniversalTransactionTest
{
	[TestClass]
	public class UniversalInterchange2UniversalTransactionTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2UniversalTransaction()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalInterchange2UniversalTransaction.TestFiles.UniversalInterchange2UniversalTransaction_input.xml";
			string expectedFile = "UniversalInterchange2UniversalTransaction.TestFiles.UniversalInterchange2UniversalTransaction_output.xml";
			mapTester.Execute<UniversalInterchange2UniversalTransaction>(sourceFile, expectedFile);

            sourceFile = "UniversalInterchange2UniversalTransaction.TestFiles.UniversalInterchange2UniversalTransaction_input_2012.xml";
            expectedFile = "UniversalInterchange2UniversalTransaction.TestFiles.UniversalInterchange2UniversalTransaction_output_2012.xml";
            mapTester.Execute<CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchange2UniversalTransaction>(sourceFile, expectedFile);
		}
	}
}
