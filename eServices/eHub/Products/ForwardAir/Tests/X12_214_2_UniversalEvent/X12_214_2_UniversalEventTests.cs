using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardAir.Transforms.X12_214_2_UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Tests
{
    [TestClass]
    public class X12_214_2_UniversalEventTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void X12_214_2_UniversalEvent()
        {
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockUnitConvertorMapper = MockRepository.GenerateMock<UnitConverter>();

            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Type", "X1")).Return("ARV").Repeat.Times(1);
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Type", "X3")).Return("ARV").Repeat.Times(2);
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Type", "D1")).Return("DLV").Repeat.Times(1);
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Description", "X1")).Return("|FAC=Delivery Location").Repeat.Times(1);
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Description", "X3")).Return("|FAC=Pickup Location").Repeat.Times(2);
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Description", "D1")).Return("|FAC=Delivery Location").Repeat.Times(1);
            mockCodeMapper.Expect(x => x.GetUNLOCOfromIATA("GSO")).Return("USGSO").Repeat.Times(2);
            mockCodeMapper.Expect(x => x.GetUNLOCOfromIATA("YYZ")).Return("CAYYZ").Repeat.Times(1);
            mockCodeMapper.Expect(x => x.GetUNLOCOfromIATA("MKE")).Return("").Repeat.Times(1);
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GFEMCIAAA").Repeat.Times(4);
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("CalculateTimeZoneOffset", "@offset", "@UNLOCO", "USNYC", "@localtime", "2005-01-28T10:15:01")).Return("-04:00").Repeat.Times(2);
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("CalculateTimeZoneOffset", "@offset", "@UNLOCO", "USNYC", "@localtime", "2014-11-02T14:39:00")).Return("-04:00").Repeat.Once();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("CalculateTimeZoneOffset", "@offset", "@UNLOCO", "USCHI", "@localtime", "2017-07-11T12:17:02")).Return("-05:00").Repeat.Once();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "19676292")).Return("30011622").Repeat.Times(2);
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "47816695")).Return("8338").Repeat.Once();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "56021780")).Return("2344598").Repeat.Once();
            var extensionObjects = new Dictionary<string, object>() 
			{ 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockUnitConvertorMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            string sourceFile = "X12_214_2_UniversalEvent.TestFiles.Test1_input.xml";
            string expectedFile = "X12_214_2_UniversalEvent.TestFiles.Test1_output.xml";
            mapTester.Execute<X12_214_2_UniversalEvent>(sourceFile, expectedFile);

            sourceFile = "X12_214_2_UniversalEvent.TestFiles.Test2_input.xml";
            expectedFile = "X12_214_2_UniversalEvent.TestFiles.Test2_output.xml";
            mapTester.Execute<X12_214_2_UniversalEvent>(sourceFile, expectedFile);

            sourceFile = "X12_214_2_UniversalEvent.TestFiles.Test3_input.xml";
            expectedFile = "X12_214_2_UniversalEvent.TestFiles.Test3_output.xml";
            mapTester.Execute<X12_214_2_UniversalEvent>(sourceFile, expectedFile);

            sourceFile = "X12_214_2_UniversalEvent.TestFiles.Test5_input.xml";
            expectedFile = "X12_214_2_UniversalEvent.TestFiles.Test5_output.xml";
            mapTester.Execute<X12_214_2_UniversalEvent>(sourceFile, expectedFile);

            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void X12_214_2_UniversalEvent_NotSubscribed()
        {
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

            mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2013-09-30T15:35:44");
            mockCodeMapper.Stub(x => x.CallActionProcedureHelper("CalculateTimeZoneOffset", "@offset", "@UNLOCO", "USNYC", "@localtime", "2014-11-02T14:39:00")).Return("");
            mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "47816695")).Return("");
            mockCodeMapper.Stub(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Type", "X1")).Return("ARV");
            mockCodeMapper.Stub(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Receive 214", "Event Type", "Event Description", "X1")).Return("|FAC=Delivery Location");
            mockCodeMapper.Stub(x => x.GetUNLOCOfromIATA("YYZ")).Return("");
            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GFEMCIAAA");
            var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor }
			};

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            string sourceFile = "X12_214_2_UniversalEvent.TestFiles.Test4_input.xml";
            string expectedFile = "X12_214_2_UniversalEvent.TestFiles.Test4_output.xml";
            mapTester.ExecuteCompiled<X12_214_2_UniversalEvent>(sourceFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
