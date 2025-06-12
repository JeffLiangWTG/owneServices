using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.CNCustoms.Transforms.DecImportResponse2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Tests
{
    [TestClass]
    public class DecImportResponse2UniversalEventTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DecImportResponse2UniversalEvent_Test1()
        {
            var input = "DecImportResponse2UniversalEvent_input.DecImportResponse2UniversalEvent_input01.xml";
            var output = "DecImportResponse2UniversalEvent_output.DecImportResponse2UniversalEvent_output01.xml";
            AssertMapping(input, output, "Successed_000000000000194233_201812071406567072642_20181207140729493752412");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DecImportResponse2UniversalEvent_Test2()
        {
            var input = "DecImportResponse2UniversalEvent_input.DecImportResponse2UniversalEvent_input02.xml";
            var output = "DecImportResponse2UniversalEvent_output.DecImportResponse2UniversalEvent_output02.xml";
            AssertMapping(input, output, "Failed_000000000000191267_201812071412054640195_20181207141244");
        }

        void AssertMapping(string input, string expectedOutput, string overrideFileName)
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Stub(_ => _.GetContextProperty("OverrideFilename", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(overrideFileName);

            var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<DecImportResponse2UniversalEvent>(input, expectedOutput);
        }
    }
}
