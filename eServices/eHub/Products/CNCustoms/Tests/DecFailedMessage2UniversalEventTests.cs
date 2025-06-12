using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.CNCustoms.Transforms.DecFailedMessage2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Tests
{
	[TestClass]
	public class DecFailedMessage2UniversalEventTests
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecFailedMessage2UniversalEvent_Test1()
		{
			var input = "DecFailedMessage2UniversalEvent_Input.DecFailedMessage2UniversalEvent_Input1.xml";
			var output = "DecFailedMessage2UniversalEvent_Output.DecFailedMessage2UniversalEvent_Output1.xml";

			var responseDetailXpath = "/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context' and ./*[local-name()='Type']/text()='ResponseDetail']/*[local-name()='Value']";
			AssertMapping(input, output, "Failed_CUS201912110000077_20191211165352016_201912111652136505305.xml", responseDetailXpath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecFailedMessage2UniversalEvent_Test2()
		{
			var input = "DecFailedMessage2UniversalEvent_Input.DecFailedMessage2UniversalEvent_Input2.xml";
			var output = "DecFailedMessage2UniversalEvent_Output.DecFailedMessage2UniversalEvent_Output2.xml";
			AssertMapping(input, output, "Failed_CUS202001030000025_20200102160417779_202001021606564549316.xml");
		}

		void AssertMapping(string input, string expectedOutput, string overrideFileName, params string[] excludeNodesXPaths)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Stub(_ => _.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return(overrideFileName);
			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
			};

			var excludes = new ExcludingComparer(new List<string>(excludeNodesXPaths));

			new MapTester(Assembly.GetExecutingAssembly(), excludes, extensionObjects).ExecuteCompiled<DecFailedMessage2UniversalEvent>(input, expectedOutput);
		}
	}
}
