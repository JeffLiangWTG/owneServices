using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Riba.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Riba.Tests
{
    /// <summary>
    /// Summary description for FlexibleFormatWrapper2UniversalEventTest
    /// </summary>
    [TestClass]
    public class FlexibleFormatWrapper2UniversalEventTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFlexibleFormatWrapper2UniversalEvent_WithNativeContent()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SenderID");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RecipientID");
            mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "RecipientID"));
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "SenderID"));
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("PAR00001042.txt");

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss.fff"))).Return("2015-06-15T10:08:33.000");

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper}
			};

            string inputFile = "TestFiles.FlexibleFormatWrapper_NativeContent.xml";
            string expectedFile = "TestFiles.UniversalEvent_output_NativeContent.xml";
            var comparer = new ExcludingComparer(new List<string>() { "//*[local-name()='EventTime']" });

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<FlexibleFormatWrapper2UniversalEvent>(inputFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFlexibleFormatWrapper2UniversalEvent_WithXMLContent()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SenderID");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RecipientID");
            mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "RecipientID"));
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "SenderID"));
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("PAR00001042.txt");

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss.fff"))).Return("2015-06-15T10:08:33.000");

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper}
			};

            string inputFile = "TestFiles.FlexibleFormatWrapper_XMLContent.xml";
            string expectedFile = "TestFiles.UniversalEvent_output_XMLContent.xml";
            var comparer = new ExcludingComparer(new List<string>() { "//*[local-name()='EventTime']" });

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<FlexibleFormatWrapper2UniversalEvent>(inputFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
