using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.BT.Tests.Maps.CDSPentantSOAPResponse2UniversalEvent
{
    [TestClass]
    public class CDSPentantSOAPResponse2UniversalEventTests
    {
        const string filePath = "Maps.CDSPentantSOAPResponse2UniversalEvent.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test_CDSPentantSOAPResponse2UniversalEvent_success()
        {
            var input = filePath + "01 - InputSuccess.xml";
            var expectedOutput = filePath + "01 - OutputSuccess.xml";
            AssertMapping(input, expectedOutput);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test_CDSPentantSOAPResponse2UniversalEvent_noTID()
        {
            var input = filePath + "02 - InputNoTID.xml";
            var expectedOutput = filePath + "02 - OutputNoTID.xml";
            AssertMapping(input, expectedOutput);
        }

        static void AssertMapping(string input, string expectedOutput)
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

            mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2018-09-17T14:21:01").Repeat.Once();
            mockContextAccessor.Expect(_ => _.GetContextProperty("eHubTrackingID", "")).Return("093b255c-8a20-44bc-bfb1-596bfa9eae13");
            mockContextAccessor.Expect(_ => _.GetContextProperty("CustomsDeclaration", "")).Return("S00030780");

            var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<Transforms.CDSPentantSOAPResponse2UEvent.CDSPentantSOAPResponse2UniversalEvent>(input, expectedOutput);

            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
