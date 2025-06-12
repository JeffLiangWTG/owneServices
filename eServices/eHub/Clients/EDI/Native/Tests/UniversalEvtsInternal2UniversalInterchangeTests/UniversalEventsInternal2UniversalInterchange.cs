using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.Native.UniversalEventsInternal2UniversalInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.Native.Tests.UniversalEventsInternal2UniversalInterchangeTests
{
	[TestClass]
	public class UniversalEventsInternal2UniversalInterchangeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEventsInternal2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalEvtsInternal2UniversalInterchangeTests.TestFiles.UniversalEventsInternal.xml";
			string expectedFile = "UniversalEvtsInternal2UniversalInterchangeTests.TestFiles.UniversalInterchange.xml";
			mapTester.Execute<UniversalEventsInternal2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEventsInternal2UniversalInterchange_EmptyMessage()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalEvtsInternal2UniversalInterchangeTests.TestFiles.UniversalEventsInternalEmpty.xml";
			string expectedFile = "UniversalEvtsInternal2UniversalInterchangeTests.TestFiles.UniversalInterchangeEmpty.xml";
			mapTester.Execute<UniversalEventsInternal2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}
