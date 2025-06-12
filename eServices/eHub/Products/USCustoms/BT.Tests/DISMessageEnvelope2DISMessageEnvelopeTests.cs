using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.USCustoms.BT.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.USCustoms.BT.Tests
{
    [TestClass]
    public class DISMessageEnvelope2DISMessageEnvelopeTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DISMessageEnvelope2DISMessageEnvelope()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("USDIS").Repeat.Once();
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ABCABCABC").Repeat.Once();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "")).Repeat.Once();

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
                {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            const string sourceFile = "TestFiles.DISMessageEnvelope.xml";
            const string expectedFile = "TestFiles.DISMessageEnvelope_Output.xml";
            mapTester.Execute<DISMessageEnvelope2DISMessageEnvelope>(sourceFile, expectedFile);

            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DISMessageEnvelope2DISMessageEnvelope_TestClient()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("USDISTest").Repeat.Once();
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ABCABCABC").Repeat.Once();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "")).Repeat.Once();

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
                {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            const string sourceFile = "TestFiles.DISMessageEnvelope.xml";
            const string expectedFile = "TestFiles.DISMessageEnvelope_Output.xml";
            mapTester.Execute<DISMessageEnvelope2DISMessageEnvelope>(sourceFile, expectedFile);

            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DISMessageEnvelope2DISMessageEnvelope_Outbound()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ABCABCABC").Repeat.Once();
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("USDIS").Repeat.Once();
            mockCodeMapper.Stub(x => x.GetRecipientCode("USDIS", "USDIS", "USDIS Configuration", "Transmitter ID", "TransmitterID", "USDIS")).Return("0123456789").Repeat.Once();
            mockCodeMapper.Stub(x => x.GetRecipientCode("USDIS", "USDIS", "USDIS Configuration", "Transmitter Site Code", "TransmitterSiteCode", "USDIS")).Return("9876543210").Repeat.Once();
            mockCodeMapper.Stub(x => x.GetRecipientCode("USDIS", "USDIS", "USDIS Configuration", "Message Routing", "Protocol", "ABCABCABC")).Return("MQ").Repeat.Once();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "MQ")).Repeat.Once();

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
                {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            const string sourceFile = "TestFiles.DISMessageEnvelope.xml";
            const string expectedFile = "TestFiles.DISMessageEnvelope_Outbound_Output.xml";
            mapTester.Execute<DISMessageEnvelope2DISMessageEnvelope>(sourceFile, expectedFile);

            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
