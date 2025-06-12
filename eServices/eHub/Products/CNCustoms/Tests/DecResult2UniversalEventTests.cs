using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.CNCustoms.Transforms.DecResult2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class DecResult2UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecResult2UniversalEvent_Test1()
		{
			var input = "DecResult2UniversalEvent_input.DecResult2UniversalEvent_input01.xml";
			var output = "DecResult2UniversalEvent_output.DecResult2UniversalEvent_output01.xml";
			AssertMapping(input, output);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecResult2UniversalEvent_Test2()
		{
			var input = "DecResult2UniversalEvent_input.DecResult2UniversalEvent_input02.xml";
			var output = "DecResult2UniversalEvent_output.DecResult2UniversalEvent_output02.xml";
			AssertMapping(input, output);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecResult2UniversalEvent_Test3()
		{
			var input = "DecResult2UniversalEvent_input.DecResult2UniversalEvent_input03.xml";
			var output = "DecResult2UniversalEvent_output.DecResult2UniversalEvent_output03.xml";
			AssertMapping(input, output);
		}

		void AssertMapping(string input, string expectedOutput)
		{
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<DecResult2UniversalEvent>(input, expectedOutput);
		}
	}
}
