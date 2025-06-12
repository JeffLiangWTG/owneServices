using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Hawking.UnitTest.Tools.Xslt;

using Hawking.Xslt.ExtensionObjects.Interfaces;
using Moq;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    public class AIRAED2UEvent_Tests
    {
        const string xsl = "AIRAED2UEvent.MapFiles.AIRAED2UEvent.xsl";

        [Fact]
        public void TestAIRAED2UEvent1_failure()
        {
            var input = "AIRAED2UEvent.TestFiles.Test1_input.xml";
            var expectedOutput = "AIRAED2UEvent.TestFiles.Test1_output_failure.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIDateMapper.Setup(x => x.CurrentDateTime("s")).Returns("2017-08-23T11:30:01");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB1MAN000009200001E", "@referenceType", "CNRF")).Returns("000011");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB2MAN000009200002E", "@referenceType", "CNRF")).Returns("000012|00002413");

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGTVWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 99999999;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAcc");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void TestAIRAED2UEvent1_success()
        {
            var input = "AIRAED2UEvent.TestFiles.Test1_input.xml";
            var expectedOutput = "AIRAED2UEvent.TestFiles.Test1_output_success.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIDateMapper.Setup(x => x.CurrentDateTime("s")).Returns("2017-08-23T11:30:01");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB1MAN000009200001E", "@referenceType", "CNRF")).Returns("000011");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB2MAN000009200002E", "@referenceType", "CNRF")).Returns("000012|00002413");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGTVWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(()=>
                {
                   return "ACKSuccess;";
                });

            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void TestAIRAED2UEvent2()
        {
            var input = "AIRAED2UEvent.TestFiles.Test2_input.xml";
            var expectedOutput = "AIRAED2UEvent.TestFiles.Test2_output.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIDateMapper.Setup(x => x.CurrentDateTime("s")).Returns("2017-08-24T11:30:01");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "876E", "@referenceType", "CNRF")).Returns("0000166|0000267|0000368");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "82727E", "@referenceType", "CNRF")).Returns("0000169");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "812746E", "@referenceType", "CNRF")).Returns("0000170");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "82746E", "@referenceType", "CNRF")).Returns("0000170");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "8176E", "@referenceType", "CNRF")).Returns("0000169");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "812727E", "@referenceType", "CNRF")).Returns("0000169");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "8716E", "@referenceType", "CNRF")).Returns("0000166|0000267|0000368");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "827127E", "@referenceType", "CNRF")).Returns("0000169");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value", "827146E", "@referenceType", "CNRF")).Returns("0000170");

            mockIContextAccessor.Setup(x =>
                    x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"))
                .Returns("Source");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("Destination");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 99999999;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAcc");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }
    }
}