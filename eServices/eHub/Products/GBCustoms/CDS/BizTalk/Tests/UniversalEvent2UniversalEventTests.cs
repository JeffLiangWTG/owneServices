using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Rhino.Mocks;
using CargoWise.eHub.Products.GBCustoms.CDS.BT.Orchestrations.Transformations;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Tests
{
    [TestClass]
    public class UniversalEvent2UniversalEventTests
    {
        const string filePath = "Helpers.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalEvent2UniversalEvent_MapTest_DirectDocument()
        {
            InitilizeAndExecute("HYEMIKDUK", "TestInput3.xml", "TestOutput.xml", "HYEMIK.GB666673196000.DOZ");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalEvent2UniversalEvent_MapTest_DirectQuery()
        {
            InitilizeAndExecute("HYEMIKDUK", "TestInput6.xml", "TestOutput3.xml", "HYEMIK.GB666673196000.DOZ", "GBCustoms-DirectQuery");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalEvent2UniversalEvent_MapTest_DirectDefault()
        {
            InitilizeAndExecute("HYEMIKDUK", "TestInput7.xml", "TestOutput4.xml", "HYEMIK.GB666673196000.DOZ", "GBCustoms-DirectQuery");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalEvent2UniversalEvent_MapTest_NamespacePrefix()
        {
			InitilizeAndExecute("HYEMIKDUK", "TestInput4.xml", "TestOutput2.xml", "HYEMIK.GB666673196000.DOZ");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalEvent2UniversalEvent_MapTest_Exception()
        {
	        var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEMIKDUK");
	        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GBCustoms");

	        var extensionObjects = new Dictionary<string, object>
	        {
		        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
	        };

	        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<UniversalEvent2UniversalEvent>(filePath + "TestInput5.xml", "Message from CW1 client did not provide a Context Key");
        }

		static void InitilizeAndExecute(string sourceDestination, string inputFile, string outputFile, string contextKey, string recipientID = "GBCustoms-DirectDocument")
        {
            var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceDestination);
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GBCustoms");

            var extensionObjects = new Dictionary<string, object>
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<UniversalEvent2UniversalEvent>(filePath + inputFile, filePath + outputFile);

            mockContextAccessor.AssertWasCalled(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID));
			mockContextAccessor.AssertWasCalled(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", contextKey));
        }
    }
}
