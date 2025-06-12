using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests.UniversalInterchange2UniversalTransactionTest
{
	[TestClass]
	public class Universal_2012_11_Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Universal_2012_11_UniversalInterchangeInclude2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2012_11.TestFiles.UniversalInterchangeInclude2UniversalInterchange_input.xml";
			string expectedFile = "Universal_2012_11.TestFiles.UniversalInterchangeInclude2UniversalInterchange_output.xml";
			mapTester.Execute<UniversalInterchangeInclude2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Universal_2012_11_UniversalInterchangeInclude2UniversalInterchangeEnvelope()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2012_11.TestFiles.UniversalInterchangeInclude2UniversalInterchangeEnvelope_input.xml";
			string expectedFile = "Universal_2012_11.TestFiles.UniversalInterchangeInclude2UniversalInterchangeEnvelope_output.xml";
			mapTester.Execute<UniversalInterchangeInclude2UniversalInterchangeEnvelope>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2UniversalEvent()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2012_11.TestFiles.UniversalInterchange2UniversalEvent_input.xml";
			string expectedFile = "Universal_2012_11.TestFiles.UniversalInterchange2UniversalEvent_output.xml";
			mapTester.Execute<UniversalInterchange2UniversalEvent>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "Universal_2012_11.TestFiles.UniversalEvent2UniversalInterchange_input.xml";
			string expectedFile = "Universal_2012_11.TestFiles.UniversalEvent2UniversalInterchange_output.xml";
			mapTester.Execute<UniversalEvent2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}
