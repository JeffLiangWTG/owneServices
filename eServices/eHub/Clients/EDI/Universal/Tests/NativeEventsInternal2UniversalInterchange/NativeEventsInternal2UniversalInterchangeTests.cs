using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.NativeEventsInternal2UniversalInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.Native.Tests.NativeEventsInternal2UniversalInterchangeTests
{
	[TestClass]
	public class NativeEventsInternal2UniversalInterchangeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNativeEventsInternal2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "NativeEventsInternal2UniversalInterchange.TestFiles.NativeEventsInternal2UniversalInterchange_input.xml";
			string expectedFile = "NativeEventsInternal2UniversalInterchange.TestFiles.NativeEventsInternal2UniversalInterchange_output.xml";
			mapTester.Execute<NativeEventsInternal2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}
